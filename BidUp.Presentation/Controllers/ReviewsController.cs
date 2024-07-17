using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.DTOs.ReviewsDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

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
    public async Task<IActionResult> AddReview(int userId, AddReviewRequest addReviewRequest)
    {
        var reviewerId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await reviewsService.AddReview(reviewerId, userId, addReviewRequest);

        if (!result.Succeeded)
        {
            var errorCode = result.Error!.ErrorCode;
            if (errorCode == ErrorCode.RESOURCE_NOT_FOUND)
                return NotFound(result.Error);

            else if (errorCode == ErrorCode.PERMISSION_DENIED)
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
        var reviewerId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await reviewsService.GetReview(reviewerId, userId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpPut("my-review")]
    [Authorize]
    [ProducesResponseType(typeof(MyReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyReview(int userId, UpdateReviewRequest updateReviewRequest)
    {
        var reviewerId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await reviewsService.UpdateReview(reviewerId, userId, updateReviewRequest);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return NoContent();
    }

}
