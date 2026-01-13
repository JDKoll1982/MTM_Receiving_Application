using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Helpers.Database;

/// <summary>
/// Helper class for executing stored procedures with retry logic, 
/// performance monitoring, and standardized error handling
/// </summary>
public static class Helper_Database_StoredProcedure
{
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = { 100, 200, 400 };

    /// <summary>
    /// Executes a stored procedure using MySqlParameter array (compatible with legacy/receiving DAOs)
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <param name="connectionString"></param>
    public static async Task<Model_Dao_Result> ExecuteAsync(
        string procedureName,
        MySqlParameter[] parameters,
        string connectionString)
    {
        var result = new Model_Dao_Result();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var affectedRows = await command.ExecuteNonQueryAsync();

                // Handle output parameters if any
                foreach (var param in command.Parameters)
                {
                    if (param is MySqlParameter p && (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.InputOutput))
                    {
                        // You might want to capture output values here if needed
                        // For now, we just ensure the command executes
                    }
                }

                stopwatch.Stop();
                result.Success = true;
                result.AffectedRows = affectedRows;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return Model_Dao_Result_Factory.Failure($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        return Model_Dao_Result_Factory.Failure($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
    }

    /// <summary>
    /// Validates that all required parameters are present and not null
    /// </summary>
    /// <param name="parameters"></param>
    public static bool ValidateParameters(MySqlParameter[]? parameters)
    {
        if (parameters == null)
        {
            return true;
        }

        foreach (var param in parameters)
        {
            if (param.Value == null && param.Direction == ParameterDirection.Input)
            {
                // Allow DBNull.Value but not null
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Executes a stored procedure that returns no data (INSERT, UPDATE, DELETE)
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result> ExecuteNonQueryAsync(
        string connectionString,
        string procedureName,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DB] ExecuteNonQueryAsync: {procedureName} (Attempt {attempt}/{MaxRetries})");

                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                System.Diagnostics.Debug.WriteLine($"[DB] Executing: {procedureName}");
                foreach (MySqlParameter param in command.Parameters)
                {
                    var valueDisplay = param.Value == DBNull.Value ? "NULL" : param.Value?.ToString() ?? "NULL";
                    System.Diagnostics.Debug.WriteLine($"[DB]   {param.ParameterName} = {valueDisplay} ({param.Value?.GetType().Name ?? "DBNull"})");
                }

                var affectedRows = await command.ExecuteNonQueryAsync();

                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DB] Success: {affectedRows} rows affected in {stopwatch.ElapsedMilliseconds}ms");
                result.Success = true;
                result.AffectedRows = affectedRows;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = $"Stored procedure '{procedureName}' failed: {ex.Message}";
                result.Severity = Enum_ErrorSeverity.Error;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.Exception = ex;
                return result;
            }
        }

        stopwatch.Stop();
        result.Success = false;
        result.ErrorMessage = $"Stored procedure '{procedureName}' failed after {MaxRetries} attempts";
        result.Severity = Enum_ErrorSeverity.Critical;
        result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Executes a stored procedure that returns a single record mapped to T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="mapper"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result<T>> ExecuteSingleAsync<T>(
        string connectionString,
        string procedureName,
        Func<IDataReader, T> mapper,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result<T>();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DB] ExecuteSingleAsync: {procedureName} (Attempt {attempt}/{MaxRetries})");

                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                System.Diagnostics.Debug.WriteLine($"[DB] Executing: {procedureName}");
                foreach (MySqlParameter param in command.Parameters)
                {
                    var valueDisplay = param.Value == DBNull.Value ? "NULL" : param.Value?.ToString() ?? "NULL";
                    System.Diagnostics.Debug.WriteLine($"[DB]   {param.ParameterName} = {valueDisplay} ({param.Value?.GetType().Name ?? "DBNull"})");
                }

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    result.Data = mapper(reader);
                    result.Success = true;
                    result.AffectedRows = 1;
                    System.Diagnostics.Debug.WriteLine($"[DB] Success: 1 record found in {stopwatch.ElapsedMilliseconds}ms");
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "No record found";
                    result.Severity = Enum_ErrorSeverity.Info;
                    System.Diagnostics.Debug.WriteLine($"[DB] No record found");
                }

                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DB] ERROR in ExecuteSingleAsync: {procedureName}");
                System.Diagnostics.Debug.WriteLine($"[DB] Error Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[DB] Error Message: {ex.Message}");
                if (ex is MySqlException sqlEx)
                {
                    var errorDesc = GetMySqlErrorDescription(sqlEx.Number);
                    System.Diagnostics.Debug.WriteLine($"[DB] MySQL Error {sqlEx.Number}: {errorDesc}");
                    System.Diagnostics.Debug.WriteLine($"[DB] SQL State: {sqlEx.SqlState}");
                }
                return Model_Dao_Result_Factory.Failure<T>($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        return Model_Dao_Result_Factory.Failure<T>($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
    }

    /// <summary>
    /// Executes a stored procedure that returns a list of records mapped to T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="mapper"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result<List<T>>> ExecuteListAsync<T>(
        string connectionString,
        string procedureName,
        Func<IDataReader, T> mapper,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result<List<T>>();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DB] ExecuteListAsync: {procedureName} (Attempt {attempt}/{MaxRetries})");

                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                System.Diagnostics.Debug.WriteLine($"[DB] Executing: {procedureName}");
                foreach (MySqlParameter param in command.Parameters)
                {
                    var valueDisplay = param.Value == DBNull.Value ? "NULL" : param.Value?.ToString() ?? "NULL";
                    System.Diagnostics.Debug.WriteLine($"[DB]   {param.ParameterName} = {valueDisplay} ({param.Value?.GetType().Name ?? "DBNull"})");
                }

