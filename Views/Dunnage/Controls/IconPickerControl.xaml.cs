using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage.Controls;

public sealed partial class IconPickerControl : UserControl
{
    public static readonly DependencyProperty SelectedIconProperty =
        DependencyProperty.Register(
            nameof(SelectedIcon),
            typeof(string),
            typeof(IconPickerControl),
            new PropertyMetadata(default(string)));

    public static readonly DependencyProperty RecentlyUsedIconsProperty =
        DependencyProperty.Register(
            nameof(RecentlyUsedIcons),
            typeof(ObservableCollection<Model_IconDefinition>),
            typeof(IconPickerControl),
            new PropertyMetadata(default(ObservableCollection<Model_IconDefinition>)));

    public string SelectedIcon
    {
        get => (string)GetValue(SelectedIconProperty);
        set => SetValue(SelectedIconProperty, value);
    }

    public ObservableCollection<Model_IconDefinition> RecentlyUsedIcons
    {
        get => (ObservableCollection<Model_IconDefinition>)GetValue(RecentlyUsedIconsProperty);
        set => SetValue(RecentlyUsedIconsProperty, value);
    }

    public IconPickerControl()
    {
        InitializeComponent();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Implement icon search filtering logic here
        var searchText = SearchBox.Text.ToLower();
        // Filter AllIconsGrid items based on tooltip/name
    }
}
