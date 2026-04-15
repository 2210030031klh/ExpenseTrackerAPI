using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.Models;

public class Expense
{
    public Guid ExpenseId { get; set; }                  // PK

    public Guid UserId { get; set; }                     // FK → Users.UserId

    public int CategoryId { get; set; }                  // FK → Categories.CategoryId

    [MaxLength(100)]                                      // VARCHAR(100) in MySQL
    public string Name { get; set; } = string.Empty;    // expense name e.g. "Lunch", "Uber"

    public int Amount { get; set; }                      // how much was spent

    [MaxLength(200)]                                      // VARCHAR(200) in MySQL
    public string? Description { get; set; }             // optional notes

    public DateOnly Date { get; set; }                   // when expense happened
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow); // auto set

    // Navigation Properties:
    public Category Category { get; set; } = null!;     // used to get CategoryName

    // Relationships:
    // Expense (Many) → User (1)
    // Expense (Many) → Category (1)
}