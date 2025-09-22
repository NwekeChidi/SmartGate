using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SmartGate.Api.Common;

namespace SmartGate.Api.ErrorHandling;

public sealed class FlatErrorsProblemDetailsFactory : ProblemDetailsFactory
{

    public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null,
        string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var pd = new ProblemDetails
        {
            Status = statusCode ?? StatusCodes.Status500InternalServerError,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
        pd.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        return pd;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext,
        ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null,
        string? type = null, string? detail = null, string? instance = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var vpd = new ValidationProblemDetails
        {
            Status = statusCode ?? StatusCodes.Status400BadRequest,
            Title = title ?? AppConstants.RFCErrors.ValidationErrorTitle,
            Type = type ?? AppConstants.RFCErrors.DefaultErrorType,
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path
        };
        var errors = TransformModelStateErrors(modelStateDictionary);

        vpd.Extensions["errors"] = errors;
        vpd.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        vpd.Errors.Clear();
        return vpd;
    }

    private static List<object> TransformModelStateErrors(ModelStateDictionary modelStateDictionary)
    {
        return modelStateDictionary
            .Where(kv => kv.Value?.Errors?.Count > 0)
            .SelectMany(kv =>
                kv.Value!.Errors.Select(e => new
                {
                    field = kv.Key.StartsWith("$.") ? kv.Key.TrimStart('$', '.') : kv.Key.TrimStart('.'),
                    message = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(e.ErrorMessage) && e.Exception != null
                        ? e.Exception.Message
                        : e.ErrorMessage)
                }))
            .Where(e => !string.Equals(e.field, "body", StringComparison.OrdinalIgnoreCase))
            .Cast<object>()
            .ToList();
    }
}
