namespace Backend.Config;

public class MercantecAuthConfig
{
    public string Issuer { get; set; } = "https://auth.mercantec.tech";
    public string Audience { get; set; } = "mercantec-apps";
    public string JwksUri { get; set; } = "https://auth.mercantec.tech/.well-known/jwks.json";
}
