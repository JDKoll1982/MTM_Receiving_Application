# **MTM Receiving Application - Feature Enhancement Prompt**

## **Persona:  WinUI 3 Developer Expert**
You are a senior WinUI 3 developer specializing in MVVM architecture, with extensive experience in manufacturing receiving workflows and database operations. You follow strict MVVM patterns using CommunityToolkit. Mvvm and write clean, maintainable code. 

---

## **Phase 1: Problem Analysis & Requirements**

### **User Interface Issues**
1. **PO Textbox Alignment**: The PO textbox is misaligned when PO is not entered properly.  Alignment should only be correct after valid PO entry.
2. **Auto-Correction Timing**: PO should auto-correct **only when leaving the textbox** (on LostFocus event), NOT during text changes (TextChanged event).

### **Data Display Enhancement**
3. **Replace "Quantity Shown" with "Remaining Quantity"**: 
   - **Current**: Display shows "Quantity Shown" field
   - **Target**: Calculate and display "Remaining Quantity" instead
   - **Calculation**: `Remaining Quantity = Quantity Ordered - Quantity Received`
   - **Database References**: Use schema files to establish proper relationships: 
     - `MTMFG_Schema_FKs.csv` - Foreign key relationships
     - `MTMFG_Schema_PKs.csv` - Primary keys for table joins
     - `MTMFG_Schema_Tables.csv` - Table structure and columns
   - **Data Type**:  Display as **whole numbers only** (no decimal places)

### **User Experience Improvements**
4. **Focus Management**: When entering a new step/view in the wizard, automatically set focus to the first textbox. 
5. **Heat/Lot Number Handling**: 
   - Field is **NOT required** (not all packlists include them)
   - If left blank, set value to `"Not Entered"`

6. **Package Type Auto-Detection**:
   - If Part Number starts with `"MMC"` → Package Type = `"Coils"`
   - If Part Number starts with `"MMF"` → Package Type = `"Sheets"`
   - Otherwise → Default to `"Skids"`

### **Review and Save Step Redesign**
7. **Replace DataGrid with Form View**:
   - **Current**:  DataGrid displays user-entered data
   - **Target**: Display as labeled textboxes for better readability
   - **Navigation Controls**:
     - **Next Button**: Navigate to next part entry
     - **Back Button**: Navigate to previous part entry
     - **Table Button**: Switch to table view (showing DataGrid)
     - **Single View Button** (in table view): Switch back to single-entry form

### **XAML Redesign Requirements**
8. **Complete XAML Overhaul**:  For each XAML file modified during this process: 
   - Redesign from scratch with modern WinUI 3 best practices
   - Use proper **Fluent Design System** icons
   - Apply consistent **styling** and **theming**
   - Ensure proper use of **whitespace** and **layout**
   - Handle **DPI scaling correctly** (support 100%, 150%, 200%)
   - Follow **window sizing standards** (see `.github/instructions/window-sizing.instructions.md`)

---

## **Phase 2: Technical Implementation**

### **Architecture Patterns to Follow**

#### **1. ViewModel Pattern (Mandatory)**
```csharp
public partial class ReceivingWizardViewModel : BaseStepViewModel<ReceivingStepData>
{
    private readonly IService_InforVisual _inforVisualService;
    
    [ObservableProperty]
    private string _poNumber = string.Empty;
    
    [ObservableProperty]
    private int _remainingQuantity;
    
    [ObservableProperty]
    private string _heatLotNumber = "Not Entered";
    
    [ObservableProperty]
    private string _packageType = "Skids";
    
    partial void OnPoNumberChanged(string value)
    {
        // Validation logic only - NO auto-correction here
    }
    
    [RelayCommand]
    private void OnPoTextBoxLostFocus()
    {
        // Auto-correction logic here
        PONumber = PONumber. Trim().ToUpper();
    }
    
    [RelayCommand]
    private async Task LoadRemainingQuantityAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var result = await _inforVisualService.GetRemainingQuantityAsync(PONumber, PartID);
            if (result. IsSuccess)
            {
                RemainingQuantity = (int)Math.Floor(result.Data); // Whole numbers only
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, 
                nameof(LoadRemainingQuantityAsync), nameof(ReceivingWizardViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    partial void OnPartIDChanged(string value)
    {
        // Auto-detect package type
        if (string.IsNullOrWhiteSpace(value)) return;
        
        if (value.StartsWith("MMC", StringComparison.OrdinalIgnoreCase))
            PackageType = "Coils";
        else if (value. StartsWith("MMF", StringComparison.OrdinalIgnoreCase))
            PackageType = "Sheets";
        else
            PackageType = "Skids";
    }
}
```

