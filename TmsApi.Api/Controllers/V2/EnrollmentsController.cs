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
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
[Authorize(Roles = "Student")]
public class EnrollmentsController(
    IMediator mediator,
    IEnrollmentService enrollmentService,
    IStudentService studentService,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{
    public record EnrollRequest(string CourseCode);

    [HttpPost]
    public async Task<IActionResult> Enroll(
        [FromBody] EnrollRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var studentId = await studentService.GetIdByUserIdAsync(userId, ct);

        if (studentId is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Student profile not found",
                Detail = "The authenticated user is not linked to a student profile.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var command = new EnrollStudentCommand(
            studentId.Value,
            request.CourseCode);

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