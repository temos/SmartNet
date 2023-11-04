using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace SmartNet.Handlers;
public class ConfigurationHandler : ILifecycleHandler
{
    const string FirstPageId = "1";

    private readonly ILogger<ConfigurationHandler> _logger;
    private readonly SmartNetOptions _options;

    public ConfigurationHandler(ILogger<ConfigurationHandler> logger, IOptions<SmartNetOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var phase = doc.RootElement.GetProperty("configurationData").GetProperty("phase").GetString();
        _logger.LogInformation("Configuration phase {Phase}", phase);

        if (phase == "INITIALIZE")
            return await HandleInitializeAsync(doc);

        if (phase == "PAGE")
            return await HandlePageAsync(doc);

        _logger.LogError("Unknown configuration phase {Phase}", phase);
        return Results.StatusCode(503);
    }

    async Task<IResult> HandleInitializeAsync(JsonDocument doc)
    {
        var configurationJson = $$"""
            {
              "configurationData": {
                "initialize": {
                  "name": "{{_options.AppName}}",
                  "description": "{{_options.AppDescription}}",
                  "id": "{{_options.AppName.ToLower().Replace(" ", "")}}",
                  "permissions": [{{string.Join(",", _options.Permissions.Select(p => $"\"{p}\""))}}],
                  "firstPageId": "{{FirstPageId}}"
                }
              }
            }
            """;

        return Results.Extensions.JsonText(configurationJson);
    }

    async Task<IResult> HandlePageAsync(JsonDocument doc)
    {
        var pageId = doc.RootElement.GetProperty("configurationData").GetProperty("pageId").GetString();
        _logger.LogInformation("Configuration Page {PageId}", pageId);

        if (pageId == FirstPageId)
        {
            var page1Json = """
                {
                    "configurationData": {
                        "page": {
                            "pageId": "1",
                            "name": "My Cloud SmartApp",
                            "nextPageId": null,
                            "previousPageId": null,
                            "complete": true,
                            "sections": []
                        }
                    }
                }
                """;

            return Results.Extensions.JsonText(page1Json);
        }

        _logger.LogError("Unknown page {PageId}", pageId);
        return Results.StatusCode(503);
    }
}
