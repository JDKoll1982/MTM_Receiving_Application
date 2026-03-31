using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Active waitlist page for Outside Service lines.
/// </summary>
public sealed partial class View_OutsideService_Waitlist : Page
{
    public ViewModel_OutsideService_Waitlist ViewModel { get; }

    public View_OutsideService_Waitlist()
    {
        ViewModel = App.GetService<ViewModel_OutsideService_Waitlist>();
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnMarkCompleteClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasSelectedSetupLine || XamlRoot is null || ViewModel.SelectedLine is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Complete Order?",
            Content = $"Mark {ViewModel.SelectedLine.QueueKey} as complete?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            DefaultButton = ContentDialogButton.Close,
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await ViewModel.MarkSelectedLineCompleteCommand.ExecuteAsync(null);
        }
    }
}
