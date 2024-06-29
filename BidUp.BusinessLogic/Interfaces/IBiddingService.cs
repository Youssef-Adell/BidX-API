using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IBiddingService
{
    Task<AppResult<IEnumerable<BidResponse>>> GetAuctionBids(int auctionId);
    Task<AppResult<BidResponse>> GetAcceptedBid(int auctionId);
    Task<AppResult<BidResponse>> GetHighestBid(int auctionId);
    Task<AppResult<BidResponse>> BidUp(int bidderId, BidRequest bidRequest);
    Task<AppResult<BidResponse>> AcceptBid(int currentUserId, int bidId);
}
