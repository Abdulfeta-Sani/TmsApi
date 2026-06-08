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

var app = builder.Build();

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

app.Run();
