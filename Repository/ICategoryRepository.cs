using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public interface ICategoryRepository
{
    Task<List<Category>> GetCategoriesAsync(Guid userId, string? type = null);
    Task AddCategoryAsync(Category category);
    Task<Category?> GetCategoryByIdAsync(int id, Guid userId);
    Task DeleteCategoryAsync(Category category);
    Task SaveChangesAsync();
}