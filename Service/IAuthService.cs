using ExpenseApi.DTOs;
using ExpenseApi.Models;

namespace ExpenseApi.Service;

public interface IAuthService
{
    Task<RegisterResponseDto?> RegisterAsync(UserDto request);
    Task<bool> LoginAsync(LoginDto request);

    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);

    Task<TokenResponseDto?> VerifyOtpAsync(VerifyOtpDto request);


    Task<bool> ForgotPasswordAsync(ForgotPasswordDto request);
    Task<bool> ResetPasswordAsync(ResetPasswordDto request);

    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request);

    
}