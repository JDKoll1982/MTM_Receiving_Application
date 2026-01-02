using System;
using System.IO;
using System.Reflection;

namespace MTM_Receiving_Application.Helpers.Database
{
    /// <summary>
    /// Helper class to load SQL queries from embedded resource files
    /// </summary>
    public static class Helper_SqlQueryLoader
    {
        /// <summary>
        /// Loads a SQL query from an embedded resource file
        /// </summary>
        /// <param name="resourcePath">Path to the SQL file relative to Database/InforVisual (e.g., "01_GetPOWithParts.sql")</param>
        /// <returns>The SQL query text</returns>
        public static string LoadInforVisualQuery(string resourcePath)
        {
            try
            {
                // Get the executing assembly
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"MTM_Receiving_Application.Database.InforVisualScripts.Queries.{resourcePath}";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Fallback: Try to load from file system during development
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var filePath = Path.Combine(baseDir, "Database", "InforVisualScripts", "Queries", resourcePath);

                    if (File.Exists(filePath))
                    {
                        return File.ReadAllText(filePath);
                    }

                    // Try relative to solution root
                    var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName;
                    if (solutionRoot != null)
                    {
                        filePath = Path.Combine(solutionRoot, "Database", "InforVisualScripts", "Queries", resourcePath);
                        if (File.Exists(filePath))
                        {
                            return File.ReadAllText(filePath);
                        }
                    }

                    throw new FileNotFoundException($"SQL query file not found: {resourcePath}. Resource name attempted: {resourceName}");
                }

                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load SQL query from {resourcePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cleans a SQL query by removing comments and normalizing whitespace
        /// </summary>
        /// <param name="sql">Raw SQL text</param>
        /// <returns>Cleaned SQL query ready for execution</returns>
        public static string CleanQuery(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return string.Empty;
            }

            // Remove single-line comments (-- comments)
            var lines = sql.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanedLines = new System.Collections.Generic.List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip comment-only lines
                if (trimmedLine.StartsWith("--"))
                {
                    continue;
                }

                // Remove inline comments
                var commentIndex = line.IndexOf("--");
                if (commentIndex >= 0)
                {
                    var lineWithoutComment = line.Substring(0, commentIndex).TrimEnd();
                    if (!string.IsNullOrWhiteSpace(lineWithoutComment))
                    {
                        cleanedLines.Add(lineWithoutComment);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    cleanedLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, cleanedLines);
        }

        /// <summary>
        /// Extracts the actual query from a SQL file that includes DECLARE statements for testing
        /// </summary>
        /// <param name="sqlFileContent">Full SQL file content with DECLAREs</param>
        /// <returns>Just the SELECT/query portion without DECLARE statements</returns>
        public static string ExtractQueryFromFile(string sqlFileContent)
        {
            if (string.IsNullOrWhiteSpace(sqlFileContent))
            {
                return string.Empty;
            }

            // Find the start of the actual query (after DECLARE statements)
            var lines = sqlFileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var queryLines = new System.Collections.Generic.List<string>();
            bool foundQueryStart = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip comments
                if (trimmedLine.StartsWith("--"))
                {
                    continue;
                }

                // Skip DECLARE statements
                if (trimmedLine.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Look for SELECT, WITH, INSERT, UPDATE, DELETE as query start
                if (!foundQueryStart &&
                    (trimmedLine.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                     trimmedLine.StartsWith("WITH", StringComparison.OrdinalIgnoreCase) ||
                     trimmedLine.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
                     trimmedLine.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                     trimmedLine.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)))
                {
                    foundQueryStart = true;
                }

                if (foundQueryStart && !string.IsNullOrWhiteSpace(line))
                {
                    queryLines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, queryLines);
        }

        /// <summary>
        /// Loads and prepares a SQL query for execution (removes test DECLAREs and comments)
        /// </summary>
        /// <param name="resourcePath">SQL file name</param>
        /// <returns>Clean, executable SQL query</returns>
        public static string LoadAndPrepareQuery(string resourcePath)
        {
            var rawSql = LoadInforVisualQuery(resourcePath);
            var extractedQuery = ExtractQueryFromFile(rawSql);
            return extractedQuery.Trim();
        }
    }
}
