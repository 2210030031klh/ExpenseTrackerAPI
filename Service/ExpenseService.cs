namespace ExpenseApi.Service;
using Microsoft.EntityFrameworkCore;
using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Repository;
using ClosedXML.Excel;

public class ExpenseService(IExpenseRepository expenseRepository) : IExpenseService
{
    public async Task<PagedResultDto<GetExpenseDto>> GetAllExpensesAsync(Guid userId, int pageNumber, int pageSize)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (pageNumber <= 0)
            pageNumber = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var query = await expenseRepository.GetAllExpensesQueryAsync(userId);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Select(e => new GetExpenseDto
            {
                ExpenseId = e.ExpenseId,
                UserId = e.UserId,
                Name = e.Name,
                CategoryName = e.Category.CategoryName,
                Amount = e.Amount,
                Description = e.Description,
                Date = e.Date,
                CreatedAt = e.CreatedAt
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<GetExpenseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }

    public async Task<PagedResultDto<GetExpenseDto>> GetExpensesByCategoryAsync(string category, Guid userId, int pageNumber, int pageSize)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category is required.");

        if (pageNumber <= 0)
            pageNumber = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var query = await expenseRepository.GetExpensesByCategoryQueryAsync(userId, category);

        var totalRecords = await query.CountAsync();

        var items = await query
            .Select(e => new GetExpenseDto
            {
                ExpenseId = e.ExpenseId,
                UserId = e.UserId,
                Name = e.Name,
                CategoryName = e.Category.CategoryName,
                Amount = e.Amount,
                Description = e.Description,
                Date = e.Date,
                CreatedAt = e.CreatedAt
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<GetExpenseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
    }

    public async Task<CreateExpenseDto> AddExpenseAsync(CreateExpenseDto expense, Guid userId)
    {
        if (expense == null)
            throw new ArgumentNullException(nameof(expense), "Expense data is required.");

        var userExists = await expenseRepository.UserExistsAsync(userId);
        if (!userExists)
            throw new ArgumentException("User not found.");

        var categoryExists = await expenseRepository.CategoryExistsAsync(expense.CategoryId);
        if (!categoryExists)
            throw new ArgumentException("Category does not exist.");

        var expenseName = expense.Name?.Trim();

        if (string.IsNullOrWhiteSpace(expenseName))
            throw new ArgumentException("Expense name is required.");

        if (expense.Amount > 10000000)
            throw new ArgumentException("Amount is too large.");

        var today = DateOnly.FromDateTime(DateTime.Now);

        if (expense.Date == default)
            throw new ArgumentException("Expense date is required.");

        if (expense.Date > today)
            throw new ArgumentException("Expense date cannot be in the future.");

        if (expense.Date < new DateOnly(1900, 1, 1))
            throw new ArgumentException("Expense date is too old.");

        var description = expense.Description?.Trim();

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.");

        var duplicateExists = await expenseRepository.DuplicateExpenseExistsAsync(
            userId,
            expense.CategoryId,
            expense.Amount,
            expense.Date,
            expenseName,
            description
        );

        if (duplicateExists)
            throw new ArgumentException("Duplicate expense already exists.");

        var newExpense = new Expense
        {
            UserId = userId,
            Name = expenseName,
            CategoryId = expense.CategoryId,
            Amount = expense.Amount,
            Description = description,
            Date = expense.Date
        };

        try
        {
            await expenseRepository.AddExpenseAsync(newExpense);
            await expenseRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new Exception("Database error occurred while saving expense.");
        }
        catch (Exception)
        {
            throw new Exception("An unexpected error occurred while adding expense.");
        }

        return new CreateExpenseDto
        {
            CategoryId = newExpense.CategoryId,
            Name = newExpense.Name,
            Amount = newExpense.Amount,
            Description = newExpense.Description,
            Date = newExpense.Date
        };
    }

    public async Task<bool> UpdateExpenseAsync(Guid id, UpdateExpenseDto expense, Guid userId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid expense ID.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (expense == null)
            throw new ArgumentNullException(nameof(expense), "Expense data is required.");

        var existing = await expenseRepository.GetExpenseByIdEntityAsync(id, userId);

        if (existing is null)
            return false;

        if (string.IsNullOrWhiteSpace(expense.Name))
            throw new ArgumentException("Expense name is required.");

        var expenseName = expense.Name.Trim();

        if (expense.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        if (expense.Amount > 10000000)
            throw new ArgumentException("Amount is too large.");

        var today = DateOnly.FromDateTime(DateTime.Now);

        if (expense.Date == default)
            throw new ArgumentException("Expense date is required.");

        if (expense.Date > today)
            throw new ArgumentException("Expense date cannot be in the future.");

        if (expense.Date < new DateOnly(1900, 1, 1))
            throw new ArgumentException("Expense date is too old.");

        var description = expense.Description?.Trim();

        var categoryExists = await expenseRepository.CategoryExistsAsync(expense.CategoryId);

        if (!categoryExists)
            throw new ArgumentException("Category does not exist.");

        var duplicateExists = await expenseRepository.DuplicateExpenseExistsForUpdateAsync(
            id,
            userId,
            expense.CategoryId,
            expense.Amount,
            expense.Date,
            expenseName,
            description
        );

        if (duplicateExists)
            throw new ArgumentException("Another expense with same details already exists.");

        existing.CategoryId = expense.CategoryId;
        existing.Name = expenseName;
        existing.Amount = expense.Amount;
        existing.Description = description;
        existing.Date = expense.Date;

        try
        {
            await expenseRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new Exception("Database error occurred while updating expense.");
        }
        catch (Exception)
        {
            throw new Exception("An unexpected error occurred while updating expense.");
        }

        return true;
    }

    public async Task<bool> DeleteExpenseAsync(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid expense ID.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var expense = await expenseRepository.GetExpenseByIdEntityAsync(id, userId);

        if (expense is null)
            return false;

        try
        {
            await expenseRepository.DeleteExpenseAsync(expense);
            await expenseRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new Exception("Database error occurred while deleting expense.");
        }
        catch (Exception)
        {
            throw new Exception("An unexpected error occurred while deleting expense.");
        }

        return true;
    }

    public async Task<GetExpenseDto?> GetExpensesByIdAsync(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Invalid expense ID.");

        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        return await expenseRepository.GetExpenseByIdAsync(id, userId);
    }

    public async Task<List<GetExpenseDto>> GetExpensesByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (startDate == default)
            throw new ArgumentException("Start date is required.");

        if (endDate == default)
            throw new ArgumentException("End date is required.");

        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be greater than end date.");

        return await expenseRepository.GetExpensesByDateRangeAsync(userId, startDate, endDate);
    }

    public async Task<List<MonthlyReportDto>> GetMonthlyReportAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        return await expenseRepository.GetMonthlyReportAsync(userId);
    }

    public async Task<List<CategoryReportDto>> GetCategoryReportAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        return await expenseRepository.GetCategoryReportAsync(userId);
    }

