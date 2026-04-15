using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.DTOs;

public class GetExpenseDto
{
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string? Description { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly CreatedAt { get; set; }
}

public class CategoryReportDto
{
    public string CategoryName { get; set; } = string.Empty;  // e.g. Food
    public int TotalAmount { get; set; }                       // total spent in category
    public int TotalExpenses { get; set; }                     // number of expenses in category
}


public class UpdateExpenseDto
{
    [Required(ErrorMessage = "Expense name is required.")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid category.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    public DateOnly Date { get; set; }
}


public class CreateExpenseDto
{
    [Required(ErrorMessage = "Expense name is required.")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;    // e.g. "Lunch", "Uber"

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid category.")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    public DateOnly Date { get; set; }
}


public class MonthlyReportDto
{
    public int Year { get; set; }           // e.g. 2026
    public int Month { get; set; }          // 1-12
    public int TotalAmount { get; set; }    // total spent that month
    public int TotalExpenses { get; set; }  // number of expenses that month
}

public class ExpenseSummaryDto
{
    public int TotalAmount { get; set; }
    public int TotalTransactions { get; set; }
    public int ThisMonthTransactions { get; set; }
    public double AverageAmount { get; set; }
}