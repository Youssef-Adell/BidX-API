using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.Presentation.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidX.Presentation;

[ApiController]
[Route("api/chats")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class ChatsController : ControllerBase
{
    private readonly IChatsService chatsService;

    public ChatsController(IChatsService chatsService)
    {
        this.chatsService = chatsService;
    }


    /// <summary>
    /// Gets the chats of the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Page<ChatDetailsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserChats([FromQuery] ChatsQueryParams queryParams)
    {
        var userId = User.GetUserId();

        var response = await chatsService.GetUserChats(userId, queryParams);

        return Ok(response);
    }


    /// <summary>
    /// Creates a chat or retrieves it if exists
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatSummeryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateChatOrGetIfExist(CreateChatRequest request)
    {
        var senderId = User.GetUserId();

        var result = await chatsService.CreateChatOrGetIfExist(senderId, request);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return CreatedAtAction(nameof(GetChat), new { chatId = result.Response!.Id }, result.Response);
    }


    [HttpGet("{chatId}")]
    [ProducesResponseType(typeof(ChatSummeryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChat(int chatId)
    {
        var userId = User.GetUserId();

        var result = await chatsService.GetChat(userId, chatId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpGet("{chatId}/messages")]
    [ProducesResponseType(typeof(Page<MessageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatMessages(int chatId, [FromQuery] MessagesQueryParams queryParams)
    {
        var userId = User.GetUserId();

        var result = await chatsService.GetChatMessages(userId, chatId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }
}
