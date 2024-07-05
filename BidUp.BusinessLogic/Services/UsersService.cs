using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.UserProfileDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class UsersService : IUsersService
{
    private readonly AppDbContext appDbContext;

    public UsersService(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;

    }


    public async Task<AppResult<UserProfileResponse>> GetUserProfile(int userId)
    {
        var userProfileResponse = await appDbContext.Users
        .Select(u => new UserProfileResponse
        {
            Id = u.Id,
            Name = $"{u.FirstName} {u.LastName}",
            ProfilePictureUrl = u.ProfilePictureUrl,
        })
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Id == userId);

        if (userProfileResponse is null)
            return AppResult<UserProfileResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["User not found."]);

        return AppResult<UserProfileResponse>.Success(userProfileResponse);
    }

}
