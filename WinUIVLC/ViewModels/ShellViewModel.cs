using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

using WinUIVLC.Contracts.Services;

namespace WinUIVLC.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    public ICommand MenuFileExitCommand
    {
        get;
    }

    public ICommand MenuSettingsCommand
    {
        get;
    }

    public ICommand MenuViewsMainCommand
    {
        get;
    }

    public INavigationService NavigationService
    {
        get;
    }

    public IWindowPresenterService _windowPresenterService
    {
        get;
    }

    public bool IsNotFullScreen => !_windowPresenterService.IsFullScreen;

    public ShellViewModel(INavigationService navigationService, IWindowPresenterService windowPresenterService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;

        _windowPresenterService = windowPresenterService;
        _windowPresenterService.WindowPresenterChanged += OnWindowPresenterChanged;

        MenuFileExitCommand = new RelayCommand(OnMenuFileExit);
        MenuSettingsCommand = new RelayCommand(OnMenuSettings);
        MenuViewsMainCommand = new RelayCommand(OnMenuViewsMain);
    }

    private void OnWindowPresenterChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsNotFullScreen));
    }

    private void OnNavigated(object sender, NavigationEventArgs e) => IsBackEnabled = NavigationService.CanGoBack;

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);

    private void OnMenuViewsMain() => NavigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!);
}
