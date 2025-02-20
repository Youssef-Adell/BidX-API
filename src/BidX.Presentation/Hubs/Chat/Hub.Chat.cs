using BidX.BusinessLogic.DTOs.ChatDTOs;
using Microsoft.AspNetCore.Authorization;

namespace BidX.Presentation.Hubs;

public partial class Hub
{
    [Authorize]
    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        await chatsService.SendMessage(userId, request);
    }

    [Authorize]
    public async Task MarkMessageAsRead(MarkMessageAsReadRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        await chatsService.MarkMessageAsRead(userId, request);
    }

    [Authorize]
    public async Task MarkAllMessagesAsRead(MarkAllMessagesAsReadRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        await chatsService.MarkAllMessagesAsRead(userId, request);
    }

    /// <summary>
    /// The client must call this method when the chat page loaded to be able to receive messages updates in realtime
    /// </summary>
    [Authorize]
    public async Task JoinChatRoom(JoinChatRoomRequest request)
    {
        var groupName = $"CHAT#{request.ChatId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// The client must call this method when the chat page loaded is about to be closed to stop receiving unnecessary messages updates
    /// </summary>
    public async Task LeaveChatRoom(LeaveChatRoomRequest request)
    {
        var groupName = $"CHAT#{request.ChatId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
