using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public class AuthRepository(AppDbContext context) : IAuthRepository
{
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task AddUserAsync(User user)
    {
        await context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
    public async Task<bool> ChangePasswordAsync(User user)
    {
        context.Users.Update(user);
        return await context.SaveChangesAsync() > 0;
    }
}