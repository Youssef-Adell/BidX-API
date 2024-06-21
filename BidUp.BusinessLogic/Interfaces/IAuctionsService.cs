using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IAuctionsService
{
    Task<IEnumerable<AuctionResponse>> GetAuctions();
    Task<AppResult<AuctionDetailsResponse>> GetAuction(int auctionId);
    Task<AppResult<AuctionResponse>> CreateAuction(int currentUserId, CreateAuctionRequest createAuctionRequest, IEnumerable<Stream> productImages);
    Task<AppResult> DeleteAuction(int currentUserId, int auctionId);
}
