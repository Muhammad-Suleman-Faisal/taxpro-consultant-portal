using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServicesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _db.Services.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Select(s => new { s.Id, s.Name, s.Description, s.Icon, s.Price })
            .ToListAsync();
        return Ok(services);
    }
}
