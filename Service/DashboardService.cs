using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;

namespace ShashiControllerAPI.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentMonth = today.Month;
            var currentYear = today.Year;

            var totalIncome = await _context.Incomes
                .Where(i => i.UserId == userId)
                .SumAsync(i => (decimal?)i.Amount) ?? 0;

            var totalExpenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var thisMonthIncome = await _context.Incomes
                .Where(i => i.UserId == userId &&
                            i.Date.Month == currentMonth &&
                            i.Date.Year == currentYear)
                .SumAsync(i => (decimal?)i.Amount) ?? 0;

            var thisMonthExpense = await _context.Expenses
                .Where(e => e.UserId == userId &&
                            e.Date.Month == currentMonth &&
                            e.Date.Year == currentYear)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

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
}