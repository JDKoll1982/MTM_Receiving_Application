# User Provided Header
MTM Receiving Application - Module_Dunnage Code-Only Export

# Files

## File: Module_Dunnage/Enums/Enum_DunnageWorkflowStep.cs
```csharp

```

## File: Module_Dunnage/Models/Model_DunnageLine.cs
```csharp
public class Model_DunnageLine
```

## File: Module_Dunnage/Models/Model_DunnagePart.cs
```csharp
public class Model_DunnagePart : INotifyPropertyChanged
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
⋮----
private void DeserializeSpecValues()
⋮----
if (string.IsNullOrWhiteSpace(SpecValues))
```

## File: Module_Dunnage/Models/Model_DunnageSession.cs
```csharp
public class Model_DunnageSession : INotifyPropertyChanged
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
```

## File: Module_Dunnage/Models/Model_DunnageSpec.cs
```csharp
public class Model_DunnageSpec : INotifyPropertyChanged
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
⋮----
private void DeserializeSpecs()
⋮----
if (string.IsNullOrWhiteSpace(SpecValue))
```

## File: Module_Dunnage/Models/Model_DunnageType.cs
```csharp
public partial class Model_DunnageType : ObservableObject
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
if (!string.IsNullOrEmpty(Icon) && Enum.TryParse<MaterialIconKind>(Icon, true, out var kind))
```

## File: Module_Dunnage/Models/Model_IconDefinition.cs
```csharp
public partial class Model_IconDefinition : ObservableObject
⋮----
private MaterialIconKind _kind;
⋮----
get => Kind.ToString();
⋮----
else if (LegacyMapping.TryGetValue(value, out var legacyResult))
⋮----
Name = legacyResult.ToString();
```

## File: Module_Dunnage/Models/Model_InventoriedDunnage.cs
```csharp
public class Model_InventoriedDunnage : INotifyPropertyChanged
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
⋮----
protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
⋮----
if (EqualityComparer<T>.Default.Equals(field, value))
```

## File: Module_Dunnage/Models/Model_SpecInput.cs
```csharp
public partial class Model_SpecInput : ObservableObject
```

## File: Module_Dunnage/Models/Model_SpecItem.cs
```csharp
public class Model_SpecItem
⋮----
parts.Add($"Min: {MinValue.Value}");
⋮----
parts.Add($"Max: {MaxValue.Value}");
⋮----
if (!string.IsNullOrEmpty(Unit))
⋮----
parts.Add(Unit);
⋮----
desc += $" ({string.Join(", ", parts)})";
```

## File: Module_Dunnage/Models/SpecDefinition.cs
```csharp
public class SpecDefinition
```

## File: Module_Dunnage/Views/View_Dunnage_AdminInventoryView.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminInventoryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.Resources>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_StringFormat x:Key="DateTimeToStringConverter"/>
    </Page.Resources>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock 
                Text="Manage Inventoried Parts" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Configure parts that require inventory tracking" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="0,8,0,0"/>
        </StackPanel>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,16">
            <AppBarButton 
                Icon="Add" 
                Label="Add Part to List"
                Command="{x:Bind ViewModel.ShowAddToListCommand}"
                ToolTipService.ToolTip="Add a part to the inventory tracking list"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Entry"
                Command="{x:Bind ViewModel.ShowEditEntryCommand}"
                IsEnabled="{x:Bind ViewModel.HasSelection, Mode=OneWay}"
                ToolTipService.ToolTip="Edit the selected inventory entry"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Remove from List"
                Command="{x:Bind ViewModel.ShowRemoveConfirmationCommand}"
                IsEnabled="{x:Bind ViewModel.HasSelection, Mode=OneWay}"
                ToolTipService.ToolTip="Remove the selected part from inventory tracking"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Back" 
                Label="Back to Admin Hub"
                Command="{x:Bind ViewModel.BackToHubCommand}"
                ToolTipService.ToolTip="Return to the admin hub"/>
        </CommandBar>

        <!-- DataGrid -->
        <Border 
            Grid.Row="2" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="1">
            <Grid>
                <controls:DataGrid
                    ItemsSource="{x:Bind ViewModel.InventoriedParts, Mode=OneWay}"
                    SelectedItem="{x:Bind ViewModel.SelectedInventoriedPart, Mode=TwoWay}"
                    AutoGenerateColumns="False"
                    IsReadOnly="True"
                    GridLinesVisibility="Horizontal"
                    HeadersVisibility="Column"
                    SelectionMode="Single"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="True"
                    VerticalScrollBarVisibility="Auto"
                    HorizontalScrollBarVisibility="Auto"
                    AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">

                    <controls:DataGrid.Columns>
                        <!-- Part ID Column -->
                        <controls:DataGridTextColumn 
                            Header="Part ID" 
                            Binding="{Binding PartId}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Inventory Method Column -->
                        <controls:DataGridTextColumn 
                            Header="Inventory Method" 
                            Binding="{Binding InventoryMethod}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Notes Column -->
                        <controls:DataGridTextColumn 
                            Header="Notes" 
                            Binding="{Binding Notes}"
                            Width="3*"
                            CanUserResize="True"/>

                        <!-- Date Added Column -->
                        <controls:DataGridTextColumn 
                            Header="Date Added" 
                            Binding="{Binding CreatedDate, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Added By Column -->
                        <controls:DataGridTextColumn 
                            Header="Added By" 
                            Binding="{Binding CreatedBy}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Last Modified Column -->
                        <controls:DataGridTextColumn 
                            Header="Last Modified" 
                            Binding="{Binding ModifiedDate, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Modified By Column -->
                        <controls:DataGridTextColumn 
                            Header="Modified By" 
                            Binding="{Binding ModifiedBy}"
                            Width="*"
                            CanUserResize="True"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>

                <!-- Empty State -->
                <StackPanel 
                    VerticalAlignment="Center" 
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Visibility="{x:Bind ViewModel.InventoriedParts.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}">
                    <FontIcon 
                        Glyph="&#xE7C3;" 
                        FontSize="72"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="No inventoried parts found" 
                        Style="{StaticResource SubtitleTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="Click 'Add Part to List' to configure parts that require inventory tracking"
                        Style="{StaticResource CaptionTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="8" Margin="0,16,0,0">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
            <TextBlock 
                Text="|"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="8,0,8,0"/>
            <TextBlock>
                <Run Text="Total Parts:"/>
                <Run Text="{x:Bind ViewModel.InventoriedParts.Count, Mode=OneWay}" FontWeight="SemiBold"/>
            </TextBlock>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_AdminMainView.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminMainView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:local="using:MTM_Receiving_Application.Module_Dunnage.Views"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Grid Padding="24">
        <!-- Main Navigation Hub (4-card view) -->
        <Grid Visibility="{x:Bind ViewModel.IsMainNavigationVisible, Mode=OneWay}">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Header -->
            <StackPanel Grid.Row="0" Margin="0,0,0,24">
                <TextBlock 
                    Text="Dunnage Management" 
                    Style="{StaticResource TitleTextBlockStyle}"/>
                <TextBlock 
                    Text="Manage types, parts, specifications, and inventory" 
                    Style="{StaticResource BodyTextBlockStyle}"
                    Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                    Margin="0,8,0,0"/>
            </StackPanel>

            <!-- 4 Navigation Cards (2x2 Grid) -->
            <Grid Grid.Row="1" VerticalAlignment="Center">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

                <!-- Type Management Card -->
                <Button 
                    Grid.Row="0" 
                    Grid.Column="0"
                    Command="{x:Bind ViewModel.NavigateToManageTypesCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="0,0,12,12"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE7C3;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Manage Types" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Add, edit, and delete dunnage types"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>

                <!-- Spec Management Card -->
                <Button 
                    Grid.Row="0" 
                    Grid.Column="1"
                    Command="{x:Bind ViewModel.NavigateToManageSpecsCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="12,0,0,12"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE8FD;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Manage Specs" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Define specifications for each type"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>

                <!-- Part Management Card -->
                <Button 
                    Grid.Row="1" 
                    Grid.Column="0"
                    Command="{x:Bind ViewModel.NavigateToManagePartsCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="0,12,12,0"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE8F1;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Manage Parts" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Add and edit dunnage part numbers"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>

                <!-- Inventoried List Card -->
                <Button 
                    Grid.Row="1" 
                    Grid.Column="1"
                    Command="{x:Bind ViewModel.NavigateToInventoriedListCommand}"
                    HorizontalAlignment="Stretch"
                    VerticalAlignment="Stretch"
                    Margin="12,12,0,0"
                    Padding="24"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8">
                    <StackPanel Spacing="12">
                        <FontIcon 
                            Glyph="&#xE8EF;" 
                            FontSize="48"
                            Foreground="{ThemeResource SystemAccentColor}"/>
                        <TextBlock 
                            Text="Inventoried List" 
                            Style="{StaticResource SubtitleTextBlockStyle}"
                            HorizontalAlignment="Center"/>
                        <TextBlock 
                            Text="Manage inventory tracking settings"
                            Style="{StaticResource CaptionTextBlockStyle}"
                            Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                            TextWrapping="Wrap"
                            HorizontalAlignment="Center"
                            TextAlignment="Center"/>
                    </StackPanel>
                </Button>
            </Grid>

            <!-- Status Bar -->
            <StackPanel Grid.Row="2" Orientation="Horizontal" Spacing="8" Margin="0,24,0,0">
                <ProgressRing 
                    IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                    Width="20" 
                    Height="20"/>
                <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
            </StackPanel>
        </Grid>

        <!-- Type Management Section -->
        <Grid Visibility="{x:Bind ViewModel.IsManageTypesVisible, Mode=OneWay}">
            <local:View_Dunnage_AdminTypesView/>
        </Grid>

        <!-- Spec Management Section (placeholder) -->
        <Grid Visibility="{x:Bind ViewModel.IsManageSpecsVisible, Mode=OneWay}">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="16">
                <FontIcon Glyph="&#xE8FD;" FontSize="72" Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                <TextBlock 
                    Text="Spec Management" 
                    Style="{StaticResource TitleTextBlockStyle}"
                    HorizontalAlignment="Center"/>
                <TextBlock 
                    Text="This view will be implemented in a future phase"
                    Style="{StaticResource BodyTextBlockStyle}"
                    Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                    HorizontalAlignment="Center"/>
                <Button 
                    Content="Back to Main Menu"
                    Command="{x:Bind ViewModel.ReturnToMainNavigationCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    HorizontalAlignment="Center"
                    Margin="0,16,0,0"/>
            </StackPanel>
        </Grid>

        <!-- Part Management Section -->
        <Grid Visibility="{x:Bind ViewModel.IsManagePartsVisible, Mode=OneWay}">
            <local:View_Dunnage_AdminPartsView/>
        </Grid>

        <!-- Inventoried List Section (placeholder) -->
        <Grid Visibility="{x:Bind ViewModel.IsInventoriedListVisible, Mode=OneWay}">
            <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="16">
                <FontIcon Glyph="&#xE8EF;" FontSize="72" Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                <TextBlock 
                    Text="Inventoried List" 
                    Style="{StaticResource TitleTextBlockStyle}"
                    HorizontalAlignment="Center"/>
                <TextBlock 
                    Text="This view will be implemented in a future phase"
                    Style="{StaticResource BodyTextBlockStyle}"
                    Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                    HorizontalAlignment="Center"/>
                <Button 
                    Content="Back to Main Menu"
                    Command="{x:Bind ViewModel.ReturnToMainNavigationCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    HorizontalAlignment="Center"
                    Margin="0,16,0,0"/>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_AdminMainView.xaml.cs
```csharp
public sealed partial class View_Dunnage_AdminMainView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
```

## File: Module_Dunnage/Views/View_Dunnage_AdminPartsView.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminPartsView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"
    Loaded="OnPageLoaded">

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock 
                Text="Manage Dunnage Parts" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Add, edit, and search dunnage parts with spec values" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="0,8,0,0"/>
        </StackPanel>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,16">
            <AppBarButton 
                Icon="Add" 
                Label="Add New Part"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.CreatePart'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Part"
                IsEnabled="{x:Bind ViewModel.CanEdit, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.EditPart'), Mode=OneWay}"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Delete Part"
                Command="{x:Bind ViewModel.ShowDeleteConfirmationCommand}"
                IsEnabled="{x:Bind ViewModel.CanDelete, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.DeletePart'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Back" 
                Label="Back to Admin Hub"
                Command="{x:Bind ViewModel.ReturnToAdminHubCommand}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ReturnToAdmin'), Mode=OneWay}"/>
        </CommandBar>

        <!-- Search and Filter Bar -->
        <Grid Grid.Row="2" Margin="0,0,0,16">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Search Box -->
            <TextBox 
                Grid.Column="0"
                Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}"
                PlaceholderText="Search by Part ID..."
                Margin="0,0,12,0">
                <TextBox.KeyboardAccelerators>
                    <KeyboardAccelerator Key="Enter" Invoked="OnSearchKeyboardAccelerator"/>
                </TextBox.KeyboardAccelerators>
            </TextBox>

            <!-- Type Filter -->
            <ComboBox 
                Grid.Column="1"
                ItemsSource="{x:Bind ViewModel.AvailableTypes, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedFilterType, Mode=TwoWay}"
                PlaceholderText="Filter by Type"
                DisplayMemberPath="DunnageType"
                MinWidth="200"
                Margin="0,0,12,0"/>

            <!-- Filter Buttons -->
            <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="8">
                <Button 
                    Content="Search"
                    Command="{x:Bind ViewModel.SearchPartsCommand}"
                    Style="{StaticResource AccentButtonStyle}"/>
                <Button 
                    Content="Filter by Type"
                    Command="{x:Bind ViewModel.FilterByTypeCommand}"/>
                <Button 
                    Content="Clear"
                    Command="{x:Bind ViewModel.ClearFiltersCommand}"/>
            </StackPanel>
        </Grid>

        <!-- DataGrid -->
        <Border 
            Grid.Row="3" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="1">
            <Grid>
                <controls:DataGrid
                    ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                    SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                    AutoGenerateColumns="False"
                    IsReadOnly="True"
                    GridLinesVisibility="Horizontal"
                    HeadersVisibility="Column"
                    SelectionMode="Single"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="True"
                    VerticalScrollBarVisibility="Auto"
                    HorizontalScrollBarVisibility="Auto"
                    AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">

                    <controls:DataGrid.Columns>
                        <!-- Part ID Column -->
                        <controls:DataGridTextColumn 
                            Header="Part ID" 
                            Binding="{Binding PartId}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Type Column -->
                        <controls:DataGridTextColumn 
                            Header="Type" 
                            Binding="{Binding DunnageTypeName}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Spec Values Column (Note: Dynamic spec columns would require code-behind) -->
                        <controls:DataGridTextColumn 
                            Header="Specifications" 
                            Binding="{Binding DunnageSpecValuesJson}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Date Added Column -->
                        <controls:DataGridTextColumn 
                            Header="Date Added" 
                            Binding="{Binding CreatedDate, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Added By Column -->
                        <controls:DataGridTextColumn 
                            Header="Added By" 
                            Binding="{Binding CreatedBy}"
                            Width="*"
                            CanUserResize="True"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>

                <!-- Empty State -->
                <StackPanel 
                    VerticalAlignment="Center" 
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Visibility="{x:Bind ViewModel.Parts.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}">
                    <FontIcon 
                        Glyph="&#xE8F1;" 
                        FontSize="72"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="No parts found" 
                        Style="{StaticResource SubtitleTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="Try adjusting your filters or add a new part"
                        Style="{StaticResource CaptionTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Pagination Controls -->
        <Grid Grid.Row="4" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Record Count -->
            <TextBlock Grid.Column="0" VerticalAlignment="Center">
                <Run Text="Showing"/>
                <Run Text="{x:Bind ViewModel.Parts.Count, Mode=OneWay}" FontWeight="SemiBold"/>
                <Run Text="of"/>
                <Run Text="{x:Bind ViewModel.TotalRecords, Mode=OneWay}" FontWeight="SemiBold"/>
                <Run Text="parts"/>
            </TextBlock>

            <!-- Pagination Buttons -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
                <Button 
                    Content="First"
                    Command="{x:Bind ViewModel.FirstPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigatePrevious, Mode=OneWay}"
                    MinWidth="80"/>
                <Button 
                    Content="Previous"
                    Command="{x:Bind ViewModel.PreviousPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigatePrevious, Mode=OneWay}"
                    MinWidth="80"/>
                <TextBlock 
                    VerticalAlignment="Center"
                    Margin="12,0,12,0">
                    <Run Text="Page"/>
                    <Run Text="{x:Bind ViewModel.CurrentPage, Mode=OneWay}" FontWeight="SemiBold"/>
                    <Run Text="of"/>
                    <Run Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}" FontWeight="SemiBold"/>
                </TextBlock>
                <Button 
                    Content="Next"
                    Command="{x:Bind ViewModel.NextPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigateNext, Mode=OneWay}"
                    MinWidth="80"/>
                <Button 
                    Content="Last"
                    Command="{x:Bind ViewModel.LastPageCommand}"
                    IsEnabled="{x:Bind ViewModel.CanNavigateNext, Mode=OneWay}"
                    MinWidth="80"/>
            </StackPanel>
        </Grid>

        <!-- Status Bar -->
        <StackPanel Grid.Row="5" Orientation="Horizontal" Spacing="8" Margin="0,16,0,0">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_AdminPartsView.xaml.cs
```csharp
public sealed partial class View_Dunnage_AdminPartsView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.LoadPartsCommand.ExecuteAsync(null);
⋮----
private async void OnSearchKeyboardAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
⋮----
await ViewModel.SearchPartsCommand.ExecuteAsync(null);
```

