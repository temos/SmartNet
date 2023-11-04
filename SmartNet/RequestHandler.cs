using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartNet.Handlers;
using System.Text.Json;
using EventHandler = SmartNet.Handlers.EventHandler;

namespace SmartNet;
public class RequestHandler
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RequestHandler> _logger;

    public RequestHandler(IServiceProvider services, ILogger<RequestHandler> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var lifecycle = GetLifecycle(doc);
        var handler = GetHandler(lifecycle);
        _logger.LogInformation("Handling {Lifecycle}", lifecycle);
        return await handler.HandleAsync(doc);
    }

    string GetLifecycle(JsonDocument doc)
    {
        return doc.RootElement.GetProperty("lifecycle").GetString();
    }

    ILifecycleHandler GetHandler(string lifecycle)
    {
        return lifecycle switch
        {
            "CONFIRMATION" => _services.GetRequiredService<ConfirmationHandler>(),
            "CONFIGURATION" => _services.GetRequiredService<ConfigurationHandler>(),
            "INSTALL" => _services.GetRequiredService<InstallHandler>(),
            "UPDATE" => _services.GetRequiredService<UpdateHandler>(),
            "EVENT" => _services.GetRequiredService<EventHandler>(),
            "OAUTH_CALLBACK" => _services.GetRequiredService<OAuthHandler>(),
            "UNINSTALL" => _services.GetRequiredService<UninstallHandler>(),
            _ => throw new NotImplementedException($"Lifecycle '{lifecycle}' is not supported")
        };
    }
}
