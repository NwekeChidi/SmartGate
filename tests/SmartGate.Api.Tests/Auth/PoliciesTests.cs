using System.Security.Claims;
using SmartGate.Api.Auth;

namespace SmartGate.Api.Tests.Auth;

public class PoliciesTests
{
    [Fact]
    public void HasScopeOrRole_WithMatchingScope_ReturnsTrue()
    {
        var claims = new[]
        {
            new Claim("scope", "visits:read visits:write")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:read");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeOrRole_WithMatchingScp_ReturnsTrue()
    {
        var claims = new[]
        {
            new Claim("scp", "visits:read visits:write")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:write");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeOrRole_WithMatchingRole_ReturnsTrue()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "visits:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:read");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeOrRole_WithMatchingRolesClaim_ReturnsTrue()
    {
        var claims = new[]
        {
            new Claim("roles", "visits:write")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:write");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeOrRole_CaseInsensitive_ReturnsTrue()
    {
        var claims = new[]
        {
            new Claim("scope", "VISITS:READ visits:write")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:read");

        result.Should().BeTrue();
    }

    [Fact]
    public void HasScopeOrRole_WithoutMatchingClaims_ReturnsFalse()
    {
        var claims = new[]
        {
            new Claim("scope", "other:read")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:read");

        result.Should().BeFalse();
    }

    [Fact]
    public void HasScopeOrRole_WithEmptyScope_ReturnsFalse()
    {
        var claims = new[]
        {
            new Claim("scope", "")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = Policies.HasScopeOrRole(user, "visits:read");

        result.Should().BeFalse();
    }

    [Fact]
    public void HasScopeOrRole_WithNoClaims_ReturnsFalse()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var result = Policies.HasScopeOrRole(user, "visits:read");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("visits:read")]
    [InlineData("visits:write")]
    public void Constants_HaveCorrectValues(string expected)
    {
        var constants = new[] { Policies.ReadScope, Policies.WriteScope };
        constants.Should().Contain(expected);
    }
}