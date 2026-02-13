using System.Diagnostics;
using System.Reflection;
using System.Windows;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.Windows.Main_Window;
using MTM_Waitlist_Application_2._0.WinForms.JobDetails;
using MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Windows.UserLogin
{
    public partial class UserLogin : Window
    {
        public static string? UserName;
        public static string? Password;
        public static string? FullName;
        public static string? Shift;
        public static string? Role;
        public static bool UserAdded = false;

        public UserLogin()
        {
            try
            {
                InitializeComponent();
                NewUserRole.SelectedIndex = 0;
                NewUserRole.IsEnabled = false;
                this.Height = 150;
                this.Width = 300;
                CenterWindow();
                UserID.Focus();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                UserName = UserID.Text;
                Password = UserPin.Password;

                SqlCommands.GetUser();
                if (FullName != null && Shift != null && Role != null)
                {
                    if (Application.Current.MainWindow != null) Application.Current.MainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid Username or Password.");
                }
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
                UserLoginGrid.Visibility = Visibility.Collapsed;
                NewUserGrid.Visibility = Visibility.Visible;
                NewUserSaveButton.IsEnabled = false;
                this.Height = 175;
                this.Width = 600;
                CenterWindow();
                NewUserFirstName.Focus();
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

        private void NewUserSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCommands.AddUser(NewUserFirstName.Text, NewUserLastName.Text, NewUserPin.Password, NewUserShift.Text, NewUserRole.Text);
                if (UserAdded)
                {
                    if (Application.Current.MainWindow == null)
                    {
                        Window? mainWindow = Application.Current.MainWindow;
                        if (mainWindow != null) mainWindow.IsEnabled = true;
                        UserAdded = false;
                        this.Close();
                    }
                    else
                    {
                        UserAdded = false;
                        UserLoginGrid.Visibility = Visibility.Visible;
                        NewUserGrid.Visibility = Visibility.Collapsed;
                        this.Height = 150;
                        this.Width = 300;
                        CenterWindow();
                        UserID.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void NewUserBackButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NewUserBackButton.Content.ToString() == "Exit")
                {
                    Window? mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null) mainWindow.IsEnabled = true;
                    UserAdded = false;
                    this.Close();
                }
                else
                {
                    UserLoginGrid.Visibility = Visibility.Visible;
                    NewUserGrid.Visibility = Visibility.Collapsed;
                    this.Height = 150;
                    this.Width = 300;
                    CenterWindow();
                    UserID.Focus();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        private void PasswordChecker(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NewUserPin.Password == NewUserPinValidate.Password && NewUserPin != null)
                {
                    NewUserSaveButton.IsEnabled = true;
                }
                else
                {
                    NewUserSaveButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public void CenterWindow()
        {
            try
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double windowWidth = this.Width;
                double windowHeight = this.Height;
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

    }
}
