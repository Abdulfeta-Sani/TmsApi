using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
    IMediator mediator,
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCoursesQuery(request), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);

        if (course is null)
        {
            return NotFound();
        }

        var selfLink = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id })!;

        var updateLink = linkGenerator.GetPathByName(
            HttpContext,
            nameof(UpdateCourse),
            new { id })!;

        var deleteLink = linkGenerator.GetPathByName(
            HttpContext,
            nameof(DeleteCourse),
            new { id })!;

        var enrollmentsLink = linkGenerator.GetPathByName(
            HttpContext,
            "ListCourseEnrollments",
            new { courseId = id })!;

        var links = new List<LinkDto>
        {
            new(selfLink, "self", "GET"),
            new(updateLink, "update", "PUT"),
            new(deleteLink, "delete", "DELETE"),
            new(enrollmentsLink, "enrollments", "GET")
        };

        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(enrollmentsLink, "enroll", "POST"));
        }

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
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        var createdId = await mediator.Send(
            new CreateCourseCommand(
                request.Code,
                request.Title,
                request.MaxCapacity),
            ct);

        var result = await courseService.GetByIdAsync(createdId, ct);

        return CreatedAtAction(nameof(GetCourseById), new { id = createdId }, result);
    }

    [HttpPut("{id:int}", Name = nameof(UpdateCourse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update an existing course")]
    [EndpointDescription("Updates a course by ID. Returns 404 if the course does not exist and 409 if the new code conflicts with another course.")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken ct)
    {
        var updated = await mediator.Send(
            new UpdateCourseCommand(
                id,
                request.Code,
                request.Title,
                request.MaxCapacity),
            ct);

        if (!updated)
        {
            var courseExists = await courseService.GetByIdAsync(id, ct);

            if (courseExists is null)
            {
                return NotFound();
            }

            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}", Name = nameof(DeleteCourse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a course")]
    [EndpointDescription("Deletes a course by ID. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var deleted = await mediator.Send(new DeleteCourseCommand(id), ct);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}