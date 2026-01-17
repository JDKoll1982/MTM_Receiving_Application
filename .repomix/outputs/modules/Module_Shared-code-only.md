# User Provided Header
MTM Receiving Application - Module_Shared Code-Only Export

# Files

## File: Module_Shared/ViewModels/ViewModel_Shared_Base.cs
```csharp
public abstract partial class ViewModel_Shared_Base : ObservableObject
⋮----
protected readonly IService_ErrorHandler _errorHandler;
protected readonly IService_LoggingUtility _logger;
⋮----
private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;
⋮----
public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
```

## File: Module_Shared/ViewModels/ViewModel_Shared_HelpDialog.cs
```csharp
public partial class ViewModel_Shared_HelpDialog : ViewModel_Shared_Base
⋮----
private readonly IService_Help _helpService;
⋮----
public async Task LoadHelpContentAsync(Model_HelpContent content)
⋮----
IsDismissed = await _helpService.IsDismissedAsync(content.Key);
⋮----
RelatedTopics.Clear();
⋮----
var relatedContent = _helpService.GetHelpContent(relatedKey);
⋮----
RelatedTopics.Add(relatedContent);
⋮----
_errorHandler.HandleException(
⋮----
private async Task ViewRelatedTopicAsync()
⋮----
public async Task LoadRelatedTopicAsync(Model_HelpContent? relatedContent)
⋮----
private async Task CopyContentAsync()
⋮----
if (HelpContent != null && !string.IsNullOrEmpty(HelpContent.Content))
⋮----
dataPackage.SetText(HelpContent.Content);
Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
⋮----
await _logger.LogInfoAsync("Help content copied to clipboard");
⋮----
partial void OnIsDismissedChanged(bool value)
⋮----
if (HelpContent != null && !string.IsNullOrEmpty(HelpContent.Key))
⋮----
_ = _helpService.SetDismissedAsync(HelpContent.Key, value);
```

## File: Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs
```csharp
public partial class ViewModel_Shared_NewUserSetup : ViewModel_Shared_Base
⋮----
private readonly IService_Authentication _authService;
⋮----
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task LoadDepartmentsAsync()
⋮----
var departmentList = await _authService.GetActiveDepartmentsAsync();
Departments.Clear();
⋮----
Departments.Add(dept);
⋮----
_logger.LogInfo($"Loaded {Departments.Count} departments for new user setup");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
public async Task<bool> CreateAccountAsync()
⋮----
if (!int.TryParse(EmployeeNumber, out int empNum) || empNum <= 0)
⋮----
var pinValidation = await _authService.ValidatePinAsync(Pin, empNum);
⋮----
var newUser = new Model_User
⋮----
var result = await _authService.CreateNewUserAsync(newUser, CreatedBy, progress);
⋮----
_logger.LogInfo($"New user account created: {FullName} (Emp #{NewEmployeeNumber}) by {CreatedBy}");
⋮----
_logger.LogWarning($"Failed to create account for {WindowsUsername}: {ErrorMessage}");
⋮----
public bool ValidatePinFormat(string pin)
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
if (!char.IsDigit(c))
⋮----
public bool ValidatePinMatch(string pin, string confirmPin)
⋮----
return !string.IsNullOrWhiteSpace(pin) && pin == confirmPin;
⋮----
public bool ValidateFullName(string fullName)
⋮----
return !string.IsNullOrWhiteSpace(fullName) && fullName.Trim().Length >= 2;
```

## File: Module_Shared/ViewModels/ViewModel_Shared_SharedTerminalLogin.cs
```csharp
public partial class ViewModel_Shared_SharedTerminalLogin : ViewModel_Shared_Base
⋮----
private readonly IService_Authentication _authService;
⋮----
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task<bool> LoginAsync()
⋮----
if (string.IsNullOrWhiteSpace(Username))
⋮----
if (string.IsNullOrWhiteSpace(Pin))
⋮----
var result = await _authService.AuthenticateByPinAsync(Username, Pin, progress);
⋮----
_logger.LogInfo($"PIN authentication successful for user: {Username}");
⋮----
_logger.LogWarning($"PIN authentication failed for user: {Username}. Attempt {AttemptCount}");
⋮----
await _errorHandler.HandleErrorAsync(
```

