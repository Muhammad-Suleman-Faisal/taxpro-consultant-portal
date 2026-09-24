namespace TaxProApi.Services;

public static class ReferenceGenerator
{
    private static readonly Random _rng = new();

    public static string BookingRef()
        => $"TXP-{_rng.Next(10000, 99999)}";

    public static string ConsultationRef()
        => $"TXP-ADV-{_rng.Next(10000, 99999)}";

    public static string PaymentRef()
        => $"PAY-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{_rng.Next(100, 999)}";

    public static string InquiryRef()
        => $"INQ-{_rng.Next(10000, 99999)}";

    public static string ReceiptNumber()
    {
        var year = DateTime.UtcNow.Year;
        var seq = _rng.Next(1000, 9999);
        return $"RCP-{year}-{seq}";
    }
}
