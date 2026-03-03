using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u=>u.TenantId)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired().HasMaxLength(255);

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        builder.Property(u=>u.PasswordHash).IsRequired();

        builder.Property(u=>u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany(u=>u.RefreshTokens)
            .WithOne(rt=>rt.User)
            .HasForeignKey(rt=>rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting tenant if users exist

        
    }
    
}
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        // Index for fast lookup during token validation
        builder.HasIndex(rt => rt.Token);

        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.IsRevoked).IsRequired();
        builder.Property(rt => rt.RevokedReason).HasMaxLength(200);
    }
}