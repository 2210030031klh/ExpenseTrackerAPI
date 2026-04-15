using ExpenseApi.DTOs;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public interface IExpenseRepository
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<bool> CategoryExistsAsync(int categoryId);

    Task<IQueryable<Expense>> GetAllExpensesQueryAsync(Guid userId);
    Task<IQueryable<Expense>> GetExpensesByCategoryQueryAsync(Guid userId, string category);
    Task<Expense?> GetExpenseByIdEntityAsync(Guid id, Guid userId);
    Task<GetExpenseDto?> GetExpenseByIdAsync(Guid id, Guid userId);
    Task<List<GetExpenseDto>> GetExpensesByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task<List<MonthlyReportDto>> GetMonthlyReportAsync(Guid userId);
    Task<List<CategoryReportDto>> GetCategoryReportAsync(Guid userId);
    Task<List<GetExpenseDto>> GetAllUsersExpensesAsync();
    Task<List<Expense>> GetUserExpensesAsync(Guid userId);

    Task<bool> DuplicateExpenseExistsAsync(Guid userId, int categoryId, int amount, DateOnly date, string name, string? description);
    Task<bool> DuplicateExpenseExistsForUpdateAsync(Guid id, Guid userId, int categoryId, int amount, DateOnly date, string name, string? description);

    Task AddExpenseAsync(Expense expense);
    Task DeleteExpenseAsync(Expense expense);
    Task SaveChangesAsync();
     Task<List<Expense>> GetUserExpensesForExportAsync(Guid userId);


}