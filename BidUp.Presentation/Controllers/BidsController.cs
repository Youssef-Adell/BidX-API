using BidUp.BusinessLogic.DTOs.BidDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/auctions/{auctionId}/bids")]
public class BidsController : ControllerBase
{
    private readonly IBiddingService biddingService;

    public BidsController(IBiddingService biddingService)
    {
        this.biddingService = biddingService;

    }


    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BidResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuctionBids(int auctionId)
    {
        var result = await biddingService.GetAuctionBids(auctionId);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }
}
