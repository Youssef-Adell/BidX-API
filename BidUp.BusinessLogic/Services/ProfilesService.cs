using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.ProfileDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class ProfilesService : IProfilesService
{
    private readonly AppDbContext appDbContext;
    private readonly ICloudService cloudService;
    private readonly IMapper mapper;

    public ProfilesService(AppDbContext appDbContext, ICloudService cloudService, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.cloudService = cloudService;
        this.mapper = mapper;
    }


    public async Task<AppResult<ProfileResponse>> GetProfile(int userId)
    {
        var userProfile = await appDbContext.Users
        .ProjectTo<ProfileResponse>(mapper.ConfigurationProvider)
        .AsNoTracking()
        .SingleOrDefaultAsync(u => u.Id == userId);

        if (userProfile is null)
            return AppResult<ProfileResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        return AppResult<ProfileResponse>.Success(userProfile);
    }

    public async Task UpdateProfile(int userId, ProfileUpdateRequest request)
    {
        await appDbContext.Users
                  .Where(u => u.Id == userId)
                  .ExecuteUpdateAsync(setters => setters
                      .SetProperty(u => u.FirstName, request.FirstName)
                      .SetProperty(u => u.LastName, request.LastName));
    }

    public async Task<AppResult<UpdatedProfilePictureResponse>> UpdateProfilePicture(int userId, Stream profilePicture)
    {
        var uploadResult = await cloudService.UploadThumbnail(profilePicture);

        if (!uploadResult.Succeeded)
            return AppResult<UpdatedProfilePictureResponse>.Failure(uploadResult.Error!);

        var profilePictureUrl = uploadResult.Response!.FileUrl;

        await appDbContext.Users
          .Where(u => u.Id == userId)
          .ExecuteUpdateAsync(setters => setters
              .SetProperty(u => u.ProfilePictureUrl, profilePictureUrl));

        return AppResult<UpdatedProfilePictureResponse>.Success(new() { ProfilePictureUrl = profilePictureUrl });
    }
}
