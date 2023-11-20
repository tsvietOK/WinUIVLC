using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using Serilog;
using Serilog.Events;
using Windows.ApplicationModel;
using Windows.Storage;
using WinUIVLC.Activation;
using WinUIVLC.Contracts.Services;
using WinUIVLC.Core.Contracts.Services;
using WinUIVLC.Core.Services;
using WinUIVLC.Helpers;
using WinUIVLC.Models;
using WinUIVLC.Notifications;
using WinUIVLC.Services;
using WinUIVLC.ViewModels;
using WinUIVLC.Views;

namespace WinUIVLC;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();
            services.AddTransient<IActivationHandler, FileActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPlaybackService, PlaybackService>();
            services.AddSingleton<IWindowPresenterService, WindowPresenterService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<VideoPlayerViewModel>();
            services.AddTransient<VideoPlayerPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        })
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.MinimumLevel.Debug().MinimumLevel
            .Override("Microsoft", LogEventLevel.Information).Enrich.FromLogContext().WriteTo
            .File(Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "logs/application.log")), rollingInterval: RollingInterval.Day);
        })
        .Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        if (e.Exception != null)
        {
            Log.Error(e.Exception, "Unhandled exception: {Exception}");
        }
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        Log.Information("Application Launched, version {Version}", GetAppVersion());

        //App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    public static string GetAppVersion()
    {
        var package = Package.Current;
        var packageId = package.Id;
        var version = packageId.Version;

        return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
    }
}
