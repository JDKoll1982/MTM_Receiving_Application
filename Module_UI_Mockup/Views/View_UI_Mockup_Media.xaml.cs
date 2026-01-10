using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Module_UI_Mockup.ViewModels;
using System;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_Media : Page
{
    public ViewModel_UI_Mockup_Media ViewModel { get; }

    public View_UI_Mockup_Media()
    {
        ViewModel = new ViewModel_UI_Mockup_Media();
        InitializeComponent();
    }

    private void RotateButton_Checked(object sender, RoutedEventArgs e)
    {
        // Rotate 180 degrees when checked
        var rotateAnimation = new DoubleAnimation
        {
            To = 180,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(rotateAnimation, RotateTransform);
        Storyboard.SetTargetProperty(rotateAnimation, "Angle");

        var storyboard = new Storyboard();
        storyboard.Children.Add(rotateAnimation);
        storyboard.Begin();
    }

    private void RotateButton_Unchecked(object sender, RoutedEventArgs e)
    {
        // Rotate back to 0 degrees when unchecked
        var rotateAnimation = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(rotateAnimation, RotateTransform);
        Storyboard.SetTargetProperty(rotateAnimation, "Angle");

        var storyboard = new Storyboard();
        storyboard.Children.Add(rotateAnimation);
        storyboard.Begin();
    }

    private void IconButton1_Click(object sender, RoutedEventArgs e)
    {
        // Scale up and rotate
        var scaleAnim = new DoubleAnimation
        {
            To = 1.3,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            AutoReverse = true,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };

        var rotateAnim = new DoubleAnimation
        {
            To = 180,
            Duration = new Duration(TimeSpan.FromMilliseconds(400)),
            AutoReverse = true,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();

        Storyboard.SetTarget(scaleAnim, AnimTransform1);
        Storyboard.SetTargetProperty(scaleAnim, "ScaleX");
        storyboard.Children.Add(scaleAnim);

        var scaleAnimY = new DoubleAnimation
        {
            To = 1.3,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            AutoReverse = true,
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleAnimY, AnimTransform1);
        Storyboard.SetTargetProperty(scaleAnimY, "ScaleY");
        storyboard.Children.Add(scaleAnimY);

        Storyboard.SetTarget(rotateAnim, AnimTransform1);
        Storyboard.SetTargetProperty(rotateAnim, "Rotation");
        storyboard.Children.Add(rotateAnim);

        storyboard.Begin();
    }

    private void IconToggle2_Checked(object sender, RoutedEventArgs e)
    {
        var fadeAnim = new DoubleAnimation
        {
            To = 0.3,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        Storyboard.SetTarget(fadeAnim, AnimIcon2);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");
        storyboard.Children.Add(fadeAnim);
        storyboard.Begin();
    }

    private void IconToggle2_Unchecked(object sender, RoutedEventArgs e)
    {
        var fadeAnim = new DoubleAnimation
        {
            To = 1.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        var storyboard = new Storyboard();
        Storyboard.SetTarget(fadeAnim, AnimIcon2);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");
        storyboard.Children.Add(fadeAnim);
        storyboard.Begin();
    }

    private void IconToggle3_Checked(object sender, RoutedEventArgs e)
    {
        AnimIcon3.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
    }

    private void IconToggle3_Unchecked(object sender, RoutedEventArgs e)
    {
        AnimIcon3.ClearValue(FontIcon.ForegroundProperty);
    }
}
