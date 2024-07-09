using AutoMapper;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class ReviewsService : IReviewsService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public ReviewsService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;
    }


    public async Task<AppResult<ReviewResponse>> GetReview(int reviewerId, int revieweeId)
    {
        var review = await appDbContext.Reviews
            .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId);

        if (review is null)
            return AppResult<ReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);

        var response = mapper.Map<Review, ReviewResponse>(review);

        return AppResult<ReviewResponse>.Success(response);
    }

    public async Task<AppResult<ReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest addReviewRequest)
    {
        var revieweeExists = await appDbContext.Users
            .AnyAsync(u => u.Id == revieweeId);

        if (!revieweeExists)
            return AppResult<ReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);


        var isAllowedToReview = await appDbContext.Auctions
            .AnyAsync(a => (a.WinnerId == reviewerId && a.AuctioneerId == revieweeId) || // Check if the current user is a winner for an auction belongs to the reviewee
                            (a.AuctioneerId == reviewerId && a.WinnerId == revieweeId)); // Check if the current user is an owner of an auction that the reviewee has won

        if (!isAllowedToReview)
            return AppResult<ReviewResponse>.Failure(ErrorCode.PERMISSION_DENIED, ["You cannot review a user you have not dealt with before."]);


        var isReviewdBefore = await appDbContext.Reviews
            .AnyAsync(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId);

        if (isReviewdBefore)
            return AppResult<ReviewResponse>.Failure(ErrorCode.REVIEW_ALREADY_EXISTS, ["You cannot review a user more than once."]);


        var review = new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            Rating = addReviewRequest.Rating,
            Comment = addReviewRequest.Comment,
        };

        appDbContext.Reviews.Add(review);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Review, ReviewResponse>(review);
        return AppResult<ReviewResponse>.Success(response);
    }

}
