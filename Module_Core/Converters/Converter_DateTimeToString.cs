using Microsoft.UI.Xaml.Data;
using System;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> or nullable <see cref="DateTime"/> to a formatted string.
    /// Accepts an optional format string via <c>ConverterParameter</c> (default: "MM/dd/yyyy").
    /// Returns an empty string for null or <see cref="DateTime.MinValue"/>.
    /// </summary>
    public class Converter_DateTimeToString : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            var format = parameter as string ?? "MM/dd/yyyy";

            return value switch
            {
                DateTime dt when dt > DateTime.MinValue => dt.ToString(format),
                _ => string.Empty
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        {
            if (value is string s && DateTime.TryParse(s, out var result))
            {
                return result;
            }

            return DateTime.MinValue;
        }
    }
}
