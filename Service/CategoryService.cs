using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Repository;

namespace ExpenseApi.Service;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<List<GetCategoryDto>> GetCategoriesAsync(Guid userId, string? type = null)
    {
        var categories = await categoryRepository.GetCategoriesAsync(userId, type);

        return categories
            .Select(c => new GetCategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                IsPersonal = c.UserId != null
            })
            .ToList();
    }

    public async Task<GetCategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id.");

        if (string.IsNullOrWhiteSpace(dto.CategoryName))
            throw new ArgumentException("Category name is required.");

        var categoryName = dto.CategoryName.Trim();

        var category = new Category
        {
            CategoryName = categoryName,
            Type = dto.Type,
            UserId = userId
        };

        await categoryRepository.AddCategoryAsync(category);
        await categoryRepository.SaveChangesAsync();

        return new GetCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            IsPersonal = true
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id, Guid userId)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(id, userId);

        if (category is null)
            return false;

        await categoryRepository.DeleteCategoryAsync(category);
        await categoryRepository.SaveChangesAsync();
        return true;
    }
}