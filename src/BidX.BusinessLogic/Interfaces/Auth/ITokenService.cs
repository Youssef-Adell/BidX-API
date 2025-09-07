using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    Task AssignRefreshToken(int userId, string refreshToken);

    /// <summary>
    /// Note that this method invalidate refresh token in the DB but the issued access tokens is still valid until its lifetime ends, so you must issue short-lived access tokens, i know it is better to revoke it completely but unfortunately this how bearer tokens works. (https://stackoverflow.com/a/26076022)
    /// </summary>
    Task ClearRefreshToken(int userId);
}
