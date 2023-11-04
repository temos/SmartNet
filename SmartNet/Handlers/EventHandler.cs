using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace SmartNet.Handlers;
public class EventHandler : ILifecycleHandler
{
    private readonly ILogger<EventHandler> _logger;
    private readonly HttpClient _httpClient;
    private readonly EventSender _eventSender;
    private readonly EventHandlerRegistry _eventHandlerRegistry;
    private readonly IServiceProvider _services;

    public EventHandler(ILogger<EventHandler> logger, HttpClient httpClient, EventHandlerRegistry eventHandlerRegistry, EventSender eventSender, IServiceProvider services)
    {
        _logger = logger;
        _httpClient = httpClient;
        _eventHandlerRegistry = eventHandlerRegistry;
        _services = services;
        _eventSender = eventSender;
    }

    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var eventData = doc.RootElement.GetProperty("eventData");
        var authToken = eventData.GetString("authToken");

        var events = eventData.GetProperty("events");
        using var eventEnumerator = events.EnumerateArray();

        while (eventEnumerator.MoveNext())
        {
            var @event = eventEnumerator.Current;
            var type = @event.GetString("eventType");

            if (type == "DEVICE_COMMANDS_EVENT")
                return await HandleDeviceCommandsEvent(authToken, @event.GetProperty("deviceCommandsEvent"));

            _logger.LogError("Unsupported event {EventType}", @event.GetRawText());
            return Results.StatusCode(503);
        }

        return Results.Extensions.JsonText("""{"eventData": {}}""");
    }

    async Task<IResult> HandleDeviceCommandsEvent(string authToken, JsonElement doc)
    {
        var deviceId = doc.GetString("deviceId");
        var externalId = doc.GetString("externalId");

        var commands = doc.GetProperty("commands");
        using var commandsEnumerator = commands.EnumerateArray();
        while (commandsEnumerator.MoveNext())
        {
            var command = commandsEnumerator.Current;
            var component = command.GetString("componentId");
            var capability = command.GetString("capability");
            var commandString = command.GetString("command");
            var arguments = command.GetProperty("arguments");

            var context = new CommandContext(
                _eventSender,
                _services,
                deviceId,
                externalId,
                authToken,
                component,
                capability,
                commandString,
                arguments);

            _logger.LogInformation("Received Command {DeviceId}/{Component}/{Capability} {Command}", deviceId, component, capability, commandString);

            var handler = _eventHandlerRegistry.ResolveCommandHandler(component, capability);
            await handler(context);
            await context.SendQueuedEventsAsync();
        }

        return Results.Ok();
    }

}