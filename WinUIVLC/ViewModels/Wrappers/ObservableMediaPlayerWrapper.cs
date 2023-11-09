using CommunityToolkit.Mvvm.ComponentModel;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace WinUIVLC.ViewModels.Wrappers;

public class ObservableMediaPlayerWrapper : ObservableObject
{
    private readonly MediaPlayer _player;
    private readonly DispatcherQueue _dispatcherQueue;

    public ObservableMediaPlayerWrapper(MediaPlayer player, DispatcherQueue dispatcherQueue)
    {
        _player = player;
        _dispatcherQueue = dispatcherQueue;
        _player.TimeChanged += (sender, time) => _dispatcherQueue.TryEnqueue(() =>
        {
            OnPropertyChanged(nameof(TimeLong));
            OnPropertyChanged(nameof(TimeString));
        });
    }

    public long TimeLong
    {
        get => _player.Time;
        set => SetProperty(_player.Time, value, _player, (u, n) => u.Time = n);
    }

    public string TimeString => TimeSpan.FromMilliseconds(TimeLong).ToString(@"hh\:mm\:ss");
}