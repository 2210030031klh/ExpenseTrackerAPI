using ShashiControllerAPI.DTOs;

namespace ShashiControllerAPI.Service
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
    }
}