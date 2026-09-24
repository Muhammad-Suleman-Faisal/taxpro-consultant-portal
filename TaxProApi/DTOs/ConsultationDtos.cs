using System.ComponentModel.DataAnnotations;
using TaxProApi.Models;

namespace TaxProApi.DTOs;

// --- Booking (simple appointment, no payment) ---
public class CreateBookingRequest
{
    [Required] public string Service { get; set; } = string.Empty;
    [Required] public string Date { get; set; } = string.Empty;
    [Required] public string Time { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    [Required] public string Phone { get; set; } = string.Empty;
}

public class BookingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public BookingDetail? Booking { get; set; }
}

public class BookingDetail
{
    public string Id { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// --- Consultation (with advance payment) ---
public class CreateConsultationRequest
{
    [Required] public string Mode { get; set; } = string.Empty; // "online" | "face_to_face"
    [Required] public string ConsultationType { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public string Date { get; set; } = string.Empty;
    [Required] public string Time { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    [Required] public ConsultationPaymentInfo Payment { get; set; } = null!;
}

public class ConsultationPaymentInfo
{
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? TransactionRef { get; set; }
}

public class ConsultationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ConsultationDetail? Consultation { get; set; }
}

public class ConsultationDetail
{
    public string Id { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string ConsultationType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string Status { get; set; } = string.Empty;
    public PaymentDetailDto? Payment { get; set; }
    public string? PrivacyNotice { get; set; }
    public string? MeetingDetails { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentDetailDto
{
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
}

// --- Payment verification (admin) ---
public class VerifyPaymentRequest
{
    [Required] public string Action { get; set; } = string.Empty; // "verify" | "reject"
    public string? Remarks { get; set; }
}

// --- Upload proof ---
// (handled as IFormFile in controller)

// --- Receipt ---
public class ReceiptDto
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string ConsultationReference { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
    public string? ClientCompany { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string ConsultationMode { get; set; } = string.Empty;
    public string? AppointmentDetails { get; set; }
    public DateTime IssuedAt { get; set; }
}

// --- Inquiry ---
public class CreateInquiryRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BizType { get; set; }
    public string? Service { get; set; }
    public string? Message { get; set; }
}

public class InquiryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public InquiryDetail? Inquiry { get; set; }
}

public class InquiryDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? BizType { get; set; }
    public string? Service { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
