using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;
using ExpenseApi.DTOs;

namespace ExpenseApi.Repository;

public class UserRepository(AppDbContext context) : IUserRepository
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