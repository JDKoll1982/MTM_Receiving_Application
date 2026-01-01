using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Views.Shared;

/// <summary>
/// Centralized help dialog for displaying contextual help content
/// </summary>
public sealed partial class Shared_HelpDialog : ContentDialog
{
    public Shared_HelpDialogViewModel ViewModel { get; }

    public Shared_HelpDialog()
    {
        ViewModel = App.GetService<Shared_HelpDialogViewModel>();
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
