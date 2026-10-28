using System.Windows;
using System.Windows.Media.Animation;

namespace EchoDeck;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();
        
        // Smooth fade-in and slide-up animation for the title
        var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(600)) 
        { 
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } 
        };
        var slide = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(600)) 
        { 
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } 
        };
        
        BeginAnimation(OpacityProperty, fade);
        TitleTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slide);
    }
}