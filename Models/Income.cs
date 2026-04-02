using System.ComponentModel.DataAnnotations;

namespace ShashiControllerAPI.Models;

public class Income
{
    public Guid IncomeId { get; set; }                   // PK

    public Guid UserId { get; set; }                     // FK → Users.UserId

    public int Amount { get; set; }   
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]                                      // VARCHAR(200) in MySQL
    public string? Description { get; set; }             // optional notes

    [MaxLength(100)]                                      // VARCHAR(100) in MySQL
    public string Source { get; set; } = string.Empty;  // e.g. Salary, Freelance, Business

    public DateOnly Date { get; set; }                   // when income was received
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow); // auto set

    // Navigation Properties:
    public User User { get; set; } = null!;              // FK navigation to User

    // Relationships:
    // Income (Many) → User (1)
}