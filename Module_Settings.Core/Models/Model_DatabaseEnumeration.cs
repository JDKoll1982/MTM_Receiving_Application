using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Result of enumerating the databases present on the configured MySQL server.
/// </summary>
/// <param name="Success">Whether the server responded.</param>
/// <param name="Databases">Existing database names when the query succeeded; empty otherwise.</param>
/// <param name="Message">Diagnostic message describing the outcome.</param>
public sealed record Model_DatabaseEnumeration(
    bool Success,
    IReadOnlyList<string> Databases,
    string Message
);
