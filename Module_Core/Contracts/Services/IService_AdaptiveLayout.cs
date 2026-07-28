using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

public interface IService_AdaptiveLayout
{
    string ResolveScannerLayoutState(double availableWidthEpx);

    Thickness GetScannerContentPadding(double availableWidthEpx);

    double CalculateBoundedViewportHeight(double containerHeightEpx, params double[] occupiedHeightsEpx);
}