## File: Module_Shared/ViewModels/ViewModel_Shared_SplashScreen.cs
```csharp
public partial class ViewModel_Shared_SplashScreen : ViewModel_Shared_Base
⋮----
public void UpdateProgress(double percentage, string message)
⋮----
public void SetIndeterminate(string message)
⋮----
public void Reset()
```

## File: Module_Shared/Views/View_Shared_HelpDialog.xaml
```
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_HelpDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:models="using:MTM_Receiving_Application.Module_Core.Models.Core"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="{x:Bind ViewModel.HelpContent.Title, Mode=OneWay}"
    PrimaryButtonText="Close"
    DefaultButton="Primary"
    Width="700"
    Height="600">

    <ContentDialog.Resources>
        <Style x:Key="HelpHeaderStyle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="24"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Margin" Value="12,0,0,0"/>
        </Style>
        
        <Style x:Key="HelpContentStyle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="TextWrapping" Value="Wrap"/>
            <Setter Property="LineHeight" Value="22"/>
        </Style>

        <Style x:Key="RelatedTopicStyle" TargetType="TextBlock">
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Margin" Value="8,4"/>
        </Style>
    </ContentDialog.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header Section -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,16">
            <mi:MaterialIcon 
                Kind="{x:Bind ViewModel.HelpContent.IconKind, Mode=OneWay}"
                Width="32" 
                Height="32"
                Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
            <TextBlock 
                Text="{x:Bind ViewModel.HelpContent.Title, Mode=OneWay}"
                Style="{StaticResource HelpHeaderStyle}"
                VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Content Section -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <TextBlock 
                Text="{x:Bind ViewModel.HelpContent.Content, Mode=OneWay}"
                Style="{StaticResource HelpContentStyle}"/>
        </ScrollViewer>

        <!-- Related Topics Section -->
        <Expander 
            Grid.Row="2"
            Header="Related Topics"
            Margin="0,16,0,0"
            HorizontalAlignment="Stretch"
            Visibility="{x:Bind ViewModel.IsRelatedHelpAvailable, Mode=OneWay}">
            <ListView 
                ItemsSource="{x:Bind ViewModel.RelatedTopics, Mode=OneWay}"
                SelectionMode="None"
                IsItemClickEnabled="True"
                ItemClick="RelatedTopics_ItemClick">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_HelpContent">
                        <StackPanel Orientation="Horizontal">
                            <mi:MaterialIcon 
                                Kind="{x:Bind IconKind}"
                                Width="20" 
                                Height="20"
                                Margin="0,0,8,0"/>
                            <TextBlock 
                                Text="{x:Bind Title}"
                                Style="{StaticResource RelatedTopicStyle}"/>
                        </StackPanel>
                    </DataTemplate>
                </ListView.ItemTemplate>
                <ListView.ItemContainerStyle>
                    <Style TargetType="ListViewItem">
                        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
                    </Style>
                </ListView.ItemContainerStyle>
            </ListView>
        </Expander>

        <!-- Footer Section -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" HorizontalAlignment="Stretch" Margin="0,12,0,0">
            <CheckBox 
                Content="Don't show this again"
                IsChecked="{x:Bind ViewModel.IsDismissed, Mode=TwoWay}"
                Visibility="{x:Bind ViewModel.CanDismiss, Mode=OneWay}"
                HorizontalAlignment="Left"/>
            
            <Button 
                Content="Copy"
                Command="{x:Bind ViewModel.CopyContentCommand}"
                HorizontalAlignment="Right"
                Margin="12,0,0,0">
                <Button.KeyboardAccelerators>
                    <KeyboardAccelerator Key="C" Modifiers="Control"/>
                </Button.KeyboardAccelerators>
            </Button>
        </StackPanel>
    </Grid>
</ContentDialog>
```

