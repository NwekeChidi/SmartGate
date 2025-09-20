using SmartGate.Application.Abstractions;
using System.Security.Claims;

namespace SmartGate.Api.Auth;

public sealed class HttpUserContext(IHttpContextAccessor http) : IUserContext
{
    private readonly IHttpContextAccessor _http = http;

    public string Subject =>
        _http.HttpContext?.User?.FindFirstValue("sub")
        ?? _http.HttpContext?.User?.Identity?.Name
        ?? "anonymous";
}
