using System;
using System.IO;
using MySql.Data.MySqlClient;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;
using System.Reflection;

namespace MTM_Waitlist_Application_2._0.Core
{
    public class ResetIDsOnStartup
    {
        public void ResetTableId(string tableName)
        {
            try
            {
                string connectionString = SqlCommands.GetConnectionString(null, "mtm_waitlist", null, null) + "; Allow User Variables=True";
                using MySqlConnection connection = new(connectionString);
                connection.Open();
                MySqlCommand commandReset = new(
                    $"SET @count = 0; UPDATE `mtm_waitlist`.`{tableName}` SET `mtm_waitlist`.`{tableName}`.`ID` = @count:= @count + 1; ALTER TABLE `mtm_waitlist`.`{tableName}` AUTO_INCREMENT = 1;",
                    connection
                );
                commandReset.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

    }
}