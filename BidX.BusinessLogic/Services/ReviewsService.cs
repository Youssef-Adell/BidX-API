using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.DTOs.ReviewsDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class ReviewsService : IReviewsService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public ReviewsService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;
    }


    public async Task<Result<Page<ReviewResponse>>> GetUserReviewsReceived(int revieweeId, ReviewsQueryParams queryParams)
    {
        var userReviewsQuery = appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId)
            .Include(r => r.Reviewer);

        var totalCount = await userReviewsQuery.CountAsync();
        if (totalCount == 0)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return Result<Page<ReviewResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return Result<Page<ReviewResponse>>.Success(new Page<ReviewResponse>([], queryParams.Page, queryParams.PageSize, totalCount));
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
        return Result<Page<ReviewResponse>>.Success(response);
    }

    public async Task<Result<MyReviewResponse>> GetReview(int reviewerId, int revieweeId)
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
                return Result<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return Result<MyReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        return Result<MyReviewResponse>.Success(review);
    }

    public async Task<Result<MyReviewResponse>> AddReview(int reviewerId, int revieweeId, AddReviewRequest request)
    {
        var validationResult = await ValidateReviewEligibility(reviewerId, revieweeId);
        if (!validationResult.Succeeded)
            return Result<MyReviewResponse>.Failure(validationResult.Error!);

        // Create and save the review
        var review = mapper.Map<AddReviewRequest, Review>(request, o =>
        {
            o.Items["ReviewerId"] = reviewerId;
            o.Items["RevieweeId"] = revieweeId;
        });
        appDbContext.Reviews.Add(review);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Review, MyReviewResponse>(review);
        return Result<MyReviewResponse>.Success(response);
    }

    public async Task<Result> UpdateReview(int reviewerId, int revieweeId, UpdateReviewRequest request)
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
                return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteReview(int reviewerId, int revieweeId)
    {
        var noOfRowsAffected = await appDbContext.Reviews
            .Where(r => r.RevieweeId == revieweeId && r.ReviewerId == reviewerId)
            .ExecuteDeleteAsync();

        if (noOfRowsAffected <= 0)
        {
            var revieweeExists = await appDbContext.Users.AnyAsync(u => u.Id == revieweeId);
            if (!revieweeExists)
                return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

            return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["You have not reviewed this user before."]);
        }

        return Result.Success();
    }


    private async Task<Result> ValidateReviewEligibility(int reviewerId, int revieweeId)
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
            return Result.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        if (!revieweeInfo.HasDealtWithReviewer)
            return Result.Failure(ErrorCode.REVIEWING_NOW_ALLOWED, ["You cannot review a user you have not dealt with before."]);

        if (revieweeInfo.HasReviewedBefore)
            return Result.Failure(ErrorCode.REVIEW_ALREADY_EXISTS, ["You cannot review a user more than once."]);

        return Result.Success();
    }
}
