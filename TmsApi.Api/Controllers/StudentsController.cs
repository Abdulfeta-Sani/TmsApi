using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController(
    IStudentService studentService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List students with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS students. PageSize is capped at 50.")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var result = await studentService.GetStudentsAsync(request, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]
    [EndpointDescription("Returns student details with HATEOAS links. Returns 404 if the student does not exist.")]
    public async Task<IActionResult> GetStudentById(
        int id,
        CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(id, ct);

        if (student is null)
        {
            return NotFound();
        }

        var selfLink = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetStudentById),
            new { id })!;

        var links = new List<LinkDto>
        {
            new(selfLink, "self", "GET"),
            new(selfLink, "update", "PUT"),
            new(selfLink, "delete", "DELETE")
        };

    var detail = new StudentDetailDto
    {
        Id = student.Id,
        RegistrationNumber = student.RegistrationNumber,
        Name = student.Name,
        GPA = student.GPA,
        IsActive = student.IsActive,
        EnrollmentCount = student.EnrollmentCount,
        Links = links
    };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new student")]
    [EndpointDescription("Creates a student with a unique registration number. Returns 409 if the registration number already exists.")]
    public async Task<IActionResult> CreateStudent(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        if (await studentService.RegistrationNumberExistsAsync(
            request.RegistrationNumber, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Registration number already exists",
                Detail = $"A student with registration number '{request.RegistrationNumber}' already exists.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await studentService.CreateAsync(request, ct);

        return CreatedAtAction(
            nameof(GetStudentById),
            new { id = result.Id },
            result);
    }
}