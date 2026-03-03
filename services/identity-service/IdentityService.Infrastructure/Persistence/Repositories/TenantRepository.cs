using IdentityService.Domain.Entities;
using IdentityService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _context;

    public TenantRepository(IdentityDbContext context)
        => _context = context;

    public async Task<Tenant?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tenant?> GetBySubdomainAsync(
        string subdomain, CancellationToken ct = default)
        => await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain.ToLower(), ct);

    public async Task<bool> SubdomainExistsAsync(
        string subdomain, CancellationToken ct = default)
        => await _context.Tenants
            .AnyAsync(t => t.Subdomain == subdomain.ToLower(), ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        await _context.Tenants.AddAsync(tenant, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(ct);
    }
}