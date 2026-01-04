using Microsoft.UI.Xaml.Data;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using WinUIInfoBarSeverity = Microsoft.UI.Xaml.Controls.InfoBarSeverity;
using System;

namespace MTM_Receiving_Application.Converters;

public class Converter_CoreInfoBarSeverityToWinUI : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is InfoBarSeverity severity)
        {
            return severity switch
            {
                InfoBarSeverity.Informational => WinUIInfoBarSeverity.Informational,
                InfoBarSeverity.Success => WinUIInfoBarSeverity.Success,
                InfoBarSeverity.Warning => WinUIInfoBarSeverity.Warning,
                InfoBarSeverity.Error => WinUIInfoBarSeverity.Error,
                _ => WinUIInfoBarSeverity.Informational
            };
        }
        return WinUIInfoBarSeverity.Informational;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