## File: Module_Dunnage/Views/View_Dunnage_AdminTypesView.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_AdminTypesView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    mc:Ignorable="d"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}"
    Loaded="OnPageLoaded">

    <Page.Resources>
        <converters:Converter_IconCodeToGlyph x:Key="IconCodeToGlyphConverter"/>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_StringFormat x:Key="DateTimeToStringConverter"/>
    </Page.Resources>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,16">
            <TextBlock 
                Text="Manage Dunnage Types" 
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="Add, edit, and delete dunnage types with impact analysis" 
                Style="{StaticResource BodyTextBlockStyle}"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="0,8,0,0"/>
        </StackPanel>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,0,0,16">
            <AppBarButton 
                Icon="Add" 
                Label="Add New Type"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.CreateType'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Type"
                Command="{x:Bind ViewModel.ShowEditTypeCommand}"
                IsEnabled="{x:Bind ViewModel.CanEdit, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.EditType'), Mode=OneWay}"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Delete Type"
                Command="{x:Bind ViewModel.ShowDeleteConfirmationCommand}"
                IsEnabled="{x:Bind ViewModel.CanDelete, Mode=OneWay}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.DeleteType'), Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Back" 
                Label="Back to Admin Hub"
                Command="{x:Bind ViewModel.ReturnToAdminHubCommand}"
                ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ReturnToAdmin'), Mode=OneWay}"/>
        </CommandBar>

        <!-- DataGrid -->
        <Border 
            Grid.Row="2" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            CornerRadius="8"
            Padding="1">
            <Grid>
                <controls:DataGrid
                    ItemsSource="{x:Bind ViewModel.Types, Mode=OneWay}"
                    SelectedItem="{x:Bind ViewModel.SelectedType, Mode=TwoWay}"
                    AutoGenerateColumns="False"
                    IsReadOnly="True"
                    GridLinesVisibility="Horizontal"
                    HeadersVisibility="Column"
                    SelectionMode="Single"
                    CanUserReorderColumns="False"
                    CanUserSortColumns="True"
                    VerticalScrollBarVisibility="Auto"
                    HorizontalScrollBarVisibility="Auto"
                    AlternatingRowBackground="{ThemeResource SubtleFillColorSecondaryBrush}">

                    <controls:DataGrid.Columns>
                        <!-- Icon Column -->
                        <controls:DataGridTemplateColumn Header="Icon" Width="60">
                            <controls:DataGridTemplateColumn.CellTemplate>
                                <DataTemplate>
                                    <materialIcons:MaterialIcon 
                                        Kind="{Binding IconKind}" 
                                        Width="24" 
                                        Height="24"
                                        Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                                        HorizontalAlignment="Center"
                                        VerticalAlignment="Center"/>
                                </DataTemplate>
                            </controls:DataGridTemplateColumn.CellTemplate>
                        </controls:DataGridTemplateColumn>

                        <!-- Type Name Column -->
                        <controls:DataGridTextColumn 
                            Header="Type Name" 
                            Binding="{Binding DunnageType}"
                            Width="2*"
                            CanUserResize="True"/>

                        <!-- Date Added Column -->
                        <controls:DataGridTextColumn 
                            Header="Date Added" 
                            Binding="{Binding DateAdded, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Added By Column -->
                        <controls:DataGridTextColumn 
                            Header="Added By" 
                            Binding="{Binding AddedBy}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Last Modified Column -->
                        <controls:DataGridTextColumn 
                            Header="Last Modified" 
                            Binding="{Binding LastModified, Converter={StaticResource DateTimeToStringConverter}}"
                            Width="*"
                            CanUserResize="True"/>

                        <!-- Modified By Column -->
                        <controls:DataGridTextColumn 
                            Header="Modified By" 
                            Binding="{Binding ModifiedBy}"
                            Width="*"
                            CanUserResize="True"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>

                <!-- Empty State -->
                <StackPanel 
                    VerticalAlignment="Center" 
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Visibility="{x:Bind ViewModel.Types.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter=Inverse}">
                    <FontIcon 
                        Glyph="&#xE7C3;" 
                        FontSize="72"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="No types found" 
                        Style="{StaticResource SubtitleTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                    <TextBlock 
                        Text="Click 'Add New Type' to create your first dunnage type"
                        Style="{StaticResource CaptionTextBlockStyle}"
                        Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"/>
                </StackPanel>
            </Grid>
        </Border>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="8" Margin="0,16,0,0">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"/>
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
            <TextBlock 
                Text="|"
                Foreground="{ThemeResource SystemControlForegroundBaseMediumBrush}"
                Margin="8,0,8,0"/>
            <TextBlock>
                <Run Text="Total Types:"/>
                <Run Text="{x:Bind ViewModel.Types.Count, Mode=OneWay}" FontWeight="SemiBold"/>
            </TextBlock>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Dunnage/Views/View_Dunnage_Control_IconPickerControl.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Control_IconPickerControl"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    xmlns:icons="using:Material.Icons"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter" />
        
        <Style x:Key="IconGridViewItemStyle" TargetType="GridViewItem">
            <Setter Property="HorizontalContentAlignment" Value="Stretch" />
            <Setter Property="VerticalContentAlignment" Value="Stretch" />
            <Setter Property="Margin" Value="2" />
            <Setter Property="Padding" Value="0" />
            <Setter Property="CornerRadius" Value="4" />
            <Setter Property="MinWidth" Value="40" />
            <Setter Property="MinHeight" Value="40" />
        </Style>

        <DataTemplate x:Key="IconTemplate" x:DataType="icons:MaterialIconKind">
            <Border Width="40" Height="40" 
                    BorderThickness="1" 
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    CornerRadius="4" 
                    Padding="4" 
                    ToolTipService.ToolTip="{x:Bind ToString()}">
                <materialIcons:MaterialIcon Kind="{x:Bind}" Width="24" Height="24" Foreground="{ThemeResource AccentFillColorDefaultBrush}" />
            </Border>
        </DataTemplate>
        
        <DataTemplate x:Key="RecentIconTemplate" x:DataType="models:Model_IconDefinition">
             <Border Width="40" Height="40" 
                    BorderThickness="1" 
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    CornerRadius="4" 
                    Padding="4" 
                    ToolTipService.ToolTip="{x:Bind Name}">
                <materialIcons:MaterialIcon Kind="{x:Bind Kind}" Width="24" Height="24" Foreground="{ThemeResource AccentFillColorDefaultBrush}" />
            </Border>
        </DataTemplate>
    </UserControl.Resources>

    <StackPanel Spacing="12">

        <!-- Recently Used Section -->
        <StackPanel Spacing="8" Visibility="{x:Bind RecentlyUsedIcons.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}}">
            <TextBlock Text="Recently Used" FontWeight="SemiBold" FontSize="12" />
            <GridView
                x:Name="RecentIconsGrid"
                SelectionMode="Single"
                MaxHeight="60"
                ItemsSource="{x:Bind RecentlyUsedIcons, Mode=OneWay}"
                ItemTemplate="{StaticResource RecentIconTemplate}"
                ItemContainerStyle="{StaticResource IconGridViewItemStyle}"
                SelectionChanged="OnRecentIconSelectionChanged">
                <GridView.ItemsPanel>
                    <ItemsPanelTemplate>
                        <ItemsWrapGrid Orientation="Horizontal" MaximumRowsOrColumns="6" />
                    </ItemsPanelTemplate>
                </GridView.ItemsPanel>
            </GridView>
        </StackPanel>

        <!-- Search Box -->
        <TextBox
            x:Name="SearchBox"
            PlaceholderText="Search icons..."
            TextChanged="OnSearchTextChanged" />

        <!-- All Icons Grid -->
        <GridView
            x:Name="AllIconsGrid"
            SelectionMode="Single"
            SelectedItem="{x:Bind SelectedIcon, Mode=TwoWay}"
            ItemTemplate="{StaticResource IconTemplate}"
            ItemContainerStyle="{StaticResource IconGridViewItemStyle}"
            MaxHeight="300">
            <GridView.ItemsPanel>
                <ItemsPanelTemplate>
                    <ItemsWrapGrid Orientation="Horizontal" MaximumRowsOrColumns="6" />
                </ItemsPanelTemplate>
            </GridView.ItemsPanel>
        </GridView>
    </StackPanel>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_DetailsEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_DetailsEntryView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,16">
            <TextBlock Text="Enter Details"
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.DetailsEntry'), Mode=OneWay}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- Inventory Notification -->
        <InfoBar Grid.Row="1"
                 IsOpen="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay}"
                 Severity="Informational"
                 Title="Inventory Action Required"
                 Message="{x:Bind ViewModel.InventoryNotificationMessage, Mode=OneWay}"
                 IsClosable="False"
                 Margin="0,0,0,12"/>

        <!-- Details Form -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <StackPanel Spacing="12">
                
                <!-- PO Number and Location Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <Grid ColumnSpacing="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <!-- PO Number -->
                        <StackPanel Grid.Column="0" Spacing="6">
                            <TextBlock Text="PO Number"
                                       Style="{StaticResource SubtitleTextBlockStyle}"/>
                            <TextBox x:Name="PoNumberTextBox"
                                     Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay}"
                                     PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PONumber'), Mode=OneWay}"
                                     HorizontalAlignment="Stretch"/>
                            <TextBlock Text="Leave blank for non-PO dunnage"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                        </StackPanel>

                        <!-- Location -->
                        <StackPanel Grid.Column="1" Spacing="6">
                            <TextBlock Text="Location"
                                       Style="{StaticResource SubtitleTextBlockStyle}"/>
                            <TextBox Text="{x:Bind ViewModel.Location, Mode=TwoWay}"
                                     PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.Location'), Mode=OneWay}"
                                     HorizontalAlignment="Stretch"/>
                            <TextBlock Text="Where this dunnage is located"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Specifications Card (Read-Only) -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="8">
                        <TextBlock Text="Specifications (from selected part)"
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>
                        
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <!-- Column 1: Text Specs -->
                            <StackPanel Grid.Column="0" Spacing="4" 
                                        Visibility="{x:Bind ViewModel.HasTextSpecs, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                                <ItemsRepeater ItemsSource="{x:Bind ViewModel.TextSpecs, Mode=OneWay}">
                                    <ItemsRepeater.ItemTemplate>
                                        <DataTemplate>
                                            <StackPanel Spacing="2" Margin="0,0,0,6">
                                                <TextBlock Text="{Binding SpecName}" FontWeight="SemiBold" FontSize="12"/>
                                                <TextBox Text="{Binding Value, Mode=OneWay}" 
                                                         IsReadOnly="True"
                                                         Width="180"/>
                                            </StackPanel>
                                        </DataTemplate>
                                    </ItemsRepeater.ItemTemplate>
                                </ItemsRepeater>
                            </StackPanel>
                            
                            <!-- Column 2: Number Specs -->
                            <StackPanel Grid.Column="1" Spacing="4"
                                        Visibility="{x:Bind ViewModel.HasNumberSpecs, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                                <ItemsRepeater ItemsSource="{x:Bind ViewModel.NumberSpecs, Mode=OneWay}">
                                    <ItemsRepeater.ItemTemplate>
                                        <DataTemplate>
                                            <StackPanel Spacing="2" Margin="0,0,0,6">
                                                <TextBlock Text="{Binding SpecName}" FontWeight="SemiBold" FontSize="12"/>
                                                <TextBox Text="{Binding Value, Mode=OneWay}" 
                                                         IsReadOnly="True"
                                                         Width="180"/>
                                            </StackPanel>
                                        </DataTemplate>
                                    </ItemsRepeater.ItemTemplate>
                                </ItemsRepeater>
                            </StackPanel>
                            
                            <!-- Column 3: Boolean Specs -->
                            <StackPanel Grid.Column="2" Spacing="4"
                                        Visibility="{x:Bind ViewModel.HasBooleanSpecs, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                                <ItemsRepeater ItemsSource="{x:Bind ViewModel.BooleanSpecs, Mode=OneWay}">
                                    <ItemsRepeater.ItemTemplate>
                                        <DataTemplate>
                                            <CheckBox Content="{Binding SpecName}" 
                                                      IsChecked="{Binding Value, Mode=OneWay}" 
                                                      IsEnabled="False"
                                                      Margin="0,0,0,6"/>
                                        </DataTemplate>
                                    </ItemsRepeater.ItemTemplate>
                                </ItemsRepeater>
                            </StackPanel>
                        </Grid>

                        <!-- Placeholder when no specs -->
                        <TextBlock Text="No specifications configured for this type."
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                   Visibility="{x:Bind ViewModel.SpecInputs.Count, Mode=OneWay, Converter={StaticResource IntToVisibilityConverter}, ConverterParameter='Inverse'}"
                                   Style="{StaticResource BodyTextBlockStyle}"/>
                    </StackPanel>
                </Border>

                <!-- Info Panel Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="6">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE946;" FontSize="16" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Note" FontWeight="SemiBold"/>
                        </StackPanel>
                        <TextBlock TextWrapping="Wrap" FontSize="12"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                            <Run Text="• PO Number: Use if receiving against a purchase order"/>
                            <LineBreak/>
                            <Run Text="• Location: Physical location of the dunnage"/>
                            <LineBreak/>
                            <Run Text="• Specifications: Pre-filled from the selected part"/>
                        </TextBlock>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_DetailsEntryView.xaml.cs
```csharp
public sealed partial class View_Dunnage_DetailsEntryView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.LoadSpecsForSelectedPartAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddMultipleRowsDialog.xaml
```
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Dialog_AddMultipleRowsDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="Add Multiple Rows"
    PrimaryButtonText="Add"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="ContentDialog_PrimaryButtonClick">

    <StackPanel Spacing="16" Padding="24">
        <TextBlock 
            Text="How many rows would you like to add?" 
            Style="{StaticResource BodyTextBlockStyle}"/>
        
        <NumberBox 
            x:Name="RowCountNumberBox"
            Header="Number of Rows"
            Minimum="1"
            Maximum="100"
            Value="10"
            SpinButtonPlacementMode="Inline"
            ValidationMode="InvalidInputOverwritten"
            AcceptsExpression="False"/>
        
        <InfoBar 
            IsOpen="True"
            Severity="Informational"
            IsClosable="False"
            Message="Maximum 100 rows can be added at once. Each row will be initialized with default values (Quantity = 1)."/>
    </StackPanel>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddMultipleRowsDialog.xaml.cs
```csharp
public sealed partial class View_Dunnage_Dialog_AddMultipleRowsDialog : ContentDialog
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, RowCountNumberBox);
⋮----
private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddToInventoriedListDialog.xaml
```
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Dialog_AddToInventoriedListDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="Add Part to Inventoried List"
    PrimaryButtonText="Add to List"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnPrimaryButtonClick">

    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="12" Padding="24">
            
            <!-- Part ID Selection -->
            <TextBlock Text="Part ID" FontWeight="SemiBold"/>
            <ComboBox x:Name="PartIdComboBox"
                      PlaceholderText="Select a part..."
                      HorizontalAlignment="Stretch"
                      IsEditable="True"
                      TextSubmitted="PartIdComboBox_TextSubmitted"/>
            <TextBlock x:Name="PartIdError"
                       Text="Part ID is required"
                       Foreground="{ThemeResource SystemErrorTextColor}"
                       Visibility="Collapsed"
                       Margin="0,-8,0,0"/>

            <!-- Inventory Method -->
            <TextBlock Text="Inventory Method" FontWeight="SemiBold" Margin="0,12,0,0"/>
            <ComboBox x:Name="InventoryMethodComboBox"
                      HorizontalAlignment="Stretch"
                      SelectedIndex="2">
                <ComboBoxItem Content="Adjust In"/>
                <ComboBoxItem Content="Receive In"/>
                <ComboBoxItem Content="Both"/>
            </ComboBox>
            <TextBlock Text="Adjust In: Only triggers during inventory adjustments"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource SystemControlDisabledBaseMediumLowBrush}"
                       TextWrapping="Wrap"
                       Margin="0,-8,0,0"/>
            <TextBlock Text="Receive In: Only triggers during receiving transactions"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource SystemControlDisabledBaseMediumLowBrush}"
                       TextWrapping="Wrap"/>
            <TextBlock Text="Both: Triggers for both adjust and receive operations"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource SystemControlDisabledBaseMediumLowBrush}"
                       TextWrapping="Wrap"/>

            <!-- Notes -->
            <TextBlock Text="Notes (optional)" FontWeight="SemiBold" Margin="0,12,0,0"/>
            <TextBox x:Name="NotesTextBox"
                     PlaceholderText="Add any notes or special instructions..."
                     AcceptsReturn="True"
                     TextWrapping="Wrap"
                     Height="80"/>

        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_AddToInventoriedListDialog.xaml.cs
```csharp
public sealed partial class View_Dunnage_Dialog_AddToInventoriedListDialog : ContentDialog
⋮----
private readonly Dao_DunnagePart _daoPart;
private readonly Dao_InventoriedDunnage _daoInventory;
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async Task LoadPartsAsync()
⋮----
var result = await _daoPart.GetAllAsync();
⋮----
PartIdComboBox.ItemsSource = result.Data.ConvertAll(p => p.PartId);
⋮----
private void PartIdComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
⋮----
if (!string.IsNullOrWhiteSpace(args.Text))
⋮----
private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
var deferral = args.GetDeferral();
⋮----
if (string.IsNullOrWhiteSpace(partId))
⋮----
var checkResult = await _daoInventory.GetByPartAsync(partId);
⋮----
var errorDialog = new ContentDialog
⋮----
await errorDialog.ShowAsync();
⋮----
var insertResult = await _daoInventory.InsertAsync(
⋮----
deferral.Complete();
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_Dunnage_AddTypeDialog.xaml
```
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_Dialog_Dunnage_AddTypeDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="using:MTM_Receiving_Application.Module_Dunnage.Views.Controls"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    x:Name="Root"
    Title="Add New Dunnage Type"
    PrimaryButtonText="Save Type"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    MaxHeight="750"
    PrimaryButtonCommand="{x:Bind ViewModel.SaveTypeCommand}"
    IsPrimaryButtonEnabled="{x:Bind ViewModel.CanSave, Mode=OneWay}">

    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Spacing="16" Padding="24">

            <!-- Duplicate Warning InfoBar -->
            <InfoBar
                Severity="Warning"
                IsOpen="{x:Bind ViewModel.ShowDuplicateWarning, Mode=OneWay}"
                Title="Duplicate Type Name"
                Message="A type with this name already exists.">
                <InfoBar.ActionButton>
                    <HyperlinkButton Content="View Existing Type" />
                </InfoBar.ActionButton>
            </InfoBar>

            <!-- Field Limit Warning InfoBar -->
            <InfoBar
                Severity="Informational"
                IsOpen="{x:Bind ViewModel.ShowFieldLimitWarning, Mode=OneWay}"
                Title="Many Custom Fields"
                Message="You have added 10+ custom fields. Consider grouping similar fields for better usability." />

            <!-- Basic Information Section -->
            <StackPanel Spacing="12">
                <TextBlock Text="Basic Information" Style="{StaticResource SubtitleTextBlockStyle}" />

                <!-- Type Name -->
                <TextBox
                    Header="Type Name"
                    PlaceholderText="Enter dunnage type name (e.g., Pallet - 48x40)"
                    Text="{x:Bind ViewModel.TypeName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                    BorderBrush="{x:Bind ViewModel.TypeNameError, Mode=OneWay, Converter={StaticResource ErrorToBrushConverter}}"
                    MaxLength="100" />

                <TextBlock
                    Text="{x:Bind ViewModel.TypeNameError, Mode=OneWay}"
                    Foreground="Red"
                    Visibility="{x:Bind ViewModel.TypeNameError, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}"
                    Margin="0,-8,0,0" />

                <!-- Icon Picker Button -->
                <TextBlock Text="Icon" Margin="0,8,0,0" FontWeight="SemiBold" />
                <Button x:Name="SelectIconButton"
                        Click="OnSelectIconClick"
                        HorizontalAlignment="Stretch"
                        Padding="16,12"
                        ToolTipService.ToolTip="Click to browse and select an icon from the icon library">
                    <StackPanel Orientation="Horizontal" Spacing="12">
                        <materialIcons:MaterialIcon 
                            Kind="{x:Bind ViewModel.SelectedIcon, Mode=OneWay}" 
                            Width="32" 
                            Height="32"
                            Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="Click to select icon" FontWeight="SemiBold"/>
                            <TextBlock 
                                Text="{x:Bind ViewModel.SelectedIcon, Mode=OneWay}"
                                FontSize="12"
                                Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </StackPanel>
                </Button>
            </StackPanel>

            <!-- Custom Specifications Section -->
            <StackPanel Spacing="12" Margin="0,16,0,0">
                <TextBlock Text="Custom Specifications" Style="{StaticResource SubtitleTextBlockStyle}" />

                <!-- Add New Field Form -->
                <Border
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="16"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
                    <StackPanel Spacing="12">
                        <TextBlock Text="Add Custom Field" FontWeight="SemiBold" />

                        <TextBox
                            Header="Field Name"
                            PlaceholderText="Enter field name (e.g., Width, Material)"
                            Text="{x:Bind ViewModel.FieldName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                            BorderBrush="{x:Bind ViewModel.FieldNameError, Mode=OneWay, Converter={StaticResource ErrorToBrushConverter}}"
                            MaxLength="100" />

                        <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,-8,0,0">
                            <TextBlock
                                Text="{x:Bind ViewModel.FieldNameError, Mode=OneWay}"
                                Foreground="Red"
                                Visibility="{x:Bind ViewModel.FieldNameError, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}" />
                            <TextBlock
                                Text="{x:Bind ViewModel.FieldCharacterCount, Mode=OneWay}"
                                Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                HorizontalAlignment="Right" />
                            <TextBlock Text="/100 characters" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                        </StackPanel>

                        <ComboBox
                            Header="Field Type"
                            SelectedItem="{x:Bind ViewModel.FieldType, Mode=TwoWay}"
                            HorizontalAlignment="Stretch">
                            <x:String>Text</x:String>
                            <x:String>Number</x:String>
                            <x:String>Dropdown</x:String>
                            <x:String>Yes/No</x:String>
                        </ComboBox>

                        <CheckBox
                            Content="Required field"
                            IsChecked="{x:Bind ViewModel.IsFieldRequired, Mode=TwoWay}" />

                        <Button
                            Content="{x:Bind ViewModel.EditingField, Mode=OneWay, Converter={StaticResource EditingFieldToTextConverter}}"
                            Command="{x:Bind ViewModel.AddFieldCommand}"
                            IsEnabled="{x:Bind ViewModel.CanAddField, Mode=OneWay}"
                            Style="{StaticResource AccentButtonStyle}"
                            HorizontalAlignment="Right" />
                    </StackPanel>
                </Border>

                <!-- Field Preview List -->
                <TextBlock
                    Text="Custom Fields Preview"
                    FontWeight="SemiBold"
                    Visibility="{x:Bind ViewModel.CustomFields.Count, Mode=OneWay, Converter={StaticResource CountToVisibilityConverter}}"
                    Margin="0,8,0,0" />

                <ItemsRepeater
                    ItemsSource="{x:Bind ViewModel.CustomFields, Mode=OneWay}">
                    <ItemsRepeater.ItemTemplate>
                        <DataTemplate x:DataType="models:Model_CustomFieldDefinition">
                            <Border
                                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                BorderThickness="1"
                                CornerRadius="8"
                                Padding="12"
                                Margin="0,4"
                                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
                                <Grid ColumnSpacing="8">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto" />
                                        <ColumnDefinition Width="*" />
                                        <ColumnDefinition Width="Auto" />
                                    </Grid.ColumnDefinitions>

                                    <!-- Drag Handle -->
                                    <FontIcon Grid.Column="0" Glyph="&#xE76F;" FontSize="16" VerticalAlignment="Center" />

                                    <!-- Field Info -->
                                    <StackPanel Grid.Column="1" Spacing="4">
                                        <TextBlock Text="{x:Bind FieldName}" FontWeight="SemiBold" />
                                        <TextBlock
                                            Text="{x:Bind GetSummary()}"
                                            Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                            FontSize="12" />
                                    </StackPanel>

                                    <!-- Edit/Delete Buttons -->
                                    <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4">
                                        <Button
                                            Content="Edit"
                                            Command="{Binding EditFieldCommand}"
                                            CommandParameter="{x:Bind}"
                                            Style="{StaticResource TextBlockButtonStyle}" />
                                        <Button
                                            Content="Delete"
                                            Command="{Binding DeleteFieldCommand}"
                                            CommandParameter="{x:Bind}"
                                            Style="{StaticResource TextBlockButtonStyle}"
                                            Foreground="Red" />
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsRepeater.ItemTemplate>
                </ItemsRepeater>

                <TextBlock
                    Text="No custom fields added yet. Click 'Add Field' to create one."
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Visibility="{x:Bind ViewModel.CustomFields.Count, Mode=OneWay, Converter={StaticResource InverseCountToVisibilityConverter}}"
                    HorizontalAlignment="Center"
                    Margin="0,16" />
            </StackPanel>

        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_Dialog_Dunnage_AddTypeDialog.xaml.cs
