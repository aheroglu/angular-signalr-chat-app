using ChatAppServer.WebAPI.Contexts;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Models;
using GenericFileService.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(
    AppDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterDto request, CancellationToken cancellationToken)
    {
        bool isNameExists = await context.Users.AnyAsync(p => p.Name == request.Name);

        if (isNameExists)
        {
            return BadRequest(new { Message = "This user name already taken!" });
        }

        string avatar = FileService.FileSaveToServer(request.File, "wwwroot/avatar/");

        User user = new()
        {
            Name = request.Name,
            Avatar = avatar,
            Status = "Offline"
        };

        await context.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> Login(string name, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

        if (user is null) return BadRequest(new { Message = "User not found!" });

        user.Status = "Online";

        await context.SaveChangesAsync(cancellationToken);

        return Ok(user);
    }
}
