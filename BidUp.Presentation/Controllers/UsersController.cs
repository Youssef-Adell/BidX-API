using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/users/{userId}")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class UsersController : ControllerBase
{
    private readonly IAuctionsService auctionsService;

    public UsersController(IAuctionsService auctionsService)
    {
        this.auctionsService = auctionsService;

    }


    /// <summary>
    /// Gets the auctions created by the user
    /// </summary>
    [HttpGet("created-auctions")]
    [ProducesResponseType(typeof(Page<AuctionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserAuctions(int userId, [FromQuery] UserAuctionsQueryParams queryParams)
    {
        var result = await auctionsService.GetUserAuctions(userId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    [HttpGet("bidded-auctions")]
    public async Task<IActionResult> GetAuctionsUserHasBidOn(int userId, [FromQuery] AuctionsUserHasBidOnQueryParams queryParams)
    {
        var result = await auctionsService.GetAuctionsUserHasBidOn(userId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

}
