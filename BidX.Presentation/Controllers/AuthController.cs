using System.Security.Claims;
using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidXesentation.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly LinkGenerator linkGenerator;
    const string NameOfRefreshTokenCookie = "RefreshToken";

    public AuthController(IAuthService authService, LinkGenerator linkGenerator)
    {
        this.authService = authService;
        this.linkGenerator = linkGenerator;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.Register(request);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return await SendConfirmationEmail(new() { Email = request.Email });
    }

    /*
    when the user hit this endpoint there is message contains a link to the "confirm-email" endpoint with (userId, token) in the query parameters will be sent to his email
    and when the user hit this link a get request will be sent to the "confirm-email" endpoint which will take the token in the query param to validate it and return an html page to indicate if the email confirmed or not
    */
    [HttpPost("resend-confirmation-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendConfirmationEmail(SendConfirmationEmailRequest request)
    {
        var urlOfConfirmationEndpoint = linkGenerator.GetUriByAction(HttpContext, nameof(ConfirmEmail));

        await authService.SendConfirmationEmail(request.Email, urlOfConfirmationEndpoint!);

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


    /// <response code="200">If the request is sent from a browser client the refreshToken will be set as an http-only cookie and won't be returned in the response body.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.Login(request);

        if (!result.Succeeded)
            return Unauthorized(result.Error);

        if (IsBrowserClient())
        {
            SetRefreshTokenCookie(result.Response!.RefreshToken);
            return Ok(new { result.Response!.User, result.Response.AccessToken }); // RefreshToken won't returned in the payload
        }
        else
            return Ok(result.Response);
    }

    /// <summary>Browser Clients don't have to send the request body, the server will extract the refresh token from the cookies instead.</summary>
    /// <response code="200">If the request is sent from a browser client the refreshToken will be set as an http-only cookie and won't be returned in the response body.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshRequest? request)
    {
        var refreshToken = IsBrowserClient() ? Request.Cookies[NameOfRefreshTokenCookie] : request?.RefreshToken;

        var result = await authService.Refresh(refreshToken);
        if (!result.Succeeded)
            return Unauthorized(result.Error);

        if (IsBrowserClient())
        {
            SetRefreshTokenCookie(result.Response!.RefreshToken);
            return Ok(new { result.Response!.User, result.Response.AccessToken }); // RefreshToken won't returned in the payload
        }
        else
            return Ok(result.Response);
    }

    /*
    when the user enter his email and hit the "forget-password" endpoint there is message contains a link to the "reset-password-page" endpoint with (accountId, token) in the query parameters will be sent to it
    the link will return an html page that has a form to set a new password and as i said there is accountId and resetCode included in the query parameter by the previous endpoint
    when the user press the submit button to submit the new password there is a post request contains (accountId, token, newPassword) will be sent to "reset-password" endpoint
    */
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await authService.SendPasswordResetEmail(request.Email, linkGenerator.GetUriByAction(HttpContext, nameof(GetPasswordResetPage))!);

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
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var result = await authService.ResetPassword(request);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = User.GetUserId();

        var result = await authService.ChangePassword(userId, request);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return NoContent();
    }

    [HttpPost("Logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId();

        await authService.RevokeRefreshToken(userId);

        if (IsBrowserClient())
            DeleteRefreshTokenCookie();

        return NoContent();
    }


    // Determines if the current request from a browser client or not
    [NonAction]
    private bool IsBrowserClient()
    {
        var userAgent = Request.Headers.UserAgent.ToString().ToLower();

        return userAgent.Contains("mozilla") ||        // Matches most modern browsers
                userAgent.Contains("chrome") ||        // Google Chrome
                userAgent.Contains("safari") ||        // Safari (exclude Chrome here)
                userAgent.Contains("edge") ||          // Microsoft Edge
                userAgent.Contains("firefox") ||       // Mozilla Firefox
                userAgent.Contains("opera") ||         // Opera browser
                userAgent.Contains("msie") ||          // Internet Explorer (older)
                userAgent.Contains("trident");         // Internet Explorer (newer)
    }

    // Sets the refresh token as an HTTP-only cookie with CHIPS
    [NonAction]
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,               // Ensures HTTPS is used
            SameSite = SameSiteMode.None, // Allows cross-site requests
            Path = "/",
            Expires = DateTime.UtcNow.AddMonths(1),
            IsEssential = true,          // Important for non-tracking cookies
        };

        // Add Partitioned attribute for CHIPS (https://developers.google.com/privacy-sandbox/cookies/chips)
        cookieOptions.Extensions.Add("Partitioned");

        Response.Cookies.Append(NameOfRefreshTokenCookie, refreshToken, cookieOptions);
    }

    [NonAction]
    private void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7),
            IsEssential = true,
        };
        cookieOptions.Extensions.Add("Partitioned");

        Response.Cookies.Delete(NameOfRefreshTokenCookie, cookieOptions);
    }
}
