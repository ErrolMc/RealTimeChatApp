using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using ChatAppFrontEnd;
using ChatAppFrontEnd.Source.Utils;
using ChatAppFrontEnd.Source.Services;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            // Read config injected into index.html by entrypoint.sh (Docker/Azure)
            var appConfig = JSHost.GlobalThis.GetPropertyAsJSObject("__APP_CONFIG__");
            ServiceConfig.BackendUri = appConfig.GetPropertyAsString("BackendUri");
            ServiceConfig.SignalRUri = appConfig.GetPropertyAsString("SignalRUri");
            Console.WriteLine($"[Config] Loaded from window.__APP_CONFIG__");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Config] No injected config found ({ex.Message}), using compile-time defaults");
            // Fallback to compile-time defaults (local dev without Docker)
            ServiceConfig.BackendUri = BuildConfig.BackendUri;
            ServiceConfig.SignalRUri = BuildConfig.SignalRUri;
        }
        Console.WriteLine($"[Config] BackendUri={ServiceConfig.BackendUri}, SignalRUri={ServiceConfig.SignalRUri}");

        await BuildAvaloniaApp()
            .WithInterFont()
            .UseReactiveUI()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
