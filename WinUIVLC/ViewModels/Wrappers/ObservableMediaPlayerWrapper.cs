using CommunityToolkit.Mvvm.ComponentModel;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace WinUIVLC.ViewModels.Wrappers;

public class ObservableMediaPlayerWrapper : ObservableObject
{
    private readonly MediaPlayer _player;
    private readonly DispatcherQueue _dispatcherQueue;
    private int previousVolume;

    private const int rewindOffset = 10000;

    public ObservableMediaPlayerWrapper(MediaPlayer player, DispatcherQueue dispatcherQueue)
    {
        _player = player;
        _dispatcherQueue = dispatcherQueue;
        _player.TimeChanged += (sender, time) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(TimeLong));
            OnPropertyChanged(nameof(TimeString));
        });

        _player.VolumeChanged += (sender, time) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(Volume));
        });
    }

    public long TimeLong
    {
        get => _player.Time;
        set => SetProperty(_player.Time, value, _player, (u, n) => u.Time = n);
    }

    public string TimeString => TimeSpan.FromMilliseconds(TimeLong).ToString(@"hh\:mm\:ss");

    public int Volume
    {
        get => _player.Volume;
        set => SetProperty(_player.Volume, value, _player, (u, n) => u.Volume = n);
    }

    public bool IsPlaying => _player.IsPlaying;

    public void VolumeUp()
    {
        if (Volume <= 200)
        {
            Volume += 5;
        }
    }

    public void VolumeDown()
    {
        if (Volume >= 5)
        {
            Volume -= 5;
        }
    }

    public void Mute()
    {
        if (Volume == 0)
        {
            Volume = previousVolume;
        }
        else
        {
            previousVolume = Volume;
            Volume = 0;
        }
    }

    public void PlayPause()
    {
        if (!IsPlaying)
        {
            _player.Play();
        }
        else
        {
            _player.Pause();
        }
    }

    public void Stop()
    {
        _player.Stop();
    }

    public void FastForward()
    {
        TimeLong += rewindOffset;
    }

    public void Rewind()
    {
        TimeLong -= rewindOffset;
    }
}