using ShashiControllerAPI.Models;

public interface IUserService
{
Task<UserProfileDto?> GetUserByIdAsync(Guid userId);
}