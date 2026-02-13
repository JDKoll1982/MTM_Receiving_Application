using MTM_Waitlist_Application_2._0.Core;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Windows.Main_Window
{
    public partial class MainWindow
    {
        public static bool UserIsAdmin;
        public static bool ProgramIsRunning = true;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.Title = "MTM Waitlist Application " + UserLogin.UserLogin.FullName + " | " + UserLogin.UserLogin.Role;
                UserIsAdmin = UserLogin.UserLogin.Role != "Operator";
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }


        private void NewUserButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UserLogin.UserLogin.Role != "Operator")
                {
                    UserLogin.UserLogin userLogin = new UserLogin.UserLogin();
                    this.IsEnabled = false;
                    userLogin.UserLoginGrid.Visibility = Visibility.Collapsed;
                    userLogin.NewUserGrid.Visibility = Visibility.Visible;
                    userLogin.NewUserRole.IsEnabled = true;
                    userLogin.NewUserSaveButton.IsEnabled = false;
                    userLogin.NewUserBackButton.Content = "Exit";
                    userLogin.Height = 175;
                    userLogin.Width = 600;
                    userLogin.CenterWindow();
                    userLogin.NewUserFirstName.Focus();
                    userLogin.ShowDialog();
                }
                else
                {
                    MessageBox.Show("You do not have permission to access this feature.");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void ExitButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}
