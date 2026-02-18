using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class AcceptLanguageHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Idioma da resposta (pt-BR | en-US)",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("pt-BR")
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Access-Key",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Key de acesso",
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("FSDF4523GKIOP13Y642F526109A")
            }
        });
    }
}
