using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IAuthService
{
    Task<Result> Register(RegisterRequest request, string userRole = "User");
    Task SendConfirmationEmail(string email);
    Task<Result<LoginResponse>> ConfirmEmail(ConfirmEmailRequest request);
    Task<Result<LoginResponse>> Login(LoginRequest request);
    Task<Result<LoginResponse>> LoginWithExternalProvider(LoginWithExternalProviderRequest request);
    Task<Result<LoginResponse>> Refresh(string? refreshToken);
    Task SendPasswordResetEmail(string email);
    Task<Result> ResetPassword(ResetPasswordRequest request);
    Task<Result> ChangePassword(int userId, ChangePasswordRequest request);
    Task Logout(int userId);
}