#### **2. Service Layer - Infor Visual Integration (READ-ONLY)**
```csharp
public class Service_InforVisual : IService_InforVisual
{
    private readonly string _connectionString = 
        "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;... ";
    
    public async Task<Model_Dao_Result<decimal>> GetRemainingQuantityAsync(string poNumber, string partId)
    {
        try
        {
            // ⚠️ READ-ONLY - Never write to Infor Visual
            string query = @"
                SELECT 
                    pol. QUANTITY_ORDERED - ISNULL(SUM(rl.QTY_RECEIVED), 0) AS RemainingQuantity
                FROM PURCHASE_ORDER po
                INNER JOIN PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
                LEFT JOIN RECEIVER_LINE rl ON pol.PURC_ORDER_ID = rl.PURC_ORDER_ID 
                    AND pol.LINE_NO = rl. PURC_ORDER_LINE_NO
                WHERE po.ID = @PoNumber AND pol.PART_ID = @PartId
                GROUP BY pol.QUANTITY_ORDERED";
            
            // Execute with ADO.NET or Dapper
            var remainingQty = await ExecuteScalarAsync<decimal>(query, 
                new { PoNumber = poNumber, PartId = partId });
            
            return Model_Dao_Result<decimal>. Success(remainingQty);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<decimal>. Failure($"Error retrieving remaining quantity: {ex.Message}", ex);
        }
    }
}
```

#### **3.  XAML View with x:Bind**
```xml
<Page
    x:Class="MTM_Receiving_Application.Views. Receiving. ReceivingWizardView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.Receiving"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    
    <Grid Padding="24" RowSpacing="16">
        <!-- PO Number TextBox with LostFocus event -->
        <TextBox 
            Header="Purchase Order Number"
            PlaceholderText="Enter PO Number"
            Text="{x:Bind ViewModel.StepData.PONumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
            LostFocus="{x:Bind ViewModel. OnPoTextBoxLostFocusCommand}"
            HorizontalAlignment="Stretch"
            TabIndex="0"/>
        
        <!-- Remaining Quantity (Read-Only, No Decimals) -->
        <TextBox 
            Header="Remaining Quantity"
            Text="{x:Bind ViewModel.StepData.RemainingQuantity, Mode=OneWay}"
            IsReadOnly="True"
            Foreground="{ThemeResource SystemAccentColor}"/>
        
        <!-- Heat/Lot Number (Optional) -->
        <TextBox 
            Header="Heat/Lot Number (Optional)"
            PlaceholderText="Leave blank if not provided"
            Text="{x: Bind ViewModel.StepData.HeatLotNumber, Mode=TwoWay}"/>
        
        <!-- Package Type (Auto-detected) -->
        <ComboBox 
            Header="Package Type"
            SelectedItem="{x:Bind ViewModel.StepData.PackageType, Mode=TwoWay}"
            IsEnabled="False">
            <ComboBoxItem Content="Coils"/>
            <ComboBoxItem Content="Sheets"/>
            <ComboBoxItem Content="Skids"/>
        </ComboBox>
    </Grid>
</Page>
```

