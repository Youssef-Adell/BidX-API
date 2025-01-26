using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BidX.Presentation.Hubs;

public class SignalrRealTimeService : IRealTimeService
{
    private IHubContext<Hub, IHubClient> hubContext;

    public SignalrRealTimeService(IHubContext<Hub, IHubClient> hubContext)
    {
        this.hubContext = hubContext;
    }

    #region Chat
    public async Task SendMessageToChat(int chatId, MessageResponse message)
    {
        var groupName = $"CHAT#{chatId}";
        await hubContext.Clients
            .Group(groupName)
            .MessageReceived(message);
    }

    public async Task MarkAllMessagesAsRead(int chatId, int readerId)
    {
        var groupName = $"CHAT#{chatId}";
        await hubContext.Clients
            .Group(groupName)
            .AllMessagesRead(new() { ReaderId = readerId });
    }

    public async Task MarkMessageAsRead(int chatId, int messageId)
    {
        var groupName = $"CHAT#{chatId}";
        await hubContext.Clients
            .Group(groupName)
            .MessageRead(new() { MessageId = messageId });
    }

    public async Task NotifyUserWithUnreadChatsCount(int userId, int unreadChatsCount)
    {
        await hubContext.Clients
            .User($"{userId}")
            .UnreadChatsCountChanged(new() { UnreadChatsCount = unreadChatsCount });
    }

    public async Task NotifyParticipantsWithUserStatus(IEnumerable<int> chatsIds, int userId, bool isOnline)
    {
        var chatIdsToNotify = chatsIds.Select(id => $"CHAT#{id}");
        await hubContext.Clients.Groups(chatIdsToNotify)
            .UserStatusChanged(new()
            {
                UserId = userId,
                IsOnline = isOnline
            });
    }
    #endregion


    #region Auction
    public async Task SendPlacedBidToAuctionRoom(int auctionId, BidResponse bid)
    {
        var groupName = $"AUCTION#{auctionId}";
        await hubContext.Clients
            .Group(groupName)
            .BidPlaced(bid);
    }

    public async Task SendAcceptedBidToAuctionRoom(int auctionId, BidResponse bid)
    {
        var groupName = $"AUCTION#{auctionId}";
        await hubContext.Clients
            .Group(groupName)
            .BidAccepted(bid);
    }

    public async Task SendAuctionToFeed(AuctionResponse auction)
    {
        var groupName = "FEED";
        await hubContext.Clients
            .Group(groupName)
            .AuctionCreated(auction);
    }

    public async Task DeleteAuctionFromFeed(int auctionId)
    {
        var groupName = "FEED";
        await hubContext.Clients
            .Group(groupName)
            .AuctionDeleted(new() { AuctionId = auctionId });
    }

    public async Task UpdateAuctionPriceInFeed(int auctionId, decimal newPrice)
    {
        var groupName = "FEED";
        await hubContext.Clients
            .Group(groupName)
            .AuctionPriceUpdated(new()
            {
                AuctionId = auctionId,
                NewPrice = newPrice
            });
    }

    public async Task MarkAuctionAsEndedInFeed(int auctionId, decimal finalPrice)
    {
        var groupName = "FEED";
        await hubContext.Clients
            .Group(groupName)
            .AuctionEnded(new()
            {
                AuctionId = auctionId,
                FinalPrice = finalPrice
            });
    }
    #endregion


    #region Common
    public async Task SendErrorToUser(int userId, ErrorResponse error)
    {
        await hubContext.Clients
            .User($"{userId}")
            .ErrorOccurred(error);
    }

    public async Task SendErrorToUser(int userId, ErrorCode errorCode, IEnumerable<string> errorMessages)
    {
        await hubContext.Clients
            .User($"{userId}")
            .ErrorOccurred(new ErrorResponse(errorCode, errorMessages));
    }
    #endregion
}
