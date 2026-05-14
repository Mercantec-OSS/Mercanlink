using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly EventBannerStorageService _storage;
    private readonly ILogger<MediaController> _logger;

    public MediaController(EventBannerStorageService storage, ILogger<MediaController> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <summary>
    /// Upload et banner-billede til objektlager (S3/MinIO). Returnerer en URL der kan gemmes i BannerImageUrl.
    /// </summary>
    [HttpPost("events/banner")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> UploadEventBanner(IFormFile file, CancellationToken cancellationToken)
    {
        if (!_storage.IsEnabled)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Fil-lagring (S3/MinIO) er ikke konfigureret på serveren." });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vælg en billedfil." });
        }

        try
        {
            var url = await _storage.UploadBannerAsync(file, cancellationToken);
            return Ok(new { url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Banner-upload fejlede");
            return StatusCode(500, new { message = "Upload fejlede. Prøv igen senere." });
        }
    }
}