```csharp
public sealed partial class View_Dunnage_Dialog_Dunnage_AddTypeDialog : ContentDialog
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnSelectIconClick(object sender, RoutedEventArgs e)
⋮----
var iconSelector = new View_Shared_IconSelectorWindow();
iconSelector.SetInitialSelection(ViewModel.SelectedIcon);
iconSelector.Activate();
var selectedIcon = await iconSelector.WaitForSelectionAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_EditModeView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_EditModeView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    xmlns:enums="using:MTM_Receiving_Application.Module_Core.Models.Enums"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="Root">

    <UserControl.Resources>
        <converters:Converter_DecimalToString x:Key="DecimalToStringConverter"/>
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar - T154: 3 data source buttons -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <!-- Main Toolbar Area -->
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <!-- Load Data Section - T154 -->
                <TextBlock Text="Load Data From:" VerticalAlignment="Center" FontWeight="SemiBold" Margin="0,0,4,0"/>
                
                <Button Command="{x:Bind ViewModel.LoadFromCurrentMemoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadSessionMemory'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE81D;"/>
                        <TextBlock Text="Current Memory"/>
                    </StackPanel>
                </Button>

                <Button Command="{x:Bind ViewModel.LoadFromCurrentLabelsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadRecentCSV'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8E5;"/>
                        <TextBlock Text="Current Labels"/>
                    </StackPanel>
                </Button>
                
                <Button Command="{x:Bind ViewModel.LoadFromHistoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadHistoricalData'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74C;"/>
                        <TextBlock Text="History"/>
                    </StackPanel>
                </Button>

                <!-- Separator -->
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>

                <!-- Edit Actions Section -->
                <Button Command="{x:Bind ViewModel.SelectAllCommand}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B3;"/>
                        <TextBlock Text="Select All"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveSelectedRowsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveSelectedRows'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Selected"/>
                    </StackPanel>
                </Button>
            </StackPanel>
        </Grid>

        <!-- Date Filter Toolbar - T164: Dynamic button text -->
        <Grid Grid.Row="1" Margin="0,0,0,12">
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <TextBlock Text="Filter Date:" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <CalendarDatePicker Date="{x:Bind ViewModel.FromDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.StartDate'), Mode=OneWay}"
                                   Width="140"/>
                <TextBlock Text="to" VerticalAlignment="Center"/>
                <CalendarDatePicker Date="{x:Bind ViewModel.ToDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.EndDate'), Mode=OneWay}"
                                   Width="140"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <!-- T158, T159, T160, T161, T162, T163, T164: Date filter buttons with dynamic text -->
                <Button Content="{x:Bind ViewModel.LastWeekButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterLastWeekCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterLastWeek'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.TodayButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterTodayCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterToday'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisWeekButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterThisWeekCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisWeek'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisMonthButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterThisMonthCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisMonth'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisQuarterButtonText, Mode=OneTime}" 
                        Command="{x:Bind ViewModel.SetFilterThisQuarterCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisQuarter'), Mode=OneWay}"/>
                <Button Content="Show All" 
                        Command="{x:Bind ViewModel.SetFilterShowAllCommand}"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterShowAll'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="2"
                          x:Name="EditModeDataGrid"
                          ItemsSource="{x:Bind ViewModel.FilteredLoads, Mode=OneWay}"
                          AutoGenerateColumns="False"
                          IsReadOnly="False"
                          CanUserSortColumns="True"
                          GridLinesVisibility="All"
                          HeadersVisibility="Column"
                          SelectionMode="Single"
                          KeyDown="EditModeDataGrid_KeyDown"
                          CurrentCellChanged="EditModeDataGrid_CurrentCellChanged"
                          Tapped="EditModeDataGrid_Tapped">
            <controls:DataGrid.Columns>
                <controls:DataGridTemplateColumn Header="" Width="50" CanUserResize="False">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <CheckBox IsChecked="{Binding IsSelected, Mode=TwoWay}" 
                                      HorizontalAlignment="Center" 
                                      VerticalAlignment="Center"
                                      MinWidth="0"
                                      Margin="0"/>
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>
                <controls:DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Type" Binding="{Binding TypeName}" Width="*"/>
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartId}" Width="*"/>
                <controls:DataGridTextColumn Header="Quantity" Binding="{Binding Quantity}" Width="Auto"/>
                <controls:DataGridTextColumn Header="PO Number" Binding="{Binding PoNumber}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Location" Binding="{Binding Location}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Created Date" Binding="{Binding CreatedDate}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Created By" Binding="{Binding CreatedBy}" IsReadOnly="True" Width="Auto"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Footer: Pagination and Save -->
        <Grid Grid.Row="3" Margin="0,12,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <!-- Pagination Controls -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
                <Button Command="{x:Bind ViewModel.FirstPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FirstPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE892;"/>
                </Button>
                <Button Command="{x:Bind ViewModel.PreviousPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.PreviousPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE76B;"/>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <TextBlock Text="Page" VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.CurrentPage, Mode=OneWay}" VerticalAlignment="Center" FontWeight="SemiBold"/>
                <TextBlock Text="of" VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Command="{x:Bind ViewModel.NextPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.NextPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE76C;"/>
                </Button>
                <Button Command="{x:Bind ViewModel.LastPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LastPage'), Mode=OneWay}">
                    <FontIcon Glyph="&#xE893;"/>
                </Button>
            </StackPanel>

            <!-- Save Button -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="12">
                <Button Content="Save Changes" Command="{x:Bind ViewModel.SaveAllCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveChanges'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_EditModeView.xaml.cs
```csharp
public sealed partial class View_Dunnage_EditModeView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
_focusService.AttachFocusOnVisibility(this);
⋮----
private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void EditModeDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] Tapped: Grid empty");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[Dunnage_EditModeView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[Dunnage_EditModeView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_ManualEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    xmlns:enums="using:MTM_Receiving_Application.Module_Core.Models.Enums"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:Name="Root">

    <UserControl.Resources>
        <converters:Converter_DecimalToString x:Key="DecimalToStringConverter"/>
        <converters:Converter_LoadNumberToOneBased x:Key="LoadNumberConverter"/>
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled">
                <StackPanel Orientation="Horizontal" Spacing="12" Padding="0,0,12,0">
                <Button Command="{x:Bind ViewModel.AddRowCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE710;"/>
                        <TextBlock Text="Add Row"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.AddMultipleRowsCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddMultipleRows'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE710;"/>
                        <TextBlock Text="Add Multiple"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveRowCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Row"/>
                    </StackPanel>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Command="{x:Bind ViewModel.AutoFillCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AutoFill'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74E;"/>
                        <TextBlock Text="Auto-Fill"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.FillBlankSpacesCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FillBlanks'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B7;"/>
                        <TextBlock Text="Fill Blank Spaces"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.SortForPrintingCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SortForPrinting'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8CB;"/>
                        <TextBlock Text="Sort"/>
                    </StackPanel>
                </Button>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                </StackPanel>
            </ScrollViewer>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="1"
                          x:Name="ManualEntryDataGrid"
                          ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                          SelectedItem="{x:Bind ViewModel.SelectedLoad, Mode=TwoWay}"
                          AutoGenerateColumns="False"
                          IsReadOnly="False"
                          CanUserSortColumns="True"
                          GridLinesVisibility="All"
                          HeadersVisibility="Column"
                          SelectionMode="Single"
                          KeyDown="ManualEntryDataGrid_KeyDown"
                          CurrentCellChanged="ManualEntryDataGrid_CurrentCellChanged"
                          Tapped="ManualEntryDataGrid_Tapped">
            <controls:DataGrid.Columns>
                <controls:DataGridTextColumn Header="Load #" 
                                           Binding="{Binding LoadNumber, Converter={StaticResource LoadNumberConverter}}" 
                                           IsReadOnly="True" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="Type" 
                                           Binding="{Binding TypeName}" 
                                           Width="*"/>
                <controls:DataGridTextColumn Header="Part ID" 
                                           Binding="{Binding PartId}" 
                                           Width="*"/>
                <controls:DataGridTextColumn Header="Quantity" 
                                           Binding="{Binding Quantity}" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="PO Number" 
                                           Binding="{Binding PoNumber}" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="Location" 
                                           Binding="{Binding Location}" 
                                           Width="Auto"/>
                <controls:DataGridTextColumn Header="Home Location" 
                                           Binding="{Binding HomeLocation}" 
                                           Width="Auto"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Navigation -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0" Spacing="12">
            <Button Content="Save &amp; Finish" Command="{x:Bind ViewModel.SaveAllCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndFinish'), Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_ManualEntryView.xaml.cs
```csharp
public sealed partial class View_Dunnage_ManualEntryView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Loads_CollectionChanged: New row added");
ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] Loads_CollectionChanged: Selecting new item LoadNumber={newItem.LoadNumber}");
⋮----
ManualEntryDataGrid.ScrollIntoView(newItem, ManualEntryDataGrid.Columns.FirstOrDefault());
_ = Task.Run(async () =>
⋮----
await Task.Delay(100);
⋮----
private void ManualEntryDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void ManualEntryDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void ManualEntryDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Tapped: Grid empty, triggering AddRow command.");
if (ViewModel.AddRowCommand.CanExecute(null))
⋮----
ViewModel.AddRowCommand.Execute(null);
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[Dunnage_ManualEntryView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[Dunnage_ManualEntryView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_ModeSelectionView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
    mc:Ignorable="d">

    <Grid>
        <StackPanel Spacing="32" HorizontalAlignment="Center" VerticalAlignment="Center">
            <StackPanel Orientation="Horizontal" Spacing="24">
                <!-- Guided Wizard Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectGuidedModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Guided Wizard Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE771;" 
                                      FontSize="48" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Guided Wizard" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Step-by-step workflow for dunnage receiving with validation at each stage." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Guided Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsGuidedModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetGuidedAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Tooltip.Button.QuickGuidedWizard'), Mode=OneWay}"/>
                </StackPanel>

                <!-- Manual Entry Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectManualModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Manual Entry Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE7F0;" 
                                      FontSize="48" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Manual Entry" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Bulk grid entry for receiving multiple dunnage items simultaneously." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Manual Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsManualModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetManualAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Tooltip.Button.QuickManualEntry'), Mode=OneWay}"/>
                </StackPanel>

                <!-- Edit Mode Column -->
                <StackPanel Spacing="12">
                    <Button Command="{x:Bind ViewModel.SelectEditModeCommand}"
                            Width="280" Height="180"
                            Padding="20"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            AutomationProperties.Name="Edit Mode">
                        <StackPanel Spacing="12">
                            <FontIcon Glyph="&#xE70F;" 
                                      FontSize="48" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Edit Mode" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Load historical dunnage records for editing and correction." 
                                       Style="{StaticResource BodyTextBlockStyle}" 
                                       TextWrapping="Wrap" 
                                       TextAlignment="Center"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </Button>
                    
                    <!-- Edit Mode Checkbox -->
                    <CheckBox Content="Set as default mode"
                              IsChecked="{x:Bind ViewModel.IsEditModeDefault, Mode=TwoWay}"
                              Command="{x:Bind ViewModel.SetEditAsDefaultCommand}"
                              CommandParameter="{Binding IsChecked, RelativeSource={RelativeSource Self}}"
                              HorizontalAlignment="Center"
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickEditMode'), Mode=OneWay}"/>
                </StackPanel>
            </StackPanel>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_ModeSelectionView.xaml.cs
```csharp
public sealed partial class View_Dunnage_ModeSelectionView : UserControl
```

## File: Module_Dunnage/Views/View_Dunnage_PartSelectionView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_PartSelectionView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_PartSelectionView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,24">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <TextBlock Style="{StaticResource TitleTextBlockStyle}" Text="Select Part - "/>
                <mi:MaterialIcon Kind="{x:Bind ViewModel.SelectedTypeIconKind, Mode=OneWay}" 
                                 Width="24" Height="24"
                                 Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                                 VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.SelectedTypeName, Mode=OneWay}" 
                           Style="{StaticResource TitleTextBlockStyle}"/>
            </StackPanel>
            <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.PartSelection'), Mode=OneWay}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- Inventory Notification -->
        <InfoBar Grid.Row="1"
                 IsOpen="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay}"
                 Severity="Warning"
                 Message="{x:Bind ViewModel.InventoryNotificationMessage, Mode=OneWay}"
                 Margin="0,0,0,16"/>

        <!-- Part Selection Form -->
        <ScrollViewer Grid.Row="2">
            <StackPanel Spacing="24">
                
                <!-- Part Dropdown -->
                <StackPanel Spacing="12">
                    <TextBlock Text="Select Part" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <ComboBox x:Name="PartNumberComboBox"
                              ItemsSource="{x:Bind ViewModel.AvailableParts, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                              DisplayMemberPath="PartId"
                              PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PartSelection'), Mode=OneWay}"
                              HorizontalAlignment="Stretch"
                              MinWidth="300"/>
                </StackPanel>

                <!-- Part Details Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20"
                        Visibility="{x:Bind ViewModel.IsPartSelected, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                    <Grid RowSpacing="12">
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>

                        <!-- Header -->
                        <TextBlock Grid.Row="0" 
                                   Text="Part Details" 
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>

                        <!-- Details Grid -->
                        <Grid Grid.Row="1" ColumnSpacing="24" RowSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>

                            <!-- Part ID -->
                            <TextBlock Grid.Row="0" Grid.Column="0" 
                                       Text="Part ID:" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <TextBlock Grid.Row="0" Grid.Column="1" 
                                       Text="{x:Bind ViewModel.SelectedPart.PartId, Mode=OneWay}" 
                                       FontWeight="SemiBold"/>

                            <!-- Type -->
                            <TextBlock Grid.Row="1" Grid.Column="0" 
                                       Text="Type:" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <StackPanel Grid.Row="1" Grid.Column="1" Orientation="Horizontal" Spacing="6">
                                <mi:MaterialIcon Kind="{x:Bind ViewModel.SelectedTypeIconKind, Mode=OneWay}" 
                                                 Width="16" Height="16"
                                                 Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                                <TextBlock Text="{x:Bind ViewModel.SelectedTypeName, Mode=OneWay}"/>
                            </StackPanel>
                        </Grid>

                        <!-- Inventory Method (if applicable) -->
                        <StackPanel Grid.Row="2" 
                                    Orientation="Horizontal" 
                                    Spacing="8"
                                    Visibility="{x:Bind ViewModel.IsInventoryNotificationVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <FontIcon Glyph="&#xE946;" 
                                      FontSize="16" 
                                      Foreground="{ThemeResource SystemFillColorCautionBrush}"/>
                            <TextBlock Text="Inventory Method:" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <TextBlock Text="{x:Bind ViewModel.InventoryMethod, Mode=OneWay}" 
                                       FontWeight="SemiBold"
                                       Foreground="{ThemeResource SystemFillColorCautionBrush}"/>
                        </StackPanel>
                    </Grid>
                </Border>

                <!-- Action Buttons -->
                <StackPanel Orientation="Horizontal" Spacing="12">
                    <Button Command="{x:Bind ViewModel.QuickAddPartCommand}"
                            HorizontalAlignment="Left">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE710;" FontSize="16"/>
                            <TextBlock Text="Add New Part"/>
                        </StackPanel>
                    </Button>
                    
                    <Button Command="{x:Bind ViewModel.LoadPartsCommand}"
                            HorizontalAlignment="Left"
                            ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.Refresh'), Mode=OneWay}">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE72C;" FontSize="16"/>
                            <TextBlock Text="Refresh Parts"/>
                        </StackPanel>
                    </Button>
                </StackPanel>

            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_PartSelectionView.xaml.cs
```csharp
public sealed partial class View_Dunnage_PartSelectionView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, PartNumberComboBox);
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
System.Diagnostics.Debug.WriteLine("Dunnage_PartSelectionView: OnLoaded called");
await ViewModel.InitializeAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_QuantityEntryView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_QuantityEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_QuantityEntryView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Spacing="8" Margin="0,0,0,16">
            <TextBlock Text="Enter Quantity"
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="Specify how many labels you need to generate."
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}"/>
        </StackPanel>

        <!-- Context Banner -->
        <Border Grid.Row="1"
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="12"
                Margin="0,0,0,12">
            <StackPanel Spacing="6">
                <TextBlock Text="Selection Summary"
                           FontWeight="SemiBold"
                           FontSize="13"/>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Text="Type:" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                    <mi:MaterialIcon Kind="{x:Bind ViewModel.SelectedTypeIconKind, Mode=OneWay}" 
                                     Width="16" Height="16"
                                     Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="{x:Bind ViewModel.SelectedTypeName, Mode=OneWay}"
                               FontWeight="SemiBold"
                               FontSize="12"/>
                </StackPanel>
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Text="Part:" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                    <TextBlock Text="{x:Bind ViewModel.SelectedPartName, Mode=OneWay}"
                               FontWeight="SemiBold"
                               FontSize="12"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- Quantity Input -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <StackPanel Spacing="12">
                <!-- Quantity Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="6">
                        <TextBlock Text="Quantity"
                                   Style="{StaticResource SubtitleTextBlockStyle}"/>
                        <NumberBox x:Name="QuantityNumberBox"
                                   Value="{x:Bind ViewModel.Quantity, Mode=TwoWay}"
                                   Minimum="1"
                                   Maximum="9999"
                                   SpinButtonPlacementMode="Inline"
                                   HorizontalAlignment="Stretch"
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.Quantity'), Mode=OneWay}"/>
                        
                        <!-- Validation Message -->
                        <TextBlock Text="{x:Bind ViewModel.ValidationMessage, Mode=OneWay}"
                                   Foreground="{ThemeResource SystemFillColorCriticalBrush}"
                                   Visibility="{x:Bind ViewModel.ValidationMessage, Mode=OneWay, Converter={StaticResource EmptyStringToVisibilityConverter}}"
                                   TextWrapping="Wrap"
                                   FontSize="12"/>
                    </StackPanel>
                </Border>

                <!-- Info Panel Card -->
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="16">
                    <StackPanel Spacing="6">
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon Glyph="&#xE946;" 
                                      FontSize="16" 
                                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Tip" FontWeight="SemiBold"/>
                        </StackPanel>
                        <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.QuantityEntry'), Mode=OneWay}"
                                   TextWrapping="Wrap"
                                   FontSize="12"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_QuantityEntryView.xaml.cs
