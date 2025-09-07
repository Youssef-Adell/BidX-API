namespace BidX.BusinessLogic.Interfaces;

public interface IAuthProviderFactory
{
    IAuthProvider GetProvider(string providerName);
}
