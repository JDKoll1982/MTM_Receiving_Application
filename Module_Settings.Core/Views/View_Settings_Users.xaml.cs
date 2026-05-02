using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        UserSearchTextBox.Text = ViewModel.SearchText;
        ShowDeactivatedFilterCheckBox.IsChecked = ViewModel.ShowDeactivatedOnly;
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
        if (ViewModel.SelectedUser is not null)
        {
            ViewModel.EditUserCommand.Execute(ViewModel.SelectedUser);
        }
    }

    private async void ToggleSelectedUserActive_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ToggleSelectedUserActiveAsync();
    }

    private void UserSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.SearchText = UserSearchTextBox.Text;
    }

    private void ShowDeactivatedFilterCheckBox_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ShowDeactivatedOnly = ShowDeactivatedFilterCheckBox.IsChecked;
    }

    private async void SaveUserRole_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ViewModel_SettingsUserRoleRow row })
        {
            await ViewModel.SaveUserRoleCommand.ExecuteAsync(row);
        }
    }
}
