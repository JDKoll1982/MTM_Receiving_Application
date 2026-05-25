using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views.Controls;

/// <summary>
/// Crystal-style grouped report preview used to align the WinUI page with the HTML mockup.
/// </summary>
public sealed partial class View_CustomerPullPack_CrystalReportLines : UserControl
{
    private static readonly IList<Model_CustomerPullPack_CrystalReportGroup> PlaceholderGroups =
        CreatePlaceholderGroups();

    public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(
        nameof(Groups),
        typeof(IList<Model_CustomerPullPack_CrystalReportGroup>),
        typeof(View_CustomerPullPack_CrystalReportLines),
        new PropertyMetadata(null, OnGroupsChanged)
    );

    public View_CustomerPullPack_CrystalReportLines()
    {
        InitializeComponent();
        RefreshDisplayGroups();
    }

    public IList<Model_CustomerPullPack_CrystalReportGroup>? Groups
    {
        get => (IList<Model_CustomerPullPack_CrystalReportGroup>?)GetValue(GroupsProperty);
        set => SetValue(GroupsProperty, value);
    }

    public IList<Model_CustomerPullPack_CrystalReportGroup> DisplayGroups { get; private set; } =
        PlaceholderGroups;

    public Model_CustomerPullPack_CrystalSubPartLocation? SelectedSubPartPlaceholder { get; set; }

    public Model_CustomerPullPack_CrystalRequestLine? SelectedRequestLinePlaceholder { get; set; }

    public string ReportDateDisplay => "5/20/2026";

    public string ReportTimeDisplay => "5:11:23 AM";

    private static void OnGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((View_CustomerPullPack_CrystalReportLines)d).RefreshDisplayGroups();
    }

    private void RefreshDisplayGroups()
    {
        DisplayGroups = Groups is { Count: > 0 } ? Groups : PlaceholderGroups;
        Bindings.Update();
    }

    private void OnSelectableLocationListLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ListView listView)
        {
            return;
        }

        SyncLocationListSelectionFromModel(listView);
    }

    private void OnSelectableLocationListSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (sender is not ListView listView)
        {
            return;
        }

        ApplyLocationSelectionToModel(listView);
    }

    private void OnSelectableRequestLineListLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ListView listView)
        {
            return;
        }

        SyncRequestLineSelectionFromModel(listView);
    }

    private void OnSelectableRequestLineListSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (sender is not ListView listView)
        {
            return;
        }

        ApplyRequestLineSelectionToModel(listView);
    }

    private void SyncLocationListSelectionFromModel(ListView listView)
    {
        listView.SelectedItems.Clear();

        foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalSubPartLocation>())
        {
            if (item.IsSelected)
            {
                listView.SelectedItems.Add(item);
            }
        }

        Bindings.Update();
    }

    private void ApplyLocationSelectionToModel(ListView listView)
    {
        var selectedItems = listView
            .SelectedItems.OfType<Model_CustomerPullPack_CrystalSubPartLocation>()
            .ToHashSet();

        foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalSubPartLocation>())
        {
            item.IsSelected = selectedItems.Contains(item);
        }

        Bindings.Update();
    }

    private void SyncRequestLineSelectionFromModel(ListView listView)
    {
        listView.SelectedItems.Clear();

        foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalRequestLine>())
        {
            if (item.IsSelected)
            {
                listView.SelectedItems.Add(item);
            }
        }

        Bindings.Update();
    }

    private void ApplyRequestLineSelectionToModel(ListView listView)
    {
        var selectedItems = listView
            .SelectedItems.OfType<Model_CustomerPullPack_CrystalRequestLine>()
            .ToHashSet();

        foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalRequestLine>())
        {
            item.IsSelected = selectedItems.Contains(item);
        }

        Bindings.Update();
    }

    private static IList<Model_CustomerPullPack_CrystalReportGroup> CreatePlaceholderGroups()
    {
        return
        [
            new Model_CustomerPullPack_CrystalReportGroup
            {
                ParentPartId = "20433220",
                QuantityToPack = 69,
                QuantitySelected = 69,
                FgLocationId = "DD-E0-12",
                FgOnHandQuantity = 69,
                SubPartLocations =
                [
                    new Model_CustomerPullPack_CrystalSubPartLocation
                    {
                        PartLocationId = "20433220-PKG DD-M1-01",
                        OnHandQuantity = 58,
                    },
                    new Model_CustomerPullPack_CrystalSubPartLocation
                    {
                        PartLocationId = "20433220-PKG DD-M1-03",
                        OnHandQuantity = 14,
                        IsSelected = true,
                    },
                ],
                RequestLines =
                [
                    new Model_CustomerPullPack_CrystalRequestLine
                    {
                        CustomerOrderId = "CO-086516",
                        ParentPartId = "20433220",
                        LocationId = "undefined",
                        CustomerLabel = "Volvo - Volvo Group (10)",
                        ShipQuantity = 69,
                        PullDateDisplay = "5/27/26",
                        IsSelected = true,
                    },
                ],
                ServiceNote = "SERVICE PARTS - PRIORITIZE OTHER ORDERS FIRST!",
            },
            new Model_CustomerPullPack_CrystalReportGroup
            {
                ParentPartId = "20461010",
                QuantityToPack = 550,
                QuantitySelected = 550,
                FgLocationId = "DD-E0-01",
                FgOnHandQuantity = 40,
                ShortageFlag = true,
                SubPartLocations =
                [
                    new Model_CustomerPullPack_CrystalSubPartLocation
                    {
                        PartLocationId = "20461010-PKG DC-H3-23",
                        OnHandQuantity = 1296,
                    },
                    new Model_CustomerPullPack_CrystalSubPartLocation
                    {
                        PartLocationId = "20461010-PKG DD-A0-09",
                        OnHandQuantity = 1710,
                    },
                    new Model_CustomerPullPack_CrystalSubPartLocation
                    {
                        PartLocationId = "20461010-PKG DD-A1-19",
                        OnHandQuantity = 1690,
                        IsSelected = true,
                    },
                ],
                RequestLines =
                [
                    new Model_CustomerPullPack_CrystalRequestLine
                    {
                        CustomerOrderId = "CO-086516",
                        ParentPartId = "20461010",
                        LocationId = "undefined",
                        CustomerLabel = "Volvo - Volvo Group (10)",
                        ShipQuantity = 550,
                        PullDateDisplay = "5/27/26",
                        IsSelected = true,
                        StatusBadgeText = "Problem",
                    },
                ],
                ServiceNote = "SERVICE PARTS - PRIORITIZE OTHER ORDERS FIRST!",
            },
        ];
    }
}
