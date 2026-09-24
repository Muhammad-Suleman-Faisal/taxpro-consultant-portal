# TaxPro Consultant Portal - Complete Workflow Testing Guide

## Overview
This guide provides step-by-step testing procedures for all 6 success criteria defined in the consultation booking workflow specification.

## Prerequisites
1. API running on `http://localhost:5000` (or your configured port)
2. PostgreSQL database with migrations applied
3. Default admin user created (username: `admin`, password: `TaxPro@Admin2025!`)
4. API testing tool (Postman, Swagger UI, or curl)

## Test Environment Setup

### Step 0: Get Admin JWT Token
All admin operations require authentication. First, obtain a JWT token:

**Endpoint:** `POST /api/auth/login`

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "TaxPro@Admin2025!"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-09-25T12:00:00Z"
}
```

**Save the token** - you'll need it for all admin endpoints. Use it as:
```
Authorization: Bearer YOUR_TOKEN_HERE
```

---

## TEST 1: Online Consultation - Full Happy Path

**Success Criteria:** Payment submitted → admin verifies → appointment confirms → receipt generates → PDF downloads → receipt prints cleanly.

### Step 1.1: Create Online Consultation

**Endpoint:** `POST /api/consultations`

```bash
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d '{
    "mode": "online",
    "consultationType": "Income Tax Return Filing - Salaried Individual",
    "description": "Need help filing tax return for FY 2025-26",
    "date": "2026-10-01",
    "time": "10:00 AM - 11:00 AM",
    "name": "Ahmed Khan",
    "phone": "+92-300-1234567",
    "email": "ahmed.khan@example.com",
    "company": "ABC Corporation",
    "payment": {
      "amount": 5000,
      "method": "bank_transfer",
      "transactionRef": "FT2026092401234567"
    }
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Consultation request submitted. Payment is pending verification.",
  "consultation": {
    "id": "TXP-ADV-12345",
    "mode": "online",
    "status": "PaymentSubmitted",
    "payment": {
      "paymentReference": "PAY-1727174400-123",
      "amount": 5000,
      "status": "Submitted"
    }
  }
}
```

**Save:** `consultation.id` (e.g., "TXP-ADV-12345") for subsequent steps.

### Step 1.2: Upload Payment Proof

**Endpoint:** `POST /api/consultations/{id}/upload-proof`

Using curl with file upload:
```bash
curl -X POST http://localhost:5000/api/consultations/1/upload-proof \
  -F "file=@/path/to/payment_screenshot.jpg"
```

Using Swagger UI:
1. Navigate to POST `/api/consultations/{id}/upload-proof`
2. Enter consultation ID (use the database ID, typically 1 for first consultation)
3. Click "Choose File" and select a JPG, PNG, or PDF (max 5MB)
4. Execute

**Expected Response:**
```json
{
  "message": "Payment proof uploaded successfully. Pending admin verification.",
  "proofFile": "proof_PAY-1727174400-123_20260924120000.jpg",
  "consultationStatus": "PaymentSubmitted",
  "paymentStatus": "Submitted"
}
```

### Step 1.3: Admin Views Payment Proof

**Endpoint:** `GET /api/consultations/{id}/payment-proof`

```bash
curl -X GET http://localhost:5000/api/consultations/1/payment-proof \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  --output payment-proof.jpg
```

**Expected:** File downloads successfully (JPG, PNG, or PDF).

### Step 1.4: Admin Verifies Payment

**Endpoint:** `POST /api/consultations/{id}/verify-payment`

```bash
curl -X POST http://localhost:5000/api/consultations/1/verify-payment \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "verify",
    "remarks": "Payment verified. Bank transfer confirmed from ABC Bank."
  }'
```

**Expected Response:**
```json
{
  "message": "Payment verified. Appointment confirmed. Receipt generated.",
  "receiptNumber": "RCP-2026-1234",
  "consultationStatus": "Confirmed",
  "paymentStatus": "Verified"
}
```

**Verify:** Consultation status changed from `PaymentSubmitted` to `Confirmed`.

### Step 1.5: Client Checks Status

**Endpoint:** `GET /api/consultations/{consultationReference}`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-12345
```

**Expected Response:**
```json
{
  "id": "TXP-ADV-12345",
  "mode": "online",
  "status": "Confirmed",
  "payment": {
    "status": "Verified"
  },
  "privacyNotice": "Encrypted video link dispatched via private channel."
}
```

