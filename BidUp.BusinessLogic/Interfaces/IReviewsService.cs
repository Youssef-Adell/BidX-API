using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IReviewsService
{
    Task<AppResult<Page<ReviewResponse>>> GetUserReviewsReceived(int revieweeId, ReviewsQueryParams queryParams);
    Task<AppResult<ReviewResponse>> GetReview(int reviewerId, int revieweeId);
    Task<AppResult<ReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest addReviewRequest);
}
