Critical Analysis & Full Application Plan
Current Workflow Analysis
What You're Doing Now:
Crystal Report → Extract system data from Infor Visual
Manual Physical Count → Walk warehouse with clipboard/device, record actual inventory
Google Sheets → Enter data, run comparison scripts
Results → Identify discrepancies, manually correct in Infor Visual
Repeat → Periodic cycle counts
Pain Points Identified:
❌ Double data entry (physical count → spreadsheet)
❌ No real-time visibility during counting
❌ Manual report generation
❌ No integration back to Infor Visual
❌ Limited audit trail
❌ No mobile optimization
❌ Difficult to track corrections over time
❌ No barcode/scanning capability
Full Application Vision
Core Objectives:
Eliminate double entry - Capture once at source
Real-time reconciliation - See discrepancies as you count
Mobile-first - Count on phone/tablet with barcode scanning
Bi-directional ERP integration - Pull from & push to Infor Visual
Audit trail - Track who counted what, when, and all changes
Analytics - Trending, accuracy metrics, problem areas
Application Architecture
Module_InventoryCounting/
├── ViewModels/
│   ├── ViewModel_Counting_Session.cs         // Session management
│   ├── ViewModel_Counting_PhysicalEntry.cs   // Mobile-style data entry
│   └── ViewModel_Counting_Reconciliation.cs  // Comparison logic
├── Views/
│   ├── View_Counting_Session.xaml            // Desktop interface
│   ├── View_Counting_Entry.xaml              // Optimized for tablets
│   └── View_Counting_Dashboard.xaml          // Analytics
├── Services/
│   ├── Service_Counting_Session.cs
│   ├── Service_Counting_Reconciliation.cs
│   └── Service_InforVisual_Sync.cs           // Reuse existing integration
└── Data/
    ├── Dao_CountSession.cs
    ├── Dao_PhysicalCount.cs
    └── Dao_Discrepancy.cs

// 2. Database: MySQL (not PostgreSQL)
// Already have connection infrastructure

// 3. Mobile Strategy: Windows Tablet App (not PWA)
// - WinUI 3 runs on Windows tablets
// - Use Windows Camera APIs for barcode scanning
// - Offline-capable with local SQLite sync
-- Integrate with existing MTM database
USE mtm_receiving;

-- Add to existing schema
CREATE TABLE tbl_count_sessions (
    SessionID INT PRIMARY KEY AUTO_INCREMENT,
    SessionName VARCHAR(100) NOT NULL,
    CreatedBy VARCHAR(50) NOT NULL,
    StartedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CompletedAt DATETIME NULL,
    Status ENUM('in_progress', 'completed', 'approved') DEFAULT 'in_progress',
    Notes TEXT,
    INDEX idx_status (Status),
    INDEX idx_created_by (CreatedBy)
) ENGINE=InnoDB;

CREATE TABLE tbl_physical_counts (
    CountID INT PRIMARY KEY AUTO_INCREMENT,
    SessionID INT NOT NULL,
    PartID VARCHAR(20) NOT NULL,
    Location VARCHAR(20) NOT NULL,
    CoilWeight DECIMAL(10,2) NOT NULL,
    CountedBy VARCHAR(50) NOT NULL,
    CountedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PhotoPath VARCHAR(500) NULL,
    Notes TEXT,
    FOREIGN KEY (SessionID) REFERENCES tbl_count_sessions(SessionID),
    INDEX idx_session (SessionID),
    INDEX idx_part_location (PartID, Location)
) ENGINE=InnoDB;