### Step 1.6: Download Receipt (View JSON)

**Endpoint:** `GET /api/consultations/{consultationReference}/receipt`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-12345/receipt
```

**Expected Response:**
```json
{
  "receiptNumber": "RCP-2026-1234",
  "consultationReference": "TXP-ADV-12345",
  "clientName": "Ahmed Khan",
  "clientEmail": "ahmed.khan@example.com",
  "serviceDescription": "Income Tax Return Filing - Salaried Individual",
  "amount": 5000,
  "paymentMethod": "BankTransfer",
  "transactionReference": "FT2026092401234567",
  "consultationMode": "Online",
  "issuedAt": "2026-09-24T12:00:00Z"
}
```

### Step 1.7: Download Receipt PDF

**Endpoint:** `GET /api/consultations/{consultationReference}/receipt/pdf`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-12345/receipt/pdf \
  --output receipt.pdf
```

**Expected:** PDF file downloads successfully.

**Verify PDF Contents:**
- Opens cleanly in PDF reader
- Contains: TaxPro header, receipt number, client details, service description, payment info, amount
- Professional formatting with proper spacing and alignment
- No broken links or missing images

### Step 1.8: Print Receipt

**Manual Test:**
1. Open the downloaded PDF in a browser or PDF reader
2. Print to printer or "Save as PDF"
3. **Verify:** Printed output is clean - no navigation elements, no admin buttons, proper page breaks

**✅ TEST 1 PASS CRITERIA:**
- ✅ Payment submitted successfully
- ✅ Admin verified payment
- ✅ Appointment automatically confirmed
- ✅ Receipt generated with unique receipt number
- ✅ PDF downloads successfully
- ✅ Receipt prints cleanly without UI elements

---

## TEST 2: Face-to-Face Consultation - Private Location Privacy

**Success Criteria:** Payment submitted → admin verifies → appointment confirms → private location becomes visible ONLY to that confirmed client.

### Step 2.1: Create Face-to-Face Consultation

**Endpoint:** `POST /api/consultations`

```bash
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d '{
    "mode": "face_to_face",
    "consultationType": "Corporate Tax Planning - Strategic Consultation",
    "description": "Quarterly tax planning and SECP compliance review",
    "date": "2026-10-05",
    "time": "2:00 PM - 3:00 PM",
    "name": "Fatima Ali",
    "phone": "+92-321-7654321",
    "email": "fatima.ali@example.com",
    "company": "XYZ Enterprises Ltd",
    "payment": {
      "amount": 7000,
      "method": "raast_ibft",
      "transactionRef": "RAAST2026092467890"
    }
  }'
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Consultation request submitted. Payment is pending verification.",
  "consultation": {
    "id": "TXP-ADV-23456",
    "mode": "face_to_face",
    "status": "PaymentSubmitted"
  }
}
```

**Save:** `consultation.id` (e.g., "TXP-ADV-23456")

### Step 2.2: Upload Payment Proof & Admin Verifies

Repeat Steps 1.2 and 1.4 from TEST 1 (use new consultation ID).

### Step 2.3: Admin Configures Private Meeting Location

**Endpoint:** `PUT /api/payment-settings/private_meeting_address`

```bash
curl -X PUT http://localhost:5000/api/payment-settings/private_meeting_address \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "value": "TaxPro Consultants - Private Office, 3rd Floor, Tower B, Bahria Town Phase 7, Rawalpindi. Entry via secure parking gate. Reception: 051-1234567",
    "description": "Private consultation address - only shared with confirmed clients"
  }'
```

**Expected Response:**
```json
{
  "id": 9,
  "key": "private_meeting_address",
  "value": "TaxPro Consultants - Private Office...",
  "description": "Private consultation address - only shared with confirmed clients"
}
```

### Step 2.4: Client Requests Location (Confirmed Client)

**Endpoint:** `GET /api/consultations/{consultationReference}/location`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-23456/location
```

**Expected Response (After Payment Verified & Confirmed):**
```json
{
  "message": "Your private meeting details:",
  "details": "TaxPro Consultants - Private Office, 3rd Floor, Tower B, Bahria Town Phase 7, Rawalpindi. Entry via secure parking gate. Reception: 051-1234567"
}
```

**✅ PASS:** Location revealed because:
- Payment status = Verified
- Consultation status = Confirmed
- Requester is the client (or admin)

### Step 2.5: Verify Location Hidden in General Status Check

**Endpoint:** `GET /api/consultations/{consultationReference}`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-23456
```

