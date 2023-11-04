using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SmartNet.Handlers;
public class UninstallHandler : ILifecycleHandler
{
    public async Task<IResult> HandleAsync(JsonDocument doc)
    {
        var installedApp = doc.RootElement.GetProperty("uninstallData").GetProperty("installedApp");
        var appId = installedApp.GetString("installedAppId");
        var locationId = installedApp.GetString("locationId");


        var uninstallJson = """
        {
            "uninstallData": {}
        }
        """;

        return Results.Extensions.JsonText(uninstallJson);
    }
}
