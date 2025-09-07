using BidX.BusinessLogic.Interfaces;

namespace BidX.BusinessLogic.Services;

public class AuthProviderFactory : IAuthProviderFactory
{
    private readonly Dictionary<string, IAuthProvider> providers;

    public AuthProviderFactory(IEnumerable<IAuthProvider> providers)
    {
        this.providers = providers.ToDictionary(provider => provider.ProviderName, provider => provider, StringComparer.OrdinalIgnoreCase);
    }

    public IAuthProvider GetProvider(string providerName)
    {
        if (providers.TryGetValue(providerName, out var provider))
        {
            return provider;
        }

        throw new KeyNotFoundException($"Auth provider with name: {providerName} not found.");
    }
}
