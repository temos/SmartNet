using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace SmartNet.Handlers;
public class InstallHandler : ILifecycleHandler
{
    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var installData = doc.RootElement.GetProperty("installData");
        var authToken = installData.GetString("authToken");
        var refreshToken = installData.GetString("refreshToken");

        var installedApp = installData.GetProperty("installedApp");
        var appId = installedApp.GetString("installedAppId");
        var locationId = installedApp.GetString("locationId");

        var installReqJson = $$"""
            {
                "label": "Super Traktor",
                "locationId": "{{locationId}}",
                "app": {
                    "profileId": "7cf7f1dc-e209-4304-aa36-c39290160923",
                    "installedAppId": "{{appId}}",
                    "externalId": "traktor123"
                }
            }
            """;

        var _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        var result = await _httpClient.PostAsync("https://api.smartthings.com/v1/devices", new StringContent(installReqJson, Encoding.ASCII, "application/json"));
        result.EnsureSuccessStatusCode();

        return Results.Extensions.JsonText("""{"installData": {}}""");
    }
}
