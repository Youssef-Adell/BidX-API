using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Mappings;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BidX.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;
    private readonly AppDbContext appDbContext;
    private readonly IEmailService emailService;
    private readonly IConfiguration configuration;

    public AuthService(UserManager<User> userManager, AppDbContext appDbContext, IEmailService emailService, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.appDbContext = appDbContext;
        this.emailService = emailService;
        this.configuration = configuration;
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
            user.RefreshToken = CreateRefreshToken(); // Assign a refresh token to the user to be saved while confirming the email

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(error => error.Description);
                return Result<LoginResponse>.Failure(ErrorCode.AUTH_EMAIL_CONFIRMATION_FAILD, errorMessages);
            }

            return Result<LoginResponse>.Success(await CreateLoginResponse(user));
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


        user.AccessFailedCount = 0; // Reset failed attempts counter on successful login
        user.RefreshToken = CreateRefreshToken();

        await appDbContext.SaveChangesAsync();

        return Result<LoginResponse>.Success(await CreateLoginResponse(user));
    }

    public async Task<Result<LoginResponse>> Refresh(string? refreshToken)
    {
        var user = userManager.Users.SingleOrDefault(user => user.RefreshToken == refreshToken && user.RefreshToken != null);
        if (user is null)
            return Result<LoginResponse>.Failure(ErrorCode.AUTH_INVALID_REFRESH_TOKEN, ["Invalid refresh token."]);

        return Result<LoginResponse>.Success(await CreateLoginResponse(user));
    }

    public async Task<Result<LoginResponse>> LoginWithGoogle(LoginWithGoogleRequest request)
    {
        GoogleJsonWebSignature.Payload idTokenPayload;

        // Vlidate the Id Token
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")!]
            };

            idTokenPayload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException ex)
        {
            return Result<LoginResponse>.Failure(ErrorCode.AUTH_EXTERNAL_LOGIN_FAILED, [ex.Message]);
        }

        // Attempt to get the user by email.
        var user = await userManager.FindByEmailAsync(idTokenPayload.Email);

        // Register the user if he is not exist.
        if (user is null)
        {
            user = await CreateUserWithGoogleCredentials(idTokenPayload);
            if (user is null)
                return Result<LoginResponse>.Failure(ErrorCode.AUTH_EXTERNAL_LOGIN_FAILED, ["Faild to signup with google."]);
        }
        // Otherwise assign a refresh token for him before return the LoginResponse
        else
        {
            user.RefreshToken = CreateRefreshToken();
            await appDbContext.Users
                .Where(u => u.Id == user.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.RefreshToken, user.RefreshToken));
        }

        // Create and return the login response.
        return Result<LoginResponse>.Success(await CreateLoginResponse(user));
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

    public async Task RevokeRefreshToken(int userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user != null)
        {
            user.RefreshToken = null;
            await userManager.UpdateAsync(user);
        }
    }


    private async Task<User?> CreateUserWithGoogleCredentials(GoogleJsonWebSignature.Payload payload)
    {
        var user = new User
        {
            FirstName = payload.GivenName,
            LastName = payload.FamilyName,
            ProfilePictureUrl = payload.Picture,
            UserName = payload.Email,
            Email = payload.Email,
            EmailConfirmed = true,
            RefreshToken = CreateRefreshToken(),
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
            return null;

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return null;
        }

        return user;
    }

    private async Task<LoginResponse> CreateLoginResponse(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = CreateAccessToken(user, roles);

        return user.ToLoginResponse(roles.First(), accessToken);
    }

    private string CreateRefreshToken()
    {
        var randomNumber = new Byte[32];

        using (var randomNumberGenerator = RandomNumberGenerator.Create())
        {
            // fills the byte array(randomNumber) with a cryptographically strong random sequence of values.
            randomNumberGenerator.GetBytes(randomNumber);
        }

        var refreshToken = Convert.ToBase64String(randomNumber);  // Converting the byte array to a base64 string to ensures that the refresh token is in a format that can be easily transmitted or stored in the database

        return refreshToken;
    }

    private string CreateAccessToken(User user, IEnumerable<string> roles)
    {
        var jwtToken = new JwtSecurityToken(
            claims: GetClaims(user, roles),
            signingCredentials: GetSigningCredentials(),
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:AccessTokenExpirationTimeInMinutes"])) //there is no diffrerence between using DateTime.UtcNow and DateTime.Now because it is converted to epoch timestamp format anyway
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return token;
    }

    private List<Claim> GetClaims(User user, IEnumerable<string> roles)
    {
        //JwtRegisteredClaimNames vs ClaimTypes see this https://stackoverflow.com/questions/50012155/jwt-claim-names , https://stackoverflow.com/questions/68252520/httpcontext-user-claims-doesnt-match-jwt-token-sub-changes-to-nameidentifie, https://stackoverflow.com/questions/57998262/why-is-claimtypes-nameidentifier-not-mapping-to-sub
        var claims = new List<Claim>
        {
            // new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var secretkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("BIDX_JWT_SECRET_KEY")!));
        return new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);
    }
}
