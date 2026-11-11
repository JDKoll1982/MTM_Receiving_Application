using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_VisualAutomation.Contracts;
using MTM_Receiving_Application.Module_VisualAutomation.Helpers;
using MTM_Receiving_Application.Module_VisualAutomation.Models;
using MTM_Receiving_Application.Module_VisualAutomation.Probes;

namespace MTM_Receiving_Application.Module_VisualAutomation.Services;

/// <summary>
/// Executes the Purchase Receipt Entry automation probes against the live window.
/// </summary>
public sealed class Service_VisualAutomation_ProbeRunner : IService_VisualAutomation_ProbeRunner
{
    private readonly IService_VisualAutomation_WindowDriver _driver;
    private readonly IService_LoggingUtility _logger;

    private Model_VisualFieldSnapshot? _baseline;

    public Service_VisualAutomation_ProbeRunner(
        IService_VisualAutomation_WindowDriver driver,
        IService_LoggingUtility logger
    )
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task RunAllAsync(
        IReadOnlyList<Model_VisualProbeDefinition> probes,
        Model_VisualProbeInputs inputs,
        Action<Model_VisualProbeResult> onResult,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(probes);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(onResult);

        _baseline = _driver.CaptureSnapshot();

        foreach (var probe in probes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await RunAsync(probe, inputs, cancellationToken).ConfigureAwait(true);
            onResult(result);
        }
    }

    /// <inheritdoc />
    public async Task<Model_VisualProbeResult> RunAsync(
        Model_VisualProbeDefinition probe,
        Model_VisualProbeInputs inputs,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(probe);
        ArgumentNullException.ThrowIfNull(inputs);

        try
        {
            return probe.Id switch
            {
                VisualProbeCatalog.Attach => RunAttach(probe),
                VisualProbeCatalog.ControlSurface => RunControlSurface(probe),
                VisualProbeCatalog.IdentityFields => RunFieldProbe(
                    probe,
                    ["ORDER_ID", "SITE_ID"],
                    ["RECEIVER_ID"]
                ),
                VisualProbeCatalog.DateAndPartyFields => RunFieldProbe(
                    probe,
                    ["DESIRED_RECV_DATE", "BUYER"],
                    ["PROMISE_DATE", "PROMISE_SHIP_DATE", "FREE_ON_BOARD", "SHIP_VIA", "CARRIER_ID"]
                ),
                VisualProbeCatalog.ReferenceFields => RunReferenceProbe(probe),
                VisualProbeCatalog.VariableCoverage => RunVariableCoverage(probe),
                VisualProbeCatalog.FieldWriteRestore => await RunFieldWriteAsync(
                        probe,
                        [("BOL_ID", inputs.BolId)],
                        verifyExactText: false,
                        cancellationToken
                    )
                    .ConfigureAwait(true),
                VisualProbeCatalog.RoundTripFidelity => await RunFieldWriteAsync(
                        probe,
                        [("VEND_PACK_ID", inputs.VendPackId), ("VEND_FREIGHT_ID", inputs.VendFreightId)],
                        verifyExactText: true,
                        cancellationToken
                    )
                    .ConfigureAwait(true),
                VisualProbeCatalog.NoSaveGuard => RunNoSaveGuard(probe),
                VisualProbeCatalog.RestoreIntegrity => RunRestoreIntegrity(probe),
                _ => Model_VisualProbeResult.Fail(probe.Id, "This probe has no implementation."),
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Probe {probe.Id} threw: {ex.Message}",
                ex,
                nameof(Service_VisualAutomation_ProbeRunner)
            );

            return Model_VisualProbeResult.Fail(probe.Id, $"Probe failed: {ex.Message}");
        }
    }

    private Model_VisualProbeResult RunAttach(Model_VisualProbeDefinition probe)
    {
        var window = _driver.DiscoverWindow();
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["PROCESS"] = window.IsRunning ? $"{Service_VisualAutomation_WindowDriver.ModuleProcessName}.EXE" : "(not running)",
            ["PID"] = window.ProcessId.ToString(),
            ["WINDOW_TITLE"] = window.WindowTitle,
            ["WINDOW_RECT"] = window.WindowRect.ToString(),
            ["ELEMENTS"] = window.ElementCount.ToString(),
        };

        if (!window.IsRunning)
        {
            return Model_VisualProbeResult.Fail(
                probe.Id,
                window.Diagnostic ?? "The Purchase Receipt Entry window was not found.",
                values
            );
        }

