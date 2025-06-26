namespace Backend.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Controller til håndtering af webhook tests og notifikationer
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(HttpClient httpClient, ILogger<WebhookController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Test Discord webhook med en simpel besked
    /// </summary>
    /// <param name="request">Test besked data</param>
    /// <returns>Test resultat</returns>
    /// <response code="200">Webhook test succesfult</response>
    /// <response code="400">Ugyldig data</response>
    /// <response code="500">Webhook fejl</response>
    [HttpPost("test-discord")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> TestDiscordWebhook([FromBody] TestWebhookRequest request)
    {
        try
        {
            // Standard webhook URL - kan gøres konfigurerbar senere
            const string webhookUrl = "https://discordapp.com/api/webhooks/1375568530157732022/GHAArdAKPRChIm4UbxSEjWRwOfkJlmlR5vvWT6a5f0XWsCOwaRFr8XasXG8ARuyyMb1W";

            var payload = new
            {
                content = request.Message ?? "Test besked fra Mercantec-Space Backend",
                username = request.Username ?? "Mercantec Bot",
                avatar_url = request.AvatarUrl,
                embeds = request.CreateEmbed ? new[]
                {
                    new
                    {
                        title = "🧪 Webhook Test",
                        description = request.Message ?? "Dette er en test besked fra admin panelet",
                        color = 0x3b82f6, // Blå farve
                        timestamp = DateTime.UtcNow.ToString("o"),
                        footer = new
                        {
                            text = "Mercantec-Space",
                            icon_url = "https://cdn.discordapp.com/icons/1351185531836436541/a_01234567890123456789012345678901.gif"
                        },
                        fields = new[]
                        {
                            new
                            {
                                name = "Test Type",
                                value = "Admin Panel Webhook Test",
                                inline = true
                            },
                            new
                            {
                                name = "Tidspunkt",
                                value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                                inline = true
                            }
                        }
                    }
                } : null
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook test sendt succesfuldt til Discord");
                return Ok(new { message = "Webhook test sendt succesfuldt!", status = "success" });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Discord webhook fejlede: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return BadRequest(new { message = $"Discord webhook fejlede: {response.StatusCode}", error = errorContent });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under webhook test");
            return StatusCode(500, new { message = "Der opstod en fejl under webhook test", error = ex.Message });
        }
    }

    /// <summary>
    /// Send notifikation til Discord webhook
    /// </summary>
    /// <param name="request">Notifikations data</param>
    /// <returns>Send resultat</returns>
    /// <response code="200">Notifikation sendt succesfuldt</response>
    /// <response code="400">Ugyldig data</response>
    /// <response code="500">Send fejl</response>
    [HttpPost("notify")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        try
        {
            const string webhookUrl = "https://discordapp.com/api/webhooks/1375568530157732022/GHAArdAKPRChIm4UbxSEjWRwOfkJlmlR5vvWT6a5f0XWsCOwaRFr8XasXG8ARuyyMb1W";

            var embed = new
            {
                title = request.Title,
                description = request.Description,
                color = GetColorFromType(request.Type),
                timestamp = DateTime.UtcNow.ToString("o"),
                footer = new
                {
                    text = "Mercantec-Space Admin",
                    icon_url = "https://cdn.discordapp.com/icons/1351185531836436541/a_01234567890123456789012345678901.gif"
                },
                thumbnail = request.Type switch
                {
                    "success" => new { url = "https://cdn.discordapp.com/emojis/✅.png" },
                    "warning" => new { url = "https://cdn.discordapp.com/emojis/⚠️.png" },
                    "error" => new { url = "https://cdn.discordapp.com/emojis/❌.png" },
                    "info" => new { url = "https://cdn.discordapp.com/emojis/ℹ️.png" },
                    _ => null
                }
            };

            var payload = new
            {
                username = "Mercantec Admin",
                avatar_url = "https://cdn.discordapp.com/icons/1351185531836436541/a_01234567890123456789012345678901.gif",
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Notifikation sendt til Discord: {Title}", request.Title);
                return Ok(new { message = "Notifikation sendt succesfuldt!", status = "success" });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Discord notifikation fejlede: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return BadRequest(new { message = $"Discord notifikation fejlede: {response.StatusCode}", error = errorContent });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under sending af notifikation");
            return StatusCode(500, new { message = "Der opstod en fejl under sending af notifikation", error = ex.Message });
        }
    }

    /// <summary>
    /// Hent webhook information
    /// </summary>
    /// <returns>Webhook detaljer</returns>
    /// <response code="200">Webhook information hentet</response>
    [HttpGet("info")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> GetWebhookInfo()
    {
        try
        {
            const string webhookUrl = "https://discordapp.com/api/webhooks/1375568530157732022/GHAArdAKPRChIm4UbxSEjWRwOfkJlmlR5vvWT6a5f0XWsCOwaRFr8XasXG8ARuyyMb1W";

            var response = await _httpClient.GetAsync(webhookUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var webhookInfo = JsonSerializer.Deserialize<object>(content);
                
                _logger.LogInformation("Webhook information hentet succesfuldt");
                return Ok(webhookInfo);
            }
            else
            {
                return BadRequest(new { message = $"Kunne ikke hente webhook info: {response.StatusCode}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under hentning af webhook information");
            return StatusCode(500, new { message = "Der opstod en fejl under hentning af webhook information" });
        }
    }

    /// <summary>
    /// Send struktureret announcement til Discord med templates
    /// </summary>
    /// <param name="request">Announcement data</param>
    /// <returns>Send resultat</returns>
    /// <response code="200">Announcement sendt succesfuldt</response>
    /// <response code="400">Ugyldig data</response>
    /// <response code="500">Send fejl</response>
    [HttpPost("announce")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult> SendAnnouncement([FromBody] AnnouncementRequest request)
    {
        try
        {
            const string webhookUrl = "https://discordapp.com/api/webhooks/1375568530157732022/GHAArdAKPRChIm4UbxSEjWRwOfkJlmlR5vvWT6a5f0XWsCOwaRFr8XasXG8ARuyyMb1W";

            var embed = CreateAnnouncementEmbed(request);
            
            var payload = new
            {
                content = GetContentForType(request.Type),
                username = GetUsernameForType(request.Type),
                avatar_url = GetAvatarForType(request.Type),
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Announcement sendt til Discord: {Type} - {Title}", request.Type, request.Title);
                return Ok(new { message = "Announcement sendt succesfuldt!", status = "success" });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Discord announcement fejlede: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return BadRequest(new { message = $"Discord announcement fejlede: {response.StatusCode}", error = errorContent });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under sending af announcement");
            return StatusCode(500, new { message = "Der opstod en fejl under sending af announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Send virksomheds-opslag til Discord
    /// </summary>
    /// <param name="request">Virksomheds opslag data</param>
    /// <returns>Send resultat</returns>
    /// <response code="200">Virksomheds-opslag sendt succesfuldt</response>
    /// <response code="400">Ugyldig data</response>
    /// <response code="500">Send fejl</response>
    [HttpPost("company-post")]
    [Authorize(Roles = "Admin,Teacher,Developer")]
    public async Task<ActionResult> SendCompanyPost([FromBody] CompanyPostRequest request)
    {
        try
        {
            const string webhookUrl = "https://discordapp.com/api/webhooks/1375568530157732022/GHAArdAKPRChIm4UbxSEjWRwOfkJlmlR5vvWT6a5f0XWsCOwaRFr8XasXG8ARuyyMb1W";

            var embed = new
            {
                title = $"🏢 {request.CompanyName} - {request.JobTitle}",
                description = request.Description,
                color = 0x059669, // Grøn farve for virksomheds-opslag
                timestamp = DateTime.UtcNow.ToString("o"),
                thumbnail = !string.IsNullOrEmpty(request.CompanyLogo) ? new { url = request.CompanyLogo } : null,
                image = !string.IsNullOrEmpty(request.ImageUrl) ? new { url = request.ImageUrl } : null,
                footer = new
                {
                    text = $"📍 {request.Location} • Mercantec-Space",
                    icon_url = "https://cdn.discordapp.com/icons/1351185531836436541/a_01234567890123456789012345678901.gif"
                },
                fields = new List<object>
                {
                    new
                    {
                        name = "💼 Stilling",
                        value = request.JobTitle,
                        inline = true
                    },
                    new
                    {
                        name = "🏢 Virksomhed",
                        value = request.CompanyName,
                        inline = true
                    },
                    new
                    {
                        name = "📍 Lokation",
                        value = request.Location ?? "Ikke angivet",
                        inline = true
                    }
                }
                .Concat(request.SalaryRange != null ? new[]
                {
                    new
                    {
                        name = "💰 Løn",
                        value = request.SalaryRange,
                        inline = true
                    }
                } : Array.Empty<object>())
                .Concat(request.WorkType != null ? new[]
                {
                    new
                    {
                        name = "⏰ Arbejdstype",
                        value = request.WorkType,
                        inline = true
                    }
                } : Array.Empty<object>())
                .Concat(request.RequiredSkills?.Any() == true ? new[]
                {
                    new
                    {
                        name = "🛠️ Krævet færdigheder",
                        value = string.Join(", ", request.RequiredSkills),
                        inline = false
                    }
                } : Array.Empty<object>())
                .Concat(request.ApplicationDeadline.HasValue ? new[]
                {
                    new
                    {
                        name = "⏰ Ansøgningsfrist",
                        value = request.ApplicationDeadline.Value.ToString("dd/MM/yyyy"),
                        inline = true
                    }
                } : Array.Empty<object>())
                .Concat(!string.IsNullOrEmpty(request.ContactPerson) ? new[]
                {
                    new
                    {
                        name = "👤 Kontaktperson",
                        value = request.ContactPerson,
                        inline = true
                    }
                } : Array.Empty<object>())
                .Concat(!string.IsNullOrEmpty(request.ApplicationLink) ? new[]
                {
                    new
                    {
                        name = "🔗 Ansøg her",
                        value = $"[Ansøg nu]({request.ApplicationLink})",
                        inline = false
                    }
                } : Array.Empty<object>())
                .ToArray()
            };

            var payload = new
            {
                content = $"🚀 **Ny lærerplads tilgængelig!**\n\n{request.CompanyName} søger en {request.JobTitle}! 👨‍💼👩‍💼",
                username = "Mercantec Karriere Bot",
                avatar_url = "https://cdn.discordapp.com/emojis/1234567890123456789.png",
                embeds = new[] { embed }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Virksomheds-opslag sendt til Discord: {Company} - {Job}", request.CompanyName, request.JobTitle);
                return Ok(new { message = "Virksomheds-opslag sendt succesfuldt!", status = "success" });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Discord virksomheds-opslag fejlede: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return BadRequest(new { message = $"Discord virksomheds-opslag fejlede: {response.StatusCode}", error = errorContent });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl under sending af virksomheds-opslag");
            return StatusCode(500, new { message = "Der opstod en fejl under sending af virksomheds-opslag", error = ex.Message });
        }
    }

    /// <summary>
    /// Hent tilgængelige announcement templates
    /// </summary>
    /// <returns>Liste over templates</returns>
    /// <response code="200">Templates hentet succesfuldt</response>
    [HttpGet("templates")]
    [Authorize(Roles = "Admin,Teacher")]
    public ActionResult GetAnnouncementTemplates()
    {
        var templates = new[]
        {
            new
            {
                Type = "new_course",
                Name = "Nyt Fag",
                Description = "Annoncér et nyt fag eller kursus",
                Icon = "📚",
                Color = 0x3b82f6,
                RequiredFields = new[] { "title", "description", "startDate", "instructor" },
                OptionalFields = new[] { "duration", "prerequisites", "maxStudents", "location" }
            },
            new
            {
                Type = "internship",
                Name = "Lærerplads",
                Description = "Opslag om lærerpladser",
                Icon = "🎓",
                Color = 0x059669,
                RequiredFields = new[] { "companyName", "jobTitle", "description", "location" },
                OptionalFields = new[] { "salaryRange", "workType", "requiredSkills", "applicationDeadline", "contactPerson", "applicationLink" }
            },
            new
            {
                Type = "event",
                Name = "Event",
                Description = "Arrangementer og events",
                Icon = "🎉",
                Color = 0x8b5cf6,
                RequiredFields = new[] { "title", "description", "eventDate", "location" },
                OptionalFields = new[] { "maxParticipants", "registrationLink", "organizer", "price" }
            },
            new
            {
                Type = "news",
                Name = "Nyheder",
                Description = "Generelle nyheder og opdateringer",
                Icon = "📰",
                Color = 0x6b7280,
                RequiredFields = new[] { "title", "description" },
                OptionalFields = new[] { "author", "readMoreLink", "category" }
            },
            new
            {
                Type = "urgent",
                Name = "Vigtig Besked",
                Description = "Vigtige beskeder der kræver opmærksomhed",
                Icon = "🚨",
                Color = 0xef4444,
                RequiredFields = new[] { "title", "description" },
                OptionalFields = new[] { "deadline", "actionRequired", "contactInfo" }
            }
        };

        return Ok(templates);
    }

    // Helper methods
    private object CreateAnnouncementEmbed(AnnouncementRequest request)
    {
        var baseEmbed = new
        {
            title = $"{GetIconForType(request.Type)} {request.Title}",
            description = request.Description,
            color = GetColorFromAnnouncementType(request.Type),
            timestamp = DateTime.UtcNow.ToString("o"),
            thumbnail = !string.IsNullOrEmpty(request.ThumbnailUrl) ? new { url = request.ThumbnailUrl } : null,
            image = !string.IsNullOrEmpty(request.ImageUrl) ? new { url = request.ImageUrl } : null,
            footer = new
            {
                text = "Mercantec-Space",
                icon_url = "https://cdn.discordapp.com/icons/1351185531836436541/a_01234567890123456789012345678901.gif"
            }
        };

        var fields = new List<object>();

        // Type-specific fields
        switch (request.Type.ToLower())
        {
            case "new_course":
                if (request.StartDate.HasValue)
                    fields.Add(new { name = "📅 Start dato", value = request.StartDate.Value.ToString("dd/MM/yyyy"), inline = true });
                if (!string.IsNullOrEmpty(request.Instructor))
                    fields.Add(new { name = "👨‍🏫 Instruktør", value = request.Instructor, inline = true });
                if (!string.IsNullOrEmpty(request.Duration))
                    fields.Add(new { name = "⏱️ Varighed", value = request.Duration, inline = true });
                if (!string.IsNullOrEmpty(request.Prerequisites))
                    fields.Add(new { name = "📋 Forudsætninger", value = request.Prerequisites, inline = false });
                if (request.MaxStudents.HasValue)
                    fields.Add(new { name = "👥 Max elever", value = request.MaxStudents.Value.ToString(), inline = true });
                break;

            case "event":
                if (request.EventDate.HasValue)
                    fields.Add(new { name = "📅 Event dato", value = request.EventDate.Value.ToString("dd/MM/yyyy HH:mm"), inline = true });
                if (!string.IsNullOrEmpty(request.Location))
                    fields.Add(new { name = "📍 Lokation", value = request.Location, inline = true });
                if (request.MaxParticipants.HasValue)
                    fields.Add(new { name = "👥 Max deltagere", value = request.MaxParticipants.Value.ToString(), inline = true });
                if (!string.IsNullOrEmpty(request.Organizer))
                    fields.Add(new { name = "👤 Arrangør", value = request.Organizer, inline = true });
                if (!string.IsNullOrEmpty(request.Price))
                    fields.Add(new { name = "💰 Pris", value = request.Price, inline = true });
                if (!string.IsNullOrEmpty(request.RegistrationLink))
                    fields.Add(new { name = "🔗 Tilmelding", value = $"[Tilmeld dig her]({request.RegistrationLink})", inline = false });
                break;

            case "urgent":
                if (request.Deadline.HasValue)
                    fields.Add(new { name = "⏰ Deadline", value = request.Deadline.Value.ToString("dd/MM/yyyy HH:mm"), inline = true });
                if (!string.IsNullOrEmpty(request.ActionRequired))
                    fields.Add(new { name = "⚡ Handling påkrævet", value = request.ActionRequired, inline = false });
                if (!string.IsNullOrEmpty(request.ContactInfo))
                    fields.Add(new { name = "📞 Kontakt", value = request.ContactInfo, inline = true });
                break;

            case "news":
                if (!string.IsNullOrEmpty(request.Author))
                    fields.Add(new { name = "✍️ Forfatter", value = request.Author, inline = true });
                if (!string.IsNullOrEmpty(request.Category))
                    fields.Add(new { name = "📂 Kategori", value = request.Category, inline = true });
                if (!string.IsNullOrEmpty(request.ReadMoreLink))
                    fields.Add(new { name = "📖 Læs mere", value = $"[Læs hele artiklen]({request.ReadMoreLink})", inline = false });
                break;
        }

        return new
        {
            baseEmbed.title,
            baseEmbed.description,
            baseEmbed.color,
            baseEmbed.timestamp,
            baseEmbed.thumbnail,
            baseEmbed.image,
            baseEmbed.footer,
            fields = fields.ToArray()
        };
    }

    private static string GetIconForType(string type) => type.ToLower() switch
    {
        "new_course" => "📚",
        "internship" => "🎓",
        "event" => "🎉",
        "news" => "📰",
        "urgent" => "🚨",
        _ => "ℹ️"
    };

    private static string GetContentForType(string type) => type.ToLower() switch
    {
        "new_course" => "📚 **Nyt fag tilgængeligt!** 🎓",
        "internship" => "🎓 **Ny lærerplads!** 💼",
        "event" => "🎉 **Nyt event!** 🗓️",
        "news" => "📰 **Nyheder fra Mercantec!** ✨",
        "urgent" => "🚨 **VIGTIG BESKED** 🚨",
        _ => "ℹ️ **Opdatering fra Mercantec** ℹ️"
    };

    private static string GetUsernameForType(string type) => type.ToLower() switch
    {
        "new_course" => "Mercantec Undervisning",
        "internship" => "Mercantec Karriere",
        "event" => "Mercantec Events",
        "news" => "Mercantec Nyheder",
        "urgent" => "Mercantec Admin",
        _ => "Mercantec Bot"
    };

    private static string GetAvatarForType(string type) => type.ToLower() switch
    {
        "new_course" => "https://cdn.discordapp.com/emojis/📚.png",
        "internship" => "https://cdn.discordapp.com/emojis/🎓.png",
        "event" => "https://cdn.discordapp.com/emojis/🎉.png",
        "news" => "https://cdn.discordapp.com/emojis/📰.png",
        "urgent" => "https://cdn.discordapp.com/emojis/🚨.png",
        _ => "https://cdn.discordapp.com/icons/1351185531836436541/a_01234567890123456789012345678901.gif"
    };

    private static int GetColorFromAnnouncementType(string type) => type.ToLower() switch
    {
        "new_course" => 0x3b82f6,  // Blå
        "internship" => 0x059669,  // Grøn
        "event" => 0x8b5cf6,       // Lilla
        "news" => 0x6b7280,       // Grå
        "urgent" => 0xef4444,     // Rød
        _ => 0x3b82f6             // Standard blå
    };

    private static int GetColorFromType(string type)
    {
        return type?.ToLower() switch
        {
            "success" => 0x10b981, // Grøn
            "warning" => 0xf59e0b, // Gul
            "error" => 0xef4444,   // Rød
            "info" => 0x3b82f6,    // Blå
            _ => 0x6b7280          // Grå
        };
    }
}

/// <summary>
/// Request til struktureret announcement
/// </summary>
public class AnnouncementRequest
{
    /// <summary>Type af announcement (new_course, internship, event, news, urgent)</summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>Titel på announcement</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Beskrivelse</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Thumbnail URL</summary>
    public string? ThumbnailUrl { get; set; }
    
    /// <summary>Billede URL</summary>
    public string? ImageUrl { get; set; }
    
    // New Course fields
    public DateTime? StartDate { get; set; }
    public string? Instructor { get; set; }
    public string? Duration { get; set; }
    public string? Prerequisites { get; set; }
    public int? MaxStudents { get; set; }
    
    // Event fields
    public DateTime? EventDate { get; set; }
    public string? Location { get; set; }
    public int? MaxParticipants { get; set; }
    public string? Organizer { get; set; }
    public string? Price { get; set; }
    public string? RegistrationLink { get; set; }
    
    // News fields
    public string? Author { get; set; }
    public string? Category { get; set; }
    public string? ReadMoreLink { get; set; }
    
    // Urgent fields
    public DateTime? Deadline { get; set; }
    public string? ActionRequired { get; set; }
    public string? ContactInfo { get; set; }
}

/// <summary>
/// Request til virksomheds-opslag
/// </summary>
public class CompanyPostRequest
{
    /// <summary>Virksomhedens navn</summary>
    public string CompanyName { get; set; } = string.Empty;
    
    /// <summary>Job titel</summary>
    public string JobTitle { get; set; } = string.Empty;
    
    /// <summary>Job beskrivelse</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Lokation</summary>
    public string? Location { get; set; }
    
    /// <summary>Løn range</summary>
    public string? SalaryRange { get; set; }
    
    /// <summary>Arbejdstype (fuldtid, deltid, praktik)</summary>
    public string? WorkType { get; set; }
    
    /// <summary>Krævet færdigheder</summary>
    public List<string>? RequiredSkills { get; set; }
    
    /// <summary>Ansøgningsfrist</summary>
    public DateTime? ApplicationDeadline { get; set; }
    
    /// <summary>Kontaktperson</summary>
    public string? ContactPerson { get; set; }
    
    /// <summary>Ansøgnings-link</summary>
    public string? ApplicationLink { get; set; }
    
    /// <summary>Virksomheds logo URL</summary>
    public string? CompanyLogo { get; set; }
    
    /// <summary>Billede URL</summary>
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Request til webhook test
/// </summary>
public class TestWebhookRequest
{
    /// <summary>
    /// Besked der skal sendes
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Brugernavn for webhook
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Avatar URL for webhook
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Om der skal oprettes et embed
    /// </summary>
    public bool CreateEmbed { get; set; } = true;
}

/// <summary>
/// Request til notifikation
/// </summary>
public class NotificationRequest
{
    /// <summary>
    /// Titel på notifikationen
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Beskrivelse af notifikationen
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Type af notifikation (success, warning, error, info)
    /// </summary>
    public string Type { get; set; } = "info";
} 