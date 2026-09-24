using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.Models;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/payment-settings")]
public class PaymentSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PaymentSettingsController(AppDbContext db) => _db = db;

    // GET /api/payment-settings/bank-details — Public: safe bank details for frontend display
    [HttpGet("bank-details")]
    public async Task<IActionResult> GetBankDetails()
    {
        var safeKeys = new[] { "bank_name", "account_title", "account_number", "iban", "branch_code", "raast_id",
                               "consultation_fee_online", "consultation_fee_f2f" };

        var settings = await _db.PaymentSettings
            .AsNoTracking()
            .Where(ps => safeKeys.Contains(ps.Key) && ps.IsActive)
            .ToDictionaryAsync(ps => ps.Key, ps => ps.Value);

        return Ok(settings);
    }

    // GET /api/payment-settings — Admin: all settings
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var settings = await _db.PaymentSettings.AsNoTracking().OrderBy(ps => ps.Key).ToListAsync();
        return Ok(settings);
    }

    // PUT /api/payment-settings/{key} — Admin: update setting
    [HttpPut("{key}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateSettingRequest req)
    {
        var setting = await _db.PaymentSettings.FirstOrDefaultAsync(ps => ps.Key == key);
        if (setting == null)
        {
            // Create new setting
            setting = new PaymentSetting { Key = key, Value = req.Value, Description = req.Description };
            _db.PaymentSettings.Add(setting);
        }
        else
        {
            setting.Value = req.Value;
            if (!string.IsNullOrEmpty(req.Description))
                setting.Description = req.Description;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(setting);
    }

    // POST /api/payment-settings — Admin: bulk update
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate([FromBody] Dictionary<string, string> settings)
    {
        foreach (var (key, value) in settings)
        {
            var existing = await _db.PaymentSettings.FirstOrDefaultAsync(ps => ps.Key == key);
            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync();
        return Ok(new { message = "Settings updated." });
    }
}

public class UpdateSettingRequest
{
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
