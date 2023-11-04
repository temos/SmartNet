using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SmartNet.Handlers;
using System.Text.Json;
using EventHandler = SmartNet.Handlers.EventHandler;

namespace SmartNet;
public static class HostBuilderExtensions
{
    public static IHostBuilder AddSmartNet(this IHostBuilder hostBuilder, Action<SmartNetOptions> configure, Action<EventHandlerRegistry> configureHandlers, int localPort = 8080)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services
                .AddOptions<SmartNetOptions>()
                .Configure(options =>
                {
                    options.WebHookPath = "/st";
                })
                .Configure(configure);

            services.AddHttpClient();

            services
                .AddScoped<RequestHandler>()
                .AddScoped<ConfirmationHandler>()
                .AddScoped<ConfigurationHandler>()
                .AddScoped<InstallHandler>()
                .AddScoped<UpdateHandler>()
                .AddScoped<EventHandler>()
                .AddScoped<OAuthHandler>()
                .AddScoped<UninstallHandler>();

            services.AddSingleton<EventSender>();

            var registry = new EventHandlerRegistry();
            configureHandlers(registry);
            services.AddSingleton(registry);
        });

        hostBuilder.ConfigureWebHostDefaults(web =>
        {
            web.UseUrls("http://0.0.0.0:" + localPort);
            web.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(MapSmartNet);
            });
        });

        return hostBuilder;
    }

    static void MapSmartNet(this IEndpointRouteBuilder app)
    {
        var options = app.ServiceProvider.GetRequiredService<IOptions<SmartNetOptions>>();

        app.MapPost(options.Value.WebHookPath, async (Stream bodyStream, RequestHandler handler) =>
        {
            using var jsonDoc = await JsonDocument.ParseAsync(bodyStream);
            return await handler.HandleAsync(jsonDoc);
        });
    }
}
