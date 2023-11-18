using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Serilog;
using WinRT;
using WinUIVLC.Contracts.Services;
using WinUIVLC.ViewModels;

namespace WinUIVLC.Activation;

public class FileActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;
    private readonly ILogger _log;

    public FileActivationHandler(INavigationService navigationService, ILogger log)
    {
        _navigationService = navigationService;
        _log = log;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        var eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        return eventArgs?.Kind == ExtendedActivationKind.File;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _log.Information("Activation handled by {0}", typeof(FileActivationHandler));

        var eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        var files = ((Windows.ApplicationModel.Activation.FileActivatedEventArgs)eventArgs.Data).Files;
        _log.Information("Files list: {@Files}", files.Select(x=> x.Path));

        _navigationService.NavigateTo(typeof(MainViewModel).FullName!, files);

        await Task.CompletedTask;
    }
}
