using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IAuctionsService
{
    Task<AppResult<AuctionResponse>> CreateAuction(int auctioneerId, CreateAuctionRequest createAuctionRequest, IEnumerable<Stream> productImages);
}
