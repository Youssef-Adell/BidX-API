using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.UserProfileDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IUsersService
{
    Task<AppResult<UserProfileResponse>> GetUserProfile(int userId);
}