```csharp
public sealed partial class View_Dunnage_QuantityEntryView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this, QuantityNumberBox);
⋮----
private void OnLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.LoadContextData();
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_QuickAddPartDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Add New Dunnage Part"
    PrimaryButtonText="Add Part"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnPrimaryButtonClick"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="500">

    <ScrollViewer MaxHeight="500">
        <StackPanel Spacing="20" Padding="8">
            <!-- Instructions -->
            <TextBlock 
                Text="Enter details for the new dunnage part"
                TextWrapping="Wrap"
                Style="{StaticResource BodyTextBlockStyle}"/>

            <!-- Type Display (Read-only) -->
            <Border Background="{ThemeResource LayerFillColorDefaultBrush}"
                    Padding="12"
                    CornerRadius="4">
                <StackPanel Spacing="4">
                    <TextBlock Text="Dunnage Type" 
                               Style="{StaticResource CaptionTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    <TextBlock x:Name="TypeNameTextBlock"
                               FontWeight="SemiBold"/>
                </StackPanel>
            </Border>

            <!-- Part ID Field -->
            <StackPanel Spacing="8">
                <TextBlock Text="Part ID (Auto-generated)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBox 
                    x:Name="PartIdTextBox"
                    IsReadOnly="True"
                    PlaceholderText="Auto-generated based on dimensions"
                    MaxLength="100"/>
                <TextBlock 
                    Text="Part ID is generated from Type and Dimensions"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>

            <!-- Specifications Section -->
            <StackPanel Spacing="12">
                <TextBlock Text="Specifications" 
                           Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBlock 
                    Text="Enter dimensions to generate the Part ID"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Style="{StaticResource CaptionTextBlockStyle}"/>

                <Grid ColumnSpacing="12" RowSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>

                    <!-- Width -->
                    <StackPanel Grid.Column="0" Spacing="4">
                        <TextBlock Text="Width (in)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        <NumberBox x:Name="WidthNumberBox" 
                                   PlaceholderText="0"
                                   SpinButtonPlacementMode="Hidden"
                                   Minimum="0"
                                   Maximum="999"
                                   SmallChange="1"
                                   LargeChange="10"
                                   ValueChanged="OnDimensionChanged"/>
                    </StackPanel>

                    <!-- Height -->
                    <StackPanel Grid.Column="1" Spacing="4">
                        <TextBlock Text="Height (in)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        <NumberBox x:Name="HeightNumberBox" 
                                   PlaceholderText="0"
                                   SpinButtonPlacementMode="Hidden"
                                   Minimum="0"
                                   Maximum="999"
                                   SmallChange="1"
                                   LargeChange="10"
                                   ValueChanged="OnDimensionChanged"/>
                    </StackPanel>

                    <!-- Depth -->
                    <StackPanel Grid.Column="2" Spacing="4">
                        <TextBlock Text="Depth (in)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        <NumberBox x:Name="DepthNumberBox" 
                                   PlaceholderText="0"
                                   SpinButtonPlacementMode="Hidden"
                                   Minimum="0"
                                   Maximum="999"
                                   SmallChange="1"
                                   LargeChange="10"
                                   ValueChanged="OnDimensionChanged"/>
                    </StackPanel>
                </Grid>
            </StackPanel>

            <!-- Dynamic Specs Container -->
            <StackPanel x:Name="DynamicSpecsPanel" Spacing="12"/>

            <!-- Additional Notes -->
            <StackPanel Spacing="8">
                <TextBlock Text="Additional Notes (Optional)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBox 
                    x:Name="NotesTextBox"
                    PlaceholderText="Any additional specifications or notes"
                    TextWrapping="Wrap"
                    AcceptsReturn="True"
                    MaxLength="500"
                    Height="80"/>
            </StackPanel>

        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddPartDialog.xaml.cs
```csharp
public sealed partial class View_Dunnage_QuickAddPartDialog : ContentDialog
⋮----
private void GenerateSpecFields()
⋮----
if (string.Equals(spec.SpecKey, "Width", System.StringComparison.OrdinalIgnoreCase) ||
string.Equals(spec.SpecKey, "Height", System.StringComparison.OrdinalIgnoreCase) ||
string.Equals(spec.SpecKey, "Depth", System.StringComparison.OrdinalIgnoreCase))
⋮----
def = new SpecDefinition { DataType = "Text" };
⋮----
var stackPanel = new StackPanel { Spacing = 4 };
⋮----
if (!string.IsNullOrEmpty(def.Unit))
⋮----
var label = new TextBlock
⋮----
stackPanel.Children.Add(label);
⋮----
if (string.Equals(def.DataType, "Number", System.StringComparison.OrdinalIgnoreCase))
⋮----
var numberBox = new NumberBox
⋮----
else if (string.Equals(def.DataType, "Boolean", System.StringComparison.OrdinalIgnoreCase))
⋮----
var checkBox = new CheckBox
⋮----
var textBox = new TextBox
⋮----
PlaceholderText = $"Enter {spec.SpecKey.ToLower()}",
⋮----
stackPanel.Children.Add(inputControl);
DynamicSpecsPanel.Children.Add(stackPanel);
⋮----
private void OnDimensionChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
⋮----
private void UpdatePartId()
⋮----
parts.Add(TypeName);
⋮----
if (kvp.Value is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
⋮----
textSpecs.Add(tb.Text.Trim());
⋮----
if (!double.IsNaN(WidthNumberBox.Value) && WidthNumberBox.Value > 0)
⋮----
numbers.Add(WidthNumberBox.Value);
⋮----
if (!double.IsNaN(HeightNumberBox.Value) && HeightNumberBox.Value > 0)
⋮----
numbers.Add(HeightNumberBox.Value);
⋮----
if (!double.IsNaN(DepthNumberBox.Value) && DepthNumberBox.Value > 0)
⋮----
numbers.Add(DepthNumberBox.Value);
⋮----
if (kvp.Value is NumberBox nb && !double.IsNaN(nb.Value) && nb.Value > 0)
⋮----
numbers.Add(nb.Value);
⋮----
var formattedNumbers = numbers.Select(n =>
n == Math.Floor(n) ? ((int)n).ToString() : n.ToString("0.##")
⋮----
parts.Add($"({string.Join("x", formattedNumbers)})");
⋮----
// 4. Boolean specs (only if true, abbreviated if >2 words)
⋮----
var words = specName.Split(new[] { ' ', '_' }, System.StringSplitOptions.RemoveEmptyEntries);
⋮----
// Abbreviate: use first letter of each word
boolSpecs.Add(string.Join("", words.Select(w => char.ToUpper(w[0]))));
⋮----
// Use full name
boolSpecs.Add(specName);
⋮----
parts.Add(string.Join(", ", boolSpecs));
⋮----
// Add text specs if any
⋮----
// Insert text specs after type name
parts.Insert(1, string.Join(", ", textSpecs));
⋮----
PartIdTextBox.Text = string.Join(" - ", parts);
⋮----
private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
// Set Part ID
PartId = PartIdTextBox.Text.Trim();
// Build spec values dictionary
⋮----
// Add dynamic specs
⋮----
specValues[kvp.Key] = tb.Text.Trim();
⋮----
else if (kvp.Value is NumberBox nb && !double.IsNaN(nb.Value))
⋮----
// Add dimensions if provided
⋮----
if (!string.IsNullOrWhiteSpace(NotesTextBox.Text))
⋮----
specValues["Notes"] = NotesTextBox.Text.Trim();
⋮----
SpecValuesJson = JsonSerializer.Serialize(specValues);
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddTypeDialog.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_QuickAddTypeDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    xmlns:local="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    Title="Add New Dunnage Type"
    PrimaryButtonText="Add Type"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnPrimaryButtonClick"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="900">

    <ContentDialog.Resources>
        <converters:Converter_IconCodeToGlyph x:Key="IconCodeToGlyphConverter"/>
        <!-- Help Button Style -->
        <Style x:Key="HelpButtonStyle" TargetType="Button">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Padding" Value="8"/>
            <Setter Property="CornerRadius" Value="4"/>
        </Style>
    </ContentDialog.Resources>

    <ScrollViewer VerticalScrollBarVisibility="Auto" MaxHeight="600">
        <StackPanel Spacing="24" Padding="12">
            <!-- Header with Help Button -->
            <Grid>
                <TextBlock 
                    Text="Enter details for the new dunnage type"
                    TextWrapping="Wrap"
                    VerticalAlignment="Center"
                    Style="{StaticResource BodyTextBlockStyle}"/>
                
                <Button x:Name="HelpButton"
                        HorizontalAlignment="Right"
                        VerticalAlignment="Top"
                        Style="{StaticResource HelpButtonStyle}"
                        ToolTipService.ToolTip="Click for help about dunnage types">
                    <StackPanel Orientation="Horizontal" Spacing="6">
                        <FontIcon Glyph="&#xE946;" FontSize="14" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                        <TextBlock Text="Help" FontSize="12" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    </StackPanel>
                    <Button.Flyout>
                        <Flyout>
                            <StackPanel Spacing="16" MaxWidth="400" Padding="4">
                                <StackPanel Spacing="8">
                                    <TextBlock Text="Dunnage Types" Style="{StaticResource SubtitleTextBlockStyle}" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <Border Height="2" Background="{ThemeResource AccentFillColorDefaultBrush}" CornerRadius="1" HorizontalAlignment="Left" Width="60"/>
                                    <TextBlock TextWrapping="Wrap" Foreground="{ThemeResource TextFillColorSecondaryBrush}">Define categories of dunnage materials for receiving classification.</TextBlock>
                                </StackPanel>
                                
                                <Border Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="12" CornerRadius="6" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                                    <StackPanel Spacing="8">
                                        <TextBlock Text="Type Name" FontWeight="SemiBold" FontSize="13"/>
                                        <TextBlock TextWrapping="Wrap" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}">Choose a descriptive name like 'Pallet', 'Crate', or 'Foam Insert'. This will be displayed in selection lists.</TextBlock>
                                    </StackPanel>
                                </Border>
                                
                                <Border Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="12" CornerRadius="6" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                                    <StackPanel Spacing="8">
                                        <TextBlock Text="Specifications" FontWeight="SemiBold" FontSize="13"/>
                                        <TextBlock TextWrapping="Wrap" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}">Add custom fields to capture specific data for this type. For example, 'Material' (Text), 'Weight Capacity' (Number with unit 'lbs'), or 'Recyclable' (Boolean).</TextBlock>
                                    </StackPanel>
                                </Border>
                                
                                <Border Background="{ThemeResource SystemFillColorAttentionBackgroundBrush}" Padding="12" CornerRadius="6" BorderThickness="1" BorderBrush="{ThemeResource SystemFillColorAttentionBrush}">
                                    <StackPanel Spacing="6">
                                        <StackPanel Orientation="Horizontal" Spacing="6">
                                            <FontIcon Glyph="&#xE946;" FontSize="12" Foreground="{ThemeResource SystemFillColorAttentionBrush}"/>
                                            <TextBlock Text="Tip" FontWeight="SemiBold" FontSize="12" Foreground="{ThemeResource SystemFillColorAttentionBrush}"/>
                                        </StackPanel>
                                        <TextBlock TextWrapping="Wrap" FontSize="11">Required specs must be filled when creating new parts. Use Number type for measurements and validation.</TextBlock>
                                    </StackPanel>
                                </Border>
                            </StackPanel>
                        </Flyout>
                    </Button.Flyout>
                </Button>
            </Grid>

            <!-- Type Name Field -->
            <StackPanel Spacing="12">
                <TextBlock Text="Type Name" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBox 
                    x:Name="TypeNameTextBox"
                    PlaceholderText="e.g., Pallet, Crate, Blocking"
                    MaxLength="100"
                    TextChanged="OnTypeNameChanged"
                    ToolTipService.ToolTip="Enter a unique descriptive name for this dunnage type"/>
                <TextBlock 
                    x:Name="ValidationTextBlock"
                    Text="Type name is required"
                    Foreground="{ThemeResource SystemFillColorCriticalBrush}"
                    Visibility="Collapsed"
                    Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>

            <!-- Icon Selection -->
            <StackPanel Spacing="12">
                <TextBlock Text="Select Icon" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                <TextBlock 
                    Text="Choose an icon to represent this type"
                    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                    Style="{StaticResource CaptionTextBlockStyle}"/>
                
                <Button x:Name="SelectIconButton"
                        Click="OnSelectIconClick"
                        HorizontalAlignment="Stretch"
                        Padding="16,12"
                        ToolTipService.ToolTip="Click to browse and select an icon from the icon library">
                    <StackPanel Orientation="Horizontal" Spacing="12">
                        <materialIcons:MaterialIcon x:Name="SelectedIconDisplay" 
                                  Kind="{x:Bind SelectedIconKind, Mode=OneWay}" 
                                  Width="24" Height="24"
                                  Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                        <StackPanel VerticalAlignment="Center">
                            <TextBlock Text="Click to select icon" FontWeight="SemiBold"/>
                            <TextBlock x:Name="SelectedIconNameText"
                                       Text="Box"
                                       FontSize="12"
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </StackPanel>
                    </StackPanel>
                </Button>
            </StackPanel>

            <!-- Specifications -->
            <StackPanel Spacing="12">
                <StackPanel Spacing="4">
                    <TextBlock Text="Specifications" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <TextBlock 
                        Text="Define custom fields to capture specific data for this dunnage type"
                        Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                        Style="{StaticResource CaptionTextBlockStyle}"/>
                </StackPanel>
            
                <!-- Add Spec Form -->
                <Border Background="{ThemeResource LayerFillColorDefaultBrush}" 
                        CornerRadius="6" 
                        Padding="16"
                        BorderThickness="1"
                        BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}">
                    <StackPanel Spacing="12">
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="2*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            <TextBox x:Name="NewSpecNameBox" 
                                     PlaceholderText="Field Name (e.g. Material, Weight)"
                                     ToolTipService.ToolTip="Enter a unique name for this specification field"/>
                            <ComboBox x:Name="NewSpecTypeCombo" 
                                      Grid.Column="1"
                                      SelectedIndex="0" 
                                      HorizontalAlignment="Stretch"
                                      ToolTipService.ToolTip="Select the data type for this field">
                                <ComboBoxItem Content="Text"/>
                                <ComboBoxItem Content="Number"/>
                                <ComboBoxItem Content="Boolean"/>
                            </ComboBox>
                        </Grid>
                        
                        <!-- Number Type Options -->
                        <StackPanel x:Name="NumberOptionsPanel" Spacing="8" Visibility="Collapsed">
                            <TextBlock Text="Number Validation" FontSize="12" FontWeight="SemiBold" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <Grid ColumnSpacing="12">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                <NumberBox x:Name="NewSpecMinValueBox" 
                                           Header="Min Value" 
                                           PlaceholderText="Optional"
                                           SpinButtonPlacementMode="Compact"
                                           ToolTipService.ToolTip="Minimum allowed value (optional)"/>
                                <NumberBox x:Name="NewSpecMaxValueBox" 
                                           Grid.Column="1"
                                           Header="Max Value" 
                                           PlaceholderText="Optional"
                                           SpinButtonPlacementMode="Compact"
                                           ToolTipService.ToolTip="Maximum allowed value (optional)"/>
                                <TextBox x:Name="NewSpecUnitBox" 
                                         Grid.Column="2"
                                         Header="Unit" 
                                         PlaceholderText="e.g. inches, lbs"
                                         ToolTipService.ToolTip="Unit of measurement (optional)"/>
                            </Grid>
                        </StackPanel>
                        
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <StackPanel Orientation="Horizontal" Spacing="16" VerticalAlignment="Center">
                                <CheckBox x:Name="NewSpecRequiredCheck" 
                                          Content="Required Field"
                                          ToolTipService.ToolTip="Check if this field is required"/>
                            </StackPanel>
                            <Button Grid.Column="1" 
                                    Content="Add Field" 
                                    Click="OnAddSpecClick" 
                                    Style="{StaticResource AccentButtonStyle}"
                                    ToolTipService.ToolTip="Add this specification to the list below"/>
                        </Grid>
                    </StackPanel>
                </Border>

                <ListView x:Name="SpecsListView" 
                          Height="130"
                          BorderThickness="1"
                          BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}"
                          CornerRadius="6"
                          Padding="4"
                          ToolTipService.ToolTip="List of specifications defined for this type">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_SpecItem">
                        <Grid Padding="0,4">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            <StackPanel Spacing="2">
                                <TextBlock Text="{x:Bind Name}" FontWeight="SemiBold" FontSize="13"/>
                                <TextBlock Text="{x:Bind Description}" 
                                           FontSize="11" 
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            </StackPanel>
                            <Border Grid.Column="1" 
                                    Background="{ThemeResource SystemFillColorAttentionBackgroundBrush}"
                                    CornerRadius="3"
                                    Padding="6,2"
                                    Margin="8,0"
                                    Visibility="{x:Bind IsRequiredVisibility}">
                                <TextBlock Text="Required" FontSize="10" FontWeight="SemiBold"/>
                            </Border>
                            <Button Grid.Column="2" 
                                    Content="&#xE74D;"
                                    FontFamily="Segoe MDL2 Assets"
                                    ToolTipService.ToolTip="Remove this specification"
                                    Click="OnRemoveSpecClick" 
                                    Style="{StaticResource SubtleButtonStyle}"/>
                        </Grid>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>
            </StackPanel>
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

## File: Module_Dunnage/Views/View_Dunnage_QuickAddTypeDialog.xaml.cs
```csharp
public sealed partial class View_Dunnage_QuickAddTypeDialog : ContentDialog, INotifyPropertyChanged
⋮----
private MaterialIconKind _selectedIcon = MaterialIconKind.PackageVariantClosed;
⋮----
private void OnSpecTypeChanged(object sender, SelectionChangedEventArgs e)
⋮----
var type = item.Content.ToString();
⋮----
public void InitializeForEdit(string typeName, string iconName, Dictionary<string, SpecDefinition> specs)
⋮----
SelectedIconNameText.Text = kind.ToString();
⋮----
Specs.Clear();
⋮----
Specs.Add(new Model_SpecItem
⋮----
private void OnAddSpecClick(object sender, RoutedEventArgs e)
⋮----
private void AddSpec()
⋮----
var name = NewSpecNameBox.Text.Trim();
if (string.IsNullOrWhiteSpace(name))
⋮----
if (Specs.Any(s => s.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
⋮----
var type = typeItem?.Content.ToString() ?? "Text";
⋮----
unit = NewSpecUnitBox.Text.Trim();
if (!double.IsNaN(NewSpecMinValueBox.Value))
⋮----
if (!double.IsNaN(NewSpecMaxValueBox.Value))
⋮----
private void OnRemoveSpecClick(object sender, RoutedEventArgs e)
⋮----
Specs.Remove(spec);
⋮----
private async void OnSelectIconClick(object sender, RoutedEventArgs e)
⋮----
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(iconWindow);
var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
iconWindow.Activate();
⋮----
iconWindow.Closed += (s, args) => tcs.SetResult(true);
⋮----
private void OnTypeNameChanged(object sender, TextChangedEventArgs e)
⋮----
private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
var input = TypeNameTextBox.Text.Trim();
if (string.IsNullOrWhiteSpace(input))
⋮----
if (!char.IsUpper(input[0]))
⋮----
if (input.Contains(" "))
⋮----
if (!AlphanumericRegex().IsMatch(input))
⋮----
private static partial System.Text.RegularExpressions.Regex AlphanumericRegex();
private void ShowError(string message)
⋮----
private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
⋮----
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
```

## File: Module_Dunnage/Views/View_Dunnage_ReviewView.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_ReviewView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
    xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    x:DataType="views:View_Dunnage_ReviewView"
    Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Success Message -->
        <InfoBar Grid.Row="0"
                 IsOpen="{x:Bind ViewModel.IsSuccessMessageVisible, Mode=OneWay}"
                 Severity="Success"
                 Message="{x:Bind ViewModel.SuccessMessage, Mode=OneWay}"
                 IsClosable="False">
            <InfoBar.ActionButton>
                <Button Content="Start New Entry" Command="{x:Bind ViewModel.StartNewEntryCommand}"/>
            </InfoBar.ActionButton>
        </InfoBar>

        <!-- Content Section with View Toggle -->
        <Grid Grid.Row="1">
            <!-- Single Entry View -->
            <Grid Visibility="{x:Bind ViewModel.IsSingleView, Mode=OneWay}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Entry Counter -->
                <Border Grid.Row="0" 
                       Background="{ThemeResource AccentFillColorDefaultBrush}"
                       Padding="12,6"
                       CornerRadius="8,8,0,0">
                    <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                        <FontIcon Glyph="&#xE8FD;" 
                                 FontSize="16" 
                                 Foreground="White"/>
                        <TextBlock Style="{StaticResource SubtitleTextBlockStyle}"
                                   Foreground="White">
                            <Run Text="Entry"/>
                            <Run Text="{x:Bind ViewModel.CurrentEntryIndex, Mode=OneWay}"/>
                            <Run Text="of"/>
                            <Run Text="{x:Bind ViewModel.LoadCount, Mode=OneWay}"/>
                        </TextBlock>
                    </StackPanel>
                </Border>

                <!-- Single Entry Form -->
                <Grid Grid.Row="1">
                    <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                           BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                           BorderThickness="1,0,1,1"
                           CornerRadius="0,0,8,8"
                           Padding="20">
                        <Grid MaxWidth="900">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>

                            <!-- Left Column -->
                            <StackPanel Grid.Column="0" Spacing="12" Margin="0,0,12,0">
                                <TextBlock Text="Load Details" 
                                          Style="{StaticResource SubtitleTextBlockStyle}"
                                          Margin="0,0,0,8"/>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Type" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <StackPanel Orientation="Horizontal" Spacing="6">
                                        <mi:MaterialIcon Kind="{x:Bind ViewModel.CurrentLoad.TypeIconKind, Mode=OneWay}" 
                                                         Width="18" Height="18"
                                                         Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                                        <TextBlock Text="{x:Bind ViewModel.CurrentLoad.TypeName, Mode=OneWay}" 
                                                  Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                    </StackPanel>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Part ID" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.PartId, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Quantity" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.Quantity, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>

                            <!-- Right Column -->
                            <StackPanel Grid.Column="1" Spacing="12" Margin="12,0,0,0">
                                <TextBlock Text="Additional Information" 
                                          Style="{StaticResource SubtitleTextBlockStyle}"
                                          Margin="0,0,0,8"/>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="PO Number" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.PoNumber, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Location" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.Location, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>

                                <StackPanel Spacing="4">
                                    <TextBlock Text="Inventory Method" 
                                              Style="{StaticResource CaptionTextBlockStyle}"
                                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="{x:Bind ViewModel.CurrentLoad.InventoryMethod, Mode=OneWay}" 
                                              Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>
                        </Grid>
                    </Border>
                </Grid>

                <!-- Navigation Bar for Single View -->
                <CommandBar Grid.Row="2" DefaultLabelPosition="Right" Margin="0,12,0,0">
                    <AppBarButton Icon="Back" 
                                 Label="Previous" 
                                 Command="{x:Bind ViewModel.PreviousEntryCommand}"
                                 IsEnabled="{x:Bind ViewModel.CanGoBack, Mode=OneWay}"
                                 AutomationProperties.Name="Previous Entry"/>
                    
                    <AppBarButton Icon="Forward" 
                                 Label="Next" 
                                 Command="{x:Bind ViewModel.NextEntryCommand}"
                                 IsEnabled="{x:Bind ViewModel.CanGoNext, Mode=OneWay}"
                                 AutomationProperties.Name="Next Entry"/>
                    
                    <AppBarSeparator/>
                    
                    <AppBarButton Icon="ViewAll" 
                                 Label="Table View" 
                                 Command="{x:Bind ViewModel.SwitchToTableViewCommand}"
                                 AutomationProperties.Name="Switch to Table View"/>
                </CommandBar>
            </Grid>

            <!-- Table View (DataGrid) -->
            <Grid Visibility="{x:Bind ViewModel.IsTableView, Mode=OneWay}">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <Border Grid.Row="0"
                       Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                       BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                       BorderThickness="1"
                       CornerRadius="8"
                       Padding="16">
                    <controls:DataGrid x:Name="ReviewDataGrid"
                                      ItemsSource="{x:Bind ViewModel.SessionLoads, Mode=OneWay}"
                                      AutoGenerateColumns="False"
                                      CanUserReorderColumns="False"
                                      CanUserResizeColumns="True"
                                      CanUserSortColumns="True"
                                      GridLinesVisibility="None"
                                      HeadersVisibility="Column"
                                      SelectionMode="Single"
                                      IsReadOnly="True">
                        <controls:DataGrid.Columns>
                            <controls:DataGridTextColumn Header="Type" 
                                                        Binding="{Binding TypeName}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Part ID" 
                                                        Binding="{Binding PartId}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Quantity" 
                                                        Binding="{Binding Quantity}" 
                                                        Width="100"/>
                            <controls:DataGridTextColumn Header="PO Number" 
                                                        Binding="{Binding PoNumber}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Location" 
                                                        Binding="{Binding Location}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Inventory Method" 
                                                        Binding="{Binding InventoryMethod}" 
                                                        Width="*"/>
                        </controls:DataGrid.Columns>
                    </controls:DataGrid>
                </Border>

                <!-- Navigation Bar for Table View -->
                <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,16,0,0">
                    <AppBarButton Label="Single View" 
                                 Command="{x:Bind ViewModel.SwitchToSingleViewCommand}"
                                 AutomationProperties.Name="Switch to Single View">
                        <AppBarButton.Icon>
                            <FontIcon Glyph="&#xE8FD;"/>
                        </AppBarButton.Icon>
                    </AppBarButton>
                </CommandBar>
            </Grid>
        </Grid>

        <!-- Action Buttons -->
        <Grid Grid.Row="2" ColumnSpacing="12" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Button Grid.Column="1" 
                   Command="{x:Bind ViewModel.AddAnotherCommand}"
                   AutomationProperties.Name="Add Another Load"
                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddAnother'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE710;" FontSize="16"/>
                    <TextBlock Text="Add Another"/>
                </StackPanel>
            </Button>
            
            <Button Grid.Column="2" 
                   Command="{x:Bind ViewModel.SaveAllCommand}"
                   Style="{StaticResource AccentButtonStyle}"
                   IsEnabled="{x:Bind ViewModel.CanSave, Mode=OneWay}"
                   AutomationProperties.Name="Save to Database">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE74E;" FontSize="16"/>
                    <TextBlock Text="Save All"/>
                </StackPanel>
            </Button>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_ReviewView.xaml.cs
```csharp
public sealed partial class View_Dunnage_ReviewView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private void OnLoaded(object sender, RoutedEventArgs e)
⋮----
ViewModel.LoadSessionLoads();
```

## File: Module_Dunnage/Contracts/IService_DunnageAdminWorkflow.cs
```csharp
public interface IService_DunnageAdminWorkflow
⋮----
public Task NavigateToSectionAsync(Enum_DunnageAdminSection section);
public Task NavigateToHubAsync();
public Task<bool> CanNavigateAwayAsync();
public void MarkDirty();
public void MarkClean();
```

## File: Module_Dunnage/Contracts/IService_DunnageCSVWriter.cs
```csharp
public interface IService_DunnageCSVWriter
⋮----
public Task<Model_CSVWriteResult> WriteToCsvAsync(List<Model_DunnageLoad> loads, string typeName);
public Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads);
public Task<Model_CSVWriteResult> WriteDynamicCsvAsync(
⋮----
public Task<Model_CSVWriteResult> ExportSelectedLoadsAsync(
⋮----
public Task<bool> IsNetworkPathAvailableAsync(int timeout = 3);
public string GetLocalCsvPath(string filename);
public string GetNetworkCsvPath(string filename);
public Task<Model_CSVDeleteResult> ClearCSVFilesAsync(string? filenamePattern = null);
```

## File: Module_Dunnage/Contracts/IService_DunnageWorkflow.cs
```csharp
public interface IService_DunnageWorkflow
⋮----
public event EventHandler StepChanged;
⋮----
public Task<bool> StartWorkflowAsync();
public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync();
public void GoToStep(Enum_DunnageWorkflowStep step);
public Task<Model_SaveResult> SaveSessionAsync();
public Task<Model_SaveResult> SaveToCSVOnlyAsync();
public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();
public void ClearSession();
public Task<Model_CSVDeleteResult> ResetCSVFilesAsync();
public void AddCurrentLoadToSession();
```

