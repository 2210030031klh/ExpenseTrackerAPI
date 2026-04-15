namespace ExpenseApi.Repository;

public interface IDashboardRepository
{
    Task<decimal> GetTotalIncomeAsync(Guid userId);
    Task<decimal> GetTotalExpensesAsync(Guid userId);
    Task<decimal> GetThisMonthIncomeAsync(Guid userId, int month, int year);
    Task<decimal> GetThisMonthExpensesAsync(Guid userId, int month, int year);
}