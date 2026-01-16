# XAML Converter Standards

**Category**: UI / View
**Last Updated**: December 26, 2025
**Applies To**: `Converters/*.cs`, `App.xaml`, `Views/**/*.xaml`

## Overview

Value converters allow data binding to transform data from the ViewModel format to the View format (e.g., `bool` to `Visibility`, `decimal` to `string`).

## Standard Converters

The application includes a set of standard converters in the `Converters/` namespace.

| Converter | Source Type | Target Type | Usage |
|-----------|-------------|-------------|-------|
| `BooleanToVisibilityConverter` | `bool` | `Visibility` | Show/hide elements based on flag. True=Visible. |
| `BoolToStringConverter` | `bool` | `string` | Display "Yes"/"No" or custom strings. |
| `DecimalToStringConverter` | `decimal` | `string` | Format numbers (e.g., currency, fixed decimal). |
| `IntToVisibilityConverter` | `int` | `Visibility` | Show if count > 0. |
| `StringFormatConverter` | `object` | `string` | General string formatting. |
| `InverseBoolConverter` | `bool` | `bool` | Negate boolean value (often used with IsEnabled). |

## Usage in XAML

1. **Resource Definition**: Converters are typically defined as global resources in `App.xaml` or local resources in the Page/Window.

    ```xml
    <Application.Resources>
        <converters:BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter"/>
        <converters:InverseBoolConverter x:Key="InverseBoolConverter"/>
    </Application.Resources>
    ```

2. **Binding**: Use the `Converter` property in `x:Bind` or `Binding`.

    ```xml
    <StackPanel Visibility="{x:Bind ViewModel.IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}">
        <ProgressRing IsActive="True"/>
    </StackPanel>
    
    <Button IsEnabled="{x:Bind ViewModel.IsBusy, Converter={StaticResource InverseBoolConverter}}"/>
    ```

## Creating New Converters

1. Implement `IValueConverter`.
2. Implement `Convert` (Source -> Target).
3. Implement `ConvertBack` (Target -> Source) if TwoWay binding is needed (often `NotImplementedException` for OneWay).
4. Add `[ValueConversion(typeof(Source), typeof(Target))]` attribute (optional but good practice).

```csharp
public class MyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // Logic
    }
    // ...
}
```
