using Amazon.S3;
using Amazon.S3.Model;
using Backend.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Backend.Services;

/// <summary>Uploader event-bannere til S3/MinIO og returnerer offentlig URL.</summary>
public sealed class EventBannerStorageService : IDisposable
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
    };

    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly ILogger<EventBannerStorageService> _logger;
    private readonly S3MediaOptions _options;
    private readonly IAmazonS3? _client;

    public EventBannerStorageService(IOptions<S3MediaOptions> options, ILogger<EventBannerStorageService> logger)
    {
        _logger = logger;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || string.IsNullOrWhiteSpace(_options.AccessKey)
            || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            _logger.LogInformation("S3Media er ikke fuldt konfigureret — banner-upload er deaktiveret.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            _logger.LogWarning("S3Media:PublicBaseUrl mangler — upload returnerer ikke brugbare browser-URL'er.");
        }

        var endpoint = _options.Endpoint.TrimEnd('/');
        var cfg = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = _options.Region,
        };

        _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, cfg);
        _logger.LogInformation("S3/MinIO klient initialiseret mod {Endpoint}, bucket {Bucket}.", endpoint, _options.Bucket);
    }

    public bool IsEnabled => _client != null;

    public async Task<string> UploadBannerAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Fil-lagring er ikke konfigureret.");
        }

        if (file.Length == 0 || file.Length > MaxBytes)
        {
            throw new ArgumentException($"Filen skal være mellem 1 byte og {MaxBytes / 1024 / 1024} MB.");
        }

        if (string.IsNullOrEmpty(file.ContentType) || !AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Tilladte typer: JPEG, PNG, GIF, WebP.");
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || ext.Length > 8)
        {
            ext = file.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".bin",
            };
        }

        ext = ext.ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
        {
            throw new ArgumentException("Ugyldig filendelse.");
        }

        var key = $"events/banners/{Guid.NewGuid():N}{ext}";
        await using var stream = file.OpenReadStream();

        var put = new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
            AutoCloseStream = true,
            // MinIO / nogle S3-kompatible endpoints kræver ofte unsigned payload ved stream-upload.
            DisablePayloadSigning = true,
        };

        await _client.PutObjectAsync(put, cancellationToken);

        var publicBase = (_options.PublicBaseUrl ?? _options.Endpoint)!.TrimEnd('/');
        var url = $"{publicBase}/{_options.Bucket}/{key}";
        _logger.LogInformation("Banner uploadet: {Key} ({Bytes} bytes)", key, file.Length);
        return url;
    }

    public void Dispose() => (_client as IDisposable)?.Dispose();
}
