using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    /// <summary>
    /// Converts an integer value to Visibility
    /// - If ConverterParameter is an integer string (e.g., "1", "2"), compares value == parameter
    /// - If ConverterParameter is "Inverse", shows when value == 0
    /// - If no parameter, shows when value > 0
    /// </summary>
    public class Converter_IntToVisibility : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value == null)
                return Visibility.Collapsed;

            int intValue = value is int i ? i : 0;
            bool isVisible;

            // Handle different parameter types
            if (parameter is string paramStr)
            {
                // Check for "Inverse" parameter (legacy behavior: show when count is 0)
                if (paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
                {
                    isVisible = intValue == 0;
                }
                // Check for numeric parameter (e.g., "1", "2", "3" for step matching)
                else if (int.TryParse(paramStr, out int paramInt))
                {
                    isVisible = intValue == paramInt;
                }
                else
                {
                    // Fallback for unknown string parameters
                    isVisible = intValue > 0;
                }
            }
            else if (parameter is int paramIntDirect)
            {
                // Direct integer parameter comparison
                isVisible = intValue == paramIntDirect;
            }
            else
            {
                // No parameter: show if value > 0 (legacy behavior)
                isVisible = intValue > 0;
            }

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

