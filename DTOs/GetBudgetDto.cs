namespace ShashiControllerAPI.DTOs;

public class GetBudgetDto
{
    public Guid BudgetId { get; set; }

    public int CategoryId { get; set; }                            // foreign key to Category
    public string CategoryName { get; set; } = string.Empty;  // from Category table
    public int Amount { get; set; }                            // budget limit
    public int Month { get; set; }                             // 1-12
    public int Year { get; set; }                              // e.g. 2026
    
}