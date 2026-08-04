using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Queries;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentListItemDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List all enrollments")]
    [EndpointDescription("Returns all enrollments with student and course details")]
    public async Task<IActionResult> GetEnrollments(CancellationToken ct)
    {
        var result = await mediator.Send(new GetEnrollmentsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EnrollmentListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get enrollment by ID")]
    [EndpointDescription("Returns one enrollment row by its ID.")]
    public async Task<IActionResult> GetEnrollmentById(int id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetEnrollmentByIdQuery(id), ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}