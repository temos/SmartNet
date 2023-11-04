using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartNet;
public class EventHandlerRegistry
{
    readonly Dictionary<string, STCommandHandler> _commandHandlers = new Dictionary<string, STCommandHandler>();

    public void HandleCommand(string capability, STCommandHandler handler)
        => HandleCommand("main", capability, handler);

    public void HandleCommand(string component, string capability, STCommandHandler handler)
    {
        _commandHandlers[CreateKey(component, capability)] = handler;
    }

    public STCommandHandler ResolveCommandHandler(string component, string capability)
    {
        return _commandHandlers[CreateKey(component, capability)];
    }

    string CreateKey(string component, string capability) => $"{component}/{capability}";
}

public record CommandContext(
    EventSender EventSender,
    IServiceProvider Services,
    string DeviceId,
    string ExternalDeviceId,
    string AuthToken,
    string Component,
    string Capability,
    string Command,
    JsonElement Arguments)
{
    readonly List<Event> _events = new List<Event>();

    public void QueueEvent(string component, string capability, string attribute, object value, string unit = null)
        => _events.Add(new Event(component, capability, attribute, value, unit));

    public void QueueEvent(string capability, string attribute, object value, string unit = null)
        => _events.Add(new Event(Component, capability, attribute, value, unit));

    public void QueueEvent(string attribute, object value, string unit = null)
        => _events.Add(new Event(Component, Capability, attribute, value, unit));

    public string GetArgumentString(int index) => Arguments[index].GetString();
    public int GetArgumentInt(int index) => Arguments[index].GetInt32();
    public float GetArgumentFloat(int index) => Arguments[index].GetSingle();

    public async Task SendQueuedEventsAsync()
    {
        if (_events.Count == 0)
            return;

        await EventSender.SendEventsAsync(this, _events);
        _events.Clear();
    }
}

public delegate Task STCommandHandler(CommandContext context);