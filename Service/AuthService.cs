using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Repository;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

namespace ExpenseApi.Service;

public class AuthService(IAuthRepository authRepository, IConfiguration configuration) : IAuthService
{
    //register new user with username, email , passweord
    public async Task<RegisterResponseDto?> RegisterAsync(UserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new Exception("Username is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new Exception("Password is required");

        request.Username = request.Username.Trim().ToLower();
        request.Email = request.Email.Trim();

        if (await authRepository.UsernameExistsAsync(request.Username))
            throw new Exception($"Username '{request.Username}' is already taken.");

        if (await authRepository.EmailExistsAsync(request.Email))
            throw new Exception($"Email '{request.Email}' is already in use.");

        if (!request.Email.Contains("@"))
            throw new Exception("Invalid email format");

        if (request.Password.Length < 6)
            throw new Exception("Password must be atleast 6 characters");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = string.Empty,
            Role = request.Role
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

        await authRepository.AddUserAsync(user);
        await authRepository.SaveChangesAsync();

        return new RegisterResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
    //logs in user, creates otp then calls sendotpemail
    public async Task<bool> LoginAsync(LoginDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new Exception("Username is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new Exception("Password is required");

        var user = await authRepository.GetUserByUsernameAsync(request.Username.Trim().ToLower());

        if (user == null)
            return false;

        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
            return false;

        var otp = new Random().Next(100000, 999999).ToString();

        user.Otp = otp;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);

        await authRepository.SaveChangesAsync();

        await SendOtpEmail(user.Email, otp);

        return true;
    }
    //login calls this mehtos, to send otp thorugh smtp
    private async Task SendOtpEmail(string toEmail, string otp)
    {
        using var message = new MailMessage();
        message.From = new MailAddress("gshashidhar.reddyy@gmail.com");
        message.To.Add(toEmail);
        message.Subject = "Your OTP Code";
        message.Body = $"Hi User,\nOTP for login to your Expense Tracker account is: {otp}, it is valid for next 5 minutes";

        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(
                configuration["Email:Sender"],
                configuration["Email:Password"]
            ),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }
    //used to verify otp, if valid creates access and refresh token and return to user
    public async Task<TokenResponseDto?> VerifyOtpAsync(VerifyOtpDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new Exception("Username is required");

        if (string.IsNullOrWhiteSpace(request.Otp))
            throw new Exception("OTP is required");

        var user = await authRepository.GetUserByUsernameAsync(request.Username.Trim().ToLower());

        if (user == null)
            return null;

        if (user.Otp != request.Otp || user.OtpExpiryTime == null || user.OtpExpiryTime < DateTime.UtcNow)
            return null;

        user.Otp = null;
        user.OtpExpiryTime = null;

        await authRepository.SaveChangesAsync();

        return await CreateTokenResponse(user);
    }

    private async Task<TokenResponseDto> CreateTokenResponse(User user)
    {
        return new TokenResponseDto
        {
            UserId = user.UserId,
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

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await authRepository.SaveChangesAsync();
        return refreshToken;
    }
    //creates JWT using claims, issuer, ausience, key

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);


        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(configuration["Jwt:ExpiryInMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await authRepository.GetUserByIdAsync(request.UserId);

        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        return await CreateTokenResponse(user);
    }
        //gets emai from user, then generates otp then calls smtp sendforgotpasswordotp to mail
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email is required");

        var email = request.Email.Trim();

        var user = await authRepository.GetUserByEmailAsync(email);
        if (user == null)
            return false;

        var otp = new Random().Next(100000, 999999).ToString();

        user.Otp = otp;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);

        await authRepository.SaveChangesAsync();

        await SendForgotPasswordOtpEmail(user.Email, otp);

        return true;
    }
    //verify otp and update password
    public async Task<bool> ResetPasswordAsync(ResetPasswordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("Email is required");

        if (string.IsNullOrWhiteSpace(request.Otp))
            throw new Exception("OTP is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new Exception("New password is required");

        if (request.NewPassword.Length < 6)
            throw new Exception("Password must be atleast 6 characters");

        if (request.NewPassword != request.ConfirmPassword)
            throw new Exception("Passwords do not match");

        var email = request.Email.Trim();

        var user = await authRepository.GetUserByEmailAsync(email);
        if (user == null)
            return false;

        if (user.Otp != request.Otp || user.OtpExpiryTime == null || user.OtpExpiryTime < DateTime.UtcNow)
            return false;

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.NewPassword);
        user.Otp = null;
        user.OtpExpiryTime = null;

        await authRepository.SaveChangesAsync();

        return true;
    }
    //send otp
    private async Task SendForgotPasswordOtpEmail(string toEmail, string otp)
    {
        using var message = new MailMessage();
        message.From = new MailAddress("gshashidhar.reddyy@gmail.com");
        message.To.Add(toEmail);
        message.Subject = "Password Reset OTP";
        message.Body = $"Hi User,\nYour OTP to reset your Expense Tracker password is: {otp}. It is valid for the next 5 minutes.";

        using var smtp = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(
                configuration["Email:Sender"],
                configuration["Email:Password"]
            ),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }
      //if user loged in then take curent password and let him change passs
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            throw new Exception("Current password is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new Exception("New password is required");

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
            throw new Exception("Confirm password is required");

        if (request.NewPassword.Length < 6)
            throw new Exception("Password must be at least 6 characters");

        if (request.NewPassword != request.ConfirmPassword)
            throw new Exception("Passwords do not match");

        var user = await authRepository.GetUserByIdAsync(userId);

        if (user == null)
            throw new Exception("User not found");

        var hasher = new PasswordHasher<User>();

        var verifyResult = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.CurrentPassword
        );

        if (verifyResult == PasswordVerificationResult.Failed)
            return false;

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.NewPassword)
            != PasswordVerificationResult.Failed)
            throw new Exception("New password cannot be same as old password");

        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);

        await authRepository.SaveChangesAsync();

        return true;
    }
}