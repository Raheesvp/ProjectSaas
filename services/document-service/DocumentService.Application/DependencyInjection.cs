using DocumentService.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentService.Application;

// Application layer DI registration
// Called from DocumentService.API Program.cs
// Registers MediatR, FluentValidation, ValidationBehavior
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // MediatR — scans this assembly for all handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                AssemblyReference.Assembly));

        // FluentValidation — scans for all validators
        services.AddValidatorsFromAssembly(
            AssemblyReference.Assembly);

        // Validation pipeline — runs before every command
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}