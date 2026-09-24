using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.Models;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    // --- Dashboard Stats ---
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var totalConsultations = await _db.Consultations.CountAsync();
        var pendingPayments = await _db.Consultations.CountAsync(c => c.Status == ConsultationStatus.PaymentSubmitted);
        var confirmedToday = await _db.Consultations.CountAsync(c => c.ConfirmedAt.HasValue && c.ConfirmedAt.Value.Date == DateTime.UtcNow.Date);
        var totalInquiries = await _db.Inquiries.CountAsync();
        var newInquiries = await _db.Inquiries.CountAsync(i => i.Status == InquiryStatus.New);
        var totalAppointments = await _db.Appointments.CountAsync();
        var totalReceipts = await _db.Receipts.CountAsync();
        var totalRevenue = await _db.Payments.Where(p => p.Status == PaymentStatus.Verified).SumAsync(p => p.Amount);

        return Ok(new
        {
            totalConsultations,
            pendingPayments,
            confirmedToday,
            totalInquiries,
            newInquiries,
            totalAppointments,
            totalReceipts,
            totalRevenue
        });
    }

    // --- Clients ---
    [HttpGet("clients")]
    public async Task<IActionResult> GetClients([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Clients.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("clients/{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
        var c = await _db.Clients.FindAsync(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] Client client)
    {
        client.CreatedAt = DateTime.UtcNow;
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return StatusCode(201, client);
    }

    [HttpPut("clients/{id}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] Client updated)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null) return NotFound();
        client.FullName = updated.FullName;
        client.Email = updated.Email;
        client.Phone = updated.Phone;
        client.Company = updated.Company;
        client.NTN = updated.NTN;
        await _db.SaveChangesAsync();
        return Ok(client);
    }

    // --- Services ---
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var services = await _db.Services.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync();
        return Ok(services);
    }

    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] Service service)
    {
        service.CreatedAt = DateTime.UtcNow;
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return StatusCode(201, service);
    }

    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] Service updated)
    {
        var s = await _db.Services.FindAsync(id);
        if (s == null) return NotFound();
        s.Name = updated.Name;
        s.Description = updated.Description;
        s.Icon = updated.Icon;
        s.Price = updated.Price;
        s.IsActive = updated.IsActive;
        s.SortOrder = updated.SortOrder;
        await _db.SaveChangesAsync();
        return Ok(s);
    }

    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var s = await _db.Services.FindAsync(id);
        if (s == null) return NotFound();
        s.IsActive = false; // Soft delete
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // --- FAQs ---
    [HttpGet("faqs")]
    public async Task<IActionResult> GetFAQs()
    {
        var faqs = await _db.FAQs.AsNoTracking().OrderBy(f => f.SortOrder).ToListAsync();
        return Ok(faqs);
    }

    [HttpPost("faqs")]
    public async Task<IActionResult> CreateFAQ([FromBody] FAQ faq)
    {
        faq.CreatedAt = DateTime.UtcNow;
        _db.FAQs.Add(faq);
        await _db.SaveChangesAsync();
        return StatusCode(201, faq);
    }

    [HttpPut("faqs/{id}")]
    public async Task<IActionResult> UpdateFAQ(int id, [FromBody] FAQ updated)
    {
        var f = await _db.FAQs.FindAsync(id);
        if (f == null) return NotFound();
        f.Question = updated.Question;
        f.Answer = updated.Answer;
        f.Category = updated.Category;
        f.SortOrder = updated.SortOrder;
        f.IsActive = updated.IsActive;
        f.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(f);
    }

    [HttpDelete("faqs/{id}")]
    public async Task<IActionResult> DeleteFAQ(int id)
    {
        var f = await _db.FAQs.FindAsync(id);
        if (f == null) return NotFound();
        _db.FAQs.Remove(f);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // --- Blog Posts / Resources ---
    [HttpGet("resources")]
    public async Task<IActionResult> GetResources()
    {
        var posts = await _db.BlogPosts.AsNoTracking().OrderByDescending(b => b.CreatedAt).ToListAsync();
        return Ok(posts);
    }

    [HttpPost("resources")]
    public async Task<IActionResult> CreateResource([FromBody] BlogPost post)
    {
        post.CreatedAt = DateTime.UtcNow;
        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();
        return StatusCode(201, post);
    }

    [HttpPut("resources/{id}")]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] BlogPost updated)
    {
        var b = await _db.BlogPosts.FindAsync(id);
        if (b == null) return NotFound();
        b.Title = updated.Title;
        b.Category = updated.Category;
        b.ReadTime = updated.ReadTime;
        b.Content = updated.Content;
        b.Summary = updated.Summary;
        b.IsPublished = updated.IsPublished;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(b);
    }

    [HttpDelete("resources/{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var b = await _db.BlogPosts.FindAsync(id);
        if (b == null) return NotFound();
        _db.BlogPosts.Remove(b);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // --- Receipts ---
    [HttpGet("receipts")]
    public async Task<IActionResult> GetReceipts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Receipts.AsNoTracking().Include(r => r.Consultation).OrderByDescending(r => r.IssuedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    // --- Payments ---
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = _db.Payments.AsNoTracking().Include(p => p.Consultation).AsQueryable();
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var s))
            query = query.Where(p => p.Status == s);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }
}
