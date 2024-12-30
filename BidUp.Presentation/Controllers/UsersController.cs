using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.DTOs.UserProfileDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UsersController : ControllerBase
{
    private readonly IAuctionsService auctionsService;
    private readonly IUsersService usersService;

    public UsersController(IAuctionsService auctionsService, IUsersService usersService)
    {
        this.auctionsService = auctionsService;
        this.usersService = usersService;
    }


    /// <summary>
    /// Gets the auctions created by the user
    /// </summary>
    [HttpGet("{userId}/created-auctions")]
    [ProducesResponseType(typeof(Page<AuctionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserAuctions(int userId, [FromQuery] UserAuctionsQueryParams queryParams)
    {
        var result = await auctionsService.GetUserAuctions(userId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    /// <summary>
    /// Gets the auctions that the user has bid on
    /// </summary>
    [HttpGet("{userId}/bidded-auctions")]
    [ProducesResponseType(typeof(Page<AuctionUserHasBidOnResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuctionsUserHasBidOn(int userId, [FromQuery] AuctionsUserHasBidOnQueryParams queryParams)
    {
        var result = await auctionsService.GetAuctionsUserHasBidOn(userId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpGet("{userId}/profile")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(int userId)
    {
        var result = await usersService.GetUserProfile(userId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpPut("current/profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUserProfile(UserProfileUpdateRequest userProfileUpdateRequest)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        await usersService.UpdateUserProfile(userId, userProfileUpdateRequest);

        return NoContent();
    }

    [HttpPut("current/profile/picture")]
    [Authorize]
    [ProducesResponseType(typeof(UpdatedProfilePictureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUserProfilePicture([Required] IFormFile profilePicture)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        using (var profilePictureStream = profilePicture.OpenReadStream())
        {
            var result = await usersService.UpdateUserProfilePicture(userId, profilePictureStream);

            if (!result.Succeeded)
                return UnprocessableEntity(result.Error);

            return Ok(result.Response);
        }
    }

}
