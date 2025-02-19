using BidX.BusinessLogic.DTOs.ReviewsDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class ReviewMappingExtensions
{
    public static Review ToReviewEntity(this AddReviewRequest request, int reviewerId, int revieweeId)
    {
        return new Review
        {
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
        };
    }

    public static MyReviewResponse ToMyReviewResponse(this Review review)
    {
        return new MyReviewResponse
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
        };
    }

    public static IQueryable<ReviewResponse> ProjectToReviewResponse(this IQueryable<Review> query)
    {
        return query.Select(r => new ReviewResponse
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            Reviewer = new Reviewer
            {
                Id = r.Reviewer!.Id,
                FullName = r.Reviewer.FullName,
                ProfilePictureUrl = r.Reviewer.ProfilePictureUrl,
                AverageRating = r.Reviewer.AverageRating
            }
        });
    }

    public static IQueryable<MyReviewResponse> ProjectToMyReviewResponse(this IQueryable<Review> query)
    {
        return query.Select(r => new MyReviewResponse
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
    }
}
