using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    IMediator mediator,
    IEnrollmentService enrollmentService,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
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

    [HttpPost]
    public async Task<IActionResult> Enroll(
EnrollStudentCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.Match<IActionResult>(onSuccess: created => CreatedAtAction(
            nameof(GetSchedule),
            new { studentId = created.StudentId }, created),
            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "course_full" or "already_enrolled" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var updated = await enrollmentService.UpdateStatusAsync(
            id,
            EnrollmentStatus.Approved,
            ct);

        if (!updated)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Enrollment not found",
                Detail = $"Enrollment {id} was not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        await hubContext.Clients.All.ReceiveEnrollmentStatusUpdated(
            id.ToString(),
            EnrollmentStatus.Approved.ToString());

        return NoContent();
    }

    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
    int studentId, CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId), ct);
        return Ok(schedule);
    }
}