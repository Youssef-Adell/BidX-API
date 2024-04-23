using BidUp.BusinessLogic.DTOs.AuthDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;

namespace BidUp.BusinessLogic.Services;

public interface IAuthService
{
    public Task<AppResult> Register(RegisterRequest registerRequest, string userRole = "User");
}
