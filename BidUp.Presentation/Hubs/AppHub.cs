using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace BidUp.Presentation.Hubs;

public class AppHub : Hub<IAppHubClient>
{
    private readonly IBiddingService biddingService;

    public AppHub(IBiddingService biddingService)
    {
        this.biddingService = biddingService;
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
        await Clients.All.AuctionPriceUpdated(new() { AuctionId = createdBid.AuctionId, NewPrice = createdBid.Amount });
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
        await Clients.All.AuctionDeletedOrEnded(new() { AuctionId = acceptedBid.AuctionId });
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
}