-- Link to existing Infor Visual integration
CREATE TABLE tbl_count_discrepancies (
    DiscrepancyID INT PRIMARY KEY AUTO_INCREMENT,
    SessionID INT NOT NULL,
    PartID VARCHAR(20) NOT NULL,
    IssueType VARCHAR(50) NOT NULL,
    CurrentLocation VARCHAR(20),
    ExpectedLocation VARCHAR(20),
    ExpectedQty DECIMAL(10,2),
    ActualQty DECIMAL(10,2),
    Variance DECIMAL(10,2),
    Status ENUM('open', 'reviewed', 'resolved') DEFAULT 'open',
    ResolvedBy VARCHAR(50) NULL,
    ResolvedAt DATETIME NULL,
    FOREIGN KEY (SessionID) REFERENCES tbl_count_sessions(SessionID)
) ENGINE=InnoDB;
// ✅ CORRECT - Follows your constitutional architecture
public partial class ViewModel_Counting_Session : ViewModel_Shared_Base
{
    private readonly IService_Counting_Session _sessionService;
    private readonly IService_InforVisual_Integration _inforService;

    [ObservableProperty]
    private ObservableCollection<Model_CountSession> _sessions = new();

    [ObservableProperty]
    private Model_CountSession? _currentSession;

    [ObservableProperty]
    private string _sessionName = string.Empty;

    public ViewModel_Counting_Session(
        IService_Counting_Session sessionService,
        IService_InforVisual_Integration inforService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _sessionService = sessionService;
        _inforService = inforService;
    }