## File: Module_Dunnage/Contracts/IService_MySQL_Dunnage.cs
```csharp
public interface IService_MySQL_Dunnage
⋮----
public Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
public Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
public Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon);
public Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type);
public Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
public Task<Model_Dao_Result> DeleteTypeAsync(int typeId);
public Task<Model_Dao_Result<int>> CheckDuplicateTypeNameAsync(string typeName);
public Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId);
public Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec);
public Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec);
public Task<Model_Dao_Result> DeleteSpecAsync(int specId);
public Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId);
public Task<List<string>> GetAllSpecKeysAsync();
public Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync();
public Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId);
public Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId);
public Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part);
public Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part);
public Task<Model_Dao_Result> DeletePartAsync(string partId);
public Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId = null);
public Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads);
public Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end);
public Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync();
public Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid);
public Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load);
public Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid);
public Task<bool> IsPartInventoriedAsync(string partId);
public Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId);
public Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync();
public Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item);
public Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId);
public Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item);
public Task<Model_Dao_Result<int>> GetPartCountByTypeIdAsync(int typeId);
public Task<Model_Dao_Result<int>> GetTransactionCountByPartIdAsync(string partId);
public Task<Model_Dao_Result<int>> GetTransactionCountByTypeIdAsync(int typeId);
public Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(int typeId, string specKey);
public Task<Model_Dao_Result<int>> GetPartCountByTypeAsync(int typeId) => GetPartCountByTypeIdAsync(typeId);
public Task<Model_Dao_Result<int>> GetTransactionCountByTypeAsync(int typeId) => GetTransactionCountByTypeIdAsync(typeId);
public Task<Model_Dao_Result<int>> GetTransactionCountByPartAsync(string partId) => GetTransactionCountByPartIdAsync(partId);
public Task<Model_Dao_Result> InsertCustomFieldAsync(int typeId, Model_CustomFieldDefinition field);
public Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId);
public Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId);
public Task<Model_Dao_Result> UpsertUserPreferenceAsync(string key, string value);
public Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(int count);
```

## File: Module_Dunnage/Data/Dao_DunnageLine.cs
```csharp
public static class Dao_DunnageLine
⋮----
public static void SetErrorHandler(IService_ErrorHandler errorHandler)
⋮----
public static async Task<Model_Dao_Result> InsertDunnageLineAsync(Model_DunnageLine line)
⋮----
string connectionString = Helper_Database_Variables.GetConnectionString(useProduction: true);
⋮----
new MySqlParameter("@p_Line1", line.Line1 ?? string.Empty),
new MySqlParameter("@p_Line2", line.Line2 ?? string.Empty),
new MySqlParameter("@p_PONumber", line.PONumber),
new MySqlParameter("@p_Date", line.Date),
new MySqlParameter("@p_EmployeeNumber", line.EmployeeNumber),
new MySqlParameter("@p_VendorName", line.VendorName ?? "Unknown"),
new MySqlParameter("@p_Location", line.Location ?? string.Empty),
new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
⋮----
return new Model_Dao_Result
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
var errorResult = new Model_Dao_Result
⋮----
await _errorHandler.HandleErrorAsync(
```

