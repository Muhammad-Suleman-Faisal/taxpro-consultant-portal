using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public enum TaxEntityType
{
    Salaried,
    Business,
    Corporate,
    AOP
}

public class TaxRule
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string TaxYear { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Statute { get; set; } = string.Empty;

    public TaxEntityType EntityType { get; set; }

    public decimal MinIncome { get; set; }
    public decimal MaxIncome { get; set; }  // 0 = unlimited
    public decimal Rate { get; set; }       // as decimal e.g. 0.05 = 5%
    public decimal BaseAmount { get; set; } // fixed base tax amount

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
