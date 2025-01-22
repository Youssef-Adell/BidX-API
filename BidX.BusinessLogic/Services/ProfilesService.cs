using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.ProfileDTOs;
using BidX.BusinessLogic.Interfaces;
using BidX.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

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


    public async Task<Result<ProfileResponse>> GetProfile(int userId)
    {
        var userProfile = await appDbContext.Users
        .ProjectTo<ProfileResponse>(mapper.ConfigurationProvider)
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
