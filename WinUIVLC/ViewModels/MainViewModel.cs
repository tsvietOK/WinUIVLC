using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Platforms.Windows;
using LibVLCSharp.Shared;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Serilog;
using Windows.Storage;
using Windows.System;
using WinUIVLC.Contracts.Services;
using WinUIVLC.Contracts.ViewModels;
using WinUIVLC.Models.Enums;
using WinUIVLC.ViewModels.Wrappers;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace WinUIVLC.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;
    private readonly IWindowPresenterService _windowPresenterService;
    private readonly ILogger _log;

    private LibVLC libVLC;
    private MediaPlayer mediaPlayer;
    private string filePath = "Empty";
    private ObservableMediaPlayerWrapper mediaPlayerWrapper;
    private Visibility controlsVisibility;

    private readonly DispatcherTimer controlsHideTimer = new()
    {
        Interval = TimeSpan.FromSeconds(1),
    };

    public MainViewModel(INavigationService navigationService, IWindowPresenterService windowPresenterService, ILogger log)
    {
        _navigationService = navigationService;
        _windowPresenterService = windowPresenterService;
        _log = log;

        _windowPresenterService.WindowPresenterChanged += OnWindowPresenterChanged;

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    ~MainViewModel()
    {
        Dispose();
    }

    private LibVLC LibVLC
    {
        get => libVLC;
        set => libVLC = value;
    }

    public MediaPlayer Player
    {
        get => mediaPlayer;
        set => SetProperty(ref mediaPlayer, value);
    }

    public string FilePath
    {
        get => filePath;
        set => SetProperty(ref filePath, value);
    }

    public ObservableMediaPlayerWrapper MediaPlayerWrapper
    {
        get => mediaPlayerWrapper;
        set => SetProperty(ref mediaPlayerWrapper, value);
    }

    public bool IsNotFullScreen => !_windowPresenterService.IsFullScreen;

    public Visibility ControlsVisibility
    {
        get => controlsVisibility;
        set => SetProperty(ref controlsVisibility, value);
    }

    public int RowSpan => _windowPresenterService.IsFullScreen ? 2 : 1;

    public bool LoadPlayer => FilePath != "Empty";

    [RelayCommand]
    private void Initialized(InitializedEventArgs eventArgs)
    {
        if (FilePath == "Empty")
        {
            _log.Information("Skipping LibVLC initialization, because no media file specified.");
            return;
        }

        _log.Information("Initializing LibVLC");

        LibVLC = new LibVLC(true, eventArgs.SwapChainOptions);
        Player = new MediaPlayer(LibVLC);

        var media = new Media(LibVLC, new Uri(FilePath));
        Player.Play(media);
        _log.Information("Starting playback of '{0}'", FilePath);

        MediaPlayerWrapper = new ObservableMediaPlayerWrapper(Player, _dispatcherQueue, _log);
    }

    [RelayCommand]
    private void PointerMoved(PointerRoutedEventArgs? args)
    {
        if (_windowPresenterService.IsFullScreen)
        {
            if (ControlsVisibility == Visibility.Collapsed)
            {
                ShowControls();
            }
            else
            {
                controlsHideTimer.Stop();
                controlsHideTimer.Start();
            }
        }
    }

    private void Timer_Tick(object? sender, object e)
    {
        HideControls();
        controlsHideTimer.Stop();
    }

    private void OnWindowPresenterChanged(object? sender, EventArgs e)
    {
        if (sender is not IWindowPresenterService windowPresenter)
        {
            return;
        }

        if (windowPresenter.IsFullScreen)
        {
            controlsHideTimer.Tick += Timer_Tick;
        }
        else
        {
            controlsHideTimer.Stop();
            controlsHideTimer.Tick -= Timer_Tick;
            ShowControls();
        }

        OnPropertyChanged(nameof(IsNotFullScreen));
        OnPropertyChanged(nameof(ControlsVisibility));
        OnPropertyChanged(nameof(RowSpan));
    }

    private void ShowControls()
    {
        ControlsVisibility = Visibility.Visible;
        _log.Information("Showing controls");
    }

    private void HideControls()
    {
        ControlsVisibility = Visibility.Collapsed;
        _log.Information("Hiding controls");
    }

    [RelayCommand]
    private void PlayPause()
    {
        MediaPlayerWrapper?.PlayPause();
    }

    [RelayCommand]
    private void Stop()
    {
        MediaPlayerWrapper?.Stop();
    }

    [RelayCommand]
    private void Mute()
    {
        MediaPlayerWrapper?.Mute();
    }

    [RelayCommand]
    private void FullScreen()
    {
        _windowPresenterService.ToggleFullScreen();
    }

    [RelayCommand]
    private void VolumeDown()
    {
        MediaPlayerWrapper?.VolumeDown();
    }

    [RelayCommand]
    private void VolumeUp()
    {
        MediaPlayerWrapper?.VolumeUp();
    }

    [RelayCommand]
    private void ScrollChanged(PointerRoutedEventArgs args)
    {
        var delta = args.GetCurrentPoint(null).Properties.MouseWheelDelta;
        if (delta > 0)
        {
            MediaPlayerWrapper?.VolumeUp();
        }
        else
        {
            MediaPlayerWrapper?.VolumeDown();
        }
    }

    [RelayCommand]
    private void FastForward(object args)
    {
        if (args is KeyboardAcceleratorInvokedEventArgs keyboardAcceleratorInvokedEventArgs)
        {
            var modifier = keyboardAcceleratorInvokedEventArgs.KeyboardAccelerator.Modifiers;
            switch (modifier)
            {
                case VirtualKeyModifiers.None:
                case VirtualKeyModifiers.Menu://10s
                    MediaPlayerWrapper?.FastForward(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control://60s
                    MediaPlayerWrapper?.FastForward(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift://3s
                    MediaPlayerWrapper?.FastForward(RewindMode.Short);
                    break;
            }
            keyboardAcceleratorInvokedEventArgs.Handled = true;
        }
        else
        {
            MediaPlayerWrapper?.FastForward(RewindMode.Normal);
        }
    }

    [RelayCommand]
    private void Rewind(object args)
    {
        if (args is KeyboardAcceleratorInvokedEventArgs keyboardAcceleratorInvokedEventArgs)
        {
            var modifier = keyboardAcceleratorInvokedEventArgs.KeyboardAccelerator.Modifiers;
            switch (modifier)
            {
                case VirtualKeyModifiers.None:
                case VirtualKeyModifiers.Menu://10s
                    MediaPlayerWrapper?.Rewind(RewindMode.Normal);
                    break;
                case VirtualKeyModifiers.Control://60s
                    MediaPlayerWrapper?.Rewind(RewindMode.Long);
                    break;
                case VirtualKeyModifiers.Shift://3s
                    MediaPlayerWrapper?.Rewind(RewindMode.Short);
                    break;
            }
            keyboardAcceleratorInvokedEventArgs.Handled = true;
        }
        else
        {
            MediaPlayerWrapper?.Rewind(RewindMode.Normal);
        }
    }

    public void OnNavigatedTo(object parameter)
    {
        if (parameter is IReadOnlyList<IStorageItem> fileList)
        {
            var filePath = fileList.First().Path;
            FilePath = filePath;
        }
    }

    public void OnNavigatedFrom()
    {
        //Player.Playing -= Player_Playing;
        //Player.TimeChanged -= Player_TimeChanged;
        //Player.Media.DurationChanged -= Media_DurationChanged;
        //Player.MediaChanged -= Player_MediaChanged;
        //Player.Paused -= Player_Paused;
        //Player.Stopped -= Player_Stopped;
        //Player.VolumeChanged -= Player_VolumeChanged;
    }

    public void Dispose()
    {
        var mediaPlayer = Player;
        Player = null;
        mediaPlayer?.Dispose();
        LibVLC?.Dispose();
        LibVLC = null;
    }
}
