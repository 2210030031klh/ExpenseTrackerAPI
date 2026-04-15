using ExpenseApi.DTOs;
using ExpenseApi.Models;

namespace ExpenseApi.Service;

public interface IExpenseService
{
    Task<PagedResultDto<GetExpenseDto>> GetAllExpensesAsync(Guid userId, int pageNumber, int pageSize);

    Task<GetExpenseDto?> GetExpensesByIdAsync(Guid id, Guid userId);

    Task<PagedResultDto<GetExpenseDto>> GetExpensesByCategoryAsync(string category, Guid userId, int pageNumber, int pageSize);

    Task<CreateExpenseDto> AddExpenseAsync(CreateExpenseDto expense, Guid userId);

    Task<bool> UpdateExpenseAsync(Guid id, UpdateExpenseDto  expense, Guid userId);

    Task<bool> DeleteExpenseAsync(Guid id, Guid userId);

    Task<List<GetExpenseDto>> GetExpensesByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);

    Task<List<MonthlyReportDto>> GetMonthlyReportAsync(Guid userId);

    Task<List<CategoryReportDto>> GetCategoryReportAsync(Guid userId);

    Task<List<GetExpenseDto>> GetAllUsersExpensesAsync();

    
    Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid userId);

    Task<byte[]> ExportExpensesToExcelAsync(Guid userId);


}