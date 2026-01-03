using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Material.Icons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.Models;
using MTM_Receiving_Application.Helpers.UI;

namespace MTM_Receiving_Application.DunnageModule.Views;

public sealed partial class View_Dunnage_Control_IconPickerControl : UserControl
{
    public static readonly DependencyProperty SelectedIconProperty =
        DependencyProperty.Register(
            nameof(SelectedIcon),
            typeof(MaterialIconKind?),
            typeof(View_Dunnage_Control_IconPickerControl),
            new PropertyMetadata(MaterialIconKind.PackageVariantClosed, OnSelectedIconChanged));

    public static readonly DependencyProperty RecentlyUsedIconsProperty =
        DependencyProperty.Register(
            nameof(RecentlyUsedIcons),
            typeof(ObservableCollection<Model_IconDefinition>),
            typeof(View_Dunnage_Control_IconPickerControl),
            new PropertyMetadata(default(ObservableCollection<Model_IconDefinition>)));

    public MaterialIconKind? SelectedIcon
    {
        get => (MaterialIconKind?)GetValue(SelectedIconProperty);
        set => SetValue(SelectedIconProperty, value);
    }

    public ObservableCollection<Model_IconDefinition> RecentlyUsedIcons
    {
        get => (ObservableCollection<Model_IconDefinition>)GetValue(RecentlyUsedIconsProperty);
        set => SetValue(RecentlyUsedIconsProperty, value);
    }

    private readonly List<MaterialIconKind> _allIcons;
    private readonly ObservableCollection<MaterialIconKind> _filteredIcons;

    public View_Dunnage_Control_IconPickerControl()
    {
        InitializeComponent();

        // Load all icons
        _allIcons = Helper_MaterialIcons.GetAllIcons();
        _filteredIcons = new ObservableCollection<MaterialIconKind>(_allIcons.Take(200)); // Initial limit for performance

        AllIconsGrid.ItemsSource = _filteredIcons;
    }

    private static void OnSelectedIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is View_Dunnage_Control_IconPickerControl control && e.NewValue is MaterialIconKind kind)
        {
            // Update RecentIconsGrid selection if the icon is in the list
            if (control.RecentlyUsedIcons != null)
            {
                var recent = control.RecentlyUsedIcons.FirstOrDefault(x => x.Kind == kind);
                if (recent != null)
                {
                    control.RecentIconsGrid.SelectedItem = recent;
                }
                else
                {
                    control.RecentIconsGrid.SelectedItem = null;
                }
            }
        }
    }

    private void OnRecentIconSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RecentIconsGrid.SelectedItem is Model_IconDefinition selectedModel)
        {
            SelectedIcon = selectedModel.Kind;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text;

        _filteredIcons.Clear();

        var matches = Helper_MaterialIcons.SearchIcons(searchText).Take(200);
        foreach (var icon in matches)
        {
            _filteredIcons.Add(icon);
        }
    }
}
