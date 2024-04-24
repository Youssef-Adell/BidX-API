using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly LinkGenerator linkGenerator;

    public AuthController(IAuthService authService, LinkGenerator linkGenerator)
    {
        this.authService = authService;
        this.linkGenerator = linkGenerator;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var result = await authService.Register(registerRequest);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return await SendConfirmationEmail(new() { Email = registerRequest.Email });
    }

    /*
    when the user hit this endpoint there is message contains a link to the "confirm-email" endpoint with (userId, token) in the query parameters will be sent to his email
    and when the user hit this link a get request will be sent to the "confirm-email" endpoint which will take the token in the query param to validate it and return an html page to indicate if the email confirmed or not
    */
    [HttpPost("resend-confirmation-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendConfirmationEmail(SendConfirmationEmailRequest sendConfirmationEmailRequest)
    {
        var urlOfConfirmationEndpoint = linkGenerator.GetUriByAction(HttpContext, nameof(ConfirmEmail));

        await authService.SendConfirmationEmail(sendConfirmationEmailRequest.Email, urlOfConfirmationEndpoint!);

        return NoContent();
    }

    [HttpGet("confirm-email")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ContentResult> ConfirmEmail(string userId, string token)
    {
        var isConfirmed = await authService.ConfirmEmail(userId, token);

        string html;

        if (isConfirmed)
            html = await System.IO.File.ReadAllTextAsync(@"./wwwroot/Pages/confirmation-succeeded.html");
        else
            html = await System.IO.File.ReadAllTextAsync(@"./wwwroot/pages/confirmation-faild.html");

        return Content(html, "text/html");
    }
}
