using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(TmsDbContext context) : ControllerBase
{
    // active students have GPA >= 3.0
    [HttpGet("active-students-count")]
    public async Task<IActionResult> ActiveStudentsCount()
    {
        var count = await context.Students
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
        var list = await context.Courses
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
        var list = await context.Enrollments
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
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // students have zero enrollments, Using EF Core 10 LeftJoin
    [HttpGet("students-without-enrollments-b")]
    public async Task<IActionResult> StudentsWithoutEnrollmentsB()
    {
        var list = await context.Students
            .LeftJoin(
                context.Enrollments,
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

        var students = await context.Students
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
        var courses = await context.Enrollments
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
        var students = await context.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var s in students)
        {
            var count = await context.Enrollments
                .AsNoTracking()
                .CountAsync(
                    e => e.StudentId == s.Id,
                    cancellationToken);

            Console.WriteLine(
                $"{s.Name}: {count} enrollments");
        }

        return Ok("Check SQL logs");
    }
}