using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        var userReviewsQuery = appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId)
            .Include(r => r.Reviewer);

        var totalCount = await userReviewsQuery.CountAsync();
        if (totalCount == 0)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return AppResult<Page<ReviewResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return AppResult<Page<ReviewResponse>>.Success(new Page<ReviewResponse>([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        var userReviews = await userReviewsQuery
            // Get the newly added reviews first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ProjectTo<ReviewResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .ToListAsync();

        var response = new Page<ReviewResponse>(userReviews, queryParams.Page, queryParams.PageSize, totalCount);
        return AppResult<Page<ReviewResponse>>.Success(response);
    }

    public async Task<AppResult<MyReviewResponse>> GetReview(int reviewerId, int revieweeId)
    {
        var review = await appDbContext.Reviews
            .Where(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId)
            .ProjectTo<MyReviewResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .SingleOrDefaultAsync();

        if (review is null)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return AppResult<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return AppResult<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        return AppResult<MyReviewResponse>.Success(review);
    }

    public async Task<AppResult<MyReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest request)
    {
        var validationResult = await ValidateReviewEligibility(reviewerId, revieweeId);
        if (!validationResult.Succeeded)
            return AppResult<MyReviewResponse>.Failure(validationResult.Error!);

        // Create and save the review
        var review = mapper.Map<AddReviewRequest, Review>(request, o =>
        {
            o.Items["ReviewerId"] = reviewerId;
            o.Items["RevieweeId"] = revieweeId;
        });
        appDbContext.Reviews.Add(review);
        await appDbContext.SaveChangesAsync();
        await UpdateRevieweeAverageRating(revieweeId);

        var response = mapper.Map<Review, MyReviewResponse>(review);
        return AppResult<MyReviewResponse>.Success(response);
    }

    public async Task<AppResult> UpdateReview(int reviewerId, int revieweeId, UpdateReviewRequest request)
    {
        var noOfRowsAffected = await appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId && r.ReviewerId == reviewerId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Rating, request.Rating)
                .SetProperty(r => r.Comment, request.Comment)
                .SetProperty(r => r.UpdatedAt, DateTimeOffset.UtcNow));

        if (noOfRowsAffected <= 0)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        await UpdateRevieweeAverageRating(revieweeId);

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

        await UpdateRevieweeAverageRating(revieweeId);

        return AppResult.Success();
    }


    private async Task<AppResult> ValidateReviewEligibility(int reviewerId, int revieweeId)
    {
        var revieweeInfo = await appDbContext.Users
            .Where(u => u.Id == revieweeId)
            .Select(u => new
            {
                HasDealtWithReviewer = appDbContext.Auctions.Any(a =>
                    (a.WinnerId == reviewerId && a.AuctioneerId == revieweeId) ||
                    (a.AuctioneerId == reviewerId && a.WinnerId == revieweeId)),
                HasReviewedBefore = appDbContext.Reviews.Any(r => r.ReviewerId == reviewerId && r.RevieweeId == revieweeId)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (revieweeInfo == null)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        if (!revieweeInfo.HasDealtWithReviewer)
            return AppResult.Failure(ErrorCode.REVIEWING_NOW_ALLOWED, ["You cannot review a user you have not dealt with before."]);

        if (revieweeInfo.HasReviewedBefore)
            return AppResult.Failure(ErrorCode.REVIEW_ALREADY_EXISTS, ["You cannot review a user more than once."]);

        return AppResult.Success();
    }

    private async Task UpdateRevieweeAverageRating(int revieweeId)
    {
        await appDbContext.Users
            .Where(u => u.Id == revieweeId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.AverageRating,
                    appDbContext.Reviews
                        .Where(r => r.RevieweeId == revieweeId)
                        .Average(r => (decimal?)r.Rating) ?? 0));
    }
}
