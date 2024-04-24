using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.DataAccess.Entites;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BidUp.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> userManager;
    private readonly IEmailService emailService;
    private readonly IConfiguration configuration;

    public AuthService(UserManager<User> userManager, IEmailService emailService, IConfiguration configuration)
    {
        this.userManager = userManager;
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

    public async Task<AppResult<LoginResponse>> Login(LoginRequest loginRequest)
    {
        var user = await userManager.FindByEmailAsync(loginRequest.Email);
        if (user is null)
            return new AppResult<LoginResponse>(ErrorCode.AUTH_INVALID_USERNAME_OR_PASSWORD, "Invalid email or password.");

        if (await userManager.IsLockedOutAsync(user))
            return new AppResult<LoginResponse>(ErrorCode.AUTH_ACCOUNT_IS_LOCKED_OUT, "The account has been temporarily locked out.");

        if (!await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            await userManager.AccessFailedAsync(user);
            return new AppResult<LoginResponse>(ErrorCode.AUTH_INVALID_USERNAME_OR_PASSWORD, "Invalid email or password.");
        }

        if (!user.EmailConfirmed)
            return new AppResult<LoginResponse>(ErrorCode.AUTH_EMAIL_NOT_CONFIRMED, "The email has not been confirmed.");


        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresIn) = CreateAccessToken(user, roles);

        var loginResponse = new LoginResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            Role = roles.First(),
            AccessToken = token,
            ExpiresIn = expiresIn
        };

        return new AppResult<LoginResponse>(loginResponse);
    }


    private (string token, double expiresIn) CreateAccessToken(User user, IEnumerable<string> roles)
    {
        var jwtToken = new JwtSecurityToken(
            claims: GetClaims(user, roles),
            signingCredentials: GetSigningCredentials(),
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["JwtSettings:AccessTokenExpirationTimeInMinutes"])) //there is no diffrerence between using DateTime.UtcNow and DateTime.Now because it is converted to epoch timestamp format anyway
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        var expiresIn = Convert.ToDouble(configuration["JwtSettings:AccessTokenExpirationTimeInMinutes"]);

        return (token, expiresIn);
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
