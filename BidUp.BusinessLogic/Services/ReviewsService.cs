using AutoMapper;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
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


    public async Task<AppResult<Page<ReviewResponse>>> GetUserReviewsReceived(int revieweeId, ReviewsQueryParams queryParams)
    {
        var revieweeExists = await appDbContext.Users
            .AnyAsync(u => u.Id == revieweeId);

        if (!revieweeExists)
            return AppResult<Page<ReviewResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);


        var userReviewsQuery = appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId)
            .Include(r => r.Reviewer)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                Reviewer = new Reviewer
                {
                    Id = r.ReviewerId,
                    Name = $"{r.Reviewer!.FirstName} {r.Reviewer.LastName}",
                    ProfilePictureUrl = r.Reviewer.ProfilePictureUrl
                }
            });

        var totalCount = await userReviewsQuery.CountAsync();

        if (totalCount == 0)
            return AppResult<Page<ReviewResponse>>.Success(new Page<ReviewResponse>([], queryParams.Page, queryParams.PageSize, totalCount));

        var userReviewsResponses = await userReviewsQuery
            // Get the newly added reviews first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();


        var response = new Page<ReviewResponse>(userReviewsResponses, queryParams.Page, queryParams.PageSize, totalCount);
        return AppResult<Page<ReviewResponse>>.Success(response);
    }

    public async Task<AppResult<MyReviewResponse>> GetReview(int reviewerId, int revieweeId)
    {
        var revieweeExists = await appDbContext.Users
            .AnyAsync(u => u.Id == revieweeId);

        if (!revieweeExists)
            return AppResult<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);


        var review = await appDbContext.Reviews
            .FirstOrDefaultAsync(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId);

        if (review is null)
            return AppResult<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);


        var response = mapper.Map<Review, MyReviewResponse>(review);
        return AppResult<MyReviewResponse>.Success(response);
    }

    public async Task<AppResult<MyReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest addReviewRequest)
    {
        var revieweeExists = await appDbContext.Users
            .AnyAsync(u => u.Id == revieweeId);

        if (!revieweeExists)
            return AppResult<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);


        var isAllowedToReview = await appDbContext.Auctions
            .AnyAsync(a => (a.WinnerId == reviewerId && a.AuctioneerId == revieweeId) || // Check if the current user is a winner for an auction belongs to the reviewee
                            (a.AuctioneerId == reviewerId && a.WinnerId == revieweeId)); // Check if the current user is an owner of an auction that the reviewee has won

        if (!isAllowedToReview)
            return AppResult<MyReviewResponse>.Failure(ErrorCode.PERMISSION_DENIED, ["You cannot review a user you have not dealt with before."]);


        var isReviewdBefore = await appDbContext.Reviews
            .AnyAsync(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId);

        if (isReviewdBefore)
            return AppResult<MyReviewResponse>.Failure(ErrorCode.REVIEW_ALREADY_EXISTS, ["You cannot review a user more than once."]);


        var review = new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            Rating = addReviewRequest.Rating,
            Comment = addReviewRequest.Comment,
        };

        appDbContext.Reviews.Add(review);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Review, MyReviewResponse>(review);
        return AppResult<MyReviewResponse>.Success(response);
    }

    public async Task<AppResult> UpdateReview(int reviewerId, int revieweeId, UpdateReviewRequest updateReviewRequest)
    {
        var noOfRowsAffected = await appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId && r.ReviewerId == reviewerId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Rating, updateReviewRequest.Rating)
                .SetProperty(r => r.Comment, updateReviewRequest.Comment));

        if (noOfRowsAffected <= 0)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        return AppResult.Success();
    }

    public async Task<AppResult> DeleteReview(int reviewerId, int revieweeId)
    {
        var noOfRowsAffected = await appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId && r.ReviewerId == reviewerId)
            .ExecuteDeleteAsync();

        if (noOfRowsAffected <= 0)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        return AppResult.Success();
    }
}
