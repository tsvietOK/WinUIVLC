using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Platforms.Windows;
using LibVLCSharp.Shared;
using WinUIVLC.Contracts.Services;

namespace WinUIVLC.Services;

class PlaybackService : IPlaybackService
{
    private LibVLC LibVLC;

    public MediaPlayer MediaPlayer
    {
        get;
        private set;
    }

    public PlaybackService()
    {

    }

    public void Init(InitializedEventArgs eventArgs)
    {
        LibVLC = new LibVLC(true, eventArgs.SwapChainOptions);
        MediaPlayer = new MediaPlayer(LibVLC);
    }

    public void PlayPause()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
