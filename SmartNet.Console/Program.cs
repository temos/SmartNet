using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SmartNet;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, logging) =>
    {
        const string logFormat = "[{Timestamp:HH:mm:ss} {Level:u3}] ({RequestId}) {Message:ij}{NewLine}{Exception}";
        logging.WriteTo
            .Console(outputTemplate: logFormat)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
    })
    .AddSmartNet(c =>
    {
        c.AppId = "00000000-0000-0000-0000-000000000000";
        c.AppName = "SmartNet Device";
        c.AppDescription = "SmartNet Test Device";
        c.Permissions.Add("r:devices:*");
        c.Permissions.Add("w:devices:*");
        c.Permissions.Add("r:locations:*");
        c.Permissions.Add("i:deviceprofiles:*");
    }, ConfigureCommandHandlers)
    .Build();

await host.RunAsync();

void ConfigureCommandHandlers(EventHandlerRegistry registry)
{
    registry.HandleCommand("switch", async context =>
    {
        context.QueueEvent("switch", context.Command);
        Console.WriteLine("SWITCH TOGGLE TO " + context.Command);
    });

    registry.HandleCommand("notification", async context =>
    {
        Console.WriteLine("NOTIFICATION " + context.GetArgumentString(0));
    });

    registry.HandleCommand("refresh", async context =>
    {
        context.QueueEvent("battery", "battery", value: Random.Shared.Next(1, 101));
        context.QueueEvent("switch", "switch", value: "off");
        context.QueueEvent("temperatureMeasurement", "temperature", Random.Shared.NextSingle() * 100, "C");
        context.QueueEvent("imageCapture", "encrypted", value: false);
        context.QueueEvent("imageCapture", "captureTime", value: "2022-06-19T14:20:02Z");
        context.QueueEvent("imageCapture", "image", value: "https://www.valtra.com/content/dam/Brands/Valtra/en/NewsandEvents/News/2022/reddot2022/Red%20Dot%202022_1600x900.jpg");
    });

    registry.HandleCommand("imageCapture", async context =>
    {
        Console.WriteLine("capturing image");
        context.QueueEvent("imageCapture", "encrypted", value: false);
        context.QueueEvent("imageCapture", "captureTime", value: "2022-06-19T14:20:02Z");
        context.QueueEvent("imageCapture", "image", value: "https://www.deere.com/assets/images/region-4/products/tractors/utility-tractors/product-slider/compacttractors-r4a067591-776x436.jpg");
    });

    //events.Add(new Event("main", "colorControl", "color", "#00FF00"));
    //events.Add(new Event("main", "colorControl", "saturation", 100));
    //events.Add(new Event("main", "colorControl", "hue", 300));
}