using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

public sealed class AcceptLanguageHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Idioma da resposta (pt-BR | en-US)",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Default = JsonValue.Create("pt-BR")
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
                Type = JsonSchemaType.String,
                Default = JsonValue.Create("FSDF4523GKIOP13Y642F526109A")
            }
        });
    }
}
