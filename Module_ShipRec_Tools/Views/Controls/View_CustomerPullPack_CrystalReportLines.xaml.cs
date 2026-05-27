using System;
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
    private bool _isSynchronizingLocationSelection;
    private bool _isSynchronizingRequestLineSelection;

    public static readonly DependencyProperty GroupsProperty = DependencyProperty.Register(
        nameof(Groups),
        typeof(IList<Model_CustomerPullPack_CrystalReportGroup>),
        typeof(View_CustomerPullPack_CrystalReportLines),
        new PropertyMetadata(null, OnGroupsChanged)
    );

    public static readonly DependencyProperty ReportDateDisplayProperty =
        DependencyProperty.Register(
            nameof(ReportDateDisplay),
            typeof(string),
            typeof(View_CustomerPullPack_CrystalReportLines),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty ReportTimeDisplayProperty =
        DependencyProperty.Register(
            nameof(ReportTimeDisplay),
            typeof(string),
            typeof(View_CustomerPullPack_CrystalReportLines),
            new PropertyMetadata(string.Empty)
        );

    public View_CustomerPullPack_CrystalReportLines()
    {
        InitializeComponent();
    }

    public IList<Model_CustomerPullPack_CrystalReportGroup>? Groups
    {
        get => (IList<Model_CustomerPullPack_CrystalReportGroup>?)GetValue(GroupsProperty);
        set => SetValue(GroupsProperty, value);
    }

    public string ReportDateDisplay
    {
        get => (string)GetValue(ReportDateDisplayProperty);
        set => SetValue(ReportDateDisplayProperty, value);
    }

    public string ReportTimeDisplay
    {
        get => (string)GetValue(ReportTimeDisplayProperty);
        set => SetValue(ReportTimeDisplayProperty, value);
    }

    public IList<Model_CustomerPullPack_CrystalReportGroup> DisplayGroups { get; private set; } =
    [];

    public event EventHandler<CrystalLocationSelectionChangedEventArgs>? LocationSelectionChanged;

    public event EventHandler<CrystalRequestLineSelectionChangedEventArgs>? RequestLineSelectionChanged;

    private static void OnGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((View_CustomerPullPack_CrystalReportLines)d).DisplayGroups =
            ((View_CustomerPullPack_CrystalReportLines)d).Groups ?? [];
    }

    private void RefreshDisplayGroups()
    {
        DisplayGroups = Groups ?? [];
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
        if (_isSynchronizingLocationSelection)
        {
            return;
        }

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
        if (_isSynchronizingRequestLineSelection)
        {
            return;
        }

        if (sender is not ListView listView)
        {
            return;
        }

        ApplyRequestLineSelectionToModel(listView);
    }

    private void SyncLocationListSelectionFromModel(ListView listView)
    {
        _isSynchronizingLocationSelection = true;

        try
        {
            listView.SelectedItems.Clear();

            foreach (
                var item in listView.Items.OfType<Model_CustomerPullPack_CrystalSubPartLocation>()
            )
            {
                if (item.IsSelected)
                {
                    listView.SelectedItems.Add(item);
                }
            }
        }
        finally
        {
            _isSynchronizingLocationSelection = false;
        }
    }

    private void ApplyLocationSelectionToModel(ListView listView)
    {
        if (
            listView.DataContext is Model_CustomerPullPack_CrystalReportGroup gatedGroup
            && gatedGroup.HasSelectedRequestLine is false
        )
        {
            _isSynchronizingLocationSelection = true;

            try
            {
                listView.SelectedItems.Clear();

                foreach (
                    var item in listView.Items.OfType<Model_CustomerPullPack_CrystalSubPartLocation>()
                )
                {
                    item.IsSelected = false;
                }
            }
            finally
            {
                _isSynchronizingLocationSelection = false;
            }

            return;
        }

        var selectedItems = listView
            .SelectedItems.OfType<Model_CustomerPullPack_CrystalSubPartLocation>()
            .ToHashSet();

        foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalSubPartLocation>())
        {
            item.IsSelected = selectedItems.Contains(item);
        }

        if (listView.DataContext is Model_CustomerPullPack_CrystalReportGroup group)
        {
            LocationSelectionChanged?.Invoke(
                this,
                new CrystalLocationSelectionChangedEventArgs(
                    group.ParentPartId,
                    selectedItems.Select(static item => item.LocationId).ToList()
                )
            );
        }
    }

    private void SyncRequestLineSelectionFromModel(ListView listView)
    {
        _isSynchronizingRequestLineSelection = true;

        try
        {
            var selectedItem = listView
                .Items.OfType<Model_CustomerPullPack_CrystalRequestLine>()
                .FirstOrDefault(item => item.IsSelected);

            if (listView.SelectionMode == ListViewSelectionMode.Single)
            {
                listView.SelectedItem = selectedItem;
                return;
            }

            listView.SelectedItems.Clear();

            foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalRequestLine>())
            {
                if (item.IsSelected)
                {
                    listView.SelectedItems.Add(item);
                }
            }
        }
        finally
        {
            _isSynchronizingRequestLineSelection = false;
        }
    }

    private void ApplyRequestLineSelectionToModel(ListView listView)
    {
        var selectedRequestLine =
            listView.SelectionMode == ListViewSelectionMode.Single
                ? listView.SelectedItem as Model_CustomerPullPack_CrystalRequestLine
                : listView
                    .SelectedItems.OfType<Model_CustomerPullPack_CrystalRequestLine>()
                    .FirstOrDefault();

        foreach (var item in listView.Items.OfType<Model_CustomerPullPack_CrystalRequestLine>())
        {
            item.IsSelected = ReferenceEquals(item, selectedRequestLine);
        }

        RequestLineSelectionChanged?.Invoke(
            this,
            new CrystalRequestLineSelectionChangedEventArgs(
                selectedRequestLine?.SourceLineKey ?? string.Empty
            )
        );
    }
}

public sealed class CrystalLocationSelectionChangedEventArgs : EventArgs
{
    public CrystalLocationSelectionChangedEventArgs(
        string parentPartId,
        IReadOnlyList<string> selectedLocationIds
    )
    {
        ParentPartId = parentPartId;
        SelectedLocationIds = selectedLocationIds;
    }

    public string ParentPartId { get; }

    public IReadOnlyList<string> SelectedLocationIds { get; }
}

public sealed class CrystalRequestLineSelectionChangedEventArgs : EventArgs
{
    public CrystalRequestLineSelectionChangedEventArgs(string selectedSourceLineKey)
    {
        SelectedSourceLineKey = selectedSourceLineKey;
    }

    public string SelectedSourceLineKey { get; }
}
