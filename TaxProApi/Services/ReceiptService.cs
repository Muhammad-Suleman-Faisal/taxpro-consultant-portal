using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaxProApi.DTOs;

namespace TaxProApi.Services;

public interface IReceiptService
{
    byte[] GeneratePdf(ReceiptDto receipt);
}

public class ReceiptService : IReceiptService
{
    public byte[] GeneratePdf(ReceiptDto receipt)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("TaxPro Consultants")
                                .FontSize(22).Bold().FontColor("#0037b0");
                            c.Item().Text("Premier Tax & Corporate Advisory")
                                .FontSize(10).FontColor("#747686");
                            c.Item().Text("Pakistan | FBR & SRB Authorized Advisors")
                                .FontSize(10).FontColor("#747686");
                        });

                        row.ConstantItem(150).Column(c =>
                        {
                            c.Item().AlignRight().Text("OFFICIAL RECEIPT")
                                .FontSize(16).Bold().FontColor("#0037b0");
                            c.Item().AlignRight().Text($"#{receipt.ReceiptNumber}")
                                .FontSize(12).Bold();
                            c.Item().AlignRight().Text($"Date: {receipt.IssuedAt:dd MMM yyyy}")
                                .FontSize(10).FontColor("#747686");
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#0037b0");

                    // Consultation Info
                    col.Item().PaddingVertical(8).Column(c =>
                    {
                        c.Item().Text("CONSULTATION DETAILS").Bold().FontSize(12).FontColor("#0037b0");
                        c.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(160);
                                cols.RelativeColumn();
                            });
                            AddTableRow(table, "Consultation Ref:", receipt.ConsultationReference);
                            AddTableRow(table, "Service:", receipt.ServiceDescription);
                            AddTableRow(table, "Mode:", receipt.ConsultationMode == "FaceToFace" ? "Private Face-to-Face" : "Online Consultation");
                            AddTableRow(table, "Schedule:", receipt.AppointmentDetails ?? "As Confirmed");
                        });
                    });

                    col.Item().PaddingVertical(4).LineHorizontal(1).LineColor("#dae2fd");

                    // Client Info
                    col.Item().PaddingVertical(8).Column(c =>
                    {
                        c.Item().Text("CLIENT INFORMATION").Bold().FontSize(12).FontColor("#0037b0");
                        c.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(160);
                                cols.RelativeColumn();
                            });
                            AddTableRow(table, "Client Name:", receipt.ClientName);
                            if (!string.IsNullOrEmpty(receipt.ClientCompany))
                                AddTableRow(table, "Company:", receipt.ClientCompany);
                            if (!string.IsNullOrEmpty(receipt.ClientPhone))
                                AddTableRow(table, "Phone:", receipt.ClientPhone);
                            if (!string.IsNullOrEmpty(receipt.ClientEmail))
                                AddTableRow(table, "Email:", receipt.ClientEmail);
                        });
                    });

                    col.Item().PaddingVertical(4).LineHorizontal(1).LineColor("#dae2fd");

                    // Payment Info
                    col.Item().PaddingVertical(8).Column(c =>
                    {
                        c.Item().Text("PAYMENT INFORMATION").Bold().FontSize(12).FontColor("#0037b0");
                        c.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(160);
                                cols.RelativeColumn();
                            });
                            AddTableRow(table, "Payment Method:", receipt.PaymentMethod ?? "Bank Transfer");
                            if (!string.IsNullOrEmpty(receipt.TransactionReference))
                                AddTableRow(table, "Transaction Ref:", receipt.TransactionReference);
                            AddTableRow(table, "Amount Paid:", $"PKR {receipt.Amount:N0}");
                            AddTableRow(table, "Status:", "VERIFIED & CONFIRMED");
                        });
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(2).LineColor("#0037b0");

                    // Amount box
                    col.Item().PaddingVertical(10).Background("#f0f4ff").Padding(12).Row(row =>
                    {
                        row.RelativeItem().Text("Total Amount Paid:").FontSize(14).Bold();
                        row.ConstantItem(200).AlignRight().Text($"PKR {receipt.Amount:N0}").FontSize(16).Bold().FontColor("#0037b0");
                    });

                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#dae2fd");

                    // Footer
                    col.Item().PaddingTop(20).Column(c =>
                    {
                        c.Item().Text("This is a system-generated receipt. No signature required.")
                            .FontSize(9).FontColor("#747686").Italic();
                        c.Item().PaddingTop(4).Text("TaxPro Consultants — FBR NTN: 8940211-7 | SRB Active | SECP Registered")
                            .FontSize(9).FontColor("#747686");
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingVertical(3).Text(label).Bold().FontSize(10).FontColor("#434655");
        table.Cell().PaddingVertical(3).Text(value).FontSize(10);
    }
}
