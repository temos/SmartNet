using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SmartNet.Handlers;
public class UpdateHandler : ILifecycleHandler
{
    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var updateData = doc.RootElement.GetProperty("updateData");
        var authToken = updateData.GetString("authToken");
        var refreshToken = updateData.GetString("refreshToken");

        var installedApp = updateData.GetProperty("installedApp");
        var appId = installedApp.GetString("installedAppId");
        var locationId = installedApp.GetString("locationId");

        return Results.Extensions.JsonText("""{"updateData": {}}""");
    }
}
