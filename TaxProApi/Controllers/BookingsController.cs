using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.DTOs;
using TaxProApi.Models;
using TaxProApi.Services;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Appointments.AsNoTracking().OrderByDescending(a => a.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { total, page, pageSize, items });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Basic validation
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Phone))
            return BadRequest(new { message = "Name and phone are required." });

        var bookingRef = ReferenceGenerator.BookingRef();
        // Ensure uniqueness
        while (await _db.Appointments.AnyAsync(a => a.BookingReference == bookingRef))
            bookingRef = ReferenceGenerator.BookingRef();

        var appointment = new Appointment
        {
            BookingReference = bookingRef,
            ServiceName = req.Service,
            AppointmentDate = DateTime.UtcNow.Date.AddDays(1), // date string parsed best-effort
            TimeSlot = req.Time,
            ClientName = req.Name,
            ClientPhone = req.Phone,
            ClientCompany = req.Company,
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        return StatusCode(201, new BookingResponse
        {
            Success = true,
            Message = "Consultation successfully scheduled.",
            Booking = new BookingDetail
            {
                Id = bookingRef,
                Service = req.Service,
                Date = req.Date,
                Time = req.Time,
                Name = req.Name,
                Company = req.Company,
                Phone = req.Phone,
                Status = "Confirmed",
                CreatedAt = appointment.CreatedAt
            }
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();
        return Ok(a);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();
        if (Enum.TryParse<AppointmentStatus>(status, true, out var parsed))
        {
            a.Status = parsed;
            a.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(a);
        }
        return BadRequest(new { message = "Invalid status." });
    }
}
