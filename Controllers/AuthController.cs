using Microsoft.AspNetCore.Mvc;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;
using ShashiControllerAPI.Service;
using Microsoft.AspNetCore.Authorization;

namespace ShashiControllerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
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

    [Authorize(Roles = "Accountant")]
    [HttpGet("Admin-Only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("You are an admin!");
    }
}
