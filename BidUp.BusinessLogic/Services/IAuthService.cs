using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Services;

public interface IAuthService
{
    Task<AppResult> Register(RegisterRequest registerRequest, string userRole = "User");
    Task SendConfirmationEmail(string email, string urlOfConfirmationEndpoint);
    Task<bool> ConfirmEmail(string userId, string token);
    Task<AppResult<LoginResponse>> Login(LoginRequest loginRequest);

}
