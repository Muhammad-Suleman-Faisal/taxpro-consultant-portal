using System.ComponentModel.DataAnnotations;

namespace TaxProApi.Models;

public class BlogPost
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(50)]
    public string? ReadTime { get; set; }

    [MaxLength(100)]
    public string? Slug { get; set; }

    [MaxLength(5000)]
    public string? Content { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
