using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace BidUp.Presentation.Hubs;

public class AppHub : Hub<IAppHubClient>
{
    private readonly IBiddingService biddingService;
    private readonly IChatService chatService;

    public AppHub(IBiddingService biddingService, IChatService chatService)
    {
        this.biddingService = biddingService;
        this.chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            var hasUnseenMessages = await chatService.HasUnseenMessages(userId);

            if (hasUnseenMessages)
                await Clients.Caller.MessageReceivedNotification();

            var chatIdsToNotify = await chatService.ChangeUserStatus(userId, isOnline: true);

            var groupNames = chatIdsToNotify.Select(chatId => $"CHAT#{chatId}");

            await Clients.Groups(groupNames).UserStatusChanged(new() { UserId = userId, IsOnline = true });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (int.TryParse(Context.UserIdentifier, out int userId))
        {
            var chatIdsToNotify = await chatService.ChangeUserStatus(userId, isOnline: false);

            var groupNames = chatIdsToNotify.Select(chatId => $"CHAT#{chatId}");

            await Clients.Groups(groupNames).UserStatusChanged(new() { UserId = userId, IsOnline = false });
        }

        await base.OnDisconnectedAsync(exception);
    }


    [Authorize]
    public async Task SendMessage(MessageRequest messageRequest)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await chatService.SendMessage(userId, messageRequest);

        if (!result.Succeeded)
        {
            await Clients.Caller.ErrorOccurred(result.Error!);
            return;
        }

        var createdMessage = result.Response!;
        var groupName = $"CHAT#{createdMessage.ChatId}";

        await Clients.Group(groupName).MessageReceived(createdMessage);
        await Clients.User($"{createdMessage.ReceiverId}").MessageReceivedNotification();
    }

    // The client must call this method when the chat page loaded to be able to receive messages updates in realtime
    [Authorize]
    public async Task JoinChatRoom(JoinChatRoomRequest joinChatRoomRequest)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await chatService.MarkReceivedMessagesAsSeen(userId, joinChatRoomRequest.ChatId);

        if (!result.Succeeded)
        {
            await Clients.Caller.ErrorOccurred(result.Error!);
            return;
        }

        var groupName = $"CHAT#{joinChatRoomRequest.ChatId}";

        await Clients.Group(groupName).MessagesSeen(); // If the other user is currently in the chat page he will be notified that his messages seen
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    // The client must call this method when the chat page loaded is about to be closed to stop receiving unnecessary messages updates
    public async Task LeaveChatRoom(LeaveChatRoomRequest leaveChatRoomRequest)
    {
        var groupName = $"CHAT#{leaveChatRoomRequest.ChatId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }


    [Authorize]
    public async Task BidUp(BidRequest bidRequest)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await biddingService.BidUp(userId, bidRequest);

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
    public async Task AcceptBid(AcceptBidRequest acceptBidRequest)
    {
        var userId = int.Parse(Context.UserIdentifier!);

        var result = await biddingService.AcceptBid(userId, acceptBidRequest);

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
    public async Task JoinAuctionRoom(JoinAuctionRoomRequest joinAuctionRoomRequest)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, joinAuctionRoomRequest.AuctionId.ToString());
    }

    // The client must call this method when the auction page is about to be closed to stop receiving unnecessary bidding updates
    public async Task LeaveAuctionRoom(LeaveAuctionRoomRequest leaveAuctionRoomRequest)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, leaveAuctionRoomRequest.AuctionId.ToString());
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
}
