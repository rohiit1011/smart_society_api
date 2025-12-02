
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocietyManagementAPI.Helpers
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasFileParam = context.MethodInfo.GetParameters()
                .Any(p => p.ParameterType == typeof(IFormFile));

            if (!hasFileParam) return; // Only affects methods with IFormFile

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
    {
        ["multipart/form-data"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                Properties = context.MethodInfo.GetParameters()
                    .ToDictionary(
                        p => p.Name,
                        p => p.ParameterType == typeof(IFormFile)
                            ? new OpenApiSchema { Type = "string", Format = "binary" }
                            : new OpenApiSchema { Type = "string" }
                    ),
                Required = context.MethodInfo.GetParameters()
                    .Select(p => p.Name)
                    .ToHashSet()   // <-- here
            }
        }
    }
            };

        }
    }

}
