using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation;

[ApiController]
[Route("api/chats")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ChatsController : ControllerBase
{
    private readonly IChatService chatService;

    public ChatsController(IChatService chatService)
    {
        this.chatService = chatService;
    }


    /// <summary>
    /// Gets the chats of the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Page<ChatDetailsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserChats([FromQuery] ChatsQueryParams queryParams)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var response = await chatService.GetUserChats(userId, queryParams);

        return Ok(response);
    }


    [HttpGet("{chatId}/messages")]
    [ProducesResponseType(typeof(Page<MessageResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatMessages(int chatId, [FromQuery] MessagesQueryParams queryParams)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var response = await chatService.GetChatMessages(userId, chatId, queryParams);

        return Ok(response);
    }


    /// <summary>
    /// Creates a new chat with a user or retrieves old chat if it exists
    /// </summary>
    [HttpPost("initiate/{receiverId}")]
    [ProducesResponseType(typeof(ChatSummeryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IntiateChat(int receiverId)
    {
        var senderId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await chatService.IntiateChat(senderId, receiverId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }
}
