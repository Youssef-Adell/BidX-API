using BidX.BusinessLogic.DTOs.AuthDTOs;

namespace BidX.BusinessLogic.Interfaces;

public interface IAuthProvider
{
    string ProviderName { get; }
    Task<ExternalUserInfo?> Authenticate(string idToken);
}
