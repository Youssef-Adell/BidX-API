using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IAuthService
{
    Task<Result> Register(RegisterRequest request, string userRole = "User");
    Task SendConfirmationEmail(string email);
    Task<Result<LoginResponse>> ConfirmEmail(ConfirmEmailRequest request);
    Task<Result<LoginResponse>> Login(LoginRequest request);
    Task<Result<LoginResponse>> LoginWithGoogle(LoginWithGoogleRequest request);
    Task<Result<LoginResponse>> Refresh(string? refreshToken);
    Task SendPasswordResetEmail(string email);
    Task<Result> ResetPassword(ResetPasswordRequest request);
    Task<Result> ChangePassword(int userId, ChangePasswordRequest request);
    /// <summary>
    /// Note that this method invalidate refresh token in the DB but the issued access tokens is still valid until its lifetime ends, so you must issue short-lived access tokens, i know it is better to revoke it completely but unfortunately this how bearer tokens works. (https://stackoverflow.com/a/26076022)
    /// </summary>
    Task RevokeRefreshToken(int userId);
}
