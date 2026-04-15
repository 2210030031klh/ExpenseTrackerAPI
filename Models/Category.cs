using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.Models;

public class Category
{
    public int CategoryId { get; set; }                  // PK

    [MaxLength(50)]                                       // VARCHAR(50) in MySQL
    public required string CategoryName { get; set; }    // e.g. Food, Transport, Gym

    public Guid? UserId { get; set; }   

    [MaxLength(20)]
    public string Type { get; set; } = string.Empty; // "Expense" or "Income"
    // Relationships:
    // Category (1) → Expenses (Many)
    // Category (1) → Budgets (Many)
}