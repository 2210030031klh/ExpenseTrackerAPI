using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ShashiControllerAPI.Service;
namespace ShashiControllerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("Invalid token");

        if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            return Unauthorized("Invalid user id");

        var user = await userService.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound("User not found");

        return Ok(user);
    }
}