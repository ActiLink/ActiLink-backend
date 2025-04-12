using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ActiLink
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the action or its declaring controller has the [Authorize] attribute.
            var hasAuthorize = context.MethodInfo.DeclaringType!.GetCustomAttributes(true)
                                    .OfType<AuthorizeAttribute>().Any() ||
                               context.MethodInfo.GetCustomAttributes(true)
                                    .OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize)
                return;

            // Ensure operation.Security is initialized
            operation.Security ??= [];

            // Add the security requirement for JWT Bearer.
            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [securityScheme] = []
            });
        }
    }
}