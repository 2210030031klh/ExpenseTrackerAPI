using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Repository;

namespace ExpenseApi.Service;

public class BudgetService(IBudgetRepository budgetRepository) : IBudgetService
{
    public async Task<List<GetBudgetDto>> GetAllBudgetsAsync(Guid userId)
    {
        var budgets = await budgetRepository.GetAllBudgetsAsync(userId);

        return budgets.Select(b => new GetBudgetDto
        {
            BudgetId = b.BudgetId,
            CategoryId = b.CategoryId,
            CategoryName = b.Category.CategoryName,
            Amount = b.Amount,
            Month = b.Month,
            Year = b.Year
        }).ToList();
    }

    public async Task<CreateBudgetDto> AddBudgetAsync(CreateBudgetDto budget, Guid userId)
    {
        if (budget is null)
            throw new ArgumentNullException(nameof(budget), "Budget data is required.");

        if (budget.CategoryId == 0)
            throw new ArgumentException("Valid category is required.");

        if (budget.Amount <= 0)
            throw new ArgumentException("Budget amount must be greater than 0.");

        if (budget.Month < 1 || budget.Month > 12)
            throw new ArgumentException("Month must be between 1 and 12.");

        if (budget.Year < 2000 || budget.Year > 2100)
            throw new ArgumentException("Year is invalid.");

        var categoryExists = await budgetRepository.CategoryExistsAsync(budget.CategoryId);

        if (!categoryExists)
            throw new ArgumentException("Selected category does not exist.");

        var exists = await budgetRepository.BudgetExistsAsync(userId, budget.CategoryId, budget.Month, budget.Year);

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

        await budgetRepository.AddBudgetAsync(newBudget);
        await budgetRepository.SaveChangesAsync();

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
        if (budget is null)
            throw new ArgumentNullException(nameof(budget), "Budget data is required.");

        if (budget.CategoryId == 0)
            throw new ArgumentException("Valid category is required.");

        if (budget.Amount <= 0)
            throw new ArgumentException("Budget amount must be greater than 0.");

        if (budget.Month < 1 || budget.Month > 12)
            throw new ArgumentException("Month must be between 1 and 12.");

        if (budget.Year < 2000 || budget.Year > 2100)
            throw new ArgumentException("Year is invalid.");

        var existing = await budgetRepository.GetBudgetByIdAsync(id, userId);

        if (existing is null)
            return false;

        var categoryExists = await budgetRepository.CategoryExistsAsync(budget.CategoryId);

        if (!categoryExists)
            throw new ArgumentException("Selected category does not exist.");

        var duplicateExists = await budgetRepository.DuplicateBudgetExistsAsync(
            userId,
            budget.CategoryId,
            budget.Month,
            budget.Year,
            id
        );

        if (duplicateExists)
            throw new ArgumentException("Another budget already exists for this category and month.");

        if (existing.CategoryId == budget.CategoryId &&
            existing.Amount == budget.Amount &&
            existing.Month == budget.Month &&
            existing.Year == budget.Year)
            throw new ArgumentException("No changes detected.");

        existing.CategoryId = budget.CategoryId;
        existing.Amount = budget.Amount;
        existing.Month = budget.Month;
        existing.Year = budget.Year;

        await budgetRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBudgetAsync(Guid id, Guid userId)
    {
        var budget = await budgetRepository.GetBudgetByIdAsync(id, userId);

        if (budget is null)
            return false;

        await budgetRepository.DeleteBudgetAsync(budget);
        await budgetRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<BudgetReportDto>> GetBudgetReportAsync(Guid userId, int month, int year)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("Month must be between 1 and 12.");

        if (year < 2000 || year > 2100)
            throw new ArgumentException("Year is invalid.");

        var budgets = await budgetRepository.GetBudgetsForReportAsync(userId, month, year);

        if (!budgets.Any())
            return new List<BudgetReportDto>();

        var categoryIds = budgets.Select(b => b.CategoryId).ToList();

        var expenseTotals = await budgetRepository.GetExpenseTotalsAsync(userId, categoryIds, month, year);

        var result = new List<BudgetReportDto>();

        foreach (var budget in budgets)
        {
            var spent = expenseTotals.ContainsKey(budget.CategoryId)
                ? expenseTotals[budget.CategoryId]
                : 0;

            var remaining = budget.Amount - spent;
            var percentage = budget.Amount > 0
                ? Math.Round((double)spent / budget.Amount * 100, 2)
                : 0;

            result.Add(new BudgetReportDto
            {
                CategoryName = budget.Category.CategoryName,
                BudgetAmount = budget.Amount,
                SpentAmount = (int)spent,
                RemainingAmount = (int)remaining,
                Percentage = percentage,
                IsOverBudget = spent > budget.Amount
            });
        }       

        return result;
    }
}