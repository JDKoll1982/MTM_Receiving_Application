using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Data;

namespace MTM_Receiving_Application.Module_Routing.Converters;

public class IntToFontWeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int currentStep && parameter is string stepStr && int.TryParse(stepStr, out int step))
        {
            return currentStep == step ? FontWeights.Bold : FontWeights.Normal;
        }
        return FontWeights.Normal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}