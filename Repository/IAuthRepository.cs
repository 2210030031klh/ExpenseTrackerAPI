using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public interface IAuthRepository
{
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task AddUserAsync(User user);
    Task<bool> ChangePasswordAsync(User user);
    Task SaveChangesAsync();

}