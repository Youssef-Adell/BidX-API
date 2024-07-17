using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IReviewsService
{
    Task<AppResult<Page<ReviewResponse>>> GetUserReviewsReceived(int revieweeId, ReviewsQueryParams queryParams);
    Task<AppResult<MyReviewResponse>> GetReview(int reviewerId, int revieweeId);
    Task<AppResult<MyReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest addReviewRequest);
    Task<AppResult> UpdateReview(int reviewerId, int revieweeId, UpdateReviewRequest updateReviewRequest);
}
