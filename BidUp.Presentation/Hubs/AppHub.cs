using Microsoft.AspNetCore.SignalR;


namespace BidUp.Presentation.Hubs;

public class AppHub : Hub<IAppHubClient>
{

    // The client should call this method when the feed page loads to be able to receive feed updates in realtime
    public async Task JoinAuctionsFeedRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AuctionsFeed");
    }

    // The client should call this method when the feed page is about to be closed to stop receiving unnecessary feed updates
    public async Task LeaveAuctionsFeedRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AuctionsFeed");
    }
}
