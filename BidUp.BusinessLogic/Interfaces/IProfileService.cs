using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.ProfileDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IProfilesService
{
    Task<AppResult<ProfileResponse>> GetProfile(int userId);
    Task UpdateProfile(int userId, ProfileUpdateRequest request);
    Task<AppResult<UpdatedProfilePictureResponse>> UpdateProfilePicture(int userId, Stream profilePicture);
}
