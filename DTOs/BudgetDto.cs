namespace ShashiControllerAPI.DTOs;

using System.ComponentModel.DataAnnotations;
public class BudgetReportDto
{
    public string CategoryName { get; set; } = string.Empty;  // category name
    public int BudgetAmount { get; set; }                      // how much budgeted
    public int SpentAmount { get; set; }                       // how much spent
    public int RemainingAmount { get; set; }                   // budget - spent
    public double Percentage { get; set; }                     // spent/budget × 100
    public bool IsOverBudget { get; set; }                     // spent > budget
}

public class CreateBudgetDto
{
    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid category.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Budget amount must be greater than 0.")]
    public int Amount { get; set; }

    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; }

    [Range(2000, 2100, ErrorMessage = "Invalid year.")]
    public int Year { get; set; }
}


public class GetBudgetDto
{
    public Guid BudgetId { get; set; }
    public int CategoryId { get; set; }                            // foreign key to Category
    public string CategoryName { get; set; } = string.Empty;  // from Category table
    public int Amount { get; set; }                            // budget limit
    public int Month { get; set; }                             // 1-12
    public int Year { get; set; }                              // e.g. 2026
    
}

