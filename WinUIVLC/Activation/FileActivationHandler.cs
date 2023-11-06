using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinRT;
using WinUIVLC.Contracts.Services;
using WinUIVLC.ViewModels;

namespace WinUIVLC.Activation;

public class FileActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public FileActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        var eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        return eventArgs?.Kind == ExtendedActivationKind.File;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        var eventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        _navigationService.NavigateTo(typeof(MainViewModel).FullName!, ((Windows.ApplicationModel.Activation.FileActivatedEventArgs)eventArgs.Data).Files);

        await Task.CompletedTask;
    }
}
