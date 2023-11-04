using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SmartNet.Handlers;
public interface ILifecycleHandler
{
    Task<IResult> HandleAsync(JsonDocument doc);
}
