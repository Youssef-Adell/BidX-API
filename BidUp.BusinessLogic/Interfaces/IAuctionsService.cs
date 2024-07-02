using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IAuctionsService
{
    Task<Page<AuctionResponse>> GetAuctions(AuctionsQueryParams queryParams);
    Task<AppResult<Page<AuctionResponse>>> GetUserAuctions(int userId, UserAuctionsQueryParams queryParams);
    Task<AppResult<AuctionDetailsResponse>> GetAuction(int auctionId);
    Task<AppResult<AuctionResponse>> CreateAuction(int currentUserId, CreateAuctionRequest createAuctionRequest, IEnumerable<Stream> productImages);
    Task<AppResult> DeleteAuction(int currentUserId, int auctionId);
}