## File: Module_Dunnage/Data/Dao_DunnagePart.cs
```csharp
public class Dao_DunnagePart
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetByTypeAsync(int typeId)
⋮----
System.Diagnostics.Debug.WriteLine($"Dao_DunnagePart: GetByTypeAsync called for typeId={typeId}");
⋮----
System.Diagnostics.Debug.WriteLine($"Dao_DunnagePart: GetByTypeAsync returned {result.Data?.Count ?? 0} parts. Success: {result.IsSuccess}");
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnagePart>> GetByIdAsync(string partId)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId, int typeId, string specValues, string homeLocation, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_part_id", partId),
new MySqlParameter("@p_type_id", typeId),
new MySqlParameter("@p_spec_values", specValues),
new MySqlParameter("@p_home_location", homeLocation),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string specValues, string homeLocation, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> CountTransactionsAsync(string partId)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchAsync(string searchText, int? typeId = null)
⋮----
private Model_DunnagePart MapFromReader(IDataReader reader)
⋮----
return new Model_DunnagePart
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
PartId = reader.GetString(reader.GetOrdinal("part_id")),
TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
SpecValues = reader.IsDBNull(reader.GetOrdinal("spec_values")) ? "{}" : reader.GetString(reader.GetOrdinal("spec_values")),
HomeLocation = reader.IsDBNull(reader.GetOrdinal("home_location")) ? string.Empty : reader.GetString(reader.GetOrdinal("home_location")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_DunnageSpec.cs
```csharp
public class Dao_DunnageSpec
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetByTypeAsync(int typeId)
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnageSpec>> GetByIdAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(int typeId, string specKey, string specValue, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_type_id", typeId),
new MySqlParameter("@p_spec_key", specKey),
new MySqlParameter("@p_spec_value", specValue),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string specValue, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteByIdAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result> DeleteByTypeAsync(int typeId)
⋮----
public virtual async Task<Model_Dao_Result<int>> CountPartsUsingSpecAsync(int typeId, string specKey)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("part_count")),
⋮----
public virtual async Task<Model_Dao_Result<List<string>>> GetAllSpecKeysAsync()
⋮----
reader => reader.GetString(reader.GetOrdinal("SpecKey"))
⋮----
private Model_DunnageSpec MapFromReader(IDataReader reader)
⋮----
return new Model_DunnageSpec
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
SpecKey = reader.GetString(reader.GetOrdinal("spec_key")),
SpecValue = reader.IsDBNull(reader.GetOrdinal("spec_value")) ? "{}" : reader.GetString(reader.GetOrdinal("spec_value")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_DunnageUserPreference.cs
```csharp
public class Dao_DunnageUserPreference
⋮----
public virtual async Task<Model_Dao_Result> UpsertAsync(string userId, string key, string value)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(string userId, int count)
⋮----
reader => new Model_IconDefinition
⋮----
IconName = reader.GetString(reader.GetOrdinal("icon_name"))
```

## File: Module_Dunnage/Enums/Enum_DunnageAdminSection.cs
```csharp

```

## File: Module_Dunnage/Models/Model_CustomFieldDefinition.cs
```csharp
public partial class Model_CustomFieldDefinition : ObservableObject
⋮----
private DateTime _createdDate = DateTime.Now;
⋮----
public string GetSummary()
```

## File: Module_Dunnage/Models/Model_DunnageLoad.cs
```csharp
public partial class Model_DunnageLoad : ObservableObject
⋮----
private Guid _loadUuid;
⋮----
if (!string.IsNullOrEmpty(TypeIcon) && Enum.TryParse<MaterialIconKind>(TypeIcon, true, out var kind))
⋮----
private DateTime _receivedDate = DateTime.Now;
⋮----
private DateTime _createdDate = DateTime.Now;
```

## File: Module_Dunnage/Services/Service_MySQL_Dunnage.cs
```csharp
public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
⋮----
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_LoggingUtility _logger;
private readonly IService_UserSessionManager _sessionManager;
private readonly Dao_DunnageLoad _daoDunnageLoad;
private readonly Dao_DunnageType _daoDunnageType;
private readonly Dao_DunnagePart _daoDunnagePart;
private readonly Dao_DunnageSpec _daoDunnageSpec;
private readonly Dao_InventoriedDunnage _daoInventoriedDunnage;
private readonly Dao_DunnageCustomField _daoCustomField;
private readonly Dao_DunnageUserPreference _daoUserPreference;
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync()
⋮----
return await _daoDunnageType.GetAllAsync();
⋮----
public async Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId)
⋮----
return await _daoDunnageType.GetByIdAsync(typeId);
⋮----
public async Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon)
⋮----
return await _daoDunnageType.InsertAsync(typeName, icon, CurrentUser);
⋮----
public async Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type)
⋮----
await _logger.LogInfoAsync($"Inserting new dunnage type: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}");
var result = await _daoDunnageType.InsertAsync(type.TypeName, type.Icon, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully inserted dunnage type '{type.TypeName}' with ID: {type.Id}");
return Model_Dao_Result_Factory.Success();
⋮----
if (result.ErrorMessage.Contains("Duplicate entry"))
⋮----
await _logger.LogWarningAsync($"Failed to insert dunnage type '{type.TypeName}': Duplicate entry");
return Model_Dao_Result_Factory.Failure($"The dunnage type name '{type.TypeName}' is already in use.");
⋮----
await _logger.LogErrorAsync($"Failed to insert dunnage type '{type.TypeName}': {result.ErrorMessage}");
return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
⋮----
await _logger.LogErrorAsync($"Exception in InsertTypeAsync for type '{type.TypeName}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting dunnage type: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type)
⋮----
await _logger.LogInfoAsync($"Updating dunnage type ID {type.Id}: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}");
var result = await _daoDunnageType.UpdateAsync(type.Id, type.TypeName, type.Icon, CurrentUser);
if (!result.IsSuccess && result.ErrorMessage.Contains("Duplicate entry"))
⋮----
await _logger.LogWarningAsync($"Failed to update dunnage type ID {type.Id}: Duplicate entry for '{type.TypeName}'");
⋮----
await _logger.LogInfoAsync($"Successfully updated dunnage type ID {type.Id}: {type.TypeName}");
⋮----
await _logger.LogErrorAsync($"Failed to update dunnage type ID {type.Id}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in UpdateTypeAsync for type ID {type.Id}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating dunnage type: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeleteTypeAsync(int typeId)
⋮----
await _logger.LogInfoAsync($"Attempting to delete dunnage type ID {typeId} by user: {CurrentUser}");
var partsResult = await _daoDunnagePart.GetByTypeAsync(typeId);
⋮----
await _logger.LogWarningAsync($"Cannot delete dunnage type ID {typeId}: Used by {partsResult.Data.Count} parts");
return Model_Dao_Result_Factory.Failure($"Cannot delete type. It is used by {partsResult.Data.Count} parts.");
⋮----
var specsResult = await _daoDunnageSpec.GetByTypeAsync(typeId);
⋮----
await _logger.LogWarningAsync($"Cannot delete dunnage type ID {typeId}: Has {specsResult.Data.Count} specifications defined");
return Model_Dao_Result_Factory.Failure($"Cannot delete type. It has {specsResult.Data.Count} specifications defined. Please delete them first.");
⋮----
var result = await _daoDunnageType.DeleteAsync(typeId);
⋮----
await _logger.LogInfoAsync($"Successfully deleted dunnage type ID {typeId}");
⋮----
await _logger.LogErrorAsync($"Failed to delete dunnage type ID {typeId}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in DeleteTypeAsync for type ID {typeId}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting dunnage type: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<int>> CheckDuplicateTypeNameAsync(string typeName)
⋮----
var result = await _daoDunnageType.CheckDuplicateNameAsync(typeName);
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId)
⋮----
return await _daoDunnageSpec.GetByTypeAsync(typeId);
⋮----
public async Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec)
⋮----
await _logger.LogInfoAsync($"Inserting spec '{spec.SpecKey}' for type ID {spec.TypeId} by user: {CurrentUser}");
var result = await _daoDunnageSpec.InsertAsync(spec.TypeId, spec.SpecKey, spec.SpecValue, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully inserted spec '{spec.SpecKey}' with ID: {spec.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to insert spec '{spec.SpecKey}' for type ID {spec.TypeId}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in InsertSpecAsync for spec '{spec.SpecKey}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting spec: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec)
⋮----
await _logger.LogInfoAsync($"Updating spec ID {spec.Id}: {spec.SpecKey} = {spec.SpecValue} by user: {CurrentUser}");
var result = await _daoDunnageSpec.UpdateAsync(spec.Id, spec.SpecValue, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully updated spec ID {spec.Id}: {spec.SpecKey}");
⋮----
await _logger.LogErrorAsync($"Failed to update spec ID {spec.Id}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in UpdateSpecAsync for spec ID {spec.Id}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating spec: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeleteSpecAsync(int specId)
⋮----
return await _daoDunnageSpec.DeleteByIdAsync(specId);
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting spec: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId)
⋮----
public async Task<List<string>> GetAllSpecKeysAsync()
⋮----
var result = await _daoDunnageSpec.GetAllAsync();
⋮----
.Select(s => s.SpecKey)
.Distinct()
.Order()
.ToList();
⋮----
public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync()
⋮----
return await _daoDunnagePart.GetAllAsync();
⋮----
public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId)
⋮----
_logger.LogInfo($"Service_MySQL_Dunnage: GetPartsByTypeAsync called for typeId={typeId}", "DunnageService");
⋮----
var result = await _daoDunnagePart.GetByTypeAsync(typeId);
_logger.LogInfo($"Service_MySQL_Dunnage: GetPartsByTypeAsync returned {result.Data?.Count ?? 0} parts", "DunnageService");
⋮----
public async Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId)
⋮----
return await _daoDunnagePart.GetByIdAsync(partId);
⋮----
public async Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part)
⋮----
await _logger.LogInfoAsync($"Inserting new dunnage part: {part.PartId} (Type ID: {part.TypeId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}");
var result = await _daoDunnagePart.InsertAsync(part.PartId, part.TypeId, part.SpecValues, part.HomeLocation, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully inserted dunnage part '{part.PartId}' with ID: {part.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to insert dunnage part '{part.PartId}': {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in InsertPartAsync for part '{part.PartId}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part)
⋮----
await _logger.LogInfoAsync($"Updating dunnage part ID {part.Id} (Part ID: {part.PartId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}");
var result = await _daoDunnagePart.UpdateAsync(part.Id, part.SpecValues, part.HomeLocation, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully updated dunnage part ID {part.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to update dunnage part ID {part.Id}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in UpdatePartAsync for part ID {part.Id}: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeletePartAsync(string partId)
⋮----
return Model_Dao_Result_Factory.Failure("Delete part not implemented in DAO yet.");
⋮----
public async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId)
⋮----
result = await _daoDunnagePart.GetByTypeAsync(typeId.Value);
⋮----
result = await _daoDunnagePart.GetAllAsync();
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
var filtered = result.Data.Where(p =>
p.PartId.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
p.DunnageTypeName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
).ToList();
⋮----
public async Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads)
⋮----
await _logger.LogInfoAsync("SaveLoadsAsync called with no loads to save");
⋮----
await _logger.LogInfoAsync($"Saving batch of {loads.Count} dunnage loads by user: {CurrentUser}");
var result = await _daoDunnageLoad.InsertBatchAsync(loads, CurrentUser);
⋮----
var totalQuantity = loads.Sum(l => l.Quantity);
await _logger.LogInfoAsync($"Successfully saved {loads.Count} dunnage loads (Total Quantity: {totalQuantity})");
⋮----
await _logger.LogErrorAsync($"Failed to save {loads.Count} dunnage loads: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in SaveLoadsAsync for {loads?.Count ?? 0} loads: {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error saving loads: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end)
⋮----
return await _daoDunnageLoad.GetByDateRangeAsync(start, end);
⋮----
public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync()
⋮----
return await _daoDunnageLoad.GetAllAsync();
⋮----
public async Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid)
⋮----
if (Guid.TryParse(loadUuid, out var guid))
⋮----
return await _daoDunnageLoad.GetByIdAsync(guid);
⋮----
public async Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load)
⋮----
return Model_Dao_Result_Factory.Failure("Update load not implemented in DAO yet.");
⋮----
public async Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid)
⋮----
return Model_Dao_Result_Factory.Failure("Delete load not implemented in DAO yet.");
⋮----
public async Task<bool> IsPartInventoriedAsync(string partId)
⋮----
var result = await _daoInventoriedDunnage.CheckAsync(partId);
⋮----
public async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId)
⋮----
return await _daoInventoriedDunnage.GetByPartAsync(partId);
⋮----
public async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync()
⋮----
return await _daoInventoriedDunnage.GetAllAsync();
⋮----
public async Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item)
⋮----
await _logger.LogInfoAsync($"Adding part '{item.PartId}' to inventoried list (Method: {item.InventoryMethod}) by user: {CurrentUser}");
var result = await _daoInventoriedDunnage.InsertAsync(item.PartId, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
⋮----
await _logger.LogInfoAsync($"Successfully added part '{item.PartId}' to inventoried list with ID: {item.Id}");
⋮----
await _logger.LogErrorAsync($"Failed to add part '{item.PartId}' to inventoried list: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception in AddToInventoriedListAsync for part '{item.PartId}': {ex.Message}");
⋮----
return Model_Dao_Result_Factory.Failure($"Error adding to inventory list: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId)
⋮----
return Model_Dao_Result_Factory.Failure("Remove from inventory list not implemented in DAO yet.");
⋮----
public async Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item)
⋮----
return await _daoInventoriedDunnage.UpdateAsync(item.Id, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating inventory item: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<int>> GetPartCountByTypeIdAsync(int typeId)
⋮----
return await _daoDunnageType.CountPartsAsync(typeId);
⋮----
public async Task<Model_Dao_Result<int>> GetTransactionCountByPartIdAsync(string partId)
⋮----
return await _daoDunnagePart.CountTransactionsAsync(partId);
⋮----
public async Task<Model_Dao_Result<int>> GetTransactionCountByTypeIdAsync(int typeId)
⋮----
return await _daoDunnageType.CountTransactionsAsync(typeId);
⋮----
public async Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(int typeId, string specKey)
⋮----
return await _daoDunnageSpec.CountPartsUsingSpecAsync(typeId, specKey);
⋮----
public async Task<Model_Dao_Result> InsertCustomFieldAsync(int typeId, Model_CustomFieldDefinition field)
⋮----
var result = await _daoCustomField.InsertAsync(typeId, field, CurrentUser);
⋮----
return Model_Dao_Result_Factory.Failure($"Error inserting custom field: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId)
⋮----
return await _daoCustomField.GetByTypeAsync(typeId);
⋮----
public async Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId)
⋮----
return await _daoCustomField.DeleteAsync(fieldId);
⋮----
return Model_Dao_Result_Factory.Failure($"Error deleting custom field: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpsertUserPreferenceAsync(string key, string value)
⋮----
return await _daoUserPreference.UpsertAsync(CurrentUser, key, value);
⋮----
return Model_Dao_Result_Factory.Failure($"Error saving user preference: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(int count)
⋮----
return await _daoUserPreference.GetRecentlyUsedIconsAsync(CurrentUser, count);
⋮----
private void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
⋮----
_ = _errorHandler.HandleErrorAsync($"Error in {method} ({className}): {ex.Message}", severity, ex);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AddTypeDialogViewModel.cs
```csharp
public partial class ViewModel_Dunnage_AddTypeDialog : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly DispatcherQueue _dispatcherQueue;
⋮----
private MaterialIconKind _selectedIcon = MaterialIconKind.PackageVariantClosed;
⋮----
_dispatcherQueue = DispatcherQueue.GetForCurrentThread();
_validationTimer = _dispatcherQueue.CreateTimer();
_validationTimer.Interval = TimeSpan.FromMilliseconds(300);
⋮----
private async Task SaveTypeAsync()
⋮----
var typeResult = await _dunnageService.InsertTypeAsync(TypeName, SelectedIcon.ToString());
⋮----
await _errorHandler.ShowUserErrorAsync(typeResult.ErrorMessage, "Save Failed", nameof(SaveTypeAsync));
⋮----
var fieldResult = await _dunnageService.InsertCustomFieldAsync(typeId, field);
⋮----
await _errorHandler.ShowUserErrorAsync($"Failed to save field '{field.FieldName}': {fieldResult.ErrorMessage}",
⋮----
await _dunnageService.UpsertUserPreferenceAsync($"RecentIcon_{SelectedIcon}", DateTime.Now.ToString("O"));
⋮----
_errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
⋮----
private void AddField()
⋮----
if (string.IsNullOrWhiteSpace(FieldName) || CustomFields.Count >= 25)
⋮----
var field = new Model_CustomFieldDefinition
⋮----
CustomFields.Add(field);
⋮----
private void EditField(Model_CustomFieldDefinition field)
⋮----
private void DeleteField(Model_CustomFieldDefinition field)
⋮----
CustomFields.Remove(field);
⋮----
partial void OnTypeNameChanged(string value)
⋮----
partial void OnFieldNameChanged(string value)
⋮----
private void OnValidationTimerTick(object? sender, object e)
⋮----
private async void ValidateTypeName()
⋮----
if (string.IsNullOrWhiteSpace(TypeName))
⋮----
var result = await _dunnageService.CheckDuplicateTypeNameAsync(TypeName);
⋮----
DuplicateTypeId = result.Data.ToString();
⋮----
private void ValidateFieldName()
⋮----
if (string.IsNullOrWhiteSpace(FieldName))
⋮----
if (FieldName.Any(c => "<>{}[]|\\".Contains(c)))
⋮----
// Check for duplicate field name
if (CustomFields.Any(f => f.FieldName.Equals(FieldName, StringComparison.OrdinalIgnoreCase) && f != EditingField))
⋮----
private void UpdateCanSave()
⋮----
CanSave = !string.IsNullOrWhiteSpace(TypeName) &&
string.IsNullOrEmpty(TypeNameError) &&
⋮----
private async Task LoadRecentlyUsedIconsAsync()
⋮----
var result = await _dunnageService.GetRecentlyUsedIconsAsync(6);
⋮----
RecentlyUsedIcons.Clear();
⋮----
RecentlyUsedIcons.Add(icon);
⋮----
await _logger.LogErrorAsync($"Failed to load recently used icons: {ex.Message}");
⋮----
public void OnDragItemsCompleted()
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminPartsViewModel.cs
```csharp
public partial class ViewModel_Dunnage_AdminParts : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
private readonly IService_Pagination _paginationService;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
⋮----
private async Task LoadPartsAsync()
⋮----
var typesResult = await _dunnageService.GetAllTypesAsync();
⋮----
AvailableTypes.Clear();
⋮----
AvailableTypes.Add(type);
⋮----
var partsResult = await _dunnageService.GetAllPartsAsync();
⋮----
await _errorHandler.HandleDaoErrorAsync(partsResult, "LoadPartsAsync", true);
⋮----
_paginationService.SetSource(_allParts);
⋮----
await _logger.LogInfoAsync($"Loaded {TotalRecords} dunnage parts", "PartManagement");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private async Task FilterByTypeAsync()
⋮----
var result = await _dunnageService.GetPartsByTypeAsync(SelectedFilterType.Id);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "FilterByTypeAsync", true);
⋮----
private async Task SearchPartsAsync()
⋮----
if (string.IsNullOrWhiteSpace(SearchText))
⋮----
var result = await _dunnageService.SearchPartsAsync(SearchText);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "SearchPartsAsync", true);
⋮----
private async Task ClearFiltersAsync()
⋮----
private void NextPage()
⋮----
if (_paginationService.NextPage())
⋮----
private void PreviousPage()
⋮----
if (_paginationService.PreviousPage())
⋮----
private void FirstPage()
⋮----
if (_paginationService.FirstPage())
⋮----
private void LastPage()
⋮----
if (_paginationService.LastPage())
⋮----
private void LoadPage()
⋮----
Parts.Clear();
⋮----
Parts.Add(item);
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
private void UpdateNavigationButtons()
⋮----
private async Task ShowDeleteConfirmationAsync()
⋮----
var countResult = await _dunnageService.GetTransactionCountByPartAsync(SelectedPart.PartId);
⋮----
await _errorHandler.HandleDaoErrorAsync(countResult, "GetTransactionCountByPartAsync", true);
⋮----
var warningDialog = new ContentDialog
⋮----
XamlRoot = _windowService.GetXamlRoot()
⋮----
await warningDialog.ShowAsync();
⋮----
var confirmDialog = new ContentDialog
⋮----
var result = await confirmDialog.ShowAsync();
⋮----
private async Task DeletePartAsync()
⋮----
var result = await _dunnageService.DeletePartAsync(SelectedPart.PartId);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "DeletePartAsync", true);
⋮----
await _logger.LogInfoAsync($"Deleted part: {SelectedPart.PartId}", "PartManagement");
⋮----
private async Task ReturnToAdminHubAsync()
⋮----
await _adminWorkflow.NavigateToHubAsync();
⋮----
partial void OnSelectedPartChanged(Model_DunnagePart? value)
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_DetailsEntryViewModel.cs
```csharp
public partial class ViewModel_Dunnage_DetailsEntry : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Help _helpService;
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_Dispatcher _dispatcher;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_dispatcher.TryEnqueue(async () =>
⋮----
public async Task LoadSpecsForSelectedPartAsync()
⋮----
_logger.LogError("No type selected in workflow session", null, "DetailsEntry");
SpecInputs.Clear();
⋮----
_logger.LogInfo($"Loading specs for type ID: {selectedTypeId}", "DetailsEntry");
var specsResult = await _dunnageService.GetSpecsForTypeAsync(selectedTypeId);
⋮----
_logger.LogWarning($"No specs found for type {selectedTypeId}: {specsResult.ErrorMessage}", "DetailsEntry");
⋮----
_logger.LogInfo($"Loaded {specs.Count} specs from database", "DetailsEntry");
⋮----
if (selectedPart != null && !string.IsNullOrWhiteSpace(selectedPart.SpecValues))
⋮----
_logger.LogInfo($"Loaded spec values from part: {selectedPart.SpecValues}", "DetailsEntry");
⋮----
_logger.LogError($"Failed to parse part spec values: {ex.Message}", ex, "DetailsEntry");
⋮----
TextSpecs.Clear();
NumberSpecs.Clear();
BooleanSpecs.Clear();
⋮----
var specType = specValueDict.ContainsKey("type") ? specValueDict["type"]?.ToString()?.ToLowerInvariant() ?? "text" : "text";
⋮----
if (!string.IsNullOrWhiteSpace(defaultValue))
⋮----
typedValue = double.Parse(defaultValue);
⋮----
typedValue = bool.Parse(defaultValue);
⋮----
_logger.LogWarning($"Failed to convert value '{defaultValue}' to type '{specType}': {ex.Message}", "DetailsEntry");
⋮----
var input = new Model_SpecInput
⋮----
Unit = specValueDict.ContainsKey("unit") ? specValueDict["unit"]?.ToString() : null,
IsRequired = specValueDict.ContainsKey("required") && bool.Parse(specValueDict["required"]?.ToString() ?? "false"),
⋮----
SpecInputs.Add(input);
var typeFromDb = specValueDict.ContainsKey("type") ? specValueDict["type"]?.ToString() ?? "null" : "not found";
_logger.LogInfo($"Processing spec: {spec.SpecKey}, Type from DB: {typeFromDb}, Normalized: {specType}", "DetailsEntry");
⋮----
BooleanSpecs.Add(input);
_logger.LogInfo($"Added {spec.SpecKey} to BooleanSpecs", "DetailsEntry");
⋮----
NumberSpecs.Add(input);
_logger.LogInfo($"Added {spec.SpecKey} to NumberSpecs", "DetailsEntry");
⋮----
TextSpecs.Add(input);
_logger.LogInfo($"Added {spec.SpecKey} to TextSpecs", "DetailsEntry");
⋮----
_logger.LogInfo($"Created {SpecInputs.Count} spec input controls (Text: {TextSpecs.Count}, Number: {NumberSpecs.Count}, Boolean: {BooleanSpecs.Count})", "DetailsEntry");
⋮----
var isInventoried = await _dunnageService.IsPartInventoriedAsync(selectedPart.PartId);
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private Dictionary<string, object> ParseSpecValue(string specValue)
⋮----
if (string.IsNullOrWhiteSpace(specValue))
⋮----
_logger.LogError($"Failed to parse spec value JSON: {ex.Message}", ex, "DetailsEntry");
⋮----
partial void OnPoNumberChanged(string value)
⋮----
if (string.IsNullOrWhiteSpace(value))
⋮----
private void UpdateInventoryMessage()
⋮----
private bool ValidateInputs()
⋮----
foreach (var spec in SpecInputs.Where(s => s.IsRequired))
⋮----
if (spec.Value == null || string.IsNullOrWhiteSpace(spec.Value.ToString()))
⋮----
private void GoBack()
⋮----
_logger.LogInfo("Navigating back to Quantity Entry", "DetailsEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
⋮----
private async Task GoNextAsync()
⋮----
var specValues = SpecInputs.ToDictionary(
⋮----
_logger.LogInfo("Details saved, navigating to Review", "DetailsEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.Review);
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_EditModeViewModel.cs
```csharp
public partial class ViewModel_Dunnage_EditMode : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_Pagination _paginationService;
private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
FromDate = DateTimeOffset.Now.AddDays(-7);
⋮----
public string LastWeekButtonText => $"Last Week ({DateTime.Now.Date.AddDays(-7):MMM d} - {DateTime.Now.Date:MMM d})";
⋮----
var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
⋮----
startOfWeek = startOfWeek.AddDays(-7);
⋮----
var quarterStart = new DateTime(today.Year, startMonth, 1);
⋮----
private async Task LoadFromCurrentMemoryAsync()
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
_logger.LogInfo("No unsaved loads found in current session", "EditMode");
⋮----
_allLoads = _workflowService.CurrentSession.Loads.ToList();
⋮----
_paginationService.SetSource(_allLoads);
⋮----
_logger.LogInfo($"Loaded {TotalRecords} loads from current session", "EditMode");
⋮----
private async Task LoadFromCurrentLabelsAsync()
⋮----
var localPath = System.IO.Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
⋮----
if (!System.IO.File.Exists(localPath))
⋮----
_logger.LogWarning($"CSV file not found at {localPath}", "EditMode");
⋮----
csv.Read();
csv.ReadHeader();
while (csv.Read())
⋮----
var load = new Model_DunnageLoad
⋮----
ReceivedDate = DateTime.TryParse(csv.GetField<string>("Date"), out var date) ? date : DateTime.Now
⋮----
loadsList.Add(load);
⋮----
_logger.LogWarning($"Failed to parse line {lineNumber}: {rowEx.Message}", "EditMode");
⋮----
_logger.LogInfo($"Loaded {TotalRecords} loads from CSV file", "EditMode");
⋮----
private async Task LoadFromHistoryAsync()
⋮----
var startDate = FromDate?.DateTime ?? DateTime.Now.AddDays(-7);
⋮----
var result = await _dunnageService.GetLoadsByDateRangeAsync(startDate, endDate);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "LoadFromHistoryAsync", true);
⋮----
_logger.LogInfo($"Loaded {TotalRecords} historical loads from {startDate:d} to {endDate:d}", "EditMode");
⋮----
private async Task SetFilterLastWeekAsync()
⋮----
FromDate = DateTime.Now.Date.AddDays(-7);
⋮----
private async Task SetFilterTodayAsync()
⋮----
private async Task SetFilterThisWeekAsync()
⋮----
private async Task SetFilterThisMonthAsync()
⋮----
FromDate = new DateTime(today.Year, today.Month, 1);
⋮----
private async Task SetFilterThisQuarterAsync()
⋮----
FromDate = new DateTime(today.Year, startMonth, 1);
⋮----
private async Task SetFilterShowAllAsync()
⋮----
FromDate = DateTime.Now.Date.AddYears(-1);
⋮----
private void SetDateRangeToday()
⋮----
private void SetDateRangeLastWeek()
⋮----
private void SetDateRangeLastMonth()
⋮----
FromDate = DateTime.Now.Date.AddMonths(-1);
⋮----
private void FirstPage()
⋮----
private void PreviousPage()
⋮----
private void NextPage()
⋮----
private void LastPage()
⋮----
private void LoadPage(int pageNumber)
⋮----
_paginationService.GoToPage(pageNumber);
⋮----
FilteredLoads.Clear();
⋮----
FilteredLoads.Add(load);
⋮----
_logger.LogInfo($"Loaded page {CurrentPage} of {TotalPages}", "EditMode");
⋮----
private void SelectAll()
⋮----
SelectedLoads.Clear();
⋮----
SelectedLoads.Add(load);
⋮----
_logger.LogInfo($"Selected all {FilteredLoads.Count} loads on page", "EditMode");
⋮----
private void RemoveSelectedRows()
⋮----
var loadsToRemove = SelectedLoads.ToList();
⋮----
FilteredLoads.Remove(load);
_allLoads.Remove(load);
⋮----
_logger.LogInfo($"Removed {loadsToRemove.Count} loads", "EditMode");
⋮----
private async Task SaveAllAsync()
⋮----
if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
⋮----
var saveResult = await _dunnageService.SaveLoadsAsync(_allLoads);
⋮----
await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(_allLoads);
⋮----
_logger.LogInfo($"Saved {_allLoads.Count} loads", "EditMode");
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null", null, "EditMode");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, clearing data", "EditMode");
⋮----
_allLoads.Clear();
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to return to mode selection: {ex.Message}", ex, "EditMode");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex, true);
⋮----
_logger.LogInfo("User cancelled return to mode selection", "EditMode");
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
_logger.LogInfo($"Page changed to {CurrentPage} of {TotalPages}", "EditMode");
⋮----
private void UpdateCanSave()
⋮----
CanSave = _allLoads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_ManualEntryViewModel.cs
```csharp
public partial class ViewModel_Dunnage_ManualEntry : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
public async Task InitializeAsync()
⋮----
var typesResult = await _dunnageService.GetAllTypesAsync();
⋮----
AvailableTypes.Clear();
⋮----
AvailableTypes.Add(type);
⋮----
var partsResult = await _dunnageService.GetAllPartsAsync();
⋮----
AvailableParts.Clear();
⋮----
AvailableParts.Add(part);
⋮----
_logger.LogInfo("Manual Entry initialized", "ManualEntry");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private void AddRow()
⋮----
var newLoad = new Model_DunnageLoad
⋮----
Loads.Add(newLoad);
⋮----
_logger.LogInfo($"Added new row, total: {Loads.Count}", "ManualEntry");
⋮----
private async Task AddMultipleRowsAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null", null, "ManualEntry");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error, null, true);
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogInfo($"Added {count} rows via dialog, total: {Loads.Count}", "ManualEntry");
⋮----
private void RemoveRow()
⋮----
Loads.Remove(SelectedLoad);
SelectedLoad = Loads.LastOrDefault();
⋮----
_logger.LogInfo($"Removed row, total: {Loads.Count}", "ManualEntry");
⋮----
private void FillBlankSpaces()
⋮----
var lastLoad = Loads.LastOrDefault();
⋮----
if (string.IsNullOrWhiteSpace(load.PoNumber) && !string.IsNullOrWhiteSpace(lastLoad.PoNumber))
⋮----
if (string.IsNullOrWhiteSpace(load.Location) && !string.IsNullOrWhiteSpace(lastLoad.Location))
⋮----
_logger.LogInfo("Fill blank spaces executed", "ManualEntry");
⋮----
private void SortForPrinting()
⋮----
.OrderBy(l => l.PartId)
.ThenBy(l => l.PoNumber)
.ThenBy(l => l.TypeName)
.ToList();
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
_logger.LogInfo("Sort for printing executed", "ManualEntry");
⋮----
private async Task AutoFillAsync()
⋮----
if (string.IsNullOrWhiteSpace(SelectedLoad.PartId))
⋮----
var partResult = await _dunnageService.GetPartByIdAsync(SelectedLoad.PartId);
⋮----
var type = AvailableTypes.FirstOrDefault(t => t.Id == part.TypeId);
⋮----
_logger.LogInfo($"Auto-filled {part.SpecValuesDict.Count} spec values for Part ID: {SelectedLoad.PartId}", "ManualEntry");
⋮----
if (string.IsNullOrWhiteSpace(SelectedLoad.InventoryMethod))
⋮----
SelectedLoad.InventoryMethod = string.IsNullOrWhiteSpace(SelectedLoad.PoNumber) ? "Adjust In" : "Receive In";
⋮----
_logger.LogInfo($"Auto-fill completed for Part ID: {SelectedLoad.PartId}", "ManualEntry");
⋮----
private async Task SaveToHistoryAsync()
⋮----
var result = await _dunnageService.SaveLoadsAsync(Loads.ToList());
⋮----
_logger.LogInfo($"Saved {Loads.Count} loads to history", "ManualEntry");
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "SaveToHistoryAsync", true);
⋮----
private async Task SaveAllAsync()
⋮----
if (string.IsNullOrWhiteSpace(load.TypeName) || string.IsNullOrWhiteSpace(load.PartId))
⋮----
var saveResult = await _dunnageService.SaveLoadsAsync(Loads.ToList());
⋮----
await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(Loads.ToList());
⋮----
_logger.LogInfo($"Saved {Loads.Count} loads", "ManualEntry");
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, clearing loads", "ManualEntry");
⋮----
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to return to mode selection: {ex.Message}", ex, "ManualEntry");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex, true);
⋮----
_logger.LogInfo("User cancelled return to mode selection", "ManualEntry");
⋮----
private async Task LoadSpecColumnsAsync()
⋮----
var specKeys = await _dunnageService.GetAllSpecKeysAsync();
⋮----
_logger.LogInfo($"Loaded {SpecColumnHeaders.Count} dynamic spec columns", "ManualEntry");
⋮----
_logger.LogError($"Failed to load spec columns: {ex.Message}", ex, "ManualEntry");
⋮----
public async Task OnPartIdChangedAsync(Model_DunnageLoad load)
⋮----
if (load == null || string.IsNullOrWhiteSpace(load.PartId))
⋮----
var partResult = await _dunnageService.GetPartByIdAsync(load.PartId);
⋮----
_logger.LogInfo($"Auto-populated data for Part ID: {load.PartId}", "ManualEntry");
⋮----
_logger.LogError($"Error auto-populating part data: {ex.Message}", ex, "ManualEntry");
⋮----
private void UpdateCanSave()
⋮----
CanSave = Loads.Any(l => !string.IsNullOrWhiteSpace(l.TypeName) || !string.IsNullOrWhiteSpace(l.PartId));
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_PartSelectionViewModel.cs
```csharp
public partial class ViewModel_Dunnage_PartSelection : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService; private readonly IService_Help _helpService; private readonly IService_Dispatcher _dispatcher;
⋮----
_logger.LogInfo("PartSelection: ViewModel constructed and subscribed to StepChanged", "PartSelection");
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_logger.LogInfo($"PartSelection: Workflow step changed to {_workflowService.CurrentStep}", "PartSelection");
⋮----
_logger.LogInfo("PartSelection: Step is PartSelection, calling InitializeAsync via dispatcher", "PartSelection");
_dispatcher.TryEnqueue(async () =>
⋮----
if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse<MaterialIconKind>(SelectedTypeIcon, true, out var kind))
⋮----
public async Task InitializeAsync()
⋮----
_logger.LogInfo("PartSelection: InitializeAsync called", "PartSelection");
⋮----
_logger.LogInfo("PartSelection: InitializeAsync returning because IsBusy is true", "PartSelection");
⋮----
_logger.LogInfo($"PartSelection: SelectedTypeId={SelectedTypeId}, SelectedTypeName={SelectedTypeName}, SelectedTypeIcon={SelectedTypeIcon}", "PartSelection");
⋮----
_logger.LogInfo($"PartSelection: {StatusMessage}", "PartSelection");
⋮----
_logger.LogError($"PartSelection: Failed to initialize: {ex.Message}", ex, "PartSelection");
await _errorHandler.HandleErrorAsync(
⋮----
private async Task LoadPartsAsync()
⋮----
_logger.LogInfo($"PartSelection: LoadPartsAsync called with SelectedTypeId={SelectedTypeId}", "PartSelection");
⋮----
_logger.LogInfo("PartSelection: SelectedTypeId is 0, returning from LoadPartsAsync", "PartSelection");
⋮----
_logger.LogInfo($"PartSelection: Calling _dunnageService.GetPartsByTypeAsync({SelectedTypeId})", "PartSelection");
var result = await _dunnageService.GetPartsByTypeAsync(SelectedTypeId);
⋮----
AvailableParts.Clear();
⋮----
AvailableParts.Add(part);
⋮----
_logger.LogInfo($"PartSelection: Successfully loaded {AvailableParts.Count} parts", "PartSelection");
⋮----
_logger.LogWarning($"PartSelection: Failed to load parts: {result.ErrorMessage}", "PartSelection");
await _errorHandler.HandleDaoErrorAsync(
⋮----
_logger.LogError($"PartSelection: Error in LoadPartsAsync: {ex.Message}", ex, "PartSelection");
⋮----
partial void OnSelectedPartChanged(Model_DunnagePart? oldValue, Model_DunnagePart? newValue)
⋮----
_logger.LogInfo($"Part selected via ComboBox: {newValue.PartId}", "PartSelection");
⋮----
_logger.LogInfo($"Updated workflow session with selected part: {newValue.PartId}", "PartSelection");
⋮----
SelectPartCommand.NotifyCanExecuteChanged();
⋮----
private async Task CheckInventoryStatusAsync(Model_DunnagePart part)
⋮----
var isInventoried = await _dunnageService.IsPartInventoriedAsync(part.PartId);
⋮----
_logger.LogInfo($"Part {part.PartId} is inventoried - showing notification", "PartSelection");
⋮----
_logger.LogError($"Error checking inventory status: {ex.Message}", ex, "PartSelection");
⋮----
private void UpdateInventoryMessage()
⋮----
private void GoBack()
⋮----
_logger.LogInfo("Returning to Type Selection", "PartSelection");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
⋮----
private async Task SelectPartAsync()
⋮----
_logger.LogInfo($"Selected part: {SelectedPart.PartId}", "PartSelection");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
⋮----
private async Task QuickAddPartAsync()
⋮----
_logger.LogInfo($"Quick Add Part requested for type {SelectedTypeName}", "PartSelection");
var specsResult = await _dunnageService.GetSpecsForTypeAsync(SelectedTypeId);
⋮----
_logger.LogInfo("Cannot show dialog: XamlRoot is null", "PartSelection");
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogInfo($"Adding new part: {partId} for type {SelectedTypeName}", "PartSelection");
var newPart = new Model_DunnagePart
⋮----
var insertResult = await _dunnageService.InsertPartAsync(newPart);
⋮----
_logger.LogInfo($"Successfully added part: {partId}", "PartSelection");
⋮----
var addedPart = AvailableParts.FirstOrDefault(p => p.PartId == partId);
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/Views/View_Dunnage_AdminInventoryView.xaml.cs
```csharp
public sealed partial class View_Dunnage_AdminInventoryView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
protected override async void OnNavigatedTo(NavigationEventArgs e)
⋮----
base.OnNavigatedTo(e);
App.GetService<IService_LoggingUtility>().LogInfo("Admin Inventory View loaded", "AdminInventoryView");
await ViewModel.InitializeAsync();
```

## File: Module_Dunnage/Views/View_Dunnage_AdminTypesView.xaml.cs
```csharp
public sealed partial class View_Dunnage_AdminTypesView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnPageLoaded(object sender, RoutedEventArgs e)
⋮----
App.GetService<IService_LoggingUtility>().LogInfo("Admin Types View loaded", "AdminTypesView");
await ViewModel.LoadTypesCommand.ExecuteAsync(null);
```

## File: Module_Dunnage/Views/View_Dunnage_Control_IconPickerControl.xaml.cs
```csharp
public sealed partial class View_Dunnage_Control_IconPickerControl : UserControl
⋮----
public static readonly DependencyProperty SelectedIconProperty =
DependencyProperty.Register(
⋮----
new PropertyMetadata(MaterialIconKind.PackageVariantClosed, OnSelectedIconChanged));
public static readonly DependencyProperty RecentlyUsedIconsProperty =
⋮----
new PropertyMetadata(default(ObservableCollection<Model_IconDefinition>)));
⋮----
_allIcons = Helper_MaterialIcons.GetAllIcons();
_filteredIcons = new ObservableCollection<MaterialIconKind>(_allIcons.Take(200));
⋮----
private static void OnSelectedIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
⋮----
var recent = control.RecentlyUsedIcons.FirstOrDefault(x => x.Kind == kind);
⋮----
private void OnRecentIconSelectionChanged(object sender, SelectionChangedEventArgs e)
⋮----
private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
⋮----
_filteredIcons.Clear();
var matches = Helper_MaterialIcons.SearchIcons(searchText).Take(200);
⋮----
_filteredIcons.Add(icon);
```

## File: Module_Dunnage/Views/View_Dunnage_TypeSelectionView.xaml
```
<UserControl x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_dunnage_typeselectionView"
             x:Name="RootUserControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
             xmlns:models="using:MTM_Receiving_Application.Module_Dunnage.Models"
             xmlns:materialIcons="using:Material.Icons.WinUI3"
             xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
             mc:Ignorable="d"
             Loaded="OnLoaded">

    <UserControl.Resources>
        <converters:Converter_IconCodeToGlyph x:Key="IconCodeToGlyphConverter" />
    </UserControl.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0"
                    Spacing="8"
                    Margin="0,0,0,32">
            <TextBlock Text="Select Dunnage Type"
                       Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.GetTip('Dunnage.TypeSelection'), Mode=OneWay}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       Style="{StaticResource BodyTextBlockStyle}" />
        </StackPanel>

        <!-- 3x3 Type Grid -->
        <GridView Grid.Row="1"
                  ItemsSource="{x:Bind ViewModel.DisplayedTypes, Mode=OneWay}"
                  SelectedItem="{x:Bind ViewModel.SelectedType, Mode=TwoWay}"
                  SelectionMode="None"
                  IsItemClickEnabled="True"
                  ItemClick="TypeCard_ItemClick"
                  MaxWidth="960"
                  HorizontalAlignment="Center">
            <GridView.ItemsPanel>
                <ItemsPanelTemplate>
                    <ItemsWrapGrid Orientation="Horizontal"
                                   MaximumRowsOrColumns="3" />
                </ItemsPanelTemplate>
            </GridView.ItemsPanel>
            <GridView.ItemTemplate>
                <DataTemplate x:DataType="models:Model_DunnageType">
                    <Border Width="280"
                            Height="180"
                            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                            BorderThickness="1"
                            CornerRadius="8"
                            Padding="20">
                        <Border.ContextFlyout>
                            <MenuFlyout>
                                <MenuFlyoutItem Text="Edit"
                                                Icon="Edit"
                                                Command="{Binding ElementName=RootUserControl, Path=ViewModel.EditTypeCommand}"
                                                CommandParameter="{x:Bind}" />
                                <MenuFlyoutItem Text="Delete"
                                                Icon="Delete"
                                                Command="{Binding ElementName=RootUserControl, Path=ViewModel.DeleteTypeCommand}"
                                                CommandParameter="{x:Bind}" />
                            </MenuFlyout>
                        </Border.ContextFlyout>
                        <Grid>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto" />
                                <RowDefinition Height="*" />
                                <RowDefinition Height="Auto" />
                            </Grid.RowDefinitions>

                            <!-- Icon -->
                            <materialIcons:MaterialIcon Grid.Row="0"
                                                        Kind="{x:Bind IconKind, Mode=OneWay}"
                                                        Width="48"
                                                        Height="48"
                                                        Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                                                        HorizontalAlignment="Center"
                                                        Margin="0,0,0,12" />

                            <!-- Type Name -->
                            <TextBlock Grid.Row="1"
                                       Text="{x:Bind TypeName}"
                                       Style="{StaticResource SubtitleTextBlockStyle}"
                                       HorizontalAlignment="Center"
                                       VerticalAlignment="Center"
                                       TextAlignment="Center"
                                       TextWrapping="Wrap" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </GridView.ItemTemplate>
        </GridView>

        <!-- Pagination Controls -->
        <StackPanel Grid.Row="2"
                    Orientation="Horizontal"
                    HorizontalAlignment="Center"
                    Spacing="16"
                    Margin="0,32,0,0">
            <!-- First Page -->
            <Button Command="{x:Bind ViewModel.FirstPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FirstPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE892;"
                          FontSize="16" />
            </Button>

            <!-- Previous Page -->
            <Button Command="{x:Bind ViewModel.PreviousPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.PreviousPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE76B;"
                          FontSize="16" />
            </Button>

            <!-- Page Info -->
            <Border Background="{ThemeResource LayerFillColorDefaultBrush}"
                    Padding="16,8"
                    CornerRadius="4"
                    MinWidth="120">
                <TextBlock Text="{x:Bind ViewModel.PageInfo, Mode=OneWay}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center" />
            </Border>

            <!-- Next Page -->
            <Button Command="{x:Bind ViewModel.NextPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.NextPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE76C;"
                          FontSize="16" />
            </Button>

            <!-- Last Page -->
            <Button Command="{x:Bind ViewModel.LastPageCommand}"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LastPage'), Mode=OneWay}">
                <FontIcon Glyph="&#xE893;"
                          FontSize="16" />
            </Button>

            <!-- Quick Add Button -->
            <Button Command="{x:Bind ViewModel.QuickAddTypeCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    Margin="32,0,0,0">
                <StackPanel Orientation="Horizontal"
                            Spacing="8">
                    <FontIcon Glyph="&#xE710;"
                              FontSize="16" />
                    <TextBlock Text="Quick Add Type" />
                </StackPanel>
            </Button>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Dunnage/Views/View_Dunnage_TypeSelectionView.xaml.cs
```csharp
public sealed partial class View_dunnage_typeselectionView : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
System.Diagnostics.Debug.WriteLine("dunnage_typeselectionView: OnLoaded called");
await ViewModel.InitializeAsync();
⋮----
private async void TypeCard_ItemClick(object sender, ItemClickEventArgs e)
⋮----
await ViewModel.SelectTypeCommand.ExecuteAsync(type);
```

## File: Module_Dunnage/Data/Dao_DunnageCustomField.cs
```csharp
public class Dao_DunnageCustomField
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(int typeId, Model_CustomFieldDefinition field, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_type_id", typeId),
new MySqlParameter("@p_field_name", field.FieldName),
new MySqlParameter("@p_field_type", field.FieldType),
new MySqlParameter("@p_display_order", field.DisplayOrder),
new MySqlParameter("@p_is_required", field.IsRequired),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetByTypeAsync(int typeId)
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int fieldId)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
private Model_CustomFieldDefinition MapFromReader(IDataReader reader)
⋮----
return new Model_CustomFieldDefinition
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
FieldName = reader.GetString(reader.GetOrdinal("field_name")),
FieldType = reader.GetString(reader.GetOrdinal("field_type")),
DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
IsRequired = reader.GetBoolean(reader.GetOrdinal("is_required"))
```

## File: Module_Dunnage/Data/Dao_DunnageLoad.cs
```csharp
public class Dao_DunnageLoad
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnageLoad>> GetByIdAsync(Guid loadUuid)
⋮----
{ "load_uuid", loadUuid.ToString() }
⋮----
public virtual async Task<Model_Dao_Result> InsertAsync(Guid loadUuid, string partId, decimal quantity, string user)
⋮----
{ "load_uuid", loadUuid.ToString() },
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> InsertBatchAsync(List<Model_DunnageLoad> loads, string user)
⋮----
var uniquePartIds = loads.Select(l => l.PartId).Distinct().ToList();
var daoPart = new Dao_DunnagePart(_connectionString);
⋮----
var partResult = await daoPart.GetByIdAsync(partId);
⋮----
invalidParts.Add(partId);
⋮----
return new Model_Dao_Result
⋮----
ErrorMessage = $"Cannot save loads: The following Part ID(s) have not been registered in the system: {string.Join(", ", invalidParts)}. " +
⋮----
loadData.Add(new
⋮----
load_uuid = load.LoadUuid.ToString(),
⋮----
string json = JsonSerializer.Serialize(loadData);
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
if (result.ErrorMessage.Contains("FK_dunnage_history_part_id") ||
result.ErrorMessage.Contains("foreign key constraint"))
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(Guid loadUuid, decimal quantity, string user)
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(Guid loadUuid)
⋮----
private Model_DunnageLoad MapFromReader(IDataReader reader)
⋮----
return new Model_DunnageLoad
⋮----
LoadUuid = (Guid)reader[reader.GetOrdinal("load_uuid")],
PartId = reader.GetString(reader.GetOrdinal("part_id")),
Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_DunnageType.cs
```csharp
public class Dao_DunnageType
⋮----
public virtual async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllAsync()
⋮----
Console.WriteLine($"[Dao_DunnageType] GetAllAsync called");
Console.WriteLine($"[Dao_DunnageType] Connection string: {_connectionString}");
⋮----
Console.WriteLine($"[Dao_DunnageType] Result - IsSuccess: {result.IsSuccess}, Data Count: {result.Data?.Count ?? 0}, Error: {result.ErrorMessage}");
⋮----
public virtual async Task<Model_Dao_Result<Model_DunnageType>> GetByIdAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(string typeName, string icon, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_type_name", typeName),
new MySqlParameter("@p_icon", icon),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string typeName, string icon, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
public virtual async Task<Model_Dao_Result<int>> CountPartsAsync(int typeId)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("part_count")),
⋮----
public virtual async Task<Model_Dao_Result<int>> CountTransactionsAsync(int typeId)
⋮----
reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
⋮----
public virtual async Task<Model_Dao_Result<bool>> CheckDuplicateNameAsync(string typeName, int? excludeId = null)
⋮----
var pExists = new MySqlParameter("@p_exists", MySqlDbType.Bit)
⋮----
new MySqlParameter("@p_exclude_id", excludeId.HasValue ? (object)excludeId.Value : DBNull.Value),
⋮----
return Model_Dao_Result_Factory.Success<bool>(Convert.ToBoolean(pExists.Value));
⋮----
private Model_DunnageType MapFromReader(IDataReader reader)
⋮----
return new Model_DunnageType
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
TypeName = reader.GetString(reader.GetOrdinal("type_name")),
Icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? "Help" : reader.GetString(reader.GetOrdinal("icon")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Data/Dao_InventoriedDunnage.cs
```csharp
public class Dao_InventoriedDunnage
⋮----
public virtual async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllAsync()
⋮----
public virtual async Task<Model_Dao_Result<bool>> CheckAsync(string partId)
⋮----
reader => reader.GetBoolean(reader.GetOrdinal("requires_inventory")),
⋮----
public virtual async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetByPartAsync(string partId)
⋮----
public virtual async Task<Model_Dao_Result<int>> InsertAsync(string partId, string inventoryMethod, string notes, string user)
⋮----
var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
new MySqlParameter("@p_part_id", partId),
new MySqlParameter("@p_inventory_method", inventoryMethod),
new MySqlParameter("@p_notes", notes),
new MySqlParameter("@p_user", user),
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
⋮----
return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
⋮----
public virtual async Task<Model_Dao_Result> UpdateAsync(int id, string inventoryMethod, string notes, string user)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
⋮----
private Model_InventoriedDunnage MapFromReader(IDataReader reader)
⋮----
return new Model_InventoriedDunnage
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
PartId = reader.GetString(reader.GetOrdinal("part_id")),
InventoryMethod = reader.IsDBNull(reader.GetOrdinal("inventory_method")) ? null : reader.GetString(reader.GetOrdinal("inventory_method")),
Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by")) ? null : reader.GetString(reader.GetOrdinal("modified_by")),
ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? null : reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Dunnage/Services/Service_DunnageAdminWorkflow.cs
```csharp
public class Service_DunnageAdminWorkflow : IService_DunnageAdminWorkflow
⋮----
private Enum_DunnageAdminSection _currentSection = Enum_DunnageAdminSection.Hub;
⋮----
public async Task NavigateToSectionAsync(Enum_DunnageAdminSection section)
⋮----
public async Task NavigateToHubAsync()
⋮----
public Task<bool> CanNavigateAwayAsync()
⋮----
return Task.FromResult(true);
⋮----
return Task.FromResult(false);
⋮----
public void MarkDirty()
⋮----
public void MarkClean()
```

## File: Module_Dunnage/Services/Service_DunnageCSVWriter.cs
```csharp
public class Service_DunnageCSVWriter : IService_DunnageCSVWriter
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
private CsvConfiguration GetRfc4180Configuration()
⋮----
return new CsvConfiguration(CultureInfo.InvariantCulture)
⋮----
ShouldQuote = args => !string.IsNullOrEmpty(args.Field) &&
(args.Field.Contains(",") ||
args.Field.Contains("\"") ||
args.Field.Contains("\n") ||
args.Field.Contains("\r")),
⋮----
public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads)
⋮----
public async Task<Model_CSVWriteResult> WriteToCsvAsync(List<Model_DunnageLoad> loads, string typeName)
⋮----
await _logger.LogWarningAsync("WriteToCsvAsync called with no loads to export");
return new Model_CSVWriteResult { ErrorMessage = "No loads to export." };
⋮----
await _logger.LogInfoAsync($"Starting CSV export for {loads.Count} loads of type '{typeName}'");
var specKeys = await _dunnageService.GetAllSpecKeysAsync();
specKeys.Sort();
⋮----
dynamic record = new ExpandoObject();
⋮----
if (load.Specs != null && load.Specs.TryGetValue(key, out object? value))
⋮----
dict[key] = ""; // FR-043: Empty string if missing
⋮----
records.Add(record);
⋮----
// Write to local path
⋮----
await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to local CSV: {localPath}");
⋮----
// Ensure directory exists for network path
var networkDir = Path.GetDirectoryName(networkPath);
if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
⋮----
Directory.CreateDirectory(networkDir);
⋮----
await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to network CSV: {networkPath}");
⋮----
await _logger.LogErrorAsync($"Failed to write to network path '{networkPath}': {ex.Message}");
// FR-044: Network failure graceful handling
⋮----
return new Model_CSVWriteResult
⋮----
await _logger.LogErrorAsync($"CSV export failed for type '{typeName}': {ex.Message}");
await _errorHandler.HandleErrorAsync($"Export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
return new Model_CSVWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
⋮----
/// <summary>
/// Write dunnage loads to CSV with dynamic columns for all spec keys
/// Used for Manual Entry and Edit Mode exports (all types in one file)
/// </summary>
/// <param name="loads"></param>
public async Task<Model_CSVWriteResult> WriteDynamicCsvAsync(
⋮----
await _logger.LogWarningAsync("WriteDynamicCsvAsync called with no loads to export");
⋮----
await _logger.LogInfoAsync($"Starting dynamic CSV export for {loads.Count} loads");
var specKeys = allSpecKeys ?? await _dunnageService.GetAllSpecKeysAsync();
⋮----
dict[key] = ""; // Blank cell for specs not applicable to this load's type
⋮----
// Generate filename if not provided
⋮----
await _logger.LogWarningAsync("Network path not available for CSV export");
⋮----
await _errorHandler.HandleErrorAsync($"Dynamic CSV export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
⋮----
public async Task<Model_CSVWriteResult> ExportSelectedLoadsAsync(
⋮----
return new Model_CSVWriteResult { ErrorMessage = "No loads selected for export." };
⋮----
specKeys = await _dunnageService.GetAllSpecKeysAsync();
⋮----
.Where(l => l.TypeId.HasValue)
.Select(l => l.TypeId!.Value)
.Distinct()
.ToList();
⋮----
var specs = await _dunnageService.GetSpecsForTypeAsync(typeId);
⋮----
var keys = specs.Data.Select(s => s.SpecKey).Distinct();
specKeys.AddRange(keys);
⋮----
specKeys = specKeys.Distinct().ToList();
⋮----
await _errorHandler.HandleErrorAsync($"Selected loads export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
⋮----
public async Task<bool> IsNetworkPathAvailableAsync(int timeout = 3)
⋮----
return await Task.Run(() =>
⋮----
return Directory.Exists(networkRoot);
⋮----
public string GetLocalCsvPath(string filename)
⋮----
var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var folder = Path.Combine(appData, "MTM_Receiving_Application");
if (!Directory.Exists(folder))
⋮----
Directory.CreateDirectory(folder);
⋮----
return Path.Combine(folder, filename);
⋮----
public string GetNetworkCsvPath(string filename)
⋮----
private async Task WriteCsvFileAsync(string path, IEnumerable<dynamic> records)
⋮----
var encoding = new UTF8Encoding(true);
await using (var writer = new StreamWriter(path, false, encoding))
await using (var csv = new CsvWriter(writer, GetRfc4180Configuration()))
⋮----
await csv.WriteRecordsAsync(records);
⋮----
private string FormatDateTime(DateTime value)
⋮----
return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
⋮----
public async Task<Model_CSVDeleteResult> ClearCSVFilesAsync(string? filenamePattern = null)
⋮----
var result = new Model_CSVDeleteResult();
⋮----
await _logger.LogInfoAsync($"Starting CSV file clearing with pattern: {filenamePattern ?? "all"}");
// Clear local files
⋮----
var localFolder = Path.GetDirectoryName(GetLocalCsvPath("dummy.csv"));
if (Directory.Exists(localFolder))
⋮----
var filesToClear = Directory.GetFiles(localFolder, filenamePattern ?? "*.csv");
⋮----
await using var writer = new StreamWriter(file, false, new UTF8Encoding(true));
await using var csv = new CsvWriter(writer, GetRfc4180Configuration());
⋮----
csv.NextRecord();
await _logger.LogInfoAsync($"Cleared local CSV file: {file}");
⋮----
await _logger.LogInfoAsync("Local CSV directory does not exist - no files to clear");
⋮----
errors.Add(result.LocalError);
await _logger.LogErrorAsync($"Failed to clear local CSV files: {ex.Message}");
⋮----
var networkFolder = Path.GetDirectoryName(GetNetworkCsvPath("dummy.csv"));
if (Directory.Exists(networkFolder))
⋮----
var filesToClear = Directory.GetFiles(networkFolder, filenamePattern ?? "*.csv");
⋮----
await _logger.LogInfoAsync($"Cleared network CSV file: {file}");
⋮----
await _logger.LogInfoAsync("Network CSV directory does not exist - no files to clear");
⋮----
errors.Add("Network path not available");
await _logger.LogWarningAsync("Network path not available for CSV clearing");
⋮----
errors.Add(result.NetworkError);
await _logger.LogErrorAsync($"Failed to clear network CSV files: {ex.Message}");
⋮----
await _logger.LogInfoAsync($"CSV clearing completed. Local: {result.LocalDeleted}, Network: {result.NetworkDeleted}");
⋮----
await _errorHandler.HandleErrorAsync($"CSV clearing failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
```

## File: Module_Dunnage/Services/Service_DunnageWorkflow.cs
```csharp
public class Service_DunnageWorkflow : IService_DunnageWorkflow
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
public Task<bool> StartWorkflowAsync()
⋮----
if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultDunnageMode))
⋮----
switch (currentUser.DefaultDunnageMode.ToLower())
⋮----
return Task.FromResult(true);
⋮----
public Task<Model_WorkflowStepResult> AdvanceToNextStepAsync()
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please select a dunnage type." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Please select a part." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Quantity must be greater than zero." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = "Already at Review step. Use Save to finish." });
⋮----
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = true, TargetStep = CurrentStep });
⋮----
_errorHandler.HandleErrorAsync("Error advancing step", Enum_ErrorSeverity.Error, ex, true);
return Task.FromResult(new Model_WorkflowStepResult { IsSuccess = false, ErrorMessage = ex.Message });
⋮----
public void GoToStep(Enum_DunnageWorkflowStep step)
⋮----
public async Task<Model_SaveResult> SaveToCSVOnlyAsync()
⋮----
loads.Add(load);
⋮----
return new Model_SaveResult { IsSuccess = false, ErrorMessage = "No data to save." };
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(loads);
return new Model_SaveResult
⋮----
await _errorHandler.HandleErrorAsync("Error saving CSV", Enum_ErrorSeverity.Error, ex, true);
return new Model_SaveResult { IsSuccess = false, ErrorMessage = ex.Message };
⋮----
public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
⋮----
var dbResult = await _dunnageService.SaveLoadsAsync(loads);
⋮----
await _errorHandler.HandleErrorAsync("Error saving to database", Enum_ErrorSeverity.Error, ex, true);
⋮----
public async Task<Model_SaveResult> SaveSessionAsync()
⋮----
await _errorHandler.HandleErrorAsync("Error saving session", Enum_ErrorSeverity.Error, ex, true);
⋮----
public void ClearSession()
⋮----
CurrentSession = new Model_DunnageSession();
_viewModelRegistry.ClearAllInputs();
⋮----
public async Task<Model_CSVDeleteResult> ResetCSVFilesAsync()
⋮----
return await _csvWriter.ClearCSVFilesAsync();
⋮----
public void AddCurrentLoadToSession()
⋮----
var location = string.IsNullOrWhiteSpace(CurrentSession.Location)
⋮----
var poNumber = string.IsNullOrWhiteSpace(CurrentSession.PONumber)
⋮----
var load = new Model_DunnageLoad
⋮----
LoadUuid = Guid.NewGuid(),
⋮----
CurrentSession.Loads.Add(load);
_logger.LogInfo($"Added load to session: Part {load.PartId}, Qty {load.Quantity}", "DunnageWorkflow");
⋮----
_errorHandler.HandleErrorAsync("Error adding load to session", Enum_ErrorSeverity.Medium, ex, false);
⋮----
private Model_DunnageLoad CreateLoadFromCurrentSession()
⋮----
return new Model_DunnageLoad
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminMainViewModel.cs
```csharp
public partial class ViewModel_Dunnage_AdminMain : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
⋮----
private async Task NavigateToManageTypesAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Types);
await _logger.LogInfoAsync("Navigated to Type Management", "AdminMain");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private async Task NavigateToManageSpecsAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Specs);
await _logger.LogInfoAsync("Navigated to Spec Management", "AdminMain");
⋮----
private async Task NavigateToManagePartsAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Parts);
await _logger.LogInfoAsync("Navigated to Part Management", "AdminMain");
⋮----
private async Task NavigateToInventoriedListAsync()
⋮----
await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.InventoriedList);
await _logger.LogInfoAsync("Navigated to Inventoried List", "AdminMain");
⋮----
private async Task ReturnToMainNavigationAsync()
⋮----
await _adminWorkflow.NavigateToHubAsync();
await _logger.LogInfoAsync("Returned to main navigation hub", "AdminMain");
⋮----
private void OnSectionChanged(object? sender, Enum_DunnageAdminSection section)
⋮----
private void OnStatusMessageRaised(object? sender, string message)
⋮----
private void UpdateVisibility(Enum_DunnageAdminSection section)
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_QuantityEntryViewModel.cs
```csharp
public partial class ViewModel_Dunnage_QuantityEntry : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Dispatcher _dispatcher;
private readonly IService_Help _helpService;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_dispatcher.TryEnqueue(LoadContextData);
⋮----
if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse<MaterialIconKind>(SelectedTypeIcon, true, out var kind))
⋮----
public void LoadContextData()
⋮----
_logger.LogInfo($"Initialized workflow session quantity to {Quantity}", "QuantityEntry");
⋮----
_logger.LogInfo($"Loaded context: Type={SelectedTypeName}, Part={SelectedPartName}, Quantity={_workflowService.CurrentSession.Quantity}", "QuantityEntry");
⋮----
_logger.LogError($"Error loading context data: {ex.Message}", ex, "QuantityEntry");
⋮----
partial void OnQuantityChanged(int value)
⋮----
_logger.LogInfo($"Quantity changed to {value}, updated workflow session", "QuantityEntry");
⋮----
GoNextCommand.NotifyCanExecuteChanged();
⋮----
private void ValidateQuantity()
⋮----
private void GoBack()
⋮----
_logger.LogInfo("Navigating back to Part Selection", "QuantityEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
⋮----
private async Task GoNextAsync()
⋮----
_logger.LogInfo($"Quantity set to {Quantity}", "QuantityEntry");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs
```csharp
public partial class ViewModel_Dunnage_Review : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService; private readonly IService_Help _helpService; private readonly IService_DunnageCSVWriter _csvWriter;
private readonly IService_Window _windowService;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
public void LoadSessionLoads()
⋮----
SessionLoads.Clear();
⋮----
SessionLoads.Add(load);
⋮----
_logger.LogInfo($"Loaded {LoadCount} loads for review", "Review");
⋮----
_logger.LogError($"Error loading session loads: {ex.Message}", ex, "Review");
⋮----
private void UpdateNavigationButtons()
⋮----
private void SwitchToSingleView()
⋮----
_logger.LogInfo("Switched to Single View", "Review");
⋮----
private void SwitchToTableView()
⋮----
_logger.LogInfo("Switched to Table View", "Review");
⋮----
private void PreviousEntry()
⋮----
private void NextEntry()
⋮----
private async Task AddAnotherAsync()
⋮----
_logger.LogInfo("User requested to add another load", "Review");
⋮----
_logger.LogInfo("User cancelled add another load", "Review");
⋮----
var saveResult = await _workflowService.SaveToCSVOnlyAsync();
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
_logger.LogInfo("Navigated to Type Selection for new load, workflow data cleared", "Review");
⋮----
_logger.LogError($"Error in AddAnotherAsync: {ex.Message}", ex);
⋮----
private async Task<bool> ConfirmAddAnotherAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogWarning("XamlRoot is null, proceeding without confirmation", "Review");
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex, "Review");
⋮----
private void ClearTransientWorkflowData()
⋮----
_logger.LogInfo("Transient workflow data and UI inputs cleared for new entry", "Review");
⋮----
_logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex, "Review");
⋮----
private void ClearUIInputsForNewEntry()
⋮----
_logger.LogInfo("UI inputs cleared for new entry (loads preserved)", "Review");
⋮----
_logger.LogError($"Error clearing UI inputs: {ex.Message}", ex, "Review");
⋮----
private async Task SaveAllAsync()
⋮----
await _logger.LogInfoAsync($"Starting SaveAllAsync: {LoadCount} loads to save");
var saveResult = await _dunnageService.SaveLoadsAsync(SessionLoads.ToList());
⋮----
await _logger.LogErrorAsync($"Failed to save {LoadCount} loads: {saveResult.ErrorMessage}");
await _errorHandler.HandleDaoErrorAsync(saveResult, "SaveAllAsync", true);
⋮----
await _logger.LogInfoAsync($"Successfully saved {LoadCount} loads to database");
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(SessionLoads.ToList());
⋮----
await _logger.LogWarningAsync($"CSV export failed for {LoadCount} loads: {csvResult.ErrorMessage}");
⋮----
await _logger.LogInfoAsync($"Successfully exported {LoadCount} loads to CSV");
⋮----
await _logger.LogInfoAsync($"Completed SaveAllAsync: {LoadCount} loads processed successfully");
⋮----
await _logger.LogErrorAsync($"Exception in SaveAllAsync: {ex.Message}");
⋮----
private void StartNewEntry()
⋮----
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Dunnage.Review");
⋮----
private void Cancel()
⋮----
_logger.LogInfo("Cancelling review, clearing session", "Review");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_TypeSelectionViewModel.cs
```csharp
public partial class ViewModel_dunnage_typeselection : ViewModel_Shared_Base, IResettableViewModel
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_Pagination _paginationService;
private readonly IService_Help _helpService;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
_viewModelRegistry.Register(this);
⋮----
public void ResetToDefaults()
⋮----
public async Task InitializeAsync()
⋮----
_logger.LogInfo("TypeSelection: InitializeAsync called", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo("TypeSelection: Already busy, returning", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo("TypeSelection: Starting to load types", "ViewModel_dunnage_typeselection");
⋮----
_errorHandler.HandleErrorAsync(
⋮----
).Wait();
⋮----
private async Task LoadTypesAsync()
⋮----
_logger.LogInfo("TypeSelection: LoadTypesAsync called", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo("TypeSelection: Calling service.GetAllTypesAsync()", "ViewModel_dunnage_typeselection");
var result = await _dunnageService.GetAllTypesAsync();
_logger.LogInfo($"TypeSelection: Service returned - IsSuccess: {result.IsSuccess}, Data null: {result.Data == null}, Count: {result.Data?.Count ?? 0}", "ViewModel_dunnage_typeselection");
⋮----
_paginationService.SetSource(result.Data);
_logger.LogInfo($"TypeSelection: Pagination configured with PageSize=9, TotalItems={result.Data.Count}", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo($"TypeSelection: Successfully loaded {result.Data.Count} dunnage types with {TotalPages} pages, DisplayedTypes.Count={DisplayedTypes.Count}", "ViewModel_dunnage_typeselection");
⋮----
await _errorHandler.HandleDaoErrorAsync(
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private void NextPage()
⋮----
_paginationService.NextPage();
⋮----
private void PreviousPage()
⋮----
_paginationService.PreviousPage();
⋮----
private void FirstPage()
⋮----
_paginationService.FirstPage();
⋮----
private void LastPage()
⋮----
_paginationService.LastPage();
⋮----
private async Task SelectTypeAsync(Model_DunnageType? type)
⋮----
_logger.LogInfo($"TypeSelection: Selected dunnage type: {type.TypeName} (ID: {type.Id})", "TypeSelection");
⋮----
_logger.LogInfo($"TypeSelection: Session updated - SelectedTypeId={_workflowService.CurrentSession.SelectedTypeId}", "TypeSelection");
_logger.LogInfo("TypeSelection: Navigating to PartSelection step", "TypeSelection");
_workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
⋮----
_logger.LogError($"TypeSelection: Error selecting type: {ex.Message}", ex, "TypeSelection");
⋮----
private async Task QuickAddTypeAsync()
⋮----
_logger.LogInfo("Quick Add Type requested", "TypeSelection");
⋮----
_logger.LogInfo("Cannot show dialog: XamlRoot is null", "TypeSelection");
⋮----
var result = await dialog.ShowAsync();
⋮----
var iconName = dialog.SelectedIconKind.ToString();
_logger.LogInfo($"Adding new type: {typeName} with icon {iconName}", "TypeSelection");
var newType = new Model_DunnageType
⋮----
var insertResult = await _dunnageService.InsertTypeAsync(newType);
⋮----
_logger.LogInfo($"Successfully added type: {typeName}", "TypeSelection");
⋮----
var specDef = new SpecDefinition
⋮----
var specModel = new Model_DunnageSpec
⋮----
SpecValue = JsonSerializer.Serialize(specDef)
⋮----
await _dunnageService.InsertSpecAsync(specModel);
⋮----
private async Task EditTypeAsync(Model_DunnageType type)
⋮----
_logger.LogInfo($"Edit Type requested for {type.TypeName}", "TypeSelection");
⋮----
var specsResult = await _dunnageService.GetSpecsForTypeAsync(type.Id);
⋮----
existingSpecsDict[s.SpecKey] = new SpecDefinition();
⋮----
dialog.InitializeForEdit(type.TypeName, type.Icon, existingSpecsDict);
⋮----
var newIcon = dialog.SelectedIconKind.ToString();
⋮----
var updateResult = await _dunnageService.UpdateTypeAsync(type);
⋮----
await _errorHandler.HandleDaoErrorAsync(updateResult, nameof(EditTypeAsync), true);
⋮----
var newSpecNames = newSpecs.Select(s => s.Name).ToList();
var removedSpecKeys = existingSpecsDict.Keys.Except(newSpecNames).ToList();
⋮----
await _dunnageService.DeleteSpecAsync(specToDelete.Id);
⋮----
var json = JsonSerializer.Serialize(specDef);
if (existingSpecsDict.ContainsKey(specItem.Name))
⋮----
await _dunnageService.UpdateSpecAsync(existingModel);
⋮----
_logger.LogInfo($"Successfully updated type: {type.TypeName}", "TypeSelection");
⋮----
await _errorHandler.HandleErrorAsync("Error updating dunnage type", Enum_ErrorSeverity.Error, ex, true);
⋮----
private async Task DeleteTypeAsync(Model_DunnageType type)
⋮----
_logger.LogInfo($"Delete Type requested for {type.TypeName}", "TypeSelection");
⋮----
var deleteResult = await _dunnageService.DeleteTypeAsync(type.Id);
⋮----
_logger.LogInfo($"Successfully deleted type: {type.TypeName}", "TypeSelection");
⋮----
await _errorHandler.HandleDaoErrorAsync(deleteResult, nameof(DeleteTypeAsync), true);
⋮----
await _errorHandler.HandleErrorAsync("Error deleting dunnage type", Enum_ErrorSeverity.Error, ex, true);
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
private void UpdatePaginationProperties()
⋮----
NextPageCommand.NotifyCanExecuteChanged();
PreviousPageCommand.NotifyCanExecuteChanged();
⋮----
private void UpdatePageDisplay()
⋮----
_logger.LogInfo($"TypeSelection: UpdatePageDisplay - Got {currentItems.Count()} items from pagination service", "ViewModel_dunnage_typeselection");
DisplayedTypes.Clear();
⋮----
DisplayedTypes.Add(type);
_logger.LogInfo($"TypeSelection: Added type to DisplayedTypes - ID: {type.Id}, Name: {type.TypeName}", "ViewModel_dunnage_typeselection");
⋮----
_logger.LogInfo($"TypeSelection: DisplayedTypes.Count after update: {DisplayedTypes.Count}", "ViewModel_dunnage_typeselection");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml
```
<Page x:Class="MTM_Receiving_Application.Module_Dunnage.Views.View_Dunnage_WorkflowView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:viewmodels="using:MTM_Receiving_Application.Module_Dunnage.ViewModels"
      xmlns:views="using:MTM_Receiving_Application.Module_Dunnage.Views"
      xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
      mc:Ignorable="d">

    <Page.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter" />
    </Page.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- Content Area -->
        <Grid Grid.Row="1"
              Margin="16,12,16,16">
            <!-- Mode Selection View -->
            <views:View_Dunnage_ModeSelectionView Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <!-- Wizard Steps -->
            <views:View_dunnage_typeselectionView Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_PartSelectionView Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_QuantityEntryView Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_DetailsEntryView Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_ReviewView Visibility="{x:Bind ViewModel.IsReviewVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <!-- Bulk Operation Views -->
            <views:View_Dunnage_ManualEntryView Visibility="{x:Bind ViewModel.IsManualEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

            <views:View_Dunnage_EditModeView Visibility="{x:Bind ViewModel.IsEditModeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
        </Grid>

        <!-- Navigation Buttons -->
        <Grid Grid.Row="2"
              Margin="24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>

            <!-- Left-aligned buttons -->
            <StackPanel Grid.Column="0"
                        Orientation="Horizontal"
                        HorizontalAlignment="Left"
                        Spacing="12">
                <!-- Mode Selection Button (visible in wizard steps) -->
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />

                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsManualEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
                <Button Content="Mode Selection"
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        ToolTipService.ToolTip="Return to mode selection (clears current work)"
                        Visibility="{x:Bind ViewModel.IsEditModeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}" />
            </StackPanel>

            <!-- Center help button -->
            <Button Grid.Column="1"
                    HorizontalAlignment="Center"
                    Style="{StaticResource AccentButtonStyle}"
                    Padding="12,8"
                    Click="HelpButton_Click"
                    ToolTipService.ToolTip="Click for help about the current step">
                <StackPanel Orientation="Horizontal"
                            Spacing="8">
                    <FontIcon Glyph="&#xE946;"
                              FontSize="16" />
                    <TextBlock Text="Help" />
                </StackPanel>
            </Button>

            <!-- Right-aligned navigation buttons (wizard flow) -->
            <StackPanel Grid.Column="2"
                        Orientation="Horizontal"
                        HorizontalAlignment="Right"
                        Spacing="12">
                <!-- Back Button (not visible on first wizard step) -->
                <Button Content="Back"
                        Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnBackClick"
                        ToolTipService.ToolTip="Go back to previous step (Ctrl+Left)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Left"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Back"
                        Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnBackClick"
                        ToolTipService.ToolTip="Go back to previous step (Ctrl+Left)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Left"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Back"
                        Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnBackClick"
                        ToolTipService.ToolTip="Go back to previous step (Ctrl+Left)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Left"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>

                <!-- Next Button (wizard flow) -->
                <Button Content="Next"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsTypeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnNextClick"
                        ToolTipService.ToolTip="Proceed to next step (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Next"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsPartSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnNextClick"
                        ToolTipService.ToolTip="Proceed to next step (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Next"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnNextClick"
                        ToolTipService.ToolTip="Proceed to next step (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
                <Button Content="Save &amp; Review"
                        Style="{StaticResource AccentButtonStyle}"
                        Visibility="{x:Bind ViewModel.IsDetailsEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        Click="OnSaveAndReviewClick"
                        ToolTipService.ToolTip="Save entry and proceed to review (Ctrl+Right)">
                    <Button.KeyboardAccelerators>
                        <KeyboardAccelerator Key="Right"
                                             Modifiers="Control" />
                    </Button.KeyboardAccelerators>
                </Button>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminInventoryViewModel.cs
```csharp
public partial class ViewModel_Dunnage_AdminInventory : ViewModel_Shared_Base
⋮----
private readonly Dao_InventoriedDunnage _daoInventory;
private readonly Dao_DunnagePart _daoPart;
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
private readonly IService_Window _windowService;
⋮----
_daoInventory = daoInventory ?? throw new ArgumentNullException(nameof(daoInventory));
_daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
_adminWorkflow = adminWorkflow ?? throw new ArgumentNullException(nameof(adminWorkflow));
_windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
⋮----
public async Task InitializeAsync()
⋮----
private async Task LoadInventoriedPartsAsync()
⋮----
var result = await _daoInventory.GetAllAsync();
⋮----
InventoriedParts.Clear();
⋮----
InventoriedParts.Add(part);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task ShowAddToListAsync()
⋮----
var result = await dialog.ShowAsync();
⋮----
private async Task ShowEditEntryAsync()
⋮----
var dialog = new ContentDialog
⋮----
XamlRoot = _windowService.GetXamlRoot()
⋮----
var stackPanel = new StackPanel { Spacing = 12 };
stackPanel.Children.Add(new TextBlock { Text = "Part ID (read-only)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
var partIdBox = new TextBox
⋮----
stackPanel.Children.Add(partIdBox);
stackPanel.Children.Add(new TextBlock { Text = "Inventory Method", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
var methodCombo = new ComboBox
⋮----
stackPanel.Children.Add(methodCombo);
stackPanel.Children.Add(new TextBlock { Text = "Notes (optional)", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
var notesBox = new TextBox
⋮----
stackPanel.Children.Add(notesBox);
⋮----
var dialogResult = await dialog.ShowAsync();
⋮----
var updateResult = await _daoInventory.UpdateAsync(
⋮----
private async Task ShowRemoveConfirmationAsync()
⋮----
private async Task RemoveFromListAsync()
⋮----
var result = await _daoInventory.DeleteAsync(SelectedInventoriedPart.Id);
⋮----
private async Task BackToHubAsync()
⋮----
_logger.LogInfo("Returning to Settings Mode Selection from Admin Inventory");
⋮----
settingsWorkflow.GoBack();
⋮----
partial void OnSelectedInventoriedPartChanged(Model_InventoriedDunnage? value)
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_AdminTypesViewModel.cs
```csharp
public partial class ViewModel_Dunnage_AdminTypes : ViewModel_Shared_Base
⋮----
private readonly IService_MySQL_Dunnage _dunnageService;
private readonly IService_DunnageAdminWorkflow _adminWorkflow;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
private async Task LoadTypesAsync()
⋮----
var result = await _dunnageService.GetAllTypesAsync();
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "LoadTypesAsync", true);
⋮----
Types.Clear();
⋮----
Types.Add(type);
⋮----
await _logger.LogInfoAsync($"Loaded {Types.Count} dunnage types", "TypeManagement");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
private async Task ShowAddTypeAsync()
⋮----
XamlRoot = _windowService.GetXamlRoot()
⋮----
var result = await dialog.ShowAsync();
⋮----
await _logger.LogInfoAsync("New dunnage type added via Add Type Dialog", "TypeManagement");
⋮----
private async Task ShowEditTypeAsync()
⋮----
var editedType = new Model_DunnageType
⋮----
var dialog = new ContentDialog
⋮----
var stackPanel = new StackPanel { Spacing = 12 };
var typeNameBox = new TextBox
⋮----
var iconSelectorButton = new Button
⋮----
Padding = new Thickness(16, 12, 16, 12)
⋮----
var iconButtonPanel = new StackPanel
⋮----
var iconTextPanel = new StackPanel
⋮----
var iconLabel = new TextBlock
⋮----
var iconName = new TextBlock
⋮----
iconTextPanel.Children.Add(iconLabel);
iconTextPanel.Children.Add(iconName);
iconButtonPanel.Children.Add(iconDisplay);
iconButtonPanel.Children.Add(iconTextPanel);
⋮----
iconSelector.SetInitialSelection(editedType.IconKind);
iconSelector.Activate();
var selectedIcon = await iconSelector.WaitForSelectionAsync();
⋮----
editedType.Icon = selectedIcon.Value.ToString();
⋮----
iconName.Text = selectedIcon.Value.ToString();
⋮----
var iconHeader = new TextBlock
⋮----
Margin = new Thickness(0, 8, 0, 0),
⋮----
stackPanel.Children.Add(typeNameBox);
stackPanel.Children.Add(iconHeader);
stackPanel.Children.Add(iconSelectorButton);
⋮----
editedType.DunnageType = typeNameBox.Text.Trim();
if (string.IsNullOrWhiteSpace(editedType.DunnageType))
⋮----
await _errorHandler.ShowUserErrorAsync("Type name is required", "Validation Error", "ShowEditTypeAsync");
⋮----
var updateResult = await _dunnageService.UpdateTypeAsync(editedType);
⋮----
await _errorHandler.HandleDaoErrorAsync(updateResult, "UpdateTypeAsync", true);
⋮----
await _logger.LogInfoAsync($"Updated type: {editedType.DunnageType}", "TypeManagement");
⋮----
private async Task ShowDeleteConfirmationAsync()
⋮----
var partCountResult = await _dunnageService.GetPartCountByTypeAsync(SelectedType.Id);
var transactionCountResult = await _dunnageService.GetTransactionCountByTypeAsync(SelectedType.Id);
⋮----
await _errorHandler.HandleDaoErrorAsync(
⋮----
var warningDialog = new ContentDialog
⋮----
await warningDialog.ShowAsync();
⋮----
var confirmDialog = new ContentDialog
⋮----
stackPanel.Children.Add(new TextBlock { Text = impactMessage, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
var confirmBox = new TextBox
⋮----
stackPanel.Children.Add(confirmBox);
⋮----
var result = await confirmDialog.ShowAsync();
⋮----
private async Task DeleteTypeAsync()
⋮----
var result = await _dunnageService.DeleteTypeAsync(SelectedType.Id);
⋮----
await _errorHandler.HandleDaoErrorAsync(result, "DeleteTypeAsync", true);
⋮----
await _logger.LogInfoAsync($"Deleted type: {SelectedType.DunnageType}", "TypeManagement");
⋮----
private async Task ReturnToAdminHubAsync()
⋮----
_logger.LogInfo("Returning to Settings Mode Selection from Admin Types");
⋮----
settingsWorkflow.GoBack();
⋮----
partial void OnSelectedTypeChanged(Model_DunnageType? value)
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_WorkFlowViewModel.cs
```csharp
public partial class ViewModel_Dunnage_WorkFlowViewModel : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Window _windowService;
⋮----
_currentLine = new Model_DunnageLine();
⋮----
private async Task InitializeWorkflowAsync()
⋮----
await _workflowService.StartWorkflowAsync();
⋮----
_errorHandler.HandleException(
⋮----
private Model_DunnageLine _currentLine;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
private async Task ResetCSVAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var result = await dialog.ShowAsync();
⋮----
var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
⋮----
var warnResult = await warnDialog.ShowAsync();
⋮----
var deleteResult = await _workflowService.ResetCSVFilesAsync();
⋮----
private void ReturnToModeSelection()
⋮----
_workflowService.ClearSession();
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ModeSelection);
⋮----
private async Task AddLineAsync()
```

## File: Module_Dunnage/Views/View_Dunnage_WorkflowView.xaml.cs
```csharp
public sealed partial class View_Dunnage_WorkflowView : Page
⋮----
private readonly IService_Focus _focusService;
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
private async void HelpButton_Click(object sender, RoutedEventArgs e)
⋮----
private async System.Threading.Tasks.Task ShowHelpForCurrentStepAsync()
⋮----
await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
⋮----
private async void OnNextClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
var result = await workflowService.AdvanceToNextStepAsync();
⋮----
var dialog = new ContentDialog
⋮----
var dialogResult = await dialog.ShowAsync();
⋮----
private async void OnSaveAndReviewClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
var confirmDialog = new ContentDialog
⋮----
var confirmResult = await confirmDialog.ShowAsync();
⋮----
var errorDialog = new ContentDialog
⋮----
await errorDialog.ShowAsync();
⋮----
private void OnBackClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.QuantityEntry);
⋮----
workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
```

## File: Module_Dunnage/ViewModels/ViewModel_Dunnage_ModeSelectionViewModel.cs
```csharp
public partial class ViewModel_Dunnage_ModeSelection : ViewModel_Shared_Base
⋮----
private readonly IService_DunnageWorkflow _workflowService;
private readonly IService_Help _helpService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_UserPreferences _userPreferencesService;
private readonly IService_Window _windowService;
⋮----
private void LoadDefaultMode()
⋮----
private async Task SelectGuidedModeAsync()
⋮----
_logger.LogInfo("User selected Guided Wizard mode");
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.TypeSelection);
⋮----
private async Task SelectManualModeAsync()
⋮----
_logger.LogInfo("User selected Manual Entry mode");
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.ManualEntry);
⋮----
private async Task SelectEditModeAsync()
⋮----
_logger.LogInfo("User selected Edit Mode");
⋮----
_workflowService.GoToStep(Enum_DunnageWorkflowStep.EditMode);
⋮----
private bool HasUnsavedData()
⋮----
!string.IsNullOrEmpty(_workflowService.CurrentSession.PONumber) ||
!string.IsNullOrEmpty(_workflowService.CurrentSession.Location))
⋮----
private async Task<bool> ConfirmModeChangeAsync()
⋮----
_logger.LogInfo("No unsaved data detected, skipping confirmation dialog");
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogWarning("XamlRoot is null, proceeding without confirmation");
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
_logger.LogError($"Error showing confirmation dialog: {ex.Message}", ex);
⋮----
private void ClearWorkflowData()
⋮----
_logger.LogInfo("Dunnage workflow data and UI inputs cleared for mode change");
⋮----
_logger.LogError($"Error clearing dunnage workflow data: {ex.Message}", ex);
⋮----
private void ClearAllUIInputs()
⋮----
_logger.LogInfo("UI inputs cleared across all Dunnage ViewModels");
⋮----
_logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
⋮----
private async Task SetGuidedAsDefaultAsync(bool isChecked)
⋮----
var result = await _userPreferencesService.UpdateDefaultDunnageModeAsync(currentUser.WindowsUsername, newMode ?? "");
⋮----
// Update in-memory user object
⋮----
// Update UI state
⋮----
_logger.LogInfo($"Dunnage default mode set to: {newMode ?? "none"}");
⋮----
await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
⋮----
await _errorHandler.HandleErrorAsync($"Failed to set default dunnage mode: {ex.Message}",
⋮----
private async Task SetManualAsDefaultAsync(bool isChecked)
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Dunnage.ModeSelection");
⋮----
private async Task SetEditAsDefaultAsync(bool isChecked)
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```
