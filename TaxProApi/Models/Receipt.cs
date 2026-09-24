using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public class Receipt
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public int ConsultationId { get; set; }
    public Consultation Consultation { get; set; } = null!;

    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;

    // Snapshot fields (preserved even if original data changes)
    [Required, MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ClientEmail { get; set; }

    [MaxLength(30)]
    public string? ClientPhone { get; set; }

    [MaxLength(200)]
    public string? ClientCompany { get; set; }

    [Required, MaxLength(200)]
    public string ServiceDescription { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    [MaxLength(200)]
    public string? TransactionReference { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? PdfFilePath { get; set; }

    // Navigation
    public string? ConsultationMode { get; set; }
    public string? AppointmentDetails { get; set; }
}
