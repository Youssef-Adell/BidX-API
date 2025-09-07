using BidX.BusinessLogic.DTOs.AuthDTOs;
using BidX.BusinessLogic.Interfaces;
using Google.Apis.Auth;

namespace BidX.BusinessLogic.Services;

public class GoogleAuthProvider : IAuthProvider
{
    public string ProviderName => "Google";

    public async Task<ExternalUserInfo?> Authenticate(string idToken)
    {
        GoogleJsonWebSignature.Payload idTokenPayload;

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")!]
            };

            idTokenPayload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            var user = new ExternalUserInfo
            {
                FirstName = idTokenPayload.GivenName,
                LastName = idTokenPayload.FamilyName,
                Picture = idTokenPayload.Picture,
                Email = idTokenPayload.Email,
            };

            return user;
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }

}
