using System.Security.Claims;

namespace SmartGate.Api.Auth;

public static class Policies
{
    public const string ReadScope  = "visits:read";
    public const string WriteScope = "visits:write";

    public static bool HasScopeOrRole(ClaimsPrincipal user, string required)
    {
        var scope = user.FindFirst("scope")?.Value
                    ?? user.FindFirst("scp")?.Value;

        if (!string.IsNullOrWhiteSpace(scope))
            return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Contains(required, StringComparer.OrdinalIgnoreCase);

        // Roles
        return user.IsInRole(required)
            || user.Claims.Any(c =>
                   (c.Type is ClaimTypes.Role or "roles" or "role")
                   && string.Equals(c.Value, required, StringComparison.OrdinalIgnoreCase));
    }
}