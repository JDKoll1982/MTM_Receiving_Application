using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.ViewModels;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Workbench view code-behind. Keeps business logic in the ViewModel; this file only
/// handles focus/selection and the Step 6b-a1-a stock-location modal.
/// </summary>
public sealed partial class View_Scanner_Workbench : Page
{
    private TextBox? _toLocationTextBox;
    private bool _isSanitizingToLocation;
    private bool _isPickerOpen;

    public ViewModel_Scanner_Workbench ViewModel { get; }

    public View_Scanner_Workbench(ViewModel_Scanner_Workbench viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;

        _toLocationTextBox = FindToLocationTextBox();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private TextBox? FindToLocationTextBox()
    {
        foreach (var child in ItemEntryGrid.Children)
        {
            if (child is TextBox textBox && string.Equals(textBox.Header?.ToString(), "To Loc", StringComparison.OrdinalIgnoreCase))
            {
                return textBox;
            }
        }

        return null;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.FromLocationInventoryPickerRequested += OnFromLocationInventoryPickerRequestedAsync;
        ViewModel.PartIdFocusRequested += OnPartIdFocusRequested;
        ViewModel.ToLocationFocusRequested += OnToLocationFocusRequested;

        ViewModel.Activate();
        await ViewModel.EnsureCurrentSessionAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.FromLocationInventoryPickerRequested -= OnFromLocationInventoryPickerRequestedAsync;
        ViewModel.PartIdFocusRequested -= OnPartIdFocusRequested;
        ViewModel.ToLocationFocusRequested -= OnToLocationFocusRequested;
        ViewModel.Deactivate();
    }

    // ── Step 6b-a1-a: stock-location modal ───────────────────────────────────────

    private async Task<Model_ScannerStockPick?> OnFromLocationInventoryPickerRequestedAsync(
        string partId,
        string warehouseCode,
        string currentLocation
    )
    {
        if (_isPickerOpen)
        {
            return null;
        }

        _isPickerOpen = true;
        try
        {
            var rowsResult = await ViewModel.GetFromInventoryLocationsAsync();
            if (!rowsResult.Success || rowsResult.Data is null || rowsResult.Data.Count == 0)
            {
                return null;
            }

            return await ShowStockPickerDialogAsync(partId, warehouseCode, rowsResult.Data);
        }
        finally
        {
            _isPickerOpen = false;
        }
    }

    private async Task<Model_ScannerStockPick?> ShowStockPickerDialogAsync(
        string partId,
        string warehouseCode,
        IReadOnlyList<Model_InforVisualMaterialLocationRow> rows
    )
    {
        var xamlRoot = XamlRoot ?? this.XamlRoot;
        if (xamlRoot is null)
        {
            return null;
        }

        var listView = new ListView
        {
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 360,
        };

        var pickRows = rows.Select(row => new StockPickRow
        {
            LocationId = row.LocationId,
            WarehouseCode = row.WarehouseCode,
            OnHand = row.Quantity,
            Quantity = row.Quantity.ToString("0.####", CultureInfo.InvariantCulture),
        }).ToList();

        listView.ItemsSource = pickRows;
        listView.ItemTemplate = BuildStockPickTemplate();
        listView.SelectionChanged += (_, _) =>
        {
            if (listView.SelectedItem is StockPickRow selected)
            {
                selected.Quantity = selected.OnHand.ToString("0.####", CultureInfo.InvariantCulture);
            }
        };

        var dialog = new ContentDialog
        {
            Title = $"Stock locations for {partId}",
            Content = listView,
            PrimaryButtonText = "Use Selected",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary || listView.SelectedItem is not StockPickRow chosen)
        {
            return null;
        }

        if (!decimal.TryParse(chosen.Quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
        {
            quantity = 0m;
        }

        quantity = Math.Clamp(quantity, 0m, chosen.OnHand);

        return new Model_ScannerStockPick
        {
            Location = chosen.LocationId,
            Quantity = quantity.ToString("0.####", CultureInfo.InvariantCulture),
        };
    }

    private DataTemplate BuildStockPickTemplate()
    {
        return (DataTemplate)XamlReader.Load(
            """
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <Grid Padding="4,4" ColumnSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="*" />
                        <ColumnDefinition Width="Auto" />
                    </Grid.ColumnDefinitions>
                    <TextBlock Grid.Column="0" Text="{Binding LocationId}" FontWeight="SemiBold" VerticalAlignment="Center" />
                    <TextBlock Grid.Column="1" Text="{Binding OnHand}" VerticalAlignment="Center" />
                    <TextBox Grid.Column="2" Width="120" Text="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
                </Grid>
            </DataTemplate>
            """
        );
    }

    // ── Focus/selection wiring ───────────────────────────────────────────────────

    private void OnPartIdFocusRequested()
    {
        PartIdLookupControl?.FocusInput();
    }

    private void OnToLocationFocusRequested()
    {
        if (_toLocationTextBox is null)
        {
            return;
        }

        _toLocationTextBox.IsEnabled = true;
        _toLocationTextBox.Focus(FocusState.Programmatic);
        _toLocationTextBox.SelectAll();
    }

    // ── Event handlers ───────────────────────────────────────────────────────────

    private async void PartIdLookupControl_ValidationCompleted(
        object sender,
        Model_SharedLookupValidationCompletedEventArgs args
    )
    {
        if (args?.Result is null)
        {
            return;
        }

        var resolved = args.Result.ResolvedValue
            ?? args.Result.FormattedValue
            ?? args.Result.RawInput
            ?? ViewModel.NewPartId;
        await ViewModel.PartValidationCompletedAsync(resolved);
    }

    private async void ToLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox toLocBox)
        {
            return;
        }

        if (_isSanitizingToLocation)
        {
            return;
        }

        _isSanitizingToLocation = true;
        try
        {
            ViewModel.NewToLocation = toLocBox.Text;
            bool isValid = await ViewModel.ValidateToLocationFormatAsync();
            if (!isValid)
            {
                toLocBox.Focus(FocusState.Programmatic);
                toLocBox.SelectAll();
            }
        }
        finally
        {
            _isSanitizingToLocation = false;
        }
    }

    private void QuantityTextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        if (args is null || args.NewText.Length == 0)
        {
            return;
        }

        if (!decimal.TryParse(args.NewText, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) || value < 0)
        {
            args.Cancel = true;
            return;
        }

        if (ViewModel.MaxQuantity is decimal max && value > max)
        {
            args.Cancel = true;
            return;
        }
    }

    /// <summary>Local row type for the stock-location picker.</summary>
    private sealed class StockPickRow
    {
        public string LocationId { get; init; } = string.Empty;

        public string WarehouseCode { get; init; } = string.Empty;

        public decimal OnHand { get; init; }

        public string Quantity { get; set; } = string.Empty;
    }
}
