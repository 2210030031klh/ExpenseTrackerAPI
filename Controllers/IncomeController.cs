using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExpenseApi.DTOs;
using ExpenseApi.Service;

namespace ExpenseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncomeController(IIncomeService incomeService) : ControllerBase
{
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return Guid.Parse(userIdClaim);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetIncomeDto>>> GetAllIncomes()
    {
        var userId = GetUserId();
        var incomes = await incomeService.GetAllIncomesAsync(userId);
        return Ok(incomes);
    }

    // [HttpGet("{id:guid}")]
    // public async Task<ActionResult<GetIncomeDto>> GetIncomeById(Guid id)
    // {
    //     var userId = GetUserId();
    //     var income = await incomeService.GetIncomeByIdAsync(id, userId);

    //     if (income is null)
    //         return NotFound("Income not found");

    //     return Ok(income);
    // }

    [HttpPost]
    public async Task<ActionResult<CreateIncomeDto>> AddIncome(CreateIncomeDto income)
    {
        var userId = GetUserId();
        var created = await incomeService.AddIncomeAsync(income, userId);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateIncome(Guid id, UpdateIncomeDto income)
    {
        var userId = GetUserId();
        var updated = await incomeService.UpdateIncomeAsync(id, income, userId);

        if (!updated)
            return NotFound("Income not found");

        return Ok("Income updated successfully");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteIncome(Guid id)
    {
        var userId = GetUserId();
        var deleted = await incomeService.DeleteIncomeAsync(id, userId);

        if (!deleted)
            return NotFound("Income not found");

        return Ok("Income deleted successfully");
    }

    [HttpGet("source/{source}")]
    public async Task<ActionResult<List<GetIncomeDto>>> GetBySource(string source)
    {
        var userId = GetUserId();
        var incomes = await incomeService.GetIncomesBySourceAsync(source, userId);
        return Ok(incomes);
    }

    [HttpGet("ByDateRange")]
    public async Task<ActionResult<List<GetIncomeDto>>> GetByDateRange(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        var userId = GetUserId();

        if (startDate > endDate)
            return BadRequest("Start date cannot be greater than end date.");

        var incomes = await incomeService.GetIncomesByDateRangeAsync(startDate, endDate, userId);
        return Ok(incomes);
    }

    // [HttpGet("report/source")]
    // public async Task<ActionResult<List<GetIncomeSourceReportDto>>> GetSourceReport()
    // {
    //     var userId = GetUserId();
    //     var report = await incomeService.GetSourceReportAsync(userId);
    //     return Ok(report);
    // }

    [HttpGet("summary")]
    public async Task<ActionResult<IncomeSummaryDto>> GetIncomeSummary()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await incomeService.GetIncomeSummaryAsync(userId);
        return Ok(result);
    }
}