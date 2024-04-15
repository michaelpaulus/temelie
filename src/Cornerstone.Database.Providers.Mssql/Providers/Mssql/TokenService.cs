using System.Collections.Concurrent;
using Azure.Core;
using Azure.Identity;

namespace Temelie.Database.Providers.Mssql;

internal class TokenService
{
    private static readonly ConcurrentDictionary<string, AccessToken> _tokenCache = new ConcurrentDictionary<string, AccessToken>();
    private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

    public static string GetToken(string resource)
    {
        AccessToken tryGetToken()
        {
            if (_tokenCache.TryGetValue(resource, out var token))
            {
                if (token.ExpiresOn.DateTime >= DateTime.UtcNow.AddMinutes(5))
                {
                    return token;
                }
            }
            return default;
        }

        var response = tryGetToken();
        if (!string.IsNullOrEmpty(response.Token))
        {
            return response.Token;
        }

        try
        {
            _semaphoreSlim.Wait();

            response = tryGetToken();
            if (!string.IsNullOrEmpty(response.Token))
            {
                return response.Token;
            }

            var credential = new DefaultAzureCredential(true);
            var newToken = credential.GetToken(new TokenRequestContext(new[] { resource }), default);

            if (_tokenCache.TryGetValue(resource, out var existingToken))
            {
                _tokenCache.TryUpdate(resource, newToken, existingToken);
            }
            else
            {
                _tokenCache.TryAdd(resource, newToken);
            }

            return newToken.Token;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

}
