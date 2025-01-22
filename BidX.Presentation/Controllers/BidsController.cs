using BidX.BusinessLogic.DTOs.BidDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidXesentation.Controllers;

[ApiController]
[Route("api/auctions/{auctionId}/bids")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BidsController : ControllerBase
{
    private readonly IBidsService bidsService;

    public BidsController(IBidsService bidsService)
    {
        this.bidsService = bidsService;

    }


    [HttpGet]
    [ProducesResponseType(typeof(Page<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuctionBids(int auctionId, [FromQuery] BidsQueryParams queryParams)
    {
        var result = await bidsService.GetAuctionBids(auctionId, queryParams);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

    [HttpGet("accepted-bid")]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAcceptedBid(int auctionId)
    {
        var result = await bidsService.GetAcceptedBid(auctionId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

    [HttpGet("highest-bid")]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHighestBid(int auctionId)
    {
        var result = await bidsService.GetHighestBid(auctionId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }
}
