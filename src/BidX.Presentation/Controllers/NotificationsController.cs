using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.Presentation.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BidX.Presentation.Controllers;


[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
[Authorize]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsService notificationsService;

    public NotificationsController(INotificationsService notificationsService)
    {
        this.notificationsService = notificationsService;
    }


    /// <summary>
    /// Gets the notifications of the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Page<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserChats([FromQuery] NotificationsQueryParams queryParams)
    {
        var userId = User.GetUserId();

        var response = await notificationsService.GetUserNotifications(userId, queryParams);

        return Ok(response);
    }
}
