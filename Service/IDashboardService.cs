using ExpenseApi.DTOs;

namespace ExpenseApi.Service
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
    }
}