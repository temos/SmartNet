using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SmartNet.Handlers;
public class OAuthHandler : ILifecycleHandler
{
    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        throw new NotImplementedException();
    }
}
