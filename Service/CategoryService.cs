namespace ShashiControllerAPI.Service;
using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;

public class CategoryService(AppDbContext context) : ICategoryService
{
    // Get system categories + user's personal categories
public async Task<List<GetCategoryDto>> GetCategoriesAsync(Guid userId, string? type = null)
{
    var query = context.Categories
        .Where(c => c.UserId == null || c.UserId == userId);

    if (!string.IsNullOrWhiteSpace(type))
    {
        query = query.Where(c => c.Type == type);
    }

    return await query
        .Select(c => new GetCategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            IsPersonal = c.UserId != null
        })
        .ToListAsync();
}

   // Create personal category
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

        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return new GetCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            IsPersonal = true
        };
    }
    // Delete only if it belongs to the user
    public async Task<bool> DeleteCategoryAsync(int id, Guid userId)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

        if (category is null)
            return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}