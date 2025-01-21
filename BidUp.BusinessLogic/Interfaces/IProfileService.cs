using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.ProfileDTOs;

namespace BidUp.BusinessLogic.Interfaces;

public interface IProfilesService
{
    Task<Result<ProfileResponse>> GetProfile(int userId);
    Task UpdateProfile(int userId, ProfileUpdateRequest request);
    Task<Result<UpdatedProfilePictureResponse>> UpdateProfilePicture(int userId, Stream profilePicture);
}
