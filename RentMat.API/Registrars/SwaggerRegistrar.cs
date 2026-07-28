using Microsoft.OpenApi;
using RentMat.API.Common.Security;

namespace RentMat.API.Registrars;

internal static class SwaggerRegistrar
{
    public static IServiceCollection RegisterSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
            
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });
        return services;
    }
}