namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Shares one deployed scratch schema across every stored procedure test, so the tree is deployed
/// once instead of once per test class.
/// </summary>
/// <remarks>
/// Parallelism is off because the tests seed and delete rows in that shared schema.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class StoredProcedureCoverageCollection : ICollectionFixture<StoredProcedureTestDatabase>
{
    public const string Name = "Stored procedure coverage";
}
