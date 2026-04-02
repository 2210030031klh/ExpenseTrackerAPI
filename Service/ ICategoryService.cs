namespace ShashiControllerAPI.Service;
using ShashiControllerAPI.DTOs;

public interface ICategoryService
{
    Task<List<GetCategoryDto>> GetCategoriesAsync(Guid userId, string? type = null);
    Task<GetCategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId);
    Task<bool> DeleteCategoryAsync(int id, Guid userId);
}