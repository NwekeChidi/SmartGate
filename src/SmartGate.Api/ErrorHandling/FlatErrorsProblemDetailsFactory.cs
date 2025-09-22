using System.Diagnostics;
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
        
        vpd.Extensions["errors"] = GetFormattedErrors(modelStateDictionary);
        vpd.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        vpd.Errors.Clear();
        
        return vpd;
    }

    private static List<object> GetFormattedErrors(ModelStateDictionary modelState) =>
        modelState
            .Where(kv => kv.Value?.Errors?.Count > 0)
            .SelectMany(kv => kv.Value!.Errors.Select(e => CreateErrorObject(kv.Key, e)))
            .Where(e => !IsBodyField(e))
            .Cast<object>()
            .ToList();

    private static object CreateErrorObject(string key, ModelError error) => new
    {
        field = FormatFieldName(key),
        message = GetErrorMessage(error)
    };

    private static string FormatFieldName(string key) =>
        key.StartsWith("$.") ? key[2..] : key.TrimStart('.');

    private static string GetErrorMessage(ModelError error) =>
        string.IsNullOrWhiteSpace(error.ErrorMessage) && error.Exception != null
            ? error.Exception.Message
            : error.ErrorMessage;

    private static bool IsBodyField(object errorObj) =>
        errorObj.GetType().GetProperty("field")?.GetValue(errorObj) is string field &&
        string.Equals(field, "body", StringComparison.OrdinalIgnoreCase);
}
