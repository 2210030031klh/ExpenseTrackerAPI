using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.Models;

public class Budget
{
    public Guid BudgetId { get; set; }                   // PK

    public Guid UserId { get; set; }                     // FK -> Users.UserId

    public int CategoryId { get; set; }                  // FK -> Categories.CategoryId

    public int Amount { get; set; }                      // budget limit e.g. 5000

    public int Month { get; set; }                       // 1-12 e.g. 3 = March

    public int Year { get; set; }                        // e.g. 2026

    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow); // auto set

    // Navigation Properties:
    public User User { get; set; } = null!;              // FK navigation to User
    public Category Category { get; set; } = null!;      // FK navigation to Category
                                                          // used to get CategoryName in reports

    // Relationships:
    // Budget (Many) → User (1)
    // Budget (Many) → Category (1)
    // One user can only have ONE budget per category per month
}