using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExpenseApi.DTOs;
using ExpenseApi.Service;

namespace ExpenseApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BudgetController(IBudgetService budgetService) : ControllerBase
{
    // GET all budgets for logged in user
    [HttpGet]
    public async Task<ActionResult<List<GetBudgetDto>>> GetAllBudgets()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await budgetService.GetAllBudgetsAsync(userId);
        return result.Count is 0 ? NotFound("No budgets found.") : Ok(result);
    }

    // POST add new budget
    [HttpPost]
    public async Task<ActionResult<CreateBudgetDto>> AddBudget(CreateBudgetDto budget)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var added = await budgetService.AddBudgetAsync(budget, userId);
            return Ok(added);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT update budget
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBudget(Guid id, CreateBudgetDto budget)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await budgetService.UpdateBudgetAsync(id, budget,userId);

        return updated ? NoContent() : NotFound($"Budget with ID {id} not found.");
    }

    // DELETE budget
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBudget(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await budgetService.DeleteBudgetAsync(id, userId);
        return deleted ? NoContent() : NotFound($"Budget with ID {id} not found.");
    }

    // budget page
    [HttpGet("report")]
    public async Task<ActionResult<List<BudgetReportDto>>> GetBudgetReport(
        [FromQuery] int month,
        [FromQuery] int year)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");
        if (year < 2000 || year > 2100)
            return BadRequest("Invalid year.");
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await budgetService.GetBudgetReportAsync(userId, month, year);
        return result.Count is 0 ? NotFound("No budget data found.") : Ok(result);
    }
}