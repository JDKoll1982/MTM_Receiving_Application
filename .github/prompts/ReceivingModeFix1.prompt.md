# MTM Receiving Application - Receiving Wizard Enhancement

## 🎭 Persona: Expert WinUI 3 Manufacturing Application Developer

You are a senior WinUI 3 developer specializing in **MVVM architecture** for manufacturing receiving workflows. You have deep expertise in: 
- C# . NET 8 and WinUI 3 desktop applications
- CommunityToolkit.Mvvm (ObservableProperty, RelayCommand)
- Manufacturing ERP integrations (Infor Visual SQL Server - READ ONLY)
- MySQL database design and stored procedures
- Fluent Design System and DPI-aware XAML
- Clean architecture and separation of concerns

---

## 📋 Context & Problem Statement

The **MTM Receiving Application** is a WinUI 3 desktop app for receiving parts from vendors. The receiving workflow uses a **wizard pattern** with multiple steps.  Several UX issues and missing features need to be addressed. 

### Critical Architecture Rules (from constitution.md & AGENTS.md):
⚠️ **NEVER write to Infor Visual (SQL Server)** - ApplicationIntent=ReadOnly ALWAYS  
✅ **ALWAYS use stored procedures** for MySQL operations  
✅ **ALWAYS use x:Bind** (compile-time binding), NEVER use Binding  
✅ **ALWAYS make ViewModels partial classes** for CommunityToolkit.Mvvm source generators  
✅ **ALWAYS inherit from BaseViewModel** or **BaseStepViewModel<T>** for wizard steps  
✅ **ALWAYS return Model_Dao_Result<T>** from DAO methods (never throw exceptions)  
✅ **ALWAYS register services in App.xaml.cs** DI container  

---

## 🎯 Phase 1: Requirements Analysis

### **R1: PO TextBox Alignment & Auto-Correction**
**Current Problem**: PO textbox alignment is incorrect until valid PO is entered  
**Required Behavior**:
- Alignment should be consistent regardless of validation state
- Auto-correction (trim, uppercase, format) should **ONLY** occur on **LostFocus event**
- **NOT** on TextChanged event (removes performance issues and prevents cursor jumping)

**Implementation**:
```csharp
// In ViewModel
[RelayCommand]
private void PoTextBoxLostFocus()
{
    PONumber = PONumber?. Trim().ToUpper() ?? string.Empty;
    // Trigger validation after correction
    ValidatePONumber();
}
```

```xml
<!-- In XAML -->
<TextBox 
    Header="Purchase Order Number"
    Text="{x:Bind ViewModel.StepData.PONumber, Mode=TwoWay}"
    LostFocus="{x:Bind ViewModel. PoTextBoxLostFocusCommand}"
    HorizontalAlignment="Stretch"/>
```

---

### **R2: Replace "Quantity Shown" with "Remaining Quantity"**
**Current**:  Displays "Quantity Shown" (unclear field)  
**Required**:  Calculate and display "Remaining Quantity" using Infor Visual database

**Calculation Logic**:
```
Remaining Quantity = Quantity Ordered - Quantity Received
```

**Database Schema References** (READ-ONLY Infor Visual):
- **PURCHASE_ORDER** table (PK: ID)
- **PURC_ORDER_LINE** table (FK:  PURC_ORDER_ID → PURCHASE_ORDER.ID)
- **RECEIVER_LINE** table (FK: PURC_ORDER_ID, PURC_ORDER_LINE_NO)

**SQL Query** (from MTMFG_Schema_*. csv):
```sql
SELECT 
    pol.QUANTITY_ORDERED - ISNULL(SUM(rl.QTY_RECEIVED), 0) AS RemainingQuantity
FROM PURC_ORDER_LINE pol
LEFT JOIN RECEIVER_LINE rl 
    ON pol.PURC_ORDER_ID = rl. PURC_ORDER_ID 
    AND pol.LINE_NO = rl.PURC_ORDER_LINE_NO
WHERE pol.PURC_ORDER_ID = @PONumber 
    AND pol. PART_ID = @PartID
GROUP BY pol.QUANTITY_ORDERED
```

**Display Format**:  
- **Whole numbers only** (no decimal places)
- Use `Math.Floor()` or `(int)` cast
- Example: 150. 75 → displays as **150**

