using System.Text;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Splits a MySQL script file into individual statements, honouring the client-side
/// DELIMITER directive so procedure/trigger bodies survive intact.
/// </summary>
public static class Service_SqlScriptParser
{
    public static IReadOnlyList<string> SplitStatements(string script)
    {
        var statements = new List<string>();
        if (string.IsNullOrWhiteSpace(script))
        {
            return statements;
        }

        var current = new StringBuilder();
        var delimiter = ";";
        var index = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;
        var inBacktick = false;
        var inLineComment = false;
        var inBlockComment = false;

        void Flush()
        {
            var statement = current.ToString().Trim();
            current.Clear();
            if (string.IsNullOrWhiteSpace(statement))
            {
                return;
            }

            if (statement.All(character => char.IsWhiteSpace(character) || character == ';'))
            {
                return;
            }

            statements.Add(statement);
        }

        while (index < script.Length)
        {
            var character = script[index];

            if (inLineComment)
            {
                current.Append(character);
                if (character == '\n')
                {
                    inLineComment = false;
                }

                index++;
                continue;
            }

            if (inBlockComment)
            {
                current.Append(character);
                if (character == '*' && index + 1 < script.Length && script[index + 1] == '/')
                {
                    current.Append('/');
                    index += 2;
                    inBlockComment = false;
                    continue;
                }

                index++;
                continue;
            }

            if (!inSingleQuote && !inDoubleQuote && !inBacktick)
            {
                if (character == '-' && index + 1 < script.Length && script[index + 1] == '-'
                    && (index + 2 >= script.Length || char.IsWhiteSpace(script[index + 2])))
                {
                    inLineComment = true;
                    current.Append("--");
                    index += 2;
                    continue;
                }

                if (character == '#')
                {
                    inLineComment = true;
                    current.Append(character);
                    index++;
                    continue;
                }

                if (character == '/' && index + 1 < script.Length && script[index + 1] == '*')
                {
                    inBlockComment = true;
                    current.Append("/*");
                    index += 2;
                    continue;
                }

                if (IsLineStart(script, index))
                {
                    var lineEnd = script.IndexOf('\n', index);
                    var line = lineEnd < 0 ? script[index..] : script[index..lineEnd];
                    var trimmedLine = line.Trim();

                    if (trimmedLine.StartsWith("DELIMITER", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = trimmedLine["DELIMITER".Length..].Trim();
                        Flush();
                        if (token.Length > 0)
                        {
                            delimiter = token;
                        }

                        index = lineEnd < 0 ? script.Length : lineEnd + 1;
                        continue;
                    }
                }

                if (string.CompareOrdinal(script, index, delimiter, 0, delimiter.Length) == 0)
                {
                    index += delimiter.Length;
                    Flush();
                    continue;
                }
            }

            if (character == '\'' && !inDoubleQuote && !inBacktick)
            {
                inSingleQuote = !inSingleQuote;
                current.Append(character);
                index++;
                continue;
            }

            if (character == '"' && !inSingleQuote && !inBacktick)
            {
                inDoubleQuote = !inDoubleQuote;
                current.Append(character);
                index++;
                continue;
            }

            if (character == '`' && !inSingleQuote && !inDoubleQuote)
            {
                inBacktick = !inBacktick;
                current.Append(character);
                index++;
                continue;
            }

            if (character == '\\' && (inSingleQuote || inDoubleQuote))
            {
                current.Append(character);
                index++;
                if (index < script.Length)
                {
                    current.Append(script[index]);
                    index++;
                }

                continue;
            }

            current.Append(character);
            index++;
        }

        Flush();
        return statements;
    }

    private static bool IsLineStart(string script, int index)
    {
        for (var position = index - 1; position >= 0; position--)
        {
            var character = script[position];
            if (character == '\n')
            {
                return true;
            }

            if (!char.IsWhiteSpace(character))
            {
                return false;
            }
        }

        return true;
    }
}
