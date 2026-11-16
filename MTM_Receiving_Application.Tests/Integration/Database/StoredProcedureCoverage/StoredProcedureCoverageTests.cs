using System.Text.RegularExpressions;
using FluentAssertions;
using MTM_Database_Deployment.Models;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Guards the shape of the shipped procedure tree and proves that no procedure is left without a
/// test that names it.
/// </summary>
/// <remarks>
/// The "named by a test" rule is what keeps coverage honest as the tree grows: adding a procedure
/// without a test that calls it by name turns this test red and prints the names that are missing.
/// </remarks>
public sealed partial class StoredProcedureCoverageTests
{
    [Fact]
    public void SqlTree_ShouldDeclareStoredProcedures()
    {
        var names = StoredProcedureRepository.LoadProcedureNames();

        names.Should().NotBeEmpty(
            "the SQL tree must be reachable from the test output folder, otherwise every coverage "
            + "test here is meaningless");
    }

    [Fact]
    public void EveryProcedureName_ShouldBeUnique()
    {
        var names = StoredProcedureRepository.LoadProcedureNames();
        var declared = StoredProcedureRepository
            .LoadSqlObjects()
            .Where(item => item.Kind == Enum_SqlObjectKind.Procedure)
            .SelectMany(item => StoredProcedureRepository.ReadDeclaredProcedureNames(item.FullPath))
            .ToList();

        var duplicates = declared
            .GroupBy(name => name, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        duplicates.Should().BeEmpty("a procedure name may only be created once in a schema");
        names.Should().HaveCount(declared.Count);
    }

    [Fact]
    public void EveryProcedureScript_ShouldDeclareAtLeastOneProcedure()
    {
        var scriptsWithoutProcedure = StoredProcedureRepository
            .LoadSqlObjects()
            .Where(item => item.Kind == Enum_SqlObjectKind.Procedure)
            .Where(item => StoredProcedureRepository.ReadDeclaredProcedureNames(item.FullPath).Count == 0)
            .Select(item => item.RelativePath)
            .ToList();

        scriptsWithoutProcedure.Should().BeEmpty(
            "every file under StoredProcedures must create a procedure");
    }

    [Fact]
    public void EveryProcedure_ShouldBeNamedByATest()
    {
        var names = StoredProcedureRepository.LoadProcedureNames();
        var sources = StoredProcedureRepository.LoadTestSourceFiles();

        sources.Should().NotBeEmpty("the test project sources must be reachable");

        var testSource = string.Concat(sources.Select(File.ReadAllText));
        var referenced = ProcedureTokenPattern()
            .Matches(testSource)
            .Select(match => match.Value)
            .ToHashSet(StringComparer.Ordinal);

        var untested = names.Where(name => !referenced.Contains(name)).ToList();

        untested.Should().BeEmpty(
            $"every stored procedure needs a test that calls it by name; {untested.Count} of "
            + $"{names.Count} are missing one: {string.Join(", ", untested)}");
    }

    [Fact]
    public void ProcedureNames_ShouldFollowTheSpPrefixConvention()
    {
        var offConvention = StoredProcedureRepository
            .LoadProcedureNames()
            .Where(name => !name.StartsWith("sp_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        offConvention.Should().BeEmpty("procedure names start with sp_");
    }

    [GeneratedRegex(@"\bsp_[A-Za-z0-9_]+")]
    private static partial Regex ProcedureTokenPattern();
}
