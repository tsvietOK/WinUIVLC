using System.Windows.Input;
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
    private string totalTimeString = "--:--:--";
    private string playStatusIcon = "\uE768";
    private TimeSpan totalTime = new(0, 0, 0);
    private string volumeIcon = "\uE767";
    private string filePath = "Empty";
    private long totalTimeLong;
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

        InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);
        PlayPauseCommand = new RelayCommand(PlayPause);
        StopCommand = new RelayCommand(Stop);
        MuteCommand = new RelayCommand(Mute);
        FullScreenCommand = new RelayCommand(FullScreen);
        RewindCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(Rewind);
        FastForwardCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(FastForward);
        VolumeUpCommand = new RelayCommand(VolumeUp);
        VolumeDownCommand = new RelayCommand(VolumeDown);
        ScrollChangedCommand = new RelayCommand<PointerRoutedEventArgs>(ScrollChanged);
        PointerMovedCommand = new RelayCommand<PointerRoutedEventArgs>(PointerMoved);

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

    public long TotalTimeLong
    {
        get => totalTimeLong;
        set
        {
            if (SetProperty(ref totalTimeLong, value))
            {
                TotalTime = TimeSpan.FromMilliseconds(value);
            }
        }
    }

    public TimeSpan TotalTime
    {
        get => totalTime;
        set
        {
            if (SetProperty(ref totalTime, value))
            {
                TotalTimeString = totalTime.ToString(@"hh\:mm\:ss");
            }
        }
    }

    public string TotalTimeString
    {
        get => totalTimeString;
        set => SetProperty(ref totalTimeString, value);
    }

    public string PlayStatusIcon
    {
        get => playStatusIcon;
        set => SetProperty(ref playStatusIcon, value);
    }

    public string VolumeIcon
    {
        get => volumeIcon;
        set => SetProperty(ref volumeIcon, value);
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

    private void Initialize(InitializedEventArgs eventArgs)
    {
        LibVLC = new LibVLC(true, eventArgs.SwapChainOptions);
        Player = new MediaPlayer(LibVLC);

        if (FilePath == "Empty")
        {
            return;
        }

        var media = new Media(LibVLC, new Uri(FilePath));
        Player.Play(media);
        MediaPlayerWrapper = new ObservableMediaPlayerWrapper(Player, _dispatcherQueue);

        Player.Playing += Player_Playing;
        Player.Media.DurationChanged += Media_DurationChanged;
        Player.MediaChanged += Player_MediaChanged;
        Player.Paused += Player_Paused;
        Player.Stopped += Player_Stopped;
        Player.VolumeChanged += Player_VolumeChanged;
    }

    private void Player_VolumeChanged(object? sender, MediaPlayerVolumeChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdateVolumeIcon();
        });

    }

    private void Player_MediaChanged(object? sender, MediaPlayerMediaChangedEventArgs e)
    {
        //_dispatcherQueue.TryEnqueue(() =>
        //{
        //    Volume = (int)e.Volume;
        //});
    }

    private void Media_DurationChanged(object? sender, MediaDurationChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            //TotalTime = TimeSpan.FromMilliseconds(e.Duration);
            TotalTimeLong = e.Duration;
        });
    }

    private void Player_Stopped(object? sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdatePlayIcon();
        });
    }

    private void Player_Paused(object? sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdatePlayIcon();
        });
    }

    private void Player_Playing(object? sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdatePlayIcon();
        });
    }

    private void PointerMoved(PointerRoutedEventArgs? args)
    {
        if (_windowPresenterService.IsFullScreen)
        {
            ShowControls();
            controlsHideTimer.Stop();
            controlsHideTimer.Start();
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
            controlsHideTimer.Tick -= Timer_Tick;
            controlsHideTimer.Stop();
            ShowControls();
        }

        OnPropertyChanged(nameof(IsNotFullScreen));
        OnPropertyChanged(nameof(ControlsVisibility));
        OnPropertyChanged(nameof(RowSpan));
    }

    private void ShowControls()
    {
        ControlsVisibility = Visibility.Visible;
    }

    private void HideControls()
    {
        ControlsVisibility = Visibility.Collapsed;
    }

    private void UpdateVolumeIcon()
    {
        if (MediaPlayerWrapper.Volume == 0)
        {
            VolumeIcon = "\uE74F";
        }
        else if (MediaPlayerWrapper.Volume > 0 && MediaPlayerWrapper.Volume <= 25)
        {
            VolumeIcon = "\uE992";
        }
        else if (MediaPlayerWrapper.Volume > 25 && MediaPlayerWrapper.Volume <= 50)
        {
            VolumeIcon = "\uE993";
        }
        else if (MediaPlayerWrapper.Volume > 50 && MediaPlayerWrapper.Volume <= 75)
        {
            VolumeIcon = "\uE994";
        }
        else
        {
            VolumeIcon = "\uE767";
        }
    }

    private void UpdatePlayIcon()
    {
        if (MediaPlayerWrapper.IsPlaying)
        {
            PlayStatusIcon = "\uE769";
        }
        else
        {
            PlayStatusIcon = "\uE768";
        }
    }

    private void PlayPause()
    {
        MediaPlayerWrapper?.PlayPause();
    }

    private void Stop()
    {
        MediaPlayerWrapper?.Stop();
    }

    private void Mute()
    {
        MediaPlayerWrapper?.Mute();
    }

    private void FullScreen()
    {
        _windowPresenterService.ToggleFullScreen();
    }


    private void VolumeDown()
    {
        MediaPlayerWrapper?.VolumeDown();
    }

    private void VolumeUp()
    {
        MediaPlayerWrapper?.VolumeUp();
    }

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

    private void FastForward(KeyboardAcceleratorInvokedEventArgs args)
    {
        var modifier = args.KeyboardAccelerator.Modifiers;
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
    }

    private void Rewind(KeyboardAcceleratorInvokedEventArgs args)
    {
        var modifier = args.KeyboardAccelerator.Modifiers;
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


    }

    public ICommand InitializedCommand
    {
        get; set;
    }

    public ICommand PlayPauseCommand
    {
        get; set;
    }

    public ICommand StopCommand
    {
        get; set;
    }

    public ICommand MuteCommand
    {
        get; set;
    }

    public ICommand ChangeProgressCommand
    {
        get; set;
    }

    public ICommand FullScreenCommand
    {
        get; set;
    }

    public ICommand RewindCommand
    {
        get; set;
    }

    public ICommand FastForwardCommand
    {
        get; set;
    }

    public ICommand VolumeUpCommand
    {
        get; set;
    }

    public ICommand VolumeDownCommand
    {
        get; set;
    }

    public ICommand ScrollChangedCommand
    {
        get; set;
    }

    public ICommand PointerMovedCommand
    {
        get; set;
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
