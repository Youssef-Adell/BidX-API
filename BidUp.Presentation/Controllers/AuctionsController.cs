using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.AuctionDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.Presentation.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;


namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionsService auctionsService;
    private readonly IHubContext<AppHub, IAppHubClient> hubContext;

    public AuctionsController(IAuctionsService auctionsService, IHubContext<AppHub, IAppHubClient> hubContext)
    {
        this.auctionsService = auctionsService;
        this.hubContext = hubContext;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAuction([FromForm] CreateAuctionRequest createAuctionRequest, [Required][Length(1, 10, ErrorMessage = "The ProductImages field is required and must has 1 item at least item and 10 items at max.")] IEnumerable<IFormFile> productImages)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var imagesStreams = productImages.Select(i => i.OpenReadStream());

        try
        {
            var result = await auctionsService.CreateAuction(userId, createAuctionRequest, imagesStreams);

            if (!result.Succeeded)
                return UnprocessableEntity(result.Error);

            await hubContext.Clients.All.AuctionCreated(result.Response!);

            return NoContent(); // To be changed later to CreatedAtAction after implementing GetAuction endpoint.
        }
        finally
        {   // Even if return is called within the try block, the finally block will still be executed.
            // IFormFile.OpenReadStream() doesn't need to be disposed. But disposing it also doesn't hurt anything (https://stackoverflow.com/a/66799724)
            foreach (var image in imagesStreams)
                await image.DisposeAsync();
        }
    }
}
