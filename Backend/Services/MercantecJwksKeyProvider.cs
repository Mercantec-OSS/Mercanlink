namespace Backend.Services;

using Microsoft.IdentityModel.Tokens;

public class MercantecJwksKeyProvider
{
    private static readonly HttpClient HttpClient = new();
    private readonly string _jwksUri;
    private readonly object _syncRoot = new();
    private IReadOnlyCollection<SecurityKey> _cachedKeys = Array.Empty<SecurityKey>();
    private DateTime _nextRefreshUtc = DateTime.MinValue;

    public MercantecJwksKeyProvider(string jwksUri)
    {
        _jwksUri = jwksUri;
    }

    public IReadOnlyCollection<SecurityKey> GetSigningKeys()
    {
        lock (_syncRoot)
        {
            if (_cachedKeys.Count > 0 && DateTime.UtcNow < _nextRefreshUtc)
            {
                return _cachedKeys;
            }

            try
            {
                var jwksJson = HttpClient.GetStringAsync(_jwksUri).GetAwaiter().GetResult();
                var jwks = new JsonWebKeySet(jwksJson);
                _cachedKeys = jwks.GetSigningKeys().ToList();
                _nextRefreshUtc = DateTime.UtcNow.AddMinutes(10);
            }
            catch (Exception ex)
            {
                if (_cachedKeys.Count == 0)
                {
                    throw new InvalidOperationException($"Kunne ikke hente JWKS fra {_jwksUri}", ex);
                }
            }

            return _cachedKeys;
        }
    }
}
