using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUIVLC.UserControls;

public sealed class CustomNavigationView : NavigationView
{
    public static DependencyProperty IsFullScreenProperty = DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(CustomNavigationView), null);

    public bool IsFullScreen
    {
        get => (bool)GetValue(IsFullScreenProperty);
        set
        {
            SetValue(IsFullScreenProperty, value);

            if (value)
            {
                IsPaneVisible = false;
                VisualStateManager.GoToState(this, "FullScreen", true);
            }
            else
            {
                IsPaneVisible = true;
                VisualStateManager.GoToState(this, "NotFullScreen", true);
            }
        }
    }
}
