# TaxPro Consultant Portal - API Endpoints Reference

## Authentication

### Admin Login
- **POST** `/api/auth/login`
- **Auth:** Public
- **Body:** `{ "username": "admin", "password": "TaxPro@Admin2025!" }`
- **Response:** JWT token

---

## Consultations

### Create Consultation (Client)
- **POST** `/api/consultations`
- **Auth:** Public
- **Body:** Consultation request with payment info
- **Returns:** Consultation reference

### Get All Consultations (Admin)
- **GET** `/api/consultations?page=1&pageSize=20&status=PaymentSubmitted`
- **Auth:** Admin (JWT)
- **Returns:** Paginated list of consultations

### Get Consultation by Reference (Client)
- **GET** `/api/consultations/{consultationReference}`
- **Auth:** Public (reference acts as access token)
- **Returns:** Consultation details (location hidden until confirmed)

### Get Private Meeting Location (Client)
- **GET** `/api/consultations/{consultationReference}/location`
- **Auth:** Public (requires consultation reference)
- **Business Rule:** Only returns location if Payment=Verified AND Status=Confirmed
- **Returns:** Private meeting address

---

## Payment Management

### Upload Payment Proof (Client)
- **POST** `/api/consultations/{id}/upload-proof`
- **Auth:** Public
- **Content-Type:** multipart/form-data
- **File:** JPG, PNG, or PDF (max 5MB)
- **Returns:** Upload confirmation

### View Payment Proof (Admin)
- **GET** `/api/consultations/{id}/payment-proof`
- **Auth:** Admin (JWT)
- **Returns:** Payment proof file (image or PDF)

### Verify or Reject Payment (Admin)
- **POST** `/api/consultations/{id}/verify-payment`
- **Auth:** Admin (JWT)
- **Body:** `{ "action": "verify|reject", "remarks": "..." }`
- **State Transitions:**
  - **verify:** Payment→Verified, Consultation→Confirmed, generates Receipt
  - **reject:** Payment→Rejected, Consultation→Cancelled (automatic)

---

## Receipts

### Get Receipt Details (Client)
- **GET** `/api/consultations/{consultationReference}/receipt`
- **Auth:** Public (requires consultation reference)
- **Business Rule:** Only available if Payment=Verified AND Status=Confirmed
- **Returns:** Receipt JSON

### Download Receipt PDF (Client)
- **GET** `/api/consultations/{consultationReference}/receipt/pdf`
- **Auth:** Public (requires consultation reference)
- **Business Rule:** Only available if Payment=Verified AND Status=Confirmed
- **Returns:** PDF file for download/print

---

## Payment Settings

### Get Public Bank Details
- **GET** `/api/payment-settings/bank-details`
- **Auth:** Public
- **Returns:** Safe payment settings (bank details, consultation fees)

### Get All Settings (Admin)
- **GET** `/api/payment-settings`
- **Auth:** Admin (JWT)
- **Returns:** All payment settings including private ones

### Update Setting (Admin)
- **PUT** `/api/payment-settings/{key}`
- **Auth:** Admin (JWT)
- **Body:** `{ "value": "...", "description": "..." }`
- **Returns:** Updated setting

### Bulk Update Settings (Admin)
- **POST** `/api/payment-settings/bulk`
- **Auth:** Admin (JWT)
- **Body:** `{ "key1": "value1", "key2": "value2" }`
- **Returns:** Success confirmation

---

## State Machine Overview

### Payment States
```
Pending → Submitted → Verified (triggers Consultation.Confirmed)
                   ↘ Rejected (triggers Consultation.Cancelled)
```

### Consultation States
```
PendingPayment → PaymentSubmitted → Confirmed → Completed
                                  ↘ Cancelled (terminal)
```

### Critical Business Rules
1. **Confirmation Blocker:** Consultation can ONLY be Confirmed if Payment is Verified
2. **Automatic Cancellation:** Payment Rejection automatically cancels Consultation
3. **Receipt Generation:** Receipt ONLY generated when Payment=Verified AND Consultation=Confirmed
4. **Location Privacy:** Private F2F location ONLY visible when Payment=Verified AND Consultation=Confirmed

---

## Response Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET/PUT operation |
| 201 | Created | Successful POST (resource created) |
| 400 | Bad Request | Invalid input, business rule violation |
| 401 | Unauthorized | Missing or invalid JWT token |
| 403 | Forbidden | Valid token but insufficient permissions |
| 404 | Not Found | Resource not found |
| 500 | Server Error | Unexpected server error |

---

## Security Headers

### Admin Endpoints
All admin endpoints require:
```
Authorization: Bearer <JWT_TOKEN>
```

### Public Endpoints
No authentication required, but:
- Consultation reference acts as access token
- Private data (location, receipt) only accessible with correct reference
- References are cryptographically random and unguessable

---

## File Storage Security

### Payment Proofs
- **Storage:** `private_uploads/proofs/` (NOT in wwwroot)
- **Access:** Admin-only via `/api/consultations/{id}/payment-proof`
- **Naming:** `proof_{paymentReference}_{timestamp}.{ext}`

### Receipt PDFs
- **Storage:** `wwwroot/uploads/receipts/`
- **Access:** Public via consultation reference (receipts are meant to be shareable)
- **Naming:** `receipt_{receiptNumber}.pdf`

---

## Example Workflow

```
1. Client submits consultation → receives TXP-ADV-12345
2. Client uploads payment proof
3. Admin views proof via admin panel
4. Admin verifies payment
   → Payment.Status = Verified
   → Consultation.Status = Confirmed
   → Receipt generated automatically
5. Client checks status → sees "Confirmed"
6. Client downloads receipt PDF
7. For F2F: Client accesses private location via /location endpoint
```

---

## Testing Quick Commands

```bash
# Get admin token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"TaxPro@Admin2025!"}'

# Create consultation
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d @consultation.json

# Verify payment (admin)
curl -X POST http://localhost:5000/api/consultations/1/verify-payment \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"action":"verify","remarks":"Confirmed"}'

# Download receipt
curl http://localhost:5000/api/consultations/TXP-ADV-12345/receipt/pdf \
  -o receipt.pdf
```

---

## Environment Configuration

### Required Settings (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taxproportal;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "YOUR_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "TaxProApi",
    "Audience": "TaxProAdmin",
    "ExpiryHours": "12"
  }
}
```

### Seeded Payment Settings
- `bank_name`: Meezan Bank Ltd
- `account_title`: TaxPro Consultants
- `account_number`: 01230123456789
- `iban`: PK36MEZN0001230123456789
- `consultation_fee_online`: 5000 PKR
- `consultation_fee_f2f`: 7000 PKR

---

## Swagger UI

**URL:** `http://localhost:5000/swagger`

- Interactive API documentation
- Test endpoints directly in browser
- View request/response schemas
- Authenticate with JWT token via "Authorize" button
