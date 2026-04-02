// namespace ShashiControllerAPI.DTOs;

// public class CreateCategoryDto
// {
//     public required string CategoryName { get; set; }
// }

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