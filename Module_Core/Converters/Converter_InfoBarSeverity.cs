using System;
using Microsoft.UI.Xaml.Data;
using CoreInfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;
using WinInfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Core.Converters
{
    public class Converter_InfoBarSeverity : IValueConverter
    {
        /// <summary>
        /// Converts the core InfoBar severity to the WinUI InfoBar severity.
        /// </summary>
        /// <param name="value">The core severity value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="language">The language for the conversion.</param>
        /// <returns>The WinUI severity value.</returns>
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            if (value is not CoreInfoBarSeverity severity)
            {
                return WinInfoBarSeverity.Informational;
            }

            return severity switch
            {
                CoreInfoBarSeverity.Success => WinInfoBarSeverity.Success,
                CoreInfoBarSeverity.Warning => WinInfoBarSeverity.Warning,
                CoreInfoBarSeverity.Error => WinInfoBarSeverity.Error,
                _ => WinInfoBarSeverity.Informational,
            };
        }

        /// <summary>
        /// Converts a WinUI InfoBar severity back to the core severity.
        /// </summary>
        /// <param name="value">The WinUI severity value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional conversion parameter.</param>
        /// <param name="language">The language for the conversion.</param>
        /// <returns>The core severity value.</returns>
        /// <exception cref="NotImplementedException">Always thrown; this conversion is not supported.</exception>
        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            string language
        )
        {
            throw new NotImplementedException();
        }
    }
}
