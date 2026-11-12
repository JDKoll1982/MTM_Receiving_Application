using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Supplies the module's two configurable windows. Every read falls back to the
/// module's own default rather than failing the day (FR-059).
/// </summary>
public interface IService_ReconciliationSettings
{
    /// <summary>Days either side of the receipt date used to discover movements.</summary>
    Task<int> GetDiscoveryWindowDaysAsync();

    /// <summary>Days after the receipt date used for strict validation.</summary>
    Task<int> GetStrictWindowDaysAsync();

    /// <summary>Stores the movement discovery window.</summary>
    /// <param name="days">The window in days.</param>
    Task SetDiscoveryWindowDaysAsync(int days);

    /// <summary>Stores the strict validation window.</summary>
    /// <param name="days">The window in days.</param>
    Task SetStrictWindowDaysAsync(int days);
}
