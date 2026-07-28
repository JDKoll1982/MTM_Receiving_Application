using System;
using System.Linq;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services.UI;

public class Service_AdaptiveLayout : IService_AdaptiveLayout
{
    private const double CompactBreakpoint = 1280;
    private const double WideBreakpoint = 1700;

    public string ResolveScannerLayoutState(double availableWidthEpx)
    {
        if (availableWidthEpx <= CompactBreakpoint)
        {
            return "ScannerCompactState";
        }

        if (availableWidthEpx >= WideBreakpoint)
        {
            return "ScannerWideState";
        }

        return "ScannerDefaultState";
    }

    public Thickness GetScannerContentPadding(double availableWidthEpx)
    {
        if (availableWidthEpx <= CompactBreakpoint)
        {
            return new Thickness(10);
        }

        if (availableWidthEpx >= WideBreakpoint)
        {
            return new Thickness(20);
        }

        return new Thickness(16);
    }

    public double CalculateBoundedViewportHeight(
        double containerHeightEpx,
        params double[] occupiedHeightsEpx
    )
    {
        if (double.IsNaN(containerHeightEpx) || double.IsInfinity(containerHeightEpx))
        {
            return 0;
        }

        var occupiedHeight = occupiedHeightsEpx
            .Where(value => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0)
            .Sum();

        var availableHeight = containerHeightEpx - occupiedHeight;
        return Math.Max(0, availableHeight);
    }
}
