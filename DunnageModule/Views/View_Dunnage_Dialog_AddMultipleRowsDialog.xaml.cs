using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.DunnageModule.Views
{
    /// <summary>
    /// Dialog for adding multiple rows at once
    /// </summary>
    public sealed partial class View_Dunnage_Dialog_AddMultipleRowsDialog : ContentDialog
    {
        public int RowCount { get; private set; }

        public View_Dunnage_Dialog_AddMultipleRowsDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            RowCount = (int)RowCountNumberBox.Value;
        }
    }
}
