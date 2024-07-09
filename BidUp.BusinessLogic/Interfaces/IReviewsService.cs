using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IReviewsService
{
    Task<AppResult<ReviewResponse>> GetReview(int reviewerId, int revieweeId);
    Task<AppResult<ReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest addReviewRequest);
}
