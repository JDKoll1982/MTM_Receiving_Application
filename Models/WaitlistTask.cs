using System.Collections.ObjectModel;
using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MySql.Data.MySqlClient;

// Encapsulation completed. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Models
{
    public class WaitlistTask
    {
        public int Id { get; set; }
        public string? WorkCenter { get; set; }
        public string? RequestType { get; set; }
        public string? Request { get; set; }
        public string? RequestPriority { get; set; }
        public string TimeRemaining { get; set; }
        public TimeSpan TimeRemainingValue { get; set; }
        public string Status { get; set; }
        public string? MHandler { get; set; }
        public DateTime RequestTime { get; set; }
        public bool IsOverdue { get; set; }

        public WaitlistTask(int id, string? workCenter, string? requestType, string? request, string? requestPriority, TimeSpan timeRemaining, string status, string? mHandler, DateTime requestTime)
        {
            try
            {
                var now = DateTime.Now;
                var jobShouldBeDone = requestTime + timeRemaining;
                var timeSpan = jobShouldBeDone - now;

                var isOverdue = timeSpan < TimeSpan.Zero;
                if (isOverdue)
                {
                    Status = "Overdue!";
                    TimeRemaining = "-" + timeSpan.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    Status = "On Time";
                    TimeRemaining = timeSpan.ToString(@"hh\:mm\:ss");
                }
                TimeRemainingValue = timeSpan;
                Id = id;
                WorkCenter = workCenter;
                RequestType = requestType;
                Request = request;
                RequestPriority = requestPriority;
                MHandler = mHandler;
                RequestTime = requestTime;
                IsOverdue = isOverdue;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static ObservableCollection<SqlCommands.GetActiveWaitlistData> GetActiveWaitlist()
        {
            ObservableCollection<SqlCommands.GetActiveWaitlistData> returnThese = new();

            try
            {
                using MySqlConnection connection = new(SqlCommands.GetConnectionString(null!, null!, null!, null!));
                connection.Open();

                using MySqlCommand command = new("SELECT * FROM `waitlist_active`", connection);
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    SqlCommands.GetActiveWaitlistData a = new()
                    {
                        Id = reader.GetInt32(0),
                        WorkCenter = reader.GetString(1),
                        RequestType = reader.GetString(2),
                        Request = reader.GetString(3),
                        RequestPriority = reader.GetString(4),
                        MHandler = reader.GetString(5),
                        RemainingTime = reader.GetDateTime(6)
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
    }
}
