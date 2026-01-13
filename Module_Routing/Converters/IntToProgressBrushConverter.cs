using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Routing.Converters;

public class IntToProgressBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
        {
            if (currentStep >= step)
            {
                return Application.Current.Resources["AccentFillColorDefaultBrush"] as Brush ?? DependencyProperty.UnsetValue;
            }
        }
        return Application.Current.Resources["SystemControlBackgroundBaseLowBrush"] as Brush ?? DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}