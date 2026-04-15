using ExpenseApi.DTOs;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public interface IIncomeRepository
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<List<GetIncomeDto>> GetAllIncomesAsync(Guid userId);
    Task<GetIncomeDto?> GetIncomeByIdAsync(Guid id, Guid userId);
    Task<Income?> GetIncomeByIdEntityAsync(Guid id, Guid userId);
    Task AddIncomeAsync(Income income);
    Task DeleteIncomeAsync(Income income);
    Task SaveChangesAsync();
    Task<List<GetIncomeDto>> GetIncomesBySourceAsync(string source, Guid userId);
    Task<List<GetIncomeDto>> GetIncomesByDateRangeAsync(DateOnly startDate, DateOnly endDate, Guid userId);
    Task<List<GetIncomeSourceReportDto>> GetSourceReportAsync(Guid userId);
    Task<List<Income>> GetUserIncomesAsync(Guid userId);
}