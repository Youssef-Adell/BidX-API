using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;
using Microsoft.AspNetCore.Authorization;

namespace BidX.Presentation.Hubs;

public partial class Hub
{
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
