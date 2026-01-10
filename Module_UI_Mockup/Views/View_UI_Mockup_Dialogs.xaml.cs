using System;
using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_Dialogs : Page
{
    public ViewModel_UI_Mockup_Dialogs ViewModel { get; }

    public View_UI_Mockup_Dialogs()
    {
        ViewModel = new ViewModel_UI_Mockup_Dialogs();
        InitializeComponent();
    }

    private async void ShowContentDialog_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Delete File?",
            Content = "Are you sure you want to delete this file? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private void ShowFlyout_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Flyout is defined in XAML
    }

    private void ShowTeachingTip_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        SampleTeachingTip.IsOpen = true;
    }
}
