using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_Dialog_PartInfoModal : ContentDialog
{
    public ViewModel_Dunnage_PartInfoModal ViewModel { get; }

    public View_Dunnage_Dialog_PartInfoModal(ViewModel_Dunnage_PartInfoModal viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        Title = ViewModel.Heading;
    }
}
