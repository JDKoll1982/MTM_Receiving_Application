using System;

namespace MTM_Receiving_Application.Tests.Fixtures;

/// <summary>
/// Shared test fixture for database-related tests.
/// Keep this lightweight; prefer per-test isolation unless explicitly needed.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    public DatabaseFixture()
    {
        // Intentionally empty: initialize per-module DB helpers when needed.
    }

    public void Dispose()
    {
        // Intentionally empty: dispose connections/resources when added.
    }
}
