using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed,
    NoShow
}

public class Appointment
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string BookingReference { get; set; } = string.Empty;

    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    public int? ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required, MaxLength(200)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    public DateTime AppointmentDate { get; set; }

    [MaxLength(100)]
    public string? TimeSlot { get; set; }

    [Required, MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ClientPhone { get; set; }

    [MaxLength(200)]
    public string? ClientCompany { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
