using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ContentManager,Viewer")]
public class PricingController(IPricingService pricingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTiers()
    {
        var tiers = await pricingService.GetAllTiersAsync();
        return Ok(tiers);
    }

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedTiers()
    {
        var tiers = await pricingService.GetPublishedTiersAsync();
        return Ok(tiers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTierById(int id)
    {
        var tier = await pricingService.GetTierByIdAsync(id);
        if (tier == null)
            return NotFound(new { message = "Pricing tier not found" });
        return Ok(tier);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> CreateTier([FromBody] CreatePricingTierDto dto)
    {
        var tier = await pricingService.CreateTierAsync(dto);
        return CreatedAtAction(nameof(GetTierById), new { id = tier.Id }, tier);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UpdateTier(int id, [FromBody] UpdatePricingTierDto dto)
    {
        var tier = await pricingService.UpdateTierAsync(id, dto);
        if (tier == null)
            return NotFound(new { message = "Pricing tier not found" });
        return Ok(tier);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> DeleteTier(int id)
    {
        var result = await pricingService.DeleteTierAsync(id);
        if (!result)
            return NotFound(new { message = "Pricing tier not found" });
        return Ok(new { message = "Pricing tier deleted successfully" });
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> PublishTier(int id)
    {
        var result = await pricingService.PublishTierAsync(id);
        if (!result)
            return NotFound(new { message = "Pricing tier not found" });
        return Ok(new { message = "Pricing tier published successfully" });
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UnpublishTier(int id)
    {
        var result = await pricingService.UnpublishTierAsync(id);
        if (!result)
            return NotFound(new { message = "Pricing tier not found" });
        return Ok(new { message = "Pricing tier unpublished successfully" });
    }
}
