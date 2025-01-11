using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IBidsService
{
    Task<AppResult<Page<BidResponse>>> GetAuctionBids(int auctionId, BidsQueryParams queryParams);
    Task<AppResult<BidResponse>> GetAcceptedBid(int auctionId);
    Task<AppResult<BidResponse>> GetHighestBid(int auctionId);
    Task<AppResult<BidResponse>> PlaceBid(int bidderId, BidRequest request);
    Task<AppResult<BidResponse>> AcceptBid(int callerId, AcceptBidRequest request);
}
