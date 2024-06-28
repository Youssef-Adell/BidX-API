using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IBiddingService
{
    Task<AppResult<BidResponse>> BidUp(int bidderId, BidRequest bidRequest);
    Task<AppResult<IEnumerable<BidResponse>>> GetAuctionBids(int auctionId);
}
