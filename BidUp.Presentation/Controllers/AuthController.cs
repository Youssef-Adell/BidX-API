using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidUp.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var result = await _authService.Register(registerRequest);

        if (!result.Succeeded)
            return UnprocessableEntity(result.Error);

        return NoContent();
    }
}
