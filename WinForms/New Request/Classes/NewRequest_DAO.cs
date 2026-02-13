using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Forms;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Request.Classes
{
    public class NewRequestDao
    {

        public static object? ComboBoxFiller(string column, string table, MySqlDataAdapter dataAdapter, DataTable dataTable, string placeHolder)
        {
            try
            {
                string connectionString = SqlCommands.GetConnectionString(null, null, null, null);
                string commandString = GetCommandString(column, table, placeHolder);

                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new(commandString, connection);

                if (placeHolder == "Enter Coil")
                {
                    command.Parameters.AddWithValue("@mmc", "%MM%");
                }

                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataTable);
                AddPlaceholderRow(dataTable, placeHolder);

                return dataTable;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return null;
            }
        }

        private static string GetCommandString(string column, string table, string placeHolder)
        {
            return placeHolder == "Enter Coil"
                ? $"SELECT DISTINCT {column} FROM {table} WHERE {column} LIKE @mmc;"
                : $"SELECT DISTINCT {column} FROM {table};";
        }

        private static void AddPlaceholderRow(DataTable dataTable, string placeHolder)
        {
            DataRow dataRow = dataTable.NewRow();
            dataRow[0] = placeHolder;
            dataTable.Rows.InsertAt(dataRow, 0);
        }
 
        public static void OnStatrup_StepOne(ComboBox workOrderCb, ComboBox partIdcb, ComboBox operationCb, string workCenterCbText)
        {

            List<WorkCenter> list = new();

            string workCenter = "";
            string? workOrder = "";
            string? partId = "";
            string? operation = "";

            string connectionString = SqlCommands.GetConnectionString(null, null, null, null);

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new MySqlCommand("SELECT work_center FROM waitlist_current_jobs WHERE work_center = @workCenter", connection);
                command.Parameters.AddWithValue("@workCenter", workCenterCbText);

                connection.Open();
                using MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    workCenter = reader.GetString(0);
                    workOrder = OnStartup_StepTwo(workCenter);
                    partId = OnStartup_StepThree(workCenter);
                    operation = OnStartup_StepFour(workCenter);
                }
                if (workOrder != "" && partId != "" && operation != "")
                {
                    list.Add(new WorkCenter { Workcenter = workCenter, Workorder = workOrder, PartId = partId, Operation = operation });

                    workOrderCb.DataSource = list;
                    workOrderCb.DisplayMember = "Workorder";
                    workOrderCb.SelectedIndex = 0;

                    partIdcb.DataSource = list;
                    partIdcb.DisplayMember = "PartId";
                    partIdcb.SelectedIndex = 0;

                    operationCb.DataSource = list;
                    operationCb.DisplayMember = "Operation";
                    operationCb.SelectedIndex = 0;
                }
                else
                {
                    List<WorkCenter> emptyList = new();

                    workOrderCb.DataSource = emptyList;
                    workOrderCb.DisplayMember = "Workcenter";
                    workOrderCb.Text = "";

                    partIdcb.DataSource = emptyList;
                    partIdcb.DisplayMember = "PartId";
                    partIdcb.Text = "";

                    operationCb.DataSource = emptyList;
                    operationCb.DisplayMember = "Operation";
                    operationCb.Text = "";
                    MessageBox.Show(@"No Job currently setup up for "+ workCenterCbText +@", please contact a Setup Tech.");
                }


            }

            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }


        }

        private static string? OnStartup_StepTwo(string workCenter)
        {
            string? workOrder = null;
            string connectionString = SqlCommands.GetConnectionString(null, null, null, null);

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new MySqlCommand("SELECT work_order FROM waitlist_current_jobs WHERE work_center = @work_center", connection);
                command.Parameters.AddWithValue("@work_center", workCenter);

                connection.Open();
                workOrder = command.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            return workOrder;
        }

        private static string? OnStartup_StepThree(string workCenter)
        {
            string? partId = null;
            string connectionString = SqlCommands.GetConnectionString(null, null, null, null);

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new MySqlCommand("SELECT part_id FROM waitlist_current_jobs WHERE work_center = @work_center", connection);
                command.Parameters.AddWithValue("@work_center", workCenter);

                connection.Open();
                partId = command.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            return partId;
        }
        
        private static string? OnStartup_StepFour(string workCenter)
        {
            string? operation = null;
            string connectionString = SqlCommands.GetConnectionString(null, null, null, null);

            try
            {
                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new MySqlCommand("SELECT operation FROM waitlist_current_jobs WHERE work_center = @work_center", connection);
                command.Parameters.AddWithValue("@work_center", workCenter);

                connection.Open();
                operation = command.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
            return operation;
        }

        public static (TimeSpan, string) GetEta(TimeSpan jobTime)
        {
            var connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null);
            const string query = $"SELECT AllocatedTime FROM waitlist_active;";

            var total = new TimeSpan();

            var timeSpans = new ObservableCollection<TimeSpan>();

            using (MySqlConnection connection = new (connectionString))
            {
                MySqlCommand command = new (query, connection);

                connection.Open();

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    timeSpans.Add(reader.GetTimeSpan(0));
                }
                connection.Close();
                command.Dispose();
            }


            foreach (TimeSpan time in timeSpans)
            {
                total += time;
            }
            
            var etaTime = total + jobTime;

            var eta = etaTime.Days > 0 ? $"{etaTime.Days:D2} Days, {etaTime.Hours:D2} Hours, {etaTime.Minutes:D2} Minutes" : $"{etaTime.Hours:D2} Hours, {etaTime.Minutes:D2} Minutes";

            return (etaTime, eta);
        }

        public static (TimeSpan, string) GetJobTime(string jobType)
        {
            try
            {
                var connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null);
                string query = $"SELECT DISTINCT DefaultTime FROM application_database WHERE request_type = '{jobType}';";

                TimeSpan returnTime = new TimeSpan(0, 0, 0, 0);
                string data = "";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(query, connection);

                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        returnTime = reader.GetTimeSpan(0);
                        data = string.Format("{0:D2} Minutes", returnTime.Minutes);
                    }
                    connection.Close();
                    command.Dispose();
                }

                return (returnTime, data);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return (new TimeSpan(), "");
            }
        }



        public class WorkCenter
        {
            public string? Workcenter { get; set; }
            public string? Workorder { get; set; }
            public string? PartId { get; set; }
            public string? Operation { get; set; }

            public override string? ToString()
            {
                try
                {
                    return Workcenter;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                    return string.Empty;
                }
            }
        }


    }
}
