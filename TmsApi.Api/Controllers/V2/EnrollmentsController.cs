using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{
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

    // add temporary enrollment approval broadcast endpoint b/c the enrollment status in the domain model is not yet implemented
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, CancellationToken ct)
    {
        await hubContext.Clients.All.ReceiveEnrollmentStatusUpdated(
            id,
            "Approved");

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