using System.Text;
using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Mappings;
using BidX.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace BidX.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;
    private readonly IEmailService emailService;
    private readonly IConfiguration configuration;
    private readonly ITokenService tokenService;
    private readonly IAuthProviderFactory authProviderFactory;

    public AuthService(UserManager<User> userManager, IEmailService emailService, IConfiguration configuration, ITokenService tokenService, IAuthProviderFactory authProviderFactory)
    {
        this.userManager = userManager;
        this.emailService = emailService;
        this.configuration = configuration;
        this.tokenService = tokenService;
        this.authProviderFactory = authProviderFactory;
    }

    public async Task<Result> Register(RegisterRequest request, string userRole = "User")
    {
        var user = request.ToUserEntity();

        var creationResult = await userManager.CreateAsync(user, request.Password);
        if (!creationResult.Succeeded)
        {
            var errorMessages = creationResult.Errors.Select(error => error.Description);
            return Result.Failure(ErrorCode.AUTH_VIOLATE_REGISTER_RULES, errorMessages);
        }

        var addingRolesResult = await userManager.AddToRoleAsync(user, userRole);
        if (!addingRolesResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new Exception($"Faild to add roles while registering the user whose email is: {request.Email}."); // will be catched and logged by the global error handler middleware
        }

        await SendConfirmationEmail(user.Email!);

        return Result.Success();
    }

    public async Task SendConfirmationEmail(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null && !user.EmailConfirmed)
        {
            var emailConfirmationPageUrl = configuration["AuthPages:EmailConfirmationPageUrl"];

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmationLink = $"{emailConfirmationPageUrl}?userId={user.Id}&token={token}";

            await emailService.SendConfirmationEmail(email, confirmationLink);
        }
    }

    public async Task<Result<LoginResponse>> ConfirmEmail(ConfirmEmailRequest request)
    {
        var user = await userManager.FindByIdAsync($"{request.UserId}");

        if (user == null)
            return Result<LoginResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        if (!user.EmailConfirmed)
        {
            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(error => error.Description);
                return Result<LoginResponse>.Failure(ErrorCode.AUTH_EMAIL_CONFIRMATION_FAILD, errorMessages);
            }

            return Result<LoginResponse>.Success(await GenerateAuthResponse(user));
        }

        return Result<LoginResponse>.Failure(ErrorCode.AUTH_EMAIL_CONFIRMATION_FAILD, ["Email is Already Confirmed."]);
    }

    public async Task<Result<LoginResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_INVALID_USERNAME_OR_PASSWORD,
                ["Invalid email or password."]);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_ACCOUNT_IS_LOCKED_OUT,
                ["The account has been temporarily locked out."]);
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);

            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_INVALID_USERNAME_OR_PASSWORD,
                ["Invalid email or password."]);
        }

        if (!user.EmailConfirmed)
        {
            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_EMAIL_NOT_CONFIRMED,
                ["The email has not been confirmed."]);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return Result<LoginResponse>.Success(await GenerateAuthResponse(user));
    }

    public async Task<Result<LoginResponse>> Refresh(string? refreshToken)
    {
        var user = userManager.Users.SingleOrDefault(user => user.RefreshToken == refreshToken && user.RefreshToken != null);

        if (user is null)
            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_INVALID_REFRESH_TOKEN,
                ["Invalid refresh token."]);

        return Result<LoginResponse>.Success(await GenerateAuthResponse(user));
    }

    public async Task<Result<LoginResponse>> LoginWithExternalProvider(LoginWithExternalProviderRequest request)
    {
        var provider = authProviderFactory.GetProvider(request.Provider);

        var externalUser = await provider.Authenticate(request.IdToken);

        if (externalUser is null)
            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_EXTERNAL_LOGIN_FAILED,
                [$"Failed to login with {request.Provider}."]);

        var user = await GetOrCreateUser(externalUser);

        if (user is null)
            return Result<LoginResponse>.Failure(
                ErrorCode.AUTH_EXTERNAL_LOGIN_FAILED,
                [$"Failed to signup with {request.Provider}."]);


        return Result<LoginResponse>.Success(await GenerateAuthResponse(user));
    }

    public async Task SendPasswordResetEmail(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null && user.EmailConfirmed)
        {
            var resetPasswordPageUrl = configuration["AuthPages:ResetPasswordPageUrl"];

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var urlOfResetPasswordPageForCurrentUser = $"{resetPasswordPageUrl}?userId={user.Id}&token={token}";

            await emailService.SendPasswordResetEmail(email, urlOfResetPasswordPageForCurrentUser);
        }
    }

    public async Task<Result> ResetPassword(ResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user != null && user.EmailConfirmed)
        {
            request.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            var resetResult = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (!resetResult.Succeeded)
            {
                var errorMessages = resetResult.Errors.Select(error => error.Description);
                return Result.Failure(ErrorCode.AUTH_PASSWORD_RESET_FAILD, errorMessages);
            }

            return Result.Success();
        }

        return Result.Failure(ErrorCode.AUTH_PASSWORD_RESET_FAILD, ["Oops! Something went wrong."]);
    }

    public async Task<Result> ChangePassword(int userId, ChangePasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user != null)
        {
            var changingResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!changingResult.Succeeded)
            {
                var errorMessages = changingResult.Errors.Select(error => error.Description);
                return Result.Failure(ErrorCode.AUTH_PASSWORD_CHANGE_FAILD, errorMessages);
            }

            return Result.Success();
        }

        return Result.Failure(ErrorCode.AUTH_PASSWORD_CHANGE_FAILD, ["Oops! Something went wrong."]);
    }

    public async Task Logout(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user != null)
            await tokenService.ClearRefreshToken(user.Id);
    }


    private async Task<LoginResponse> GenerateAuthResponse(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken();

        await tokenService.AssignRefreshToken(user.Id, refreshToken);

        return user.ToLoginResponse(roles.First(), accessToken, refreshToken);
    }

    private async Task<User?> GetOrCreateUser(ExternalUserInfo externalUser)
    {
        var user = await userManager.FindByEmailAsync(externalUser.Email);

        if (user is null)
        {
            user = new User
            {
                FirstName = externalUser.FirstName,
                LastName = externalUser.LastName,
                ProfilePictureUrl = externalUser.Picture,
                UserName = externalUser.Email,
                Email = externalUser.Email,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return null;
            }

            var roleResult = await userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return null;
            }
        }

        return user;
    }
}
