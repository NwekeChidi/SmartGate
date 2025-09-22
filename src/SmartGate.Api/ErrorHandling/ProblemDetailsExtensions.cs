using System.Web;
using Microsoft.AspNetCore.Mvc;
using SmartGate.Application.Abstractions;
using SmartGate.Domain.Common;

namespace SmartGate.Api.ErrorHandling;

public static class ProblemDetailsExtensions
{
    public static IResult ToProblem(this Exception ex, HttpContext http)
    {
        var pd = new ProblemDetails
        {
            Instance = http.Request.Path,
            Extensions = { ["traceId"] = http.TraceIdentifier }
        };

        switch (ex)
        {
            case DomainException dex:
                pd.Title = "Domain rule violated";
                pd.Detail = dex.Message;
                pd.Status = StatusCodes.Status409Conflict;
                break;

            case KeyNotFoundException:
                pd.Title = "Resource not found";
                pd.Detail = ex.Message;
                pd.Status = StatusCodes.Status404NotFound;
                break;

            case FluentValidation.ValidationException vex:
                pd.Title = "Validation failed";
                pd.Detail = "One or more fields are invalid.";
                pd.Status = StatusCodes.Status400BadRequest;
                pd.Extensions["errors"] = vex.Errors.Select(e => new { field = ToCamelCase(e.PropertyName), message = e.ErrorMessage });
                break;
            
            case DuplicateRequestException dupEx:
                pd.Title = "Duplicate request";
                pd.Detail = dupEx.Message;
                pd.Status = StatusCodes.Status409Conflict;
                break;

            default:
                pd.Title = "Internal server error";
                pd.Detail = ex.Message;
                pd.Status = StatusCodes.Status500InternalServerError;
                break;
        }

        return Results.Problem(title: HttpUtility.HtmlEncode(pd.Title), detail: HttpUtility.HtmlEncode(pd.Detail), statusCode: pd.Status,
            instance: pd.Instance, extensions: pd.Extensions);
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return string.Join(".", input.Split('.').Select(FormatPart));
        
        static string FormatPart(string part)
        {
            var bracketIndex = part.IndexOf('[');
            return bracketIndex >= 0
                ? char.ToLowerInvariant(part[0]) + part[1..bracketIndex] + part[bracketIndex..]
                : char.ToLowerInvariant(part[0]) + part[1..];
        }
    }
}
