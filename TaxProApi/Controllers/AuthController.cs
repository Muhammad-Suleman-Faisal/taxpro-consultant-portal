using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.DTOs;
using TaxProApi.Services;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthController(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var admin = await _db.AdminUsers
            .FirstOrDefaultAsync(a => a.Username == req.Username && a.IsActive);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(req.Password, admin.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        admin.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(admin);
        var expiry = DateTime.UtcNow.AddHours(12);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = admin.Username,
            ExpiresAt = expiry
        });
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var admin = await _db.AdminUsers.FindAsync(adminId);
        if (admin == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, admin.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password updated successfully." });
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public IActionResult Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name,
            role = "Admin"
        });
    }
}
