using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace MTM_Receiving_Application.Tests;

/// <summary>
/// Configures the LiveCharts2 SkiaSharp renderer once per test process so that
/// headless SKCartesianChart rendering (used by the Delivery Schedule chart-image
/// export) works inside unit tests. Mirrors the app-startup configuration.
/// </summary>
internal static class LiveChartsTestInitializer
{
    [ModuleInitializer]
    internal static void ConfigureLiveCharts()
    {
        LiveCharts.Configure(settings => settings.AddSkiaSharp());
    }
}
