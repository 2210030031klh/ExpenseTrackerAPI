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
            throw new Exception("User ID not found in token.");

        return Guid.Parse(userIdClaim);
    }

    [HttpGet]
    public async Task<ActionResult<List<GetIncomeDto>>> GetAllIncomes()
    {
        try
        {
            var userId = GetUserId();
            var incomes = await incomeService.GetAllIncomesAsync(userId);
            return Ok(incomes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CreateIncomeDto>> AddIncome(CreateIncomeDto income)
    {
        try
        {
            var userId = GetUserId();
            var created = await incomeService.AddIncomeAsync(income, userId);

            return Ok(new
            {
                message = "Income added successfully",
                data = created
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateIncome(Guid id, UpdateIncomeDto income)
    {
        try
        {
            var userId = GetUserId();
            var updated = await incomeService.UpdateIncomeAsync(id, income, userId);

            if (!updated)
                return NotFound(new { message = "Income not found" });

            return Ok(new { message = "Income updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteIncome(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await incomeService.DeleteIncomeAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Income not found" });

            return Ok(new { message = "Income deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("source/{source}")]
    public async Task<ActionResult<List<GetIncomeDto>>> GetBySource(string source)
    {
        try
        {
            var userId = GetUserId();
            var incomes = await incomeService.GetIncomesBySourceAsync(source, userId);
            return Ok(incomes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("ByDateRange")]
    public async Task<ActionResult<List<GetIncomeDto>>> GetByDateRange(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        try
        {
            var userId = GetUserId();
            var incomes = await incomeService.GetIncomesByDateRangeAsync(startDate, endDate, userId);
            return Ok(incomes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IncomeSummaryDto>> GetIncomeSummary()
    {
        try
        {
            var userId = GetUserId();
            var result = await incomeService.GetIncomeSummaryAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}