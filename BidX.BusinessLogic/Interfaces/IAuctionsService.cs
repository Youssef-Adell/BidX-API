using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IAuctionsService
{
    Task<Page<AuctionResponse>> GetAuctions(AuctionsQueryParams queryParams);
    Task<Result<Page<AuctionResponse>>> GetUserAuctions(int userId, UserAuctionsQueryParams queryParams);
    Task<Result<Page<AuctionUserHasBidOnResponse>>> GetAuctionsUserHasBidOn(int userId, AuctionsUserHasBidOnQueryParams queryParams);
    Task<Result<AuctionDetailsResponse>> GetAuction(int auctionId);
    Task<Result<AuctionResponse>> CreateAuction(int callerId, CreateAuctionRequest request, IEnumerable<Stream> productImages);
    Task<Result> DeleteAuction(int callerId, int auctionId);
}
