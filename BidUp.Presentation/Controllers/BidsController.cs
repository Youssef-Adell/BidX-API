using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/auctions/{auctionId}/bids")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BidsController : ControllerBase
{
    private readonly IBiddingService biddingService;

    public BidsController(IBiddingService biddingService)
    {
        this.biddingService = biddingService;

    }


    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BidResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuctionBids(int auctionId)
    {
        var result = await biddingService.GetAuctionBids(auctionId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

    [HttpGet("accepted-bid")]
    [ProducesResponseType(typeof(BidResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAcceptedBid(int auctionId)
    {
        var result = await biddingService.GetAcceptedBid(auctionId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }

    [HttpGet("highest-bid")]
    public async Task<IActionResult> GetHighestBid(int auctionId)
    {
        var result = await biddingService.GetHighestBid(auctionId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }
}
