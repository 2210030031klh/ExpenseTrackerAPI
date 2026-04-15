using Microsoft.EntityFrameworkCore;
using ExpenseApi.Data;
using ExpenseApi.DTOs;
using ExpenseApi.Models;

namespace ExpenseApi.Repository;

public class IncomeRepository(AppDbContext context) : IIncomeRepository
{
    public async Task<bool> UserExistsAsync(Guid userId)
    {
        return await context.Users.AnyAsync(u => u.UserId == userId);
    }

    public async Task<List<GetIncomeDto>> GetAllIncomesAsync(Guid userId)
    {
        return await context.Incomes
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .Select(i => new GetIncomeDto
            {
                IncomeId = i.IncomeId,
                UserId = i.UserId,
                Name = i.Name,
                Amount = i.Amount,
                Description = i.Description,
                Source = i.Source,
                Date = i.Date,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<GetIncomeDto?> GetIncomeByIdAsync(Guid id, Guid userId)
    {
        return await context.Incomes
            .AsNoTracking()
            .Where(i => i.IncomeId == id && i.UserId == userId)
            .Select(i => new GetIncomeDto
            {
                IncomeId = i.IncomeId,
                UserId = i.UserId,
                Name = i.Name,
                Amount = i.Amount,
                Description = i.Description,
                Source = i.Source,
                Date = i.Date,
                CreatedAt = i.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Income?> GetIncomeByIdEntityAsync(Guid id, Guid userId)
    {
        return await context.Incomes
            .FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);
    }

    public async Task AddIncomeAsync(Income income)
    {
        await context.Incomes.AddAsync(income);
    }

    public Task DeleteIncomeAsync(Income income)
    {
        context.Incomes.Remove(income);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task<List<GetIncomeDto>> GetIncomesBySourceAsync(string source, Guid userId)
    {
        return await context.Incomes
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Source == source)
            .Select(i => new GetIncomeDto
            {
                IncomeId = i.IncomeId,
                UserId = i.UserId,
                Name = i.Name,
                Amount = i.Amount,
                Description = i.Description,
                Source = i.Source,
                Date = i.Date,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<GetIncomeDto>> GetIncomesByDateRangeAsync(DateOnly startDate, DateOnly endDate, Guid userId)
    {
        return await context.Incomes
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
            .Select(i => new GetIncomeDto
            {
                IncomeId = i.IncomeId,
                UserId = i.UserId,
                Name = i.Name,
                Amount = i.Amount,
                Description = i.Description,
                Source = i.Source,
                Date = i.Date,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<GetIncomeSourceReportDto>> GetSourceReportAsync(Guid userId)
    {
        return await context.Incomes
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .GroupBy(i => i.Source)
            .Select(g => new GetIncomeSourceReportDto
            {
                Source = g.Key,
                TotalAmount = g.Sum(i => i.Amount)
            })
            .ToListAsync();
    }

    public async Task<List<Income>> GetUserIncomesAsync(Guid userId)
    {
        return await context.Incomes
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .ToListAsync();
    }
}