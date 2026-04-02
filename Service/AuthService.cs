using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

namespace ShashiControllerAPI.Service;

public class AuthService(AppDbContext context,IConfiguration configuration) : IAuthService
{
    public async Task<RegisterResponseDto?> RegisterAsync(UserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new Exception("Username is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new Exception("Password is required");

        // Check username
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
            throw new Exception($"Username '{request.Username}' is already taken.");

        // Check email
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception($"Email '{request.Email}' is already in use.");

        if (!request.Email.Contains("@"))
            throw new Exception("Invalid email format");

        if(request.Password.Length < 6)
            throw new Exception("Password must be atleast 6 characters");
        
        request.Username = request.Username.Trim();
        request.Email = request.Email.Trim();
        

        var user = new User
        {
            Username = request.Username.ToLower(),
            Email = request.Email,
            PasswordHash = string.Empty,  // temp, will be overwritten below
            Role = request.Role
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        // return user;
            return new RegisterResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

    }


    public async Task<bool> LoginAsync(LoginDto request)
    {
        // Implementation for login logic
        var user = await context.Users
        .FirstOrDefaultAsync(u => u.Username == request.Username.ToLower());
        if (user == null)
        {
            return false;
        }

        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return false;
            // throw new Exception("Invalid password.");
        }
        // return await CreateTokenResponse(user);
        var otp= new Random().Next(100000,999999).ToString();

        user.Otp=otp;
        user.OtpExpiryTime=DateTime.UtcNow.AddMinutes(5);

        await context.SaveChangesAsync();

        await SendOtpEmail(user.Email, otp);

        return true;
    }
    
    private async Task SendOtpEmail(string toEmail, string otp)
    {
        using var message= new MailMessage();
        message.From = new MailAddress("gshashidhar.reddyy@gmail.com");
        message.To.Add(toEmail);
        message.Subject="Your OTP Code";
        message.Body=$"Hi User,\nOTP for login to your Expense Tracker account is: {otp}, it is valid for next 5 minutes";

        using var smtp= new SmtpClient("smtp.gmail.com",587)
        {
            Credentials = new NetworkCredential(
                configuration["Email:Sender"],
                configuration["Email:Password"]

            ),
        EnableSsl=true
        };
        await smtp.SendMailAsync(message);
    }

    public async Task<TokenResponseDto?> VerifyOtpAsync(VerifyOtpDto request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u=> u.Username == request.Username);
        if(user ==null)
        {
            return null;
        }
        if(user.Otp!= request.Otp || user.OtpExpiryTime < DateTime.UtcNow)
            return null;

        user.Otp = null;
        user.OtpExpiryTime=null;
        
        await context.SaveChangesAsync();


        return await CreateTokenResponse(user);
    }

    private async Task<TokenResponseDto> CreateTokenResponse(User user)
    {
        return new TokenResponseDto
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        
    }
    private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if(user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null; // Invalid refresh token
        }
        return user;
    }  

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken; 
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Set expiry time (e.g., 24 hours)
        await context.SaveChangesAsync();
        return refreshToken;
    }   

    private string CreateToken(User user)
    {
        // Implementation for token creation logic
        var claims= new List<Claim>
        {
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
            new Claim(ClaimTypes.Role,user.Role)
        };
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if(user is null)
        {
            return null; // Invalid refresh token
        }

        // var response = new TokenResponseDto
        // {
        //     AccessToken = CreateToken(user),
        //     RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        // };

        return await CreateTokenResponse(user);
    }
}