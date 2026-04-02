using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;

namespace ShashiControllerAPI.Service;

public interface IAuthService
{
    Task<RegisterResponseDto?> RegisterAsync(UserDto request);
    Task<bool> LoginAsync(LoginDto request);

    Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);

    Task<TokenResponseDto?> VerifyOtpAsync(VerifyOtpDto request);

    
}