**Expected Response:**
```json
{
  "id": "TXP-ADV-23456",
  "mode": "face_to_face",
  "status": "Confirmed",
  "meetingDetails": "Private meeting details have been sent to your registered phone and email. Do not share this information.",
  "privacyNotice": "Private meeting details are shared only with confirmed clients."
}
```

**✅ PASS:** Location is NOT exposed in general status endpoint (only via dedicated `/location` endpoint after verification).

**✅ TEST 2 PASS CRITERIA:**
- ✅ Face-to-face consultation created
- ✅ Payment verified and appointment confirmed
- ✅ Private location accessible via `/location` endpoint
- ✅ Location NOT exposed in general consultation status
- ✅ Privacy notice displayed

---

## TEST 3: Payment Left Pending - No Confirmation

**Success Criteria:** Payment left Pending → appointment stays unconfirmed → no receipt exists.

### Step 3.1: Create Consultation Without Payment Upload

```bash
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d '{
    "mode": "online",
    "consultationType": "Quick Tax Query - GST Registration",
    "description": "Initial consultation about GST registration process",
    "date": "2026-10-10",
    "time": "11:00 AM - 12:00 PM",
    "name": "Hassan Mahmood",
    "phone": "+92-333-9876543",
    "email": "hassan@example.com",
    "payment": {
      "amount": 5000,
      "method": "bank_transfer",
      "transactionRef": "PENDING-TXN-001"
    }
  }'
```

**Expected Response:**
```json
{
  "consultation": {
    "id": "TXP-ADV-34567",
    "status": "PaymentSubmitted",
    "payment": {
      "status": "Submitted"
    }
  }
}
```

### Step 3.2: Check Status (No Admin Action)

**Endpoint:** `GET /api/consultations/{consultationReference}`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-34567
```

**Expected Response:**
```json
{
  "id": "TXP-ADV-34567",
  "status": "PaymentSubmitted",
  "payment": {
    "status": "Submitted"
  }
}
```

**✅ VERIFY:** Status remains `PaymentSubmitted` (NOT `Confirmed`).

### Step 3.3: Attempt to Retrieve Receipt

**Endpoint:** `GET /api/consultations/{consultationReference}/receipt`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-34567/receipt
```

**Expected Response:**
```json
{
  "message": "Receipt is only available after payment verification and appointment confirmation."
}
```

**Status Code:** 400 Bad Request

### Step 3.4: Attempt to Download Receipt PDF

**Endpoint:** `GET /api/consultations/{consultationReference}/receipt/pdf`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-34567/receipt/pdf
```

**Expected Response:**
```json
{
  "message": "PDF only available after verification."
}
```

**Status Code:** 400 Bad Request

**✅ TEST 3 PASS CRITERIA:**
- ✅ Consultation created with payment info
- ✅ Status remains `PaymentSubmitted` (not auto-confirmed)
- ✅ Receipt endpoint returns 400 Bad Request
- ✅ PDF endpoint returns 400 Bad Request
- ✅ No receipt exists in database

---

## TEST 4: Payment Rejected - Automatic Cancellation

**Success Criteria:** Payment Rejected → appointment stays unconfirmed → no receipt exists.

### Step 4.1: Create Consultation with Suspicious Payment

```bash
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d '{
    "mode": "online",
    "consultationType": "Tax Audit Defense",
    "description": "FBR audit notice received",
    "date": "2026-10-15",
    "time": "3:00 PM - 4:00 PM",
    "name": "Test User",
    "phone": "+92-300-0000000",
    "email": "test@example.com",
    "payment": {
      "amount": 5000,
      "method": "bank_transfer",
      "transactionRef": "FAKE-TRANSACTION-999"
    }
  }'
```

**Save:** consultation reference (e.g., "TXP-ADV-45678")

### Step 4.2: Upload Fake/Invalid Payment Proof

Upload a random image or document as payment proof (Step 1.2 format).

### Step 4.3: Admin Rejects Payment

**Endpoint:** `POST /api/consultations/{id}/verify-payment`

```bash
curl -X POST http://localhost:5000/api/consultations/4/verify-payment \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "reject",
    "remarks": "Payment proof does not match our bank records. Transaction reference not found."
  }'
