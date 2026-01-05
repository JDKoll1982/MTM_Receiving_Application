using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Routing.ViewModels;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingEditModeView : Page
{
    public RoutingEditModeViewModel ViewModel { get; }

    public RoutingEditModeView()
    {
        ViewModel = App.GetService<RoutingEditModeViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private async void OnDataGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        await EditSelectedLabelCommand.ExecuteAsync(null);
    }

    private async void EditSelectedLabelCommand_ExecuteAsync(object? parameter)
    {
        if (ViewModel.SelectedLabel == null) return;

        // Create a copy for editing
        var editedLabel = new Model_RoutingLabel
        {
            Id = ViewModel.SelectedLabel.Id,
            PONumber = ViewModel.SelectedLabel.PONumber,
            POLine = ViewModel.SelectedLabel.POLine,
            PartID = ViewModel.SelectedLabel.PartID,
            RecipientID = ViewModel.SelectedLabel.RecipientID,
            Quantity = ViewModel.SelectedLabel.Quantity,
            OtherReasonID = ViewModel.SelectedLabel.OtherReasonID,
            EmployeeNumber = ViewModel.SelectedLabel.EmployeeNumber,
            CreatedDate = ViewModel.SelectedLabel.CreatedDate
        };

        // Create edit dialog
        var dialog = new ContentDialog
        {
            Title = "Edit Routing Label",
            PrimaryButtonText = "Save Changes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        // Create dialog content
        var grid = new Grid
        {
            RowSpacing = 12,
            ColumnSpacing = 12,
            Padding = new Thickness(24)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // PO Number (read-only)
        var poLabel = new TextBlock { Text = "PO Number:", VerticalAlignment = VerticalAlignment.Center };
        var poValue = new TextBox { Text = editedLabel.PONumber ?? "N/A", IsReadOnly = true };
        Grid.SetRow(poLabel, 0);
        Grid.SetColumn(poLabel, 0);
        Grid.SetRow(poValue, 0);
        Grid.SetColumn(poValue, 1);
        grid.Children.Add(poLabel);
        grid.Children.Add(poValue);

        // Part ID (read-only)
        var partLabel = new TextBlock { Text = "Part ID:", VerticalAlignment = VerticalAlignment.Center };
        var partValue = new TextBox { Text = editedLabel.PartID ?? "N/A", IsReadOnly = true };
        Grid.SetRow(partLabel, 1);
        Grid.SetColumn(partLabel, 0);
        Grid.SetRow(partValue, 1);
        Grid.SetColumn(partValue, 1);
        grid.Children.Add(partLabel);
        grid.Children.Add(partValue);

        // Recipient (editable ComboBox)
        var recipientLabel = new TextBlock { Text = "Recipient:", VerticalAlignment = VerticalAlignment.Center };
        var recipientCombo = new ComboBox
        {
            ItemsSource = ViewModel.AllRecipients,
            DisplayMemberPath = "Name",
            SelectedValuePath = "Id",
            SelectedValue = editedLabel.RecipientID,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetRow(recipientLabel, 2);
        Grid.SetColumn(recipientLabel, 0);
        Grid.SetRow(recipientCombo, 2);
        Grid.SetColumn(recipientCombo, 1);
        grid.Children.Add(recipientLabel);
        grid.Children.Add(recipientCombo);

        // Quantity (editable NumberBox)
        var qtyLabel = new TextBlock { Text = "Quantity:", VerticalAlignment = VerticalAlignment.Center };
        var qtyBox = new NumberBox
        {
            Value = editedLabel.Quantity,
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };
        Grid.SetRow(qtyLabel, 3);
        Grid.SetColumn(qtyLabel, 0);
        Grid.SetRow(qtyBox, 3);
        Grid.SetColumn(qtyBox, 1);
        grid.Children.Add(qtyLabel);
        grid.Children.Add(qtyBox);

        // Created Date (read-only)
        var dateLabel = new TextBlock { Text = "Created Date:", VerticalAlignment = VerticalAlignment.Center };
        var dateValue = new TextBox { Text = editedLabel.CreatedDate.ToString("g"), IsReadOnly = true };
        Grid.SetRow(dateLabel, 4);
        Grid.SetColumn(dateLabel, 0);
        Grid.SetRow(dateValue, 4);
        Grid.SetColumn(dateValue, 1);
        grid.Children.Add(dateLabel);
        grid.Children.Add(dateValue);

        dialog.Content = grid;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Update edited label with new values
            editedLabel.RecipientID = (int)(recipientCombo.SelectedValue ?? editedLabel.RecipientID);
            editedLabel.Quantity = (int)qtyBox.Value;

            // Save changes
            await ViewModel.SaveEditedLabelCommand.ExecuteAsync(editedLabel);
        }
    }

    private CommunityToolkit.Mvvm.Input.IAsyncRelayCommand EditSelectedLabelCommand => new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(EditSelectedLabelCommand_ExecuteAsync);
}
