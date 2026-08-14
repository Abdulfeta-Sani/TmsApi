using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/grades")]
[Tags("Grades")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class GradesController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SubmitGradeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Submit a final grade")]
    [EndpointDescription("Accepts a grade submission and returns a simple success payload for the lab.")]
    public IActionResult SubmitGrade([FromBody] SubmitGradeRequest request)
    {
        var response = new SubmitGradeResponse(
            Guid.NewGuid().ToString("N")[..12],
            true);

        return Ok(response);
    }
}