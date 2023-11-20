using Microsoft.UI.Xaml.Controls;
using WinUIVLC.ViewModels;

namespace WinUIVLC.Views;

public sealed partial class VideoPlayerPage : Page
{
    public VideoPlayerViewModel ViewModel
    {
        get;
    }

    public VideoPlayerPage()
    {
        ViewModel = App.GetService<VideoPlayerViewModel>();
        InitializeComponent();
    }
}
