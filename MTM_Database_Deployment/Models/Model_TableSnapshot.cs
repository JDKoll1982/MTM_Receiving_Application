namespace MTM_Database_Deployment.Models;

/// <summary>
/// One column as reported by information_schema, used for expected/live comparison.
/// </summary>
public sealed class Model_ColumnDefinition
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public required bool IsNullable { get; init; }
    public string? DefaultValue { get; init; }
    public string Extra { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
    public string? CharacterSet { get; init; }
    public string? Collation { get; init; }
    public int OrdinalPosition { get; init; }

    /// <summary>
    /// Full SQL fragment for a column definition (without the leading name).
    /// </summary>
    public string ToColumnDefinitionSql()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append(DataType);
        if (!string.IsNullOrWhiteSpace(CharacterSet) && !string.IsNullOrWhiteSpace(Collation))
        {
            builder.Append(" CHARACTER SET ").Append(CharacterSet).Append(" COLLATE ").Append(Collation);
        }

        builder.Append(IsNullable ? " NULL" : " NOT NULL");

        if (!string.IsNullOrWhiteSpace(DefaultValue))
        {
            builder.Append(" DEFAULT ").Append(DefaultValue);
        }

        if (Extra.Contains("auto_increment", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append(" AUTO_INCREMENT");
        }

        if (Extra.Contains("DEFAULT_GENERATED", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(DefaultValue))
        {
            // MySQL stores CURRENT_TIMESTAMP defaults as DEFAULT_GENERATED; the value already carries the expression.
            builder.Append(string.Empty);
        }

        if (Extra.Contains("on update", StringComparison.OrdinalIgnoreCase))
        {
            builder.Append(" ON UPDATE CURRENT_TIMESTAMP");
        }

        if (!string.IsNullOrWhiteSpace(Comment))
        {
            builder.Append(" COMMENT '").Append(Comment.Replace("'", "''")).Append('\'');
        }

        return builder.ToString();
    }

    public string Signature => $"{DataType}|{IsNullable}|{DefaultValue ?? "<null>"}|{Extra}|{Comment}|{Collation ?? string.Empty}";
}

/// <summary>
/// One index as reported by information_schema.
/// </summary>
public sealed class Model_IndexDefinition
{
    public required string Name { get; init; }
    public required bool IsUnique { get; init; }
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();

    public string Signature => $"{(IsUnique ? "UNIQUE" : "INDEX")}|{string.Join(",", Columns)}";

    public string ToCreateSql(string tableName, string schemaName)
    {
        var columnList = string.Join(", ", Columns.Select(column => "`" + column + "`"));
        var uniqueKeyword = IsUnique ? "UNIQUE " : string.Empty;
        return $"ALTER TABLE `{schemaName}`.`{tableName}` ADD {uniqueKeyword}INDEX `{Name}` ({columnList});";
    }
}

/// <summary>
/// One foreign key as reported by information_schema.
/// </summary>
public sealed class Model_ForeignKeyDefinition
{
    public required string Name { get; init; }
    public IReadOnlyList<string> Columns { get; init; } = Array.Empty<string>();
    public string ReferencedTable { get; init; } = string.Empty;
    public IReadOnlyList<string> ReferencedColumns { get; init; } = Array.Empty<string>();
    public string UpdateRule { get; init; } = string.Empty;
    public string DeleteRule { get; init; } = string.Empty;

    public string Signature =>
        $"{string.Join(",", Columns)}->{ReferencedTable}({string.Join(",", ReferencedColumns)})|{UpdateRule}|{DeleteRule}";

    public string ToCreateSql(string tableName, string schemaName)
    {
        var columnList = string.Join(", ", Columns.Select(column => "`" + column + "`"));
        var referencedList = string.Join(", ", ReferencedColumns.Select(column => "`" + column + "`"));
        return $"ALTER TABLE `{schemaName}`.`{tableName}` ADD CONSTRAINT `{Name}` FOREIGN KEY ({columnList}) " +
               $"REFERENCES `{ReferencedTable}` ({referencedList}) ON UPDATE {UpdateRule} ON DELETE {DeleteRule};";
    }
}

/// <summary>
/// All metadata captured for one table.
/// </summary>
public sealed class Model_TableSnapshot
{
    public required string Name { get; init; }
    public IReadOnlyList<Model_ColumnDefinition> Columns { get; init; } = Array.Empty<Model_ColumnDefinition>();
    public IReadOnlyList<Model_IndexDefinition> Indexes { get; init; } = Array.Empty<Model_IndexDefinition>();
    public IReadOnlyList<Model_ForeignKeyDefinition> ForeignKeys { get; init; } = Array.Empty<Model_ForeignKeyDefinition>();

    public Model_ColumnDefinition? FindColumn(string name) =>
        Columns.FirstOrDefault(column => string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase));
}