**Service Implementation**:
```csharp
public class Service_InforVisual : IService_InforVisual
{
    private readonly string _readOnlyConnectionString = 
        "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;... ";
    
    public async Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(
        string poNumber, string partId)
    {
        try
        {
            // ⚠️ READ-ONLY query
            var query = @"
                SELECT pol.QUANTITY_ORDERED - ISNULL(SUM(rl.QTY_RECEIVED), 0)
                FROM PURC_ORDER_LINE pol
                LEFT JOIN RECEIVER_LINE rl ... ";
            
            var result = await ExecuteScalarAsync<decimal>(query, 
                new { PONumber = poNumber, PartID = partId });
            
            return Model_Dao_Result<int>. Success((int)Math.Floor(result));
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<int>. Failure(
                $"Error retrieving remaining quantity: {ex.Message}", ex);
        }
    }
}
```

---

### **R3: Auto-Focus on First TextBox**
**Required**: When entering any new step/view in the wizard, automatically set focus to the first input field

**Implementation**:
```csharp
// In View code-behind
private void Page_Loaded(object sender, RoutedEventArgs e)
{
    // Find first TextBox and set focus
    FirstTextBox?.Focus(FocusState.Programmatic);
}
```

```xml
<Page Loaded="Page_Loaded">
    <TextBox x:Name="FirstTextBox" Header="PO Number" TabIndex="0"/>
</Page>
```

---

### **R4: Heat/Lot Number - Optional Field**
**Current**: May be treated as required  
**Required**:
- Field is **optional** (not all packlists include heat/lot numbers)
- If user leaves blank, set value to **"Not Entered"**
- Display "Not Entered" in data review

**ViewModel Logic**:
```csharp
[ObservableProperty]
private string _heatLotNumber = string.Empty;

private void PrepareForSave()
{
    if (string.IsNullOrWhiteSpace(HeatLotNumber))
    {
        HeatLotNumber = "Not Entered";
    }
}
```

---

### **R5: Auto-Detect Package Type from Part Number**
**Required**:  Automatically set Package Type based on Part Number prefix

**Business Rules**:
| Part Number Prefix | Package Type |
|--------------------|--------------|
| **MMC***           | Coils        |
| **MMF***           | Sheets       |
| All others         | Skids        |

**Implementation**:
```csharp
[ObservableProperty]
private string _partID = string.Empty;

[ObservableProperty]
private string _packageType = "Skids"; // Default

partial void OnPartIDChanged(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return;
    
    var upperPart = value. Trim().ToUpper();
    
    if (upperPart. StartsWith("MMC"))
        PackageType = "Coils";
    else if (upperPart.StartsWith("MMF"))
        PackageType = "Sheets";
    else
        PackageType = "Skids";
}
```

```xml
<ComboBox 
    Header="Package Type"
    SelectedItem="{x:Bind ViewModel.StepData.PackageType, Mode=TwoWay}"
    IsEnabled="False"> <!-- Read-only, auto-detected -->
    <ComboBoxItem Content="Coils"/>
    <ComboBoxItem Content="Sheets"/>
    <ComboBoxItem Content="Skids"/>
</ComboBox>
```

---

### **R6: Review Step - Single Entry View with Navigation**
**Current**: Shows DataGrid with all entries  
**Required**: 
- **Default view**:  Single-entry form with labeled textboxes (easier to read)
- **Navigation buttons**:
  - **⬅ Back** - Navigate to previous entry
  - **Next ➡** - Navigate to next entry
  - **📊 Table View** - Switch to DataGrid view
- **Table view** includes **📝 Single View** button to switch back

**ViewModel Implementation**:
```csharp
public partial class ReviewStepViewModel : BaseStepViewModel<ReviewStepData>
{
    [ObservableProperty]
    private bool _isSingleView = true;
    
    [ObservableProperty]
    private int _currentEntryIndex = 0;
    
    [ObservableProperty]
    private ReceivingEntry?  _currentEntry;
    
    public ObservableCollection<ReceivingEntry> Entries { get; }
    
    [RelayCommand]
    private void PreviousEntry()
    {
        if (CurrentEntryIndex > 0)
        {
            CurrentEntryIndex--;
            CurrentEntry = Entries[CurrentEntryIndex];
        }
    }
    
    [RelayCommand]
    private void NextEntry()
    {
        if (CurrentEntryIndex < Entries.Count - 1)
        {
            CurrentEntryIndex++;
            CurrentEntry = Entries[CurrentEntryIndex];
        }
    }
    
    [RelayCommand]
    private void SwitchToTableView()
    {
        IsSingleView = false;
    }
    
    [RelayCommand]
    private void SwitchToSingleView()
    {
        IsSingleView = true;
    }
}
```

