using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Settings.Core.Views.Controls;

public sealed partial class View_SettingsPlaceholderControl : UserControl
{
    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(View_SettingsPlaceholderControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionTextProperty =
        DependencyProperty.Register(nameof(DescriptionText), typeof(string), typeof(View_SettingsPlaceholderControl), new PropertyMetadata(string.Empty));

    public string TitleText
    {
        get => (string)GetValue(TitleTextProperty);
        set => SetValue(TitleTextProperty, value);
    }

    public string DescriptionText
    {
        get => (string)GetValue(DescriptionTextProperty);
        set => SetValue(DescriptionTextProperty, value);
    }

    public View_SettingsPlaceholderControl()
    {
        InitializeComponent();
    }
}
