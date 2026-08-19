using Asp.Versioning;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Api.Filters;
using TmsApi.Api.Middlewares;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Enrollments.Commands;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;
using TmsApi.Infrastructure.Transcripts;
using System.Threading.Channels;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Api.Notifications;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using TmsApi.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})

.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddIdentityCore<TmsUser>(options =>
{
    // Enterprise Password Policy
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    // Brute-Force Lockout Protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TmsDbContext>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));
builder.Services.AddHostedService<TranscriptWorker>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"paid:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 200,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),

            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"free:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 30,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),

            _ => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"anon:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                })
        };
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
        {
            retryAfter = ((int)ts.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = $"Too many requests. Retry after {retryAfter} seconds.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tms.local/errors/rate_limit_exceeded"
        }, ct);
    };

    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5; // max in-flight transcript requests
        opt.QueueLimit = 20; // queue next 20
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });
});

// Production-only leave commented in lab
// builder.Services.AddStackExchangeRedisCache(options =>
// {
// options.Configuration = builder.Configuration.GetConnectionString("Redis");
// options.InstanceName = "tms:";
// });
// builder.Services.AddHybridCache();

builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();

var app = builder.Build();

app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");
app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);

        options.AddDocument("v1", "API Version 1.0");
        options.AddDocument("v2", "API Version 2.0");
    });
}

app.UseMiddleware<V1DeprecationMiddleware>();
app.UseCors("TmsClient");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true || context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = !builder.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
    }

    await next(context);
});

app.UseRateLimiter();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}
// Exercise 9 will introduces The health check endpoints
app.MapHealthChecks("/health/live").DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();
app.Run();