**XAML Layout**:
```xml
<Grid>
    <!-- Single Entry View -->
    <StackPanel Visibility="{x:Bind ViewModel.IsSingleView, Mode=OneWay}">
        <TextBlock Text="Review Entry" Style="{ThemeResource TitleTextBlockStyle}"/>
        <TextBlock Text="{x:Bind ViewModel.CurrentEntryIndex, Mode=OneWay}" 
                   Foreground="{ThemeResource SystemAccentColor}"/>
        
        <!-- Read-only form fields -->
        <TextBox Header="Part Number" 
                 Text="{x: Bind ViewModel.CurrentEntry.PartID, Mode=OneWay}" 
                 IsReadOnly="True"/>
        <TextBox Header="Remaining Quantity" 
                 Text="{x:Bind ViewModel.CurrentEntry.RemainingQuantity, Mode=OneWay}" 
                 IsReadOnly="True"/>
        <TextBox Header="Heat/Lot Number" 
                 Text="{x: Bind ViewModel.CurrentEntry. HeatLotNumber, Mode=OneWay}" 
                 IsReadOnly="True"/>
        
        <!-- Navigation Buttons -->
        <CommandBar DefaultLabelPosition="Right">
            <AppBarButton Icon="Back" Label="Back" 
                          Command="{x: Bind ViewModel.PreviousEntryCommand}"
                          IsEnabled="{x:Bind ViewModel. CanGoBack, Mode=OneWay}"/>
            <AppBarButton Icon="Forward" Label="Next" 
                          Command="{x:Bind ViewModel.NextEntryCommand}"
                          IsEnabled="{x:Bind ViewModel.CanGoNext, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton Icon="ViewAll" Label="Table View" 
                          Command="{x: Bind ViewModel.SwitchToTableViewCommand}"/>
        </CommandBar>
    </StackPanel>
    
    <!-- Table View (DataGrid) -->
    <Grid Visibility="{x:Bind ViewModel.IsTableView, Mode=OneWay}">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <DataGrid ItemsSource="{x:Bind ViewModel.Entries, Mode=OneWay}" 
                  AutoGenerateColumns="False"/>
        
        <CommandBar Grid.Row="1">
            <AppBarButton Icon="Edit" Label="Single View" 
                          Command="{x:Bind ViewModel. SwitchToSingleViewCommand}"/>
        </CommandBar>
    </Grid>
</Grid>
```

---

### **R7: Complete XAML Redesign with Modern Standards**
For **EVERY** XAML file modified during this process:

#### **Design Requirements**: 
✅ **Fluent Design Icons**:  Use Segoe Fluent Icons (FontIcon with Glyph or SymbolIcon)  
✅ **Consistent Styling**: Apply ThemeResource styles (TitleTextBlockStyle, SubtitleTextBlockStyle, etc.)  
✅ **Proper Spacing**: Use Grid. RowSpacing, StackPanel. Spacing, Margin/Padding with 4px increments  
✅ **DPI Scaling**: All sizes in effective pixels (epx), test at 100%, 150%, 200% scaling  
✅ **Responsive Layout**: Use Grid with star sizing, avoid fixed widths  
✅ **Accessibility**: Set AutomationProperties.Name on all interactive controls  

#### **Window Sizing Standards** (from window-sizing.instructions.md):
- **Main Window**: 1400×900px (resizable)
- **Dialogs**: 600px width max (non-resizable)
- **Splash Screen**: 500×450px (non-resizable)

