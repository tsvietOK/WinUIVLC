using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUIVLC.UserControls;

public class AppBarWindowPresenterStateButton : AppBarButton
{
    public static DependencyProperty IsFullScreenProperty =
        DependencyProperty.Register("IsFullScreenProperty", typeof(bool), typeof(AppBarWindowPresenterStateButton), null);
    public bool IsFullScreen
    {
        get => (bool)GetValue(IsFullScreenProperty);
        set
        {
            SetValue(IsFullScreenProperty, value);
            Icon = IsFullScreen switch
            {
                true => FullScreenExitIcon,
                false => FullScreenIcon,
            };
        }
    }

    private static readonly FontIcon FullScreenIcon = new() { Glyph = "\uE740" };
    private static readonly FontIcon FullScreenExitIcon = new() { Glyph = "\uE73F" };
}