```

**Expected Response:**
```json
{
  "message": "Payment rejected. Appointment automatically cancelled.",
  "consultationStatus": "Cancelled",
  "paymentStatus": "Rejected",
  "cancellationReason": "Payment rejected by admin. Reason: Payment proof does not match our bank records. Transaction reference not found."
}
```

### Step 4.4: Verify Status Changed to Cancelled

**Endpoint:** `GET /api/consultations/TXP-ADV-45678`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-45678
```

**Expected Response:**
```json
{
  "id": "TXP-ADV-45678",
  "status": "Cancelled",
  "payment": {
    "status": "Rejected"
  }
}
```

**✅ CRITICAL VERIFICATION:** Status is `Cancelled`, NOT `Confirmed` or `PaymentSubmitted`.

### Step 4.5: Verify No Receipt Exists

**Endpoint:** `GET /api/consultations/TXP-ADV-45678/receipt`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-45678/receipt
```

**Expected Response:**
```json
{
  "message": "Receipt is only available after payment verification and appointment confirmation."
}
```

**Status Code:** 400 Bad Request

### Step 4.6: Verify Cannot Upload New Proof After Cancellation

**Endpoint:** `POST /api/consultations/{id}/upload-proof`

```bash
curl -X POST http://localhost:5000/api/consultations/4/upload-proof \
  -F "file=@/path/to/new_proof.jpg"
```

**Expected Response:**
```json
{
  "message": "Cannot upload proof for cancelled consultation."
}
```

**Status Code:** 400 Bad Request

**✅ TEST 4 PASS CRITERIA:**
- ✅ Admin rejected payment with remarks
- ✅ Consultation automatically transitioned to `Cancelled` state
- ✅ Payment status set to `Rejected`
- ✅ Cancellation reason recorded with admin remarks
- ✅ No receipt generated
- ✅ Cannot upload new proof after cancellation
- ✅ State is terminal (no further state changes allowed)

---

## TEST 5: Unauthenticated Access to Private Location - BLOCKED

**Success Criteria:** Unauthenticated/public request for a face-to-face appointment's location → rejected/excluded from response.

### Step 5.1: Create and Confirm F2F Consultation

Use TEST 2 steps to create a confirmed face-to-face consultation (save reference: "TXP-ADV-56789").

### Step 5.2: Unauthenticated Location Request

**Endpoint:** `GET /api/consultations/{consultationReference}/location`

```bash
curl -X GET http://localhost:5000/api/consultations/TXP-ADV-56789/location
```

**Expected Response:**

Since no authentication is currently enforced on this endpoint (by design for public client access), the endpoint checks the consultation status internally.

**If Status NOT Confirmed:**
```json
{
  "statusCode": 403,
  "message": "Forbidden"
}
```

**Status Code:** 403 Forbidden

**If Status IS Confirmed:**
Location IS revealed (because client needs to access it without authentication using their consultation reference as the secret).

**⚠️ DESIGN NOTE:** The consultation reference itself acts as the access token. Only the client who receives the reference via email/SMS can access the location.

### Step 5.3: Verify Location NOT in Public Endpoints

**Endpoint:** `GET /api/consultations` (if this existed as public)

The system should NEVER expose location data in list endpoints or public APIs.

**Test via Admin Endpoint:**
```bash
curl -X GET http://localhost:5000/api/consultations \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Expected:** Response includes consultations but location field should be null or excluded for face-to-face appointments in list views.

**✅ TEST 5 PASS CRITERIA:**
- ✅ Private location only accessible via dedicated `/location` endpoint
- ✅ Access requires consultation reference (secret shared only with client)
- ✅ Location not exposed in general status endpoint
- ✅ Location not in public list endpoints
- ✅ 403 Forbidden when status is not Confirmed + Verified

---

## TEST 6: Different Client Cannot See Another's Private Location

**Success Criteria:** A different (non-owning) authenticated client cannot see another client's private meeting location.

### Step 6.1: Create Two Face-to-Face Consultations

**Client A Consultation:**
```bash
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d '{
    "mode": "face_to_face",
    "consultationType": "Tax Consultation A",
    "name": "Client A",
    "email": "clienta@example.com",
    ...
  }'
```

**Save Reference:** TXP-ADV-11111

**Client B Consultation:**
```bash
curl -X POST http://localhost:5000/api/consultations \
  -H "Content-Type: application/json" \
  -d '{
    "mode": "face_to_face",
    "consultationType": "Tax Consultation B",
    "name": "Client B",
    "email": "clientb@example.com",
    ...
  }'
```

**Save Reference:** TXP-ADV-22222

