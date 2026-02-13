using System.Diagnostics;
using System.Windows;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.Windows.UserLogin;
using MySql.Data.MySqlClient;
using MessageBox = System.Windows.MessageBox;

namespace MTM_Waitlist_Application_2._0.Core
{
    public static class ErrorHandler
    {
        private static readonly Dictionary<string, DateTime> LastErrorTimes = new Dictionary<string, DateTime>();
        private static readonly TimeSpan ErrorCooldown = TimeSpan.FromSeconds(10); // Adjust the cooldown period as needed
        private static bool _isShutdownMessageShown = false; // Flag to track if the shutdown message has been shown

        public static void HandleError(string methodName, Exception ex)
        {
            if (ShouldShowError(methodName))
            {
                var stackTrace = new StackTrace(ex, true);
                var frame = stackTrace.GetFrame(0);
                var lineNumber = frame?.GetFileLineNumber() ?? -1;

                var message = $"Method: {methodName}\n" +
                              $"Line: {lineNumber}\n" +
                              $"Error: {ex.Message}\n" +
                              $"Exception Type: {ex.GetType().FullName}\n" +
                              $"Stack Trace: {ex.StackTrace}\n" +
                              $"Inner Exception: {ex.InnerException?.Message ?? "None"}";

                ShowErrorMessageAndShutdown(methodName, message);
            }
        }

        public static void ShowError(string method, string message)
        {
            if (ShouldShowError(method))
            {
                var errorMessage = $"Method: {method}\n" +
                                   $"An error occurred: {message}";

                ShowErrorMessageAndShutdown(method, errorMessage);
            }
        }

        private static bool ShouldShowError(string method)
        {
            if (LastErrorTimes.TryGetValue(method, out var lastErrorTime))
            {
                if (DateTime.Now - lastErrorTime < ErrorCooldown)
                {
                    return false;
                }
            }

            LastErrorTimes[method] = DateTime.Now;
            return true;
        }

        private static void LogErrorToDatabase(string method, string message)
        {
            string? user = null;
            string? connectionString = null;

            try
            {
                connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null);
                user = UserLogin.FullName ?? Environment.UserName;

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO waitlist_errors (method, message, date, time, user) VALUES (@method, @message, @date, @time, @user)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@method", method);
                        command.Parameters.AddWithValue("@message", method); // Only log the class name (method name)
                        command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@time", DateTime.Now.ToString("HH:mm:ss"));
                        command.Parameters.AddWithValue("@user", user);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // If logging fails, proceed to shutdown
            }
        }

        private static void ShowErrorMessageAndShutdown(string method, string message)
        {
            if (!_isShutdownMessageShown)
            {
                _isShutdownMessageShown = true;

                // Log the error to the database before showing the message box
                LogErrorToDatabase(method, message);

                // Show the error message
                MessageBox.Show(message + "\nThe error was reported to the developer. The application will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Stop any background activities here
                StopBackgroundActivities();

                // Forcefully kill the application after the user clicks "OK"
                ForceShutdown();
            }
        }

        private static void StopBackgroundActivities()
        {
            // Implement logic to stop any background activities
            // For example, you can cancel any running tasks, close database connections, etc.
        }

        private static void ForceShutdown()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}