using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ShashiControllerAPI.DTOs;
using ShashiControllerAPI.Service;

namespace ShashiControllerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize]
public class IncomeController(IIncomeService incomeService) : ControllerBase
{
    // GET all incomes for logged in user
    //write a function for total income or credit
    [HttpGet]
    public async Task<ActionResult<List<GetIncomeDto>>> GetAllIncomes()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await incomeService.GetAllIncomesAsync(userId);
        return result.Count is 0 ? NotFound("No incomes found.") : Ok(result);
    }

    // GET income by id
    [HttpGet("{incomeId}")]
    public async Task<ActionResult<GetIncomeDto>> GetIncomeById(Guid incomeId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await incomeService.GetIncomeByIdAsync(incomeId, userId);
        return result is null ? NotFound("Income not found.") : Ok(result);
    }

    // POST add new income
    [HttpPost]
    public async Task<ActionResult<CreateIncomeDto>> AddIncome(CreateIncomeDto income)
    {
        if (income.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var added = await incomeService.AddIncomeAsync(income, userId);
            return Ok(added);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT update income
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateIncome(Guid id, UpdateIncomeDto income)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await incomeService.UpdateIncomeAsync(id, income, userId);
        return updated ? NoContent() : NotFound($"Income with ID {id} not found.");
    }

    // DELETE income
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteIncome(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await incomeService.DeleteIncomeAsync(id, userId);
        return deleted ? NoContent() : NotFound($"Income with ID {id} not found.");
    }
}