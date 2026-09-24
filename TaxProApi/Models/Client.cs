using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public class Client
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(50)]
    public string? NTN { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
}
