using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Volvo.Views;

public sealed partial class View_Volvo_PartNumberEditDialog : ContentDialog
{
    public ViewModel_Volvo_PartNumberEditDialog ViewModel { get; } = new();

    public Model_VolvoPart? SelectedPart => ViewModel.SelectedPart;

    public View_Volvo_PartNumberEditDialog()
    {
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
    }

    
    public void Initialize(Model_VolvoPartNumberEditDialog model)
    {
        ViewModel.Initialize(model);
        DataContext = ViewModel;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        Opened += OnDialogOpened;
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void OnDialogOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        SearchTextBox.Focus(FocusState.Programmatic);
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (!ViewModel.TryValidateSelection(out _))
        {
            args.Cancel = true;
        }
    }
}
