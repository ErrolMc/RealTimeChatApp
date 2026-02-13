using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Avalonia.ReactiveUI;
using ChatAppFrontEnd;
using ChatAppFrontEnd.Source.Utils;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        ServiceConfig.BackendUri = BuildConfig.BackendUri;
        ServiceConfig.SignalRUri = BuildConfig.SignalRUri;

        return BuildAvaloniaApp()
            .WithInterFont()
            .UseReactiveUI()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}