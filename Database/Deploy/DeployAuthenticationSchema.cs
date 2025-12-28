using System;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Database.Deploy
{
    /// <summary>
    /// Deploys database schema and stored procedures for authentication feature
    /// Run this once to initialize the authentication tables
    /// </summary>
    public static class DeployAuthenticationSchema
    {
        private const string ConnectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";

        public static async Task DeployAsync()
        {
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine("Authentication Schema Deployment");
            Console.WriteLine("=".PadRight(80, '='));
            Console.WriteLine();

            try
            {
                // Deploy schema
                Console.WriteLine("1. Deploying database schema...");
                await DeploySchemaAsync();
                Console.WriteLine("   ✓ Schema deployed successfully");
                Console.WriteLine();

                // Deploy stored procedures
                Console.WriteLine("2. Deploying stored procedures...");
                await DeployStoredProcedureAsync("sp_GetUserByWindowsUsername.sql");
                await DeployStoredProcedureAsync("sp_ValidateUserPin.sql");
                await DeployStoredProcedureAsync("sp_CreateNewUser.sql");
                await DeployStoredProcedureAsync("sp_LogUserActivity.sql");
                await DeployStoredProcedureAsync("sp_GetSharedTerminalNames.sql");
                await DeployStoredProcedureAsync("sp_GetDepartments.sql");
                Console.WriteLine("   ✓ All stored procedures deployed successfully");
                Console.WriteLine();

                // Verify deployment
                Console.WriteLine("3. Verifying deployment...");
                await VerifyDeploymentAsync();
                Console.WriteLine();

                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine("✓ Deployment completed successfully!");
                Console.WriteLine("=".PadRight(80, '='));
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("✗ Deployment failed!");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Details: {ex}");
                Environment.Exit(1);
            }
        }

        private static async Task DeploySchemaAsync()
        {
            string schemaPath = Path.Combine(GetProjectRoot(), "Database", "Schemas", "02_create_authentication_tables.sql");
            string sql = await File.ReadAllTextAsync(schemaPath);

            await ExecuteSqlAsync(sql);
        }

        private static async Task DeployStoredProcedureAsync(string fileName)
        {
            Console.WriteLine($"   - Deploying {fileName}...");
            string procPath = Path.Combine(GetProjectRoot(), "Database", "StoredProcedures", "Authentication", fileName);
            string sql = await File.ReadAllTextAsync(procPath);

            await ExecuteSqlAsync(sql);
        }

        private static async Task ExecuteSqlAsync(string sql)
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Split by delimiter changes and execute each batch
            var batches = sql.Split(new[] { "DELIMITER $$", "DELIMITER ;" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var batch in batches)
            {
                var cleanBatch = batch.Trim();
                if (string.IsNullOrWhiteSpace(cleanBatch) || cleanBatch.StartsWith("--"))
                {
                    continue;
                }

                await using var command = new MySqlCommand(cleanBatch, connection);
                command.CommandTimeout = 60;
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task VerifyDeploymentAsync()
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            // Check tables
            var tablesSql = @"
                SELECT COUNT(*) FROM information_schema.tables 
                WHERE table_schema = 'mtm_receiving_application' 
                AND table_name IN ('users', 'workstation_config', 'departments', 'user_activity_log')";

            await using var cmd1 = new MySqlCommand(tablesSql, connection);
            var tableCount = Convert.ToInt32(await cmd1.ExecuteScalarAsync());
            Console.WriteLine($"   - Tables created: {tableCount}/4");

            // Check stored procedures
            var procsSql = @"
                SELECT COUNT(*) FROM information_schema.routines 
                WHERE routine_schema = 'mtm_receiving_application' 
                AND routine_name IN ('sp_GetUserByWindowsUsername', 'sp_ValidateUserPin', 
                                     'sp_CreateNewUser', 'sp_LogUserActivity',
                                     'sp_GetSharedTerminalNames', 'sp_GetDepartments')";

            await using var cmd2 = new MySqlCommand(procsSql, connection);
            var procCount = Convert.ToInt32(await cmd2.ExecuteScalarAsync());
            Console.WriteLine($"   - Stored procedures created: {procCount}/6");

            // Check sample data
            await using var cmd3 = new MySqlCommand("SELECT COUNT(*) FROM departments", connection);
            var deptCount = Convert.ToInt32(await cmd3.ExecuteScalarAsync());
            Console.WriteLine($"   - Departments: {deptCount}");

            await using var cmd4 = new MySqlCommand("SELECT COUNT(*) FROM workstation_config", connection);
            var wsCount = Convert.ToInt32(await cmd4.ExecuteScalarAsync());
            Console.WriteLine($"   - Workstations configured: {wsCount}");

            await using var cmd5 = new MySqlCommand("SELECT COUNT(*) FROM users", connection);
            var userCount = Convert.ToInt32(await cmd5.ExecuteScalarAsync());
            Console.WriteLine($"   - Test users: {userCount}");

            if (tableCount != 4 || procCount != 6)
            {
                throw new Exception("Deployment verification failed - not all objects created");
            }
        }

        private static string GetProjectRoot()
        {
            string? currentDir = Directory.GetCurrentDirectory();
            while (currentDir != null && !File.Exists(Path.Combine(currentDir, "MTM_Receiving_Application.csproj")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            return currentDir ?? throw new Exception("Could not find project root");
        }
    }
}
