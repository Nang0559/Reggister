using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/public-information")]
public sealed class PublicInformationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PublicInformationController(IConfiguration configuration) => _configuration = configuration;

    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public IActionResult Get()
    {
        var items = _configuration.GetSection("PublicInformation:Items").GetChildren()
            .Select(x => new
            {
                id = x["Id"] ?? string.Empty,
                type = x["Type"] ?? "Announcement",
                title = x["Title"] ?? string.Empty,
                summary = x["Summary"] ?? string.Empty,
                content = x["Content"] ?? string.Empty,
                publishedAt = x["PublishedAt"] ?? string.Empty,
                important = bool.TryParse(x["Important"], out var important) && important,
                attachmentUrl = x["AttachmentUrl"] ?? string.Empty
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.title))
            .OrderByDescending(x => x.important)
            .ThenByDescending(x => x.publishedAt)
            .ToList();

        return Ok(items);
    }
}