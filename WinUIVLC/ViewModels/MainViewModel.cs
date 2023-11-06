using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Platforms.Windows;
using LibVLCSharp.Shared;
using Windows.Storage;
using Windows.System;
using WinUIVLC.Contracts.Services;
using WinUIVLC.Contracts.ViewModels;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace WinUIVLC.ViewModels;

public partial class MainViewModel : ObservableRecipient, INavigationAware
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly INavigationService _navigationService;
    private LibVLC libVLC;
    private MediaPlayer mediaPlayer;
    private string elapsedTimeString = "--:--:--";
    private string totalTimeString = "--:--:--";
    private string playStatusIcon = "\uE768";
    private int volume;
    private double progress;
    private TimeSpan totalTime = new(0, 0, 0);
    private TimeSpan elapsedTime = new(0, 0, 0);
    private int previousVolume;
    private double elapsedTimeSeconds;
    private double totalTimeSeconds;
    private string volumeIcon = "\uE767";
    private string filePath = "Empty";

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

    public TimeSpan ElapsedTime
    {
        get => elapsedTime;
        set
        {
            if (SetProperty(ref elapsedTime, value))
            {
                ElapsedTimeString = elapsedTime.ToString(@"hh\:mm\:ss");
                ElapsedTimeSeconds = elapsedTime.TotalSeconds;
            }
        }
    }

    public string ElapsedTimeString
    {
        get => elapsedTimeString;
        set => SetProperty(ref elapsedTimeString, value);
    }

    public double ElapsedTimeSeconds
    {
        get => elapsedTimeSeconds;
        set => SetProperty(ref elapsedTimeSeconds, value);
    }

    public TimeSpan TotalTime
    {
        get => totalTime;
        set
        {
            if (SetProperty(ref totalTime, value))
            {
                TotalTimeString = totalTime.ToString(@"hh\:mm\:ss");
                TotalTimeSeconds = totalTime.TotalSeconds;
            }
        }
    }

    public string TotalTimeString
    {
        get => totalTimeString;
        set => SetProperty(ref totalTimeString, value);
    }

    public double TotalTimeSeconds
    {
        get => totalTimeSeconds;
        set => SetProperty(ref totalTimeSeconds, value);
    }

    public string PlayStatusIcon
    {
        get => playStatusIcon;
        set => SetProperty(ref playStatusIcon, value);
    }

    public int Volume
    {
        get => volume;
        set
        {
            if (SetProperty(ref volume, value))
            {
                Player.Volume = value;

                if (volume == 0)
                {
                    VolumeIcon = "\uE74F";
                }
                else
                {
                    VolumeIcon = "\uE767";
                }
            }
        }
    }

    public double Progress
    {
        get => progress;
        set
        {
            if (SetProperty(ref progress, value))
            {
                //Player.Time = new TimeSpan(0, 0, (int)progress).Ticks;
            }
        }
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

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);
        PlayPauseCommand = new RelayCommand(PlayPause);
        StopCommand = new RelayCommand(Stop);
        MuteCommand = new RelayCommand(Mute);
        ChangeProgressCommand = new RelayCommand<EventArgs>(ChangeProgress);
        FullScreenCommand = new RelayCommand(FullScreen);
        RewindCommand = new RelayCommand(Rewind);
        FastForwardCommand = new RelayCommand(FastForward);

        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    private void FastForward()
    {
        Player.Time += 10000;
    }

    private void Rewind()
    {
        Player.Time -= 10000;
    }

    ~MainViewModel()
    {
        Dispose();
    }

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
        Volume = Player.Volume;

        Player.Playing += Player_Playing;
        Player.TimeChanged += Player_TimeChanged;
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
            //Volume = (int)e.Volume;
            //OnPropertyChanged(nameof(Volume));
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
            TotalTime = TimeSpan.FromMilliseconds(e.Duration);
        });
    }

    private void Player_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        var currentTime = e.Time;
        _dispatcherQueue.TryEnqueue(() =>
        {
            ElapsedTime = TimeSpan.FromMilliseconds(currentTime);
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

    private void UpdatePlayIcon()
    {
        if (Player.IsPlaying)
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
        if (!Player.IsPlaying)
        {
            Player.Play();
        }
        else
        {
            Player.Pause();
        }
    }

    private void Stop()
    {
        Player.Stop();
        //MediaPlayer.Dispose();
    }

    private void Mute()
    {
        if (Volume == 0)
        {
            Volume = previousVolume;
            //VolumeIcon = "\uE767";
        }
        else
        {
            previousVolume = Volume;
            Volume = 0;
            //VolumeIcon = "\uE74F";
        }
    }

    private void ChangeProgress(EventArgs e)
    {

    }

    private void FullScreen()
    {
        //var view = ApplicationView.GetForCurrentView();
        //if (view.IsFullScreenMode)
        //{
        //    view.ExitFullScreenMode();
        //}
        //else
        //{
        //    view.TryEnterFullScreenMode();
        //}
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

    public void Dispose()
    {
        var mediaPlayer = Player;
        Player = null;
        mediaPlayer?.Dispose();
        LibVLC?.Dispose();
        LibVLC = null;
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
}
