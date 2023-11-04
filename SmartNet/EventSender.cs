using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartNet;
public class EventSender
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventSender> _logger;

    public EventSender(HttpClient httpClient, ILogger<EventSender> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task SendEventsAsync(CommandContext context, params Event[] events) => SendEventsCoreAsync(context.DeviceId, context.AuthToken, events);
    public Task SendEventsAsync(CommandContext context, IEnumerable<Event> events) => SendEventsCoreAsync(context.DeviceId, context.AuthToken, events);
    public Task SendEventsAsync(string deviceId, string authToken, params Event[] events) => SendEventsCoreAsync(deviceId, authToken, events);
    public Task SendEventsAsync(string deviceId, string authToken, IEnumerable<Event> events) => SendEventsCoreAsync(deviceId, authToken, events);

    async Task SendEventsCoreAsync(string deviceId, string authToken, IEnumerable<Event> events)
    {
        _logger.LogInformation("Sending {Count} events", events.Count());
        var json = CreateEventJson(events);

        var req = new HttpRequestMessage(HttpMethod.Post, $"https://api.smartthings.com/v1/devices/{deviceId}/events");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        req.Headers.Add("Authorization", $"Bearer {authToken}");

        var resp = await _httpClient.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Failed sending events {ErrorJson}", await resp.Content.ReadAsStringAsync());
            resp.EnsureSuccessStatusCode();
        }
    }

    string CreateEventJson(IEnumerable<Event> events)
    {
        var first = true;
        var jsonBuilder = new StringBuilder();

        jsonBuilder.Append(""" {"deviceEvents": [ """);

        foreach (var ev in events)
        {
            if (!first)
                jsonBuilder.Append(',');

            jsonBuilder.Append('{');
            jsonBuilder.Append($""" "component": "{ev.Component}" """);
            jsonBuilder.Append($""" ,"capability": "{ev.Capability}" """);
            jsonBuilder.Append($""" ,"attribute": "{ev.Attribute}" """);

            var stringValue = ev.Value switch
            {
                string => $""" "{ev.Value}" """,
                int or float => $""" {ev.Value} """,
                true => "true",
                false => "false",
                string[] arr => $""" [{string.Join(",", arr.Select(s => $"\"{s}\""))}] """,
                _ => throw new NotImplementedException($"Unsupported value type '{ev?.Value?.GetType()?.FullName ?? "null"}'")
            };
            jsonBuilder.Append($""" ,"value": {stringValue} """);

            if (ev.Unit != null)
                jsonBuilder.Append($""" ,"unit": "{ev.Unit}" """);

            jsonBuilder.Append('}');
            first = false;
        }

        jsonBuilder.Append("]}");

        return jsonBuilder.ToString();
    }
}

public record Event(string Component, string Capability, string Attribute, object Value, string Unit = null);
public record MainComponentEvent(string Capability, string Attribute, object Value, string Unit = null) : Event("main", Capability, Attribute, Value, Unit);
