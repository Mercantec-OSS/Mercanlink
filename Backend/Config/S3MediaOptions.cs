namespace Backend.Config;

/// <summary>
/// S3-kompatibel lagring (fx MinIO) til event-bannere m.m.
/// </summary>
public class S3MediaOptions
{
    public const string SectionName = "S3Media";

    /// <summary>Internt endpoint, fx http://minio:9000</summary>
    public string? Endpoint { get; set; }

    /// <summary>Offentlig base-URL som browsere kan loade billeder fra, fx http://localhost:9305</summary>
    public string? PublicBaseUrl { get; set; }

    public string? AccessKey { get; set; }

    public string? SecretKey { get; set; }

    public string Bucket { get; set; } = "events-media";

    public string Region { get; set; } = "us-east-1";
}
