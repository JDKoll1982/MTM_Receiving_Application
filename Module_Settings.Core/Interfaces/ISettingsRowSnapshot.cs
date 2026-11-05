using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// A bulk read of the settings rows, taken once so a startup does not pay a round trip per setting.
/// </summary>
/// <remarks>
/// Resolving a setting is not a lookup — the value may be encrypted, may need a default written, and may
/// have to be validated. That work belongs in one place, so this holds only the <em>raw</em> rows and the
/// handler still does the resolving; it just stops asking the database for each one. A primed snapshot is
/// authoritative: a row it does not hold is genuinely missing, not "ask again".
/// </remarks>
public interface ISettingsRowSnapshot
{
    /// <summary>True once <see cref="PrimeAsync"/> has completed, after which rows are served from here.</summary>
    bool IsPrimed { get; }

    /// <summary>
    /// Loads every row for the categories in play, in a handful of reads instead of one per setting.
    /// </summary>
    /// <param name="userId">The signed-in user, or null to load system-scoped rows only.</param>
    /// <param name="categories">The categories to load; others are left out.</param>
    Task PrimeAsync(
        int? userId,
        IReadOnlyCollection<string> categories,
        CancellationToken cancellationToken = default
    );

    /// <summary>The system row for a key, or null when the snapshot holds none.</summary>
    Model_CoreSetting? FindSystem(string category, string key);

    /// <summary>The row for a key belonging to this user, or null when the snapshot holds none.</summary>
    Model_UserSetting? FindUser(int userId, string category, string key);
}
