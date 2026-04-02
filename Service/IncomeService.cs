using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;

namespace ShashiControllerAPI.Service;

public class IncomeService(AppDbContext context) : IIncomeService
{
    public async Task<List<GetIncomeDto>> GetAllIncomesAsync(Guid userId)
        => await context.Incomes
            .Where(i => i.UserId == userId)
            .Select(i => new GetIncomeDto
            {
                IncomeId = i.IncomeId,
                UserId = i.UserId,
                Name=i.Name,
                Amount = i.Amount,
                Description = i.Description,
                Source = i.Source,
                Date = i.Date,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

    public async Task<GetIncomeDto?> GetIncomeByIdAsync(Guid id, Guid userId)
        => await context.Incomes
            .Where(i => i.IncomeId == id&& i.UserId == userId)
            .Select(i => new GetIncomeDto
            {
                IncomeId = i.IncomeId,
                UserId = i.UserId,
                Amount = i.Amount,
                Description = i.Description,
                Source = i.Source,
                Date = i.Date,
                CreatedAt = i.CreatedAt
            })
            .FirstOrDefaultAsync();

    public async Task<CreateIncomeDto> AddIncomeAsync(CreateIncomeDto income, Guid userId)
    {
        var userExists = await context.Users.AnyAsync(u => u.UserId == userId);
        if (!userExists)
            throw new ArgumentException("User not found.");
            
        if (income.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");
                // Check if user exist

        var newIncome = new Income
        {
            UserId = userId,
            Amount = income.Amount,
            Description = income.Description,
            Name = income.Name,
            Source = income.Source,
            Date = income.Date
        };  

        context.Incomes.Add(newIncome);
        await context.SaveChangesAsync();

        return new CreateIncomeDto
        {
            Name = newIncome.Name,
            Source = newIncome.Source,
            Amount = newIncome.Amount,
            Description = newIncome.Description,
            Date = newIncome.Date
        };
    }

    public async Task<bool> UpdateIncomeAsync(Guid id, UpdateIncomeDto income, Guid userId)
    {
        var existing = await context.Incomes.FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);
        if (existing is null)
            return false;

        existing.Amount = income.Amount;
        existing.Description = income.Description;
        existing.Source = income.Source;
        existing.Date = income.Date;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIncomeAsync(Guid id, Guid userId)
    {
        var income = await context.Incomes.FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);
        if (income is null)
            return false;

        context.Incomes.Remove(income);
        await context.SaveChangesAsync();
        return true;
    }
}