    [RelayCommand]
    private async Task CreateSessionAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(SessionName)) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Creating count session...";

            // Pull expected inventory from Infor Visual (READ ONLY)
            var expectedInventory = await _inforService.GetInventoryByLocationAsync();

            var session = new Model_CountSession
            {
                SessionName = SessionName,
                CreatedBy = _userService.CurrentUser.Username,
                StartedAt = DateTime.Now,
                Status = Enum_CountStatus.InProgress
            };

            var result = await _sessionService.CreateSessionAsync(session, expectedInventory);

            if (result.IsSuccess)
            {
                CurrentSession = result.Data;
                StatusMessage = $"Session '{SessionName}' created with {expectedInventory.Count} expected items";
                SessionName = string.Empty;
            }
            else
            {
                _errorHandler.ShowUserError(
                    result.ErrorMessage,
                    "Session Creation Failed",
                    nameof(CreateSessionAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.High,
                nameof(CreateSessionAsync),
                nameof(ViewModel_Counting_Session));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReconcileSessionAsync()
    {
        if (CurrentSession == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Running reconciliation...";

            var discrepancies = await _sessionService.ReconcileAsync(CurrentSession.SessionID);

            StatusMessage = $"Found {discrepancies.Count} discrepancies";
            
            // Navigate to discrepancy review
            _navigationService.NavigateTo(
                nameof(View_Counting_Discrepancies),
                CurrentSession.SessionID);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
public interface IService_Counting_Session
{
    Task<Model_Dao_Result<Model_CountSession>> CreateSessionAsync(
        Model_CountSession session,
        List<Model_ExpectedInventory> expectedItems);

    Task<List<Model_Discrepancy>> ReconcileAsync(int sessionId);

    Task<Model_Dao_Result> ExportAdjustmentsAsync(int sessionId);
}

public class Service_Counting_Session : IService_Counting_Session
{
    private readonly Dao_CountSession _sessionDao;
    private readonly Dao_PhysicalCount _countDao;
    private readonly Dao_Discrepancy _discrepancyDao;
    private readonly IService_LoggingUtility _logger;

    public Service_Counting_Session(
        Dao_CountSession sessionDao,
        Dao_PhysicalCount countDao,
        Dao_Discrepancy discrepancyDao,
        IService_LoggingUtility logger)
    {
        _sessionDao = sessionDao;
        _countDao = countDao;
        _discrepancyDao = discrepancyDao;
        _logger = logger;
    }

    public async Task<List<Model_Discrepancy>> ReconcileAsync(int sessionId)
    {
        _logger.LogInfo($"Starting reconciliation for session {sessionId}");

        // Get expected vs actual
        var expected = await _sessionDao.GetExpectedInventoryAsync(sessionId);
        var actual = await _countDao.GetPhysicalCountsAsync(sessionId);

        // Run your Google Sheets logic here
        var discrepancies = CompareInventory(expected, actual);

        // Save to database
        foreach (var discrepancy in discrepancies)
        {
            await _discrepancyDao.InsertDiscrepancyAsync(discrepancy);
        }

        return discrepancies;
    }

    private List<Model_Discrepancy> CompareInventory(
        List<Model_ExpectedInventory> expected,
        List<Model_PhysicalCount> actual)
    {
        // PORT YOUR GOOGLE SHEETS COMPARISON LOGIC HERE
        var discrepancies = new List<Model_Discrepancy>();

        // Example logic
        foreach (var exp in expected)
        {
            var act = actual.FirstOrDefault(a => 
                a.PartID == exp.PartID && a.Location == exp.Location);

            if (act == null)
            {
                // Missing physical count
                discrepancies.Add(new Model_Discrepancy
                {
                    IssueType = "missing_count",
                    PartID = exp.PartID,
                    ExpectedLocation = exp.Location,
                    ExpectedQty = exp.Quantity,
                    ActualQty = 0,
                    Variance = -exp.Quantity
                });
            }
            else if (Math.Abs(act.TotalWeight - exp.Quantity) > 100) // Tolerance
            {
                discrepancies.Add(new Model_Discrepancy
                {
                    IssueType = "quantity_variance",
                    PartID = exp.PartID,
                    CurrentLocation = act.Location,
                    ExpectedQty = exp.Quantity,
                    ActualQty = act.TotalWeight,
                    Variance = act.TotalWeight - exp.Quantity
                });
            }
        }

        return discrepancies;
    }
}
<!-- View_Counting_Entry.xaml -->
<!-- Optimized for tablets/touch -->
<Page
    x:Class="MTM_Receiving_Application.Module_InventoryCounting.Views.View_Counting_Entry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Session Header -->
        <StackPanel Grid.Row="0" Spacing="10">
            <TextBlock 
                Text="{x:Bind ViewModel.CurrentSession.SessionName, Mode=OneWay}"
                Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock 
                Text="{x:Bind ViewModel.ProgressText, Mode=OneWay}"
                Style="{StaticResource SubtitleTextBlockStyle}"/>
        </StackPanel>

        <!-- Barcode Scanner -->
        <StackPanel Grid.Row="1" Spacing="15" Margin="0,20,0,0">
            <Button 
                Content="📷 Scan Location Barcode"
                Command="{x:Bind ViewModel.ScanLocationCommand}"
                HorizontalAlignment="Stretch"
                Height="60"
                FontSize="18"/>

            <TextBox
                Text="{x:Bind ViewModel.CurrentLocation, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="Or enter location manually"
                FontSize="18"
                Height="50"/>
        </StackPanel>

        <!-- Physical Count Entry -->
        <ScrollViewer Grid.Row="2" Margin="0,20,0,0">
            <StackPanel Spacing="15">
                <!-- Expected Items at Location -->
                <TextBlock Text="Expected at this location:" FontWeight="SemiBold"/>
                <ItemsRepeater ItemsSource="{x:Bind ViewModel.ExpectedItems, Mode=OneWay}">
                    <ItemsRepeater.ItemTemplate>
                        <DataTemplate x:DataType="models:Model_ExpectedInventory">
                            <Border 
                                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                                CornerRadius="8"
                                Padding="15"
                                Margin="0,0,0,10">
                                <StackPanel>
                                    <TextBlock Text="{x:Bind PartID}" FontWeight="Bold"/>
                                    <TextBlock Text="{x:Bind QuantityDisplay}" Opacity="0.8"/>
                                </StackPanel>
                            </Border>
                        </DataTemplate>
                    </ItemsRepeater.ItemTemplate>
                </ItemsRepeater>

                <!-- Scan Part -->
                <Button 
                    Content="📷 Scan Part Number"
                    Command="{x:Bind ViewModel.ScanPartCommand}"
                    HorizontalAlignment="Stretch"
                    Height="60"
                    FontSize="18"/>

                <!-- Coil Entry -->
                <TextBlock Text="Coil Weights:" FontWeight="SemiBold" Margin="0,20,0,0"/>
                <ItemsRepeater ItemsSource="{x:Bind ViewModel.CoilWeights, Mode=OneWay}">
                    <ItemsRepeater.ItemTemplate>
                        <DataTemplate x:DataType="x:Double">
                            <TextBlock Text="{x:Bind}" FontSize="16" Margin="0,5"/>
                        </DataTemplate>
                    </ItemsRepeater.ItemTemplate>
                </ItemsRepeater>

                <NumberBox
                    Header="Add Coil Weight"
                    Value="{x:Bind ViewModel.NewCoilWeight, Mode=TwoWay}"
                    SpinButtonPlacementMode="Compact"
                    Height="60"
                    FontSize="18"/>

                <Button 
                    Content="➕ Add Coil"
                    Command="{x:Bind ViewModel.AddCoilCommand}"
                    HorizontalAlignment="Stretch"/>

                <!-- Total Summary -->
                <Border 
                    Background="{ThemeResource AccentFillColorDefaultBrush}"
                    CornerRadius="8"
                    Padding="15"
                    Margin="0,20,0,0">
                    <StackPanel>
                        <TextBlock Text="Total Weight:" FontWeight="SemiBold" Foreground="White"/>
                        <TextBlock 
                            Text="{x:Bind ViewModel.TotalWeight, Mode=OneWay}"
                            FontSize="24"
                            FontWeight="Bold"
                            Foreground="White"/>
                        <TextBlock 
                            Text="{x:Bind ViewModel.VarianceText, Mode=OneWay}"
                            Foreground="White"
                            Margin="0,5,0,0"/>
                    </StackPanel>
                </Border>
            </StackPanel>
        </ScrollViewer>

        <!-- Actions -->
        <StackPanel Grid.Row="3" Spacing="10" Margin="0,20,0,0">
            <Button 
                Content="💾 Save & Next Location"
                Command="{x:Bind ViewModel.SaveAndNextCommand}"
                HorizontalAlignment="Stretch"
                Height="60"
                FontSize="18"
                Style="{StaticResource AccentButtonStyle}"/>

            <Button 
                Content="📸 Add Photo"
                Command="{x:Bind ViewModel.AddPhotoCommand}"
                HorizontalAlignment="Stretch"/>
        </StackPanel>
    </Grid>
</Page>
// Add to ConfigureServices method

// DAOs
var connectionString = Helper_Database_Variables.GetConnectionString();
services.AddSingleton(sp => new Dao_CountSession(connectionString));
services.AddSingleton(sp => new Dao_PhysicalCount(connectionString));
services.AddSingleton(sp => new Dao_Discrepancy(connectionString));

// Services
services.AddSingleton<IService_Counting_Session, Service_Counting_Session>();

// ViewModels
services.AddTransient<ViewModel_Counting_Session>();
services.AddTransient<ViewModel_Counting_Entry>();
services.AddTransient<ViewModel_Counting_Dashboard>();
// In MTM Receiving App - Add API client service
public class Service_InventoryCountingAPI : IService_External_API
{
    private readonly HttpClient _httpClient;

    public async Task<List<Model_CountSession>> GetSessionsAsync()
    {
        var response = await _httpClient.GetAsync("https://your-api.com/api/sessions");
        return await response.Content.ReadFromJsonAsync<List<Model_CountSession>>();
    }

    public async Task ExportToInforVisualAsync(int sessionId)
    {
        // MTM app handles the Infor Visual write
        var adjustments = await _httpClient.GetAsync($"/api/adjustments/session/{sessionId}");
        // Process and write to Infor Visual using existing integration
    }
}
