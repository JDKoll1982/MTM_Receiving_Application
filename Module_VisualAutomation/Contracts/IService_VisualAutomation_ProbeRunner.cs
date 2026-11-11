using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Contracts;

/// <summary>
/// Runs an automation probe against the live Infor VISUAL receipt window and returns a verdict.
/// </summary>
public interface IService_VisualAutomation_ProbeRunner
{
    /// <summary>
    /// Runs every probe in the supplied order and reports each result as soon as it is known.
    /// </summary>
    /// <param name="probes">Probes to run.</param>
    /// <param name="inputs">The values the probes must send to VISUAL.</param>
    /// <param name="onResult">Invoked for each completed probe.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when every probe has run.</returns>
    Task RunAllAsync(
        IReadOnlyList<Model_VisualProbeDefinition> probes,
        Model_VisualProbeInputs inputs,
        Action<Model_VisualProbeResult> onResult,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Runs a single probe.
    /// </summary>
    /// <param name="probe">Probe to run.</param>
    /// <param name="inputs">The values the probe must send to VISUAL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The probe result.</returns>
    Task<Model_VisualProbeResult> RunAsync(
        Model_VisualProbeDefinition probe,
        Model_VisualProbeInputs inputs,
        CancellationToken cancellationToken = default
    );
}
