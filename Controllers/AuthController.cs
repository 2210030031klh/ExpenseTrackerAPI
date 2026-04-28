using Microsoft.AspNetCore.Mvc;
using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ExpenseApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    //
    public async Task<ActionResult <User>> Register(UserDto Request)
    {
        try{
            var user=await authService.RegisterAsync(Request);
            return Ok(user);
            }
        catch (Exception ex)
        {
            return BadRequest(new{message=ex.Message});
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<bool>> Login(LoginDto Request)
    {
        var result=await authService.LoginAsync(Request);
        if(result is false) 
            return Unauthorized("Invalid username or password.");
            
        return Ok(new{message="OTP sent to registered email"});
    }

    [HttpPost("Verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto request)
    {
        var result=await authService.VerifyOtpAsync(request);
        if(result ==null)
            return BadRequest("Invalis or Expirred Otp");
            return Ok(result);
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
    {
        var result = await authService.ForgotPasswordAsync(request);

        if (!result)
            return BadRequest(new { message = "User with this email does not exist" });

        return Ok(new { message = "OTP sent to your email" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
    {
        var result = await authService.ResetPasswordAsync(request);

        if (!result)
            return BadRequest(new { message = "Invalid OTP or OTP expired" });

        return Ok(new { message = "Password reset successful" });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new { message = "Invalid token" });

            var result = await authService.ChangePasswordAsync(userId, request);

            if (!result)
                return BadRequest(new { message = "Current password is incorrect" });

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokenAsync(request);

            if (result == null)
                return Unauthorized("Invalid or expired refresh token");

            return Ok(result);
        }
}