#### **Example Modern XAML**:
```xml
<Page
    x:Class="MTM_Receiving_Application.Views.Receiving. PoEntryView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    
    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <StackPanel Spacing="8">
            <TextBlock Text="Purchase Order Entry" 
                       Style="{ThemeResource TitleTextBlockStyle}"/>
            <TextBlock Text="Enter receiving information for vendor shipment" 
                       Style="{ThemeResource BodyTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        </StackPanel>
        
        <!-- Form Fields -->
        <ScrollViewer Grid.Row="1">
            <StackPanel Spacing="12" MaxWidth="600">
                <!-- PO Number with Icon -->
                <TextBox x:Name="FirstTextBox"
                         Header="Purchase Order Number"
                         PlaceholderText="Enter PO Number"
                         Text="{x:Bind ViewModel.StepData.PONumber, Mode=TwoWay}"
                         LostFocus="{x: Bind ViewModel.PoTextBoxLostFocusCommand}"
                         TabIndex="0"
                         AutomationProperties.Name="Purchase Order Number">
                    <TextBox.HeaderTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal" Spacing="8">
                                <FontIcon Glyph="&#xE8A5;" FontSize="16"/>
                                <TextBlock Text="Purchase Order Number"/>
                            </StackPanel>
                        </DataTemplate>
                    </TextBox.HeaderTemplate>
                </TextBox>
                
                <!-- Remaining Quantity (Read-only with accent) -->
                <TextBox Header="Remaining Quantity"
                         Text="{x:Bind ViewModel.StepData.RemainingQuantity, Mode=OneWay}"
                         IsReadOnly="True"
                         Foreground="{ThemeResource SystemAccentColor}"
                         FontWeight="SemiBold"/>
            </StackPanel>
        </ScrollViewer>
        
        <!-- Navigation Buttons -->
        <Grid Grid.Row="2" ColumnSpacing="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <Button Grid.Column="1" 
                    Content="Cancel" 
                    Command="{x:Bind ViewModel.CancelCommand}"/>
            <Button Grid.Column="2" 
                    Content="Next" 
                    Style="{StaticResource AccentButtonStyle}"
                    Command="{x: Bind ViewModel.NextStepCommand}">
                <Button.Content>
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <TextBlock Text="Next"/>
                        <FontIcon Glyph="&#xE76C;" FontSize="12"/>
                    </StackPanel>
                </Button.Content>
            </Button>
        </Grid>
    </Grid>
</Page>
```

---

## 🎯 Phase 2: Implementation Plan

### **Step 1: Create/Update Service Layer**
**Files to Create/Modify**:
- `Contracts/Services/IService_InforVisual.cs` (interface)
- `Services/Receiving/Service_InforVisual. cs` (implementation)
- Register in `App.xaml.cs` ConfigureServices

**Key Methods**:
```csharp
Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partId);
Task<Model_Dao_Result<PoDetails>> GetPoDetailsAsync(string poNumber);
```

### **Step 2: Update ViewModels**
**Files to Modify**:
- `ViewModels/Receiving/PoEntryStepViewModel.cs`
- `ViewModels/Receiving/ReviewStepViewModel.cs`
- `ViewModels/Receiving/ReceivingWizardViewModel.cs` (if exists)

**Patterns to Apply**:
- Inherit from `BaseStepViewModel<TStepData>`
- Use `[ObservableProperty]` for all bindable properties
- Use `[RelayCommand]` for all commands
- Implement auto-detection logic (PartID → PackageType)
- Implement navigation logic (Previous/Next/TableView toggle)

### **Step 3: Redesign XAML Views**
**Files to Modify**:
- `Views/Receiving/PoEntryView.xaml`
- `Views/Receiving/ReviewStepView.xaml`
- Any other receiving wizard step views

**For Each XAML File**:
1. ✅ Remove all `Binding` → replace with `x:Bind`
2. ✅ Apply Fluent Design icons (FontIcon/SymbolIcon)
3. ✅ Use ThemeResource styles consistently
4. ✅ Add proper spacing (4px increments)
5. ✅ Set AutomationProperties for accessibility
6. ✅ Test at 100%, 150%, 200% DPI scaling
7. ✅ Ensure alignment issues are resolved (Grid/StackPanel layout)

### **Step 4: Update Models (if needed)**
**Files to Modify**:
- `Models/Receiving/ReceivingEntry.cs` (add RemainingQuantity property)
- `Models/Receiving/PoEntryStepData.cs`
- `Models/Receiving/ReviewStepData.cs`

