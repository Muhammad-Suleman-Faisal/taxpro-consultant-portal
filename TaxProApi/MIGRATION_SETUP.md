# Database Migration Setup Guide

## Overview
This document explains how to set up and run database migrations for the TaxPro Consultant Portal API.

## Prerequisites
1. .NET 10.0 SDK installed
2. PostgreSQL server running (localhost:5432 by default)
3. Database credentials configured in `appsettings.json`

## Installing EF Core Tools

### Global Installation (Recommended)
```powershell
dotnet tool install --global dotnet-ef
```

### Verify Installation
```powershell
dotnet ef --version
```

## Running Migrations

### Option 1: Using EF Core Tools (Recommended)
```powershell
# Navigate to the TaxProApi directory
cd "c:\Users\Suleman\Desktop\TaxPro Consultant Portal\taxpro-consultant-portal\TaxProApi"

# Create a new migration (if needed)
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update
```

### Option 2: Automatic on Application Start
The application is configured to automatically run migrations on startup (see `Program.cs`):
```csharp
db.Database.Migrate();
```

Simply start the application and migrations will be applied automatically.

## Manual Migration Files Created

The following migration has been manually created for the workflow state tracking:
- `Migrations/20260924000001_AddWorkflowStateTracking.cs`

This migration adds:
- `Payments.RejectedAt` (DateTime?)
- `Payments.RejectedByAdminId` (int?)
- `Consultations.CancellationReason` (string, max 500 chars)
- `Consultations.CancelledAt` (DateTime?)

## Verifying Database Schema

After running migrations, verify the following tables exist:
- AdminUsers
- Clients
- Services
- Appointments
- Consultations
- Payments
- Receipts
- Inquiries
- FAQs
- BlogPosts
- TaxRules
- PaymentSettings

## Troubleshooting

### Issue: "dotnet ef" command not found
**Solution**: Install dotnet-ef tools globally:
```powershell
dotnet tool install --global dotnet-ef
```

### Issue: Connection string error
**Solution**: Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taxproportal;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Issue: Migration already applied
**Solution**: Check current migration status:
```powershell
dotnet ef migrations list
```

Remove a migration (if not yet applied to database):
```powershell
dotnet ef migrations remove
```

### Issue: Database doesn't exist
**Solution**: Create the database manually in PostgreSQL:
```sql
CREATE DATABASE taxproportal;
```

Or let EF Core create it automatically:
```powershell
dotnet ef database update
```

## Seeded Data

The migration will automatically seed the following:
- Default admin user (username: admin, password: TaxPro@Admin2025!)
- Payment settings (bank details, consultation fees)
- Services (8 tax and corporate services)
- FAQs (4 frequently asked questions)

## State Machine Implementation

The migration supports the following state machines:

### Payment States
- `Pending` → Initial state
- `Submitted` → Client uploaded payment proof
- `Verified` → Admin verified payment (triggers appointment confirmation)
- `Rejected` → Admin rejected payment (triggers automatic appointment cancellation)

### Consultation States
- `PendingPayment` → Awaiting payment submission
- `PaymentSubmitted` → Payment proof submitted, awaiting verification
- `Confirmed` → Payment verified, appointment confirmed
- `Cancelled` → Cancelled (by user, admin, or automatic via payment rejection)
- `Completed` → Consultation has occurred

## Important Notes

1. **Privacy**: Payment proof files are stored in `private_uploads/proofs/` directory, NOT in wwwroot (not publicly accessible)
2. **Security**: Only admins can view payment proofs via authenticated endpoint
3. **Business Rule**: Payment rejection automatically cancels the consultation
4. **Receipt Generation**: Receipts are ONLY generated after payment verification and appointment confirmation
