using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using WinUIVLC.Contracts.Services;
using WinUIVLC.Services;
using WinUIVLC.Views;

namespace WinUIVLC.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    [ObservableProperty]
    private NavigationViewPaneDisplayMode paneDisplayMode = NavigationViewPaneDisplayMode.Auto;

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

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public bool IsNotFullScreen => !_windowPresenterService.IsFullScreen;

    public ShellViewModel(INavigationService navigationService, IWindowPresenterService windowPresenterService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

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

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(VideoPlayerPage))
        {
            PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
        }
        else
        {
            PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
        }

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    private void OnMenuFileExit() => Application.Current.Exit();

    private void OnMenuSettings() => NavigationService.NavigateTo(typeof(SettingsViewModel).FullName!);

    private void OnMenuViewsMain() => NavigationService.NavigateTo(typeof(VideoPlayerViewModel).FullName!);
}
