using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Repository;
using Microsoft.EntityFrameworkCore;

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
        if (income == null)
            throw new ArgumentNullException(nameof(income), "Income data is required.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var userExists = await incomeRepository.UserExistsAsync(userId);
        if (!userExists)
            throw new ArgumentException("User not found.");

        var incomeName = income.Name?.Trim();
        var incomeSource = income.Source?.Trim();
        var description = income.Description?.Trim();

        if (string.IsNullOrWhiteSpace(incomeName))
            throw new ArgumentException("Income name is required.");

        if (string.IsNullOrWhiteSpace(incomeSource))
            throw new ArgumentException("Income source is required.");

        if (income.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        if (income.Amount > 10000000)
            throw new ArgumentException("Amount is too large.");

        var today = DateOnly.FromDateTime(DateTime.Now);

        if (income.Date == default)
            throw new ArgumentException("Income date is required.");

        if (income.Date > today)
            throw new ArgumentException("Income date cannot be in the future.");

        if (income.Date < new DateOnly(1900, 1, 1))
            throw new ArgumentException("Income date is too old.");

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.");

        var duplicateExists = await incomeRepository.DuplicateIncomeExistsAsync(
            userId,
            incomeName,
            incomeSource,
            income.Amount,
            income.Date,
            description
        );

        if (duplicateExists)
            throw new ArgumentException("Duplicate income already exists.");

        var newIncome = new Income
        {
            UserId = userId,
            Name = incomeName,
            Source = incomeSource,
            Amount = income.Amount,
            Description = description,
            Date = income.Date
        };

        try
        {
            await incomeRepository.AddIncomeAsync(newIncome);
            await incomeRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new Exception("Database error occurred while saving income.");
        }
        catch (Exception)
        {
            throw new Exception("An unexpected error occurred while adding income.");
        }

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

    if (userId == Guid.Empty)
        throw new ArgumentException("Invalid user ID.");

    if (income == null)
        throw new ArgumentNullException(nameof(income), "Income data is required.");

    var existing = await incomeRepository.GetIncomeByIdEntityAsync(id, userId);

    if (existing is null)
        return false;

    if (string.IsNullOrWhiteSpace(income.Name))
        throw new ArgumentException("Income name is required.");

    var incomeName = income.Name.Trim();

    if (string.IsNullOrWhiteSpace(income.Source))
        throw new ArgumentException("Income source is required.");

    var incomeSource = income.Source.Trim();

    if (income.Amount <= 0)
        throw new ArgumentException("Amount must be greater than 0.");

    if (income.Amount > 10000000)
        throw new ArgumentException("Amount is too large.");

    var today = DateOnly.FromDateTime(DateTime.Now);

    if (income.Date == default)
        throw new ArgumentException("Income date is required.");

    if (income.Date > today)
        throw new ArgumentException("Income date cannot be in the future.");

    if (income.Date < new DateOnly(1900, 1, 1))
        throw new ArgumentException("Income date is too old.");

    var description = income.Description?.Trim();

    if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        throw new ArgumentException("Description cannot exceed 500 characters.");

    var duplicateExists = await incomeRepository.DuplicateIncomeExistsForUpdateAsync(
        id,
        userId,
        incomeName,
        incomeSource,
        income.Amount,
        income.Date,
        description
    );

    if (duplicateExists)
        throw new ArgumentException("Another income with same details already exists.");

    existing.Name = incomeName;
    existing.Source = incomeSource;
    existing.Amount = income.Amount;
    existing.Description = description;
    existing.Date = income.Date;

    try
    {
        await incomeRepository.SaveChangesAsync();
    }
    catch (DbUpdateException)
    {
        throw new Exception("Database error occurred while updating income.");
    }
    catch (Exception)
    {
        throw new Exception("An unexpected error occurred while updating income.");
    }

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

        try
        {
            await incomeRepository.DeleteIncomeAsync(income);
            await incomeRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new Exception("Database error occurred while deleting income.");
        }
        catch (Exception)
        {
            throw new Exception("An unexpected error occurred while deleting income.");
        }

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

        if (startDate == default)
            throw new ArgumentException("Start date is required.");

        if (endDate == default)
            throw new ArgumentException("End date is required.");

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