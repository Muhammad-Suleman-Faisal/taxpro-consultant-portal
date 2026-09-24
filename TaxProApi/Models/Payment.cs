using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public enum PaymentStatus
{
    Pending,
    Submitted,
    Verified,
    Rejected
}

public enum PaymentMethod
{
    RaastIBFT,
    BankTransfer,
    CreditCard,
    EasyPaisa,
    JazzCash,
    Other
}

public class Payment
{
    public int Id { get; set; }

    [Required, MaxLength(60)]
    public string PaymentReference { get; set; } = string.Empty;

    public int ConsultationId { get; set; }
    public Consultation Consultation { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;

    [MaxLength(200)]
    public string? TransactionReference { get; set; }

    public DateTime? PaymentDate { get; set; }

    [MaxLength(500)]
    public string? ProofFilePath { get; set; }

    [MaxLength(100)]
    public string? ProofFileName { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(500)]
    public string? AdminRemarks { get; set; }

    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByAdminId { get; set; }

    public DateTime? RejectedAt { get; set; }
    public int? RejectedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Receipt? Receipt { get; set; }
}
