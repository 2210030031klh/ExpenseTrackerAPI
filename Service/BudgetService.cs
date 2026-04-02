using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;

namespace ShashiControllerAPI.Service;

public class BudgetService(AppDbContext context) : IBudgetService
{
    public async Task<List<GetBudgetDto>> GetAllBudgetsAsync(Guid userId)
        => await context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId)
            .Select(b => new GetBudgetDto
            {
                BudgetId = b.BudgetId,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.CategoryName,
                Amount = b.Amount,
                Month = b.Month,
                Year = b.Year
            })
            .ToListAsync();

    public async Task<CreateBudgetDto> AddBudgetAsync(CreateBudgetDto budget, Guid userId)
    {
        // Check if budget already exists for this category/month/year
    
        var exists = await context.Budgets.AnyAsync(b =>
            b.UserId == userId &&
            b.CategoryId == budget.CategoryId &&
            b.Month == budget.Month &&
            b.Year == budget.Year);

        if (exists)
            throw new ArgumentException("Budget already exists for this category and month.");

        var newBudget = new Budget
        {
            UserId = userId,
            CategoryId = budget.CategoryId,
            Amount = budget.Amount,
            Month = budget.Month,
            Year = budget.Year
        };

        context.Budgets.Add(newBudget);
        await context.SaveChangesAsync();

        return new CreateBudgetDto
        {
            CategoryId = newBudget.CategoryId,
            Amount = newBudget.Amount,
            Month = newBudget.Month,
            Year = newBudget.Year
        };
    }

    public async Task<bool> UpdateBudgetAsync(Guid id, CreateBudgetDto budget, Guid userId)
        {
            var existing = await context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);
            if (existing is null)
            return false;

            existing.Amount = budget.Amount;
            existing.Month = budget.Month;
            existing.Year = budget.Year;
            existing.CategoryId = budget.CategoryId;

            await context.SaveChangesAsync();
            return true;
        }

    public async Task<bool> DeleteBudgetAsync(Guid id, Guid userId)
    {
        var budget = await context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId);
        if (budget is null)
            return false;

        context.Budgets.Remove(budget);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<BudgetReportDto>> GetBudgetReportAsync(Guid userId, int month, int year)
    {
        var budgets = await context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .ToListAsync();

        var result = new List<BudgetReportDto>();

        foreach (var budget in budgets)
        {
            var spent = await context.Expenses
                .Where(e => e.UserId == userId &&
                            e.CategoryId == budget.CategoryId &&
                            e.Date.Month == month &&
                            e.Date.Year == year)
                .SumAsync(e => e.Amount);

            var remaining = budget.Amount - spent;
            var percentage = budget.Amount > 0
                ? Math.Round((double)spent / budget.Amount * 100, 2)
                : 0;

            result.Add(new BudgetReportDto
            {
                CategoryName = budget.Category.CategoryName,
                BudgetAmount = budget.Amount,
                SpentAmount = spent,
                RemainingAmount = remaining,
                Percentage = percentage,
                IsOverBudget = spent > budget.Amount
            });
        }

        return result;
    }
}