using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Services;

/// <summary>
/// The catalog decides what a deploy is allowed to touch. The archived migration folder must stay
/// invisible, and every shipped kind must be discovered in its deployment order.
/// </summary>
public sealed class Service_ObjectCatalogTests : IDisposable
{
    private readonly string _root;

    public Service_ObjectCatalogTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "mtm-catalog-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void LoadObjects_ShouldSkipArchivedScripts_WhenTheFileSitsUnderAnArchiveFolder()
    {
        WriteFile("Schemas/04_Table_dunnage_types.sql");
        WriteFile("Schemas/_Archive/99_Table_retired.sql");
        WriteFile("StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");
        WriteFile("StoredProcedures/_Archive/sp_Retired.sql");
        WriteFile("_Archive/Migrations/01_Migration_old.sql");

        var objects = new Service_ObjectCatalog(_root).LoadObjects();

        objects.Select(item => item.RelativePath).Should().BeEquivalentTo(
            "Schemas/04_Table_dunnage_types.sql",
            "StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");
    }

    [Fact]
    public void LoadObjects_ShouldClassifyByFolder_WhenTheTreeHasEveryKind()
    {
        WriteFile("Schemas/04_Table_dunnage_types.sql");
        WriteFile("Views/05_View_dunnage_label_data.sql");
        WriteFile("StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");
        WriteFile("Triggers/01_Trigger_demo.sql");
        WriteFile("SeedData/03_seed_dunnage_data.sql");

        var objects = new Service_ObjectCatalog(_root).LoadObjects();

        objects.Should().HaveCount(5);
        objects.Single(item => item.RelativePath.StartsWith("Schemas/", StringComparison.Ordinal)).Kind
            .Should().Be(Enum_SqlObjectKind.Schema);
        objects.Single(item => item.RelativePath.StartsWith("Views/", StringComparison.Ordinal)).Kind
            .Should().Be(Enum_SqlObjectKind.View);
        objects.Single(item => item.RelativePath.StartsWith("StoredProcedures/", StringComparison.Ordinal)).Kind
            .Should().Be(Enum_SqlObjectKind.Procedure);
        objects.Single(item => item.RelativePath.StartsWith("Triggers/", StringComparison.Ordinal)).Kind
            .Should().Be(Enum_SqlObjectKind.Trigger);
        objects.Single(item => item.RelativePath.StartsWith("SeedData/", StringComparison.Ordinal)).Kind
            .Should().Be(Enum_SqlObjectKind.Seed);
    }

    [Fact]
    public void LoadObjects_ShouldOrderTablesFirstAndSeedsLast_WhenEveryKindIsPresent()
    {
        WriteFile("SeedData/03_seed_dunnage_data.sql");
        WriteFile("Triggers/01_Trigger_demo.sql");
        WriteFile("StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");
        WriteFile("Views/05_View_dunnage_label_data.sql");
        WriteFile("Schemas/04_Table_dunnage_types.sql");

        var kinds = new Service_ObjectCatalog(_root).LoadObjects().Select(item => item.Kind);

        kinds.Should().Equal(
            Enum_SqlObjectKind.Schema,
            Enum_SqlObjectKind.View,
            Enum_SqlObjectKind.Procedure,
            Enum_SqlObjectKind.Trigger,
            Enum_SqlObjectKind.Seed);
    }

    [Fact]
    public void LoadObjects_ShouldRecurseIntoProcedureSubfoldersButNotIntoFlatFolders()
    {
        WriteFile("StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");
        WriteFile("Schemas/Nested/05_Table_unreachable.sql");

        var objects = new Service_ObjectCatalog(_root).LoadObjects();

        objects.Should().ContainSingle()
            .Which.RelativePath.Should().Be("StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");
    }

    [Fact]
    public void LoadObjects_ShouldIgnoreNonSqlFiles()
    {
        WriteFile("Schemas/04_Table_dunnage_types.sql");
        WriteFile("Schemas/notes.txt");

        new Service_ObjectCatalog(_root).LoadObjects().Should().ContainSingle();
    }

    [Fact]
    public void LoadObjects_ShouldReturnNothing_WhenTheSqlRootDoesNotExist()
    {
        var missing = Path.Combine(_root, "does-not-exist");

        new Service_ObjectCatalog(missing).LoadObjects().Should().BeEmpty();
    }

    private void WriteFile(string relativePath)
    {
        var path = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "SELECT 1;");
    }
}
