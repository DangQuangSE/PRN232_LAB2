using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace PRN232.LMSSystem.API.Helpers;

public class ExpandOptionsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attr = context.MethodInfo.GetCustomAttribute<ExpandOptionsAttribute>();
        if (attr == null || attr.Options.Length == 0) return;

        var expandParam = operation.Parameters?.FirstOrDefault(p => p.Name == "expand");
        if (expandParam == null) return;

        var values = string.Join(", ", attr.Options.Select(o => $"'{o}'"));
        expandParam.Description = $"Comma-separated related entities to expand. Available values: {values}. Example: \"{string.Join(",", attr.Options)}\".";
    }
}
