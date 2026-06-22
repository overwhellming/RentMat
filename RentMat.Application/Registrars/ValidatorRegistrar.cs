using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RentMat.Application.Validators;
using RentMat.Application.Validators.Authentication;

namespace RentMat.Application.Registrars;

public static class ValidatorRegistrar
{
    public static IServiceCollection RegisterValidators(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

        return services;
    }
}