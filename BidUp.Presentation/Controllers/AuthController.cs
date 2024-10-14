using System.Security.Claims;
using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly LinkGenerator linkGenerator;
    public AuthController(IAuthService authService, LinkGenerator linkGenerator)
    {
        this.authService = authService;
        this.linkGenerator = linkGenerator;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
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

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var result = await authService.Login(loginRequest);

        if (!result.Succeeded)
            return Unauthorized(result.Error);

        return Ok(result.Response);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshRequest refreshRequest)
    {
        var result = await authService.Refresh(refreshRequest.RefreshToken);

        if (!result.Succeeded)
            return Unauthorized(result.Error);

        return Ok(result.Response);
    }

    /*
    when the user enter his email and hit the "forget-password" endpoint there is message contains a link to the "reset-password-page" endpoint with (accountId, token) in the query parameters will be sent to it
    the link will return an html page that has a form to set a new password and as i said there is accountId and resetCode included in the query parameter by the previous endpoint
    when the user press the submit button to submit the new password there is a post request contains (accountId, token, newPassword) will be sent to "reset-password" endpoint
    */
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest forgotPasswordRequest)
    {
        await authService.SendPasswordResetEmail(forgotPasswordRequest.Email, linkGenerator.GetUriByAction(HttpContext, nameof(GetPasswordResetPage))!);

        return NoContent();
    }

    [HttpGet("password-reset-page")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ContentResult> GetPasswordResetPage(string userId, string token)
    {
        var html = await System.IO.File.ReadAllTextAsync(@"./wwwroot/Pages/reset-password.html");

        html = html.Replace("{{resetPasswordEndpointUrl}}", linkGenerator.GetUriByAction(HttpContext, nameof(ResetPassword)));

        return Content(html, "text/html");
    }

    [HttpPost("reset-password")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        var result = await authService.ResetPassword(resetPasswordRequest);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        var result = await authService.ChangePassword(userId, changePasswordRequest);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return NoContent();
    }

    [HttpPost("revoke-refresh-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevokeRefreshToken()
    {
        var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

        await authService.RevokeRefreshToken(userId);

        return NoContent();
    }
}
