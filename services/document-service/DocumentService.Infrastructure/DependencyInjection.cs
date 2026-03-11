using DocumentService.Application.Interfaces;
using DocumentService.Domain.Repositories;
using DocumentService.Infrastructure.Persistence;
using DocumentService.Infrastructure.Persistence.Interceptors;
using DocumentService.Infrastructure.Stubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<DocumentDbContext>(
            (serviceProvider, options) =>
            {
                var interceptor = serviceProvider
                    .GetRequiredService<DomainEventInterceptor>();

                options
                    .UseNpgsql(
                        configuration
                            .GetConnectionString("DocumentDb"),
                        npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorCodesToAdd: null);
                        })
                    .AddInterceptors(interceptor);
            });

        services.AddScoped<IDocumentRepository, StubDocumentRepository>();
        services.AddScoped<IDocumentReadRepository, StubDocumentReadRepository>();
        services.AddScoped<IStorageService, StubStorageService>();

        return services;
    }
}
