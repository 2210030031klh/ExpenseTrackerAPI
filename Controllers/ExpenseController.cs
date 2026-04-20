using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseApi.DTOs;
using ExpenseApi.Models;
using ExpenseApi.Service;
using System.Data;
using System.Text;

namespace ExpenseApi.Controllers;


[Route("api/[Controller]")]
[ApiController]
[Authorize]
public class ExpenseController (IExpenseService expenseService): ControllerBase
{
    
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<GetExpenseDto>>> GetAllExpenses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetAllExpensesAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<PagedResultDto<GetExpenseDto>>> GetExpensesByCategory(
        string category,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await expenseService.GetExpensesByCategoryAsync(
            category,
            userId,
            pageNumber,
            pageSize
        );

        return Ok(result);
    }

        
    [HttpPost]
    public async Task<ActionResult<Expense>> AddExpense(CreateExpenseDto expense)
    {
        if (expense.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than 0." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try{
            var added = await expenseService.AddExpenseAsync(expense, userId);
            return Ok(added);
        }
        catch(ArgumentException ex)
        {
            return BadRequest(new{message=ex.Message});
        }
    }

    
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateExpenseAsync(Guid id, UpdateExpenseDto expense)
    {
        if (string.IsNullOrWhiteSpace(expense.Name) || expense.Name.Length < 2)
            return BadRequest(new { message = "Expense name must be at least 2 characters long." });
        if (expense.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than 0." });
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var updated = await expenseService.UpdateExpenseAsync(id, expense, userId);
            if (!updated)
                return NotFound(new { message = $"Expense with ID {id} not found." });
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExpenseAsync(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await expenseService.DeleteExpenseAsync(id, userId);
        return deleted ? NoContent() : NotFound($"Expense with ID {id} not found.");
    }

    //do handle it in expense page
    [HttpGet("ByDateRange")]
    // [Authorize]
    public async Task<ActionResult<List<GetExpenseDto>>> GetExpensesByDateRange
    (
        [FromQuery] DateOnly startDate, 
        [FromQuery] DateOnly endDate)
    {
        if (startDate > endDate)
            return BadRequest("Start date cannot be greater than end date.");
        
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetExpensesByDateRangeAsync(userId, startDate, endDate);
        return Ok(result);
    }
    //dashboard
    [HttpGet("report/monthly")]
    public async Task<ActionResult<List<MonthlyReportDto>>> GetMonthlyReport()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetMonthlyReportAsync(userId);
        return Ok(result);
    }
    //dashboard
    [HttpGet("report/category")]
    public async Task<ActionResult<List<CategoryReportDto>>> GetCategoryReport()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetCategoryReportAsync(userId);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Accountant")]
    public async Task<ActionResult<List<GetExpenseDto>>> GetAllUsersExpenses()
    {
        var result = await expenseService.GetAllUsersExpensesAsync();
        return result.Count is 0 ? NotFound("No expenses found.") : Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryDto>> GetExpenseSummary()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetExpenseSummaryAsync(userId);
        return Ok(result);
    }

   [HttpGet("export")]
public async Task<IActionResult> Export(string format = "xlsx")
{
    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    if (format.ToLower() == "csv")
    {
        var expenses = await expenseService.GetExpensesForExportAsync(userId);

        var sb = new StringBuilder();
        sb.AppendLine("\"Name\",\"Amount\",\"Description\",\"Date\"");

        foreach (var e in expenses)
        {
            sb.AppendLine($"\"{e.Name}\",\"{e.Amount}\",\"{e.Description ?? ""}\",\"{e.Date:yyyy-MM-dd}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return File(bytes, "text/csv", "Expenses.csv");
    }

    var fileContents = await expenseService.ExportExpensesToExcelAsync(userId);

    return File(
        fileContents,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "Expenses.xlsx"
    );
}
}