    public async Task<List<GetExpenseDto>> GetAllUsersExpensesAsync()
    {
        return await expenseRepository.GetAllUsersExpensesAsync();
    }

    public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var expenses = await expenseRepository.GetUserExpensesAsync(userId);

        var totalAmount = expenses.Sum(e => e.Amount);
        var totalTransactions = expenses.Count;

        var today = DateTime.Now;
        var thisMonthTransactions = expenses.Count(e =>
            e.Date.Month == today.Month && e.Date.Year == today.Year
        );

        return new ExpenseSummaryDto
        {
            TotalAmount = totalAmount,
            TotalTransactions = totalTransactions,
            ThisMonthTransactions = thisMonthTransactions,
            AverageAmount = totalTransactions > 0
                ? (double)totalAmount / totalTransactions
                : 0
        };
    }

    public async Task<byte[]> ExportExpensesToExcelAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var expenses = await expenseRepository.GetUserExpensesForExportAsync(userId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Expenses");

        
        worksheet.Cell(1, 1).Value = "Name";
        worksheet.Cell(1, 2).Value = "Amount";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Date";
        

        
        for (int i = 0; i < expenses.Count; i++)
        {
            var e = expenses[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = e.Name;
            worksheet.Cell(row, 2).Value = e.Amount;
            worksheet.Cell(row, 3).Value = e.Description ?? "";
            worksheet.Cell(row, 4).Value = e.Date.ToString("yyyy-MM-dd");
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}