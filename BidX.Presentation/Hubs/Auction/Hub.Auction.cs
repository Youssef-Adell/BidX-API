using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using Microsoft.AspNetCore.Authorization;

namespace BidX.Presentation.Hubs;

public partial class Hub
{
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
