using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IBidsService
{
    Task<Result<Page<BidResponse>>> GetAuctionBids(int auctionId, BidsQueryParams queryParams);
    Task<Result<BidResponse>> GetAcceptedBid(int auctionId);
    Task<Result<BidResponse>> GetHighestBid(int auctionId);
    Task PlaceBid(int bidderId, BidRequest request);
    Task AcceptBid(int callerId, AcceptBidRequest request);
}
