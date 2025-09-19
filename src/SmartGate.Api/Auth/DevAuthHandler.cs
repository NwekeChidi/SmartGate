using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SmartGate.Api.Auth;

public class DevAuthOptions : AuthenticationSchemeOptions { }

public sealed class DevAuthHandler : AuthenticationHandler<DevAuthOptions>
{
    public new const string Scheme = "Dev";

    [Obsolete]
    public DevAuthHandler(IOptionsMonitor<DevAuthOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var sub = Request.Headers["X-Debug-User"].FirstOrDefault() ?? "dev-user";
        var scopesHeader = Request.Headers["X-Debug-Scopes"].FirstOrDefault() ?? "visits:read visits:write";

        var claims = new List<Claim>
        {
            new("sub", sub),
            new("scope", scopesHeader)
        };
        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