```csharp
public class ReceivingEntry
{
    public string PartID { get; set; } = string.Empty;
    public int RemainingQuantity { get; set; } // NEW
    public string HeatLotNumber { get; set; } = "Not Entered"; // Default
    public string PackageType { get; set; } = "Skids"; // Default
    // ... other properties
}
```

---

## 🎯 Phase 3: Success Criteria

### **Functional Requirements**
✅ **R1 - PO Alignment**: PO textbox alignment is consistent regardless of validation state  
✅ **R1 - Auto-Correction**: PO auto-corrects ONLY on LostFocus (not TextChanged)  
✅ **R2 - Remaining Quantity**:  Displays calculated value (Ordered - Received) from Infor Visual  
✅ **R2 - Whole Numbers**: Remaining Quantity shows no decimal places (150. 75 → 150)  
✅ **R3 - Auto-Focus**: First textbox receives focus when entering any step  
✅ **R4 - Heat/Lot Optional**: If blank, displays "Not Entered"  
✅ **R5 - Auto-Detect Package**: MMC→Coils, MMF→Sheets, else Skids  
✅ **R6 - Single View**: Review step shows labeled textboxes by default  
✅ **R6 - Navigation**: Back/Next/Table View buttons work correctly  
✅ **R6 - Table Toggle**: Table view has Single View button to switch back  

### **Code Quality Requirements**
✅ **MVVM Strict**:  All ViewModels are `partial`, inherit from BaseViewModel  
✅ **x:Bind Only**: No `{Binding}` in XAML (only `{x:Bind}`)  
✅ **Read-Only Infor**: All Infor Visual queries use ApplicationIntent=ReadOnly  
✅ **Error Handling**: All async methods wrapped in try-catch with IService_ErrorHandler  
✅ **DAO Pattern**: All service methods return Model_Dao_Result<T>  
✅ **DI Registered**: New services registered in App.xaml. cs  

### **UI/UX Requirements**
✅ **Fluent Icons**: All buttons/headers use Segoe Fluent Icons  
✅ **Consistent Styling**: ThemeResource styles applied throughout  
✅ **Proper Spacing**:  4px increment spacing (8, 12, 16, 24px)  
✅ **DPI Scaling**:  Tested at 100%, 150%, 200% scaling  
✅ **Accessibility**: AutomationProperties.Name set on interactive controls  
✅ **Window Sizing**:  Follows window-sizing.instructions.md standards  

### **Compliance Requirements**
✅ **constitution.md**: No violations of core architectural principles  
✅ **copilot-instructions.md**: Follows all coding standards  
✅ **AGENTS.md**:  Adheres to boundaries and constraints  
✅ **mvvm-pattern.instructions.md**: Follows ViewModel/View patterns  
✅ **dao-pattern.instructions.md**: DAO methods return Model_Dao_Result  
✅ **error-handling.instructions.md**:  Proper error severity classification  
✅ **window-sizing.instructions.md**: Window dimensions per standards  

---

## 🎯 Phase 4: Testing & Validation

### **Unit Tests** (if applicable)
```csharp
[Fact]
public async Task GetRemainingQuantity_ValidPO_ReturnsWholeNumber()
{
    // Arrange
    var service = new Service_InforVisual(connectionString);
    
    // Act
    var result = await service.GetRemainingQuantityAsync("PO12345", "MMC-001");
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.IsType<int>(result.Data);
    Assert.Equal(150, result.Data); // No decimals
}

[Theory]
[InlineData("MMC-123", "Coils")]
[InlineData("MMF-456", "Sheets")]
[InlineData("ABC-789", "Skids")]
public void PartID_AutoDetectsPackageType(string partId, string expectedPackage)
{
    // Arrange
    var vm = new PoEntryStepViewModel(errorHandler, logger);
    
    // Act
    vm.PartID = partId;
    
    // Assert
    Assert.Equal(expectedPackage, vm.PackageType);
}
```

