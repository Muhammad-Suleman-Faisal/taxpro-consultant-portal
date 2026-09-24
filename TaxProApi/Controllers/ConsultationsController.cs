using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxProApi.Data;
using TaxProApi.DTOs;
using TaxProApi.Models;
using TaxProApi.Services;

namespace TaxProApi.Controllers;

[ApiController]
[Route("api/consultations")]
public class ConsultationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IReceiptService _receiptService;
    private readonly IWebHostEnvironment _env;

    public ConsultationsController(AppDbContext db, IReceiptService receiptService, IWebHostEnvironment env)
    {
        _db = db;
        _receiptService = receiptService;
        _env = env;
    }

    // POST /api/consultations — Public: submit consultation + payment info
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConsultationRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Phone) || string.IsNullOrWhiteSpace(req.Email))
            return BadRequest(new { message = "Name, phone, and email are required." });

        if (req.Payment == null || req.Payment.Amount <= 0)
            return BadRequest(new { message = "Valid payment information is required." });

        var mode = req.Mode.ToLower() == "face_to_face" ? ConsultationMode.FaceToFace : ConsultationMode.Online;

        // Generate unique refs
        var consultRef = ReferenceGenerator.ConsultationRef();
        while (await _db.Consultations.AnyAsync(c => c.ConsultationReference == consultRef))
            consultRef = ReferenceGenerator.ConsultationRef();

        var payRef = ReferenceGenerator.PaymentRef();
        while (await _db.Payments.AnyAsync(p => p.PaymentReference == payRef))
            payRef = ReferenceGenerator.PaymentRef();

        // Parse payment method
        PaymentMethod payMethod = req.Payment.Method.ToLower() switch
        {
            "raast_ibft" => PaymentMethod.RaastIBFT,
            "card" => PaymentMethod.CreditCard,
            "easypaisa" => PaymentMethod.EasyPaisa,
            "jazzcash" => PaymentMethod.JazzCash,
            "bank_transfer" => PaymentMethod.BankTransfer,
            _ => PaymentMethod.BankTransfer
        };

        var consultation = new Consultation
        {
            ConsultationReference = consultRef,
            Mode = mode,
            ConsultationType = req.ConsultationType,
            Description = req.Description,
            AppointmentDate = DateTime.UtcNow.Date.AddDays(1),
            TimeSlot = req.Time,
            ClientName = req.Name,
            ClientPhone = req.Phone,
            ClientEmail = req.Email,
            ClientCompany = req.Company,
            Status = ConsultationStatus.PaymentSubmitted,
            CreatedAt = DateTime.UtcNow
        };

        var payment = new Payment
        {
            PaymentReference = payRef,
            Amount = req.Payment.Amount,
            Method = payMethod,
            TransactionReference = req.Payment.TransactionRef,
            PaymentDate = DateTime.UtcNow,
            Status = PaymentStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };

        consultation.Payment = payment;
        _db.Consultations.Add(consultation);
        await _db.SaveChangesAsync();

        return StatusCode(201, new ConsultationResponse
        {
            Success = true,
            Message = "Consultation request submitted. Payment is pending verification.",
            Consultation = MapToDetail(consultation, payment)
        });
    }

    // GET /api/consultations — Admin only
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var query = _db.Consultations
            .Include(c => c.Payment)
            .Include(c => c.Receipt)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ConsultationStatus>(status, true, out var s))
            query = query.Where(c => c.Status == s);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    // GET /api/consultations/{ref} — Public: get consultation by reference (for client to check status)
    [HttpGet("{reference}")]
    public async Task<IActionResult> GetByReference(string reference)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Payment)
            .Include(c => c.Receipt)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConsultationReference == reference);

        if (consultation == null) return NotFound(new { message = "Consultation not found." });

        var dto = MapToDetail(consultation, consultation.Payment);

        // CRITICAL PRIVACY RULE: only reveal meeting details if payment verified AND confirmed
        if (consultation.Mode == ConsultationMode.FaceToFace
            && consultation.Status == ConsultationStatus.Confirmed
            && consultation.Payment?.Status == PaymentStatus.Verified)
        {
            dto.MeetingDetails = "Private meeting details have been sent to your registered phone and email. Do not share this information.";
        }
        else if (consultation.Mode == ConsultationMode.FaceToFace)
        {
            dto.MeetingDetails = null; // Not revealed until payment verified & confirmed
            dto.PrivacyNotice = "Private meeting details are only shared after payment verification and appointment confirmation.";
        }

        return Ok(dto);
    }

    // GET /api/consultations/{reference}/location — Private location endpoint (strict check)
    [HttpGet("{reference}/location")]
    public async Task<IActionResult> GetMeetingLocation(string reference)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConsultationReference == reference);

        if (consultation == null) return NotFound(new { message = "Consultation not found." });

        // STRICT: payment must be Verified AND consultation must be Confirmed
        if (consultation.Payment?.Status != PaymentStatus.Verified
            || consultation.Status != ConsultationStatus.Confirmed)
        {
            return Forbid(); // 403 — never expose location before conditions are met
        }

        if (consultation.Mode != ConsultationMode.FaceToFace)
            return BadRequest(new { message = "This is not a face-to-face consultation." });

        // Read private location from PaymentSettings (never hardcoded here)
        var locationSetting = await _db.PaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(ps => ps.Key == "private_meeting_address");

        if (locationSetting == null)
            return Ok(new { message = "Meeting details have been sent to your registered contact. Please check your WhatsApp and email." });

        return Ok(new
        {
            message = "Your private meeting details:",
            details = locationSetting.Value
        });
    }

    // POST /api/consultations/{id}/upload-proof — Public: upload payment proof
    [HttpPost("{id}/upload-proof")]
    public async Task<IActionResult> UploadProof(int id, IFormFile file)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null) return NotFound(new { message = "Consultation not found." });
        if (consultation.Payment == null) return BadRequest(new { message = "No payment record found." });

        // Prevent upload for terminal states
        if (consultation.Status == ConsultationStatus.Cancelled)
            return BadRequest(new { message = "Cannot upload proof for cancelled consultation." });
        
        if (consultation.Status == ConsultationStatus.Completed)
            return BadRequest(new { message = "Cannot upload proof for completed consultation." });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        // SECURITY: Validate file type (images and PDF only for proof)
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Only JPG, PNG, or PDF files are allowed as payment proof." });

        // SECURITY: Enforce file size limit (5MB max)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File size must not exceed 5MB." });

        // SECURITY: Store in non-public uploads directory (not wwwroot for direct access)
        var uploadsDir = Path.Combine(_env.ContentRootPath, "private_uploads", "proofs");
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName).ToLower();
        
        // SECURITY: Generate safe filename (no user-controlled path components)
        var safeFileName = $"proof_{consultation.Payment.PaymentReference}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsDir, safeFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        // Store relative path from ContentRoot (not publicly accessible)
        consultation.Payment.ProofFilePath = Path.Combine("private_uploads", "proofs", safeFileName);
        consultation.Payment.ProofFileName = file.FileName;
        
        // STATE TRANSITION: PendingPayment/PaymentSubmitted → PaymentSubmitted
        consultation.Payment.Status = PaymentStatus.Submitted;
        consultation.Payment.UpdatedAt = DateTime.UtcNow;
        
        consultation.Status = ConsultationStatus.PaymentSubmitted;
        consultation.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new 
        { 
            message = "Payment proof uploaded successfully. Pending admin verification.", 
            proofFile = safeFileName,
            consultationStatus = consultation.Status.ToString(),
            paymentStatus = consultation.Payment.Status.ToString()
        });
    }

    // GET /api/consultations/{id}/payment-proof — Admin only: view payment proof file
    [HttpGet("{id}/payment-proof")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ViewPaymentProof(int id)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null) return NotFound(new { message = "Consultation not found." });
        if (consultation.Payment == null) return NotFound(new { message = "No payment record." });
        if (string.IsNullOrEmpty(consultation.Payment.ProofFilePath))
            return NotFound(new { message = "No payment proof uploaded." });

        var fullPath = Path.Combine(_env.ContentRootPath, consultation.Payment.ProofFilePath);
        
        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { message = "Payment proof file not found on server." });

        var ext = Path.GetExtension(fullPath).ToLower();
        var contentType = ext switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(fileBytes, contentType, consultation.Payment.ProofFileName ?? "payment-proof" + ext);
    }

    // POST /api/consultations/{id}/verify-payment — Admin: verify or reject payment
    [HttpPost("{id}/verify-payment")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> VerifyPayment(int id, [FromBody] VerifyPaymentRequest req)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Payment)
            .Include(c => c.Receipt)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultation == null) return NotFound(new { message = "Consultation not found." });
        if (consultation.Payment == null) return BadRequest(new { message = "No payment record." });

        // Prevent state transitions from terminal states
        if (consultation.Status == ConsultationStatus.Completed)
            return BadRequest(new { message = "Cannot modify payment for completed consultation." });
        
        if (consultation.Status == ConsultationStatus.Cancelled)
            return BadRequest(new { message = "Cannot modify payment for cancelled consultation." });

        var adminId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        if (req.Action.ToLower() == "verify")
        {
            // STATE TRANSITION: PaymentSubmitted → Confirmed
            // CRITICAL BUSINESS RULE: Only transition to Confirmed if payment is Verified
            
            consultation.Payment.Status = PaymentStatus.Verified;
            consultation.Payment.VerifiedAt = DateTime.UtcNow;
            consultation.Payment.VerifiedByAdminId = adminId;
            consultation.Payment.AdminRemarks = req.Remarks;
            consultation.Payment.UpdatedAt = DateTime.UtcNow;

            consultation.Status = ConsultationStatus.Confirmed;
            consultation.ConfirmedAt = DateTime.UtcNow;
            consultation.UpdatedAt = DateTime.UtcNow;

            // Generate receipt ONLY after payment verified AND consultation confirmed
            if (consultation.Receipt == null)
            {
                var receiptNum = ReferenceGenerator.ReceiptNumber();
                while (await _db.Receipts.AnyAsync(r => r.ReceiptNumber == receiptNum))
                    receiptNum = ReferenceGenerator.ReceiptNumber();

                var receipt = new Receipt
                {
                    ReceiptNumber = receiptNum,
                    ConsultationId = consultation.Id,
                    PaymentId = consultation.Payment.Id,
                    ClientName = consultation.ClientName,
                    ClientEmail = consultation.ClientEmail,
                    ClientPhone = consultation.ClientPhone,
                    ClientCompany = consultation.ClientCompany,
                    ServiceDescription = consultation.ConsultationType,
                    Amount = consultation.Payment.Amount,
                    PaymentMethod = consultation.Payment.Method.ToString(),
                    TransactionReference = consultation.Payment.TransactionReference,
                    IssuedAt = DateTime.UtcNow,
                    ConsultationMode = consultation.Mode.ToString(),
                    AppointmentDetails = $"{consultation.AppointmentDate:dd MMM yyyy} | {consultation.TimeSlot}"
                };

                // Generate PDF
                var receiptDto = new ReceiptDto
                {
                    ReceiptNumber = receipt.ReceiptNumber,
                    ConsultationReference = consultation.ConsultationReference,
                    ClientName = receipt.ClientName,
                    ClientEmail = receipt.ClientEmail,
                    ClientPhone = receipt.ClientPhone,
                    ClientCompany = receipt.ClientCompany,
                    ServiceDescription = receipt.ServiceDescription,
                    Amount = receipt.Amount,
                    PaymentMethod = receipt.PaymentMethod,
                    TransactionReference = receipt.TransactionReference,
                    ConsultationMode = receipt.ConsultationMode ?? "",
                    AppointmentDetails = receipt.AppointmentDetails,
                    IssuedAt = receipt.IssuedAt
                };

                var pdfBytes = _receiptService.GeneratePdf(receiptDto);
                var pdfDir = Path.Combine(_env.WebRootPath, "uploads", "receipts");
                Directory.CreateDirectory(pdfDir);
                var pdfFile = $"receipt_{receiptNum}.pdf";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(pdfDir, pdfFile), pdfBytes);
                receipt.PdfFilePath = $"/uploads/receipts/{pdfFile}";

                _db.Receipts.Add(receipt);
            }

            await _db.SaveChangesAsync();
            
            return Ok(new 
            { 
                message = "Payment verified. Appointment confirmed. Receipt generated.", 
                receiptNumber = consultation.Receipt?.ReceiptNumber,
                consultationStatus = consultation.Status.ToString(),
                paymentStatus = consultation.Payment.Status.ToString()
            });
        }
        else if (req.Action.ToLower() == "reject")
        {
            // STATE TRANSITION: PaymentSubmitted → Cancelled (via Payment Rejection)
            // CRITICAL BUSINESS RULE: Payment rejection automatically cancels the appointment
            
            consultation.Payment.Status = PaymentStatus.Rejected;
            consultation.Payment.RejectedAt = DateTime.UtcNow;
            consultation.Payment.RejectedByAdminId = adminId;
            consultation.Payment.AdminRemarks = req.Remarks;
            consultation.Payment.UpdatedAt = DateTime.UtcNow;

            consultation.Status = ConsultationStatus.Cancelled;
            consultation.CancelledAt = DateTime.UtcNow;
            consultation.CancellationReason = $"Payment rejected by admin. Reason: {req.Remarks ?? "No reason provided"}";
            consultation.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            
            return Ok(new 
            { 
                message = "Payment rejected. Appointment automatically cancelled.", 
                consultationStatus = consultation.Status.ToString(),
                paymentStatus = consultation.Payment.Status.ToString(),
                cancellationReason = consultation.CancellationReason
            });
        }

        return BadRequest(new { message = "Action must be 'verify' or 'reject'." });
    }

    // GET /api/consultations/{reference}/receipt — Get receipt by consultation reference
    [HttpGet("{reference}/receipt")]
    public async Task<IActionResult> GetReceipt(string reference)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Receipt)
            .Include(c => c.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConsultationReference == reference);

        if (consultation == null) return NotFound(new { message = "Consultation not found." });

        // Receipt only available after payment is verified
        if (consultation.Payment?.Status != PaymentStatus.Verified
            || consultation.Status != ConsultationStatus.Confirmed)
        {
            return BadRequest(new { message = "Receipt is only available after payment verification and appointment confirmation." });
        }

        if (consultation.Receipt == null) return NotFound(new { message = "Receipt not yet generated." });

        var dto = new ReceiptDto
        {
            ReceiptNumber = consultation.Receipt.ReceiptNumber,
            ConsultationReference = consultation.ConsultationReference,
            ClientName = consultation.Receipt.ClientName,
            ClientEmail = consultation.Receipt.ClientEmail,
            ClientPhone = consultation.Receipt.ClientPhone,
            ClientCompany = consultation.Receipt.ClientCompany,
            ServiceDescription = consultation.Receipt.ServiceDescription,
            Amount = consultation.Receipt.Amount,
            PaymentMethod = consultation.Receipt.PaymentMethod,
            TransactionReference = consultation.Receipt.TransactionReference,
            ConsultationMode = consultation.Receipt.ConsultationMode ?? "",
            AppointmentDetails = consultation.Receipt.AppointmentDetails,
            IssuedAt = consultation.Receipt.IssuedAt
        };

        return Ok(dto);
    }

    // GET /api/consultations/{reference}/receipt/pdf — Download PDF
    [HttpGet("{reference}/receipt/pdf")]
    public async Task<IActionResult> DownloadReceiptPdf(string reference)
    {
        var consultation = await _db.Consultations
            .Include(c => c.Receipt)
            .Include(c => c.Payment)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConsultationReference == reference);

        if (consultation == null) return NotFound();

        if (consultation.Payment?.Status != PaymentStatus.Verified
            || consultation.Status != ConsultationStatus.Confirmed)
            return BadRequest(new { message = "PDF only available after verification." });

        if (consultation.Receipt == null) return NotFound(new { message = "Receipt not generated." });

        // Re-generate PDF on demand if file missing
        if (!string.IsNullOrEmpty(consultation.Receipt.PdfFilePath))
        {
            var fullPath = Path.Combine(_env.WebRootPath, consultation.Receipt.PdfFilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, "application/pdf", $"TaxPro-Receipt-{consultation.Receipt.ReceiptNumber}.pdf");
            }
        }

        // Re-generate
        var dto = new ReceiptDto
        {
            ReceiptNumber = consultation.Receipt.ReceiptNumber,
            ConsultationReference = consultation.ConsultationReference,
            ClientName = consultation.Receipt.ClientName,
            ClientEmail = consultation.Receipt.ClientEmail,
            ClientPhone = consultation.Receipt.ClientPhone,
            ClientCompany = consultation.Receipt.ClientCompany,
            ServiceDescription = consultation.Receipt.ServiceDescription,
            Amount = consultation.Receipt.Amount,
            PaymentMethod = consultation.Receipt.PaymentMethod,
            TransactionReference = consultation.Receipt.TransactionReference,
            ConsultationMode = consultation.Receipt.ConsultationMode ?? "",
            AppointmentDetails = consultation.Receipt.AppointmentDetails,
            IssuedAt = consultation.Receipt.IssuedAt
        };
        var pdfBytes = _receiptService.GeneratePdf(dto);
        return File(pdfBytes, "application/pdf", $"TaxPro-Receipt-{consultation.Receipt.ReceiptNumber}.pdf");
    }

    private static ConsultationDetail MapToDetail(Consultation c, Payment? p)
    {
        return new ConsultationDetail
        {
            Id = c.ConsultationReference,
            Mode = c.Mode.ToString().ToLower(),
            ConsultationType = c.ConsultationType,
            Description = c.Description,
            Date = c.AppointmentDate.ToString("yyyy-MM-dd"),
            Time = c.TimeSlot ?? "",
            Name = c.ClientName,
            Phone = c.ClientPhone,
            Email = c.ClientEmail,
            Company = c.ClientCompany,
            Status = c.Status.ToString(),
            Payment = p == null ? null : new PaymentDetailDto
            {
                PaymentReference = p.PaymentReference,
                Amount = p.Amount,
                Method = p.Method.ToString(),
                Status = p.Status.ToString(),
                TransactionRef = p.TransactionReference,
                PaidAt = p.PaymentDate
            },
            PrivacyNotice = c.Mode == ConsultationMode.FaceToFace
                ? "Private meeting details are shared only with confirmed clients."
                : "Encrypted video link dispatched via private channel.",
            CreatedAt = c.CreatedAt
        };
    }
}
