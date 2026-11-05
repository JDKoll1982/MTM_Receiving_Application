using System;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// How far a print run has got, reported once per page handed to the printer.
/// </summary>
/// <remarks>
/// A print job is slow enough to look like a hang, and the only honest measure of progress is the pages the
/// driver has taken. Nothing here is estimated: <see cref="Completed"/> counts pages the printer has
/// actually finished with.
/// </remarks>
/// <param name="Completed">Pages handed to the printer so far.</param>
/// <param name="Total">Pages the run will send.</param>
public sealed record Model_LabelOverlayPrintProgress(int Completed, int Total)
{
    /// <summary>How far through the run this is, from 0 to 100.</summary>
    public double PercentComplete => Total <= 0 ? 0d : Math.Min(100d, (Completed * 100d) / Total);
}
