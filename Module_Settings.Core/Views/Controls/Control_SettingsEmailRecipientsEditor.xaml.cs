using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views.Controls;

public sealed partial class Control_SettingsEmailRecipientsEditor : UserControl
{
    public ViewModel_Settings_EmailRecipientsEditorBase? EditorViewModel { get; set; }

    public Control_SettingsEmailRecipientsEditor()
    {
        InitializeComponent();
    }

    private void EditRecipientButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Model_EmailRecipientSetting recipient })
        {
            EditorViewModel?.EditRecipientCommand.Execute(recipient);
        }
    }

    private void DeleteRecipientButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Model_EmailRecipientSetting recipient })
        {
            EditorViewModel?.DeleteRecipientCommand.Execute(recipient);
        }
    }
}
