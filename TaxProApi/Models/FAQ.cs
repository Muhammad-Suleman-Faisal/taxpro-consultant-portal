using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public class FAQ
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    public string Question { get; set; } = string.Empty;

    [Required, MaxLength(3000)]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
