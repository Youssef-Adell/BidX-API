using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Services;

public interface IAuthService
{
    Task<AppResult> Register(RegisterRequest registerRequest, string userRole = "User");
    Task SendConfirmationEmail(string email, string urlOfConfirmationEndpoint);
    Task<bool> ConfirmEmail(string userId, string token);
    Task<AppResult<LoginResponse>> Login(LoginRequest loginRequest);
    Task<AppResult<LoginResponse>> Refresh(string refreshToken);
    Task SendPasswordResetEmail(string email, string urlOfPasswordResetPage);
    Task<AppResult> ResetPassword(ResetPasswordRequest resetPasswordRequest);
    Task<AppResult> ChangePassword(int userId, ChangePasswordRequest changePasswordRequest);
    /// <summary>
    /// Note that this method invalidate refresh token in the DB but the issued access tokens is still valid until its lifetime ends, so you must issue short-lived access tokens, i know it is better to revoke it completely but unfortunately this how bearer tokens works. (https://stackoverflow.com/a/26076022)
    /// </summary>
    Task RevokeRefreshToken(int userId);
}
