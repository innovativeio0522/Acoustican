using Acoustican.Data;
using Acoustican.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeroController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHeroContent()
    {
        var content = await context.HeroContents
            .Where(h => h.IsActive)
            .OrderByDescending(h => h.UpdatedAt)
            .FirstOrDefaultAsync();

        if (content == null)
            return NotFound(new { message = "Hero content not found" });

        return Ok(content);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UpdateHeroContent([FromBody] HeroContent content)
    {
        var existing = await context.HeroContents.FindAsync(content.Id);
        if (existing == null)
        {
            context.HeroContents.Add(content);
        }
        else
        {
            context.Entry(existing).CurrentValues.SetValues(content);
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return Ok(content);
    }
}