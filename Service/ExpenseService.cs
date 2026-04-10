namespace ShashiControllerAPI.Service;
using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.Data;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;

public class ExpenseService (AppDbContext context): IExpenseService
{
    //check if which user is logged in and then return the expenses of that user only
    public async Task<PagedResultDto<GetExpenseDto>> GetAllExpensesAsync(Guid userId, int pageNumber, int pageSize)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (pageNumber <= 0)
            pageNumber = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var query = context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
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
            });

        var totalRecords = await query.CountAsync();

        var items = await query
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

        var query = context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Category.CategoryName == category)
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
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
            });

        var totalRecords = await query.CountAsync();

        var items = await query
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

        var userExists = await context.Users.AnyAsync(u => u.UserId == userId);
        if (!userExists)
            throw new ArgumentException("User not found.");

        var categoryExists = await context.Categories.AnyAsync(c => c.CategoryId == expense.CategoryId);
        if (!categoryExists)
            throw new ArgumentException("Category does not exist.");
        
        var expenseName = expense.Name?.Trim();

        if (string.IsNullOrWhiteSpace(expenseName))
            throw new ArgumentException("Expense name is required.");

        if (expenseName.Length < 2)
            throw new ArgumentException("Expense name must be at least 2 characters long.");

        if (expenseName.Length > 100)
            throw new ArgumentException("Expense name cannot exceed 100 characters.");

        if (expense.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0.");

        if (expense.Amount > 10000000)
            throw new ArgumentException("Amount is too large.");


        var today = DateOnly.FromDateTime(DateTime.Now);

        if (expense.Date == default)
            throw new ArgumentException("Expense date is required.");

        if (expense.Date > today)
            throw new ArgumentException("Expense date cannot be in the future.");

        if (expense.Date < new DateOnly(2000, 1, 1))
            throw new ArgumentException("Expense date is too old.");

        var description = expense.Description?.Trim();

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.");

        var duplicateExists = await context.Expenses.AnyAsync(e =>
            e.UserId == userId &&
            e.CategoryId == expense.CategoryId &&
            e.Amount == expense.Amount &&
            e.Date == expense.Date &&
            e.Name.ToLower() == expenseName.ToLower() &&
            (e.Description ?? "") == (description ?? "")
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
            context.Expenses.Add(newExpense);
            await context.SaveChangesAsync();
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

    var existing = await context.Expenses
        .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

    if (existing is null)
        return false;

    // Name validation
    if (string.IsNullOrWhiteSpace(expense.Name))
        throw new ArgumentException("Expense name is required.");

    var expenseName = expense.Name.Trim();

    if (expenseName.Length < 2)
        throw new ArgumentException("Expense name must be at least 2 characters.");

    if (expenseName.Length > 100)
        throw new ArgumentException("Expense name cannot exceed 100 characters.");

    // Amount validation
    if (expense.Amount <= 0)
        throw new ArgumentException("Amount must be greater than 0.");

    if (expense.Amount > 10000000)
        throw new ArgumentException("Amount is too large.");

    // Date validation
    var today = DateOnly.FromDateTime(DateTime.Now);

    if (expense.Date == default)
        throw new ArgumentException("Expense date is required.");

    if (expense.Date > today)
        throw new ArgumentException("Expense date cannot be in the future.");

    if (expense.Date < new DateOnly(2000, 1, 1))
        throw new ArgumentException("Expense date is too old.");

    // Description validation
    var description = expense.Description?.Trim();

    if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        throw new ArgumentException("Description cannot exceed 500 characters.");

    // Category validation
    var categoryExists = await context.Categories
        .AnyAsync(c => c.CategoryId == expense.CategoryId);

    if (!categoryExists)
        throw new ArgumentException("Category does not exist.");

    // Duplicate check
    var duplicateExists = await context.Expenses.AnyAsync(e =>
        e.ExpenseId != id &&
        e.UserId == userId &&
        e.CategoryId == expense.CategoryId &&
        e.Amount == expense.Amount &&
        e.Date == expense.Date &&
        e.Name == expenseName &&
        (e.Description ?? "") == (description ?? "")
    );

    if (duplicateExists)
        throw new ArgumentException("Another expense with same details already exists.");

    // Update values
    existing.CategoryId = expense.CategoryId;
    existing.Name = expenseName;
    existing.Amount = expense.Amount;
    existing.Description = description;
    existing.Date = expense.Date;

    try
    {
        await context.SaveChangesAsync();
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

        var expense = await context.Expenses
            .FirstOrDefaultAsync(e => e.ExpenseId == id && e.UserId == userId);

        if (expense is null)
            return false;

        try
        {
            context.Expenses.Remove(expense);
            await context.SaveChangesAsync();
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
        
        var result = await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.ExpenseId == id && e.UserId == userId)
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
            .FirstOrDefaultAsync();
        return result;
    }



    public async Task<List<GetExpenseDto>> GetExpensesByDateRangeAsync(
        Guid userId, DateOnly startDate, DateOnly endDate)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        if (startDate == default)
            throw new ArgumentException("Start date is required.");

        if (endDate == default)
            throw new ArgumentException("End date is required.");

        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be greater than end date.");

        var result = await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId &&
                        e.Date >= startDate &&
                        e.Date <= endDate)
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
            .ToListAsync();

        return result;
    }

    public async Task<List<MonthlyReportDto>> GetMonthlyReportAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");
        var result = await context.Expenses
        .Where(e => e.UserId == userId)
        .GroupBy(e => new { e.Date.Year, e.Date.Month })
        .Select(g => new MonthlyReportDto
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            TotalAmount = g.Sum(e => e.Amount),
            TotalExpenses = g.Count()
        })
        .OrderBy(r => r.Year)
        .ThenBy(r => r.Month)
        .ToListAsync();

        return result;
    }

    public async Task<List<CategoryReportDto>> GetCategoryReportAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user ID.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await context.Expenses
            .Include(e => e.Category)
            .Where(e => e.UserId == userId &&
                        e.Date.Year == today.Year &&
                        e.Date.Month == today.Month)
            .GroupBy(e => e.Category.CategoryName)
            .Select(g => new CategoryReportDto
            {
                CategoryName = g.Key,
                TotalAmount = g.Sum(e => e.Amount),
                TotalExpenses = g.Count()
            })
            .OrderByDescending(r => r.TotalAmount)
            .ToListAsync();

        return result;
    }
    public async Task<List<GetExpenseDto>> GetAllUsersExpensesAsync()
{    var result = await context.Expenses
        .Include(e => e.Category)
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
        .ToListAsync();

        return result;
    }

    public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(Guid userId)
{
    if (userId == Guid.Empty)
        throw new ArgumentException("Invalid user ID.");

    var expenses = await context.Expenses
        .Where(e => e.UserId == userId)
        .ToListAsync();

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
}