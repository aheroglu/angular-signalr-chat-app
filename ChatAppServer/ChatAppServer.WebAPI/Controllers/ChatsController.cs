using ChatAppServer.WebAPI.Contexts;
using ChatAppServer.WebAPI.Dtos;
using ChatAppServer.WebAPI.Hubs;
using ChatAppServer.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public sealed class ChatsController(
    AppDbContext context, IHubContext<ChatHub> hubContext) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        List<User> users = await context.Users.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet]
    public async Task<IActionResult> GetChats(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken)
    {
        List<Chat> chats = await context.Chats
            .Where(p =>
                   p.FromUserId == fromUserId &&
                   p.ToUserId == toUserId ||
                   p.ToUserId == fromUserId &&
                   p.FromUserId == toUserId)
            .OrderBy(p => p.Date)
            .ToListAsync(cancellationToken);

        return Ok(chats);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(SendMessageDto request, CancellationToken cancellationToken)
    {
        Chat chat = new()
        {
            FromUserId = request.FromUserId,
            ToUserId = request.ToUserId,
            Message = request.Message,
            Date = DateTime.Now
        };

        await context.AddAsync(chat, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        string connetionId = ChatHub.Users.First(p => p.Value == chat.ToUserId).Key;

        await hubContext.Clients.Client(connetionId).SendAsync("Messages", chat);

        return Ok(chat);
    }
}
