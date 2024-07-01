using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IBiddingService
{
    Task<AppResult<Page<BidResponse>>> GetAuctionBids(int auctionId, BidsQueryParams queryParams);
    Task<AppResult<BidResponse>> GetAcceptedBid(int auctionId);
    Task<AppResult<BidResponse>> GetHighestBid(int auctionId);
    Task<AppResult<BidResponse>> BidUp(int bidderId, BidRequest bidRequest);
    Task<AppResult<BidResponse>> AcceptBid(int currentUserId, AcceptBidRequest acceptBidRequest);
}
