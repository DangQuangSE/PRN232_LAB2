using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMSSystem.API.Helpers;

/// <summary>
/// Automatically adds standard error responses to every endpoint based on HTTP method and path,
/// mirroring what GlobalExceptionMiddleware returns at runtime.
/// </summary>
public class DefaultResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod?.ToUpper();
        var hasIdSegment = context.ApiDescription.RelativePath?.Contains("{id}") == true;

        // 500 can occur on any endpoint
        operation.Responses.TryAdd("500", new OpenApiResponse
        {
            Description = "Internal Server Error — an unexpected error occurred on the server"
        });

        // 400 on write operations that accept a request body
        if (method is "POST" or "PUT")
        {
            operation.Responses.TryAdd("400", new OpenApiResponse
            {
                Description = "Bad Request — invalid or missing input data"
            });
        }

        // 404 on any endpoint that targets a specific resource by ID
        if (hasIdSegment && method is "GET" or "PUT" or "DELETE")
        {
            operation.Responses.TryAdd("404", new OpenApiResponse
            {
                Description = "Not Found — the requested resource does not exist"
            });
        }
    }
}
