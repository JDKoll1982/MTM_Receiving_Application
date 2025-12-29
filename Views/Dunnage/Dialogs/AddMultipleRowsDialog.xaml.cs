using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Views.Dunnage.Dialogs
{
    /// <summary>
    /// Dialog for adding multiple rows at once
    /// </summary>
    public sealed partial class AddMultipleRowsDialog : ContentDialog
    {
        public int RowCount { get; private set; }

        public AddMultipleRowsDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            RowCount = (int)RowCountNumberBox.Value;
        }
    }
}
