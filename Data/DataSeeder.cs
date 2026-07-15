using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

public static class DataSeeder
{
    private static readonly (string RegistrationNumber, string Name, decimal GPA, bool IsActive)[] Students =
    [
        ("TMS-2026-0001", "Alice Smith", 3.80m, true),
        ("TMS-2026-0002", "Bob Jones", 2.90m, true),
        ("TMS-2026-0003", "Charlie Brown", 3.40m, false),
        ("TMS-2026-0004", "Diana Prince", 3.90m, true),
        ("TMS-2026-0005", "Evan Wright", 2.50m, true),
        ("TMS-2026-0006", "Fatima Hassan", 3.75m, true),
        ("TMS-2026-0007", "Gabriel Tesfaye", 3.15m, true),
        ("TMS-2026-0008", "Hannah Lee", 3.55m, true),
        ("TMS-2026-0009", "Isaac Kim", 2.70m, true),
        ("TMS-2026-0010", "Julia Roberts", 3.95m, true),
        ("TMS-2026-0011", "Kevin Johnson", 2.85m, false),
        ("TMS-2026-0012", "Liya Bekele", 3.60m, true),
        ("TMS-2026-0013", "Mohammed Ali", 3.20m, true),
        ("TMS-2026-0014", "Nathan Scott", 2.45m, true),
        ("TMS-2026-0015", "Olivia White", 3.88m, true),
        ("TMS-2026-0016", "Peter Parker", 3.30m, true),
        ("TMS-2026-0017", "Queen Amanuel", 3.05m, true),
        ("TMS-2026-0018", "Rachel Green", 2.95m, false),
        ("TMS-2026-0019", "Samuel Wilson", 3.72m, true),
        ("TMS-2026-0020", "Tigist Alemu", 3.48m, true),
        ("TMS-2026-0021", "Umer Faruk", 2.65m, true),
        ("TMS-2026-0022", "Victoria Adams", 3.82m, true),
        ("TMS-2026-0023", "William Carter", 3.12m, true),
        ("TMS-2026-0024", "Xavier Brown", 2.58m, false),
        ("TMS-2026-0025", "Zoe Parker", 3.99m, true),
    ];
    private static readonly (string Code, string Title, int MaxCapacity)[] Courses =
    [
        ("CSE-101", "Web Development Fundamentals", 30),
        ("CSE-102", "TypeScript Essentials", 30),
        ("CSE-103", "Git and Collaborative Workflows", 25),
        ("CSE-201", "ASP.NET Core Fundamentals", 28),
        ("CSE-202", "Entity Framework Core and PostgreSQL", 28),
        ("CSE-203", "Building RESTful Web APIs", 28),
        ("CSE-301", "Advanced Web API Patterns", 24),
        ("CSE-302", "Angular Fundamentals", 26),
        ("CSE-303", "Angular Advanced", 24),
        ("CSE-304", "Full-Stack Integration", 22),
        ("CSE-305", "Testing and Quality Assurance", 22),
        ("CSE-306", "Security and Authentication", 20),
        ("DAT-101", "Database Design Foundations", 30),
        ("DAT-201", "Advanced SQL and Indexing", 26),
        ("DAT-202", "Data Modelling for the Web", 26),
        ("ARC-101", "Software Architecture Patterns", 22),
        ("ARC-201", "Cloud-Native Architecture", 22),
        ("DEV-101", "DevOps Foundations", 24),
        ("DEV-201", "Continuous Delivery Pipelines", 22),
        ("MOB-101", "Mobile App Foundations", 24),
        ("MOB-201", "Cross-Platform Mobile", 22),
        ("AI-101", "Applied Machine Learning", 20),
        ("AI-201", "Generative AI for Developers", 18),
        ("UX-101", "UX Research and Wireframing", 24),
        ("UX-201", "Design Systems and Tokens", 22),
    ];

    public static async Task SeedAsync(TmsDbContext context, CancellationToken ct = default)
    {
        // Apply any pending EF Core migrations.
        await context.Database.MigrateAsync(ct);

        // Seed Students only if none exist.
        if (!await context.Students.AnyAsync(ct))
        {
            foreach (var (registrationNumber, name, gpa, isActive) in Students)
            {
                context.Students.Add(new Student
                {
                    RegistrationNumber = registrationNumber,
                    Name = name,
                    GPA = gpa,
                    IsActive = isActive
                });
            }
        }

        // Seed Courses only if none exist.
        if (!await context.Courses.AnyAsync(ct))
        {
            foreach (var (code, title, maxCapacity) in Courses)
            {
                context.Courses.Add(new Course
                {
                    Code = code,
                    Title = title,
                    MaxCapacity = maxCapacity
                });
            }
        }

        // Persist everything with one database call.
        await context.SaveChangesAsync(ct);
    }
}