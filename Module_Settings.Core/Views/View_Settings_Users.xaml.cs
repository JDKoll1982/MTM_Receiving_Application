using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_Users : Page
{
    public ViewModel_Settings_Users ViewModel { get; }

    public View_Settings_Users(ViewModel_Settings_Users viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.XamlRoot = XamlRoot;
        await ViewModel.LoadUsersCommand.ExecuteAsync(null);
    }

    private void VisualPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel.EditingUser is not null)
        {
            ViewModel.EditingUser.VisualPassword = VisualPasswordBox.Password;
        }
    }

    private void EditSelectedUser_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedUser is Model_User user)
        {
            ViewModel.EditUserCommand.Execute(user);
        }
    }

    private void DeactivateSelectedUser_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedUser is Model_User user)
        {
            ViewModel.DeactivateUserCommand.Execute(user);
        }
    }
}
