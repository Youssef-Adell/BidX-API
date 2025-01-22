using System.Security.Claims;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.DTOs.ReviewsDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidXesentation.Controllers;

[ApiController]
[Route("api/users/{userId}/reviews")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ReviewsController : ControllerBase
{
    private readonly IReviewsService reviewsService;

    public ReviewsController(IReviewsService reviewsService)
    {
        this.reviewsService = reviewsService;
    }



    [HttpGet]
    [ProducesResponseType(typeof(Page<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserReviewsReceived(int userId, [FromQuery] ReviewsQueryParams queryParams)
    {
        var result = await reviewsService.GetUserReviewsReceived(userId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(MyReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddReview(int userId, AddReviewRequest request)
    {
        var reviewerId = User.GetUserId();

        var result = await reviewsService.AddReview(reviewerId, userId, request);

        if (!result.Succeeded)
        {
            var errorCode = result.Error!.ErrorCode;
            if (errorCode == ErrorCode.RESOURCE_NOT_FOUND)
                return NotFound(result.Error);

            else if (errorCode == ErrorCode.REVIEWING_NOW_ALLOWED)
                return StatusCode(StatusCodes.Status403Forbidden, result.Error);

            else if (errorCode == ErrorCode.REVIEW_ALREADY_EXISTS)
                return StatusCode(StatusCodes.Status409Conflict, result.Error);
        }

        return CreatedAtAction(nameof(GetMyReview), new { userId = userId }, result.Response);
    }


    [HttpGet("my-review")]
    [Authorize]
    [ProducesResponseType(typeof(MyReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyReview(int userId)
    {
        var reviewerId = User.GetUserId();

        var result = await reviewsService.GetReview(reviewerId, userId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpPut("my-review")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyReview(int userId, UpdateReviewRequest request)
    {
        var reviewerId = User.GetUserId();

        var result = await reviewsService.UpdateReview(reviewerId, userId, request);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return NoContent();
    }


    [HttpDelete("my-review")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMyReview(int userId)
    {
        var reviewerId = User.GetUserId();

        var result = await reviewsService.DeleteReview(reviewerId, userId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return NoContent();
    }

}