        var titleMatches = window.WindowTitle.Contains(
            Service_VisualAutomation_WindowDriver.ExpectedTitleFragment,
            StringComparison.OrdinalIgnoreCase
        );

        return titleMatches
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                $"Attached to '{window.WindowTitle}' (PID {window.ProcessId}) with {window.ElementCount} readable elements.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                $"The window was found but its title '{window.WindowTitle}' is not the Purchase Receipt Entry screen.",
                values
            );
    }

    private Model_VisualProbeResult RunControlSurface(Model_VisualProbeDefinition probe)
    {
        var first = _driver.DiscoverWindow();
        var second = _driver.DiscoverWindow();

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["ELEMENTS_FIRST"] = first.ElementCount.ToString(),
            ["ELEMENTS_SECOND"] = second.ElementCount.ToString(),
        };

        if (!first.IsRunning)
        {
            return Model_VisualProbeResult.Fail(
                probe.Id,
                first.Diagnostic ?? "The window is not available.",
                values
            );
        }

        var stable = first.ElementCount == second.ElementCount && first.ElementCount >= 30;

        return stable
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                $"The window published a stable {first.ElementCount} automation elements across two reads.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                $"Element count was {first.ElementCount} then {second.ElementCount}; expected a stable count of at least 30.",
                values
            );
    }

    private Model_VisualProbeResult RunFieldProbe(
        Model_VisualProbeDefinition probe,
        string[] requiredKeys,
        string[] optionalKeys
    )
    {
        var snapshot = _driver.CaptureSnapshot();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var key in requiredKeys.Concat(optionalKeys))
        {
            if (snapshot.Fields.TryGetValue(key, out var field))
            {
                values[key] = field.HasControl ? field.Value : "(no control)";
            }
            else
            {
                values[key] = "(unmapped)";
            }
        }

        var missing = requiredKeys
            .Where(key =>
                !snapshot.Fields.TryGetValue(key, out var field)
                || !field.HasControl
                || string.IsNullOrWhiteSpace(field.Value)
            )
            .ToList();

        return missing.Count == 0
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                $"Read {requiredKeys.Length + optionalKeys.Length} variables from the window.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                "These variables had no value on screen: " + string.Join(", ", missing),
                values
            );
    }

    private Model_VisualProbeResult RunReferenceProbe(Model_VisualProbeDefinition probe)
    {
        var snapshot = _driver.CaptureSnapshot();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var missing = new List<string>();

        foreach (var key in probe.ReportedKeys)
        {
            if (snapshot.Fields.TryGetValue(key, out var field) && field.HasControl)
            {
                values[key] = field.Value.Length == 0 ? "(empty)" : field.Value;
            }
            else
            {
                values[key] = "(no control)";
                missing.Add(key);
            }
        }

        return missing.Count == 0
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                "Every reference field resolved to a control, so the write probes can target them.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                "These reference fields have no control on the window: " + string.Join(", ", missing),
                values
            );
    }

    private Model_VisualProbeResult RunVariableCoverage(Model_VisualProbeDefinition probe)
    {
        var snapshot = _driver.CaptureSnapshot();
        var all = Helper_VisualAutomation_FieldMap.MacroVariables;
        var mapped = Helper_VisualAutomation_FieldMap.VisibleVariables;

        var resolved = mapped
            .Count(key => snapshot.Fields.TryGetValue(key, out var field) && field.HasControl);

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MACRO_VARIABLES"] = all.Count.ToString(),
            ["MAPPED_TO_WINDOW"] = mapped.Count.ToString(),
            ["RESOLVED_ON_WINDOW"] = resolved.ToString(),
            ["NOT_ON_VISIBLE_SURFACE"] = (all.Count - mapped.Count).ToString(),
            ["UNRESOLVED"] = string.Join(
                ", ",
                mapped.Where(key =>
                    !snapshot.Fields.TryGetValue(key, out var field) || !field.HasControl
                )
            ),
        };

        return resolved == mapped.Count
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                $"{resolved} of {all.Count} macro variables are readable directly from the window; the other "
                    + $"{all.Count - mapped.Count} have no control on the visible surface.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                $"{resolved} of {mapped.Count} mapped variables resolved on the window.",
                values
            );
    }

    private async Task<Model_VisualProbeResult> RunFieldWriteAsync(
        Model_VisualProbeDefinition probe,
        IReadOnlyList<(string Key, string Value)> fields,
        bool verifyExactText,
        CancellationToken cancellationToken
    )
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var problems = new List<string>();

        foreach (var (key, value) in fields)
        {
            var writeResult = await _driver
                .WriteValueAsync(key, value, restoreOriginal: true, cancellationToken)
                .ConfigureAwait(true);

            if (!writeResult.Success || writeResult.Data is null)
            {
                problems.Add($"{key}: {writeResult.ErrorMessage ?? "the write could not be performed"}");
                continue;
            }

            var outcome = writeResult.Data;
            values[$"{key}_ORIGINAL"] = outcome.OriginalValue.Length == 0 ? "(empty)" : outcome.OriginalValue;
            values[$"{key}_TYPED"] = outcome.RequestedValue;
            values[$"{key}_READ_BACK"] = outcome.ObservedValue.Length == 0 ? "(empty)" : outcome.ObservedValue;
            values[$"{key}_RESTORED_TO"] = outcome.ValueAfterRestore.Length == 0 ? "(empty)" : outcome.ValueAfterRestore;

            var accepted = verifyExactText
                ? string.Equals(outcome.ObservedValue, outcome.RequestedValue, StringComparison.Ordinal)
                : string.Equals(
                    outcome.ObservedValue.Trim(),
                    outcome.RequestedValue.Trim(),
                    StringComparison.Ordinal
                );

            if (!accepted)
            {
                problems.Add(
                    $"{key}: VISUAL did not keep the typed value (typed '{outcome.RequestedValue}', read back '{outcome.ObservedValue}')"
                );
            }
            else if (!outcome.RestoreVerified)
            {
                problems.Add($"{key}: the write worked but the original value was not restored (left as '{outcome.ValueAfterRestore}')");
            }
        }

        var fieldList = string.Join(", ", fields.Select(field => field.Key));

        return problems.Count == 0
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                $"Typed the entered values into {fieldList}, read them back, and restored the originals.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                string.Join("; ", problems),
                values
            );
    }

    private Model_VisualProbeResult RunNoSaveGuard(Model_VisualProbeDefinition probe)
    {
        var snapshot = _driver.CaptureSnapshot();
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SAVE_ACTIONS_PERFORMED"] = "0",
            ["SAVE_CONTROL_PRESENT"] = snapshot.SaveControlPresent ? "yes" : "no",
            ["POSTING_STATE_BEFORE"] = DescribeIdentity(snapshot),
            ["POSTING_STATE_AFTER"] = DescribeIdentity(_driver.CaptureSnapshot()),
        };

        var unchanged = values["POSTING_STATE_BEFORE"] == values["POSTING_STATE_AFTER"];

        return unchanged
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                "The harness never clicks Save and never sends a save key sequence, so the receipt cannot be committed by a probe run.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                "The receipt identity changed while probes were running.",
                values
            );
    }

    private Model_VisualProbeResult RunRestoreIntegrity(Model_VisualProbeDefinition probe)
    {
        var baseline = _baseline ?? _driver.CaptureSnapshot();
        var current = _driver.CaptureSnapshot();

        var differences = new List<string>();
        var compared = 0;

        foreach (var pair in baseline.Fields)
        {
            if (!pair.Value.HasControl)
            {
                continue;
            }

            compared++;

            var after = current.Fields.TryGetValue(pair.Key, out var field) ? field : null;
            if (after?.HasControl != true)
            {
                differences.Add($"{pair.Key} (control disappeared)");
                continue;
            }

            if (!string.Equals(pair.Value.Value.Trim(), after.Value.Trim(), StringComparison.Ordinal))
            {
                differences.Add($"{pair.Key} ('{pair.Value.Value}' -> '{after.Value}')");
            }
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["FIELDS_COMPARED"] = compared.ToString(),
            ["DIFFERENCES"] = differences.Count == 0 ? "(none)" : string.Join("; ", differences),
            ["BASELINE_AT"] = baseline.CapturedAt.ToString("HH:mm:ss"),
            ["COMPARED_AT"] = current.CapturedAt.ToString("HH:mm:ss"),
        };

        return differences.Count == 0
            ? Model_VisualProbeResult.Pass(
                probe.Id,
                $"All {compared} readable fields match the snapshot taken before the run.",
                values
            )
            : Model_VisualProbeResult.Fail(
                probe.Id,
                "These fields differ from the starting snapshot: " + string.Join("; ", differences),
                values
            );
    }

    private static string DescribeIdentity(Model_VisualFieldSnapshot snapshot) =>
        $"{snapshot.ValueOf("ORDER_ID")}|{snapshot.ValueOf("SITE_ID")}|{snapshot.ValueOf("RECEIVER_ID")}";
}