### **Manual Testing Checklist**
- [ ] PO textbox alignment stays consistent (empty and filled states)
- [ ] PO auto-correction only triggers on LostFocus
- [ ] Remaining Quantity displays correct calculation
- [ ] Remaining Quantity shows whole numbers (no decimals)
- [ ] Focus moves to first textbox on step entry
- [ ] Heat/Lot blank → "Not Entered" on save
- [ ] Part MMC* → Package "Coils"
- [ ] Part MMF* → Package "Sheets"
- [ ] Part other → Package "Skids"
- [ ] Review step defaults to Single View
- [ ] Back button navigates to previous entry
- [ ] Next button navigates to next entry
- [ ] Table View button switches to DataGrid
- [ ] Single View button (in table) switches back
- [ ] UI looks correct at 100% DPI
- [ ] UI looks correct at 150% DPI
- [ ] UI looks correct at 200% DPI
- [ ] All icons display correctly (Fluent Design)
- [ ] All spacing is consistent (4px increments)

### **Build & Integration Tests**
```powershell
# Build solution
dotnet build -c Release /p:Platform=x64

# Run unit tests
dotnet test

# Check for XAML compilation errors
dotnet build MTM_Receiving_Application.csproj /p:GenerateFullPaths=true
```

---

## 📚 Reference Documentation

### **Must Read Before Implementation**: 
1. **constitution.md** - Core architectural principles (READ-ONLY Infor Visual)
2. **AGENTS.md** - Boundaries, constraints, patterns
3. **copilot-instructions.md** - Coding standards

### **Architecture Guidelines**:
- `.github/instructions/mvvm-pattern.instructions.md` - ViewModel/View patterns
- `.github/instructions/mvvm-viewmodels.instructions.md` - ObservableProperty, RelayCommand
- `.github/instructions/mvvm-views.instructions.md` - x:Bind, data binding
- `.github/instructions/dao-pattern.instructions.md` - DAO patterns, Model_Dao_Result
- `.github/instructions/database-layer.instructions.md` - Database access patterns
- `.github/instructions/error-handling.instructions.md` - Error severity, logging
- `.github/instructions/window-sizing.instructions.md` - Window dimensions

### **Database Schema References**:
- `Documentation/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_FKs.csv` - Foreign keys
- `Documentation/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_PKs.csv` - Primary keys
- `Documentation/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_Tables.csv` - Table structure

### **Key Tables**:
- **PURCHASE_ORDER** (PK: ID)
- **PURC_ORDER_LINE** (PK:  PURC_ORDER_ID, LINE_NO) (FK: PURC_ORDER_ID → PURCHASE_ORDER. ID)
- **RECEIVER_LINE** (PK: RECEIVER_ID, LINE_NO) (FK: PURC_ORDER_ID, PURC_ORDER_LINE_NO)

---

## 🚀 Implementation Workflow

1. **Review all documentation files** (constitution.md, AGENTS.md, instruction files)
2. **Create IService_InforVisual interface** with GetRemainingQuantityAsync method
3. **Implement Service_InforVisual** with READ-ONLY SQL queries
4. **Register service in App.xaml.cs** DI container
5. **Update PoEntryStepViewModel** with auto-detection logic and LostFocus command
6. **Update ReviewStepViewModel** with navigation and view toggle logic
7. **Redesign PoEntryView. xaml** with Fluent Design and proper spacing
8. **Redesign ReviewStepView.xaml** with Single/Table view toggle
9. **Test at 100%, 150%, 200% DPI scaling**
10. **Run unit tests and build verification**
11. **Manual testing against checklist**

---

## ⚠️ Critical Reminders

🚫 **NEVER**:
- Write to Infor Visual database (ApplicationIntent=ReadOnly ALWAYS)
- Use `{Binding}` in XAML (only `{x:Bind}`)
- Put business logic in View code-behind
- Create non-partial ViewModels
- Throw exceptions from DAO/Service methods

✅ **ALWAYS**: 
- Use stored procedures for MySQL operations
- Return Model_Dao_Result from service methods
- Inherit ViewModels from BaseViewModel or BaseStepViewModel<T>
- Use [ObservableProperty] and [RelayCommand] attributes
- Register services in App.xaml.cs DI container
- Handle DPI scaling in XAML
- Follow window sizing standards
- Apply Fluent Design System principles

---

**Begin implementation following strict MVVM architecture, read-only Infor Visual access, and modern WinUI 3 design standards.**