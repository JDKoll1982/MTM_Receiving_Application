using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Reprint.Dialogs;
using MTM_Receiving_Application.Module_Reprint.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_Reprint.Views;

/// <summary>
/// Shared sub-page for the three Reprint Labels modes. Bound to a
/// <see cref="ViewModel_Reprint_ModuleBase"/> so Receiving, Dunnage, and Volvo share one layout.
/// </summary>
public sealed partial class View_Reprint_ModulePage : Page
{
    public ViewModel_Reprint_ModuleBase ViewModel { get; }

    public View_Reprint_ModulePage(ViewModel_Reprint_ModuleBase viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ViewModel.BackRequested += OnBackRequested;
        ViewModel.ReprintCompleted += OnReprintCompleted;
        ViewModel.ShowColumnChooserRequested += OnShowColumnChooserRequested;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.ActivateAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.BackRequested -= OnBackRequested;
        ViewModel.ReprintCompleted -= OnReprintCompleted;
        ViewModel.ShowColumnChooserRequested -= OnShowColumnChooserRequested;
    }

    private void OnBackRequested(object? sender, EventArgs e)
    {
        NavigateToLanding();
    }

    private async void OnReprintCompleted(object? sender, Model_ReprintBatchResult result)
    {
        var summary =
            $"{result.QueuedCount} label(s) queued for reprint.\n"
            + $"{result.AlreadyQueuedCount} already queued.\n"
            + $"{result.FailedCount} failed.";

        var dialog = new ContentDialog
        {
            Title = "Reprint Summary",
            Content = summary,
            PrimaryButtonText = "Mode Selection",
            SecondaryButtonText = "Start Over",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        var choice = await dialog.ShowAsync();

        if (choice == ContentDialogResult.Primary)
        {
            NavigateToLanding();
        }
        else if (choice == ContentDialogResult.Secondary)
        {
            await ViewModel.ResetCommand.ExecuteAsync(null);
        }
    }

    private void NavigateToLanding()
    {
        if (App.MainWindow is not MainWindow mainWindow)
        {
            return;
        }

        var landingPage = App.GetService<View_Reprint_Main>();
        mainWindow.SetContentPage(landingPage, "Reprint Labels");
    }

    private async void SearchPickerButton_Click(object sender, RoutedEventArgs e)
    {
        var candidates = ViewModel.HistoryRows
            .Select(row => new Model_FuzzySearchResult
            {
                Key = row.Part,
                Label = row.Part,
                Detail = row.Reference,
            })
            .DistinctBy(item => item.Key)
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        var dialog = new Dialog_FuzzySearchPicker(
            candidates,
            "Pick a Part",
            "Pick a part from the loaded history to search by."
        )
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary || dialog.SelectedResult is null)
        {
            return;
        }

        ViewModel.SearchText = dialog.SelectedResult.Key;
        await ViewModel.ReloadCommand.ExecuteAsync(null);
    }

    /// <summary>Pressing Enter in the search box runs the search immediately.</summary>
    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            ViewModel.ReloadCommand.Execute(null);
        }
    }

    private async void OnShowColumnChooserRequested(object? sender, EventArgs e)
    {
        await ShowColumnChooserDialogAsync();
    }

    private async Task ShowColumnChooserDialogAsync()
    {
        var dialog = new Dialog_Reprint_ColumnChooser(ViewModel.ColumnOptions)
        {
            XamlRoot = XamlRoot,
        };
        dialog.PrepareDialogSize();
        dialog.HorizontalAlignment = HorizontalAlignment.Center;
        dialog.VerticalAlignment = VerticalAlignment.Center;

        await dialog.ShowAsync();
        if (!dialog.WasAccepted)
        {
            return;
        }

        await ViewModel.ApplyAndPersistColumnVisibilityAsync();
        SyncHeaderVisibility();
    }

    private void SyncHeaderVisibility()
    {
        SetHeaderVisibility(DateHeader, "Date");
        SetHeaderVisibility(PartHeader, "Part");
        SetHeaderVisibility(QtyHeader, "Qty");
        SetHeaderVisibility(ReferenceHeader, "Reference");
    }

    private void SetHeaderVisibility(TextBlock header, string key)
    {
        if (header is null)
        {
            return;
        }

        var option = ViewModel.ColumnOptions.FirstOrDefault(column => column.Key == key);
        header.Visibility =
            option?.IsVisible != false ? Visibility.Visible : Visibility.Collapsed;
    }
}
