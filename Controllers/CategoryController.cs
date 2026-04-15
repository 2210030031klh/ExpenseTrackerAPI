using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExpenseApi.DTOs;
using ExpenseApi.Service;

namespace ExpenseApi.Controllers;

[Route("api/[controller]")] 
[ApiController]
[Authorize]
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
        // GET all categories for logged in user (system + personal)
    [HttpGet]
    public async Task<ActionResult<List<GetCategoryDto>>> GetCategories([FromQuery] string? type)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid user.");

        var categories = await categoryService.GetCategoriesAsync(userId, type);
        return Ok(categories);
    }

    // POST create personal category
    [HttpPost]
    public async Task<ActionResult<GetCategoryDto>> CreateCategory(CreateCategoryDto dto )
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await categoryService.CreateCategoryAsync(dto, userId);
        return Ok(result);
    }

    // DELETE personal category
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await categoryService.DeleteCategoryAsync(id, userId);
        return deleted ? NoContent() : NotFound("Category not found or not yours.");
    }
}