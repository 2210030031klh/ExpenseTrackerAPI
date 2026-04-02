using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Models;
using ShashiControllerAPI.Service;

namespace ShashiControllerAPI.Controllers;


[Route("api/[Controller]")]
[ApiController]
public class ExpenseController (IExpenseService expenseService): ControllerBase
{
    // 1️⃣ Get all expenses
    [HttpGet]
    // [Authorize]
    public async Task<ActionResult<List<GetExpenseDto>>> GetAllExpenses()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await expenseService.GetAllExpensesAsync(userId));
    }


    // 2️⃣ Get expense by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<GetExpenseDto>> GetExpensesById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //handle it.
        var result = await expenseService.GetExpensesByIdAsync(id, userId);
        return result is null ? NotFound("The expense with the specified ID was not found.") : Ok(result);      
    }
     // 3️⃣ Get expenses by category
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<GetExpenseDto>>> GetExpensesByCategory(string category)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetExpensesByCategoryAsync(category, userId);
        return result.Count is 0 ? NotFound($"No expenses found for category '{category}'.") : Ok(result);
    }

    // 4️⃣ Add a new expense
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

    // 5️⃣ Update an existing expense
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
    // 6️⃣ Delete an expense
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExpenseAsync(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await expenseService.DeleteExpenseAsync(id, userId);
        return deleted ? NoContent() : NotFound($"Expense with ID {id} not found.");
    }

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
        return result.Count is 0 ? NotFound("No expenses found for this date range.") : Ok(result);
    }
    [HttpGet("report/monthly")]
    // [Authorize]
    public async Task<ActionResult<List<MonthlyReportDto>>> GetMonthlyReport()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetMonthlyReportAsync(userId);
        return result.Count is 0 ? NotFound("No data found.") : Ok(result);
    }

    [HttpGet("report/category")]
    // [Authorize]
    public async Task<ActionResult<List<CategoryReportDto>>> GetCategoryReport()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await expenseService.GetCategoryReportAsync(userId);
        return result.Count is 0 ? NotFound("No data found.") : Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Accountant")]
    public async Task<ActionResult<List<GetExpenseDto>>> GetAllUsersExpenses()
    {
        var result = await expenseService.GetAllUsersExpensesAsync();
        return result.Count is 0 ? NotFound("No expenses found.") : Ok(result);
    }


}