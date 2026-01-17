using Xunit;

namespace MTM_Receiving_Application.Tests.Helpers;

/// <summary>
/// Shared collection for database integration tests.
/// </summary>
[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}
