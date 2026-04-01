using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_SelectExistingSpecsDialog : ContentDialog
{
    private readonly List<Model_DunnageSpecTemplateOption> _allOptions;

    public Model_DunnageSpecTemplateOption? SelectedTemplate { get; private set; }

    public View_Dunnage_SelectExistingSpecsDialog(List<Model_DunnageSpecTemplateOption> options)
    {
        InitializeComponent();

        _allOptions = options;
        TemplatesListView.ItemsSource = new ObservableCollection<Model_DunnageSpecTemplateOption>(
            options
        );
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchTextBox.Text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(searchText)
            ? _allOptions
            : _allOptions
                .Where(option =>
                    option.PartId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || option.HomeLocation.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || option.SpecSummary.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || option.Notes.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

        TemplatesListView.ItemsSource = new ObservableCollection<Model_DunnageSpecTemplateOption>(
            filtered
        );
    }

    private void TemplatesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedTemplate = TemplatesListView.SelectedItem as Model_DunnageSpecTemplateOption;
        IsPrimaryButtonEnabled = SelectedTemplate is not null;
    }
}
