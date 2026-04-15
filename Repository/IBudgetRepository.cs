using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public interface IBudgetRepository
{
    Task<List<Budget>> GetAllBudgetsAsync(Guid userId);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<bool> BudgetExistsAsync(Guid userId, int categoryId, int month, int year);
    Task<Budget?> GetBudgetByIdAsync(Guid id, Guid userId);
    Task<bool> DuplicateBudgetExistsAsync(Guid userId, int categoryId, int month, int year, Guid id);
    Task AddBudgetAsync(Budget budget);
    Task DeleteBudgetAsync(Budget budget);
    Task SaveChangesAsync();
    Task<List<Budget>> GetBudgetsForReportAsync(Guid userId, int month, int year);
    Task<Dictionary<int, decimal>> GetExpenseTotalsAsync(Guid userId, List<int> categoryIds, int month, int year);
}