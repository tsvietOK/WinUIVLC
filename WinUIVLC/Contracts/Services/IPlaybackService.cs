using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Platforms.Windows;

namespace WinUIVLC.Contracts.Services;

public interface IPlaybackService
{
    void Init(InitializedEventArgs eventArgs);

    void PlayPause();

    void Stop();
}
