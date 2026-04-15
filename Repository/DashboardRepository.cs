using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;

namespace ExpenseApi.Repository;

public class DashboardRepository(AppDbContext context) : IDashboardRepository
{
    public async Task<decimal> GetTotalIncomeAsync(Guid userId)
    {
        return await context.Incomes
            .Where(i => i.UserId == userId)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;
    }

    public async Task<decimal> GetTotalExpensesAsync(Guid userId)
    {
        return await context.Expenses
            .Where(e => e.UserId == userId)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;
    }

    public async Task<decimal> GetThisMonthIncomeAsync(Guid userId, int month, int year)
    {
        return await context.Incomes
            .Where(i => i.UserId == userId &&
                        i.Date.Month == month &&
                        i.Date.Year == year)
            .SumAsync(i => (decimal?)i.Amount) ?? 0;
    }

    public async Task<decimal> GetThisMonthExpensesAsync(Guid userId, int month, int year)
    {
        return await context.Expenses
            .Where(e => e.UserId == userId &&
                        e.Date.Month == month &&
                        e.Date.Year == year)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;
    }
}