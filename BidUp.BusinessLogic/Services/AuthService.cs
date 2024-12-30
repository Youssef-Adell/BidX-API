using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BidUp.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;
    private readonly AppDbContext appDbContext;
    private readonly IEmailService emailService;
    private readonly IConfiguration configuration;

    public AuthService(UserManager<User> userManager, AppDbContext appContext, IEmailService emailService, IConfiguration configuration)
    {
        this.userManager = userManager;
        this.appDbContext = appContext;
        this.emailService = emailService;
        this.configuration = configuration;
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
        {
            var errorMessages = creationResult.Errors.Select(error => error.Description);
            return AppResult.Failure(ErrorCode.AUTH_VIOLATE_REGISTER_RULES, errorMessages);
        }

        var addingRolesResult = await userManager.AddToRoleAsync(user, userRole);

        return AppResult.Success();
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

    public async Task<AppResult<LoginResponse>> Login(LoginRequest loginRequest)
    {
        var user = await userManager.FindByEmailAsync(loginRequest.Email);

        if (user is null)
        {
            return AppResult<LoginResponse>.Failure(
                ErrorCode.AUTH_INVALID_USERNAME_OR_PASSWORD,
                ["Invalid email or password."]);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return AppResult<LoginResponse>.Failure(
                ErrorCode.AUTH_ACCOUNT_IS_LOCKED_OUT,
                ["The account has been temporarily locked out."]);
        }

        if (!await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            await userManager.AccessFailedAsync(user);

            return AppResult<LoginResponse>.Failure(
                ErrorCode.AUTH_INVALID_USERNAME_OR_PASSWORD,
                ["Invalid email or password."]);
        }

        if (!user.EmailConfirmed)
        {
            return AppResult<LoginResponse>.Failure(
                ErrorCode.AUTH_EMAIL_NOT_CONFIRMED,
                ["The email has not been confirmed."]);
        }


        user.AccessFailedCount = 0; // Reset failed attempts counter on successful login
        user.RefreshToken = CreateRefreshToken();

        await appDbContext.SaveChangesAsync();

        return AppResult<LoginResponse>.Success(await CreateLoginResponseAsync(user));
    }

    public async Task<AppResult<LoginResponse>> Refresh(string? refreshToken)
    {
        var user = userManager.Users.SingleOrDefault(user => user.RefreshToken == refreshToken && user.RefreshToken != null);
        if (user is null)
            return AppResult<LoginResponse>.Failure(ErrorCode.AUTH_INVALID_REFRESH_TOKEN, ["Invalid refresh token."]);

        return AppResult<LoginResponse>.Success(await CreateLoginResponseAsync(user));
    }

    public async Task SendPasswordResetEmail(string email, string urlOfPasswordResetPage)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null && user.EmailConfirmed)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var urlOfPasswordResetPageForCurrentUser = $"{urlOfPasswordResetPage}?userId={user.Id}&token={token}";

            await emailService.SendPasswordResetEmail(email, urlOfPasswordResetPageForCurrentUser);
        }
    }

    public async Task<AppResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
    {
        var user = await userManager.FindByIdAsync(resetPasswordRequest.UserId);

        if (user != null && user.EmailConfirmed)
        {
            resetPasswordRequest.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordRequest.Token));

            var resetResult = await userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);

            if (!resetResult.Succeeded)
            {
                var errorMessages = resetResult.Errors.Select(error => error.Description);
                return AppResult.Failure(ErrorCode.AUTH_PASSWORD_RESET_FAILD, errorMessages);
            }

            return AppResult.Success();
        }

        return AppResult.Failure(ErrorCode.AUTH_PASSWORD_RESET_FAILD, ["Oops! Something went wrong."]);
    }

    public async Task<AppResult> ChangePassword(int userId, ChangePasswordRequest changePasswordRequest)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user != null)
        {
            var changingResult = await userManager.ChangePasswordAsync(user, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);

            if (!changingResult.Succeeded)
            {
                var errorMessages = changingResult.Errors.Select(error => error.Description);
                return AppResult.Failure(ErrorCode.AUTH_PASSWORD_CHANGE_FAILD, errorMessages);
            }

            return AppResult.Success();
        }

        return AppResult.Failure(ErrorCode.AUTH_PASSWORD_CHANGE_FAILD, ["Oops! Something went wrong."]);
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


    private async Task<LoginResponse> CreateLoginResponseAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = CreateAccessToken(user, roles);

        var loginResponse = new LoginResponse
        {
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = roles.First(),
            },
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken!
        };

        return loginResponse;
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
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["JwtSettings:AccessTokenExpirationTimeInMinutes"])) //there is no diffrerence between using DateTime.UtcNow and DateTime.Now because it is converted to epoch timestamp format anyway
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
        var secretkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("BIDUP_JWT_SECRET_KEY")!));
        return new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);
    }
}
