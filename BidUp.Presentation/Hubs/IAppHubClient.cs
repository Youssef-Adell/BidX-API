using BidUp.BusinessLogic.DTOs.AuctionDTOs;

namespace BidUp.Presentation.Hubs;

public interface IAppHubClient
{
    Task AuctionCreated(AuctionResponse auctionResponse);
}
