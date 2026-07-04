using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService)
    : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment =
            await enrollmentService.GetByIdAsync(courseId, id, ct);

        return enrollment is not null
            ? Ok(enrollment)
            : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Step 1: Does the course exist?
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
        {
            return NotFound();
        }

        // Step 2: Is the course already full?
        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail =
                    $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        // Step 3: Create the enrollment
        var enrollment =
            await enrollmentService.CreateAsync(courseId, request, ct);

        // Step 4: Return 201 Created
        return CreatedAtAction(
            nameof(GetEnrollment),
            new
            {
                courseId,
                id = enrollment.Id
            },
            enrollment);
    }
}