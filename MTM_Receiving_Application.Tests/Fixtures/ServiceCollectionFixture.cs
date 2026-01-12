using System;

namespace MTM_Receiving_Application.Tests.Fixtures;

/// <summary>
/// Shared test fixture for DI container setup.
/// This can be expanded to register services/DAOs with test doubles.
/// </summary>
public sealed class ServiceCollectionFixture : IDisposable
{
    public ServiceCollectionFixture()
    {
        // Intentionally empty: register services in tests as needed.
    }

    public void Dispose()
    {
        // Intentionally empty: dispose provider/resources when added.
    }
}
