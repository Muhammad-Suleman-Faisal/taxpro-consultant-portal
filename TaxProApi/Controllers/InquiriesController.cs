using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.DTOs;
using TaxProApi.Models;
using TaxProApi.Services;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/inquiries")]
public class InquiriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public InquiriesController(AppDbContext db) => _db = db;

    // POST /api/inquiries — Public: submit inquiry
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInquiryRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Name is required." });

        var inqRef = ReferenceGenerator.InquiryRef();
        while (await _db.Inquiries.AnyAsync(i => i.InquiryReference == inqRef))
            inqRef = ReferenceGenerator.InquiryRef();

        var inquiry = new Inquiry
        {
            InquiryReference = inqRef,
            Name = req.Name,
            Email = req.Email,
            Phone = req.Phone,
            BusinessType = req.BizType,
            ServiceInterest = req.Service,
            Message = req.Message,
            Status = InquiryStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _db.Inquiries.Add(inquiry);
        await _db.SaveChangesAsync();

        return StatusCode(201, new InquiryResponse
        {
            Success = true,
            Message = "Official inquiry recorded.",
            Inquiry = new InquiryDetail
            {
                Id = inqRef,
                Name = req.Name,
                Email = req.Email,
                Phone = req.Phone,
                BizType = req.BizType,
                Service = req.Service,
                Message = req.Message,
                CreatedAt = inquiry.CreatedAt
            }
        });
    }

    // GET /api/inquiries — Admin only
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = _db.Inquiries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<InquiryStatus>(status, true, out var s))
            query = query.Where(i => i.Status == s);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // GET /api/inquiries/{id} — Admin only
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var i = await _db.Inquiries.FindAsync(id);
        if (i == null) return NotFound();
        return Ok(i);
    }

    // PUT /api/inquiries/{id} — Admin: update status & response
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInquiryRequest req)
    {
        var i = await _db.Inquiries.FindAsync(id);
        if (i == null) return NotFound();

        if (!string.IsNullOrEmpty(req.Status) && Enum.TryParse<InquiryStatus>(req.Status, true, out var s))
            i.Status = s;

        if (!string.IsNullOrEmpty(req.AdminResponse))
            i.AdminResponse = req.AdminResponse;

        i.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(i);
    }
}

public class UpdateInquiryRequest
{
    public string? Status { get; set; }
    public string? AdminResponse { get; set; }
}
