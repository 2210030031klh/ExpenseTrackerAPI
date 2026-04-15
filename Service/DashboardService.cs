using ExpenseApi.DTOs;
using ExpenseApi.Repository;

namespace ExpenseApi.Service;

public class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonth = today.Month;
        var currentYear = today.Year;

        var totalIncome = await dashboardRepository.GetTotalIncomeAsync(userId);
        var totalExpenses = await dashboardRepository.GetTotalExpensesAsync(userId);
        var thisMonthIncome = await dashboardRepository.GetThisMonthIncomeAsync(userId, currentMonth, currentYear);
        var thisMonthExpense = await dashboardRepository.GetThisMonthExpensesAsync(userId, currentMonth, currentYear);

        return new DashboardSummaryDto
        {
            NetBalance = totalIncome - totalExpenses,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            ThisMonth = thisMonthIncome - thisMonthExpense,
            ThisMonthIncome = thisMonthIncome,
            ThisMonthExpense = thisMonthExpense
        };
    }
}