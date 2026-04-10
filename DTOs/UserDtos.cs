using System.ComponentModel.DataAnnotations;
namespace ShashiControllerAPI.DTOs;


public class UserProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]    
    public required string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    public required string Password { get; set; }
}


public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "UserId is required.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateOnly CreatedAt { get; set; }
}

public class TokenResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}


public class UserDto
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public required string Email { get; set; }

    [MaxLength(20, ErrorMessage = "Role cannot exceed 20 characters.")]
    public string Role { get; set; } = "User"; // "User" or "Accountant"
}



public class VerifyOtpDto
{
    public string Username{ get; set;}=string.Empty;
    public string Otp{get; set;}=string.Empty;
}

 public class DashboardSummaryDto
    {
        public decimal NetBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal ThisMonth { get; set; }
        public decimal ThisMonthIncome { get; set; }
        public decimal ThisMonthExpense { get; set; }
    }