using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Fully describes one SyncTool invocation requested from the Database Config page.
/// Maps 1:1 onto the command-line surface of Database/SyncTool/sync_reference_data.py.
/// </summary>
public sealed class Model_SyncToolRunRequest
{
    /// <summary>Gets or sets the action (subcommand) to run.</summary>
    public Enum_SyncToolAction Action { get; set; } = Enum_SyncToolAction.WhatIf;

    /// <summary>
    /// Gets or sets the database the action syncs into: "core" or "backup_test"
    /// (matching the database names in Database/SyncTool/sync_policy.yml).
    /// </summary>
    public string Target { get; set; } = "core";

    /// <summary>
    /// Gets or sets a value indicating whether writes into the LIVE core database
    /// are acknowledged (maps to the tool's --confirm flag).
    /// </summary>
    public bool ConfirmCoreWrite { get; set; }

    /// <summary>Gets or sets a value indicating whether post-execution validation is skipped (--skip-validation).</summary>
    public bool SkipValidation { get; set; }

    /// <summary>Gets or sets a value indicating whether the disposable copy is kept after verify-copy (--keep-copy).</summary>
    public bool KeepCopy { get; set; }

    /// <summary>Gets or sets the output .sql path for the generate-sql action.</summary>
    public string? GenerateSqlOutputPath { get; set; }

    /// <summary>Gets or sets the absolute path to sync_reference_data.py.</summary>
    public string SyncToolScriptPath { get; set; } = string.Empty;
}
