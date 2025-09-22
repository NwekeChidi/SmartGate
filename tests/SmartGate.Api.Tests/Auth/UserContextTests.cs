using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartGate.Api.Auth;

namespace SmartGate.Api.Tests.Auth;

public class UserContextTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly HttpUserContext _userContext;

    public UserContextTests()
    {
        _userContext = new HttpUserContext(_httpContextAccessor);
    }

    [Fact]
    public void Subject_WithSubClaim_ReturnsSubValue()
    {
        var claims = new[] { new Claim("sub", "test-user") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _userContext.Subject;

        result.Should().Be("test-user");
    }

    [Fact]
    public void Subject_WithIdentityName_ReturnsIdentityName()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "identity-user") };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _userContext.Subject;

        result.Should().Be("identity-user");
    }

    [Fact]
    public void Subject_WithNoContext_ReturnsAnonymous()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var result = _userContext.Subject;

        result.Should().Be("anonymous");
    }

    [Fact]
    public void Subject_WithNoUser_ReturnsAnonymous()
    {
        var httpContext = new DefaultHttpContext { User = null! };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _userContext.Subject;

        result.Should().Be("anonymous");
    }

    [Fact]
    public void Subject_WithNoIdentity_ReturnsAnonymous()
    {
        var principal = new ClaimsPrincipal();
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var result = _userContext.Subject;

        result.Should().Be("anonymous");
    }
}