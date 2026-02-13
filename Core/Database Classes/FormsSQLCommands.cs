using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;
using System.Security.Principal;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Core.Database_Classes
{
    public class FormsSqlCommands
    {

        public ObservableCollection<string> ComboBoxFillerFilter(string column, string database, string table, string filterA, string filterB) // Method to fill the combo-boxes
        {
            try
            {
                var connectionString = SqlCommands.GetConnectionString(null, database, null, null);
                filterB = "'" + filterB + "'";
                string query = "SELECT DISTINCT " + column + " FROM " + table + " WHERE " + filterA + " = " + filterB + ";";

                ObservableCollection<string> data = new ObservableCollection<string>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(query, connection);
                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Add(reader.GetString(0));
                    }
                    connection.Close();
                    command.Dispose();
                }

                return data;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return new ObservableCollection<string>();
            }
        }

        public ObservableCollection<string> ComboBoxFillerFilterTwo(int? type, string column, string database, string table, string filterA, string filterB, string filterC, string filterD) // Method to fill the combo-boxes
        {
            try
            {
                var connectionString = SqlCommands.GetConnectionString(null, database, null, null);
                string query = $"SELECT DISTINCT {column} FROM {table} WHERE {filterA} = '{filterB}' AND {filterC} = '{filterD}';";

                if (type == 1)
                {
                    query = $"SELECT DISTINCT {column} FROM {table} WHERE {filterA} = '{filterB}' AND {filterC} = '{filterD}' AND " +
                            "(Component LIKE @mm);";
                }

                ObservableCollection<string> data = new ObservableCollection<string>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@mm", "%MM%");

                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Add(reader.GetString(0));
                    }
                    connection.Close();
                    command.Dispose();
                }

                return data;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return new ObservableCollection<string>();
            }
        }

        public static void PurgeDies()
        {
            try
            {
                var connectionString = SqlCommands.GetConnectionString(null, null, null, null);
                string query = "DELETE FROM `waitlist_dies` WHERE `FGT` = \"FGT0001-01\";";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    MySqlCommand command = new MySqlCommand(query, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    command.Dispose();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        } // Removes dies form die table with FGT0001-01 as the FGT
    }
}

