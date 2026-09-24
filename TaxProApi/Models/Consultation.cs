using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public enum ConsultationMode
{
    Online,
    FaceToFace
}

public enum ConsultationStatus
{
    PendingPayment,
    PaymentSubmitted,
    PaymentVerified,
    PaymentRejected,
    Confirmed,
    Completed,
    Cancelled
}

public class Consultation
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string ConsultationReference { get; set; } = string.Empty;

    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    public int? ServiceId { get; set; }
    public Service? Service { get; set; }

    public ConsultationMode Mode { get; set; } = ConsultationMode.Online;

    [Required, MaxLength(200)]
    public string ConsultationType { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [MaxLength(100)]
    public string? TimeSlot { get; set; }

    [Required, MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ClientPhone { get; set; }

    [MaxLength(200)]
    public string? ClientEmail { get; set; }

    [MaxLength(200)]
    public string? ClientCompany { get; set; }

    public ConsultationStatus Status { get; set; } = ConsultationStatus.PendingPayment;

    [MaxLength(500)]
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    // Navigation
    public Payment? Payment { get; set; }
    public Receipt? Receipt { get; set; }
}
