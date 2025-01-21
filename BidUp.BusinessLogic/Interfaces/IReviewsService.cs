using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IReviewsService
{
    Task<Result<Page<ReviewResponse>>> GetUserReviewsReceived(int revieweeId, ReviewsQueryParams queryParams);
    Task<Result<MyReviewResponse>> GetReview(int reviewerId, int revieweeId);
    Task<Result<MyReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest request);
    Task<Result> UpdateReview(int reviewerId, int revieweeId, UpdateReviewRequest request);
    Task<Result> DeleteReview(int reviewerId, int revieweeId);
}
