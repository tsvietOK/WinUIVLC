using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUIVLC.UserControls;

public class AppBarVolumeStateButton : AppBarButton
{
    public static DependencyProperty VolumeProperty =
        DependencyProperty.Register("VolumeProperty", typeof(int), typeof(AppBarVolumeStateButton), null);

    public int Volume
    {
        get => (int)GetValue(VolumeProperty);
        set
        {
            SetValue(VolumeProperty, value);

            Icon = Volume switch
            {
                0 => VolumeIcon0,
                > 0 and <= 25 => VolumeIcon1,
                > 25 and <= 50 => VolumeIcon2,
                > 50 and <= 75 => VolumeIcon3,
                _ => VolumeIcon4,
            };
        }
    }

    private static readonly FontIcon VolumeIcon0 = new() { Glyph = "\uE74F" };
    private static readonly FontIcon VolumeIcon1 = new() { Glyph = "\uE992" };
    private static readonly FontIcon VolumeIcon2 = new() { Glyph = "\uE993" };
    private static readonly FontIcon VolumeIcon3 = new() { Glyph = "\uE994" };
    private static readonly FontIcon VolumeIcon4 = new() { Glyph = "\uE767" };

}
