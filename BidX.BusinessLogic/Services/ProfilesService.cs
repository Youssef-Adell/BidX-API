using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.ProfileDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Mappings;
using BidX.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class ProfilesService : IProfilesService
{
    private readonly AppDbContext appDbContext;
    private readonly ICloudService cloudService;

    public ProfilesService(AppDbContext appDbContext, ICloudService cloudService)
    {
        this.appDbContext = appDbContext;
        this.cloudService = cloudService;
    }


    public async Task<Result<ProfileResponse>> GetProfile(int userId)
    {
        var userProfile = await appDbContext.Users
        .ProjectToProfileResponse()
        .AsNoTracking()
        .SingleOrDefaultAsync(u => u.Id == userId);

        if (userProfile is null)
            return Result<ProfileResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        return Result<ProfileResponse>.Success(userProfile);
    }

    public async Task UpdateProfile(int userId, ProfileUpdateRequest request)
    {
        await appDbContext.Users
                  .Where(u => u.Id == userId)
                  .ExecuteUpdateAsync(setters => setters
                      .SetProperty(u => u.FirstName, request.FirstName)
                      .SetProperty(u => u.LastName, request.LastName));
    }

    public async Task<Result<UpdatedProfilePictureResponse>> UpdateProfilePicture(int userId, Stream profilePicture)
    {
        var uploadResult = await cloudService.UploadThumbnail(profilePicture);

        if (!uploadResult.Succeeded)
            return Result<UpdatedProfilePictureResponse>.Failure(uploadResult.Error!);

        var profilePictureUrl = uploadResult.Response!.FileUrl;

        await appDbContext.Users
          .Where(u => u.Id == userId)
          .ExecuteUpdateAsync(setters => setters
              .SetProperty(u => u.ProfilePictureUrl, profilePictureUrl));

        return Result<UpdatedProfilePictureResponse>.Success(new() { ProfilePictureUrl = profilePictureUrl });
    }
}
