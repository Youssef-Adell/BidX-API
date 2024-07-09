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
            .Where(r => r.RevieweeId == revieweeId);


        var totalCount = await userReviewsQuery.CountAsync();

        if (totalCount == 0)
            return AppResult<Page<ReviewResponse>>.Success(new Page<ReviewResponse>([], queryParams.Page, queryParams.PageSize, totalCount));


        var userReviews = await userReviewsQuery
            // Get the newly added reviews first
            .OrderByDescending(a => a.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var reviewsResponses = mapper.Map<IEnumerable<Review>, IEnumerable<ReviewResponse>>(userReviews);
        var response = new Page<ReviewResponse>(reviewsResponses, queryParams.Page, queryParams.PageSize, totalCount);

        return AppResult<Page<ReviewResponse>>.Success(response);
    }

    public async Task<AppResult<ReviewResponse>> GetReview(int reviewerId, int revieweeId)
    {
        var revieweeExists = await appDbContext.Users
            .AnyAsync(u => u.Id == revieweeId);

        if (!revieweeExists)
            return AppResult<ReviewResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);


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
