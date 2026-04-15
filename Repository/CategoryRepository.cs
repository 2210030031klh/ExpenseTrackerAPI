using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<List<Category>> GetCategoriesAsync(Guid userId, string? type = null)
    {
        var query = context.Categories
            .Where(c => c.UserId == null || c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(c => c.Type == type);
        }

        return await query.ToListAsync();
    }

    public async Task AddCategoryAsync(Category category)
    {
        await context.Categories.AddAsync(category);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, Guid userId)
    {
        return await context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
    }

    public Task DeleteCategoryAsync(Category category)
    {
        context.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}