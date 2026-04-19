using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;

namespace MTM_Receiving_Application.Module_Dunnage.Views
{
    /// <summary>
    /// Dialog for adding multiple rows at once
    /// </summary>
    public sealed partial class View_Dunnage_Dialog_AddMultipleRowsDialog : ContentDialog
    {
        public int RowCount { get; private set; }
        private readonly IService_Focus _focusService;

        public View_Dunnage_Dialog_AddMultipleRowsDialog()
        {
            InitializeComponent();
            Helper_UI_ContentDialogTheme.ApplyTheme(this);
            _focusService = App.GetService<IService_Focus>();
            _focusService.AttachFocusOnVisibility(this, RowCountNumberBox);
        }

        private void ContentDialog_PrimaryButtonClick(
            ContentDialog sender,
            ContentDialogButtonClickEventArgs args
        )
        {
            RowCount = (int)RowCountNumberBox.Value;
        }
    }
}