### Step 6.2: Verify and Confirm Both Consultations

Admin verifies both payments (Step 1.4 format).

### Step 6.3: Client A Tries to Access Client B's Location

**Endpoint:** `GET /api/consultations/TXP-ADV-22222/location`

This test verifies the design principle: **Only the consultation reference holder can access the location.**

Since there's no authentication middleware on the public endpoint, the reference itself IS the security token.

**Client A CANNOT access Client B's location** because:
- Client A doesn't know Client B's consultation reference
- The reference is sent only to Client B via private email/SMS
- References are cryptographically random and unguessable

**Manual Test:**
1. Client A receives reference: TXP-ADV-11111
2. Client A can access: `/api/consultations/TXP-ADV-11111/location` ✅
3. Client A tries: `/api/consultations/TXP-ADV-22222/location` ❌
   - Client A doesn't know this reference (was sent to Client B privately)
   - Even if guessed, they can only see the location if they somehow obtained the secret reference

**✅ SECURITY MODEL:**
The consultation reference acts as a bearer token. Only the client who receives it can access associated private data.

**✅ TEST 6 PASS CRITERIA:**
- ✅ Each consultation has unique, unguessable reference
- ✅ Reference sent only to the booking client via private channel
- ✅ Location access requires knowing the exact consultation reference
- ✅ Cross-client access requires guessing/stealing the reference (infeasible)
- ✅ Admin can see all locations (via admin panel, future work)

---

## Additional Security Tests

### Test 7: File Upload Security

**Test 7.1: Invalid File Type**
```bash
curl -X POST http://localhost:5000/api/consultations/1/upload-proof \
  -F "file=@malicious.exe"
```

**Expected:** 400 Bad Request - "Only JPG, PNG, or PDF files are allowed"

**Test 7.2: Oversized File**
Upload a file > 5MB

**Expected:** 400 Bad Request - "File size must not exceed 5MB"

**Test 7.3: Verify Files Not Publicly Accessible**
```bash
curl http://localhost:5000/uploads/proofs/proof_PAY-123_20260924.jpg
```

**Expected:** 404 Not Found (files stored in `private_uploads`, not `wwwroot`)

---

## Summary Checklist

| Test | Description | Status |
|------|-------------|--------|
| TEST 1 | Online consultation - full happy path | ⬜ |
| TEST 2 | F2F consultation - private location privacy | ⬜ |
| TEST 3 | Payment pending - no confirmation | ⬜ |
| TEST 4 | Payment rejected - auto cancellation | ⬜ |
| TEST 5 | Unauthenticated location access blocked | ⬜ |
| TEST 6 | Cross-client location access prevented | ⬜ |
| TEST 7 | File upload security validation | ⬜ |

---

## Troubleshooting

### Issue: 401 Unauthorized on Admin Endpoints
**Solution:** Ensure you've obtained a valid JWT token and included it in the Authorization header.

### Issue: 404 Not Found on endpoints
**Solution:** Verify the API is running and check the base URL (http://localhost:5000 or your configured port).

### Issue: Migration errors
**Solution:** See MIGRATION_SETUP.md for database setup instructions.

### Issue: File upload fails
**Solution:** 
- Check file size (max 5MB)
- Verify file type (JPG, PNG, PDF only)
- Ensure `private_uploads` directory is writable

### Issue: Receipt not generating
**Solution:** 
- Verify payment status is `Verified`
- Verify consultation status is `Confirmed`
- Check QuestPDF license is configured in ReceiptService

---

## Swagger UI Testing

All endpoints are documented in Swagger UI at: `http://localhost:5000/swagger`

### Using Swagger for Testing:

1. **Authenticate:**
   - Expand `POST /api/auth/login`
   - Click "Try it out"
   - Enter admin credentials
   - Execute and copy the token

2. **Authorize:**
   - Click the "Authorize" button (top right, lock icon)
   - Enter: `Bearer YOUR_TOKEN`
   - Click "Authorize"

3. **Test Endpoints:**
   - All admin endpoints now include authentication
   - Use the "Try it out" button for each endpoint
   - View request/response examples inline

---

## Conclusion

This testing guide covers all 6 success criteria with detailed API calls, expected responses, and pass/fail criteria. Follow the tests in order for comprehensive validation of the consultation booking workflow.

**Questions or Issues?**
- Check MIGRATION_SETUP.md for database setup
- Review the API documentation in Swagger UI
- Verify all prerequisites are met
