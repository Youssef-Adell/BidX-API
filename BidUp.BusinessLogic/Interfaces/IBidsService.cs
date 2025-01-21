using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IBidsService
{
    Task<Result<Page<BidResponse>>> GetAuctionBids(int auctionId, BidsQueryParams queryParams);
    Task<Result<BidResponse>> GetAcceptedBid(int auctionId);
    Task<Result<BidResponse>> GetHighestBid(int auctionId);
    Task<Result<BidResponse>> PlaceBid(int bidderId, BidRequest request);
    Task<Result<BidResponse>> AcceptBid(int callerId, AcceptBidRequest request);
}
