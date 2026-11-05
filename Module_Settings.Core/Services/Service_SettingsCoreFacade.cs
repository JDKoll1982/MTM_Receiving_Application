using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Facade for external modules to access core settings.
/// </summary>
public class Service_SettingsCoreFacade : IService_SettingsCoreFacade
{
    private readonly IMediator _mediator;
    private readonly ISettingsMetadataRegistry _registry;
    private readonly ISettingsRowSnapshot _snapshot;

    public Service_SettingsCoreFacade(
        IMediator mediator,
        ISettingsMetadataRegistry registry,
        ISettingsRowSnapshot snapshot
    )
    {
        _mediator = mediator;
        _registry = registry;
        _snapshot = snapshot;
    }

    public Task<Model_Dao_Result<Model_SettingsValue>> GetSettingAsync(
        string category,
        string key,
        int? userId = null
    )
    {
        return _mediator.Send(new GetSettingQuery(category, key, userId));
    }

    public Task<Model_Dao_Result> SetSettingAsync(
        string category,
        string key,
        string value,
        int? userId = null
    )
    {
        return _mediator.Send(new SetSettingCommand(category, key, value, userId));
    }

    public Task<Model_Dao_Result> ResetSettingAsync(string category, string key, int? userId = null)
    {
        return _mediator.Send(new ResetSettingCommand(category, key, userId));
    }

    /// <summary>
    /// Makes sure a row exists for every setting that could be read, and leaves the resolved value cached
    /// so nothing has to go back to the database again during this run.
    /// </summary>
    /// <remarks>
    /// Both scopes are resolved here on purpose. Seeding is only ever needed for the user's own rows, but
    /// the cache has to be warm for everything: the screens load their wording one key at a time while the
    /// main window is coming up, and a cold system-scoped key costs a round trip each — two hundred of them,
    /// one after another, which is the blank window an operator sees after the splash goes away. Walking the
    /// whole set here, a few at a time and while the splash is still up, turns that into a visible step with
    /// a number on it.
    /// </remarks>
    public async Task InitializeDefaultsAsync(
        int? userId = null,
        IProgress<Model_SettingsSeedProgress>? progress = null
    )
    {
        var definitions = _registry
            .GetAll()
            .Where(definition =>
                // A user-scoped setting has nothing to resolve without a user; asking would only produce a
                // failure per key.
                userId.HasValue || definition.Scope != Enum_SettingsScope.User
            )
            .ToList();

        if (definitions.Count == 0)
        {
            return;
        }

        // One read per category instead of one per setting, so the pass below resolves from memory and
        // only touches the database for keys that genuinely have no row yet.
        await _snapshot.PrimeAsync(
            userId,
            definitions.Select(definition => definition.Category).Distinct().ToList()
        );

        var completed = 0;
        progress?.Report(new Model_SettingsSeedProgress(0, definitions.Count, string.Empty));

        await Parallel.ForEachAsync(
            definitions,
            new ParallelOptions { MaxDegreeOfParallelism = SeedConcurrency },
            async (definition, _) =>
            {
                await _mediator.Send(
                    new GetSettingQuery(
                        definition.Category,
                        definition.Key,
                        definition.Scope == Enum_SettingsScope.User ? userId : null
                    )
                );

                var done = Interlocked.Increment(ref completed);
                progress?.Report(
                    new Model_SettingsSeedProgress(done, definitions.Count, definition.Key)
                );
            }
        );
    }

    /// <summary>
    /// How many settings are resolved at once. Enough to hide the round-trip latency, low enough not to
    /// swamp a site's connection pool while a terminal is starting up.
    /// </summary>
    private const int SeedConcurrency = 12;
}
