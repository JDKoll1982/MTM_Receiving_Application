using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Shared.Views;

/// <summary>
/// Centralized help dialog for displaying contextual help content
/// </summary>
public sealed partial class View_Shared_HelpDialog : ContentDialog
{
    public ViewModel_Shared_HelpDialog ViewModel { get; }

    public View_Shared_HelpDialog()
    {
        ViewModel = App.GetService<ViewModel_Shared_HelpDialog>();
        InitializeComponent();
    }

    /// <summary>
    /// Sets the help content to display in the dialog
    /// </summary>
    /// <param name="content"></param>
    public void SetHelpContent(Model_HelpContent content)
    {
        _ = ViewModel.LoadHelpContentAsync(content);
    }

    /// <summary>
    /// Handles clicking on a related topic to navigate to it
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RelatedTopics_ItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
    {
        if (e.ClickedItem is Model_HelpContent relatedContent)
        {
            await ViewModel.LoadRelatedTopicAsync(relatedContent);
        }
    }
}

