using System.Security.Claims;
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
        var userId = int.Parse(Context.User!.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await biddingService.BidUp(userId, bidRequest);

        if (!result.Succeeded)
        {
            await Clients.Caller.ErrorOccurred(result.Error!);
            return;
        }

        var auctionRoom = result.Response!.AuctionId.ToString();

        await Clients.Group(auctionRoom).BidCreated(result.Response!); // Notify clients who currently in the page of this auction
    }

    // The client must call this method when the auction page loads to be able to receive bidding updates in realtime
    public async Task JoinAuctionRoom(int auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
    }

    // The client must call this method when the auction page is about to be closed to stop receiving unnecessary bidding updates
    public async Task LeaveAuctionRoom(int auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
    }

    // The client must call this method when the feed page loads to be able to receive feed updates in realtime
    public async Task JoinAuctionsFeedRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AuctionsFeed");
    }

    // The client must call this method when the feed page is about to be closed to stop receiving unnecessary feed updates
    public async Task LeaveAuctionsFeedRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AuctionsFeed");
    }
}
