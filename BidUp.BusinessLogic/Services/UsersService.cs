using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.UserProfileDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class UsersService : IUsersService
{
    private readonly AppDbContext appDbContext;
    private readonly ICloudService cloudService;

    public UsersService(AppDbContext appDbContext, ICloudService cloudService)
    {
        this.appDbContext = appDbContext;
        this.cloudService = cloudService;
    }


    public async Task<AppResult<UserProfileResponse>> GetUserProfile(int userId)
    {
        var userProfileResponse = await appDbContext.Users
        .Select(u => new UserProfileResponse
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            ProfilePictureUrl = u.ProfilePictureUrl,
            AverageRating = u.AverageRating
        })
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (userProfileResponse is null)
            return AppResult<UserProfileResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        return AppResult<UserProfileResponse>.Success(userProfileResponse);
    }

    public async Task UpdateUserProfile(int userId, UserProfileUpdateRequest userProfileUpdateRequest)
    {
        await appDbContext.Users
                  .Where(u => u.Id == userId)
                  .ExecuteUpdateAsync(setters => setters
                      .SetProperty(u => u.FirstName, userProfileUpdateRequest.FirstName)
                      .SetProperty(u => u.LastName, userProfileUpdateRequest.LastName));
    }

    public async Task<AppResult<UpdatedProfilePictureResponse>> UpdateUserProfilePicture(int userId, Stream profilePicture)
    {
        var uploadResult = await cloudService.UploadThumbnail(profilePicture);

        if (!uploadResult.Succeeded)
            return AppResult<UpdatedProfilePictureResponse>.Failure(uploadResult.Error!.ErrorCode, uploadResult.Error.ErrorMessages);

        var profilePictureUrl = uploadResult.Response!.FileUrl;

        await appDbContext.Users
          .Where(u => u.Id == userId)
          .ExecuteUpdateAsync(setters => setters
              .SetProperty(u => u.ProfilePictureUrl, profilePictureUrl));

        return AppResult<UpdatedProfilePictureResponse>.Success(new() { ProfilePictureUrl = profilePictureUrl });
    }
}
