namespace SmartGate.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseRateLimiter();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorizationProblemDetails();
        app.UseAuthorization();
        app.MapHealthChecks("/health");
        app.MapControllers();
        return app;
    }

    private static WebApplication UseAuthorizationProblemDetails(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await next();

            if (context.Response.StatusCode == 403)
            {
                context.Response.Body = originalBodyStream;
                context.Response.ContentType = "application/problem+json";
                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    title = "Forbidden",
                    status = 403,
                    detail = "Insufficient permissions."
                };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problem));
            }
            else
            {
                context.Response.Body = originalBodyStream;
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        });
        return app;
    }
}