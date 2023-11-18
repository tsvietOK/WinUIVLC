using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUIVLC.UserControls;

public class AppBarPlaybackStateButton : AppBarButton
{
    public static DependencyProperty IsPlayingProperty =
        DependencyProperty.Register("IsPlayingProperty", typeof(bool), typeof(AppBarPlaybackStateButton), null);
    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set
        {
            SetValue(IsPlayingProperty, value);
            Icon = IsPlaying switch
            {
                true => PauseIcon,
                false => PlayIcon,
            };
        }
    }

    private static readonly FontIcon PlayIcon = new() { Glyph = "\uE768" };
    private static readonly FontIcon PauseIcon = new() { Glyph = "\uE769" };
}
