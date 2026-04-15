namespace ExpenseApi.Service;
using ExpenseApi.DTOs;

public interface ICategoryService
{
    Task<List<GetCategoryDto>> GetCategoriesAsync(Guid userId, string? type = null);
    Task<GetCategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId);
    Task<bool> DeleteCategoryAsync(int id, Guid userId);
}