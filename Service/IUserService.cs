using ShashiControllerAPI.DTOs;

namespace ShashiControllerAPI.Service;

public interface IUserService
{
    Task<UserProfileDto?> GetUserByIdAsync(Guid userId);
}