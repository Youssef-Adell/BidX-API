using System.Text;
using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.DataAccess.Entites;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace BidUp.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;
    private readonly IEmailService emailService;

    public AuthService(UserManager<User> userManager, IEmailService emailService)
    {
        this.userManager = userManager;
        this.emailService = emailService;
    }

    public async Task<AppResult> Register(RegisterRequest registerRequest, string userRole)
    {
        var user = new User()
        {
            FirstName = registerRequest.FirstName.Trim(),
            LastName = registerRequest.LastName.Trim(),
            UserName = Guid.NewGuid().ToString(), // because it needs a unique value and we dont want to ask user to enter it to make the register process easier, and if we set it to the email value it will give user 2 errors in case if the entered email is already taken, one for username and one for email
            Email = registerRequest.Email.Trim()
        };

        var creationResult = await userManager.CreateAsync(user, registerRequest.Password.Trim());

        if (!creationResult.Succeeded)
            return new AppResult(ErrorCode.AUTH_VIOLATE_REGISTER_RULES, creationResult.Errors.Humanize(e => e.Description));

        var addingRolesResult = await userManager.AddToRoleAsync(user, userRole);

        return new AppResult();
    }

    public async Task SendConfirmationEmail(string email, string urlOfConfirmationEndpoint)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null && !user.EmailConfirmed)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmationLink = $"{urlOfConfirmationEndpoint}?userId={user.Id}&token={token}";

            await emailService.SendConfirmationEmail(email, confirmationLink);
        }
    }

    public async Task<bool> ConfirmEmail(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            return false;

        if (!user.EmailConfirmed)
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return false;
        }

        return true;
    }
}
