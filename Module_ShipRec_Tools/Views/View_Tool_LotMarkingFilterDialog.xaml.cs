using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// The Filter window's content. It edits the date range, the order and the visible columns, and hands
/// the result back to the tool by changing the ViewModel those choices live on.
/// </summary>
/// <remarks>
/// The page performs no filtering itself. It changes the ViewModel's choices, and the ViewModel saves
/// them and reads the list again, so the screen and the printed sheet are narrowed by the same code.
/// Cancel puts the choices back the way they were, because the page edits a live ViewModel rather than
/// a copy of it.
/// </remarks>
public sealed partial class View_Tool_LotMarkingFilterDialog : Page
{
    private readonly DateTimeOffset? _openingFromDate;
    private readonly DateTimeOffset? _openingToDate;
    private readonly Model_Tool_LotMarkingSortOption _openingSortOption;
    private readonly bool _openingSortDescending;
    private readonly bool[] _openingColumnVisibility = [];

    public ViewModel_Tool_LotMarking ViewModel { get; }

    public event EventHandler? CloseRequested;

    public event EventHandler? ApplyRequested;

    public View_Tool_LotMarkingFilterDialog(ViewModel_Tool_LotMarking viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        _openingFromDate = ViewModel.FromDate;
        _openingToDate = ViewModel.ToDate;
        _openingSortOption = ViewModel.SelectedSortOption;
        _openingSortDescending = ViewModel.IsSortDescending;

        _openingColumnVisibility = new bool[ViewModel.ColumnOptions.Count];
        for (var index = 0; index < ViewModel.ColumnOptions.Count; index++)
        {
            _openingColumnVisibility[index] = ViewModel.ColumnOptions[index].IsVisible;
        }

        BuildDatePresetMenu();

        // The first control to take focus would otherwise scroll the window to itself, opening the
        // Filter window part-way down with the date section cut off.
        Loaded += OnDialogLoaded;
    }

    private void OnDialogLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnDialogLoaded;
        ContentScroll.ChangeView(null, 0, null, disableAnimation: true);
    }

    /// <summary>
    /// Builds the quick date range menu from the shared preset list, so the choices here cannot drift
    /// from the Date Type menu the rest of the app offers.
    /// </summary>
    private void BuildDatePresetMenu()
    {
        foreach (var preset in Helper_DateRangePresets.All)
        {
            var item = new MenuFlyoutItem
            {
                Text = preset.DisplayName,
                Tag = preset.Key,
            };

            item.Click += OnDatePresetClick;
            DatePresetFlyout.Items.Add(item);
        }
    }

    private void OnDatePresetClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: string presetKey })
        {
            ViewModel.ApplyDateRangePreset(presetKey);
        }

        DatePresetFlyout.Hide();
    }

    private void OnApplyClick(object sender, RoutedEventArgs e) =>
        ApplyRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Closes without applying, and puts back the choices the window opened with so a cancelled visit
    /// leaves the list exactly as it was.
    /// </summary>
    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        ViewModel.FromDate = _openingFromDate;
        ViewModel.ToDate = _openingToDate;
        ViewModel.SelectedSortOption = _openingSortOption;
        ViewModel.IsSortDescending = _openingSortDescending;

        for (
            var index = 0;
            index < ViewModel.ColumnOptions.Count && index < _openingColumnVisibility.Length;
            index++
        )
        {
            ViewModel.ColumnOptions[index].IsVisible = _openingColumnVisibility[index];
        }

        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
