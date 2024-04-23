using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.DataAccess.Entites;
using Humanizer;
using Microsoft.AspNetCore.Identity;

namespace BidUp.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;

    public AuthService(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<AppResult> Register(RegisterRequest registerRequest, string userRole)
    {
        var user = new User() { UserName = registerRequest.Username, Email = registerRequest.Email };

        var creationResult = await userManager.CreateAsync(user, registerRequest.Password);

        if (!creationResult.Succeeded)
            return new AppResult(ErrorCode.AUTH_VIOLATE_REGISTER_RULES, creationResult.Errors.Humanize(e => e.Description));

        var addingRolesResult = await userManager.AddToRoleAsync(user, userRole);

        return new AppResult();
    }
}