#### **4. Review Step - Tabbed Navigation**
```xml
<Grid>
    <!-- Single Entry View -->
    <StackPanel x:Name="SingleEntryView" Visibility="{x:Bind ViewModel. IsSingleViewVisible, Mode=OneWay}">
        <TextBox Header="Part Number" Text="{x:Bind ViewModel.CurrentEntry.PartID, Mode=OneWay}" IsReadOnly="True"/>
        <TextBox Header="Quantity" Text="{x:Bind ViewModel.CurrentEntry.Quantity, Mode=OneWay}" IsReadOnly="True"/>
        <TextBox Header="Remaining Quantity" Text="{x: Bind ViewModel.CurrentEntry. RemainingQuantity, Mode=OneWay}" IsReadOnly="True"/>
        
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Spacing="12">
            <Button Content="⬅ Back" Command="{x:Bind ViewModel.PreviousEntryCommand}"/>
            <Button Content="Next ➡" Command="{x: Bind ViewModel.NextEntryCommand}"/>
            <Button Content="📊 Table View" Command="{x:Bind ViewModel.SwitchToTableViewCommand}"/>
        </StackPanel>
    </StackPanel>
    
    <!-- Table View (DataGrid) -->
    <Grid x:Name="TableView" Visibility="{x:Bind ViewModel.IsTableViewVisible, Mode=OneWay}">
        <DataGrid ItemsSource="{x:Bind ViewModel. Entries, Mode=OneWay}" AutoGenerateColumns="False"/>
        <Button Content="📝 Single View" Command="{x: Bind ViewModel.SwitchToSingleViewCommand}" 
                VerticalAlignment="Bottom" HorizontalAlignment="Right" Margin="24"/>
    </Grid>
</Grid>
```

---

## **Phase 3: Success Criteria**

### **Functional Requirements**
✅ **PO Textbox**:
- [ ] Aligns properly only after valid PO entry
- [ ] Auto-corrects on LostFocus event (NOT on TextChanged)

✅ **Data Display**:
- [ ] "Remaining Quantity" displayed instead of "Quantity Shown"
- [ ] Calculation uses proper database joins (refer to schema CSV files)
- [ ] Displays whole numbers only (no decimals)

✅ **User Experience**:
- [ ] Focus automatically set to first textbox on step entry
- [ ] Heat/Lot Number defaults to "Not Entered" if blank
- [ ] Package Type auto-detects based on Part Number prefix

✅ **Review Step**:
- [ ] Single-entry form view with labeled textboxes
- [ ] Next/Back buttons for navigation
- [ ] Table/Single View toggle button
- [ ] DataGrid visible in table view mode

✅ **XAML Quality**:
- [ ] Modern Fluent Design icons
- [ ] Consistent styling and theming
- [ ] Proper whitespace and layout
- [ ] DPI scaling handled correctly (100%, 150%, 200%)

### **Non-Functional Requirements**
✅ **Architecture**:
- [ ] ViewModels inherit from `BaseViewModel` or `BaseStepViewModel<T>`
- [ ] All ViewModels are `partial` classes
- [ ] Uses `[ObservableProperty]` and `[RelayCommand]` attributes
- [ ] Views use `x:Bind` (NOT `Binding`)
- [ ] Read-only access to Infor Visual database

✅ **Error Handling**:
- [ ] Try-catch blocks in all async methods
- [ ] Uses `IService_ErrorHandler` for exceptions
- [ ] Returns `Model_Dao_Result` from service methods

✅ **Testing**:
- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] Manual UI testing completed

---

## **Phase 4: Critical Constraints**

🚫 **NEVER DO**:
- Write to Infor Visual database (READ-ONLY access)
- Use `Binding` instead of `x:Bind` in XAML
- Put business logic in View code-behind
- Create non-partial ViewModels
- Throw exceptions from service methods

✅ **ALWAYS DO**: 
- Use stored procedures for MySQL operations
- Return `Model_Dao_Result` from DAOs
- Register new services in `App.xaml.cs`
- Handle DPI scaling in XAML
- Follow window sizing standards

---

## **Additional Context**

### **Database Schema References**
- **Foreign Keys**: `Documentation/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_FKs.csv`
- **Primary Keys**: `Documentation/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_PKs.csv`
- **Tables**: `Documentation/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_Tables.csv`

### **Key Tables for Remaining Quantity Calculation**
```
PURCHASE_ORDER (po)
  ├─ PURC_ORDER_LINE (pol) - FK:  po.ID = pol.PURC_ORDER_ID
  └─ RECEIVER_LINE (rl) - FK: pol.PURC_ORDER_ID = rl.PURC_ORDER_ID, pol.LINE_NO = rl.PURC_ORDER_LINE_NO
```

### **Essential Reading**
- **MVVM Pattern**: `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Pattern**: `.github/instructions/dao-pattern.instructions.md`
- **Window Sizing**: `.github/instructions/window-sizing.instructions.md`
- **Error Handling**: `.github/instructions/error-handling.instructions.md`

---

**Begin implementation following strict MVVM architecture and read-only Infor Visual access.**