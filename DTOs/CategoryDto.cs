using System.ComponentModel.DataAnnotations;

namespace ShashiControllerAPI.DTOs;
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [MinLength(2, ErrorMessage = "Category name must be at least 2 characters.")]
    [MaxLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
    public required string CategoryName { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;
}


public class GetCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Expense" or "Income"
    public bool IsPersonal { get; set; }  // true if personal, false if system
}