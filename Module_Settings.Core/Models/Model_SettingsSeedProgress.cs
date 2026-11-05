using System;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// How far the per-user settings seeding has got, reported once per setting it looks at.
/// </summary>
/// <remarks>
/// Seeding touches every user-scoped setting, which is the slowest part of starting a session. Reporting it
/// is what lets the splash screen show movement instead of appearing to hang on one step.
/// </remarks>
/// <param name="Completed">Settings looked at so far.</param>
/// <param name="Total">Settings this user needs.</param>
/// <param name="Key">The setting just looked at, for a message that says what is happening.</param>
public sealed record Model_SettingsSeedProgress(int Completed, int Total, string Key)
{
    /// <summary>How far through the seeding this is, from 0 to 100.</summary>
    public double PercentComplete => Total <= 0 ? 0d : Math.Min(100d, (Completed * 100d) / Total);
}
