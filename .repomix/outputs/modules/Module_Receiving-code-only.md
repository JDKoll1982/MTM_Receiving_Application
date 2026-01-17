# User Provided Header
MTM Receiving Application - Module_Receiving Code-Only Export

# Files

## File: Module_Receiving/Models/Model_Application_Variables.cs
```csharp
public class Model_Application_Variables
⋮----
public string LogDirectory { get; set; } = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
```

## File: Module_Receiving/Models/Model_CSVDeleteResult.cs
```csharp
public class Model_CSVDeleteResult
```

## File: Module_Receiving/Models/Model_CSVExistenceResult.cs
```csharp
public class Model_CSVExistenceResult
```

## File: Module_Receiving/Models/Model_CSVWriteResult.cs
```csharp
public class Model_CSVWriteResult
```

## File: Module_Receiving/Models/Model_InforVisualPart.cs
```csharp
public class Model_InforVisualPart
```

## File: Module_Receiving/Models/Model_InforVisualPO.cs
```csharp
public class Model_InforVisualPO
```

## File: Module_Receiving/Models/Model_PackageTypePreference.cs
```csharp
public class Model_PackageTypePreference
```

## File: Module_Receiving/Models/Model_SaveResult.cs
```csharp
public class Model_SaveResult
```

## File: Module_Receiving/Models/Model_UserPreference.cs
```csharp
public partial class Model_UserPreference : ObservableObject
⋮----
private DateTime _lastUpdated;
```

## File: Module_Receiving/Models/Model_WorkflowStepResult.cs
```csharp
public class Model_WorkflowStepResult
```

## File: Module_Receiving/Views/View_Receiving_EditMode.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_EditMode"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
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

        <!-- Toolbar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <!-- Main Toolbar Area -->
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <!-- Load Data Section -->
                <TextBlock Text="Load Data From:" VerticalAlignment="Center" FontWeight="SemiBold" Margin="0,0,4,0"/>
                
                <Button Command="{x:Bind ViewModel.LoadFromCurrentMemoryCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadSessionMemory'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8A7;"/>
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
                <Button Command="{x:Bind ViewModel.SelectAllCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SelectAll'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE8B3;"/>
                        <TextBlock Text="{x:Bind ViewModel.SelectAllButtonText, Mode=OneWay}"/>
                    </StackPanel>
                </Button>
                <Button Command="{x:Bind ViewModel.RemoveRowCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.RemoveRow'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74D;"/>
                        <TextBlock Text="Remove Row"/>
                    </StackPanel>
                </Button>
            </StackPanel>
        </Grid>

        <!-- Date Filter Toolbar -->
        <Grid Grid.Row="1" Margin="0,0,0,12">
            <StackPanel Orientation="Horizontal" Spacing="12" VerticalAlignment="Center">
                <TextBlock Text="Filter Date:" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <CalendarDatePicker Date="{x:Bind ViewModel.FilterStartDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.StartDate'), Mode=OneWay}"
                                   Width="140"/>
                <TextBlock Text="to" VerticalAlignment="Center"/>
                <CalendarDatePicker Date="{x:Bind ViewModel.FilterEndDate, Mode=TwoWay}" 
                                   PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.EndDate'), Mode=OneWay}"
                                   Width="140"/>
                
                <Rectangle Width="1" Height="24" Fill="{ThemeResource SystemControlForegroundBaseLowBrush}" Margin="8,0"/>
                
                <Button Content="Last Week" Command="{x:Bind ViewModel.SetFilterLastWeekCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterLastWeek'), Mode=OneWay}"/>
                <Button Content="Today" Command="{x:Bind ViewModel.SetFilterTodayCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterToday'), Mode=OneWay}"/>
                <Button Content="This Week" Command="{x:Bind ViewModel.SetFilterThisWeekCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisWeek'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisMonthButtonText, Mode=OneWay}" Command="{x:Bind ViewModel.SetFilterThisMonthCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisMonth'), Mode=OneWay}"/>
                <Button Content="{x:Bind ViewModel.ThisQuarterButtonText, Mode=OneWay}" Command="{x:Bind ViewModel.SetFilterThisQuarterCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.FilterThisQuarter'), Mode=OneWay}"/>
                <Button Content="Show All" Command="{x:Bind ViewModel.SetFilterShowAllCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ClearFilters'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>

        <!-- Data Grid -->
        <controls:DataGrid Grid.Row="2"
                          x:Name="EditModeDataGrid"
                          ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                          SelectedItem="{x:Bind ViewModel.SelectedLoad, Mode=TwoWay}"
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
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartID}" Width="*"/>
                <controls:DataGridTextColumn Header="Weight/Qty" Binding="{Binding WeightQuantity, Converter={StaticResource DecimalToStringConverter}}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Heat/Lot" Binding="{Binding HeatLotNumber}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Pkg Type" Binding="{Binding PackageType}" IsReadOnly="True" Width="Auto"/>
                <controls:DataGridTextColumn Header="Pkgs/Load" Binding="{Binding PackagesPerLoad}" Width="Auto"/>
                <controls:DataGridTextColumn Header="Wt/Pkg" Binding="{Binding WeightPerPackage, Converter={StaticResource DecimalToStringConverter}}" IsReadOnly="True" Width="Auto"/>
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
                <TextBox Text="{x:Bind ViewModel.GotoPageNumber, Mode=TwoWay}" Width="60" HorizontalContentAlignment="Center"/>
                <TextBlock Text="of" VerticalAlignment="Center"/>
                <TextBlock Text="{x:Bind ViewModel.TotalPages, Mode=OneWay}" VerticalAlignment="Center" FontWeight="SemiBold"/>
                
                <Button Content="Go" Command="{x:Bind ViewModel.GoToPageCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.GoToPage'), Mode=OneWay}"/>
                
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
                <Button Content="Save &amp; Finish" Command="{x:Bind ViewModel.SaveCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndFinish'), Mode=OneWay}"/>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_EditMode.xaml.cs
```csharp
public sealed partial class View_Receiving_EditMode : UserControl
⋮----
this.InitializeComponent();
⋮----
private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
⋮----
grid?.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[EditModeView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void EditModeDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[EditModeView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[EditModeView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[EditModeView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[EditModeView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[EditModeView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[EditModeView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[EditModeView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[EditModeView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[EditModeView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Receiving/Views/View_Receiving_HeatLot.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_HeatLot"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="16">
        <!-- Content Section -->
        <Grid>
            <StackPanel Spacing="16">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Text="Load Entries" Style="{StaticResource SubtitleTextBlockStyle}" VerticalAlignment="Center"/>
                    <Button Grid.Column="1" 
                            Content="Auto-Fill" 
                            Command="{x:Bind ViewModel.AutoFillCommand}"
                            Style="{StaticResource AccentButtonStyle}"
                            ToolTipService.ToolTip="Fill blank heat numbers from rows above"/>
                </Grid>
            
                <ScrollViewer VerticalScrollBarVisibility="Auto" MaxHeight="500">
                    <ItemsControl ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate>
                                <Border Margin="0,0,0,12" 
                                       Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" 
                                       Padding="16" 
                                       CornerRadius="8" 
                                       BorderThickness="1" 
                                       BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                                    <Grid ColumnSpacing="16">
                                        <Grid.ColumnDefinitions>
                                            <ColumnDefinition Width="Auto"/>
                                            <ColumnDefinition Width="*"/>
                                        </Grid.ColumnDefinitions>
                                        
                                        <StackPanel Grid.Column="0" VerticalAlignment="Center" Width="100">
                                            <StackPanel Orientation="Horizontal" Spacing="8">
                                                <FontIcon Glyph="&#xE7B8;" 
                                                         FontSize="16"
                                                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                                <TextBlock Text="{Binding LoadNumber, Converter={StaticResource StringFormatConverter}, ConverterParameter='Load #{0}'}" 
                                                          Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                            </StackPanel>
                                        </StackPanel>
                                        
                                        <TextBox Grid.Column="1"
                                                Header="Heat/Lot Number (Optional)"
                                                Text="{Binding HeatLotNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                                PlaceholderText="Enter heat/lot number or leave blank"
                                                AutomationProperties.Name="Heat Lot Number"/>
                                    </Grid>
                                </Border>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
            </StackPanel>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs
```csharp
public sealed partial class View_Receiving_HeatLot : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void HeatLotView_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_LoadEntry.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_LoadEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d">

    <Grid HorizontalAlignment="Center" VerticalAlignment="Center">
        <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="32"
                MinWidth="500">
            <StackPanel Spacing="24" HorizontalAlignment="Center">
                <StackPanel Spacing="8" HorizontalAlignment="Center">
                    <FontIcon Glyph="&#xE7B8;" 
                             FontSize="48"
                             Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                             HorizontalAlignment="Center"/>
                    <TextBlock Text="{x:Bind ViewModel.SelectedPartInfo, Mode=OneWay}" 
                               Style="{StaticResource SubtitleTextBlockStyle}"
                               Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"
                               HorizontalAlignment="Center"
                               TextAlignment="Center"/>
                </StackPanel>

                <NumberBox x:Name="NumberOfLoadsNumberBox"
                           Header="Number of Loads (1-99)"
                           Value="{x:Bind ViewModel.NumberOfLoads, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                           Minimum="1"
                           Maximum="99"
                           SmallChange="1"
                           LargeChange="10"
                           SpinButtonPlacementMode="Inline"
                           Width="250"
                           HorizontalAlignment="Center"
                           AutomationProperties.Name="Number of Loads"/>
                       
                <TextBlock Text="Enter the total number of skids/loads for this part." 
                           Style="{StaticResource CaptionTextBlockStyle}"
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                           HorizontalAlignment="Center"
                           TextAlignment="Center"/>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs
```csharp
public sealed partial class View_Receiving_LoadEntry : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
_focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
```

## File: Module_Receiving/Views/View_Receiving_ManualEntry.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_ManualEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
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
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <Grid Grid.Row="0" Margin="0,0,0,12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="12">
                <Button x:Name="AddRowButton" Command="{x:Bind ViewModel.AddRowCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddRow'), Mode=OneWay}">
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
                <Button Command="{x:Bind ViewModel.AutoFillCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AutoFill'), Mode=OneWay}">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <FontIcon Glyph="&#xE74E;"/>
                        <TextBlock Text="Auto-Fill"/>
                    </StackPanel>
                </Button>
            </StackPanel>
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
                <controls:DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" IsReadOnly="True"/>
                <controls:DataGridTextColumn Header="Part ID" Binding="{Binding PartID}" Width="*"/>
                <controls:DataGridTextColumn Header="Weight/Qty" Binding="{Binding WeightQuantity, Converter={StaticResource DecimalToStringConverter}}"/>
                <controls:DataGridTextColumn Header="Heat/Lot" Binding="{Binding HeatLotNumber}"/>
                <controls:DataGridTextColumn Header="Pkg Type" Binding="{Binding PackageType}" IsReadOnly="True"/>
                <controls:DataGridTextColumn Header="Pkgs/Load" Binding="{Binding PackagesPerLoad}"/>
                <controls:DataGridTextColumn Header="Wt/Pkg" Binding="{Binding WeightPerPackage, Converter={StaticResource DecimalToStringConverter}}" IsReadOnly="True"/>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Navigation -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,12,0,0" Spacing="12">
            <Button Content="Save &amp; Finish" Command="{x:Bind ViewModel.SaveCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndFinish'), Mode=OneWay}"/>
        </StackPanel>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_ModeSelection.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_ModeSelection"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
    mc:Ignorable="d">

    <Grid>
        <StackPanel Spacing="32" HorizontalAlignment="Center" VerticalAlignment="Center">
            <StackPanel Orientation="Horizontal" Spacing="24">
                <!-- Guided Mode Column -->
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
                            <FontIcon Glyph="&#xE771;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Guided Wizard" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Step-by-step process for standard receiving workflow." 
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
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickGuidedWizard'), Mode=OneWay}"/>
                </StackPanel>

                <!-- Manual Mode Column -->
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
                            <FontIcon Glyph="&#xE7F0;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Manual Entry" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Customizable grid for bulk data entry and editing." 
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
                              ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.QuickManualEntry'), Mode=OneWay}"/>
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
                            <FontIcon Glyph="&#xE70F;" FontSize="48" Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                            <TextBlock Text="Edit Mode" Style="{StaticResource SubtitleTextBlockStyle}" HorizontalAlignment="Center"/>
                            <TextBlock Text="Edit existing loads without adding new ones." 
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

