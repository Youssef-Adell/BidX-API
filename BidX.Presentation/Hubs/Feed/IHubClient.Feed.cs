using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.BidDTOs;

namespace BidX.Presentation.Hubs;

public partial interface IHubClient
{
    /// <summary>
    /// Triggerd for clients who currently in the Feed room
    /// </summary>
    Task AuctionCreated(AuctionResponse response);

    /// <summary>
    /// Triggerd for for clients who currently in the Feed room
    /// </summary>
    Task AuctionDeleted(AuctionDeletedResponse response);

    /// <summary>
    /// Triggerd for clients who currently in the Feed room
    /// </summary>
    Task AuctionEnded(AuctionEndedResponse response);

    /// <summary>
    /// Triggerd for clients who currently in the Feed room
    /// </summary>
    Task AuctionPriceUpdated(AuctionPriceUpdatedResponse response);
}
