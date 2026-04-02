using System.ComponentModel.DataAnnotations;

namespace ShashiControllerAPI.DTOs;
public class CreateIncomeDto
{
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Source is required.")]
    [MinLength(2)]
    [MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Amount { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    [Required]
    public DateOnly Date { get; set; }
}