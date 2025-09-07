using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.DTOs.ProfileDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class UserMappingExtensions
{
    public static User ToUserEntity(this RegisterRequest request)
    {
        return new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = Guid.NewGuid().ToString() // because it needs a unique value and we dont want to ask user to enter it to make the register process easier, and if we set it to the email value it will give user 2 errors in case if the entered email is already taken, one for username and one for email
        };
    }

    public static LoginResponse ToLoginResponse(this User user, string role, string accessToken, string refreshToken)
    {
        return new LoginResponse
        {
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = role,
            },
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    public static IQueryable<ProfileResponse> ProjectToProfileResponse(this IQueryable<User> query)
    {
        return query.Select(u => new ProfileResponse
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            ProfilePictureUrl = u.ProfilePictureUrl,
            AverageRating = u.AverageRating
        });
    }

}
