using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using MTM_Waitlist_Application_2._0.Windows.UserLogin;
using MessageBox = System.Windows.MessageBox;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using Mysqlx.Crud;
using Org.BouncyCastle.Asn1.Ocsp;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Metadata;
using DocumentFormat.OpenXml.Wordprocessing;
using MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup;
using Org.BouncyCastle.Asn1.Cms;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Core.Database_Classes
{
    public class SqlCommands
    {
        public static ResetIDsOnStartup ResetIDsOnStartup = new();

        public class GetActiveWaitlistData // Class to store the objects from the database
        {
            public int Id { get; set; }
            public string? WorkCenter { get; set; }
            public string? RequestType { get; set; }
            public string? Request { get; set; }
            public string? RequestPriority { get; set; }
            public string? MHandler { get; set; }
            public DateTime RemainingTime { get; set; }
            public TimeSpan TimeRemaining { get; set; }
        }

        public class Users // Class to store the objects from the database
        {
            public int Id { get; set; }
            public string? User { get; set; }
            public string? Pin { get; set; }
            public string? FullName { get; set; }
            public string? Shift { get; set; }
            public string? UserType { get; set; }
        }

        public class GetHistoryData // Class to store the objects from the database
        {
            public int Id { get; set; }
            public string? WorkCenter { get; set; }
            public string? RequestType { get; set; }
            public string? Request { get; set; }
            public string? RequestPriority { get; set; }
            public string? MHandler { get; set; }
            public TimeSpan RequestTime { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan StopTime { get; set; }
            public TimeSpan CancelTime { get; set; }
        }

        public static string GetConnectionString(string? server, string? database, string? uid, string? password) // Method to get the connection string
        {
            try
            {
                if (server == null)
                {
                    server = ServerSettings.Default.addressSetting;
                }
                if (database == null)
                {
                    database = "mtm_waitlist";
                }
                if (uid == null)
                {
                    uid = "root";
                }
                if (password == null)
                {
                    password = "root";
                }

                string port = ServerSettings.Default.portSetting;
                string connectionString = $"SERVER={server};PORT={port};DATABASE={database};UID={uid};PASSWORD={password};";
                return connectionString;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return "";
            }
        }


        public static ObservableCollection<GetActiveWaitlistData> GetActiveWaitlist() // Method to get the active waitlist from the database
        {
            ObservableCollection<GetActiveWaitlistData> returnThese = new();

            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                using MySqlCommand command = new("SELECT * FROM `waitlist_active`", connection);
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    GetActiveWaitlistData a = new()
                    {
                        Id = reader.GetInt32(0),
                        WorkCenter = reader.GetString(1),
                        RequestType = reader.GetString(2),
                        Request = reader.GetString(3),
                        RequestPriority = reader.GetString(4),
                        MHandler = reader.GetString(5),
                        RemainingTime = reader.GetDateTime(7)
                    };
                    returnThese.Add(a);
                }
            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return returnThese;
        }


        public static ObservableCollection<GetActiveWaitlistData> AcceptTask(int? id) // Method to accept a task from the database
        {
            ObservableCollection<GetActiveWaitlistData> returnThese = new();

            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                var fullname = UserLogin.FullName;

                using MySqlCommand setFullNameCommand = new("UPDATE `waitlist_active` SET `MHandler`=@MHandler WHERE `ID` = @ID", connection);
                setFullNameCommand.Parameters.AddWithValue("@MHandler", fullname);
                setFullNameCommand.Parameters.AddWithValue("@ID", id);
                setFullNameCommand.ExecuteNonQuery();

                using MySqlCommand setStartTimeCommand = new("UPDATE `waitlist_active` SET `StartTime` = DEFAULT WHERE `ID` = @ID", connection);
                setStartTimeCommand.Parameters.AddWithValue("@ID", id);
                setStartTimeCommand.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return returnThese;
        }


        public ObservableCollection<string> ComboBoxFiller(string column) // Method to fill the comboboxes
        {
            var connectionString = GetConnectionString(null, null, null, null);
            string query = "SELECT " + column + " FROM application_database";

            ObservableCollection<string> data = new ObservableCollection<string>();

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new(query, connection);
                connection.Open();

                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return data;
        }

        public static void AddToWaitlist(GetActiveWaitlistData getActiveWaitlistData) // Method to add a task to the waitlist
        {
            var requestTime = DateTime.Now;

            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                using MySqlCommand command = new(
                    "INSERT INTO `waitlist_active` (`WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `TimeRemaining`, `RequestTime`, `StartTime`) " +
                    "VALUES (@WorkCenter, @RequestType, @Request, @RequestPriority, @MHandler, @TimeRemaining, @RequestTime, null)", connection);

                command.Parameters.AddWithValue("@WorkCenter", getActiveWaitlistData.WorkCenter); // Add WorkCenter to the database
                command.Parameters.AddWithValue("@RequestType", getActiveWaitlistData.RequestType); // Add RequestType to the database
                command.Parameters.AddWithValue("@Request", getActiveWaitlistData.Request); // Add Request to the database
                command.Parameters.AddWithValue("@RequestPriority", getActiveWaitlistData.RequestPriority); // Add RequestPriority to the database
                command.Parameters.AddWithValue("@MHandler", ""); // Add MHandler to the database
                command.Parameters.AddWithValue("@TimeRemaining", getActiveWaitlistData.RemainingTime + getActiveWaitlistData.TimeRemaining); // Add RemainingTime to the database
                command.Parameters.AddWithValue("@RequestTime", requestTime); // Add RequestTime to the database

                command.ExecuteNonQuery();

                ResetIDsOnStartup.ResetTableId("waitlist_history");
                ResetIDsOnStartup.ResetTableId("waitlist_active");
                ResetIDsOnStartup.ResetTableId("application_database");
                ResetIDsOnStartup.ResetTableId("waitlist_canceled");
                ResetIDsOnStartup.ResetTableId("waitlist_users");
            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void AddToWaitlistSetup(PooledData getActiveWaitlistData) // Method to add a task to the waitlist
        {
            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                using MySqlCommand command = new(
                    "INSERT INTO `waitlist_active` (`ID`, `WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `TimeRemaining`, `RequestTime`, `Component1`, `Component2`, `Component3`, `Component4`, `Component5`, `Component6`, `Component7`, `Component8`, `Component9`, `Component10`, `Component11`, `Component12`, `PartID`, `Operation`, `Dunnage`, `Skid`, `Cardboard`, `Boxes`, `Other1`, `Other2`, `Other3`, `Other4`, `Other5`, `PartType`)" +
                    "VALUES(NULL, @WorkStation, @RequestType, @Request, @RequestPriority, @MHandler, @TimeRemaining, CURRENT_TIMESTAMP, @Component1, @Component2, @Component3, @Component4, @Component5, @Component6, @Component7, @Component8, @Component9, @Component10, @Component11, @Component12, @PartID, @Operation, @Dunnage, @Skid, @Cardboard, @Boxes, @Other1, @Other2, @Other3, @Other4, @Other5, @PartType)",
                    connection);

                command.Parameters.AddWithValue("@WorkStation",
                    getActiveWaitlistData.WorkStation); // Add WorkCenter to the database
                command.Parameters.AddWithValue("@RequestType", "Setup Request"); // Add RequestType to the database
                command.Parameters.AddWithValue("@Request",
                    getActiveWaitlistData.PartDescription); // Add Request to the database
                command.Parameters.AddWithValue("@RequestPriority", "Normal"); // Add RequestPriority to the database
                command.Parameters.AddWithValue("@MHandler", ""); // Add MHandler to the database
                command.Parameters.AddWithValue("@TimeRemaining",
                    DateTime.Now + TimeSpan.FromMinutes(60)); // Add RemainingTime to the database
                command.Parameters.AddWithValue("@Component1", getActiveWaitlistData.Component1);
                command.Parameters.AddWithValue("@Component2", getActiveWaitlistData.Component2);
                command.Parameters.AddWithValue("@Component3", getActiveWaitlistData.Component3);
                command.Parameters.AddWithValue("@Component4", getActiveWaitlistData.Component4);
                command.Parameters.AddWithValue("@Component5", getActiveWaitlistData.Component5);
                command.Parameters.AddWithValue("@Component6", getActiveWaitlistData.Component6);
                command.Parameters.AddWithValue("@Component7", getActiveWaitlistData.Component7);
                command.Parameters.AddWithValue("@Component8", getActiveWaitlistData.Component8);
                command.Parameters.AddWithValue("@Component9", getActiveWaitlistData.Component9);
                command.Parameters.AddWithValue("@Component10", getActiveWaitlistData.Component10);
                command.Parameters.AddWithValue("@Component11", getActiveWaitlistData.Component11);
                command.Parameters.AddWithValue("@Component12", getActiveWaitlistData.Component12);
                command.Parameters.AddWithValue("@PartID", getActiveWaitlistData.PartNumber);
                command.Parameters.AddWithValue("@Operation", getActiveWaitlistData.Operation);
                command.Parameters.AddWithValue("@Dunnage", getActiveWaitlistData.DieFgt);
                command.Parameters.AddWithValue("@Skid", getActiveWaitlistData.Skid);
                command.Parameters.AddWithValue("@Cardboard", getActiveWaitlistData.Cardboard);
                command.Parameters.AddWithValue("@Boxes", getActiveWaitlistData.Box);
                command.Parameters.AddWithValue("@Other1", getActiveWaitlistData.Other1);
                command.Parameters.AddWithValue("@Other2", getActiveWaitlistData.Other2);
                command.Parameters.AddWithValue("@Other3", getActiveWaitlistData.Other3);
                command.Parameters.AddWithValue("@Other4", getActiveWaitlistData.Other4);
                command.Parameters.AddWithValue("@Other5", getActiveWaitlistData.Other5);
                command.Parameters.AddWithValue("@PartType", getActiveWaitlistData.JobType);
                command.ExecuteNonQuery();
                connection.Close();

                connection.Open();

                ObservableCollection<string> data = new ObservableCollection<string>();

                MySqlCommand command2 = new("SELECT * FROM `waitlist_current_jobs` WHERE `work_center` = @workcenter;", connection);

                command2.Parameters.AddWithValue("@workcenter", getActiveWaitlistData.WorkStation);

                using MySqlDataReader reader = command2.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(reader.GetString(1));
                }
                connection.Close();

                if (data.Count == 0)
                {
                    connection.Open();
                    MySqlCommand command3 = new("INSERT INTO `waitlist_current_jobs` (`work_center`, `work_order`, `part_id`, `operation`) VALUES (@workcenter, @workorder, @partid, @operation);", connection);
                    command3.Parameters.AddWithValue("@workcenter", getActiveWaitlistData.WorkStation);
                    command3.Parameters.AddWithValue("@workorder", getActiveWaitlistData.WorkOrder);
                    command3.Parameters.AddWithValue("@partid", getActiveWaitlistData.PartNumber);
                    command3.Parameters.AddWithValue("@operation", getActiveWaitlistData.Operation);
                    command3.ExecuteNonQuery();
                    connection.Close();
                }
                else
                {
                    connection.Open();
                    MySqlCommand command3 = new("UPDATE `waitlist_current_jobs` SET `work_order`= @workorder, `part_id`= @partid, `operation`= @operation WHERE `work_center` = @workcenter;", connection);
                    command3.Parameters.AddWithValue("@workorder", getActiveWaitlistData.WorkOrder);
                    command3.Parameters.AddWithValue("@partid", getActiveWaitlistData.PartNumber);
                    command3.Parameters.AddWithValue("@operation", getActiveWaitlistData.Operation);
                    command3.Parameters.AddWithValue("@workcenter", getActiveWaitlistData.WorkStation);
                    command3.ExecuteNonQuery();
                    connection.Close();
                }

                ResetIDsOnStartup.ResetTableId("waitlist_history");
                ResetIDsOnStartup.ResetTableId("waitlist_active");
                ResetIDsOnStartup.ResetTableId("application_database");
                ResetIDsOnStartup.ResetTableId("waitlist_canceled");
                ResetIDsOnStartup.ResetTableId("waitlist_users");



            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void GetUser()
        {
            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();
                MySqlCommand command = new("SELECT * FROM `waitlist_users`;", connection);
                using MySqlDataReader reader = command.ExecuteReader();
                {
                    while (reader.Read())
                    {
                        Users user = new()
                        {
                            Id = reader.GetInt32(0),
                            User = reader.GetString(1),
                            Pin = reader.GetString(2),
                            FullName = reader.GetString(3),
                            Shift = reader.GetString(4),
                            UserType = reader.GetString(5),
                        };
                        if (user.User == UserLogin.UserName && user.Pin == UserLogin.Password)
                        {
                            UserLogin.FullName = user.FullName;
                            UserLogin.Shift = user.Shift;
                            UserLogin.Role = user.UserType;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void CancelTask(int id) // Method to cancel a task from the waitlist and send it to the history
        {
            var fullname = UserLogin.FullName;
            var currentTime = DateTime.Now;
            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();
                MySqlCommand insertIntoHistoryCommand = new(
                "INSERT INTO `waitlist_canceled` (`WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `TimeRemaining`, `RequestTime`, `StartTime`, `CanceledTime`) " +
                    "SELECT `WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `TimeRemaining`, `RequestTime`, `StartTime`, @Time " +
                    "FROM `waitlist_active` WHERE `ID` = @ID; " +
                    "DELETE FROM `waitlist_active` WHERE `ID` = @ID; " +
                    "UPDATE `waitlist_canceled` SET `MHandler` = @MHandler WHERE `ID` = LAST_INSERT_ID();", connection);

                insertIntoHistoryCommand.Parameters.AddWithValue("@ID", id);
                insertIntoHistoryCommand.Parameters.AddWithValue("@MHandler", fullname);
                insertIntoHistoryCommand.Parameters.AddWithValue("@Time", currentTime);

                insertIntoHistoryCommand.ExecuteNonQuery();
                ResetIDsOnStartup.ResetTableId("waitlist_history");
                ResetIDsOnStartup.ResetTableId("waitlist_active");
                ResetIDsOnStartup.ResetTableId("application_database");
                ResetIDsOnStartup.ResetTableId("waitlist_canceled");
                ResetIDsOnStartup.ResetTableId("waitlist_users");

            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }


        public static void CompleteTask(int id, string request, string partid, string workcenter, string mhandler, DateTime requesttime)
        {
            var fullname = UserLogin.FullName;
            var currentTime = DateTime.Now;

            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                // Insert into waitlist_history excluding the ID column
                MySqlCommand insertCommand = new(
                    "INSERT INTO `waitlist_history` (`WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `TimeRemaining`, `RequestTime`, `StartTime`, `CompleteTime`) " +
                    "SELECT `WorkCenter`, `RequestType`, `Request`, `RequestPriority`, `MHandler`, `TimeRemaining`, `RequestTime`, `StartTime`, @Time " +
                    "FROM `waitlist_active` WHERE `ID` = @ID;", connection);
                insertCommand.Parameters.AddWithValue("@ID", id);
                insertCommand.Parameters.AddWithValue("@Time", currentTime);
                insertCommand.ExecuteNonQuery();

                // Delete from waitlist_active
                MySqlCommand deleteCommand = new("DELETE FROM `waitlist_active` WHERE `ID` = @ID;", connection);
                deleteCommand.Parameters.AddWithValue("@ID", id);
                deleteCommand.ExecuteNonQuery();

                // Update the MHandler in waitlist_history for the newly inserted row
                MySqlCommand updateHandlerCommand = new("UPDATE `waitlist_history` SET `MHandler` = @MHandler WHERE `ID` = LAST_INSERT_ID();", connection);
                updateHandlerCommand.Parameters.AddWithValue("@MHandler", fullname);
                updateHandlerCommand.ExecuteNonQuery();

                // Check if the request contains "- Take To NCM."
                if (request.Contains("- Take To NCM."))
                {
                    MySqlCommand ncmCommand = new(
                        "INSERT INTO `waitlist_ncm_report` (`PartID`, `WorkCenter`, `RequestTime`, `MHandler`) " +
                        "VALUES (@PartID, @WorkCenter, @RequestTime, @MHandler);", connection);
                    ncmCommand.Parameters.AddWithValue("@PartID", partid);
                    ncmCommand.Parameters.AddWithValue("@WorkCenter", workcenter);
                    ncmCommand.Parameters.AddWithValue("@RequestTime", requesttime);
                    ncmCommand.Parameters.AddWithValue("@MHandler", mhandler);

                    ncmCommand.ExecuteNonQuery();
                }

                ResetIDsOnStartup.ResetTableId("waitlist_history");
                ResetIDsOnStartup.ResetTableId("waitlist_active");
                ResetIDsOnStartup.ResetTableId("application_database");
                ResetIDsOnStartup.ResetTableId("waitlist_canceled");
                ResetIDsOnStartup.ResetTableId("waitlist_users");
                ResetIDsOnStartup.ResetTableId("waitlist_ncm_report");
            }
            catch (MySqlException ex)
            {
                ErrorHandler.HandleError(nameof(CompleteTask), ex);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleError(nameof(CompleteTask), ex);
            }
        }


        public static TimeSpan GetTimeRemaining(string? requestType) // Method to get the time remaining for a task
        {
            string connectionString = GetConnectionString(null, null, null, null);
            string query = "SELECT * FROM application_database";

            TimeSpan timeRemaining = new TimeSpan(0, 15, 0);

            try
            {
                using MySqlConnection connection = new(connectionString);
                connection.Open();

                MySqlCommand command = new(query, connection);
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetString(2) == requestType)
                    {
                        timeRemaining = reader.GetTimeSpan(3);
                    }
                }
            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return timeRemaining;
        }


        public static void AddUser(string firstName, string lastName, string pin, string shift, string? role)
        {
            if (role == null)
            {
                role = "Operator";
            }

            string userName = firstName.Substring(0, 1) + lastName.Substring(0, Math.Min(4, lastName.Length));
            userName = userName.ToUpper();
            string fullName = firstName + " " + lastName;

            if (!VerifyUserExists(userName))
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Do you want to add this user?\nFull Name: {fullName}\nShift: {shift}\nRole: {role}\nUser Name: {userName}",
                    "Confirmation",
                    MessageBoxButton.YesNo
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                        connection.Open();

                        using MySqlCommand command = new(
                            "INSERT INTO `waitlist_users` (`User`, `Pin`, `Full Name`, `Shift`, `User Type`) VALUES (@User, @Pin, @FullName, @Shift, @UserType);",
                            connection
                        );

                        command.Parameters.AddWithValue("@User", userName);
                        command.Parameters.AddWithValue("@Pin", pin);
                        command.Parameters.AddWithValue("@FullName", fullName);
                        command.Parameters.AddWithValue("@Shift", shift);
                        command.Parameters.AddWithValue("@UserType", role);

                        command.ExecuteNonQuery();
                        UserLogin.UserAdded = true;
                        MessageBox.Show("User added.");
                    }
                    catch (MySqlException ex)
                    {
                        ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("User not added.");
                }
            }
        }


        public static bool VerifyUserExists(string userName)
        {
            try
            {
                using MySqlConnection connection = new(GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                using MySqlCommand command = new("SELECT * FROM `waitlist_users`;", connection);
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Users user = new()
                    {
                        Id = reader.GetInt32(0),
                        User = reader.GetString(1),
                        Pin = reader.GetString(2),
                        FullName = reader.GetString(3),
                        Shift = reader.GetString(4),
                        UserType = reader.GetString(5),
                    };

                    if (user.User == userName)
                    {
                        MessageBox.Show("User already exists!");
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }

            return false;
        }

    }
}
