using ShashiControllerAPI.DTOs;

namespace ShashiControllerAPI.Service;

public interface IBudgetService
{
    Task<List<GetBudgetDto>> GetAllBudgetsAsync(Guid userId);
    Task<CreateBudgetDto> AddBudgetAsync(CreateBudgetDto budget, Guid userId);
    Task<bool> UpdateBudgetAsync(Guid id, CreateBudgetDto budget, Guid userId);
    Task<bool> DeleteBudgetAsync(Guid id, Guid userId);
    Task<List<BudgetReportDto>> GetBudgetReportAsync(Guid userId, int month, int year);
}
