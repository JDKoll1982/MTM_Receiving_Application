using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Proves that the shipped tree deploys cleanly and that every stored procedure it declares still
/// runs against the schema it ships with.
/// </summary>
/// <remarks>
/// Each procedure is called with sample arguments derived from its own parameter metadata, so the
/// test answers one question for all of them: does the procedure body still match the tables and
/// views around it? A procedure is free to reject the sample input with its own error; only errors
/// that mean broken SQL are reported.
/// </remarks>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class StoredProcedureExecutionContractTests
{
    private readonly StoredProcedureTestDatabase _database;

    public StoredProcedureExecutionContractTests(StoredProcedureTestDatabase database)
    {
        _database = database;
    }

    public static TheoryData<string> ProcedureNames
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var name in StoredProcedureRepository.LoadProcedureNames())
            {
                data.Add(name);
            }

            return data;
        }
    }

    [SkippableFact]
    public void SqlTree_ShouldDeployWithoutFailures()
    {
        SkipDatabase();

        _database.DeploymentFailures.Should().BeEmpty(
            "every schema, view, procedure, and trigger script must deploy; the failures above "
            + "would otherwise be reported as hundreds of unrelated procedure failures");
    }

    [SkippableFact]
    public async Task DeployedSchema_ShouldHoldEveryProcedureInTheTree()
    {
        SkipDatabase();

        var deployed = await _database.ReadDeployedProcedureNamesAsync();
        var declared = StoredProcedureRepository.LoadProcedureNames();

        deployed.Should().BeEquivalentTo(declared);
    }

    [SkippableTheory]
    [MemberData(nameof(ProcedureNames))]
    public async Task Procedure_ShouldNotFailWithABrokenSqlError_WhenCalledWithSampleArguments(
        string procedureName)
    {
        SkipDatabase();

        var result = await StoredProcedureInvoker.InvokeAsync(
            _database.ConnectionString!,
            procedureName);

        result.LooksLikeBrokenSql.Should().BeFalse(
            $"{procedureName} must keep matching the deployed schema, but MySQL answered "
            + $"{result.ErrorNumber}: {result.ErrorMessage}");
    }

    private void SkipDatabase()
    {
        Skip.If(
            _database.ConnectionString is null,
            _database.SkipReason ?? "No MySQL server is available for the stored procedure tests.");
    }
}
