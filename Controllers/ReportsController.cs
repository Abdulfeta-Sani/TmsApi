using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext db) : ControllerBase
{
    // active students have GPA >= 3.0
    [HttpGet("active-students-count")]
    public async Task<IActionResult> ActiveStudentsCount()
    {
        var count = await db.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(new
        {
            ActiveStudentsWithGoodGpa = count
        });
    }

    // courses have the most enrollments, sorted descending
    [HttpGet("course-enrollments")]
    public async Task<IActionResult> CourseEnrollments()
    {
        var list = await db.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    //  the average GPA per course
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> AverageGpaPerCourse()
    {
        var list = await db.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // students have zero enrollments, Using Subquery
    [HttpGet("students-without-enrollments-a")]
    public async Task<IActionResult> StudentsWithoutEnrollmentsA()
    {
        var list = await db.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // students have zero enrollments, Using EF Core 10 LeftJoin
    [HttpGet("students-without-enrollments-b")]
    public async Task<IActionResult> StudentsWithoutEnrollmentsB()
    {
        var list = await db.Students
            .LeftJoin(
                db.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { s, e })
            .Where(x => x.e == null)
            .Select(x => x.s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // Pagination paged list of students: page size 20
    [HttpGet("students")]
    public async Task<IActionResult> GetStudentsPage(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var students = await db.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // Top 5 courses by enrollment GroupBy, order by count
    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses(
        CancellationToken cancellationToken = default)
    {
        var courses = await db.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Title = g.Key,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(courses);
    }

    // Intentionally creating the bad pattern N+1
    [HttpGet("n-plus-one-demo")]
    public async Task<IActionResult> NPlusOneDemo(
        CancellationToken cancellationToken = default)
    {
        var students = await db.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var s in students)
        {
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(
                    e => e.StudentId == s.Id,
                    cancellationToken);

            Console.WriteLine(
                $"{s.Name}: {count} enrollments");
        }

        return Ok("Check SQL logs");
    }

    // Fix N+1 with query shaping
    [HttpGet("n-plus-one-fixed")]
    public async Task<IActionResult> NPlusOneFixed(
        CancellationToken cancellationToken = default)
    {
        var report = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var r in report)
        {
            Console.WriteLine(
                $"{r.Name}: {r.EnrollmentCount} enrollments");
        }

        return Ok(report);
    }

    [HttpGet("concurrency-demo/{id:int}")]
    public async Task<IActionResult> GetStudentForConcurrencyDemo(
        int id,
        CancellationToken cancellationToken = default)
    {
        var student = await db.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.GPA,
                s.Version
            })
            .SingleOrDefaultAsync(cancellationToken);

        return student is null
            ? NotFound()
            : Ok(student);
    }

    [HttpPut("concurrency-demo/{id:int}")]
    public async Task<IActionResult> UpdateStudentConcurrencyDemo(
        int id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Load the student from the database
        var student = await db.Students
            .SingleOrDefaultAsync(s => s.Id == id, cancellationToken);

        // Step 2: If student doesn't exist, return 404 Not Found
        if (student is null)
            return NotFound();

        // Step 3: Tell EF what the original Version was and check for concurrency conflicts
        db.Entry(student)
            .Property(s => s.Version)
            .OriginalValue = request.Version;

        // Step 4: Update the student's properties
        student.Name = request.Name;
        student.GPA = request.GPA;

        // Step 5: Try to save
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "Student updated successfully", student });
        }
        catch (DbUpdateConcurrencyException)
        {
            // Step 6: If concurrency conflict, return 409 Conflict
            return Conflict(new 
            { 
                error = "Concurrency conflict",
                message = "Another user modified this student. Please reload and try again.",
            });
        }
    }

    // Demonstrate the query filter — Normal query
    [HttpGet("students-active")]
    public async Task<IActionResult> GetActiveStudents(
        CancellationToken cancellationToken = default)
    {
        var students = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // Demonstrate the query filter — Admin query
    [HttpGet("students-admin")]
    public async Task<IActionResult> GetAllStudentsForAdmin(
        CancellationToken cancellationToken = default)
    {
        var students = await db.Students
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    [HttpPost("archive-old-enrollments")]
    public async Task<IActionResult> ArchiveOldEnrollments(
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);

        var affectedRows = await db.Enrollments
            .Where(e => e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    e => e.IsArchived,
                    true),
                cancellationToken);

        return Ok(new
        {
            ArchivedEnrollments = affectedRows
        });
    }
}