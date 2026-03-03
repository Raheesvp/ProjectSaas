
using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;


public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();
        
        builder.Property(t => t.ContactEmail)
            .IsRequired().HasMaxLength(255);

        builder.Property(t => t.IsActive)
            .IsRequired().HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired();
    } 
}