using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using WinUIVLC.Contracts.Services;
using WinUIVLC.ViewModels;

namespace WinUIVLC.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;
    private readonly ILogger _log;

    public DefaultActivationHandler(INavigationService navigationService, ILogger log)
    {
        _navigationService = navigationService;
        _log = log;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        //var eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _log.Information("Activation handled by {0}", typeof(DefaultActivationHandler));

        _navigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}
