using System.ComponentModel.DataAnnotations;
using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.DTOs.ProfileDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BidX.Presentation.Utils;

namespace BidX.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UsersController : ControllerBase
{
    private readonly IAuctionsService auctionsService;
    private readonly IProfilesService profilesService;

    public UsersController(IAuctionsService auctionsService, IProfilesService profilesService)
    {
        this.auctionsService = auctionsService;
        this.profilesService = profilesService;
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
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(int userId)
    {
        var result = await profilesService.GetProfile(userId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpPut("current/profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentProfile(ProfileUpdateRequest request)
    {
        var userId = User.GetUserId();

        await profilesService.UpdateProfile(userId, request);

        return NoContent();
    }

    [HttpPut("current/profile/picture")]
    [Authorize]
    [ProducesResponseType(typeof(UpdatedProfilePictureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentProfilePicture([Required] IFormFile profilePicture)
    {
        var userId = User.GetUserId();

        using (var profilePictureStream = profilePicture.OpenReadStream())
        {
            var result = await profilesService.UpdateProfilePicture(userId, profilePictureStream);

            if (!result.Succeeded)
                return UnprocessableEntity(result.Error);

            return Ok(result.Response);
        }
    }

}
