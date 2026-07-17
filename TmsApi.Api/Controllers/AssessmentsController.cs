using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;


namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/assessments")]
[Tags("Assessments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AssessmentsController(
    IAssessmentService assessmentService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AssessmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List assessments for a course")]
    [EndpointDescription("Returns all assessments that belong to the specified course.")]
    public async Task<IActionResult> GetAssessments(
        int courseId,
        CancellationToken ct)
    {
        if (!await assessmentService.CourseExistsAsync(courseId, ct))
        {
            return NotFound();
        }

        var assessments = await assessmentService.GetByCourseAsync(courseId, ct);

        return Ok(assessments);
    }

    [HttpGet("{id:int}", Name = nameof(GetAssessment))]
    [ProducesResponseType(typeof(AssessmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get an assessment by ID")]
    [EndpointDescription("Returns assessment details together with HATEOAS links.")]
    public async Task<IActionResult> GetAssessment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var assessment = await assessmentService.GetByIdAsync(courseId, id, ct);

        if (assessment is null)
        {
            return NotFound();
        }

        var selfLink = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetAssessment),
            new { courseId, id })!;

        var collectionLink = linkGenerator.GetPathByAction(
            HttpContext,
            nameof(GetAssessments),
            values: new { courseId })!;

        var links = new List<LinkDto>
        {
            new(selfLink, "self", "GET"),
            new(selfLink, "update", "PUT"),
            new(selfLink, "delete", "DELETE"),
            new(collectionLink, "collection", "GET")
        };

        var detail = new AssessmentDetailDto
        {
            Id = assessment.Id,
            Title = assessment.Title,
            MaxScore = assessment.MaxScore,
            Weight = assessment.Weight,
            CourseId = assessment.CourseId,
            Links = links
        };

        return Ok(detail);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssessmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Create a new assessment")]
    [EndpointDescription("Creates an assessment for the specified course.")]
    public async Task<IActionResult> CreateAssessment(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct)
    {
        if (!await assessmentService.CourseExistsAsync(courseId, ct))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail = $"Course '{courseId}' does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var result = await assessmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetAssessment),
            new { courseId, id = result.Id },
            result);
    }
}