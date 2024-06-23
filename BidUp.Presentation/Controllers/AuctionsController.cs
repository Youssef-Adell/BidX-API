using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.Presentation.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionsService auctionsService;
    private readonly IHubContext<AppHub, IAppHubClient> hubContext;

    public AuctionsController(IAuctionsService auctionsService, IHubContext<AppHub, IAppHubClient> hubContext)
    {
        this.auctionsService = auctionsService;
        this.hubContext = hubContext;
    }


    [HttpGet]
    [ProducesResponseType(typeof(Page<AuctionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuctions([FromQuery] AuctionsQueryParams queryParams)
    {
        var response = await auctionsService.GetAuctions(queryParams);

        return Ok(response);
    }


    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuctionDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuction(int id)
    {
        var result = await auctionsService.GetAuction(id);

        if (!result.Succeeded)
            return NotFound(result.Error);

        return Ok(result.Response);
    }


    /// <summary>
    /// Invokes "AuctionCreated(AuctionResponse createdAuction)" on the SignalR clients who are currently in the auctions feed page
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AuctionDetailsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAuction([FromForm] CreateAuctionRequest createAuctionRequest, [Required][Length(1, 10, ErrorMessage = "The ProductImages field is required and must has 1 item at least item and 10 items at max.")] IEnumerable<IFormFile> productImages)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var imagesStreams = productImages.Select(i => i.OpenReadStream());

        try
        {
            var result = await auctionsService.CreateAuction(userId, createAuctionRequest, imagesStreams);

            if (!result.Succeeded)
                return UnprocessableEntity(result.Error);

            await hubContext.Clients.Group("AuctionsFeed").AuctionCreated(result.Response!); // Notify only the clients who currently in the feed page

            var createdAuction = (await auctionsService.GetAuction(result.Response!.Id)).Response!;

            return CreatedAtAction(nameof(GetAuction), new { id = createdAuction.Id }, createdAuction);
        }
        finally
        {   // Even if return is called within the try block, the finally block will still be executed.
            // IFormFile.OpenReadStream() doesn't need to be disposed. But disposing it also doesn't hurt anything (https://stackoverflow.com/a/66799724)
            foreach (var image in imagesStreams)
                await image.DisposeAsync();
        }
    }


    /// <summary>
    /// Invokes "AuctionDeletedOrEnded(int auctionId)" on the SignalR clients who are currently in the auctions feed page
    /// </summary>
    [HttpDelete("id")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAuction(int id)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await auctionsService.DeleteAuction(userId, id);

        if (!result.Succeeded)
            return NotFound(result.Error);

        await hubContext.Clients.Group("AuctionsFeed").AuctionDeletedOrEnded(id); // Notify only the clients who currently in the feed page

        return NoContent();
    }

}
