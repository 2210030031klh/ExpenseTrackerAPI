using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using ShashiControllerAPI.Service;

namespace ShashiControllerAPI.Service;

public class UserService(AppDbContext context) : IUserService
{
    public async Task<UserProfileDto?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users
            .Where(u => u.UserId == userId)
            .Select(u => new UserProfileDto
            {
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            })
            .FirstOrDefaultAsync();
    }
}