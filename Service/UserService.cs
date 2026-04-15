using ExpenseApi.DTOs;
using ExpenseApi.Repository;

namespace ExpenseApi.Service;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserProfileDto?> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id.");

        return await userRepository.GetUserByIdAsync(userId);
    }
}