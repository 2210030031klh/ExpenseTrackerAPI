using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Repository;

namespace ExpenseApi.Service;

public class IncomeService(IIncomeRepository incomeRepository) : IIncomeService
{
    public async Task<List<GetIncomeDto>> GetAllIncomesAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        return await incomeRepository.GetAllIncomesAsync(userId);
    }

    public async Task<GetIncomeDto?> GetIncomeByIdAsync(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid income ID.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        return await incomeRepository.GetIncomeByIdAsync(id, userId);
    }

    public async Task<CreateIncomeDto> AddIncomeAsync(CreateIncomeDto income, Guid userId)
    {
        if (income is null)
            throw new ArgumentNullException(nameof(income));

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var userExists = await incomeRepository.UserExistsAsync(userId);
        if (!userExists)
            throw new ArgumentException("User not found.");

        if (string.IsNullOrWhiteSpace(income.Name))
            throw new ArgumentException("Income name is required.");

        if (string.IsNullOrWhiteSpace(income.Source))
            throw new ArgumentException("Income source is required.");

        if (income.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        var newIncome = new Income
        {
            UserId = userId,
            Name = income.Name.Trim(),
            Amount = income.Amount,
            Description = string.IsNullOrWhiteSpace(income.Description) ? null : income.Description.Trim(),
            Source = income.Source.Trim(),
            Date = income.Date
        };

        await incomeRepository.AddIncomeAsync(newIncome);
        await incomeRepository.SaveChangesAsync();

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
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid income ID.");

        if (income is null)
            throw new ArgumentNullException(nameof(income));

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var existing = await incomeRepository.GetIncomeByIdEntityAsync(id, userId);

        if (existing is null)
            return false;

        if (string.IsNullOrWhiteSpace(income.Name))
            throw new ArgumentException("Income name is required.");

        if (string.IsNullOrWhiteSpace(income.Source))
            throw new ArgumentException("Income source is required.");

        if (income.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        if (existing.Name == income.Name.Trim() &&
            existing.Amount == income.Amount &&
            existing.Description == income.Description &&
            existing.Source == income.Source.Trim() &&
            existing.Date == income.Date)
            throw new ArgumentException("No changes detected.");

        existing.Name = income.Name.Trim();
        existing.Amount = income.Amount;
        existing.Description = string.IsNullOrWhiteSpace(income.Description) ? null : income.Description.Trim();
        existing.Source = income.Source.Trim();
        existing.Date = income.Date;

        await incomeRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIncomeAsync(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid income ID.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var income = await incomeRepository.GetIncomeByIdEntityAsync(id, userId);

        if (income is null)
            return false;

        await incomeRepository.DeleteIncomeAsync(income);
        await incomeRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<GetIncomeDto>> GetIncomesBySourceAsync(string source, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source is required.");

        source = source.Trim();

        return await incomeRepository.GetIncomesBySourceAsync(source, userId);
    }

    public async Task<List<GetIncomeDto>> GetIncomesByDateRangeAsync(DateOnly startDate, DateOnly endDate, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be greater than end date.");

        return await incomeRepository.GetIncomesByDateRangeAsync(startDate, endDate, userId);
    }

    public async Task<List<GetIncomeSourceReportDto>> GetSourceReportAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        return await incomeRepository.GetSourceReportAsync(userId);
    }

    public async Task<IncomeSummaryDto> GetIncomeSummaryAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var incomes = await incomeRepository.GetUserIncomesAsync(userId);

        var totalAmount = incomes.Sum(i => i.Amount);
        var totalTransactions = incomes.Count;

        var today = DateTime.Now;

        var thisMonthTransactions = incomes.Count(i =>
            i.Date.Month == today.Month && i.Date.Year == today.Year
        );

        return new IncomeSummaryDto
        {
            TotalAmount = totalAmount,
            TotalTransactions = totalTransactions,
            ThisMonthTransactions = thisMonthTransactions,
            AverageAmount = totalTransactions > 0
                ? (double)totalAmount / totalTransactions
                : 0
        };
    }
}