## File: Module_Shared/Views/View_Shared_HelpDialog.xaml.cs
```csharp
public sealed partial class View_Shared_HelpDialog : ContentDialog
⋮----
public void SetHelpContent(Model_HelpContent content)
⋮----
_ = ViewModel.LoadHelpContentAsync(content);
⋮----
private async void RelatedTopics_ItemClick(object sender, Microsoft.UI.Xaml.Controls.ItemClickEventArgs e)
⋮----
await ViewModel.LoadRelatedTopicAsync(relatedContent);
```

## File: Module_Shared/Views/View_Shared_IconSelectorWindow.xaml
```
<Window
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_IconSelectorWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:materialIcons="using:Material.Icons.WinUI3"
    mc:Ignorable="d"
    Title="Select Icon">

    <Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Custom Title Bar -->
        <Grid x:Name="AppTitleBar" Grid.Row="0" Height="32" Background="Transparent">
            <TextBlock Text="Icon Library" 
                       VerticalAlignment="Center" 
                       Margin="16,0,0,0"
                       Style="{StaticResource CaptionTextBlockStyle}"/>
        </Grid>

        <!-- Header & Search -->
        <Grid Grid.Row="1" Padding="32,16,32,16" Background="{ThemeResource LayerFillColorDefaultBrush}" BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}" BorderThickness="0,0,0,1">
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                
                <TextBox x:Name="SearchBox" 
                         Grid.Column="0"
                         PlaceholderText="Search icons (e.g. 'box', 'truck')..." 
                         TextChanged="OnSearchTextChanged"
                         Height="36"
                         CornerRadius="4"/>
                         
                <CheckBox x:Name="ShowAllIconsCheckBox" 
                          Grid.Column="1"
                          Content="Show all icons" 
                          Checked="OnShowAllIconsChanged"
                          Unchecked="OnShowAllIconsChanged"
                          VerticalAlignment="Center"/>
            </Grid>
        </Grid>

        <!-- Icon Grid -->
        <Viewbox Grid.Row="2" Stretch="Uniform">
            <GridView x:Name="IconGridView" 
                      Padding="16"
                      SelectionMode="Single"
                      IsItemClickEnabled="True"
                      ItemClick="OnIconItemClick"
                      ScrollViewer.VerticalScrollBarVisibility="Disabled"
                      Width="900" Height="650">
                <GridView.ItemsPanel>
                    <ItemsPanelTemplate>
                        <ItemsWrapGrid MaximumRowsOrColumns="4" Orientation="Horizontal" HorizontalAlignment="Center"/>
                    </ItemsPanelTemplate>
                </GridView.ItemsPanel>
                <GridView.ItemTemplate>
                    <DataTemplate>
                        <Border Width="200" Height="140"
                                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                BorderThickness="1" 
                                CornerRadius="8"
                                Margin="8"
                                ToolTipService.ToolTip="{Binding Name}">
                            <VisualStateManager.VisualStateGroups>
                                <VisualStateGroup x:Name="CommonStates">
                                    <VisualState x:Name="Normal" />
                                    <VisualState x:Name="PointerOver">
                                        <Storyboard>
                                            <ObjectAnimationUsingKeyFrames Storyboard.TargetName="ContentPanel" Storyboard.TargetProperty="Background">
                                                <DiscreteObjectKeyFrame KeyTime="0" Value="{ThemeResource CardBackgroundFillColorSecondaryBrush}" />
                                            </ObjectAnimationUsingKeyFrames>
                                        </Storyboard>
                                    </VisualState>
                                </VisualStateGroup>
                            </VisualStateManager.VisualStateGroups>
                            
                            <StackPanel x:Name="ContentPanel" VerticalAlignment="Center" HorizontalAlignment="Center" Spacing="12" Padding="12">
                                <materialIcons:MaterialIcon Kind="{Binding Kind}" 
                                          Width="40" Height="40"
                                          Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                                <TextBlock Text="{Binding Name}" 
                                           FontSize="14" 
                                           TextWrapping="Wrap" 
                                           TextAlignment="Center"
                                           HorizontalAlignment="Center"
                                           MaxLines="2"
                                           TextTrimming="CharacterEllipsis"/>
                            </StackPanel>
                        </Border>
                    </DataTemplate>
                </GridView.ItemTemplate>
            </GridView>
        </Viewbox>

        <!-- Footer -->
        <Grid Grid.Row="3" Padding="32,20" Background="{ThemeResource LayerFillColorDefaultBrush}" BorderBrush="{ThemeResource DividerStrokeColorDefaultBrush}" BorderThickness="0,1,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <Button x:Name="CancelButton" Grid.Column="0" Content="Cancel" Click="OnCancelClick" Width="100" Height="36" CornerRadius="4" HorizontalAlignment="Left"/>
            
            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8" HorizontalAlignment="Center">
                <Button x:Name="FirstPageButton" Click="OnFirstPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="First Page">
                    <materialIcons:MaterialIcon Kind="PageFirst" Width="16" Height="16"/>
                </Button>
                <Button x:Name="PreviousPageButton" Click="OnPreviousPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="Previous Page">
                    <materialIcons:MaterialIcon Kind="ChevronLeft" Width="16" Height="16"/>
                </Button>
                
                <Border Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" CornerRadius="4" Padding="16,0" Height="36" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" BorderThickness="1">
                    <TextBlock x:Name="PageInfoTextBlock" VerticalAlignment="Center" Text="Page 1 of 1" FontWeight="SemiBold"/>
                </Border>
                
                <Button x:Name="NextPageButton" Click="OnNextPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="Next Page">
                    <materialIcons:MaterialIcon Kind="ChevronRight" Width="16" Height="16"/>
                </Button>
                <Button x:Name="LastPageButton" Click="OnLastPageClick" Width="44" Height="36" CornerRadius="4" ToolTipService.ToolTip="Last Page">
                    <materialIcons:MaterialIcon Kind="PageLast" Width="16" Height="16"/>
                </Button>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

## File: Module_Shared/Views/View_Shared_IconSelectorWindow.xaml.cs
```csharp
public sealed partial class View_Shared_IconSelectorWindow : Window
⋮----
Debug.WriteLine($"Failed to get logging service: {ex.Message}");
⋮----
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
⋮----
appWindow.Resize(windowSize);
var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
⋮----
appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });
⋮----
public void SetInitialSelection(MaterialIconKind iconKind)
⋮----
DispatcherQueue.TryEnqueue(() =>
⋮----
var iconInfo = _allIcons.FirstOrDefault(i => i.Kind == iconKind);
⋮----
IconGridView.ScrollIntoView(iconInfo);
⋮----
public Task<MaterialIconKind?> WaitForSelectionAsync()
⋮----
private void LoadIcons()
⋮----
var icons = Helper_MaterialIcons.GetAllIcons();
_allIcons = icons.ConvertAll(k => new IconInfo(k.ToString(), k));
⋮----
.Where(i => commonIconNames.Contains(i.Name))
.ToList();
⋮----
Debug.WriteLine($"Error loading icons: {ex}");
⋮----
private void UpdateGridView()
⋮----
_totalPages = (int)Math.Ceiling((double)_filteredIcons.Count / ICONS_PER_PAGE);
⋮----
.Skip((_currentPage - 1) * ICONS_PER_PAGE)
.Take(ICONS_PER_PAGE)
⋮----
private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
⋮----
var searchText = SearchBox.Text.ToLower().Trim();
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
.Where(i => i.Name.ToLower().Contains(searchText))
⋮----
private void OnShowAllIconsChanged(object sender, RoutedEventArgs e)
⋮----
private void OnIconItemClick(object sender, ItemClickEventArgs e)
⋮----
private void OnPreviousPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnNextPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnFirstPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnLastPageClick(object sender, RoutedEventArgs e)
⋮----
private void OnCancelClick(object sender, RoutedEventArgs e)
⋮----
public class IconInfo
```

## File: Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_NewUserSetupDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Create Your Account"
    PrimaryButtonText="Create Account"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="480">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header with Icon and Welcome -->
        <StackPanel Grid.Row="0" Spacing="8" Padding="20,16,20,12" Background="{ThemeResource LayerFillColorDefaultBrush}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                
                <FontIcon Grid.Column="0" 
                          FontFamily="{StaticResource SymbolThemeFontFamily}" 
                          Glyph="&#xE77B;" 
                          FontSize="32"
                          Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                          VerticalAlignment="Center"
                          Margin="0,0,16,0"/>
                
                <StackPanel Grid.Column="1" VerticalAlignment="Center">
                    <TextBlock Text="Welcome to MTM Receiving" 
                               Style="{StaticResource SubtitleTextBlockStyle}"
                               FontWeight="SemiBold"/>
                    <TextBlock Text="Let's set up your account to get started" 
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                               Style="{StaticResource CaptionTextBlockStyle}"/>
                </StackPanel>
            </Grid>
        </StackPanel>

        <!-- Main Content - Using Grid for compact two-column layout with ScrollViewer -->
        <ScrollViewer Grid.Row="1" 
                      VerticalScrollBarVisibility="Auto" 
                      HorizontalScrollBarVisibility="Disabled"
                      MaxHeight="480">
            <Grid Padding="20,12" RowSpacing="12" ColumnSpacing="12">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>

            <!-- Row 0: Full Name (spans both columns) -->
            <StackPanel Grid.Row="0" Grid.ColumnSpan="2" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE77B;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Full Name *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <TextBox x:Name="FullNameTextBox"
                         PlaceholderText="Enter your full name"
                         MaxLength="100"
                         TabIndex="0"/>
            </StackPanel>

            <!-- Row 1: Employee Number & Windows Username -->
            <StackPanel Grid.Row="1" Grid.Column="0" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE8D4;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Employee Number *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <TextBox x:Name="EmployeeNumberTextBox"
                         PlaceholderText="e.g., 6229"
                         MaxLength="10"
                         TabIndex="1"
                         InputScope="Number"/>
            </StackPanel>

            <StackPanel Grid.Row="1" Grid.Column="1" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE910;" 
                              FontSize="16"
                              Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    <TextBlock Text="Windows Username" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"
                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                </StackPanel>
                <TextBox x:Name="WindowsUsernameTextBox"
                         IsReadOnly="True"
                         Background="{ThemeResource ControlAltFillColorSecondaryBrush}"
                         TabIndex="-1"/>
            </StackPanel>

            <!-- Row 2: Department & Shift -->
            <StackPanel Grid.Row="2" Grid.Column="0" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE716;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Department *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <ComboBox x:Name="DepartmentComboBox"
                          PlaceholderText="Select department"
                          HorizontalAlignment="Stretch"
                          TabIndex="2"
                          SelectionChanged="DepartmentComboBox_SelectionChanged"/>
            </StackPanel>

            <StackPanel Grid.Row="2" Grid.Column="1" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE823;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Shift *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <ComboBox x:Name="ShiftComboBox"
                          PlaceholderText="Select shift"
                          HorizontalAlignment="Stretch"
                          TabIndex="4">
                    <ComboBoxItem Content="1st Shift"/>
                    <ComboBoxItem Content="2nd Shift"/>
                    <ComboBoxItem Content="3rd Shift"/>
                </ComboBox>
            </StackPanel>

            <!-- Row 3: Custom Department (Visible when "Other" selected, spans both columns) -->
            <StackPanel x:Name="CustomDepartmentPanel" 
                        Grid.Row="3" 
                        Grid.ColumnSpan="2" 
                        Spacing="6" 
                        Visibility="Collapsed">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE70F;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Custom Department Name *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <TextBox x:Name="CustomDepartmentTextBox"
                         PlaceholderText="Enter department name"
                         MaxLength="50"
                         TabIndex="3"/>
            </StackPanel>

            <!-- Row 4: PIN Fields -->
            <StackPanel Grid.Row="4" Grid.Column="0" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE72E;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="4-Digit PIN *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <PasswordBox x:Name="PinPasswordBox"
                             PlaceholderText="Create PIN"
                             MaxLength="4"
                             TabIndex="5"
                             PasswordRevealMode="Peek"/>
                <TextBlock Text="For shared terminal login" 
                           Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                           Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>

            <StackPanel Grid.Row="4" Grid.Column="1" Spacing="6">
                <StackPanel Orientation="Horizontal" Spacing="8">
                    <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                              Glyph="&#xE8C9;" 
                              FontSize="16"
                              Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
                    <TextBlock Text="Confirm PIN *" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                </StackPanel>
                <PasswordBox x:Name="ConfirmPinPasswordBox"
                             PlaceholderText="Re-enter PIN"
                             MaxLength="4"
                             TabIndex="6"
                             PasswordRevealMode="Peek"/>
            </StackPanel>

            <!-- Row 5: ERP Credentials Section (Optional) -->
            <Expander x:Name="ErpExpander"
                      Grid.Row="5" 
                      Grid.ColumnSpan="2"
                      Header="ERP System Access (Optional)"
                      IsExpanded="False"
                      HorizontalAlignment="Stretch"
                      HorizontalContentAlignment="Stretch"
                      VerticalContentAlignment="Stretch">
                <Expander.HeaderTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <FontIcon FontFamily="{StaticResource SymbolThemeFontFamily}" 
                                      Glyph="&#xE89F;" 
                                      FontSize="16"/>
                            <TextBlock Text="{Binding}" Style="{StaticResource BodyStrongTextBlockStyle}"/>
                        </StackPanel>
                    </DataTemplate>
                </Expander.HeaderTemplate>
                
                <StackPanel Spacing="12" Padding="12,8,0,0">
                    <CheckBox x:Name="ConfigureErpCheckBox"
                              Content="Configure Visual/Infor ERP credentials"
                              Checked="ConfigureErpCheckBox_Checked"
                              Unchecked="ConfigureErpCheckBox_Unchecked"
                              TabIndex="7"/>

                    <StackPanel x:Name="ErpCredentialsPanel" Spacing="12" Visibility="Collapsed">
                        <InfoBar Severity="Warning" 
                                 IsOpen="True" 
                                 IsClosable="False"
                                 Message="Credentials are stored in plain text for ERP integration."/>
                        
                        <StackPanel Spacing="6">
                            <TextBlock Text="Visual/Infor Username" 
                                       Style="{StaticResource BodyStrongTextBlockStyle}"/>
                            <TextBox x:Name="VisualUsernameTextBox"
                                     PlaceholderText="ERP username"
                                     MaxLength="50"
                                     TabIndex="8"/>
                        </StackPanel>

                        <StackPanel Spacing="6">
                            <TextBlock Text="Visual/Infor Password" 
                                       Style="{StaticResource BodyStrongTextBlockStyle}"/>
                            <PasswordBox x:Name="VisualPasswordBox"
                                         PlaceholderText="ERP password"
                                         MaxLength="100"
                                         TabIndex="9"
                                         PasswordRevealMode="Peek"/>
                        </StackPanel>
                    </StackPanel>
                </StackPanel>
            </Expander>
            </Grid>
        </ScrollViewer>

        <!-- Footer: Status Bar & Progress -->
        <StackPanel Grid.Row="2" Spacing="6" Padding="20,8,20,16">
            <ProgressBar x:Name="LoadingProgressBar" 
                         IsIndeterminate="True" 
                         Visibility="Collapsed"
                         Height="4"/>
            
            <InfoBar x:Name="StatusInfoBar"
                     IsOpen="False"
                     IsClosable="True"/>
        </StackPanel>
    </Grid>
</ContentDialog>
```

## File: Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml.cs
```csharp
public sealed partial class View_Shared_NewUserSetupDialog : ContentDialog
⋮----
ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
⋮----
private void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
⋮----
private async void OnDialogLoaded(object sender, RoutedEventArgs e)
⋮----
FullNameTextBox.Focus(FocusState.Programmatic);
⋮----
private async System.Threading.Tasks.Task LoadDepartmentsAsync()
⋮----
await ViewModel.LoadDepartmentsAsync();
DepartmentComboBox.Items.Clear();
⋮----
DepartmentComboBox.Items.Add(dept);
⋮----
DepartmentComboBox.Items.Add("Other");
⋮----
private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
⋮----
string selectedDept = DepartmentComboBox.SelectedItem.ToString() ?? string.Empty;
⋮----
CustomDepartmentTextBox.Focus(FocusState.Programmatic);
⋮----
private void ConfigureErpCheckBox_Checked(object sender, RoutedEventArgs e)
⋮----
_ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
⋮----
VisualUsernameTextBox.Focus(FocusState.Programmatic);
⋮----
private void ConfigureErpCheckBox_Unchecked(object sender, RoutedEventArgs e)
⋮----
private async void OnCreateAccountButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
if (string.IsNullOrWhiteSpace(employeeNumber))
⋮----
EmployeeNumberTextBox.Focus(FocusState.Programmatic);
⋮----
if (!int.TryParse(employeeNumber, out int empNum) || empNum <= 0)
⋮----
if (string.IsNullOrWhiteSpace(fullName))
⋮----
if (string.IsNullOrWhiteSpace(department))
⋮----
DepartmentComboBox.Focus(FocusState.Programmatic);
⋮----
if (string.IsNullOrWhiteSpace(shift))
⋮----
ShiftComboBox.Focus(FocusState.Programmatic);
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
PinPasswordBox.Focus(FocusState.Programmatic);
⋮----
if (pin.Length != 4 || !pin.All(char.IsDigit))
⋮----
ConfirmPinPasswordBox.Focus(FocusState.Programmatic);
⋮----
bool success = await ViewModel.CreateAccountAsync();
⋮----
await System.Threading.Tasks.Task.Delay(2000);
⋮----
private void OnCancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
private void ShowValidationError(string message)
⋮----
private void SetLoadingState(bool isLoading)
```

## File: Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_SharedTerminalLoginDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Shared Terminal Login"
    PrimaryButtonText="Login"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonStyle="{StaticResource AccentButtonStyle}"
    Width="400">

    <StackPanel Spacing="16" Padding="8">
        <!-- Instructions -->
        <TextBlock 
            Text="Please enter your credentials to continue"
            TextWrapping="Wrap"
            Style="{StaticResource BodyTextBlockStyle}"/>

        <!-- Username Field -->
        <StackPanel Spacing="4">
            <TextBlock Text="Username" Style="{StaticResource CaptionTextBlockStyle}"/>
            <TextBox 
                x:Name="UsernameTextBox"
                PlaceholderText="Enter your username"
                MaxLength="50"
                TabIndex="0"/>
        </StackPanel>

        <!-- PIN Field -->
        <StackPanel Spacing="4">
            <TextBlock Text="4-Digit PIN" Style="{StaticResource CaptionTextBlockStyle}"/>
            <PasswordBox 
                x:Name="PinPasswordBox"
                PlaceholderText="Enter 4-digit PIN"
                MaxLength="4"
                TabIndex="1"
                PasswordRevealMode="Peek"/>
        </StackPanel>

        <!-- Attempt Counter (Hidden initially, shown after first failure) -->
        <TextBlock 
            x:Name="AttemptCounterTextBlock"
            Foreground="Gold"
            Visibility="Collapsed"
            TextWrapping="Wrap"
            Style="{StaticResource CaptionTextBlockStyle}"/>

        <!-- Error InfoBar -->
        <InfoBar 
            x:Name="ErrorInfoBar"
            Severity="Error"
            IsOpen="False"
            IsClosable="True"
            Title="Login Failed"
            Message="Invalid username or PIN. Please try again."/>

    </StackPanel>
</ContentDialog>
```

## File: Module_Shared/Views/View_Shared_SharedTerminalLoginDialog.xaml.cs
```csharp
public sealed partial class View_Shared_SharedTerminalLoginDialog : ContentDialog
⋮----
private readonly IService_Focus _focusService;
⋮----
ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
⋮----
_focusService.AttachFocusOnVisibility(this, UsernameTextBox);
⋮----
private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
if (string.IsNullOrWhiteSpace(username))
⋮----
UsernameTextBox.Focus(FocusState.Programmatic);
⋮----
if (string.IsNullOrWhiteSpace(pin))
⋮----
PinPasswordBox.Focus(FocusState.Programmatic);
⋮----
if (pin.Length != 4 || !int.TryParse(pin, out _))
⋮----
bool success = await ViewModel.LoginAsync();
⋮----
await System.Threading.Tasks.Task.Delay(5000);
⋮----
private void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
```

## File: Module_Shared/Views/View_Shared_SplashScreenWindow.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<Window
    x:Class="MTM_Receiving_Application.Module_Shared.Views.View_Shared_SplashScreenWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="MTM Receiving Application"   
    >

    <Grid Background="{ThemeResource LayerFillColorDefaultBrush}" Padding="32">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Logo and Branding -->
        <StackPanel Grid.Row="1" HorizontalAlignment="Center" Spacing="12">
            <FontIcon FontFamily="Segoe Fluent Icons" 
                      Glyph="&#xE8F1;" 
                      FontSize="64"
                      Foreground="{ThemeResource AccentFillColorDefaultBrush}"
                      HorizontalAlignment="Center"/>
            
            <TextBlock Text="MTM Receiving Application" 
                       Style="{StaticResource TitleTextBlockStyle}"
                       HorizontalAlignment="Center"
                       FontWeight="SemiBold"/>
            
            <TextBlock Text="Version 1.0.0"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       HorizontalAlignment="Center"/>
            
            <TextBlock Text="MTM Manufacturing"
                       Style="{StaticResource BodyTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                       HorizontalAlignment="Center"/>
        </StackPanel>

        <!-- Progress Section -->
        <StackPanel Grid.Row="2" Margin="0,40,0,0" Spacing="12">
            <TextBlock x:Name="StatusMessageTextBlock"
                       Text="Initializing..."
                       Style="{StaticResource BodyTextBlockStyle}"
                       HorizontalAlignment="Center"
                       TextAlignment="Center"
                       TextWrapping="Wrap"/>

            <ProgressBar x:Name="MainProgressBar"
                         Height="4"
                         Minimum="0"
                         Maximum="100"
                         Value="0"
                         HorizontalAlignment="Stretch"/>

            <TextBlock x:Name="ProgressPercentageTextBlock"
                       Text="0%"
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                       HorizontalAlignment="Center"/>
        </StackPanel>

        <!-- Copyright -->
        <TextBlock Grid.Row="3"
                   Text="© 2025 MTM Manufacturing. All rights reserved."
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorTertiaryBrush}"
                   HorizontalAlignment="Center"
                   Margin="0,28,0,0"/>
    </Grid>
</Window>
```

## File: Module_Shared/Views/View_Shared_SplashScreenWindow.xaml.cs
```csharp
public sealed partial class View_Shared_SplashScreenWindow : Window
⋮----
private void SplashScreenWindow_Closed(object sender, WindowEventArgs args)
⋮----
Application.Current.Exit();
⋮----
private void ConfigureWindow()
⋮----
this.UseCustomTitleBar();
this.HideTitleBarIcon();
this.MakeTitleBarTransparent();
this.SetWindowSize(WindowWidth, WindowHeight);
this.SetFixedSize(disableMaximize: true, disableMinimize: true);
this.CenterOnScreen();
⋮----
private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
⋮----
DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
```

## File: Module_Shared/ViewModels/ViewModel_Shared_MainWindow.cs
```csharp
public partial class ViewModel_Shared_MainWindow : ViewModel_Shared_Base
⋮----
private readonly IService_UserSessionManager _sessionManager;
⋮----
private void OnNotificationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
⋮----
private void UpdateUserDisplay(Model_User user)
```
