using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SmartNet.Handlers;
public class ConfirmationHandler : ILifecycleHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConfigurationHandler> _logger;

    public ConfirmationHandler(HttpClient httpClient, ILogger<ConfigurationHandler> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var confirmationUrl = doc.RootElement
            .GetProperty("confirmationData")
            .GetProperty("confirmationUrl")
            .GetString();

        _logger.LogInformation("Sending confirmation request");
        await _httpClient.GetAsync(confirmationUrl);

        return Results.Ok();
    }
}
