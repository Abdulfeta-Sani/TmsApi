using Microsoft.AspNetCore.Authentication;

// =============== Exercise 1: The Blind Server (Middleware Ordering)

// // Step 1 === The broken Pipeline
// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();
// app.UseRouting();
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// }));
// app.UseAuthentication();
// app.UseAuthorization();

// app.Run();

// Step 2 === Fix the Pipeline
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register authentication service
builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions,
        TrainingAuthHandler>("Training", null);

// Register authorization service
builder.Services.AddAuthorization();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddScoped<IEnrollmentService,
    EnrollmentService>();

builder.Services.AddSingleton<
    EnrollmentWorker>();

builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Routing middleware
app.UseRouting();

// Authentication middleware
app.UseAuthentication();

// Authorization middleware
app.UseAuthorization();

// Protected route
app.MapGet("/api/assessments/results", () =>
    Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    }))
    .RequireAuthorization();

app.MapControllers();

app.MapGet("/worker-smoke",
    async (EnrollmentWorker worker) =>
{
    var count =
        await worker.SmokeTestAsync();

    return Results.Ok(new
    {
        enrollmentsCreated = count
    });
});

// Temporary Verification Endpoint (For Session 2)
app.MapGet(
    "/test-duplicate",
    async (IEnrollmentService service) =>
{
    await service.EnrollAsync(
        "S-001",
        "CS-101");

    await service.EnrollAsync(
        "S-001",
        "CS-101");

    return Results.Ok(
        "Duplicate test completed");
});

app.MapGet(
    "/test-notfound/{id}",
    async (
        string id,
        IEnrollmentService service) =>
{
    var result =
        await service.GetByIdAsync(id);

    return Results.Ok(result);
});
app.Run();
