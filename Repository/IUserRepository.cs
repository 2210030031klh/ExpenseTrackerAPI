using ExpenseApi.DTOs;

namespace ExpenseApi.Repository;

public interface IUserRepository
{
    Task<UserProfileDto?> GetUserByIdAsync(Guid userId);
}