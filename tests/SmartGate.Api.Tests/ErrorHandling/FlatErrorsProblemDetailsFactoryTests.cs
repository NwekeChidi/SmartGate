using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmartGate.Api.Common;
using SmartGate.Api.ErrorHandling;

namespace SmartGate.Api.Tests.ErrorHandling;

public class FlatErrorsProblemDetailsFactoryTests
{
    private readonly FlatErrorsProblemDetailsFactory _factory = new();
    private readonly HttpContext _httpContext = new DefaultHttpContext();

    [Fact]
    public void CreateProblemDetails_WithAllParameters_ReturnsCorrectProblemDetails()
    {
        _httpContext.TraceIdentifier = "test-trace-id";
        
        var result = _factory.CreateProblemDetails(
            _httpContext,
            statusCode: 400,
            title: "Test Title",
            type: "test-type",
            detail: "Test Detail",
            instance: "test-instance"
        );

        result.Status.Should().Be(400);
        result.Title.Should().Be("Test Title");
        result.Type.Should().Be("test-type");
        result.Detail.Should().Be("Test Detail");
        result.Instance.Should().Be("test-instance");
        result.Extensions["traceId"].Should().Be("test-trace-id");
    }

    [Fact]
    public void CreateProblemDetails_WithDefaults_UsesDefaultValues()
    {
        _httpContext.TraceIdentifier = "test-trace-id";
        
        var result = _factory.CreateProblemDetails(_httpContext);

        result.Status.Should().Be(500);
        result.Extensions["traceId"].Should().Be("test-trace-id");
    }

    [Fact]
    public void CreateProblemDetails_WithActivityId_UsesActivityId()
    {
        using var activity = new Activity("test").Start();
        
        var result = _factory.CreateProblemDetails(_httpContext);

        result.Extensions["traceId"].Should().Be(activity.Id);
    }

    [Fact]
    public void CreateValidationProblemDetails_WithModelErrors_ReturnsCorrectFormat()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", "Name is required");
        modelState.AddModelError("Email", "Invalid email format");
        
        _httpContext.Request.Path = "/test";
        _httpContext.TraceIdentifier = "test-trace-id";

        var result = _factory.CreateValidationProblemDetails(
            _httpContext,
            modelState,
            statusCode: 400,
            title: "Validation Error"
        );

        result.Status.Should().Be(400);
        result.Title.Should().Be("Validation Error");
        result.Type.Should().Be(AppConstants.RFCErrors.DefaultErrorType);
        result.Instance.Should().Be("/test");
        result.Extensions["traceId"].Should().Be("test-trace-id");
        result.Errors.Should().BeEmpty();
        
        var errors = result.Extensions["errors"] as IEnumerable<object>;
        errors.Should().HaveCount(2);
    }

    [Fact]
    public void CreateValidationProblemDetails_WithDefaults_UsesDefaultValues()
    {
        var modelState = new ModelStateDictionary();
        _httpContext.Request.Path = "/test";
        
        var result = _factory.CreateValidationProblemDetails(_httpContext, modelState);

        result.Status.Should().Be(400);
        result.Title.Should().Be("One or more validation errors occurred.");
        result.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        result.Instance.Should().Be("/test");
    }

    [Fact]
    public void CreateValidationProblemDetails_FiltersBodyErrors()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("body", "Body error");
        modelState.AddModelError("Name", "Name error");
        
        var result = _factory.CreateValidationProblemDetails(_httpContext, modelState);
        
        var errors = result.Extensions["errors"] as IEnumerable<object>;
        errors.Should().HaveCount(1);
    }

    [Fact]
    public void CreateValidationProblemDetails_HandlesJsonPathFields()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("$.Name", "Name error");
        modelState.AddModelError(".Email", "Email error");
        
        var result = _factory.CreateValidationProblemDetails(_httpContext, modelState);
        
        var errors = result.Extensions["errors"] as IEnumerable<object>;
        errors.Should().HaveCount(2);
    }
}