using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface IPricingService
{
    Task<List<PricingTierDto>> GetAllTiersAsync();
    Task<List<PricingTierDto>> GetPublishedTiersAsync();
    Task<PricingTierDto?> GetTierByIdAsync(int id);
    Task<PricingTierDto> CreateTierAsync(CreatePricingTierDto dto);
    Task<PricingTierDto?> UpdateTierAsync(int id, UpdatePricingTierDto dto);
    Task<bool> DeleteTierAsync(int id);
    Task<bool> PublishTierAsync(int id);
    Task<bool> UnpublishTierAsync(int id);
}

public class PricingService : IPricingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PricingService> _logger;

    public PricingService(ApplicationDbContext context, IMapper mapper, ILogger<PricingService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<PricingTierDto>> GetAllTiersAsync()
    {
        var tiers = await _context.PricingTiers
            .Include(p => p.Features)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
        return _mapper.Map<List<PricingTierDto>>(tiers);
    }

    public async Task<List<PricingTierDto>> GetPublishedTiersAsync()
    {
        var tiers = await _context.PricingTiers
            .Include(p => p.Features)
            .Where(p => p.IsPublished)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync();
        return _mapper.Map<List<PricingTierDto>>(tiers);
    }

    public async Task<PricingTierDto?> GetTierByIdAsync(int id)
    {
        var tier = await _context.PricingTiers
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == id);
        return _mapper.Map<PricingTierDto>(tier);
    }

    public async Task<PricingTierDto> CreateTierAsync(CreatePricingTierDto dto)
    {
        if (dto.BillingPeriod != "monthly" && dto.BillingPeriod != "annually")
        {
            throw new ArgumentException("Billing period must be 'monthly' or 'annually'");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tier = _mapper.Map<PricingTier>(dto);
            _context.PricingTiers.Add(tier);
            await _context.SaveChangesAsync();

            foreach (var feature in dto.Features)
            {
                _context.PricingFeatures.Add(new PricingFeature
                {
                    PricingTierId = tier.Id,
                    Feature = feature,
                    IsIncluded = true,
                    DisplayOrder = dto.Features.IndexOf(feature)
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            tier = await _context.PricingTiers.Include(p => p.Features).FirstAsync(p => p.Id == tier.Id);
            return _mapper.Map<PricingTierDto>(tier);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating pricing tier");
            throw;
        }
    }

    public async Task<PricingTierDto?> UpdateTierAsync(int id, UpdatePricingTierDto dto)
    {
        if (dto.BillingPeriod != "monthly" && dto.BillingPeriod != "annually")
        {
            throw new ArgumentException("Billing period must be 'monthly' or 'annually'");
        }

        var tier = await _context.PricingTiers
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (tier == null) return null;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _mapper.Map(dto, tier);
            tier.UpdatedAt = DateTime.UtcNow;
            _context.PricingTiers.Update(tier);

            // Remove old features and add new ones
            _context.PricingFeatures.RemoveRange(tier.Features);
            await _context.SaveChangesAsync();

            foreach (var feature in dto.Features)
            {
                _context.PricingFeatures.Add(new PricingFeature
                {
                    PricingTierId = tier.Id,
                    Feature = feature,
                    IsIncluded = true,
                    DisplayOrder = dto.Features.IndexOf(feature)
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            tier = await _context.PricingTiers.Include(p => p.Features).FirstAsync(p => p.Id == id);
            return _mapper.Map<PricingTierDto>(tier);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating pricing tier");
            throw;
        }
    }

    public async Task<bool> DeleteTierAsync(int id)
    {
        var tier = await _context.PricingTiers.FindAsync(id);
        if (tier == null) return false;

        _context.PricingTiers.Remove(tier);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishTierAsync(int id)
    {
        var tier = await _context.PricingTiers.FindAsync(id);
        if (tier == null) return false;

        tier.IsPublished = true;
        tier.UpdatedAt = DateTime.UtcNow;
        _context.PricingTiers.Update(tier);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishTierAsync(int id)
    {
        var tier = await _context.PricingTiers.FindAsync(id);
        if (tier == null) return false;

        tier.IsPublished = false;
        tier.UpdatedAt = DateTime.UtcNow;
        _context.PricingTiers.Update(tier);
        await _context.SaveChangesAsync();
        return true;
    }
}
