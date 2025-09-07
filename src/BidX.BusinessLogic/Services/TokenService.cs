using BidX.BusinessLogic.Interfaces;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BidX.BusinessLogic.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration configuration;
    private readonly AppDbContext appDbContext;

    public TokenService(IConfiguration configuration, AppDbContext appDbContext)
    {
        this.configuration = configuration;
        this.appDbContext = appDbContext;
    }

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var jwtToken = new JwtSecurityToken(
            claims: GetClaims(user, roles),
            signingCredentials: GetSigningCredentials(),
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:AccessTokenExpirationTimeInMinutes"])) //there is no diffrerence between using DateTime.UtcNow and DateTime.Now because it is converted to epoch timestamp format anyway
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return token;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        using (var randomNumberGenerator = RandomNumberGenerator.Create())
        {
            // fills the byte array(randomNumber) with a cryptographically strong random sequence of values.
            randomNumberGenerator.GetBytes(randomNumber);
        }

        var refreshToken = Convert.ToBase64String(randomNumber);  // Converting the byte array to a base64 string to ensures that the refresh token is in a format that can be easily transmitted or stored in the database

        return refreshToken;
    }

    public async Task AssignRefreshToken(int userId, string refreshToken)
    {
        await appDbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.RefreshToken, refreshToken));
    }

    public async Task ClearRefreshToken(int userId)
    {
        await appDbContext.Users
             .Where(u => u.Id == userId)
             .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.RefreshToken, (string?)null));
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
