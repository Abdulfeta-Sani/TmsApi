using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos.Auth;
using Asp.Versioning;

namespace TmsApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(
        [FromBody] LoginRequest request,
        [FromServices] IWebHostEnvironment env)
    {
        // Demo credentials for M10 transport-layer learning
        if (request.Username == "admin" && request.Password == "Password123!")
        {
            var dummyJwt = "header.payload.signature-demo-token";

            // Append HttpOnly authentication cookie
            Response.Cookies.Append("tms_auth", dummyJwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = !env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            return Ok(new UserProfileDto("System Admin", "Admin"));
        }

        return Unauthorized(new { detail = "Invalid username or password." });
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // Browser automatically attaches the cookie on later requests
        if (Request.Cookies.TryGetValue("tms_auth", out _))
        {
            return Ok(new UserProfileDto("System Admin", "Admin"));
        }

        return Unauthorized(new { detail = "Session expired or missing authentication cookie." });
    }
}