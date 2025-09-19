using SmartGate.Application.Abstractions;
using System.Security.Claims;

namespace SmartGate.Api.Auth;

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _http;
    public HttpUserContext(IHttpContextAccessor http) => _http = http;

    public string Subject =>
        _http.HttpContext?.User?.FindFirstValue("sub")
        ?? _http.HttpContext?.User?.Identity?.Name
        ?? "anonymous";
}
