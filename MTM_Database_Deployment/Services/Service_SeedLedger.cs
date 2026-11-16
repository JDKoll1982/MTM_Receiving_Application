using System.Security.Cryptography;
using System.Text;
using MTM_Database_Deployment.Models;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Remembers which seed scripts have already been written into a database so a safe update never
/// writes the same reference data again.
/// </summary>
/// <remarks>
/// A seed is identified by its path in the SQL tree plus a fingerprint of its text. The record is kept
/// per server, port, and database inside <c>deployment.settings.json</c> next to the tool, so pointing
/// the tool at another database starts with an empty history. Editing a seed changes its fingerprint,
/// which makes the next safe update apply the edited file.
/// </remarks>
public sealed class Service_SeedLedger
{
    private readonly Model_ConnectionSettings _settings;

    public Service_SeedLedger(Model_ConnectionSettings settings) => _settings = settings;

    /// <summary>
    /// The key that scopes a seed history to one database on one server.
    /// </summary>
    public static string ScopeFor(string? server, int port, string? database) =>
        $"{Normalize(server)}:{port}/{Normalize(database)}";

    /// <summary>
    /// A stable, content-only identity for a seed file. Line endings are ignored so a checkout with
    /// different newlines does not look like an edit.
    /// </summary>
    public static string Fingerprint(string? script)
    {
        var normalized = (script ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
    }

    /// <summary>
    /// True when this exact seed text has already run against the scoped database.
    /// </summary>
    public bool WasApplied(string scope, string relativePath, string fingerprint)
    {
        if (!_settings.AppliedSeeds.TryGetValue(scope, out var seeds) || seeds is null)
        {
            return false;
        }

        return seeds.TryGetValue(Normalize(relativePath), out var applied)
               && string.Equals(applied, fingerprint, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Records that this seed text is now in the scoped database. Callers persist the settings
    /// afterwards; the change is in memory until then.
    /// </summary>
    public void Record(string scope, string relativePath, string fingerprint)
    {
        if (!_settings.AppliedSeeds.TryGetValue(scope, out var seeds) || seeds is null)
        {
            seeds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _settings.AppliedSeeds[scope] = seeds;
        }

        seeds[Normalize(relativePath)] = fingerprint;
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
