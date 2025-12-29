using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage.Dialogs;

public sealed partial class Dunnage_AddTypeDialog : ContentDialog
{
    public Dunnage_AddTypeDialogViewModel ViewModel { get; }

    public Dunnage_AddTypeDialog()
    {
        ViewModel = App.GetService<Dunnage_AddTypeDialogViewModel>();
        InitializeComponent();
    }
}
