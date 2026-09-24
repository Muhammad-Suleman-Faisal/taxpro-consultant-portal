using Microsoft.EntityFrameworkCore;
using TaxProApi.Models;

namespace TaxProApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<FAQ> FAQs => Set<FAQ>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<PaymentSetting> PaymentSettings => Set<PaymentSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AdminUser
        modelBuilder.Entity<AdminUser>()
            .HasIndex(a => a.Username).IsUnique();

        // Payment ↔ Consultation (1-to-1)
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Consultation)
            .WithOne(c => c.Payment)
            .HasForeignKey<Payment>(p => p.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Receipt ↔ Consultation (1-to-1)
        modelBuilder.Entity<Receipt>()
            .HasOne(r => r.Consultation)
            .WithOne(c => c.Receipt)
            .HasForeignKey<Receipt>(r => r.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Receipt ↔ Payment (1-to-1)
        modelBuilder.Entity<Receipt>()
            .HasOne(r => r.Payment)
            .WithOne(p => p.Receipt)
            .HasForeignKey<Receipt>(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Consultation → Client (optional FK)
        modelBuilder.Entity<Consultation>()
            .HasOne(c => c.Client)
            .WithMany(cl => cl.Consultations)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        // Appointment → Client (optional FK)
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Client)
            .WithMany(cl => cl.Appointments)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        // Inquiry → Client (optional FK)
        modelBuilder.Entity<Inquiry>()
            .HasOne(i => i.Client)
            .WithMany(cl => cl.Inquiries)
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique indexes
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.BookingReference).IsUnique();
        modelBuilder.Entity<Consultation>()
            .HasIndex(c => c.ConsultationReference).IsUnique();
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.PaymentReference).IsUnique();
        modelBuilder.Entity<Receipt>()
            .HasIndex(r => r.ReceiptNumber).IsUnique();
        modelBuilder.Entity<Inquiry>()
            .HasIndex(i => i.InquiryReference).IsUnique();
        modelBuilder.Entity<PaymentSetting>()
            .HasIndex(ps => ps.Key).IsUnique();

        // Decimal precision
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<Receipt>()
            .Property(r => r.Amount).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<Service>()
            .Property(s => s.Price).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<TaxRule>()
            .Property(t => t.MinIncome).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<TaxRule>()
            .Property(t => t.MaxIncome).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<TaxRule>()
            .Property(t => t.Rate).HasColumnType("numeric(8,6)");
        modelBuilder.Entity<TaxRule>()
            .Property(t => t.BaseAmount).HasColumnType("numeric(18,2)");

        // Seed default payment settings
        modelBuilder.Entity<PaymentSetting>().HasData(
            new PaymentSetting { Id = 1, Key = "bank_name", Value = "Meezan Bank Ltd", Description = "Bank name for payments" },
            new PaymentSetting { Id = 2, Key = "account_title", Value = "TaxPro Consultants", Description = "Account holder name" },
            new PaymentSetting { Id = 3, Key = "account_number", Value = "01230123456789", Description = "Bank account number" },
            new PaymentSetting { Id = 4, Key = "iban", Value = "PK36MEZN0001230123456789", Description = "IBAN for transfers" },
            new PaymentSetting { Id = 5, Key = "branch_code", Value = "0123", Description = "Bank branch code" },
            new PaymentSetting { Id = 6, Key = "raast_id", Value = "03001234567", Description = "Raast ID / Mobile number" },
            new PaymentSetting { Id = 7, Key = "consultation_fee_online", Value = "5000", Description = "Online consultation fee (PKR)" },
            new PaymentSetting { Id = 8, Key = "consultation_fee_f2f", Value = "7000", Description = "Face-to-face consultation fee (PKR)" }
        );

        // Seed default services
        modelBuilder.Entity<Service>().HasData(
            new Service { Id = 1, Name = "Income Tax Advisory & Filing", Icon = "account_balance", Description = "Individual & corporate return filing", IsActive = true, SortOrder = 1 },
            new Service { Id = 2, Name = "Sales Tax & GST", Icon = "receipt", Description = "FBR Federal Sales Tax & provincial GST registrations", IsActive = true, SortOrder = 2 },
            new Service { Id = 3, Name = "Withholding Tax (WHT)", Icon = "price_change", Description = "Continuous WHT monitoring and e-filing", IsActive = true, SortOrder = 3 },
            new Service { Id = 4, Name = "FBR Services & Notice Defense", Icon = "verified", Description = "NTN registration, Iris portal compliance", IsActive = true, SortOrder = 4 },
            new Service { Id = 5, Name = "SRB & Provincial Services", Icon = "location_city", Description = "Sindh Revenue Board and PRA compliance", IsActive = true, SortOrder = 5 },
            new Service { Id = 6, Name = "Corporate Tax & SECP", Icon = "corporate_fare", Description = "SECP company incorporation and filings", IsActive = true, SortOrder = 6 },
            new Service { Id = 7, Name = "Tax Assessment & Appeals", Icon = "gavel", Description = "Audit defense and appeal representation", IsActive = true, SortOrder = 7 },
            new Service { Id = 8, Name = "Strategic Business Restructuring", Icon = "trending_up", Description = "Fiscal architecture and tax optimization", IsActive = true, SortOrder = 8 }
        );

        // Seed FAQs
        modelBuilder.Entity<FAQ>().HasData(
            new FAQ { Id = 1, Question = "What is the advance consultation fee?", Answer = "Online consultations are PKR 5,000 and private face-to-face consultations are PKR 7,000. Payment must be made in advance via bank transfer or Raast.", Category = "Payments", SortOrder = 1 },
            new FAQ { Id = 2, Question = "How do I verify my payment?", Answer = "After bank transfer, provide the transaction reference number. Our team verifies it within 2-4 hours during business hours.", Category = "Payments", SortOrder = 2 },
            new FAQ { Id = 3, Question = "Is the meeting location publicly available?", Answer = "No. For confidentiality and security, private meeting location details are only shared with confirmed clients after payment verification.", Category = "Privacy", SortOrder = 3 },
            new FAQ { Id = 4, Question = "How long does a consultation session last?", Answer = "Standard consultations are 1 hour. Complex matters may require additional sessions at the same rate.", Category = "General", SortOrder = 4 }
        );
    }
}
