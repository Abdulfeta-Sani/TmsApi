using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(ICourseService courseService, LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]    
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await courseService.GetCoursesAsync(request, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]    
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        // Retrieve the course from the service layer.
        var course = await courseService.GetByIdAsync(id, ct);

        // Return 404 if the course does not exist.
        if (course is null)
        {
            return NotFound();
        }

        // Build the "self" route using the route name.
        var selfLink = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id })!;

        // Build the enrollments collection route, using GetPathByName()
        var enrollmentsLink = linkGenerator.GetPathByName(
            HttpContext,
            "ListCourseEnrollments",
            new { courseId = id })!;


        // Build the HATEOAS links.
        var links = new List<LinkDto>
        {
            new(selfLink, "self", "GET"),
            new(selfLink, "update", "PUT"),
            new(selfLink, "delete", "DELETE"),
            new(enrollmentsLink, "enrollments", "GET")
        };

        // Only expose the "enroll" action if the course still has remaining capacity.
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(
                new LinkDto(
                    enrollmentsLink,
                    "enroll",
                    "POST"));
        }

        // Build the detail DTO.
        var detail = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]    
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await courseService.CreateAsync(request, ct);

        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
    }
}