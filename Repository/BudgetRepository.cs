using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public class BudgetRepository(AppDbContext context) : IBudgetRepository
{
    public async Task<List<Budget>> GetAllBudgetsAsync(Guid userId)
    {
        return await context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await context.Categories
            .AnyAsync(c => c.CategoryId == categoryId);
    }

    public async Task<bool> BudgetExistsAsync(Guid userId, int categoryId, int month, int year)
    {
        return await context.Budgets
            .AnyAsync(b => b.UserId == userId &&
                           b.CategoryId == categoryId &&
                           b.Month == month &&
                           b.Year == year);
    }

    public async Task<Budget?> GetBudgetByIdAsync(Guid id, Guid userId)
    {
        return await context.Budgets
            .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);
    }

    public async Task<bool> DuplicateBudgetExistsAsync(Guid userId, int categoryId, int month, int year, Guid id)
    {
        return await context.Budgets
            .AnyAsync(b => b.UserId == userId &&
                           b.CategoryId == categoryId &&
                           b.Month == month &&
                           b.Year == year &&
                           b.BudgetId != id);
    }

    public async Task AddBudgetAsync(Budget budget)
    {
        await context.Budgets.AddAsync(budget);
    }

    public Task DeleteBudgetAsync(Budget budget)
    {
        context.Budgets.Remove(budget);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task<List<Budget>> GetBudgetsForReportAsync(Guid userId, int month, int year)
    {
        return await context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.UserId == userId &&
                        b.Month == month &&
                        b.Year == year)
            .ToListAsync();
    }

    public async Task<Dictionary<int, decimal>> GetExpenseTotalsAsync(Guid userId, List<int> categoryIds, int month, int year)
    {
        return await context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId &&
                        categoryIds.Contains(e.CategoryId) &&
                        e.Date.Month == month &&
                        e.Date.Year == year)
            .GroupBy(e => e.CategoryId)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => (decimal)e.Amount));
    }
}