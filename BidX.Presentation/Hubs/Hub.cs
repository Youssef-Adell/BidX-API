using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace BidX.Presentation.Hubs;

public class Hub : Hub<IHubClient>
{
    private readonly IBidsService bidsService;
    private readonly IChatsService chatsService;

    public Hub(IBidsService bidsService, IChatsService chatsService)
    {
        this.bidsService = bidsService;
        this.chatsService = chatsService;
    }

    public override async Task OnConnectedAsync()
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            await chatsService.NotifyUserWithUnreadChatsCount(userId);
            await chatsService.NotifyParticipantsWithUserStatus(userId, isOnline: true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            await chatsService.NotifyParticipantsWithUserStatus(userId, isOnline: false);
        }

        await base.OnDisconnectedAsync(exception);
    }


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


    [Authorize]
    public async Task PlaceBid(BidRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        await bidsService.PlaceBid(userId, request);
    }

    [Authorize]
    public async Task AcceptBid(AcceptBidRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        await bidsService.AcceptBid(userId, request);
    }

    /// <summary>
    /// The client must call this method when the auction page loads to be able to receive bidding updates in realtime
    /// </summary>
    public async Task JoinAuctionRoom(JoinAuctionRoomRequest request)
    {
        var groupName = $"AUCTION#{request.AuctionId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// The client must call this method when the auction page is about to be closed to stop receiving unnecessary bidding updates
    /// </summary>
    public async Task LeaveAuctionRoom(LeaveAuctionRoomRequest request)
    {
        var groupName = $"AUCTION#{request.AuctionId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }


    /// <summary>
    /// The client must call this method when the auctions feed page loads to be able to receive feed updates in realtime
    /// </summary>
    public async Task JoinFeedRoom()
    {
        var groupName = "FEED";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// The client must call this method when the auctions feed page is about to be closed to stop receiving unnecessary feed updates
    /// </summary>
    public async Task LeaveFeedRoom()
    {
        var groupName = "FEED";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
