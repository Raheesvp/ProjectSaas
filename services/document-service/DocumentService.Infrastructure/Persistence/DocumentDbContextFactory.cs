using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DocumentService.Infrastructure.Persistence;

public sealed class DocumentDbContextFactory
    : IDesignTimeDbContextFactory<DocumentDbContext>
{
    public DocumentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "DocumentService.API"))
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder =
            new DbContextOptionsBuilder<DocumentDbContext>();

        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("DocumentDb"));

        return new DocumentDbContext(optionsBuilder.Options);
    }
}
