using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// UserControl for Dunnage Type Selection view with a responsive 3x3 paginated grid.
/// </summary>
public sealed partial class View_dunnage_typeselectionView : UserControl
{
    private const int GridColumnCount = 3;
    private const int GridRowCount = 3;
    private const double MaxCardSlotWidth = 260;
    private const double MaxCardSlotHeight = 176;
    private const double MinCardSlotWidth = 168;
    private const double MinCardSlotHeight = 132;

    public ViewModel_dunnage_typeselection ViewModel { get; }

    private readonly IService_Focus _focusService;
    private ItemsWrapGrid? _typeGridItemsPanel;

    public View_dunnage_typeselectionView()
    {
        ViewModel = App.GetService<ViewModel_dunnage_typeselection>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
        CardAreaHost.SizeChanged += OnCardAreaHostSizeChanged;
        TypeGridView.Loaded += OnTypeGridViewLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("dunnage_typeselectionView: OnLoaded called");
        await ViewModel.InitializeAsync();
        UpdateTypeGridLayout();
    }

    private async void TypeCard_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Model_DunnageType type)
        {
            await ViewModel.SelectTypeCommand.ExecuteAsync(type);
        }
    }

    private void OnTypeGridViewLoaded(object sender, RoutedEventArgs e)
    {
        _typeGridItemsPanel = FindDescendant<ItemsWrapGrid>(TypeGridView);
        UpdateTypeGridLayout();
    }

    private void OnCardAreaHostSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateTypeGridLayout();
    }

    private void UpdateTypeGridLayout()
    {
        _typeGridItemsPanel ??= FindDescendant<ItemsWrapGrid>(TypeGridView);
        if (_typeGridItemsPanel is null)
        {
            return;
        }

        if (CardAreaHost.ActualWidth <= 0 || CardAreaHost.ActualHeight <= 0)
        {
            return;
        }

        double calculatedSlotWidth = Math.Floor(CardAreaHost.ActualWidth / GridColumnCount);
        double calculatedSlotHeight = Math.Floor(CardAreaHost.ActualHeight / GridRowCount);

        _typeGridItemsPanel.ItemWidth = Math.Clamp(
            calculatedSlotWidth,
            MinCardSlotWidth,
            MaxCardSlotWidth
        );
        _typeGridItemsPanel.ItemHeight = Math.Clamp(
            calculatedSlotHeight,
            MinCardSlotHeight,
            MaxCardSlotHeight
        );
    }

    private static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        int childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < childCount; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            T? descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
