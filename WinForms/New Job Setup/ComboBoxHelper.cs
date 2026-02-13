using System.Collections.ObjectModel;
using System.Data;
using MTM_Waitlist_Application_2._0.Core;
using System.Reflection;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using MySql.Data.MySqlClient;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    public static class ComboBoxHelper
    {
        public static void ResetAndFillComboBox(ComboBox comboBox, string column, string database, string table, MySqlDataAdapter dataAdapter, DataTable dataTable, string placeHolder)
        {
            comboBox.DataSource = NewJobSetup_ComboBox_Filler(column, table, dataAdapter, dataTable, placeHolder);
            comboBox.DisplayMember = column; // Set the DisplayMember to the column name
            comboBox.SelectedIndex = 0;
        }

        public static object? NewJobSetup_ComboBox_Filler(string column, string table, MySqlDataAdapter dataAdapter, DataTable dataTable, string placeHolder)
        {
            try
            {
                string connectionString = SqlCommands.GetConnectionString(null, null, null, null); // Replace with your actual connection string
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

        public static ObservableCollection<string> GetOperationsForPartId(string partId)
        {
            return new ObservableCollection<string>(
                NewJobSetup.FormsSqlCommands.ComboBoxFillerFilter("Operation", "mtm_waitlist", "waitlist_jobs", "PartID", partId)
                    .Where(item => !string.IsNullOrEmpty(item))
            );
        }

        public static ObservableCollection<string> GetFilteredDie(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "FGT", "waitlist_dies");
        }

        public static string? GetDieLocation(string? dieId)
        {
            try
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null); // Replace with your actual connection string
                string query = "SELECT Location FROM waitlist_dies WHERE FGT = @dieId LIMIT 1;";

                using MySqlConnection connection = new(connectionString);
                using MySqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@dieId", dieId);

                connection.Open();
                object? result = command.ExecuteScalar();
                connection.Close();

                return result != null ? result.ToString() : "Location not found";
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return "Error fetching location";
            }
        }


        public static ObservableCollection<string> GetFilteredContainer(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Dunnage", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredCoils(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Component", "waitlist_components", 1);
        }

        public static ObservableCollection<string> GetFilteredSkid(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Skid", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredCardboard(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Cardboard", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredBox(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Boxes", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredOther1(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Other1", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredOther2(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Other2", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredOther3(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Other3", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredOther4(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Other4", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetFilteredOther5(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "Other5", "waitlist_jobdunnage");
        }

        public static ObservableCollection<string> GetPartType(string partId, string operation)
        {
            return GetFilteredItems(partId, operation, "PartType", "waitlist_jobdunnage");
        }

        private static ObservableCollection<string> GetFilteredItems(string partId, string operation, string column, string table, int? additionalParam = null)
        {
            return new ObservableCollection<string>(
                NewJobSetup.FormsSqlCommands.ComboBoxFillerFilterTwo(
                    additionalParam,
                    column,
                    "mtm_waitlist",
                    table,
                    "PartID",
                    partId,
                    "Operation",
                    operation
                ).Where(item => !string.IsNullOrEmpty(item))
            );
        }
    }
}