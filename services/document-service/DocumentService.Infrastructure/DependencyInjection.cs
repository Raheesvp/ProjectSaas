using DocumentService.Application.Interfaces;
using DocumentService.Domain.Repositories;
using DocumentService.Infrastructure.Persistence;
using DocumentService.Infrastructure.Persistence.Interceptors;
using DocumentService.Infrastructure.Services;
using DocumentService.Infrastructure.Stubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Common;

namespace DocumentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ITenantContext, HttpTenantContext>();

        services.AddScoped<DomainEventInterceptor>();
        services.AddScoped<TenantDbCommandInterceptor>();

        services.AddDbContext<DocumentDbContext>(
            (serviceProvider, options) =>
            {
                var domainEventInterceptor = serviceProvider
                    .GetRequiredService<DomainEventInterceptor>();

                var tenantInterceptor = serviceProvider
                    .GetRequiredService<TenantDbCommandInterceptor>();

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
                    .AddInterceptors(
                        domainEventInterceptor,
                        tenantInterceptor);
            });

        services.AddScoped<IDocumentRepository,
            StubDocumentRepository>();
        services.AddScoped<IDocumentReadRepository,
            StubDocumentReadRepository>();
        services.AddScoped<IStorageService,
            StubStorageService>();

        return services;
    }
}
