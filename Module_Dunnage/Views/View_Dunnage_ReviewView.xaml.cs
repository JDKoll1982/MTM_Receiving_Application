using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_ReviewView : UserControl
{
    public ViewModel_Dunnage_Review ViewModel { get; }
    private readonly IService_Focus _focusService;
    private DataGridColumn? _lastSortedColumn;
    private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;

    public View_Dunnage_ReviewView(ViewModel_Dunnage_Review viewModel, IService_Focus focusService)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);

        ViewModel = viewModel;
        _focusService = focusService;

        InitializeComponent();
        DataContext = ViewModel;

        _focusService.AttachFocusOnVisibility(this);
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Dunnage_ReviewView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_Review>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();
        DataContext = ViewModel;
        _focusService.AttachFocusOnVisibility(this);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSessionLoadsAsync();
    }

    private void ReviewDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        if (e.Column is not DataGridBoundColumn boundColumn)
        {
            return;
        }

        if (boundColumn.Binding is not Microsoft.UI.Xaml.Data.Binding binding)
        {
            return;
        }

        var propertyName = binding.Path?.Path;
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return;
        }

        var nextDirection = _lastSortedColumn == e.Column
            && _lastSortDirection == ListSortDirection.Ascending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;

        var sortedLoads = ViewModel.SessionLoads.ToList();
        sortedLoads.Sort((left, right) => CompareByProperty(left, right, propertyName, nextDirection));

        ViewModel.SessionLoads.Clear();
        foreach (var load in sortedLoads)
        {
            ViewModel.SessionLoads.Add(load);
        }

        if (_lastSortedColumn is not null && _lastSortedColumn != e.Column)
        {
            _lastSortedColumn.SortDirection = null;
        }

        e.Column.SortDirection = nextDirection == ListSortDirection.Ascending
            ? DataGridSortDirection.Ascending
            : DataGridSortDirection.Descending;
        _lastSortedColumn = e.Column;
        _lastSortDirection = nextDirection;
    }

    private static int CompareByProperty(
        Model_DunnageLoad left,
        Model_DunnageLoad right,
        string propertyName,
        ListSortDirection direction
    )
    {
        var property = typeof(Model_DunnageLoad).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null)
        {
            return 0;
        }

        var leftValue = property.GetValue(left);
        var rightValue = property.GetValue(right);

        var comparison = CompareValues(leftValue, rightValue);
        return direction == ListSortDirection.Ascending ? comparison : -comparison;
    }

    private static int CompareValues(object? leftValue, object? rightValue)
    {
        if (ReferenceEquals(leftValue, rightValue))
        {
            return 0;
        }

        if (leftValue is null)
        {
            return 1;
        }

        if (rightValue is null)
        {
            return -1;
        }

        if (leftValue is string leftString && rightValue is string rightString)
        {
            return string.Compare(leftString, rightString, StringComparison.OrdinalIgnoreCase);
        }

        if (leftValue is IComparable leftComparable && rightValue is IComparable)
        {
            return leftComparable.CompareTo(rightValue);
        }

        return string.Compare(
            leftValue.ToString(),
            rightValue.ToString(),
            StringComparison.OrdinalIgnoreCase
        );
    }
}
