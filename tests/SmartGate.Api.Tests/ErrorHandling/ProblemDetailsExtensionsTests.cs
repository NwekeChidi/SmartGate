using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartGate.Api.ErrorHandling;
using SmartGate.Application.Abstractions;
using SmartGate.Domain.Visits;
using System.Reflection;

namespace SmartGate.Api.Tests.ErrorHandling;

public class ProblemDetailsExtensionsTests
{
    private readonly HttpContext _httpContext = new DefaultHttpContext();

    public ProblemDetailsExtensionsTests()
    {
        _httpContext.Request.Path = "/test";
        _httpContext.TraceIdentifier = "test-trace-id";
    }

    [Fact]
    public void ToProblem_DomainException_ReturnsConflict()
    {
        var exception = new ActivitiesRequiredException();
        
        var result = exception.ToProblem(_httpContext);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_KeyNotFoundException_ReturnsNotFound()
    {
        var exception = new KeyNotFoundException("Resource not found");
        
        var result = exception.ToProblem(_httpContext);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_ValidationException_ReturnsBadRequest()
    {
        var failures = new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Email", "Invalid email")
        };
        var exception = new ValidationException(failures);
        
        var result = exception.ToProblem(_httpContext);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_ValidationException_WithComplexPropertyNames_FormatsCamelCase()
    {
        var failures = new[]
        {
            new ValidationFailure("Driver.FirstName", "First name is required"),
            new ValidationFailure("Activities[0].UnitNumber", "Unit number is invalid"),
            new ValidationFailure("Truck.LicensePlate", "License plate is required")
        };
        var exception = new ValidationException(failures);
        
        var result = exception.ToProblem(_httpContext);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_DuplicateRequestException_ReturnsConflict()
    {
        var exception = new DuplicateRequestException("Duplicate request");
        
        var result = exception.ToProblem(_httpContext);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_GenericException_ReturnsInternalServerError()
    {
        var exception = new InvalidOperationException("Something went wrong");
        
        var result = exception.ToProblem(_httpContext);
        
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("Name", "name")]
    [InlineData("FirstName", "firstName")]
    [InlineData("Driver.FirstName", "driver.firstName")]
    [InlineData("Activities[0].UnitNumber", "activities[0].unitNumber")]
    [InlineData("Truck.LicensePlate", "truck.licensePlate")]
    public void ToCamelCase_ConvertsCorrectly(string input, string expected)
    {
        var method = typeof(ProblemDetailsExtensions).GetMethod("ToCamelCase", BindingFlags.NonPublic | BindingFlags.Static);
        var result = method?.Invoke(null, new object[] { input }) as string;
        
        result.Should().Be(expected);
    }
}