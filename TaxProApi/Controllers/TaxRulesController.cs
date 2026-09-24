using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.Models;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api")]
public class TaxRulesController : ControllerBase
{
    private readonly AppDbContext _db;
    public TaxRulesController(AppDbContext db) => _db = db;

    // GET /api/tax-slabs — Public (used by frontend calculator)
    [HttpGet("tax-slabs")]
    public async Task<IActionResult> GetTaxSlabs([FromQuery] string year = "2024-2025")
    {
        var rules = await _db.TaxRules
            .AsNoTracking()
            .Where(r => r.TaxYear == year && r.IsActive)
            .OrderBy(r => r.EntityType)
            .ThenBy(r => r.MinIncome)
            .ToListAsync();

        if (!rules.Any())
        {
            // Return hardcoded defaults if DB has no rules (matches existing frontend)
            return Ok(new
            {
                taxYear = "2024-2025",
                statute = "Finance Act 2024 (Income Tax Ordinance 2001)",
                salaried = new[]
                {
                    new { min = 0, max = 600000, rate = 0.0, @base = 0 },
                    new { min = 600000, max = 1200000, rate = 0.05, @base = 0 },
                    new { min = 1200000, max = 2200000, rate = 0.15, @base = 30000 },
                    new { min = 2200000, max = 3200000, rate = 0.25, @base = 180000 },
                    new { min = 3200000, max = 4100000, rate = 0.30, @base = 430000 },
                    new { min = 4100000, max = -1, rate = 0.35, @base = 700000 }
                },
                business = new[]
                {
                    new { min = 0, max = 600000, rate = 0.0, @base = 0 },
                    new { min = 600000, max = 1200000, rate = 0.15, @base = 0 },
                    new { min = 1200000, max = 1600000, rate = 0.20, @base = 90000 },
                    new { min = 1600000, max = 3200000, rate = 0.30, @base = 170000 },
                    new { min = 3200000, max = 5600000, rate = 0.40, @base = 650000 },
                    new { min = 5600000, max = -1, rate = 0.45, @base = 1610000 }
                },
                corporateRate = 0.29
            });
        }

        var grouped = rules.GroupBy(r => r.EntityType).ToDictionary(g => g.Key, g => g.ToList());

        return Ok(new
        {
            taxYear = year,
            statute = rules.First().Statute,
            rules = rules.Select(r => new
            {
                entityType = r.EntityType.ToString(),
                min = r.MinIncome,
                max = r.MaxIncome,
                rate = r.Rate,
                baseAmount = r.BaseAmount
            })
        });
    }

    // GET /api/tax-rules — Admin
    [HttpGet("tax-rules")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var rules = await _db.TaxRules.AsNoTracking().OrderBy(r => r.TaxYear).ThenBy(r => r.EntityType).ThenBy(r => r.MinIncome).ToListAsync();
        return Ok(rules);
    }

    // POST /api/tax-rules — Admin
    [HttpPost("tax-rules")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TaxRule rule)
    {
        rule.CreatedAt = DateTime.UtcNow;
        _db.TaxRules.Add(rule);
        await _db.SaveChangesAsync();
        return StatusCode(201, rule);
    }

    // PUT /api/tax-rules/{id} — Admin
    [HttpPut("tax-rules/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] TaxRule rule)
    {
        var existing = await _db.TaxRules.FindAsync(id);
        if (existing == null) return NotFound();
        existing.TaxYear = rule.TaxYear;
        existing.Statute = rule.Statute;
        existing.EntityType = rule.EntityType;
        existing.MinIncome = rule.MinIncome;
        existing.MaxIncome = rule.MaxIncome;
        existing.Rate = rule.Rate;
        existing.BaseAmount = rule.BaseAmount;
        existing.IsActive = rule.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    // DELETE /api/tax-rules/{id} — Admin
    [HttpDelete("tax-rules/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var rule = await _db.TaxRules.FindAsync(id);
        if (rule == null) return NotFound();
        _db.TaxRules.Remove(rule);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
