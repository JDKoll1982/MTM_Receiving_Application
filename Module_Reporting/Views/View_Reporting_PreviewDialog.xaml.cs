using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Reporting.Views;

public sealed partial class View_Reporting_PreviewDialog : ContentDialog
{
    public ViewModel_Reporting_Main ViewModel { get; }

    public View_Reporting_PreviewDialog(ViewModel_Reporting_Main viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void OnCustomizePreviewClick(object sender, RoutedEventArgs e)
    {
        await ShowCustomizePreviewDialogAsync();
    }

    private async Task ShowCustomizePreviewDialogAsync()
    {
        var contentPanel = new StackPanel { Spacing = 16 };

        contentPanel.Children.Add(
            new TextBlock
            {
                Text =
                    "Choose which modules appear in the preview and which detail columns remain visible. Only fields that contain data for each module are listed.",
                TextWrapping = TextWrapping.WrapWholeWords,
            }
        );

        foreach (var previewModuleCard in ViewModel.PreviewModuleCards)
        {
            var modulePanel = new StackPanel { Spacing = 10 };
            var columnCheckBoxes = new List<CheckBox>();
            var includeModuleCheckBox = new CheckBox
            {
                Content = $"Include {previewModuleCard.ModuleName}",
                IsChecked = previewModuleCard.IsIncluded,
                FontWeight = FontWeights.SemiBold,
            };

            var columnGrid = new Grid { ColumnSpacing = 16, RowSpacing = 8 };
            columnGrid.ColumnDefinitions.Add(new ColumnDefinition());
            columnGrid.ColumnDefinitions.Add(new ColumnDefinition());

            includeModuleCheckBox.Checked += (_, _) =>
            {
                previewModuleCard.IsIncluded = true;
                foreach (var columnCheckBox in columnCheckBoxes)
                {
                    columnCheckBox.IsEnabled = true;
                }
            };
            includeModuleCheckBox.Unchecked += (_, _) =>
            {
                previewModuleCard.IsIncluded = false;
                foreach (var columnCheckBox in columnCheckBoxes)
                {
                    columnCheckBox.IsEnabled = false;
                }
            };

            modulePanel.Children.Add(includeModuleCheckBox);

            if (previewModuleCard.AvailableColumns.Count == 0)
            {
                modulePanel.Children.Add(
                    new TextBlock
                    {
                        Text = "No detail columns with data are available for this module.",
                        Foreground = new SolidColorBrush(Microsoft.UI.Colors.DimGray),
                        TextWrapping = TextWrapping.WrapWholeWords,
                    }
                );
            }
            else
            {
                for (
                    var columnIndex = 0;
                    columnIndex < previewModuleCard.AvailableColumns.Count;
                    columnIndex++
                )
                {
                    var previewColumn = previewModuleCard.AvailableColumns[columnIndex];
                    var columnCheckBox = new CheckBox
                    {
                        Content = previewColumn.Header,
                        IsChecked = previewColumn.IsIncluded,
                        Margin = new Thickness(0),
                        IsEnabled = previewModuleCard.IsIncluded,
                    };

                    columnCheckBox.Checked += (_, _) => previewColumn.IsIncluded = true;
                    columnCheckBox.Unchecked += (_, _) => previewColumn.IsIncluded = false;

                    columnCheckBoxes.Add(columnCheckBox);
                    Grid.SetColumn(columnCheckBox, columnIndex % 2);
                    Grid.SetRow(columnCheckBox, columnIndex / 2);
                    columnGrid.Children.Add(columnCheckBox);
                }

                modulePanel.Children.Add(columnGrid);
            }

            contentPanel.Children.Add(
                new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12),
                    Child = modulePanel,
                }
            );
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Customize Preview",
            PrimaryButtonText = "Done",
            DefaultButton = ContentDialogButton.Primary,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = new ScrollViewer
            {
                MaxHeight = 720,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = contentPanel,
            },
        };

        await dialog.ShowAsync();
    }
}
