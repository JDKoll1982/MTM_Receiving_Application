using MTM_Database_Deployment.Models;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Compares the SQL object tree against a live database.
///
/// The expected schema is materialised in a temporary scratch schema built from the same
/// table scripts that are deployed, so the comparison always uses real MySQL metadata
/// instead of a hand written DDL parser.
/// </summary>
public sealed class Service_SchemaComparer
{
    private readonly Service_MySqlGateway _gateway;
    private readonly Service_ObjectCatalog _catalog;

    public Service_SchemaComparer(Service_MySqlGateway gateway, Service_ObjectCatalog catalog)
    {
        _gateway = gateway;
        _catalog = catalog;
    }

    public async Task<Model_DeploymentPlan> BuildPlanAsync(
        string database,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var resolution = await _gateway.ResolveServerAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(resolution);

        var objects = _catalog.LoadObjects();
        var schemaObjects = objects.Where(item => item.Kind == Enum_SqlObjectKind.Schema).ToList();
        if (schemaObjects.Count == 0)
        {
            throw new InvalidOperationException($"No table scripts were found under {_catalog.SqlRoot}.");
        }

        var scratchDatabase = "mtm_expected_" + Guid.NewGuid().ToString("N")[..8];
        var changes = new List<Model_SchemaChange>();

        try
        {
            progress?.Report("Building expected schema from table scripts...");
            await _gateway.ExecuteWithoutDatabaseAsync(
                $"DROP DATABASE IF EXISTS `{scratchDatabase}`; CREATE DATABASE `{scratchDatabase}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;",
                cancellationToken).ConfigureAwait(false);

            foreach (var schemaObject in schemaObjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var script = await File.ReadAllTextAsync(schemaObject.FullPath, cancellationToken).ConfigureAwait(false);
                try
                {
                    await _gateway.ExecuteStatementsAsync(
                        scratchDatabase,
                        Service_SqlScriptParser.SplitStatements(script),
                        continueOnError: false,
                        onError: null,
                        disableForeignKeyChecks: true,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Table script {schemaObject.RelativePath} could not be evaluated: {exception.Message}", exception);
                }
            }

            progress?.Report("Reading expected schema metadata...");
            var expectedTables = await _gateway.ReadTableSnapshotsAsync(scratchDatabase, cancellationToken).ConfigureAwait(false);

            progress?.Report($"Reading {database} schema metadata...");
            var liveTables = await _gateway.ReadTableSnapshotsAsync(database, cancellationToken).ConfigureAwait(false);

            BuildTableChanges(expectedTables, liveTables, database, changes);
            BuildObjectChanges(objects, database, changes);

            return new Model_DeploymentPlan
            {
                Server = _gateway.ActiveServer,
                Port = _gateway.Port,
                Database = database,
                GeneratedAt = DateTimeOffset.Now,
                Changes = changes
            };
        }
        finally
        {
            try
            {
                await _gateway.ExecuteWithoutDatabaseAsync($"DROP DATABASE IF EXISTS `{scratchDatabase}`;", CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Scratch cleanup is best effort.
            }
        }
    }

    private static void BuildTableChanges(
        Dictionary<string, Model_TableSnapshot> expected,
        Dictionary<string, Model_TableSnapshot> live,
        string database,
        List<Model_SchemaChange> changes)
    {
        // MySQL refuses to drop an index a foreign key relies on, so a rebuilt index needs its
        // dependent keys dropped first. Both directions are computed up front, because an inbound
        // key lives in another table that may be walked before or after this one.
        var dependentForeignKeys = CollectIndexDependentForeignKeys(live);
        var rebuiltIndexes = FindRebuiltIndexes(expected, live);
        var droppedForeignKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in rebuiltIndexes)
        {
            if (dependentForeignKeys.TryGetValue(key, out var dependents))
            {
                foreach (var dependent in dependents)
                {
                    droppedForeignKeys.Add(ForeignKeyKey(dependent.Table, dependent.ForeignKey.Name));
                }
            }
        }

        foreach (var (tableName, expectedTable) in expected.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (!live.TryGetValue(tableName, out var liveTable))
            {
                changes.Add(new Model_SchemaChange
                {
                    Kind = Enum_ChangeKind.CreateTable,
                    Target = tableName,
                    Detail = "table does not exist yet"
                });
                continue;
            }

            foreach (var column in expectedTable.Columns)
            {
                var liveColumn = liveTable.FindColumn(column.Name);
                var definition = column.ToColumnDefinitionSql();

                if (liveColumn is null)
                {
                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.AddColumn,
                        Target = $"{tableName}.{column.Name}",
                        Detail = column.DataType,
                        Sql = $"ALTER TABLE `{database}`.`{tableName}` ADD COLUMN `{column.Name}` {definition};"
                    });
                    continue;
                }

                if (!string.Equals(liveColumn.Signature, column.Signature, StringComparison.Ordinal))
                {
                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.AlterColumn,
                        Target = $"{tableName}.{column.Name}",
                        Detail = $"live: {liveColumn.DataType} -> expected: {column.DataType}",
                        Sql = $"ALTER TABLE `{database}`.`{tableName}` MODIFY COLUMN `{column.Name}` {definition};"
                    });
                }
            }

            foreach (var index in expectedTable.Indexes)
            {
                var liveIndex = liveTable.Indexes.FirstOrDefault(item => string.Equals(item.Name, index.Name, StringComparison.OrdinalIgnoreCase));
                if (liveIndex is null)
                {
                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.AddIndex,
                        Target = $"{tableName}.{index.Name}",
                        Detail = (index.IsUnique ? "unique " : string.Empty) + string.Join(", ", index.Columns),
                        Sql = index.ToCreateSql(tableName, database)
                    });
                }
                else if (!string.Equals(liveIndex.Signature, index.Signature, StringComparison.Ordinal))
                {
                    var key = IndexKey(tableName, index.Name);
                    var dependents = dependentForeignKeys.TryGetValue(key, out var found)
                        ? found
                        : new List<DependentForeignKey>();

                    // A key this table owns is released in the same statement as the index swap, so the
                    // index can never vanish while the key still points at it. A key owned by another
                    // table cannot join that statement - ALTER TABLE touches one table - so it is
                    // dropped before and re-added after.
                    var local = dependents
                        .Where(dependent => string.Equals(dependent.Table, tableName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    var remote = dependents.Except(local).ToList();

                    var sql = new System.Text.StringBuilder();

                    foreach (var dependent in remote)
                    {
                        sql.AppendLine($"ALTER TABLE `{database}`.`{dependent.Table}` DROP FOREIGN KEY `{dependent.ForeignKey.Name}`;");
                    }

                    // MySQL checks every ADD clause before any DROP clause, so a key cannot be dropped
                    // and re-added under the same name in one statement (1826). Releasing it and
                    // swapping the index together is what keeps the swap safe.
                    sql.AppendLine($"ALTER TABLE `{database}`.`{tableName}`");
                    sql.Append("  ").AppendLine(
                        string.Join(
                            "," + Environment.NewLine + "  ",
                            local.Select(dependent => $"DROP FOREIGN KEY `{dependent.ForeignKey.Name}`")
                                .Append($"DROP INDEX `{index.Name}`")
                                .Append(index.ToAddClause())));
                    sql.AppendLine(";");

                    foreach (var dependent in dependents)
                    {
                        // A key whose own definition also changed is re-created by the foreign key
                        // change below, with the new columns, so re-adding it here would collide.
                        if (!IsForeignKeyRebuilt(expected, live, dependent))
                        {
                            sql.AppendLine(dependent.ForeignKey.ToCreateSql(dependent.Table, database));
                        }
                    }

                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.AddIndex,
                        Target = $"{tableName}.{index.Name}",
                        Detail = dependents.Count == 0
                            ? "definition differs"
                            : "definition differs; dependent foreign key(s) dropped and re-added",
                        Sql = sql.ToString()
                    });
                }
            }

            foreach (var foreignKey in expectedTable.ForeignKeys)
            {
                var liveForeignKey = liveTable.ForeignKeys.FirstOrDefault(item => string.Equals(item.Name, foreignKey.Name, StringComparison.OrdinalIgnoreCase));
                if (liveForeignKey is null)
                {
                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.AddForeignKey,
                        Target = $"{tableName}.{foreignKey.Name}",
                        Detail = $"references {foreignKey.ReferencedTable}",
                        Sql = foreignKey.ToCreateSql(tableName, database)
                    });
                }
                else if (!string.Equals(liveForeignKey.Signature, foreignKey.Signature, StringComparison.Ordinal))
                {
                    // The key may already be gone because an index change dropped it to rebuild itself.
                    var dropPrefix = droppedForeignKeys.Contains(ForeignKeyKey(tableName, foreignKey.Name))
                        ? string.Empty
                        : $"ALTER TABLE `{database}`.`{tableName}` DROP FOREIGN KEY `{foreignKey.Name}`;{Environment.NewLine}";

                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.AddForeignKey,
                        Target = $"{tableName}.{foreignKey.Name}",
                        Detail = "definition differs",
                        Sql = dropPrefix + foreignKey.ToCreateSql(tableName, database)
                    });
                }
            }

            foreach (var liveColumn in liveTable.Columns)
            {
                if (expectedTable.FindColumn(liveColumn.Name) is null)
                {
                    var blocksInserts = !liveColumn.IsNullable && string.IsNullOrWhiteSpace(liveColumn.DefaultValue);
                    changes.Add(new Model_SchemaChange
                    {
                        Kind = Enum_ChangeKind.ExtraColumn,
                        Target = $"{tableName}.{liveColumn.Name}",
                        Detail = blocksInserts
                            ? $"{liveColumn.DataType} exists in the database only; no default, so inserts that omit it fail"
                            : $"{liveColumn.DataType} exists in the database only",
                        Sql = $"ALTER TABLE `{database}`.`{tableName}` DROP COLUMN `{liveColumn.Name}`;",
                        IsBlocking = blocksInserts
                    });
                }
            }
        }

        foreach (var tableName in live.Keys.Where(name => !expected.ContainsKey(name)).OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            changes.Add(new Model_SchemaChange
            {
                Kind = Enum_ChangeKind.ExtraTable,
                Target = tableName,
                Detail = "table exists in the database only",
                Sql = $"DROP TABLE `{database}`.`{tableName}`;"
            });
        }
    }

    /// <summary>
    /// A foreign key MySQL will not release its index for. <c>Table</c> is the table that owns the
    /// key, which is the table being altered - not always the table whose index is being rebuilt.
    /// </summary>
    private sealed record DependentForeignKey(string Table, Model_ForeignKeyDefinition ForeignKey);

    private static string IndexKey(string table, string index) => $"{table}|{index}";

    private static string ForeignKeyKey(string table, string foreignKey) => $"{table}|{foreignKey}";

    /// <summary>
    /// Every index whose definition differs between the SQL tree and the live database.
    /// </summary>
    private static HashSet<string> FindRebuiltIndexes(
        Dictionary<string, Model_TableSnapshot> expected,
        Dictionary<string, Model_TableSnapshot> live)
    {
        var rebuilt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (tableName, expectedTable) in expected)
        {
            if (!live.TryGetValue(tableName, out var liveTable))
            {
                continue;
            }

            foreach (var index in expectedTable.Indexes)
            {
                var liveIndex = liveTable.Indexes.FirstOrDefault(
                    item => string.Equals(item.Name, index.Name, StringComparison.OrdinalIgnoreCase));
                if (liveIndex is not null
                    && !string.Equals(liveIndex.Signature, index.Signature, StringComparison.Ordinal))
                {
                    rebuilt.Add(IndexKey(tableName, index.Name));
                }
            }
        }

        return rebuilt;
    }

    /// <summary>
    /// Maps <c>table|index</c> to the foreign keys that use that index, in both directions:
    /// a key on the same table whose columns are the index prefix, and a key on another table that
    /// references this one through those columns. Both make <c>DROP INDEX</c> fail with
    /// "Cannot drop index ...: needed in a foreign key constraint".
    /// </summary>
    private static Dictionary<string, List<DependentForeignKey>> CollectIndexDependentForeignKeys(
        Dictionary<string, Model_TableSnapshot> live)
    {
        var dependents = new Dictionary<string, List<DependentForeignKey>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (indexTable, indexTableSnapshot) in live)
        {
            foreach (var index in indexTableSnapshot.Indexes)
            {
                var found = new List<DependentForeignKey>();

                foreach (var foreignKey in indexTableSnapshot.ForeignKeys)
                {
                    if (IsLeftmostPrefix(foreignKey.Columns, index.Columns))
                    {
                        found.Add(new DependentForeignKey(indexTable, foreignKey));
                    }
                }

                foreach (var (childTable, childSnapshot) in live)
                {
                    if (string.Equals(childTable, indexTable, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var foreignKey in childSnapshot.ForeignKeys)
                    {
                        if (string.Equals(foreignKey.ReferencedTable, indexTable, StringComparison.OrdinalIgnoreCase)
                            && IsLeftmostPrefix(foreignKey.ReferencedColumns, index.Columns))
                        {
                            found.Add(new DependentForeignKey(childTable, foreignKey));
                        }
                    }
                }

                if (found.Count > 0)
                {
                    dependents[IndexKey(indexTable, index.Name)] = found;
                }
            }
        }

        return dependents;
    }

    /// <summary>
    /// True when the key's own definition also changed, so the foreign key change re-creates it and
    /// this one must not add it a second time.
    /// </summary>
    private static bool IsForeignKeyRebuilt(
        Dictionary<string, Model_TableSnapshot> expected,
        Dictionary<string, Model_TableSnapshot> live,
        DependentForeignKey dependent)
    {
        if (!expected.TryGetValue(dependent.Table, out var expectedTable)
            || !live.TryGetValue(dependent.Table, out var liveTable))
        {
            return false;
        }

        var expectedKey = expectedTable.ForeignKeys.FirstOrDefault(
            item => string.Equals(item.Name, dependent.ForeignKey.Name, StringComparison.OrdinalIgnoreCase));
        if (expectedKey is null)
        {
            return false;
        }

        var liveKey = liveTable.ForeignKeys.FirstOrDefault(
            item => string.Equals(item.Name, dependent.ForeignKey.Name, StringComparison.OrdinalIgnoreCase));

        return liveKey is not null
            && !string.Equals(liveKey.Signature, expectedKey.Signature, StringComparison.Ordinal);
    }

    private static bool IsLeftmostPrefix(IReadOnlyList<string> candidate, IReadOnlyList<string> indexColumns)
    {
        if (candidate.Count == 0 || candidate.Count > indexColumns.Count)
        {
            return false;
        }

        for (var position = 0; position < candidate.Count; position++)
        {
            if (!string.Equals(candidate[position], indexColumns[position], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static void BuildObjectChanges(
        IReadOnlyList<Model_SqlObject> objects,
        string database,
        List<Model_SchemaChange> changes)
    {
        foreach (var view in objects.Where(item => item.Kind == Enum_SqlObjectKind.View))
        {
            changes.Add(new Model_SchemaChange
            {
                Kind = Enum_ChangeKind.UpdateView,
                Target = view.RelativePath,
                Detail = "CREATE OR REPLACE VIEW",
                SourcePath = view.FullPath
            });
        }

        foreach (var procedure in objects.Where(item => item.Kind == Enum_SqlObjectKind.Procedure))
        {
            changes.Add(new Model_SchemaChange
            {
                Kind = Enum_ChangeKind.UpdateProcedure,
                Target = procedure.RelativePath,
                Detail = "DROP + CREATE PROCEDURE",
                SourcePath = procedure.FullPath
            });
        }

        foreach (var trigger in objects.Where(item => item.Kind == Enum_SqlObjectKind.Trigger))
        {
            changes.Add(new Model_SchemaChange
            {
                Kind = Enum_ChangeKind.UpdateTrigger,
                Target = trigger.RelativePath,
                Detail = "DROP + CREATE TRIGGER",
                SourcePath = trigger.FullPath
            });
        }

        foreach (var seed in objects.Where(item => item.Kind == Enum_SqlObjectKind.Seed))
        {
            changes.Add(new Model_SchemaChange
            {
                Kind = Enum_ChangeKind.RunSeed,
                Target = seed.RelativePath,
                Detail = "idempotent seed data",
                SourcePath = seed.FullPath
            });
        }
    }
}