                await using var reader = await command.ExecuteReaderAsync();
                var list = new List<T>();

                while (await reader.ReadAsync())
                {
                    list.Add(mapper(reader));
                }

                result.Data = list;
                result.Success = true;
                result.AffectedRows = list.Count;

                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DB] Success: {list.Count} records retrieved in {stopwatch.ElapsedMilliseconds}ms");
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DB] ERROR in ExecuteListAsync: {procedureName}");
                System.Diagnostics.Debug.WriteLine($"[DB] Error Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[DB] Error Message: {ex.Message}");
                if (ex is MySqlException sqlEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB] MySQL Error Code: {sqlEx.Number}");
                    System.Diagnostics.Debug.WriteLine($"[DB] SQL State: {sqlEx.SqlState}");
                }
                return Model_Dao_Result_Factory.Failure<List<T>>($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        return Model_Dao_Result_Factory.Failure<List<T>>($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
    }

    /// <summary>
    /// Executes a stored procedure that returns a DataTable (SELECT)
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result<DataTable>> ExecuteDataTableAsync(
        string connectionString,
        string procedureName,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result<DataTable>();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                await using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);

                stopwatch.Stop();
                result.Success = true;
                result.Data = dataTable;
                result.AffectedRows = dataTable.Rows.Count;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = $"Stored procedure '{procedureName}' failed: {ex.Message}";
                result.Severity = Enum_ErrorSeverity.Error;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.Exception = ex;
                return result;
            }
        }

        stopwatch.Stop();
        result.Success = false;
        result.ErrorMessage = $"Stored procedure '{procedureName}' failed after {MaxRetries} attempts";
        result.Severity = Enum_ErrorSeverity.Critical;
        result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Executes a stored procedure within an existing transaction
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result> ExecuteInTransactionAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string procedureName,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var command = new MySqlCommand(procedureName, connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddParameters(command, parameters);

            // Log the actual command being executed
            System.Diagnostics.Debug.WriteLine($"[DB] Executing SP: {procedureName}");
            foreach (MySqlParameter param in command.Parameters)
            {
                var valueDisplay = param.Value == DBNull.Value ? "NULL" : param.Value?.ToString() ?? "NULL";
                System.Diagnostics.Debug.WriteLine($"[DB]   Parameter: {param.ParameterName} = {valueDisplay} (Type: {param.MySqlDbType})");
            }

            var affectedRows = await command.ExecuteNonQueryAsync();

            stopwatch.Stop();
            result.Success = true;
            result.AffectedRows = affectedRows;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine($"[DB] Success: {affectedRows} rows affected in {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        catch (MySqlException sqlEx)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = $"Stored procedure '{procedureName}' failed: MySQL Error {sqlEx.Number}: {sqlEx.Message}";
            result.Severity = Enum_ErrorSeverity.Error;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Exception = sqlEx;

            // Detailed SQL error logging
            var errorDesc = GetMySqlErrorDescription(sqlEx.Number);
            System.Diagnostics.Debug.WriteLine($"[DB] MySQL ERROR {sqlEx.Number}: {errorDesc}");
            System.Diagnostics.Debug.WriteLine($"[DB] Error Message: {sqlEx.Message}");
            System.Diagnostics.Debug.WriteLine($"[DB] SQL State: {sqlEx.SqlState}");
            System.Diagnostics.Debug.WriteLine($"[DB] Procedure: {procedureName}");

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = $"Stored procedure '{procedureName}' failed in transaction: {ex.Message}";
            result.Severity = Enum_ErrorSeverity.Error;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Exception = ex;

            System.Diagnostics.Debug.WriteLine($"[DB] ERROR: {ex.GetType().Name}: {ex.Message}");

            return result;
        }
    }

    private static void AddParameters(MySqlCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters != null)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] Adding {parameters.Count} parameters:");
            foreach (var param in parameters)
            {
                // Automatically add 'p_' prefix if missing, as per convention
                string paramName = param.Key.StartsWith("p_") ? "@" + param.Key : "@p_" + param.Key;
                // Or just use the key if the caller is expected to provide the correct name
                // The constitution says: "Parameter names in C# match stored procedure parameters (WITHOUT p_ prefix - added automatically)"
                // So we should add p_ prefix.

                // However, existing code might be using @p_ already.
                // Let's be safe: if it starts with @, use it. If not, check if it starts with p_.

                string cleanName = param.Key.TrimStart('@');
                string finalName = cleanName.StartsWith("p_") ? "@" + cleanName : "@p_" + cleanName;

                var valueDisplay = param.Value == null ? "NULL" :
                                   param.Value == DBNull.Value ? "DBNull" :
                                   param.Value.ToString();

                System.Diagnostics.Debug.WriteLine($"[DB]   {param.Key} → {finalName} = {valueDisplay} ({param.Value?.GetType().Name ?? "null"})");

                command.Parameters.AddWithValue(finalName, param.Value ?? DBNull.Value);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[DB] No parameters provided");
        }
    }

    /// <summary>
    /// Gets a human-readable description for common MySQL error codes
    /// Reference: MySQL 5.7 Error Message Reference
    /// </summary>
    /// <param name="errorCode"></param>
    private static string GetMySqlErrorDescription(int errorCode)
    {
        return errorCode switch
        {
            // Server Error Message Reference (Chapter 2)
            1000 => "hashchk",
            1001 => "isamchk",
            1002 => "NO",
            1003 => "YES",
            1004 => "Can't create file (errno: %d - %s)",
            1005 => "Can't create table (errno: %d)",
            1006 => "Can't create database (errno: %d)",
            1007 => "Can't create database; database exists",
            1008 => "Can't drop database; database doesn't exist",
            1009 => "Error dropping database (can't delete '%s', errno: %d)",
            1010 => "Error dropping database (can't rmdir '%s', errno: %d)",
            1011 => "Error on delete of '%s' (errno: %d - %s)",
            1012 => "Can't read record in system table",
            1013 => "Can't get status of '%s' (errno: %d - %s)",
            1014 => "Can't get working directory (errno: %d - %s)",
            1015 => "Can't lock file (errno: %d - %s)",
            1016 => "Can't open file: '%s' (errno: %d - %s)",
            1017 => "Can't find file: '%s' (errno: %d - %s)",
            1018 => "Can't read dir of '%s' (errno: %d - %s)",
            1019 => "Can't change dir to '%s' (errno: %d - %s)",
            1020 => "Record has changed since last read in table '%s'",
            1021 => "Disk full (%s); waiting for someone to free some space...",
            1022 => "Can't write; duplicate key in table '%s'",
            1023 => "Error on close of '%s' (errno: %d - %s)",
            1024 => "Error reading file '%s' (errno: %d - %s)",
            1025 => "Error on rename of '%s' to '%s' (errno: %d - %s)",
            1026 => "Error writing file '%s' (errno: %d - %s)",
            1027 => "'%s' is locked against change",
            1028 => "Sort aborted",
            1029 => "View '%s' doesn't exist for '%s'",
            1030 => "Got error %d from storage engine",
            1031 => "Table storage engine for '%s' doesn't have this option",
            1032 => "Can't find record in '%s'",
            1033 => "Incorrect information in file: '%s'",
            1034 => "Incorrect key file for table '%s'; try to repair it",
            1035 => "Old key file for table '%s'; repair it!",
            1036 => "Table '%s' is read only",
            1037 => "Out of memory; restart server and try again (needed %d bytes)",
            1038 => "Out of sort memory, consider increasing server sort buffer size",
            1039 => "Unexpected EOF found when reading file '%s' (errno: %d - %s)",
            1040 => "Too many connections",
            1041 => "Out of memory; check if mysqld or some other process uses all available memory",
            1042 => "Can't get hostname for your address",
            1043 => "Bad handshake",
            1044 => "Access denied for user '%s'@'%s' to database '%s'",
            1045 => "Access denied for user '%s'@'%s' (using password: %s)",
            1046 => "No database selected",
            1047 => "Unknown command",
            1048 => "Column '%s' cannot be null",
            1049 => "Unknown database '%s'",
            1050 => "Table '%s' already exists",
            1051 => "Unknown table '%s'",
            1052 => "Column '%s' in %s is ambiguous",
            1053 => "Server shutdown in progress",
            1054 => "Unknown column '%s' in '%s'",
            1055 => "'%s' isn't in GROUP BY",
            1056 => "Can't group on '%s'",
            1057 => "Statement has sum functions and columns in same statement",
            1058 => "Column count doesn't match value count",
            1059 => "Identifier name '%s' is too long",
            1060 => "Duplicate column name '%s'",
            1061 => "Duplicate key name '%s'",
            1062 => "Duplicate entry '%s' for key %d",
            1063 => "Incorrect column specifier for column '%s'",
            1064 => "Parse error/Syntax error",
            1065 => "Query was empty",
            1066 => "Not unique table/alias: '%s'",
            1067 => "Invalid default value for '%s'",
            1068 => "Multiple primary key defined",
            1069 => "Too many keys specified; max %d keys allowed",
            1070 => "Too many key parts specified; max %d parts allowed",
            1071 => "Specified key was too long; max key length is %d bytes",
            1072 => "Key column '%s' doesn't exist in table",
            1073 => "BLOB column '%s' can't be used in key specification with the used table type",
            1074 => "Column length too big for column '%s' (max = %lu); use BLOB or TEXT instead",
            1075 => "Incorrect table definition; there can be only one auto column and it must be defined as a key",
            1076 => "Ready for connections",
            1077 => "Normal shutdown",
            1078 => "Got signal %d. Aborting!",
            1079 => "Shutdown complete",
            1080 => "Forcing close of thread %ld user: '%s'",
            1081 => "Can't create IP socket",
            1082 => "Table '%s' has no index like the one used in CREATE INDEX; recreate the table",
            1083 => "Field separator argument is not what is expected",
            1084 => "You can't use fixed rowlength with BLOBs; please use 'fields terminated by'",
            1085 => "The file '%s' must be in the database directory or be readable by all",
            1086 => "File '%s' already exists",
            1087 => "Load Info: Records: %ld Deleted: %ld Skipped: %ld Warnings: %ld",
            1088 => "Alter Info: Records: %ld Duplicates: %ld",
            1089 => "Incorrect prefix key",
            1090 => "You can't delete all columns with ALTER TABLE; use DROP TABLE instead",
            1091 => "Can't DROP '%s'; check that column/key exists",
            1092 => "Insert Info: Records: %ld Duplicates: %ld Warnings: %ld",
            1093 => "You can't specify target table '%s' for update in FROM clause",
            1094 => "Unknown thread id: %lu",
            1095 => "You are not owner of thread %lu",
            1096 => "No tables used",
            1097 => "Too many strings for column %s and SET",
            1098 => "Can't generate a unique log-filename %s.(1-999)",
            1099 => "Table '%s' was locked with a READ lock and can't be updated",
            1100 => "Table '%s' was not locked with LOCK TABLES",
            1101 => "BLOB, TEXT, GEOMETRY or JSON column '%s' can't have a default value",
            1102 => "Incorrect database name '%s'",
            1103 => "Incorrect table name '%s'",
            1104 => "The SELECT would examine more than MAX_JOIN_SIZE rows",
            1105 => "Unknown error",
            1106 => "Unknown procedure '%s'",
            1107 => "Incorrect parameter count to procedure '%s'",
            1108 => "Incorrect parameters to procedure '%s'",
            1109 => "Unknown table '%s' in %s",
            1110 => "Column '%s' specified twice",
            1111 => "Invalid use of group function",
            1112 => "Table '%s' uses an extension that doesn't exist in this MySQL version",
            1113 => "A table must have at least 1 column",
            1114 => "The table '%s' is full",
            1115 => "Unknown character set: '%s'",
            1116 => "Too many tables; MySQL can only use %d tables in a join",
            1117 => "Too many columns",
            1118 => "Row size too large",
            1119 => "Thread stack overrun",
            1120 => "Cross dependency found in OUTER JOIN; examine your ON conditions",
            1121 => "Table handler doesn't support NULL in given index",
            1122 => "Can't load function '%s'",
            1123 => "Can't initialize function '%s'; %s",
            1124 => "No paths allowed for shared library",
            1125 => "Function '%s' already exists",
            1126 => "Can't open shared library '%s' (errno: %d %s)",
            1127 => "Can't find symbol '%s' in library",
            1128 => "Function '%s' is not defined",
            1129 => "Host '%s' is blocked because of many connection errors",
            1130 => "Host '%s' is not allowed to connect to this MySQL server",
            1131 => "Anonymous users are not allowed to change passwords",
            1132 => "You must have privileges to update tables in the mysql database to change passwords",
            1133 => "Can't find any matching row in the user table",
            1134 => "Update Info: Rows matched: %ld Changed: %ld Warnings: %ld",
            1135 => "Can't create a new thread (errno %d)",
            1136 => "Column count doesn't match value count at row %ld",
            1137 => "Can't reopen table: '%s'",
            1138 => "Invalid use of NULL value",
            1139 => "Got error '%s' from regexp",
            1140 => "Mixing of GROUP columns with no GROUP columns is illegal if there is no GROUP BY clause",
            1141 => "There is no such grant defined for user '%s' on host '%s'",
            1142 => "%s command denied to user '%s'@'%s' for table '%s'",
            1143 => "%s command denied to user '%s'@'%s' for column '%s' in table '%s'",
            1144 => "Illegal GRANT/REVOKE command",
            1145 => "The host or user argument to GRANT is too long",
            1146 => "Table '%s.%s' doesn't exist",
            1147 => "There is no such grant defined for user '%s' on host '%s' on table '%s'",
            1148 => "The used command is not allowed with this MySQL version",
            1149 => "Syntax error",
            1150 => "Delayed insert thread couldn't get requested lock for table %s",
            1151 => "Too many delayed threads in use",
            1152 => "Aborted connection %ld to db: '%s' user: '%s' (%s)",
            1153 => "Got a packet bigger than 'max_allowed_packet' bytes",
            1154 => "Got a read error from the connection pipe",
            1155 => "Got an error from fcntl()",
            1156 => "Got packets out of order",
            1157 => "Couldn't uncompress communication packet",
            1158 => "Got an error reading communication packets",
            1159 => "Got timeout reading communication packets",
            1160 => "Got an error writing communication packets",
            1161 => "Got timeout writing communication packets",
            1162 => "Result string is longer than 'max_allowed_packet' bytes",
            1163 => "The used table type doesn't support BLOB/TEXT columns",
            1164 => "The used table type doesn't support AUTO_INCREMENT columns",
            1165 => "INSERT DELAYED can't be used with table '%s' because it is locked with LOCK TABLES",
            1166 => "Incorrect column name '%s'",
            1167 => "The used storage engine can't index column '%s'",
            1168 => "Unable to open underlying table",
            1169 => "Can't write, because of unique constraint, to table '%s'",
            1170 => "BLOB/TEXT column '%s' used in key specification without a key length",
            1171 => "All parts of a PRIMARY KEY must be NOT NULL",
            1172 => "Result consisted of more than one row",
            1173 => "This table type requires a primary key",
            1174 => "This version of MySQL is not compiled with RAID support",
            1175 => "You are using safe update mode and you tried to update a table without a WHERE that uses a KEY column",
            1176 => "Key '%s' doesn't exist in table '%s'",
            1177 => "Can't open table",
            1178 => "The storage engine for the table doesn't support %s",
            1179 => "You are not allowed to execute this command in a transaction",
            1180 => "Got error %d during COMMIT",
            1181 => "Got error %d during ROLLBACK",
            1182 => "Got error %d during FLUSH_LOGS",
            1183 => "Got error %d during CHECKPOINT",
            1184 => "Aborted connection %u to db: '%s' user: '%s' host: '%s' (%s)",
            1185 => "The storage engine for the table does not support binary table dump",
            1186 => "Binlog closed, cannot RESET MASTER",

            // Connection/Client specific errors often encountered
            1203 => "Max user connections exceeded",
            1205 => "Lock wait timeout exceeded",
            1213 => "Deadlock found when trying to get lock",
            1226 => "User command resources exceeded",
            1267 => "Illegal mix of collations",
            1318 => "Incorrect number of arguments for procedure",
            1366 => "Incorrect integer value",
            1452 => "Cannot add or update a child row (FK constraint fails)",
            2002 => "Can't connect to local MySQL server through socket",
            2003 => "Can't connect to MySQL server",
            2006 => "MySQL server has gone away",
            2013 => "Lost connection to MySQL server during query",
            2055 => "Lost connection to MySQL server at 'reading initial communication packet'",
            _ => "Unknown MySQL error (Check server logs)"
        };
    }

    /// <summary>
    /// Determines if a MySQL exception is transient (temporary) and should be retried
    /// Reference: MySQL 5.7 Error Message Reference
    /// </summary>
    /// <param name="ex"></param>
    private static bool IsTransientError(MySqlException ex)
    {
        // Transient error codes that can be retried:
        // 1040 = Too many connections
        // 1053 = Server shutdown in progress
        // 1080 = Forcing close of thread (server side)
        // 1152 = Aborted connection
        // 1153 = Packet bigger than max_allowed_packet (sometimes transient if config changes dynamically or network glitch)
        // 1154 = Read error from connection pipe
        // 1155 = Error from fcntl()
        // 1156 = Packets out of order
        // 1157 = Couldn't uncompress packet
        // 1158 = Error reading communication packets
        // 1159 = Timeout reading communication packets
        // 1160 = Error writing communication packets
        // 1161 = Timeout writing communication packets
        // 1184 = Aborted connection (new)
        // 1203 = User max connections exceeded
        // 1205 = Lock wait timeout exceeded
        // 1213 = Deadlock found
        // 1226 = Resource limit exceeded
        // 2002 = Can't connect to local MySQL server through socket
        // 2003 = Can't connect to MySQL server
        // 2006 = MySQL server has gone away
        // 2013 = Lost connection to MySQL server during query
        // 2055 = Lost connection to MySQL server at 'reading initial communication packet'

        return ex.Number switch
        {
            1040 or 1053 or 1080 or
            1152 or 1153 or 1154 or 1155 or 1156 or 1157 or 1158 or 1159 or 1160 or 1161 or 1184 or
            1203 or 1205 or 1213 or 1226 or
            2002 or 2003 or 2006 or 2013 or 2055 => true,
            _ => false
        };
    }
}



