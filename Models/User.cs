using System.ComponentModel.DataAnnotations;

namespace ExpenseApi.Models;

public class User
{
    public Guid UserId { get; set; }                    // PK

    [MaxLength(50)]                                      // VARCHAR(50) in MySQL
    public required string Username { get; set; }        // login name

    [MaxLength(100)]                                     // VARCHAR(100) in MySQL
    public required string Email { get; set; }           // email address

    [MaxLength(255)]                                     // VARCHAR(255) in MySQL
    public required string PasswordHash { get; set; }    // encrypted password

    [MaxLength(20)]                                      // VARCHAR(20) in MySQL
    public string Role { get; set; } = "User";           // "User" or "Accountant"

    [MaxLength(255)]                                     // VARCHAR(255) in MySQL
    public string? RefreshToken { get; set; }            // JWT refresh token

    public DateTime? RefreshTokenExpiryTime { get; set; } // refresh token expiry

    public string? Otp {get ; set; }
    public DateTime?OtpExpiryTime{get; set;}
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow); // auto set

    // Relationships:
    // User (1) → Expenses (Many)
    // User (1) → Incomes (Many)
    // User (1) → Budgets (Many)
    // User (1) → Categories (Many) personal only
}