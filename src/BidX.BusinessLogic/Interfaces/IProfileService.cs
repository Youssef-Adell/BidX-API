using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.ProfileDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IProfilesService
{
    Task<Result<ProfileResponse>> GetProfile(int userId);
    Task UpdateProfile(int userId, ProfileUpdateRequest request);
    Task<Result<UpdatedProfilePictureResponse>> UpdateProfilePicture(int userId, Stream profilePicture);
}
