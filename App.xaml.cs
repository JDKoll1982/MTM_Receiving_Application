using Microsoft.Extensions.DependencyInjection;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Services;
using MTM_Waitlist_Application_2._0.ViewModels;
using MTM_Waitlist_Application_2._0.Views;
using System.Windows;
using MTM_Waitlist_Application_2._0.Stores;
using MTM_Waitlist_Application_2._0.Windows.Main_Window;
using MTM_Waitlist_Application_2._0.Windows.UserLogin;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;
using MySql.Data.MySqlClient;
using System.Security.Principal;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0
{
    public partial class App : System.Windows.Application
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly SelectedWaitlistTaskStore? _selectedWaitlistTaskStore;

        public App()
        {
            try
            {
                _selectedWaitlistTaskStore = new SelectedWaitlistTaskStore();

                IServiceCollection services = new ServiceCollection();

                services.AddSingleton<MainWindow>(provider => new MainWindow
                {
                    DataContext = provider.GetRequiredService<MainWindowViewModel>()
                });

                services.AddSingleton<WaitlistComponentList>(provider => new WaitlistComponentList
                {
                    DataContext = provider.GetRequiredService<WaitlistComponentListViewModel>()
                });

                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<AnalyticsViewModel>();
                services.AddSingleton<HistoryViewModel>();
                services.AddSingleton<MessengerViewModel>();
                services.AddSingleton<WaitlistViewModel>();
                services.AddSingleton<RequestViewModel>();
                services.AddSingleton<SelectedWaitlistTaskStore>();

                services.AddSingleton<INavigationService, NavigationService>();

                services.AddSingleton<Func<Type, ViewModel>>(serviceProvider => viewModelType => (ViewModel)serviceProvider.GetRequiredService(viewModelType));

                _serviceProvider = services.BuildServiceProvider();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var resetIDsOnStartup = new ResetIDsOnStartup();

                resetIDsOnStartup.ResetTableId("waitlist_history");
                resetIDsOnStartup.ResetTableId("waitlist_active");
                resetIDsOnStartup.ResetTableId("application_database");
                resetIDsOnStartup.ResetTableId("waitlist_canceled");
                resetIDsOnStartup.ResetTableId("waitlist_users");



                var mainWindow = _serviceProvider.GetService<MainWindow>();
                WaitlistView waitlistView = new WaitlistView
                {
                    DataContext = new WaitlistViewModel(_selectedWaitlistTaskStore)
                };

                base.OnStartup(e);



                if (UserLogin.UserName == null)
                {
                    UserLogin userLogin = new UserLogin();
                    userLogin.ShowDialog();
                }
                else
                {
                    switch (UserLogin.Role)
                    {
                        case "admin":
                            MessageBox.Show("Welcome, admin!");
                            break;
                        case "pressfloor":
                            MessageBox.Show("Welcome, pressfloor!");
                            break;
                        case "materialhandler":
                            MessageBox.Show("Welcome, materialhandler!");
                            break;
                        case "floorlead":
                            MessageBox.Show("Welcome, floorlead!");
                            break;
                        default:
                            MessageBox.Show("Welcome!");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}
