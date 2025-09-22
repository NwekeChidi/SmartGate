using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartGate.Api.Auth;

namespace SmartGate.Api.Tests.Auth;

public class DevAuthHandlerTests
{
    private readonly IOptionsMonitor<DevAuthOptions> _options = Substitute.For<IOptionsMonitor<DevAuthOptions>>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly UrlEncoder _encoder = Substitute.For<UrlEncoder>();
    private readonly HttpContext _httpContext = new DefaultHttpContext();

    public DevAuthHandlerTests()
    {
        _options.Get(DevAuthHandler.Scheme).Returns(new DevAuthOptions());
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithHeaders_ReturnsSuccess()
    {
        _httpContext.Request.Headers["X-Debug-User"] = "test-user";
        _httpContext.Request.Headers["X-Debug-Scopes"] = "visits:read visits:write";

        var handler = new DevAuthHandler(_options, _loggerFactory, _encoder);
        await handler.InitializeAsync(new AuthenticationScheme(DevAuthHandler.Scheme, null, typeof(DevAuthHandler)), _httpContext);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        result?.Principal?.FindFirst("sub")?.Value.Should().Be("test-user");
        result?.Principal?.FindFirst("scope")?.Value.Should().Be("visits:read visits:write");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithoutHeaders_UsesDefaults()
    {
        var handler = new DevAuthHandler(_options, _loggerFactory, _encoder);
        await handler.InitializeAsync(new AuthenticationScheme(DevAuthHandler.Scheme, null, typeof(DevAuthHandler)), _httpContext);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        result?.Principal?.FindFirst("sub")?.Value.Should().Be("dev-user");
        result?.Principal?.FindFirst("scope")?.Value.Should().Be("visits:read visits:write");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithPartialHeaders_UsesDefaults()
    {
        _httpContext.Request.Headers["X-Debug-User"] = "custom-user";

        var handler = new DevAuthHandler(_options, _loggerFactory, _encoder);
        await handler.InitializeAsync(new AuthenticationScheme(DevAuthHandler.Scheme, null, typeof(DevAuthHandler)), _httpContext);

        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeTrue();
        result?.Principal?.FindFirst("sub")?.Value.Should().Be("custom-user");
        result?.Principal?.FindFirst("scope")?.Value.Should().Be("visits:read visits:write");
    }
}