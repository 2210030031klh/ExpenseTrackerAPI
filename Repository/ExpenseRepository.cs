using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;
using ExpenseApi.DTOs;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public class ExpenseRepository(AppDbContext context) : IExpenseRepository
{
    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await context.Users.AnyAsync(u => u.UserId == userId);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await context.Categories.AnyAsync(c => c.CategoryId == categoryId);
    }

    public Task<IQueryable<Expense>> GetAllExpensesQueryAsync(Guid userId)
    {
        var query = context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
            .AsQueryable();

        return Task.FromResult(query);
    }

    public Task<IQueryable<Expense>> GetExpensesByCategoryQueryAsync(Guid userId, string category)
    {
        var query = context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Category.CategoryName == category)
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
            .AsQueryable();

        return Task.FromResult(query);
    }

    public async Task<Expense?> GetExpenseByIdEntityAsync(Guid id, Guid userId)
    {
        return await context.Expenses
            .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);
    }

    public async Task<GetExpenseDto?> GetExpenseByIdAsync(Guid id, Guid userId)
    {
        return await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.ExpenseId == id && e.UserId == userId)
            .Select(e => new GetExpenseDto
            {
                ExpenseId = e.ExpenseId,
                UserId = e.UserId,
                Name = e.Name,
                CategoryName = e.Category.CategoryName,
                Amount = e.Amount,
                Description = e.Description,
                Date = e.Date,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<GetExpenseDto>> GetExpensesByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        return await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId &&
                        e.Date >= startDate &&
                        e.Date <= endDate)
            .Select(e => new GetExpenseDto
            {
                ExpenseId = e.ExpenseId,
                UserId = e.UserId,
                Name = e.Name,
                CategoryName = e.Category.CategoryName,
                Amount = e.Amount,
                Description = e.Description,
                Date = e.Date,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<MonthlyReportDto>> GetMonthlyReportAsync(Guid userId)
    {
        return await context.Expenses
            .Where(e => e.UserId == userId)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new MonthlyReportDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalAmount = g.Sum(e => e.Amount),
                TotalExpenses = g.Count()
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync();
    }

    public async Task<List<CategoryReportDto>> GetCategoryReportAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId &&
                        e.Date.Year == today.Year &&
                        e.Date.Month == today.Month)
            .GroupBy(e => e.Category.CategoryName)
            .Select(g => new CategoryReportDto
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                TotalExpenses = g.Count()
            })
            .OrderByDescending(r => r.TotalAmount)
            .ToListAsync();
    }

    public async Task<List<GetExpenseDto>> GetAllUsersExpensesAsync()
    {
        return await context.Expenses
            .Include(e => e.Category)
            .Select(e => new GetExpenseDto
            {
                ExpenseId = e.ExpenseId,
                UserId = e.UserId,
                Name = e.Name,
                CategoryName = e.Category.CategoryName,
                Amount = e.Amount,
                Description = e.Description,
                Date = e.Date,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<Expense>> GetUserExpensesAsync(Guid userId)
    {
        return await context.Expenses
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> DuplicateExpenseExistsAsync(Guid userId, int categoryId, int amount, DateOnly date, string name, string? description)
    {
        return await context.Expenses.AnyAsync(e =>
            e.UserId == userId &&
            e.CategoryId == categoryId &&
            e.Amount == amount &&
            e.Date == date &&
            e.Name.ToLower() == name.ToLower() &&
            (e.Description ?? "") == (description ?? "")
        );
    }

    public async Task<bool> DuplicateExpenseExistsForUpdateAsync(Guid id, Guid userId, int categoryId, int amount, DateOnly date, string name, string? description)
    {
        return await context.Expenses.AnyAsync(e =>
            e.ExpenseId != id &&
            e.UserId == userId &&
            e.CategoryId == categoryId &&
            e.Amount == amount &&
            e.Date == date &&
            e.Name == name &&
            (e.Description ?? "") == (description ?? "")
        );
    }

    public async Task AddExpenseAsync(Expense expense)
    {
        await context.Expenses.AddAsync(expense);
    }

    public Task DeleteExpenseAsync(Expense expense)
    {
        context.Expenses.Remove(expense);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
    public async Task<List<Expense>> GetUserExpensesForExportAsync(Guid userId)
    {
        return await context.Expenses
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }
}