using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RentMat.API.Common.Security;

internal class RemoveSecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuth =
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
            context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

        // 2. Check if [AllowAnonymous] overrides it
        var hasAllowAnonymous =
            context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();
        
        // 3. Additive strategy: Inject the requirement ONLY on valid secured endpoints
        if (hasAuth && !hasAllowAnonymous)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            // For modern Swashbuckle/OpenApi, pass context.Document into the reference
            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            };

            operation.Security.Add(requirement);
        }
        else
        {
            // Explicitly clear it for anonymous paths to prevent global bleeding
            operation.Security = new List<OpenApiSecurityRequirement>();
        }
    }
}