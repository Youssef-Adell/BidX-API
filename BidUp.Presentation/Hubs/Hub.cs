using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace BidUp.Presentation.Hubs;

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
            await NotifyUserIfHasUnreadMessages(userId);
            await NotifyChatParticipantsWithUserStatus(userId, isOnline: true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            await NotifyChatParticipantsWithUserStatus(userId, isOnline: false);
        }

        await base.OnDisconnectedAsync(exception);
    }


    [Authorize]
    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await chatsService.SendMessage(userId, request);

        if (!result.Succeeded)
        {
            await Clients.Caller.ErrorOccurred(result.Error!);
            return;
        }

        var createdMessage = result.Response!;
        var groupName = $"CHAT#{createdMessage.ChatId}";

        await Clients.Group(groupName).MessageReceived(createdMessage);
        await Clients.User($"{createdMessage.RecipientId}").MessageReceivedNotification();
    }

    // The client must call this method when the chat page loaded to be able to receive messages updates in realtime
    [Authorize]
    public async Task JoinChatRoom(JoinChatRoomRequest request)
    {
        var groupName = $"CHAT#{request.ChatId}";

        await Clients.Group(groupName).MessagesSeen(); // If the other user is currently in the chat page he will be notified that his messages seen
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    // The client must call this method when the chat page loaded is about to be closed to stop receiving unnecessary messages updates
    public async Task LeaveChatRoom(LeaveChatRoomRequest request)
    {
        var groupName = $"CHAT#{request.ChatId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }


    [Authorize]
    public async Task PlaceBid(BidRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await bidsService.PlaceBid(userId, request);

        if (!result.Succeeded)
        {
            await Clients.Caller.ErrorOccurred(result.Error!);
            return;
        }

        var createdBid = result.Response!;
        var auctionGroup = createdBid.AuctionId.ToString();

        await Clients.Group(auctionGroup).BidCreated(createdBid); // Notify clients who currently in the page of this auction
        await Clients.Group("FEED").AuctionPriceUpdated(new() { AuctionId = createdBid.AuctionId, NewPrice = createdBid.Amount });
    }

    [Authorize]
    public async Task AcceptBid(AcceptBidRequest request)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await bidsService.AcceptBid(userId, request);

        if (!result.Succeeded)
        {
            await Clients.Caller.ErrorOccurred(result.Error!);
            return;
        }

        var acceptedBid = result.Response!;
        var auctionGroup = acceptedBid.AuctionId.ToString();

        await Clients.Group(auctionGroup).BidAccepted(acceptedBid); // Notify clients who currently in the page of this auction
        await Clients.Group("FEED").AuctionEnded(new() { AuctionId = acceptedBid.AuctionId, FinalPrice = acceptedBid.Amount });
    }

    // The client must call this method when the auction page loads to be able to receive bidding updates in realtime
    public async Task JoinAuctionRoom(JoinAuctionRoomRequest request)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, request.AuctionId.ToString());
    }

    // The client must call this method when the auction page is about to be closed to stop receiving unnecessary bidding updates
    public async Task LeaveAuctionRoom(LeaveAuctionRoomRequest request)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, request.AuctionId.ToString());
    }


    // The client must call this method when the auctions feed page loads to be able to receive feed updates in realtime
    public async Task JoinFeedRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "FEED");
    }

    // The client must call this method when the auctions feed page is about to be closed to stop receiving unnecessary feed updates
    public async Task LeaveFeedRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "FEED");
    }


    private async Task NotifyUserIfHasUnreadMessages(int userId)
    {
        var hasUnreadMessages = await chatsService.HasUnreadMessages(userId);

        if (hasUnreadMessages)
            await Clients.Caller.MessageReceivedNotification();
    }

    private async Task NotifyChatParticipantsWithUserStatus(int userId, bool isOnline)
    {
        var chatIdsToNotify = await chatsService.ChangeUserStatus(userId, isOnline: true);

        var groupNames = chatIdsToNotify.Select(chatId => $"CHAT#{chatId}");

        await Clients.Groups(groupNames).UserStatusChanged(new() { UserId = userId, IsOnline = isOnline });
    }
}
