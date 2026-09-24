using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public enum InquiryStatus
{
    New,
    InProgress,
    Resolved,
    Closed
}

public class Inquiry
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string InquiryReference { get; set; } = string.Empty;

    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? BusinessType { get; set; }

    [MaxLength(200)]
    public string? ServiceInterest { get; set; }

    [MaxLength(2000)]
    public string? Message { get; set; }

    public InquiryStatus Status { get; set; } = InquiryStatus.New;

    [MaxLength(1000)]
    public string? AdminResponse { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
