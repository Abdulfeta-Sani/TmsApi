using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class TrainingAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TrainingAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the request contains the training header
        if (!Request.Headers.ContainsKey("X-Training-User"))
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Missing training user header."));
        }

        // Create claims for the authenticated user
        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                Request.Headers["X-Training-User"]!)
        };

        // Create identity
        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name);

        // Create principal
        var principal = new ClaimsPrincipal(identity);

        // Create authentication ticket
        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}