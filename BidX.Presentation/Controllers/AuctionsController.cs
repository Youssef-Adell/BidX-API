using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BidX.BusinessLogic.DTOs.AuctionDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Interfaces;
using BidXesentation.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Hub = BidXesentation.Hubs.Hub;


namespace BidXesentation.Controllers;

[ApiController]
[Route("api/auctions")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionsService auctionsService;
    private readonly IHubContext<Hub, IHubClient> hubContext;

    public AuctionsController(IAuctionsService auctionsService, IHubContext<Hub, IHubClient> hubContext)
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
    /// Triggers "AuctionCreated" event for clients who currently in the Feed room
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AuctionDetailsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAuction([FromForm] CreateAuctionRequest request, [Required][Length(1, 10, ErrorMessage = "The ProductImages field is required and must has 1 item at least item and 10 items at max.")] IEnumerable<IFormFile> productImages)
    {
        var userId = User.GetUserId();

        var imagesStreams = productImages.Select(i => i.OpenReadStream());

        try
        {
            var result = await auctionsService.CreateAuction(userId, request, imagesStreams);

            if (!result.Succeeded)
                return UnprocessableEntity(result.Error);

            await hubContext.Clients.Group("FEED").AuctionCreated(result.Response!);

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
    /// Triggers "AuctionDeleted" event for clients who currently in the Feed room
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAuction(int id)
    {
        var userId = User.GetUserId();
        var result = await auctionsService.DeleteAuction(userId, id);

        if (!result.Succeeded)
            return NotFound(result.Error);

        await hubContext.Clients.Group("FEED").AuctionDeleted(new() { AuctionId = id });

        return NoContent();
    }

}
