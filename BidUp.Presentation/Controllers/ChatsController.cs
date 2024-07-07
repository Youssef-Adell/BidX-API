using System.Security.Claims;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation;

[ApiController]
[Route("api/chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatService chatService;

    public ChatsController(IChatService chatService)
    {
        this.chatService = chatService;
    }

    [HttpPost("initiate/{receiverId}")]
    [Authorize]
    public async Task<IActionResult> IntiateChat(int receiverId)
    {
        var senderId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await chatService.IntiateChat(senderId, receiverId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }
}