## File: Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs
```csharp
public sealed partial class View_Receiving_ModeSelection : UserControl
⋮----
this.InitializeComponent();
```

## File: Module_Receiving/Views/View_Receiving_PackageType.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_PackageType"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Package Type Selection -->
        <Border Grid.Row="1"
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                Padding="20"
                CornerRadius="8"
                VerticalAlignment="Top"
                Margin="0,0,0,16">
            <StackPanel Spacing="16">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE7B8;" FontSize="16" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    <TextBlock Text="Package Type (Applied to all loads)" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                
                <StackPanel Orientation="Horizontal" Spacing="16">
                    <ComboBox x:Name="PackageTypeComboBox"
                              Header="Type"
                              ItemsSource="{x:Bind ViewModel.PackageTypes, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedPackageType, Mode=TwoWay}"
                              Width="200"
                              AutomationProperties.Name="Package Type"/>
                    
                    <TextBox Header="Custom Name"
                             Text="{x:Bind ViewModel.CustomPackageTypeName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                             Visibility="{x:Bind ViewModel.IsCustomTypeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                             Width="200"
                             AutomationProperties.Name="Custom Package Name"/>
                </StackPanel>
                
                <CheckBox Content="Save as default for this part" 
                          IsChecked="{x:Bind ViewModel.IsSaveAsDefault, Mode=TwoWay}"/>
            </StackPanel>
        </Border>

        <!-- Loads List -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="0,0,0,8" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" Padding="16,12" CornerRadius="8" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="100"/>
                                <ColumnDefinition Width="250"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            
                            <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" FontSize="14" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="{Binding LoadNumber, Converter={StaticResource StringFormatConverter}, ConverterParameter='#{0}'}" 
                                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>
                            
                            <NumberBox Grid.Column="1"
                                       Header="Packages per Load"
                                       Value="{Binding PackagesPerLoad, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                                       Minimum="1"
                                       SmallChange="1"
                                       LargeChange="5"
                                       SpinButtonPlacementMode="Inline"
                                       AutomationProperties.Name="Packages per Load"/>
                                       
                            <StackPanel Grid.Column="2" VerticalAlignment="Center" Margin="16,0,0,0">
                                <TextBlock Text="Weight per Package" Style="{StaticResource CaptionTextBlockStyle}" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                <TextBlock Text="{Binding WeightPerPackage, Converter={StaticResource StringFormatConverter}, ConverterParameter='{}{0:N2} lbs'}" 
                                           Style="{StaticResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_PackageType.xaml.cs
```csharp
public sealed partial class View_Receiving_PackageType : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this, PackageTypeComboBox);
⋮----
private async void PackageTypeView_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_POEntry.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_POEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_DecimalToInt x:Key="DecimalToIntConverter"/>
    </UserControl.Resources>

    <Grid Padding="24" RowSpacing="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Input Section (Row 0) -->
        <Grid Grid.Row="0">
            <!-- PO Entry Mode -->
            <StackPanel Visibility="{x:Bind ViewModel.IsNonPOItem, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}">
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <StackPanel Spacing="16">
                        <!-- PO Number Input with Icon -->
                        <StackPanel Spacing="8">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE8A5;" 
                                         FontSize="16"
                                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                <TextBlock Text="Purchase Order Number" 
                                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                            
                            <TextBox x:Name="PoNumberTextBox"
                                    Text="{x:Bind ViewModel.PoNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" 
                                    PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PONumber'), Mode=OneWay}"
                                    MaxWidth="400"
                                    HorizontalAlignment="Stretch"
                                    TabIndex="0"
                                    LostFocus="POTextBox_LostFocus"
                                    AutomationProperties.Name="Purchase Order Number"/>
                            
                            <!-- Validation Message -->
                            <TextBlock Text="{x:Bind ViewModel.PoValidationMessage, Mode=OneWay}" 
                                      Foreground="{ThemeResource SystemFillColorCriticalBrush}"
                                      FontSize="12"
                                      Visibility="{x:Bind ViewModel.IsLoadPOEnabled, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=Inverse}"/>
                            
                            <!-- PO Status Display -->
                            <Border BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                    BorderThickness="1"
                                    CornerRadius="4"
                                    Padding="8,4"
                                    Margin="0,8,0,0"
                                    Background="{ThemeResource CardBackgroundFillColorSecondaryBrush}">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8C9;" FontSize="14"/>
                                    <TextBlock Text="Status:" FontWeight="SemiBold" FontSize="12"/>
                                    <TextBlock Text="{x:Bind ViewModel.PoStatusDescription, Mode=OneWay}" 
                                              FontSize="12"
                                              FontWeight="SemiBold"/>
                                </StackPanel>
                            </Border>
                        </StackPanel>

                        <!-- Action Buttons -->
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <Button Grid.Column="0"
                                   Command="{x:Bind ViewModel.LoadPOCommand}" 
                                   IsEnabled="{x:Bind ViewModel.IsLoadPOEnabled, Mode=OneWay}"
                                   Style="{StaticResource AccentButtonStyle}"
                                   AutomationProperties.Name="Load Purchase Order"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LoadPO'), Mode=OneWay}">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE896;" FontSize="16"/>
                                    <TextBlock Text="Load PO"/>
                                </StackPanel>
                            </Button>
                            
                            <Button Grid.Column="1"
                                   Content="Switch to Non-PO" 
                                   Command="{x:Bind ViewModel.ToggleNonPOCommand}"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SwitchToNonPO'), Mode=OneWay}"/>
                        </Grid>
                    </StackPanel>
                </Border>
            </StackPanel>

            <!-- Non-PO Entry Mode -->
            <StackPanel Visibility="{x:Bind ViewModel.IsNonPOItem, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                        BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                        BorderThickness="1"
                        CornerRadius="8"
                        Padding="20">
                    <StackPanel Spacing="16">
                        <!-- Part ID Input with Icon -->
                        <StackPanel Spacing="8">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE8B7;" 
                                         FontSize="16"
                                         Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                <TextBlock Text="Part Identifier" 
                                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                            
                            <TextBox x:Name="PartIDTextBox"
                                    Text="{x:Bind ViewModel.PartID, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" 
                                    PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PartID'), Mode=OneWay}"
                                    MaxWidth="400"
                                    HorizontalAlignment="Stretch"
                                    AutomationProperties.Name="Part Identifier"/>
                        </StackPanel>

                        <!-- Package Type Display (Auto-detected) -->
                        <StackPanel Spacing="8">
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE7B8;" 
                                         FontSize="16"
                                         Foreground="{ThemeResource AccentTextFillColorSecondaryBrush}"/>
                                <TextBlock Text="Package Type (Auto-detected)" 
                                          Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                            </StackPanel>
                            
                            <TextBlock Text="{x:Bind ViewModel.PackageType, Mode=OneWay}"
                                      Style="{ThemeResource SubtitleTextBlockStyle}"
                                      Foreground="{ThemeResource SystemAccentColor}"/>
                        </StackPanel>

                        <!-- Action Buttons -->
                        <Grid ColumnSpacing="12">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="Auto"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>
                            
                            <Button Grid.Column="0"
                                   Command="{x:Bind ViewModel.LookupPartCommand}" 
                                   Style="{StaticResource AccentButtonStyle}"
                                   AutomationProperties.Name="Look Up Part"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.LookupPart'), Mode=OneWay}">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE721;" FontSize="16"/>
                                    <TextBlock Text="Look Up Part"/>
                                </StackPanel>
                            </Button>
                            
                            <Button Grid.Column="1"
                                   Content="Switch to PO Entry" 
                                   Command="{x:Bind ViewModel.ToggleNonPOCommand}"
                                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SwitchToPO'), Mode=OneWay}"/>
                        </Grid>
                    </StackPanel>
                </Border>
            </StackPanel>
        </Grid>

        <!-- Loading Indicator -->
        <ProgressBar Grid.Row="0" 
                    IsIndeterminate="True" 
                    VerticalAlignment="Bottom"
                    Visibility="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                    Margin="0,0,0,-10"/>

        <!-- Parts Grid Section (Row 1) -->
        <Border Grid.Row="1" 
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Padding="16"
                Visibility="{x:Bind ViewModel.IsPartsListVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="0,0,0,12">
                    <FontIcon Glyph="&#xE8FD;" 
                             FontSize="16"
                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                    <TextBlock Text="Available Parts" 
                              Style="{ThemeResource SubtitleTextBlockStyle}"/>
                </StackPanel>
                
                <controls:DataGrid Grid.Row="1"
                                  ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                                  SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                                  AutoGenerateColumns="False"
                                  IsReadOnly="True"
                                  GridLinesVisibility="Horizontal"
                                  HeadersVisibility="Column"
                                  SelectionMode="Single"
                                  AutomationProperties.Name="Parts List">
                    <controls:DataGrid.Columns>
                        <controls:DataGridTextColumn Header="Part ID" 
                                                    Binding="{Binding PartID}" 
                                                    Width="150"/>
                        <controls:DataGridTextColumn Header="Description" 
                                                    Binding="{Binding Description}" 
                                                    Width="*"/>
                        <controls:DataGridTextColumn Header="Remaining Qty" 
                                                    Binding="{Binding RemainingQuantity}" 
                                                    Width="120"/>
                        <controls:DataGridTextColumn Header="Qty Ordered" 
                                                    Binding="{Binding QtyOrdered, Converter={StaticResource DecimalToIntConverter}}" 
                                                    Width="120"/>
                        <controls:DataGridTextColumn Header="Line #" 
                                                    Binding="{Binding POLineNumber}" 
                                                    Width="80"/>
                    </controls:DataGrid.Columns>
                </controls:DataGrid>
            </Grid>
        </Border>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_POEntry.xaml.cs
```csharp
public sealed partial class View_Receiving_POEntry : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
_focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
⋮----
private void POTextBox_LostFocus(object sender, RoutedEventArgs e)
⋮----
ViewModel.PoTextBoxLostFocusCommand.Execute(null);
```

## File: Module_Receiving/Views/View_Receiving_Review.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Review"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Loaded="UserControl_Loaded">

    <UserControl.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

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
                        <TextBlock Style="{ThemeResource SubtitleTextBlockStyle}"
                                  Foreground="White">
                            <Run Text="Entry"/>
                            <Run Text="{x:Bind ViewModel.DisplayIndex, Mode=OneWay}" 
                                 FontWeight="Bold"/>
                            <Run Text="of"/>
                            <Run Text="{x:Bind ViewModel.Loads.Count, Mode=OneWay}" 
                                 FontWeight="Bold"/>
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
                            <StackPanel Grid.Column="0" Spacing="12" Margin="0,0,12,0">
                            <!-- Load Number (Read-only) -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="Load Number" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.LoadNumber, Mode=OneWay}" 
                                        IsReadOnly="True"
                                        Background="{ThemeResource ControlFillColorDisabledBrush}"/>
                            </StackPanel>

                            <!-- PO Number -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8A5;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Purchase Order Number" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PoNumber, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Part ID -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8B7;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Part ID" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PartID, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Remaining Quantity -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE9F9;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Remaining Quantity" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.RemainingQuantity, Mode=OneWay}" 
                                        IsReadOnly="True"
                                        Foreground="{ThemeResource SystemAccentColor}"
                                        FontWeight="SemiBold"/>
                            </StackPanel>

                            <!-- Weight/Quantity -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xEA8B;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Weight/Quantity" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.WeightQuantity, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>
                        </StackPanel>

                        <StackPanel Grid.Column="1" Spacing="12" Margin="12,0,0,0">
                            <!-- Heat/Lot Number -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE8B4;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Heat/Lot Number" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.HeatLotNumber, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Packages Per Load -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="Packages Per Load" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PackagesPerLoad, Mode=OneWay}" 
                                        IsReadOnly="True"/>
                            </StackPanel>

                            <!-- Package Type -->
                            <StackPanel Spacing="8">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" 
                                             FontSize="14"
                                             Foreground="{ThemeResource AccentTextFillColorSecondaryBrush}"/>
                                    <TextBlock Text="Package Type" 
                                              Style="{ThemeResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                                <TextBox Text="{x:Bind ViewModel.CurrentEntry.PackageTypeName, Mode=OneWay}" 
                                        IsReadOnly="True"/>
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
                                      ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}"
                                      AutoGenerateColumns="False"
                                      GridLinesVisibility="Horizontal"
                                      HeadersVisibility="Column"
                                      CanUserSortColumns="False"
                                      SelectionMode="Single"
                                      AutomationProperties.Name="Receiving Entries Table">
                        <controls:DataGrid.Columns>
                            <controls:DataGridTextColumn Header="Load #" 
                                                        Binding="{Binding LoadNumber}" 
                                                        IsReadOnly="True" 
                                                        Width="80"/>
                            <controls:DataGridTextColumn Header="PO Number" 
                                                        Binding="{Binding PoNumber, TargetNullValue='N/A'}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Part ID" 
                                                        Binding="{Binding PartID}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Remaining Qty" 
                                                        Binding="{Binding RemainingQuantity}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Weight/Qty" 
                                                        Binding="{Binding WeightQuantity}" 
                                                        Width="120"/>
                            <controls:DataGridTextColumn Header="Heat/Lot #" 
                                                        Binding="{Binding HeatLotNumber}" 
                                                        Width="150"/>
                            <controls:DataGridTextColumn Header="Pkgs" 
                                                        Binding="{Binding PackagesPerLoad}" 
                                                        Width="80"/>
                            <controls:DataGridTextColumn Header="Pkg Type" 
                                                        Binding="{Binding PackageTypeName}" 
                                                        Width="100"/>
                        </controls:DataGrid.Columns>
                    </controls:DataGrid>
                </Border>

                <!-- Navigation Bar for Table View -->
                <CommandBar Grid.Row="1" DefaultLabelPosition="Right" Margin="0,16,0,0">
                    <AppBarButton Label="Single View" 
                                 Command="{x:Bind ViewModel.SwitchToSingleViewCommand}"
                                 AutomationProperties.Name="Switch to Single View">
                        <AppBarButton.Icon>
                            <FontIcon Glyph="&#xE8A9;"/>
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
                   Command="{x:Bind ViewModel.AddAnotherPartCommand}"
                   AutomationProperties.Name="Add Another Part or PO"
                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.AddAnother'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE710;" FontSize="16"/>
                    <TextBlock Text="Add Another Part/PO"/>
                </StackPanel>
            </Button>
            
            <Button Grid.Column="2" 
                   Command="{x:Bind ViewModel.SaveCommand}"
                   Style="{StaticResource AccentButtonStyle}"
                   AutomationProperties.Name="Save to Database"
                   ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.SaveAndGenerate'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE74E;" FontSize="16"/>
                    <TextBlock Text="Save to Database"/>
                </StackPanel>
            </Button>
        </Grid>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_Review.xaml.cs
```csharp
public sealed partial class View_Receiving_Review : UserControl
⋮----
this.InitializeComponent();
⋮----
private async void UserControl_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_WeightQuantity.xaml
```
<UserControl
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_WeightQuantity"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <UserControl.Resources>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
        <converters:Converter_IntToVisibility x:Key="IntToVisibilityConverter"/>
        <converters:Converter_DoubleToDecimal x:Key="DoubleToDecimalConverter"/>
    </UserControl.Resources>

    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Info Header -->
        <Border Grid.Row="0" 
                Background="{ThemeResource AccentFillColorDefaultBrush}"
                Padding="16,12"
                CornerRadius="8"
                Margin="0,0,0,12">
            <StackPanel Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                <FontIcon Glyph="&#xEA8B;" FontSize="16" Foreground="White"/>
                <TextBlock Text="{x:Bind ViewModel.PoQuantityInfo, Mode=OneWay}" 
                           Style="{StaticResource BodyStrongTextBlockStyle}"
                           Foreground="White"/>
            </StackPanel>
        </Border>

        <!-- Warning Banner -->
        <InfoBar Grid.Row="1"
                 IsOpen="{x:Bind ViewModel.HasWarning, Mode=OneWay}"
                 Severity="Warning"
                 Message="{x:Bind ViewModel.WarningMessage, Mode=OneWay}"
                 IsClosable="False"
                 Margin="0,0,0,12"/>

        <!-- Loads List with Fixed Height -->
        <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Grid Margin="0,0,0,8" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" Padding="16,12" CornerRadius="8" BorderThickness="1" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}">
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="120"/>
                                <ColumnDefinition Width="*"/>
                            </Grid.ColumnDefinitions>
                            
                            <StackPanel Grid.Column="0" VerticalAlignment="Center">
                                <StackPanel Orientation="Horizontal" Spacing="8">
                                    <FontIcon Glyph="&#xE7B8;" FontSize="14" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
                                    <TextBlock Text="{Binding LoadNumber, Converter={StaticResource StringFormatConverter}, ConverterParameter='Load #{0}'}" 
                                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                                </StackPanel>
                            </StackPanel>
                            
                            <NumberBox Grid.Column="1"
                                       Header="Weight/Quantity"
                                       Value="{Binding WeightQuantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, Converter={StaticResource DoubleToDecimalConverter}}"
                                       Minimum="0"
                                       SmallChange="1"
                                       LargeChange="10"
                                       SpinButtonPlacementMode="Inline"
                                      PlaceholderText="Enter whole number"
                                       AutomationProperties.Name="Weight Quantity"/>
                        </Grid>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </ScrollViewer>
    </Grid>
</UserControl>
```

## File: Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs
```csharp
public sealed partial class View_Receiving_WeightQuantity : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this);
⋮----
private async void WeightQuantityView_Loaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.OnNavigatedToAsync();
```

## File: Module_Receiving/Views/View_Receiving_Workflow.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.View_Receiving_Workflow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Receiving.ViewModels"
    xmlns:views="using:MTM_Receiving_Application.Module_Receiving.Views"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d">

    <Page.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:Converter_BoolToString x:Key="BoolToStringConverter"/>
        <converters:Converter_StringFormat x:Key="StringFormatConverter"/>
    </Page.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Content Area -->
        <Grid Grid.Row="1" Margin="16,12,16,16">
            <!-- Mode Selection View -->
            <views:View_Receiving_ModeSelection Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <!-- Manual Entry View -->
            <views:View_Receiving_ManualEntry Visibility="{x:Bind ViewModel.IsManualEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>

            <!-- Edit Mode View -->
            <views:View_Receiving_EditMode Visibility="{x:Bind ViewModel.IsEditModeVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>

            <!-- PO Entry View -->
            <views:View_Receiving_POEntry Visibility="{x:Bind ViewModel.IsPOEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_LoadEntry Visibility="{x:Bind ViewModel.IsLoadEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_WeightQuantity Visibility="{x:Bind ViewModel.IsWeightQuantityEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_HeatLot Visibility="{x:Bind ViewModel.IsHeatLotEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_PackageType Visibility="{x:Bind ViewModel.IsPackageTypeEntryVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <views:View_Receiving_Review Visibility="{x:Bind ViewModel.IsReviewVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            
            <!-- Saving Progress -->
            <StackPanel Visibility="{x:Bind ViewModel.IsSavingVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="16">
                <ProgressRing IsActive="True" Width="64" Height="64"/>
                <TextBlock Text="{x:Bind ViewModel.SaveProgressMessage, Mode=OneWay}" 
                           Style="{StaticResource SubtitleTextBlockStyle}"
                           HorizontalAlignment="Center"/>
                <ProgressBar Value="{x:Bind ViewModel.SaveProgressValue, Mode=OneWay}" Maximum="100" Width="300"/>
            </StackPanel>

            <!-- Completion Summary -->
            <StackPanel Visibility="{x:Bind ViewModel.IsCompleteVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"
                        VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="24" MaxWidth="600">
                
                <FontIcon Glyph="&#xE930;" FontSize="64" Foreground="{ThemeResource SystemFillColorSuccessBrush}" 
                          Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <FontIcon Glyph="&#xE783;" FontSize="64" Foreground="{ThemeResource SystemFillColorCriticalBrush}" 
                          Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>

                <TextBlock Text="Success!" 
                           Style="{StaticResource TitleTextBlockStyle}" HorizontalAlignment="Center"
                           Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <TextBlock Text="Save Failed" 
                           Style="{StaticResource TitleTextBlockStyle}" HorizontalAlignment="Center"
                           Visibility="{x:Bind ViewModel.LastSaveResult.Success, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>

                <TextBlock Style="{StaticResource BodyStrongTextBlockStyle}" HorizontalAlignment="Center">
                    <Run Text="{x:Bind ViewModel.LastSaveResult.LoadsSaved, Mode=OneWay}"/>
                    <Run Text=" loads saved successfully."/>
                </TextBlock>

                <StackPanel Spacing="8" Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="16" CornerRadius="8">
                    <TextBlock Text="Save Details:" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Local CSV:"/>
                        <TextBlock Text="Saved" Foreground="Green" Visibility="{x:Bind ViewModel.LastSaveResult.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        <TextBlock Text="Failed" Foreground="Red" Visibility="{x:Bind ViewModel.LastSaveResult.LocalCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Network CSV:"/>
                        <TextBlock Text="Saved" Foreground="Green" Visibility="{x:Bind ViewModel.LastSaveResult.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        <TextBlock Text="Failed" Foreground="Red" Visibility="{x:Bind ViewModel.LastSaveResult.NetworkCSVSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>
                    </StackPanel>

                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Database:"/>
                        <TextBlock Text="Saved" Foreground="Green" Visibility="{x:Bind ViewModel.LastSaveResult.DatabaseSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                        <TextBlock Text="Failed" Foreground="Red" Visibility="{x:Bind ViewModel.LastSaveResult.DatabaseSuccess, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"/>
                    </StackPanel>
                </StackPanel>
                
                <StackPanel Orientation="Horizontal" Spacing="12" HorizontalAlignment="Center">
                    <Button Content="Start New Entry" Command="{x:Bind ViewModel.StartNewEntryCommand}" Style="{StaticResource AccentButtonStyle}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.StartNew'), Mode=OneWay}"/>
                    <Button Content="Reset CSV" Command="{x:Bind ViewModel.ResetCSVCommand}" ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ResetCSV'), Mode=OneWay}"/>
                </StackPanel>
            </StackPanel>
        </Grid>

        <!-- Navigation Buttons -->
        <Grid Grid.Row="2" Margin="24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <!-- Left-aligned buttons -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" HorizontalAlignment="Left" Spacing="12">
                <Button Content="Mode Selection" 
                        Command="{x:Bind ViewModel.ReturnToModeSelectionCommand}"
                        Background="Transparent" 
                        BorderBrush="{ThemeResource SystemAccentColor}"/>
                <Button Content="Reset CSV" 
                        Command="{x:Bind ViewModel.ResetCSVCommand}" 
                        Visibility="{x:Bind ViewModel.IsModeSelectionVisible, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter='Inverse'}"
                        Background="Transparent" 
                        BorderBrush="Transparent" 
                        Foreground="Red"
                        ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.ResetCSV'), Mode=OneWay}"/>
            </StackPanel>
            
            <!-- Center help button -->
            <Button Grid.Column="0"
                    Grid.ColumnSpan="2"
                    Canvas.ZIndex="1000"
                    HorizontalAlignment="Center"
                    Click="HelpButton_Click"
                    Style="{StaticResource AccentButtonStyle}"
                    Padding="12,8"
                    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.StepHelp'), Mode=OneWay}">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon Glyph="&#xE946;" FontSize="16"/>
                    <TextBlock Text="Help"/>
                </StackPanel>
            </Button>
            
            <!-- Right-aligned navigation buttons -->
            <StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right" Spacing="12">
                <Button Content="Back" Command="{x:Bind ViewModel.PreviousStepCommand}"/>
                <Button Content="Next" Command="{x:Bind ViewModel.NextStepCommand}" Style="{StaticResource AccentButtonStyle}"/>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Receiving/Contracts/IService_MySQL_PackagePreferences.cs
```csharp
public interface IService_MySQL_PackagePreferences
⋮----
public Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID);
public Task SavePreferenceAsync(Model_PackageTypePreference preference);
public Task<bool> DeletePreferenceAsync(string partID);
```

## File: Module_Receiving/Contracts/IService_MySQL_Receiving.cs
```csharp
public interface IService_MySQL_Receiving
⋮----
public Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads);
public Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate);
public Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate);
public Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads);
public Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads);
public Task<bool> TestConnectionAsync();
```

## File: Module_Receiving/Contracts/IService_MySQL_ReceivingLine.cs
```csharp
public interface IService_MySQL_ReceivingLine
⋮----
public Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line);
```

## File: Module_Receiving/Contracts/IService_ReceivingValidation.cs
```csharp
public interface IService_ReceivingValidation
⋮----
public Model_ReceivingValidationResult ValidatePONumber(string poNumber);
public Model_ReceivingValidationResult ValidatePartID(string partID);
public Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads);
public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity);
public Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad);
public Model_ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber);
public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(
⋮----
public Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(
⋮----
public Model_ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load);
public Model_ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads);
public Task<Model_ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID);
```

## File: Module_Receiving/Contracts/IService_ReceivingWorkflow.cs
```csharp
public interface IService_ReceivingWorkflow
⋮----
public void RaiseStatusMessage(string message);
public event EventHandler StepChanged;
⋮----
public Task<bool> StartWorkflowAsync();
public Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync();
public Model_ReceivingWorkflowStepResult GoToPreviousStep();
public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step);
public Task AddCurrentPartToSessionAsync();
public Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null);
public void ClearUIInputs();
public Task<Model_SaveResult> SaveToCSVOnlyAsync();
public Task<Model_SaveResult> SaveToDatabaseOnlyAsync();
public Task ResetWorkflowAsync();
public Task<Model_CSVDeleteResult> ResetCSVFilesAsync();
public Task PersistSessionAsync();
```

## File: Module_Receiving/Data/Dao_PackageTypePreference.cs
```csharp
public class Dao_PackageTypePreference
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<Model_UserPreference>> GetByUserAsync(string username)
⋮----
reader => new Model_UserPreference
⋮----
Username = reader["username"].ToString() ?? string.Empty,
PreferredPackageType = reader["preferred_package_type"].ToString() ?? string.Empty,
Workstation = reader["workstation"].ToString() ?? string.Empty,
LastUpdated = Convert.ToDateTime(reader["last_modified"])
⋮----
public async Task<Model_Dao_Result> UpsertAsync(Model_UserPreference preference)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
public async Task<Model_Dao_Result<Model_PackageTypePreference?>> GetPreferenceAsync(string partID)
⋮----
reader => new Model_PackageTypePreference
⋮----
PreferenceID = Convert.ToInt32(reader["PreferenceID"]),
PartID = reader["PartID"].ToString() ?? string.Empty,
PackageTypeName = reader["PackageTypeName"].ToString() ?? string.Empty,
CustomTypeName = reader["CustomTypeName"] == DBNull.Value ? null : reader["CustomTypeName"].ToString(),
LastModified = Convert.ToDateTime(reader["LastModified"])
⋮----
return Model_Dao_Result_Factory.Success(result.Data);
⋮----
public async Task<Model_Dao_Result> SavePreferenceAsync(Model_PackageTypePreference preference)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
return Model_Dao_Result_Factory.Success();
⋮----
return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
⋮----
public async Task<Model_Dao_Result<bool>> DeletePreferenceAsync(string partID)
⋮----
return Model_Dao_Result_Factory.Success(true);
```

## File: Module_Receiving/Data/Dao_ReceivingLoad.cs
```csharp
public class Dao_ReceivingLoad
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
private string? CleanPONumber(string? poNumber)
⋮----
if (string.IsNullOrEmpty(poNumber))
⋮----
return poNumber.Replace("PO-", "", StringComparison.OrdinalIgnoreCase).Trim();
⋮----
public async Task<Model_Dao_Result<int>> SaveLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
⋮----
{ "LoadID", load.LoadID.ToString() },
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
⋮----
throw new InvalidOperationException(result.ErrorMessage, result.Exception);
⋮----
await transaction.CommitAsync();
⋮----
await transaction.RollbackAsync();
⋮----
public async Task<Model_Dao_Result<int>> UpdateLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
public async Task<Model_Dao_Result<int>> DeleteLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
{ "p_LoadID", load.LoadID.ToString() }
⋮----
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetHistoryAsync(string partID, DateTime startDate, DateTime endDate)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
⋮----
loads.Add(MapRowToLoad(row));
⋮----
return Model_Dao_Result_Factory.Success(loads);
⋮----
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync(DateTime startDate, DateTime endDate)
⋮----
private Model_ReceivingLoad MapRowToLoad(DataRow row)
⋮----
return new Model_ReceivingLoad
⋮----
LoadID = Guid.Parse(row["LoadID"]?.ToString() ?? Guid.Empty.ToString()),
⋮----
PoNumber = row["PONumber"] == DBNull.Value ? null : row["PONumber"].ToString(),
⋮----
LoadNumber = Convert.ToInt32(row["LoadNumber"]),
WeightQuantity = Convert.ToDecimal(row["WeightQuantity"]),
⋮----
PackagesPerLoad = Convert.ToInt32(row["PackagesPerLoad"]),
⋮----
WeightPerPackage = Convert.ToDecimal(row["WeightPerPackage"]),
IsNonPOItem = Convert.ToBoolean(row["IsNonPOItem"]),
ReceivedDate = Convert.ToDateTime(row["ReceivedDate"])
```

## File: Module_Receiving/Models/Model_ReceivingLine.cs
```csharp
public class Model_ReceivingLine
```

## File: Module_Receiving/Models/Model_ReceivingLoad.cs
```csharp
public partial class Model_ReceivingLoad : ObservableObject
⋮----
private Guid _loadID = Guid.NewGuid();
⋮----
private Enum_PackageType _packageType = Enum_PackageType.Skid;
⋮----
private DateTime _receivedDate = DateTime.Now;
⋮----
partial void OnPartIDChanged(string value)
⋮----
if (string.IsNullOrWhiteSpace(value))
⋮----
var upperValue = value.ToUpperInvariant();
if (upperValue.Contains("MMC"))
⋮----
else if (upperValue.Contains("MMF"))
⋮----
System.Diagnostics.Debug.WriteLine($"[Model_ReceivingLoad] OnPartIDChanged error: {ex.Message}");
⋮----
partial void OnPackageTypeChanged(Enum_PackageType value)
⋮----
PackageTypeName = value.ToString();
⋮----
partial void OnPackageTypeNameChanged(string value)
⋮----
partial void OnWeightQuantityChanged(decimal value)
⋮----
partial void OnPackagesPerLoadChanged(int value)
⋮----
private void CalculateWeightPerPackage()
⋮----
WeightPerPackage = Math.Round(WeightQuantity / PackagesPerLoad, 0);
⋮----
string.IsNullOrEmpty(PoNumber) ? "N/A" : PoNumber;
```

## File: Module_Receiving/Models/Model_ReceivingSession.cs
```csharp
public class Model_ReceivingSession
⋮----
public Guid SessionID { get; set; } = Guid.NewGuid();
⋮----
Loads?.Select(l => l.PartID).Distinct().ToList() ?? new List<string>();
```

## File: Module_Receiving/Models/Model_ReceivingValidationResult.cs
```csharp
public class Model_ReceivingValidationResult
⋮----
public static Model_ReceivingValidationResult Success() => new() { IsValid = true };
public static Model_ReceivingValidationResult Error(string message) => new()
⋮----
public static Model_ReceivingValidationResult Warning(string message) => new()
```

## File: Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs
```csharp
public class Model_ReceivingWorkflowStepResult
⋮----
public static Model_ReceivingWorkflowStepResult SuccessResult(Enum_ReceivingWorkflowStep newStep, string message = "") => new()
⋮----
public static Model_ReceivingWorkflowStepResult ErrorResult(List<string> errors) => new()
```

## File: Module_Receiving/Services/Service_CSVWriter.cs
```csharp
public class Service_CSVWriter : IService_CSVWriter
⋮----
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_LoggingUtility _logger;
⋮----
_sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var appDir = Path.Combine(appDataPath, "MTM_Receiving_Application");
if (!Directory.Exists(appDir))
⋮----
Directory.CreateDirectory(appDir);
⋮----
_localCSVPath = Path.Combine(appDir, "ReceivingData.csv");
⋮----
private string GetNetworkCSVPathInternal()
⋮----
_logger.LogInfo("Resolving network CSV path...");
⋮----
var userDir = Path.Combine(networkBase, userName);
_logger.LogInfo($"Checking network directory: {userDir}");
if (!Directory.Exists(userDir))
⋮----
_logger.LogInfo($"Creating network directory: {userDir}");
Directory.CreateDirectory(userDir);
⋮----
_logger.LogWarning($"Failed to create network directory: {ex.Message}");
⋮----
return Path.Combine(userDir, "ReceivingData.csv");
⋮----
_logger.LogError($"Error resolving network path: {ex.Message}");
⋮----
public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Starting WriteToCSVAsync for {loads?.Count ?? 0} loads.");
⋮----
throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));
⋮----
var result = new Model_CSVWriteResult { RecordsWritten = loads.Count };
⋮----
_logger.LogInfo($"Writing to local CSV: {_localCSVPath}");
⋮----
_logger.LogInfo("Local CSV write successful.");
⋮----
_logger.LogError($"Local CSV write failed: {ex.Message}");
⋮----
throw new InvalidOperationException("Failed to write local CSV file", ex);
⋮----
_logger.LogInfo("Attempting network CSV write...");
var networkPath = await Task.Run(() => GetNetworkCSVPathInternal());
_logger.LogInfo($"Network path resolved: {networkPath}");
⋮----
_logger.LogInfo("Network CSV write successful.");
⋮----
_logger.LogWarning($"Network CSV write failed: {ex.Message}");
⋮----
public async Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true)
⋮----
_logger.LogInfo($"WriteToFileAsync called for: {filePath}, Append: {append}");
await Task.Run(async () =>
⋮----
bool isNewFile = !File.Exists(filePath) || !append;
_logger.LogInfo($"File exists check for {filePath}: {!isNewFile}");
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
⋮----
await using var stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.Read);
await using var writer = new StreamWriter(stream);
await using var csv = new CsvWriter(writer, config);
⋮----
csv.NextRecord();
⋮----
csv.WriteRecord(load);
⋮----
await writer.FlushAsync();
_logger.LogInfo($"Successfully wrote to {filePath}");
⋮----
_logger.LogError($"Error writing to file {filePath}: {ex.Message}");
⋮----
public async Task<List<Model_ReceivingLoad>> ReadFromCSVAsync(string filePath)
⋮----
_logger.LogInfo($"ReadFromCSVAsync called for: {filePath}");
return await Task.Run(() =>
⋮----
if (!File.Exists(filePath))
⋮----
throw new FileNotFoundException($"CSV file not found: {filePath}");
⋮----
using var reader = new StreamReader(filePath);
using var csv = new CsvReader(reader, config);
⋮----
_logger.LogInfo($"Successfully read {loads.Count} records from {filePath}");
⋮----
_logger.LogError($"Error reading from file {filePath}: {ex.Message}");
⋮----
public async Task<Model_CSVDeleteResult> ClearCSVFilesAsync()
⋮----
var result = new Model_CSVDeleteResult();
if (File.Exists(_localCSVPath))
⋮----
await Task.Run(() =>
⋮----
using var writer = new StreamWriter(_localCSVPath);
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
⋮----
if (File.Exists(networkPath))
⋮----
using var writer = new StreamWriter(networkPath);
⋮----
public async Task<Model_CSVExistenceResult> CheckCSVFilesExistAsync()
⋮----
var result = new Model_CSVExistenceResult();
⋮----
result.LocalExists = File.Exists(_localCSVPath);
⋮----
result.NetworkExists = File.Exists(networkPath);
⋮----
public string GetLocalCSVPath() => _localCSVPath;
public string GetNetworkCSVPath() => GetNetworkCSVPathInternal();
```

## File: Module_Receiving/Services/Service_MySQL_PackagePreferences.cs
```csharp
public class Service_MySQL_PackagePreferences : IService_MySQL_PackagePreferences
⋮----
private readonly Dao_PackageTypePreference _dao;
⋮----
_dao = dao ?? throw new ArgumentNullException(nameof(dao));
⋮----
_dao = new Dao_PackageTypePreference(connectionString);
⋮----
public async Task<Model_PackageTypePreference?> GetPreferenceAsync(string partID)
⋮----
if (string.IsNullOrWhiteSpace(partID))
⋮----
throw new ArgumentException("Part ID cannot be null or empty", nameof(partID));
⋮----
var result = await _dao.GetPreferenceAsync(partID);
⋮----
public async Task SavePreferenceAsync(Model_PackageTypePreference preference)
⋮----
throw new ArgumentNullException(nameof(preference));
⋮----
var result = await _dao.SavePreferenceAsync(preference);
⋮----
throw new Exception(result.ErrorMessage);
⋮----
public async Task<bool> DeletePreferenceAsync(string partID)
⋮----
var result = await _dao.DeletePreferenceAsync(partID);
```

## File: Module_Receiving/Services/Service_MySQL_Receiving.cs
```csharp
public class Service_MySQL_Receiving : IService_MySQL_Receiving
⋮----
private readonly Dao_ReceivingLoad _receivingLoadDao;
private readonly IService_LoggingUtility _logger;
⋮----
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
_receivingLoadDao = new Dao_ReceivingLoad(connectionString);
⋮----
public async Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Saving {loads.Count} loads to database.");
var result = await _receivingLoadDao.SaveLoadsAsync(loads);
⋮----
_logger.LogInfo($"Successfully saved {result.Data} loads.");
⋮----
_logger.LogError($"Failed to save loads: {result.ErrorMessage}", result.Exception);
throw new InvalidOperationException(result.ErrorMessage, result.Exception);
⋮----
public async Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Updating {loads.Count} loads in database.");
var result = await _receivingLoadDao.UpdateLoadsAsync(loads);
⋮----
_logger.LogInfo($"Successfully updated {result.Data} loads.");
⋮----
_logger.LogError($"Failed to update loads: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
⋮----
_logger.LogInfo($"Deleting {loads.Count} loads from database.");
var result = await _receivingLoadDao.DeleteLoadsAsync(loads);
⋮----
_logger.LogInfo($"Successfully deleted {result.Data} loads.");
⋮----
_logger.LogError($"Failed to delete loads: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate)
⋮----
var result = await _receivingLoadDao.GetHistoryAsync(partID, startDate, endDate);
⋮----
_logger.LogError($"Failed to get receiving history: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate)
⋮----
_logger.LogInfo($"Retrieving all receiving loads from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
var result = await _receivingLoadDao.GetAllAsync(startDate, endDate);
⋮----
_logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} receiving loads from database");
⋮----
_logger.LogError($"Failed to retrieve receiving loads: {result.ErrorMessage}", result.Exception);
⋮----
public async Task<bool> TestConnectionAsync()
```

## File: Module_Receiving/Services/Service_MySQL_ReceivingLine.cs
```csharp
public class Service_MySQL_ReceivingLine : IService_MySQL_ReceivingLine
⋮----
private readonly Dao_ReceivingLine _receivingLineDao;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
⋮----
var result = await _receivingLineDao.InsertReceivingLineAsync(line);
⋮----
_logger.LogError(
⋮----
_logger.LogInfo(
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
return Model_Dao_Result_Factory.Failure(
```

## File: Module_Receiving/Services/Service_ReceivingWorkflow.cs
```csharp
public class Service_ReceivingWorkflow : IService_ReceivingWorkflow
⋮----
private readonly IService_SessionManager _sessionManager;
private readonly IService_CSVWriter _csvWriter;
private readonly IService_MySQL_Receiving _mysqlReceiving;
private readonly IService_ReceivingValidation _validation;
private readonly IService_LoggingUtility _logger;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
public void RaiseStatusMessage(string message)
⋮----
private Enum_ReceivingWorkflowStep _currentStep = Enum_ReceivingWorkflowStep.ModeSelection;
⋮----
_logger.LogInfo($"Changing step from {_currentStep} to {value}");
⋮----
_logger.LogInfo($"Step changed to {value} (event fired)");
⋮----
_sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
_csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
_mysqlReceiving = mysqlReceiving ?? throw new ArgumentNullException(nameof(mysqlReceiving));
_validation = validation ?? throw new ArgumentNullException(nameof(validation));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_viewModelRegistry = viewModelRegistry ?? throw new ArgumentNullException(nameof(viewModelRegistry));
⋮----
public async Task<bool> StartWorkflowAsync()
⋮----
_logger.LogInfo("Starting receiving workflow.");
var existingSession = await _sessionManager.LoadSessionAsync();
⋮----
_logger.LogInfo("Restoring existing session.");
⋮----
CurrentSession = new Model_ReceivingSession();
⋮----
if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultReceivingMode))
⋮----
switch (currentUser.DefaultReceivingMode.ToLower())
⋮----
_logger.LogInfo("Starting in Guided mode (default)");
⋮----
_logger.LogInfo("Starting in Manual Entry mode (default)");
⋮----
_logger.LogInfo("Starting in Edit mode (default)");
⋮----
_logger.LogInfo("Invalid default mode, showing mode selection");
⋮----
_logger.LogInfo("No default mode set, showing mode selection");
⋮----
public async Task<Model_ReceivingWorkflowStepResult> AdvanceToNextStepAsync()
⋮----
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep);
⋮----
if (string.IsNullOrEmpty(CurrentPONumber) && !IsNonPOItem)
⋮----
validationErrors.Add("PO Number is required.");
return Model_ReceivingWorkflowStepResult.ErrorResult(validationErrors);
⋮----
validationErrors.Add("Part selection is required.");
⋮----
validationErrors.Add("Number of loads must be at least 1.");
⋮----
var result = _validation.ValidateWeightQuantity(load.WeightQuantity);
⋮----
validationErrors.Add($"Load {load.LoadNumber}: {result.Message}");
⋮----
var result = _validation.ValidateHeatLotNumber(load.HeatLotNumber);
⋮----
var result = _validation.ValidatePackageCount(load.PackagesPerLoad);
⋮----
if (string.IsNullOrWhiteSpace(load.PackageTypeName))
⋮----
validationErrors.Add($"Load {load.LoadNumber}: Package Type is required.");
⋮----
_logger.LogInfo("Transitioning from Review to Saving...");
⋮----
validationErrors.Add($"Cannot advance from step {CurrentStep}");
⋮----
_logger.LogInfo("Persisting session...");
⋮----
_logger.LogInfo("Session persisted.");
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Advanced to {CurrentStep}");
⋮----
private void GenerateLoads()
⋮----
CurrentSession.Loads.Remove(load);
⋮----
_currentBatchLoads.Clear();
⋮----
var load = new Model_ReceivingLoad
⋮----
CurrentSession.Loads.Add(load);
_currentBatchLoads.Add(load);
⋮----
public Model_ReceivingWorkflowStepResult GoToPreviousStep()
⋮----
return Model_ReceivingWorkflowStepResult.ErrorResult(new List<string> { $"Cannot go back from step {CurrentStep}" });
⋮----
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Returned to {CurrentStep}");
⋮----
public Model_ReceivingWorkflowStepResult GoToStep(Enum_ReceivingWorkflowStep step)
⋮----
return Model_ReceivingWorkflowStepResult.SuccessResult(CurrentStep, $"Navigated to {CurrentStep}");
⋮----
public async Task AddCurrentPartToSessionAsync()
⋮----
public void ClearUIInputs()
⋮----
_viewModelRegistry.ClearAllInputs();
⋮----
public async Task<Model_SaveResult> SaveToCSVOnlyAsync()
⋮----
var result = new Model_SaveResult();
var validation = _validation.ValidateSession(CurrentSession.Loads);
⋮----
var csvResult = await _csvWriter.WriteToCSVAsync(CurrentSession.Loads);
⋮----
result.LocalCSVPath = _csvWriter.GetLocalCSVPath();
result.NetworkCSVPath = _csvWriter.GetNetworkCSVPath();
⋮----
result.Errors.Add($"Local CSV write failed: {csvResult.LocalError}");
⋮----
result.Warnings.Add($"Network CSV write failed: {csvResult.NetworkError}");
⋮----
result.Errors.Add($"CSV save failed: {ex.Message}");
_logger.LogError("CSV save failed", ex);
⋮----
public async Task<Model_SaveResult> SaveToDatabaseOnlyAsync()
⋮----
int savedCount = await _mysqlReceiving.SaveReceivingLoadsAsync(CurrentSession.Loads);
⋮----
result.Errors.Add($"Database save failed: {ex.Message}");
_logger.LogError("Database save failed", ex);
⋮----
public async Task<Model_SaveResult> SaveSessionAsync(IProgress<string>? messageProgress = null, IProgress<int>? percentProgress = null)
⋮----
_logger.LogInfo("Starting session save.");
⋮----
_logger.LogInfo("Validating session before save...");
⋮----
_logger.LogWarning($"Session validation failed: {string.Join(", ", validation.Errors)}");
⋮----
_logger.LogInfo("Reporting progress: Saving to local CSV...");
⋮----
result.Errors.AddRange(csvResult.Errors);
result.Warnings.AddRange(csvResult.Warnings);
_logger.LogInfo("Reporting progress: Saving to database...");
⋮----
result.Errors.AddRange(dbResult.Errors);
⋮----
_logger.LogInfo("Reporting progress: Finalizing...");
⋮----
_logger.LogInfo("Save completed successfully. Clearing session.");
await _sessionManager.ClearSessionAsync();
CurrentSession.Loads.Clear();
await _csvWriter.ClearCSVFilesAsync();
⋮----
_logger.LogWarning($"Save completed with errors. Success: {result.Success}");
⋮----
_logger.LogError("Unexpected error during save session", ex);
⋮----
result.Errors.Add($"Unexpected error: {ex.Message}");
⋮----
public async Task ResetWorkflowAsync()
⋮----
public async Task<Model_CSVDeleteResult> ResetCSVFilesAsync()
⋮----
_logger.LogInfo("Resetting CSV files requested.");
return await _csvWriter.ClearCSVFilesAsync();
⋮----
public async Task PersistSessionAsync()
⋮----
await _sessionManager.SaveSessionAsync(CurrentSession);
```

## File: Module_Receiving/Services/Service_SessionManager.cs
```csharp
public class Service_SessionManager : IService_SessionManager
⋮----
private readonly IService_LoggingUtility _logger;
⋮----
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var appFolder = Path.Combine(appDataPath, "MTM_Receiving_Application");
if (!Directory.Exists(appFolder))
⋮----
Directory.CreateDirectory(appFolder);
⋮----
_sessionPath = Path.Combine(appFolder, "session.json");
⋮----
public async Task SaveSessionAsync(Model_ReceivingSession session)
⋮----
_logger.LogInfo("SaveSessionAsync started.");
⋮----
throw new ArgumentNullException(nameof(session));
⋮----
var options = new JsonSerializerOptions
⋮----
_logger.LogInfo("Serializing session...");
var json = JsonSerializer.Serialize(session, options);
_logger.LogInfo($"Writing session to file: {_sessionPath}");
await File.WriteAllTextAsync(_sessionPath, json);
_logger.LogInfo("Session saved successfully.");
⋮----
_logger.LogError($"Failed to save session to file: {ex.Message}", ex);
throw new InvalidOperationException("Failed to save session to file", ex);
⋮----
public async Task<Model_ReceivingSession?> LoadSessionAsync()
⋮----
if (!File.Exists(_sessionPath))
⋮----
var json = await File.ReadAllTextAsync(_sessionPath);
⋮----
File.Delete(_sessionPath);
⋮----
throw new InvalidOperationException("Failed to load session from file", ex);
⋮----
public async Task<bool> ClearSessionAsync()
⋮----
await Task.Run(() => File.Delete(_sessionPath));
⋮----
public bool SessionExists()
⋮----
return File.Exists(_sessionPath);
⋮----
public string GetSessionFilePath()
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs
```csharp
public partial class ViewModel_Receiving_EditMode : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_MySQL_Receiving _mysqlService;
private readonly IService_CSVWriter _csvWriter;
private readonly IService_Pagination _paginationService;
private readonly IService_Help _helpService;
⋮----
private Enum_DataSourceType _currentDataSource = Enum_DataSourceType.Memory;
⋮----
private DateTimeOffset _filterStartDate = DateTimeOffset.Now.AddDays(-7);
⋮----
private DateTimeOffset _filterEndDate = DateTimeOffset.Now;
⋮----
private string _thisMonthButtonText = DateTime.Now.ToString("MMMM");
⋮----
private readonly IService_Window _windowService;
⋮----
_logger.LogInfo("Edit Mode initialized");
⋮----
private static string GetQuarterText(DateTime date)
⋮----
private void OnPageChanged(object? sender, EventArgs e)
⋮----
private void UpdatePagedDisplay()
⋮----
Loads.Clear();
⋮----
Loads.Add(item);
⋮----
private void NotifyPaginationCommands()
⋮----
NextPageCommand.NotifyCanExecuteChanged();
PreviousPageCommand.NotifyCanExecuteChanged();
FirstPageCommand.NotifyCanExecuteChanged();
LastPageCommand.NotifyCanExecuteChanged();
GoToPageCommand.NotifyCanExecuteChanged();
⋮----
private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
⋮----
private void Load_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
⋮----
RemoveRowCommand.NotifyCanExecuteChanged();
⋮----
private void NotifyCommands()
⋮----
SaveCommand.NotifyCanExecuteChanged();
⋮----
SelectAllCommand.NotifyCanExecuteChanged();
⋮----
partial void OnFilterStartDateChanged(DateTimeOffset value) => ApplyDateFilter();
partial void OnFilterEndDateChanged(DateTimeOffset value) => ApplyDateFilter();
private void ApplyDateFilter()
⋮----
private void FilterAndPaginate()
⋮----
var end = FilterEndDate.Date.AddDays(1).AddTicks(-1);
⋮----
_filteredLoads = _allLoads.Where(l => l.ReceivedDate >= start && l.ReceivedDate <= end).ToList();
⋮----
_paginationService.SetSource(_filteredLoads);
⋮----
private async Task SetFilterLastWeekAsync()
⋮----
FilterStartDate = DateTime.Today.AddDays(-7);
⋮----
private async Task SetFilterTodayAsync()
⋮----
private async Task SetFilterThisWeekAsync()
⋮----
var start = today.AddDays(-(int)today.DayOfWeek);
var end = start.AddDays(6);
⋮----
private async Task SetFilterThisMonthAsync()
⋮----
FilterStartDate = new DateTime(today.Year, today.Month, 1);
FilterEndDate = FilterStartDate.AddMonths(1).AddDays(-1);
⋮----
private async Task SetFilterThisQuarterAsync()
⋮----
FilterStartDate = new DateTime(today.Year, 3 * quarter - 2, 1);
FilterEndDate = FilterStartDate.AddMonths(3).AddDays(-1);
⋮----
private async Task SetFilterShowAllAsync()
⋮----
FilterStartDate = DateTime.Today.AddYears(-1);
⋮----
private void PreviousPage() => _paginationService.PreviousPage();
⋮----
private void NextPage() => _paginationService.NextPage();
⋮----
private void FirstPage() => _paginationService.FirstPage();
⋮----
private void LastPage() => _paginationService.LastPage();
⋮----
private void GoToPage() => _paginationService.GoToPage(GotoPageNumber);
private bool CanGoNext() => _paginationService.HasNextPage;
private bool CanGoPrevious() => _paginationService.HasPreviousPage;
⋮----
private async Task LoadFromCurrentMemoryAsync()
⋮----
_logger.LogInfo("Loading data from current memory");
⋮----
_deletedLoads.Clear();
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
_allLoads.Clear();
⋮----
_allLoads.Add(load);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from current memory");
⋮----
FilterStartDate = DateTimeOffset.Now.AddYears(-1);
⋮----
_logger.LogError($"Failed to load from current memory: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to load data from current session", Enum_ErrorSeverity.Error, ex);
⋮----
private async Task LoadFromCurrentLabelsAsync()
⋮----
_logger.LogInfo("User initiated Current Labels (CSV) load");
⋮----
await _errorHandler.ShowErrorDialogAsync(
⋮----
_logger.LogError($"Failed to load from labels: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to load data from label file", Enum_ErrorSeverity.Error, ex);
⋮----
private async Task<bool> TryLoadFromDefaultCsvAsync()
⋮----
string localPath = _csvWriter.GetLocalCSVPath();
if (File.Exists(localPath))
⋮----
_logger.LogInfo($"Attempting to load from local CSV: {localPath}");
var loadedData = await _csvWriter.ReadFromCSVAsync(localPath);
⋮----
_workflowService.CurrentSession.Loads.Add(load);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from local labels");
⋮----
_logger.LogWarning($"Failed to load from local labels: {ex.Message}");
⋮----
string networkPath = _csvWriter.GetNetworkCSVPath();
if (File.Exists(networkPath))
⋮----
_logger.LogInfo($"Attempting to load from network labels: {networkPath}");
var loadedData = await _csvWriter.ReadFromCSVAsync(networkPath);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from network labels");
⋮----
_logger.LogWarning($"Failed to load from network labels: {ex.Message}");
⋮----
private async Task LoadFromHistoryAsync()
⋮----
_logger.LogInfo("User initiated history load");
⋮----
_logger.LogInfo($"Loading receiving history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
var result = await _mysqlService.GetAllReceivingLoadsAsync(startDate, endDate);
⋮----
_logger.LogInfo($"Successfully loaded {_allLoads.Count} loads from history");
⋮----
_logger.LogError($"Failed to load from history: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to load data from history", Enum_ErrorSeverity.Error, ex);
⋮----
private void SelectAll()
⋮----
bool anyUnselected = Loads.Any(l => !l.IsSelected);
⋮----
private bool CanSelectAll() => Loads.Count > 0;
⋮----
private void RemoveRow()
⋮----
var selectedLoads = Loads.Where(l => l.IsSelected).ToList();
⋮----
_logger.LogInfo($"Removing {selectedLoads.Count} selected loads");
⋮----
_deletedLoads.Add(load);
_workflowService.CurrentSession.Loads.Remove(load);
_allLoads.Remove(load);
_filteredLoads.Remove(load);
Loads.Remove(load);
⋮----
_logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
_deletedLoads.Add(SelectedLoad);
_workflowService.CurrentSession.Loads.Remove(SelectedLoad);
_allLoads.Remove(SelectedLoad);
_filteredLoads.Remove(SelectedLoad);
Loads.Remove(SelectedLoad);
⋮----
_logger.LogWarning("RemoveRow called with no selected load(s)");
⋮----
private bool CanRemoveRow() => Loads.Any(l => l.IsSelected);
⋮----
private async Task SaveAsync()
⋮----
_logger.LogInfo($"Validating and saving {_filteredLoads.Count} loads from edit mode");
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
var errorMessage = string.Join("\n", validationErrors);
_logger.LogWarning($"Edit mode validation failed: {validationErrors.Count} errors");
⋮----
await _workflowService.AdvanceToNextStepAsync();
⋮----
if (string.IsNullOrEmpty(_currentCsvPath))
⋮----
await _errorHandler.HandleErrorAsync("No label file path available for saving.", Enum_ErrorSeverity.Error);
⋮----
_logger.LogInfo($"Overwriting label file: {_currentCsvPath}");
await _csvWriter.WriteToFileAsync(_currentCsvPath, _allLoads, append: false);
⋮----
await _errorHandler.ShowErrorDialogAsync("Success", "Label file updated successfully.", Enum_ErrorSeverity.Info);
⋮----
_logger.LogInfo("Updating history records");
⋮----
_logger.LogInfo($"Deleting {_deletedLoads.Count} removed records");
deleted = await _mysqlService.DeleteReceivingLoadsAsync(_deletedLoads);
⋮----
updated = await _mysqlService.UpdateReceivingLoadsAsync(_filteredLoads);
⋮----
await _errorHandler.ShowErrorDialogAsync("Success", $"History updated successfully.\n{updated} records updated.\n{deleted} records deleted.", Enum_ErrorSeverity.Info);
⋮----
_logger.LogInfo($"Edit mode save completed successfully for source: {CurrentDataSource}");
⋮----
_logger.LogError($"Failed to save edit mode data: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
⋮----
private bool CanSave()
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, resetting workflow");
await _workflowService.ResetWorkflowAsync();
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to reset workflow: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
⋮----
_logger.LogInfo("User cancelled return to mode selection");
⋮----
private System.Collections.Generic.List<string> ValidateLoads(IEnumerable<Model_ReceivingLoad> loadsToValidate)
⋮----
if (!loadsToValidate.Any() && _deletedLoads.Count == 0)
⋮----
errors.Add("No loads to save");
⋮----
if (string.IsNullOrWhiteSpace(load.PartID))
⋮----
errors.Add($"Load #{load.LoadNumber}: Part ID is required");
⋮----
errors.Add($"Load #{load.LoadNumber}: Weight/Quantity must be greater than zero");
⋮----
errors.Add($"Load #{load.LoadNumber}: Packages per load must be greater than zero");
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.EditMode");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs
```csharp
public partial class ViewModel_Receiving_ManualEntry : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_MySQL_Receiving _mysqlService;
private readonly IService_Window _windowService;
private readonly IService_Help _helpService;
⋮----
private async Task AutoFillAsync()
⋮----
_logger.LogInfo("Starting Auto-Fill Blank Spaces operation");
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.PartID))
⋮----
if (!string.IsNullOrWhiteSpace(prev.PartID))
⋮----
if (!string.IsNullOrWhiteSpace(currentLoad.PartID))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.PoNumber) && !string.IsNullOrWhiteSpace(sourceLoad.PoNumber))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(sourceLoad.HeatLotNumber))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.PackageTypeName) && !string.IsNullOrWhiteSpace(sourceLoad.PackageTypeName))
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.UserId))
⋮----
if (i > 0 && !string.IsNullOrWhiteSpace(Loads[i - 1].UserId))
⋮----
else if (Loads.Count > 0 && !string.IsNullOrWhiteSpace(Loads[0].UserId))
⋮----
_logger.LogInfo($"Auto-Fill Blank Spaces completed. Updated {filledCount} fields across {Loads.Count} rows.");
⋮----
_logger.LogError($"Auto-fill failed: {ex.Message}");
await _errorHandler.HandleErrorAsync("Auto-fill failed", Enum_ErrorSeverity.Error, ex);
⋮----
private void AddRow()
⋮----
private async Task AddMultipleRowsAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
var result = await dialog.ShowAsync();
⋮----
if (int.TryParse(inputTextBox.Text, out int count) && count > 0 && count <= 50)
⋮----
_logger.LogInfo($"Adding {count} new rows to manual entry grid");
⋮----
_logger.LogWarning($"Invalid row count entered: {inputTextBox.Text}");
await _errorHandler.HandleErrorAsync("Please enter a valid number between 1 and 50.", Enum_ErrorSeverity.Warning);
⋮----
public void AddNewLoad()
⋮----
var newLoad = new Model_ReceivingLoad
⋮----
LoadID = System.Guid.NewGuid(),
⋮----
Loads.Add(newLoad);
_workflowService.CurrentSession.Loads.Add(newLoad);
⋮----
private void RemoveRow()
⋮----
_logger.LogInfo($"Removing load {SelectedLoad.LoadNumber} (Part ID: {SelectedLoad.PartID})");
_workflowService.CurrentSession.Loads.Remove(SelectedLoad);
Loads.Remove(SelectedLoad);
⋮----
_logger.LogWarning("RemoveRow called with no selected load");
⋮----
private async Task SaveAsync()
⋮----
_logger.LogInfo($"Saving {Loads.Count} loads from manual entry");
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
await _workflowService.AdvanceToNextStepAsync();
⋮----
_logger.LogError($"Failed to save manual entry data: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to save receiving data", Enum_ErrorSeverity.Critical, ex);
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var result = await dialog.ShowAsync().AsTask();
⋮----
_logger.LogInfo("User confirmed return to mode selection, resetting workflow");
await _workflowService.ResetWorkflowAsync();
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
⋮----
_logger.LogError($"Failed to reset workflow: {ex.Message}");
await _errorHandler.HandleErrorAsync("Failed to return to mode selection", Enum_ErrorSeverity.Error, ex);
⋮----
_logger.LogInfo("User cancelled return to mode selection");
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.ManualEntry");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_PackageType.cs
```csharp
public partial class ViewModel_Receiving_PackageType : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_MySQL_PackagePreferences _preferencesService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
public async Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private async Task LoadPreferencesAsync()
⋮----
if (string.IsNullOrEmpty(partId))
⋮----
var preference = await _preferencesService.GetPreferenceAsync(partId);
⋮----
if (PackageTypes.Contains(preference.PackageTypeName))
⋮----
if (partID.StartsWith("MMC", StringComparison.OrdinalIgnoreCase))
⋮----
else if (partID.StartsWith("MMF", StringComparison.OrdinalIgnoreCase))
⋮----
partial void OnSelectedPackageTypeChanged(string value)
⋮----
SavePreferenceAsync().ConfigureAwait(false);
⋮----
partial void OnCustomPackageTypeNameChanged(string value)
⋮----
partial void OnIsSaveAsDefaultChanged(bool value)
⋮----
DeletePreferenceAsync().ConfigureAwait(false);
⋮----
private void UpdateLoadsPackageType()
⋮----
private async Task SavePreferenceAsync()
⋮----
_workflowService.RaiseStatusMessage("Part ID too long (max 50 chars).");
⋮----
if (string.IsNullOrWhiteSpace(typeName))
⋮----
_workflowService.RaiseStatusMessage("Please enter a package type name.");
⋮----
_workflowService.RaiseStatusMessage("Package type name too long (max 50 chars).");
⋮----
if (!_regex.IsMatch(typeName))
⋮----
_workflowService.RaiseStatusMessage("Invalid characters in package type name.");
⋮----
var preference = new Model_PackageTypePreference
⋮----
await _preferencesService.SavePreferenceAsync(preference);
_workflowService.RaiseStatusMessage("Preference saved.");
⋮----
await _errorHandler.HandleErrorAsync(msg, Enum_ErrorSeverity.Warning, ex);
⋮----
private async Task DeletePreferenceAsync()
⋮----
await _preferencesService.DeletePreferenceAsync(partId);
_workflowService.RaiseStatusMessage("Preference deleted.");
⋮----
await _errorHandler.HandleErrorAsync("Failed to delete preference", Enum_ErrorSeverity.Warning, ex);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.PackageType");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs
```csharp
public sealed partial class View_Receiving_ManualEntry : UserControl
⋮----
private readonly IService_Focus _focusService;
⋮----
this.InitializeComponent();
⋮----
_focusService.AttachFocusOnVisibility(this, AddRowButton);
⋮----
private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
⋮----
Debug.WriteLine("[ManualEntryView] Loads_CollectionChanged: New row added");
ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[ManualEntryView] Loads_CollectionChanged: Selecting new item LoadNumber={newItem.LoadNumber}");
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
Debug.WriteLine($"[ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
grid.BeginEdit();
⋮----
private void ManualEntryDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
⋮----
var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
⋮----
Debug.WriteLine($"[ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
⋮----
private void ManualEntryDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
⋮----
Debug.WriteLine($"[ManualEntryView] Tapped: OriginalSource={e.OriginalSource}");
⋮----
Debug.WriteLine("[ManualEntryView] Tapped: Grid empty, triggering AddRow command.");
if (ViewModel.AddRowCommand.CanExecute(null))
⋮----
ViewModel.AddRowCommand.Execute(null);
⋮----
Debug.WriteLine("[ManualEntryView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
⋮----
Debug.WriteLine("[ManualEntryView] Tapped: Cell clicked. Enqueuing BeginEdit.");
grid.DispatcherQueue.TryEnqueue(() =>
⋮----
Debug.WriteLine($"[ManualEntryView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
⋮----
private void SelectFirstEditableCell(DataGrid grid)
⋮----
Debug.WriteLine("[ManualEntryView] SelectFirstEditableCell: Starting");
⋮----
var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
⋮----
Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
⋮----
grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
⋮----
Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
⋮----
Debug.WriteLine("[ManualEntryView] SelectFirstEditableCell: No editable column found.");
⋮----
Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
```

## File: Module_Receiving/Data/Dao_ReceivingLine.cs
```csharp
public class Dao_ReceivingLine
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
⋮----
new MySqlParameter("@p_Quantity", line.Quantity),
new MySqlParameter("@p_PartID", line.PartID ?? string.Empty),
new MySqlParameter("@p_PONumber", line.PONumber),
new MySqlParameter("@p_EmployeeNumber", line.EmployeeNumber),
new MySqlParameter("@p_Heat", line.Heat ?? string.Empty),
new MySqlParameter("@p_Date", line.Date),
new MySqlParameter("@p_InitialLocation", line.InitialLocation ?? string.Empty),
new MySqlParameter("@p_CoilsOnSkid", (object?)line.CoilsOnSkid ?? DBNull.Value),
new MySqlParameter("@p_VendorName", line.VendorName ?? "Unknown"),
new MySqlParameter("@p_PartDescription", line.PartDescription ?? string.Empty),
new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
⋮----
if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
⋮----
return new Model_Dao_Result
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteAsync(
```

## File: Module_Receiving/Services/Service_ReceivingValidation.cs
```csharp
public class Service_ReceivingValidation : IService_ReceivingValidation
⋮----
private readonly IService_InforVisual _inforVisualService;
private static readonly Regex _regex = new Regex(@"^(PO-)?\d{1,6}$", RegexOptions.IgnoreCase);
⋮----
_inforVisualService = inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
⋮----
public Model_ReceivingValidationResult ValidatePONumber(string poNumber)
⋮----
if (string.IsNullOrWhiteSpace(poNumber))
⋮----
return Model_ReceivingValidationResult.Error("PO number is required");
⋮----
if (!_regex.IsMatch(poNumber))
⋮----
return Model_ReceivingValidationResult.Error("PO number must be numeric (up to 6 digits) or in PO-###### format");
⋮----
return Model_ReceivingValidationResult.Success();
⋮----
public Model_ReceivingValidationResult ValidatePartID(string partID)
⋮----
if (string.IsNullOrWhiteSpace(partID))
⋮----
return Model_ReceivingValidationResult.Error("Part ID is required");
⋮----
return Model_ReceivingValidationResult.Error("Part ID cannot exceed 50 characters");
⋮----
public Model_ReceivingValidationResult ValidateNumberOfLoads(int numLoads)
⋮----
return Model_ReceivingValidationResult.Error("Number of loads must be at least 1");
⋮----
return Model_ReceivingValidationResult.Error("Number of loads cannot exceed 99");
⋮----
public Model_ReceivingValidationResult ValidateWeightQuantity(decimal weightQuantity)
⋮----
return Model_ReceivingValidationResult.Error("Weight/Quantity must be greater than 0");
⋮----
public Model_ReceivingValidationResult ValidatePackageCount(int packagesPerLoad)
⋮----
return Model_ReceivingValidationResult.Error("Package count must be greater than 0");
⋮----
public Model_ReceivingValidationResult ValidateHeatLotNumber(string heatLotNumber)
⋮----
if (string.IsNullOrWhiteSpace(heatLotNumber))
⋮----
return Model_ReceivingValidationResult.Error("Heat/Lot number is required");
⋮----
return Model_ReceivingValidationResult.Error("Heat/Lot number cannot exceed 50 characters");
⋮----
public Task<Model_ReceivingValidationResult> ValidateAgainstPOQuantityAsync(decimal totalQuantity, decimal orderedQuantity, string partID)
⋮----
return Task.FromResult(Model_ReceivingValidationResult.Warning(
⋮----
return Task.FromResult(Model_ReceivingValidationResult.Success());
⋮----
public async Task<Model_ReceivingValidationResult> CheckSameDayReceivingAsync(string poNumber, string partID, decimal userEnteredQuantity)
⋮----
var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partID, DateTime.Today);
⋮----
return Model_ReceivingValidationResult.Warning(
⋮----
public Model_ReceivingValidationResult ValidateReceivingLoad(Model_ReceivingLoad load)
⋮----
if (string.IsNullOrWhiteSpace(load.PartID))
⋮----
errors.Add("Part ID is required");
⋮----
if (string.IsNullOrWhiteSpace(load.PartType))
⋮----
errors.Add("Part Type is required");
⋮----
errors.Add("Load number must be at least 1");
⋮----
errors.Add($"Load {load.LoadNumber}: Weight/Quantity must be greater than 0");
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
errors.Add($"Load {load.LoadNumber}: Heat/Lot number is required");
⋮----
errors.Add($"Load {load.LoadNumber}: Package count must be greater than 0");
⋮----
if (string.IsNullOrWhiteSpace(load.PackageTypeName))
⋮----
errors.Add($"Load {load.LoadNumber}: Package type is required");
⋮----
var result = Model_ReceivingValidationResult.Error(string.Join("; ", errors));
⋮----
public Model_ReceivingValidationResult ValidateSession(List<Model_ReceivingLoad> loads)
⋮----
return Model_ReceivingValidationResult.Error("Session must contain at least one load");
⋮----
allErrors.AddRange(loadValidation.Errors);
⋮----
var result = Model_ReceivingValidationResult.Error($"{allErrors.Count} validation error(s) found");
⋮----
public async Task<Model_ReceivingValidationResult> ValidatePartExistsInVisualAsync(string partID)
⋮----
var result = await _inforVisualService.GetPartByIDAsync(partID);
⋮----
return Model_ReceivingValidationResult.Error($"Error validating part: {result.ErrorMessage}");
⋮----
return Model_ReceivingValidationResult.Error($"Part ID {partID} not found in Infor Visual database");
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_HeatLot.cs
```csharp
public partial class ViewModel_Receiving_HeatLot : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
public Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private void AutoFill()
⋮----
if (string.IsNullOrWhiteSpace(currentLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(prevLoad.HeatLotNumber))
⋮----
private Task ValidateAndContinueAsync()
⋮----
private void PrepareHeatLotFields()
⋮----
if (string.IsNullOrWhiteSpace(load.HeatLotNumber))
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.HeatLot");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs
```csharp
public partial class ViewModel_Receiving_LoadEntry : ViewModel_Shared_Base, IResettableViewModel
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
_viewModelRegistry.Register(this);
⋮----
public void ResetToDefaults()
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
private async Task CreateLoadsAsync()
⋮----
var validationResult = _validationService.ValidateNumberOfLoads(NumberOfLoads);
⋮----
await _errorHandler.HandleErrorAsync(validationResult.Message, Enum_ErrorSeverity.Warning);
⋮----
partial void OnNumberOfLoadsChanged(int value)
⋮----
public void UpdatePartInfo(string partId, string description)
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.LoadEntry");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs
```csharp
public partial class ViewModel_Receiving_POEntry : ViewModel_Shared_Base, IResettableViewModel
⋮----
private readonly IService_InforVisual _inforVisualService;
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_Help _helpService;
private readonly IService_ViewModelRegistry _viewModelRegistry;
⋮----
_viewModelRegistry.Register(this);
⋮----
public void ResetToDefaults()
⋮----
Parts.Clear();
⋮----
private void PoTextBoxLostFocus()
⋮----
if (string.IsNullOrWhiteSpace(PoNumber))
⋮----
string value = PoNumber.Trim().ToUpper();
if (value.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
⋮----
string numberPart = value.Substring(3);
if (numberPart.All(char.IsDigit) && numberPart.Length <= 6)
⋮----
PoNumber = $"PO-{numberPart.PadLeft(6, '0')}";
⋮----
else if (value.All(char.IsDigit) && value.Length <= 6)
⋮----
PoNumber = $"PO-{value.PadLeft(6, '0')}";
⋮----
private async Task LoadPOAsync()
⋮----
await _errorHandler.HandleErrorAsync("Please enter a PO number.", Enum_ErrorSeverity.Warning);
⋮----
var result = await _inforVisualService.GetPOWithPartsAsync(PoNumber);
⋮----
var remainingQtyResult = await _inforVisualService.GetRemainingQuantityAsync(PoNumber, part.PartID);
⋮----
Parts.Add(part);
⋮----
_workflowService.RaiseStatusMessage($"Purchase Order {PoNumber} loaded with {Parts.Count} parts.");
⋮----
var errorMessage = !string.IsNullOrWhiteSpace(result.ErrorMessage)
⋮----
await _errorHandler.HandleErrorAsync(errorMessage, Enum_ErrorSeverity.Error);
⋮----
private void ToggleNonPO()
⋮----
private async Task LookupPartAsync()
⋮----
if (string.IsNullOrWhiteSpace(PartID))
⋮----
await _errorHandler.HandleErrorAsync("Please enter a Part ID.", Enum_ErrorSeverity.Warning);
⋮----
var result = await _inforVisualService.GetPartByIDAsync(PartID);
⋮----
Parts.Add(result.Data);
_workflowService.RaiseStatusMessage($"Part {PartID} found.");
⋮----
await _errorHandler.HandleErrorAsync(result.ErrorMessage ?? "Part not found.", Enum_ErrorSeverity.Error);
⋮----
partial void OnPoNumberChanged(string value)
⋮----
if (string.IsNullOrWhiteSpace(value))
⋮----
string validatedPO = value.Trim();
⋮----
if (validatedPO.StartsWith("po-", StringComparison.OrdinalIgnoreCase))
⋮----
string numberPart = validatedPO.Substring(3);
if (numberPart.All(char.IsDigit))
⋮----
validatedPO = $"PO-{numberPart.PadLeft(6, '0')}";
⋮----
else if (validatedPO.All(char.IsDigit))
⋮----
validatedPO = $"PO-{validatedPO.PadLeft(6, '0')}";
⋮----
partial void OnPartIDChanged(string value)
⋮----
var upperPart = value.Trim().ToUpper();
if (upperPart.StartsWith("MMC"))
⋮----
else if (upperPart.StartsWith("MMF"))
⋮----
partial void OnSelectedPartChanged(Model_InforVisualPart? value)
⋮----
if (value != null && !string.IsNullOrWhiteSpace(value.PartID))
⋮----
var upperPart = value.PartID.Trim().ToUpper();
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.POEntry");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
⋮----
private async void InitializeAsync()
⋮----
await _logger.LogInfoAsync($"[MOCK DATA MODE] Auto-filling PO number: {defaultPO}");
⋮----
_workflowService.RaiseStatusMessage($"[MOCK DATA] Auto-filled PO: {defaultPO}");
await Task.Delay(500);
⋮----
await _logger.LogErrorAsync($"Error during initialization: {ex.Message}", ex);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_Review.cs
```csharp
public partial class ViewModel_Receiving_Review : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_Help _helpService;
private readonly IService_Window _windowService;
⋮----
private void OnStepChanged(object? sender, System.EventArgs e)
⋮----
public async Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private void PreviousEntry()
⋮----
private void NextEntry()
⋮----
private void SwitchToTableView()
⋮----
private void SwitchToSingleView()
⋮----
private async Task AddAnotherPartAsync()
⋮----
_logger.LogInfo("User requested to add another part/PO");
⋮----
_logger.LogInfo("User cancelled add another part/PO");
⋮----
var saveResult = await _workflowService.SaveToCSVOnlyAsync();
⋮----
await _errorHandler.HandleErrorAsync(
$"Failed to save CSV backup: {string.Join(", ", saveResult.Errors)}",
⋮----
await _workflowService.AddCurrentPartToSessionAsync();
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
_logger.LogInfo("Navigated to PO Entry for new part, workflow data cleared");
⋮----
_logger.LogError($"Error in AddAnotherPartAsync: {ex.Message}", ex);
⋮----
private async Task<bool> ConfirmAddAnotherAsync()
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
private void ClearTransientWorkflowData()
⋮----
_workflowService.ClearUIInputs();
_logger.LogInfo("Transient workflow data and UI inputs cleared for new entry");
⋮----
_logger.LogError($"Error clearing transient workflow data: {ex.Message}", ex);
⋮----
private async Task SaveAsync()
⋮----
await _workflowService.AdvanceToNextStepAsync();
⋮----
public void HandleCascadingUpdate(Model_ReceivingLoad changedLoad, string propertyName)
⋮----
var index = Loads.IndexOf(changedLoad);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.Review");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_WeightQuantity.cs
```csharp
public partial class ViewModel_Receiving_WeightQuantity : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_ReceivingValidation _validationService;
private readonly IService_InforVisual _inforVisualService;
private readonly IService_Help _helpService;
⋮----
private void OnStepChanged(object? sender, EventArgs e)
⋮----
public async Task OnNavigatedToAsync()
⋮----
Loads.Clear();
⋮----
Loads.Add(load);
⋮----
private void UpdatePOQuantityInfo()
⋮----
private async Task CheckSameDayReceivingAsync()
⋮----
if (string.IsNullOrEmpty(poNumber) || string.IsNullOrEmpty(partId))
⋮----
var result = await _inforVisualService.GetSameDayReceivingQuantityAsync(poNumber, partId, DateTime.Today);
⋮----
await _errorHandler.LogErrorAsync("Failed to check same-day receiving", Enum_ErrorSeverity.Warning, ex);
⋮----
private async Task ValidateAndContinueAsync()
⋮----
var result = _validationService.ValidateWeightQuantity(load.WeightQuantity);
⋮----
await _errorHandler.HandleErrorAsync($"Load {load.LoadNumber}: {result.Message}", Enum_ErrorSeverity.Warning);
⋮----
var totalWeight = Loads.Sum(l => l.WeightQuantity);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.WeightQuantity");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs
```csharp
public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_Help _helpService;
⋮----
private readonly IService_Dispatcher _dispatcherService;
private readonly IService_Window _windowService;
⋮----
private void OnWorkflowStepChanged(object? sender, EventArgs e)
⋮----
_logger.LogInfo("StepChanged event received in ViewModel. Updating visibility.");
⋮----
_logger.LogInfo("Step is Saving. Enqueuing PerformSaveAsync via Dispatcher.");
_dispatcherService.TryEnqueue(async () =>
⋮----
HelpContent = Helper_WorkflowHelpContentGenerator.GenerateHelpContent(_workflowService.CurrentStep);
_logger.LogInfo($"Visibility updated. Current Step: {_workflowService.CurrentStep}, Title: {CurrentStepTitle}");
⋮----
private async Task NextStepAsync()
⋮----
_logger.LogInfo("NextStepAsync command triggered.");
var result = await _workflowService.AdvanceToNextStepAsync();
_logger.LogInfo($"AdvanceToNextStepAsync returned. Success: {result.Success}, Step: {_workflowService.CurrentStep}");
⋮----
await _errorHandler.HandleErrorAsync(
string.Join("\n", result.ValidationErrors),
⋮----
_logger.LogError($"Error in NextStepAsync: {ex.Message}", ex);
await _errorHandler.HandleErrorAsync($"An error occurred: {ex.Message}", Enum_ErrorSeverity.Error);
⋮----
private async Task PerformSaveAsync()
⋮----
_logger.LogInfo("PerformSaveAsync called but already saving. Ignoring.");
⋮----
_logger.LogInfo("PerformSaveAsync started.");
⋮----
_logger.LogInfo($"Save progress message: {msg}");
⋮----
_logger.LogInfo($"Save progress percent: {pct}");
⋮----
_logger.LogInfo("Calling _workflowService.SaveSessionAsync...");
LastSaveResult = await _workflowService.SaveSessionAsync(messageProgress, percentProgress);
_logger.LogInfo($"SaveSessionAsync returned. Success: {LastSaveResult.Success}");
await _workflowService.AdvanceToNextStepAsync();
⋮----
_logger.LogError($"Error in PerformSaveAsync: {ex.Message}", ex);
await _errorHandler.HandleErrorAsync($"Save failed: {ex.Message}", Enum_ErrorSeverity.Error);
⋮----
private async Task StartNewEntryAsync()
⋮----
await _workflowService.ResetWorkflowAsync();
⋮----
private async Task ResetCSVAsync()
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
_logger.LogError("Cannot show dialog: XamlRoot is null");
await _errorHandler.HandleErrorAsync("Unable to display dialog", Enum_ErrorSeverity.Error);
⋮----
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
var saveResult = await _workflowService.SaveToDatabaseOnlyAsync();
⋮----
var warnDialog = new ContentDialog
⋮----
Content = $"Failed to save to database: {string.Join(", ", saveResult.Errors)}\n\nDo you want to proceed with deleting CSV files anyway?",
⋮----
var warnResult = await warnDialog.ShowAsync();
⋮----
var deleteResult = await _workflowService.ResetCSVFilesAsync();
⋮----
private void PreviousStep()
⋮----
var result = _workflowService.GoToPreviousStep();
⋮----
private async Task ReturnToModeSelectionAsync()
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ModeSelection);
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.Workflow");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```

## File: Module_Receiving/Views/View_Receiving_Workflow.xaml.cs
```csharp
public sealed partial class View_Receiving_Workflow : Page
⋮----
this.InitializeComponent();
⋮----
private async void HelpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
⋮----
protected override void OnNavigatedTo(NavigationEventArgs e)
⋮----
base.OnNavigatedTo(e);
⋮----
if (!string.IsNullOrEmpty(defaultMode))
⋮----
workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
⋮----
workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
```

## File: Module_Receiving/ViewModels/ViewModel_Receiving_ModeSelection.cs
```csharp
public partial class ViewModel_Receiving_ModeSelection : ViewModel_Shared_Base
⋮----
private readonly IService_ReceivingWorkflow _workflowService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_UserPreferences _userPreferencesService;
private readonly IService_Help _helpService;
private readonly IService_Window _windowService;
⋮----
private void LoadDefaultMode()
⋮----
private async Task SelectGuidedModeAsync()
⋮----
_logger.LogInfo("User selected Guided Mode.");
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.POEntry);
⋮----
private async Task SelectManualModeAsync()
⋮----
_logger.LogInfo("User selected Manual Mode.");
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.ManualEntry);
⋮----
private async Task SelectEditModeAsync()
⋮----
_logger.LogInfo("User selected Edit Mode.");
⋮----
_workflowService.GoToStep(Enum_ReceivingWorkflowStep.EditMode);
⋮----
private bool HasUnsavedData()
⋮----
if (!string.IsNullOrEmpty(_workflowService.CurrentSession.PoNumber) ||
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
_logger.LogInfo("Workflow data and UI inputs cleared for mode change");
⋮----
_logger.LogError($"Error clearing workflow data: {ex.Message}", ex);
⋮----
private void ClearAllUIInputs()
⋮----
_logger.LogInfo("UI inputs cleared across all Receiving ViewModels");
⋮----
_logger.LogError($"Error clearing UI inputs: {ex.Message}", ex);
⋮----
private async Task SetGuidedAsDefaultAsync(bool isChecked)
⋮----
var result = await _userPreferencesService.UpdateDefaultReceivingModeAsync(currentUser.WindowsUsername, newMode ?? "");
⋮----
// Update in-memory user object
⋮----
// Update UI state
⋮----
_logger.LogInfo($"Default mode set to: {newMode ?? "none"}");
⋮----
await _errorHandler.ShowErrorDialogAsync("Save Error", result.ErrorMessage, Enum_ErrorSeverity.Error);
⋮----
await _errorHandler.HandleErrorAsync($"Failed to set default mode: {ex.Message}",
⋮----
private async Task SetManualAsDefaultAsync(bool isChecked)
⋮----
private async Task SetEditAsDefaultAsync(bool isChecked)
⋮----
private async Task ShowHelpAsync()
⋮----
await _helpService.ShowHelpAsync("Receiving.ModeSelection");
⋮----
public string GetTooltip(string key) => _helpService.GetTooltip(key);
public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
public string GetTip(string key) => _helpService.GetTip(key);
```
