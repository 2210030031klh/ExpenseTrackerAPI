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


public class GetIncomeDto
{
    public Guid IncomeId { get; set; }
    public Guid UserId { get; set; }
    public string? Name{get; set;}
    public int Amount { get; set; }
    public string? Description { get; set; }
    public string Source { get; set; } = string.Empty;  // e.g. Salary, Freelance
    public DateOnly Date { get; set; }
    public DateOnly CreatedAt { get; set; }
}


public class UpdateIncomeDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public int Amount { get; set; }

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Source is required.")]
    [MinLength(2, ErrorMessage = "Source must be at least 2 characters.")]
    [MaxLength(100, ErrorMessage = "Source cannot exceed 100 characters.")]
    public string Source { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required.")]
    public DateOnly Date { get; set; }
}

public class GetIncomeSourceReportDto
{
    public string Source { get; set; } = string.Empty;
    public int TotalAmount { get; set; }
    public int Count { get; set; }
}

public class IncomeSummaryDto
{
    public int TotalAmount { get; set; }
    public int TotalTransactions { get; set; }
    public int ThisMonthTransactions { get; set; }
    public double AverageAmount { get; set; }
}