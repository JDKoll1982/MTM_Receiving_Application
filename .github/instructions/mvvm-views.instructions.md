---
description: Guidelines for creating Views (XAML pages) in the MTM Receiving Application
applyTo: 'Views/**/*.xaml'
---

# View (XAML) Development Guidelines

## Purpose

This file provides guidelines for creating WinUI 3 XAML pages following MVVM pattern.

## Core Principles

1. **Every View has a corresponding ViewModel**
2. **Use data binding, not code-behind logic**
3. **ViewModel is retrieved from DI container**
4. **Use x:Bind for performance (not Binding)**

## XAML Page Structure

### Basic Page Template

```xml
<Page
    x:Class="MTM_Receiving_Application.Views.FeatureName.MyPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.ViewModels.FeatureName"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Grid Padding="24">
        <!-- Content here -->
    </Grid>
</Page>
```

### Code-Behind Template

```csharp
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.FeatureName;

namespace MTM_Receiving_Application.Views.FeatureName;

public sealed partial class MyPage : Page
{
    public MyViewModel ViewModel { get; }

    public MyPage()
    {
        InitializeComponent();
        ViewModel = App.GetService<MyViewModel>();
        DataContext = ViewModel;
    }
}
```

## Data Binding

### Use x:Bind (Compiled Binding)

✅ Preferred:

```xml
<TextBox Text="{x:Bind ViewModel.PartID, Mode=TwoWay}"/>
<TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
<Button Command="{x:Bind ViewModel.SaveCommand}"/>
```

❌ Avoid (slower):

```xml
<TextBox Text="{Binding PartID, Mode=TwoWay}"/>
```

### Binding Modes

- `Mode=TwoWay` - For input controls (TextBox, ComboBox, etc.)
- `Mode=OneWay` - For display (TextBlock, ListViews, etc.)
- `Mode=OneTime` - For static values that don't change

## Common Controls

### TextBox

```xml
<TextBox 
    Header="Part ID"
    PlaceholderText="Enter part ID"
    Text="{x:Bind ViewModel.PartID, Mode=TwoWay}"/>
```

### Buttons

```xml
<Button 
    Content="Save"
    Command="{x:Bind ViewModel.SaveCommand}"
    Style="{StaticResource AccentButtonStyle}"/>
```

### ListView/DataGrid

```xml
<ListView ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:MyModel">
            <TextBlock Text="{x:Bind PropertyName}"/>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### DatePicker

```xml
<CalendarDatePicker 
    Header="Date"
    Date="{x:Bind ViewModel.SelectedDate, Mode=TwoWay}"/>
```

### ComboBox

```xml
<ComboBox 
    Header="Location"
    ItemsSource="{x:Bind ViewModel.Locations, Mode=OneWay}"
    SelectedItem="{x:Bind ViewModel.SelectedLocation, Mode=TwoWay}"/>
```

## Layout

### Grid with Spacing

```xml
<Grid ColumnSpacing="12" RowSpacing="12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    <!-- Content -->
</Grid>
```

### StackPanel

```xml
<StackPanel Spacing="8" Orientation="Vertical">
    <!-- Vertical stack of controls -->
</StackPanel>
```

## Status Display

### Show Busy Indicator

```xml
<ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" 
              Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay}"/>
```

### Status Message

```xml
<TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
           Foreground="{ThemeResource SystemAccentColor}"/>
```

## Command Buttons

### CommandBar

```xml
<CommandBar DefaultLabelPosition="Right">
    <AppBarButton 
        Icon="Save" 
        Label="Save to History" 
        Command="{x:Bind ViewModel.SaveToHistoryCommand}"/>
    <AppBarButton 
        Icon="Sort" 
        Label="Sort" 
        Command="{x:Bind ViewModel.SortCommand}"/>
</CommandBar>
```

## Styling

### Use Theme Resources

```xml
<TextBlock Style="{StaticResource TitleTextBlockStyle}"/>
<TextBlock Style="{StaticResource BodyTextBlockStyle}"/>
<Button Style="{StaticResource AccentButtonStyle}"/>
```

### Margins and Padding

```xml
<StackPanel Margin="0,12,0,0" Padding="24">
    <!-- Margin: Top Right Bottom Left -->
</StackPanel>
```

## Registration in DI

Add Views to `App.xaml.cs`:

```csharp
services.AddTransient<MyPage>();
```

## File Locations

- XAML: `Views/[FeatureName]/[PageName].xaml`
- Code-behind: `Views/[FeatureName]/[PageName].xaml.cs`
- Example: `Views/Main/Main_ReceivingLabelPage.xaml`

## Common Patterns

### Form Grid Layout

```xml
<Grid ColumnSpacing="12" RowSpacing="12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <TextBox Grid.Column="0" Header="Field 1" Text="{x:Bind ViewModel.Field1, Mode=TwoWay}"/>
    <TextBox Grid.Column="1" Header="Field 2" Text="{x:Bind ViewModel.Field2, Mode=TwoWay}"/>
    <TextBox Grid.Column="2" Header="Field 3" Text="{x:Bind ViewModel.Field3, Mode=TwoWay}"/>
</Grid>
```

### Header Section

```xml
<StackPanel Orientation="Horizontal" Spacing="16" Margin="0,0,0,24">
    <TextBlock Text="Page Title" Style="{StaticResource TitleTextBlockStyle}"/>
    <TextBlock Text="{x:Bind ViewModel.Subtitle, Mode=OneWay}" 
               Style="{StaticResource BodyTextBlockStyle}" 
               VerticalAlignment="Center"/>
</StackPanel>
```

## Things to Avoid

❌ Don't put business logic in code-behind
❌ Don't use `Binding` when you can use `x:Bind`
❌ Don't hard-code values that should be in ViewModel
❌ Don't manipulate ViewModel data in code-behind
❌ Don't use events when Commands are available
❌ Don't forget Mode=TwoWay for input controls

## Accessibility

Always include:

```xml
<TextBox 
    Header="Part ID"
    AutomationProperties.Name="Part ID Input"/>
```

## Full Page Example

```xml
<Page
    x:Class="MTM_Receiving_Application.Views.Receiving.ReceivingLabelPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.ViewModels.Receiving"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0" 
                   Text="Receiving Label Entry" 
                   Style="{StaticResource TitleTextBlockStyle}"
                   Margin="0,0,0,24"/>

        <!-- Input Form -->
        <Grid Grid.Row="1" ColumnSpacing="12" RowSpacing="12">
            <TextBox Header="Part ID" 
                     Text="{x:Bind ViewModel.PartID, Mode=TwoWay}"/>
        </Grid>

        <!-- Data Grid -->
        <ListView Grid.Row="2" 
                  ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"/>

        <!-- Actions -->
        <CommandBar Grid.Row="3">
            <AppBarButton Icon="Save" 
                          Label="Save" 
                          Command="{x:Bind ViewModel.SaveCommand}"/>
        </CommandBar>
    </Grid>
</Page>
```
