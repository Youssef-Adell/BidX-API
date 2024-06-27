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
    public async Task<IActionResult> GetAuctionBids(int auctionId)
    {
        var response = await biddingService.GetAuctionBids(auctionId);

        return Ok(response);
    }
}
