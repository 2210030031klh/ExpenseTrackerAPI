namespace ShashiControllerAPI.DTOs;

public class GetCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Expense" or "Income"
    public bool IsPersonal { get; set; }  // true if personal, false if system
}