using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage.Helpers;

namespace MTM_Receiving_Application.Views.Dunnage.Helpers;

/// <summary>
/// DataTemplateSelector for dynamic specification input controls.
/// Selects appropriate template (TextBox, NumberBox, CheckBox) based on spec data type.
/// </summary>
public class SpecInputTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? NumberTemplate { get; set; }
    public DataTemplate? BooleanTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        if (item is SpecInputViewModel spec)
        {
            return spec.DataType.ToLower() switch
            {
                "text" => TextTemplate,
                "number" => NumberTemplate,
                "boolean" => BooleanTemplate,
                _ => TextTemplate // Default to text input
            };
        }

        return base.SelectTemplateCore(item);
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return SelectTemplateCore(item);
    }
}
