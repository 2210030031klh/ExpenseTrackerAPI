using ExpenseApi.DTOs;

namespace ExpenseApi.Service;

public interface IUserService
{
    Task<UserProfileDto?> GetUserByIdAsync(Guid userId);
}