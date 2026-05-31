using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMSSystem.API.Helpers;

public class LowercaseQueryParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter.In == ParameterLocation.Query && parameter.Name.Length > 0)
            parameter.Name = char.ToLower(parameter.Name[0]) + parameter.Name[1..];
    }
}
