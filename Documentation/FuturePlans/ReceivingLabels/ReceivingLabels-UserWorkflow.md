# Receiving Label Application - User Workflow & UI Specification

## WinUI 3 Page Structure

```
MainWindow.xaml
  └── NavigationView
       ├── HomePage (Dashboard)
       ├── ReceivingPage (Main workflow)
       ├── HistoryPage (View past entries)
       └── SettingsPage (App configuration)
```

---

## Step-by-Step Workflow with UI Mockups

### Step 0: CSV Reset Prompt (StartupDialog)

**Trigger**: Application startup

**UI**: ContentDialog

```xaml
<ContentDialog
    Title="Reset CSV File?"
    PrimaryButtonText="Yes, Reset"
    SecondaryButtonText="No, Continue">
    <StackPanel>
        <TextBlock TextWrapping="Wrap">
            Do you want to reset the CSV file?
        </TextBlock>
        <TextBlock TextWrapping="Wrap" Margin="0,10,0,0" 
                   Foreground="{ThemeResource SystemColorControlAccentBrush}">
            Warning: This will make all previously saved work unprintable.
        </TextBlock>
    </StackPanel>
</ContentDialog>
```

**Logic**:
- If "Yes, Reset": Delete existing CSV files (both local and network)
- If "No, Continue": Load existing CSV if present

---

### Step 1: Enter PO Number

**Page**: `ReceivingPage.xaml`

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  Receiving Entry                                    │
├─────────────────────────────────────────────────────┤
│                                                     │
│  [Step 1] Enter PO Number                          │
│                                                     │
│  PO Number: [___________]  [Load PO]  button       │
│                                                     │
│  Status: Ready to load PO                          │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Controls**:
- `NumberBox` for PO Number (max 6 digits)
- `Button` "Load PO" (calls Infor Visual database)
- `TextBlock` for status messages

**ViewModel Code**:
```csharp
[RelayCommand]
private async Task LoadPOAsync()
{
    if (PONumber <= 0)
    {
        await ShowErrorAsync("Please enter a valid PO number.");
        return;
    }

    IsLoading = true;
    StatusMessage = "Loading PO from Infor Visual...";

    try
    {
        var result = await _inforVisualService.GetPOAsync(PONumber);
        if (result.IsSuccess && result.Data != null)
        {
            CurrentPO = result.Data;
            Parts = new ObservableCollection<Model_InforVisualPart>(result.Data.Parts);
            StatusMessage = $"PO {PONumber} loaded. {Parts.Count} parts found.";
            CurrentStep = 2; // Move to part selection
        }
        else
        {
            await ShowErrorAsync(result.ErrorMessage ?? "Failed to load PO.");
        }
    }
    catch (Exception ex)
    {
        await ShowErrorAsync($"Error loading PO: {ex.Message}");
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

### Step 2: Select Part Number

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 2] Select Part Number                       │
│                                                     │
│  Parts on PO #123456:                              │
│                                                     │
│  ┌───────────────────────────────────────────┐    │
│  │ Part ID     │ Type        │ Qty Ordered   │    │
│  ├───────────────────────────────────────────┤    │
│  │ > PART-001  │ Raw Mat.    │ 500          │    │
│  │   PART-002  │ Raw Mat.    │ 300          │    │
│  │   PART-003  │ Finished    │ 250          │    │
│  └───────────────────────────────────────────┘    │
│                                                     │
│  [Select Part] button                              │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Controls**:
- `DataGrid` or `ListView` with SelectionMode="Single"
- Columns: PartID, PartType, QuantityOrdered, Description
- `Button` "Select Part" (enabled when row selected)

**ViewModel Code**:
```csharp
[ObservableProperty]
private Model_InforVisualPart? selectedPart;

[RelayCommand(CanExecute = nameof(CanSelectPart))]
private void SelectPart()
{
    if (SelectedPart == null) return;

    CurrentPartID = SelectedPart.PartID;
    CurrentPartType = SelectedPart.PartType;
    StatusMessage = $"Part {CurrentPartID} selected. Enter skid information.";
    CurrentStep = 3;
}

private bool CanSelectPart() => SelectedPart != null;
```

---

### Step 3: Enter Skid Information

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 3] Skid Information for PART-001            │
│                                                     │
│  Number of Lines: [___]  (How many skids?)         │
│  Total Skids:     [___]  (Same as # of lines)      │
│                                                     │
│  Note: Each line represents one skid               │
│                                                     │
│  [Continue to Quantities] button                   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Controls**:
- `NumberBox` for Number of Lines (1-99)
- `NumberBox` for Total Skids (same value, or calculated)
- `Button` "Continue to Quantities"

**ViewModel Code**:
```csharp
[ObservableProperty]
private int numberOfLines = 1;

[RelayCommand]
private void ContinueToQuantities()
{
    if (NumberOfLines <= 0)
    {
        StatusMessage = "Number of lines must be greater than 0.";
        return;
    }

    // Initialize temporary line collection
    TempLines = new ObservableCollection<Model_ReceivingLine>();
    for (int i = 0; i < NumberOfLines; i++)
    {
        TempLines.Add(new Model_ReceivingLine
        {
            PartID = CurrentPartID,
            PartType = CurrentPartType,
            PONumber = PONumber
        });
    }

    CurrentStep = 4;
    StatusMessage = $"Enter quantities for {NumberOfLines} lines.";
}
```

---

### Step 4: Enter Quantities

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 4] Enter Quantities for Each Line           │
│                                                     │
│  Part: PART-001  |  PO: 123456                     │
│                                                     │
│  ┌───────────────────────────────────────────┐    │
│  │ Line # │ Quantity │ Status                │    │
│  ├───────────────────────────────────────────┤    │
│  │ 1      │ [____]   │ [✓] or [Pending]     │    │
│  │ 2      │ [____]   │ [Pending]             │    │
│  │ 3      │ [____]   │ [Pending]             │    │
│  └───────────────────────────────────────────┘    │
│                                                     │
│  [Continue to Heat Numbers] button                 │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Controls**:
- `DataGrid` with editable Quantity column
- Each row represents one line from Step 3
- `Button` "Continue to Heat Numbers" (enabled when all quantities entered)

**ViewModel Code**:
```csharp
[RelayCommand(CanExecute = nameof(CanContinueToHeat))]
private void ContinueToHeat()
{
    CurrentStep = 5;
    StatusMessage = "Enter heat/lot numbers for each line.";
}

private bool CanContinueToHeat() => 
    TempLines != null && TempLines.All(l => l.Quantity > 0);
```

---

### Step 5: Enter Heat Numbers (with Smart Selection)

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 5] Enter Heat/Lot Numbers                   │
│                                                     │
│  Part: PART-001  |  PO: 123456                     │
│                                                     │
│  ┌───────────────────────────────────────────┐    │
│  │ Line # │ Qty │ Heat Number │ Status       │    │
│  ├───────────────────────────────────────────┤    │
│  │ 1      │ 500 │ [________]  │ [Pending]    │    │
│  │ 2      │ 300 │             │              │    │
│  │ 3      │ 250 │             │              │    │
│  └───────────────────────────────────────────┘    │
│                                                     │
│  Quick Select Heat Numbers:                        │
│  ┌──────────────────────────────────────┐         │
│  │ [☑] H123456  (Line 1)                │         │
│  │ [☐] H789012                          │         │
│  └──────────────────────────────────────┘         │
│                                                     │
│  [Continue to Packages] button                     │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Logic**:
- As user enters heat number for Line 1, add it to quick-select ListView
- When user enters heat for Line 2:
  - If it's a new heat, add checkbox to list
  - If it matches Line 1's heat, show checked checkbox
- User can check/uncheck boxes to apply heat to multiple lines

**ViewModel Code**:
```csharp
[ObservableProperty]
private ObservableCollection<HeatCheckboxItem> availableHeats = new();

partial void OnCurrentLineHeatChanged(string? value)
{
    if (string.IsNullOrWhiteSpace(value)) return;

    // Add to quick-select list if not already there
    if (!AvailableHeats.Any(h => h.HeatNumber == value))
    {
        AvailableHeats.Add(new HeatCheckboxItem
        {
            HeatNumber = value,
            IsChecked = false,
            LineNumber = CurrentLineIndex + 1
        });
    }
}

[RelayCommand]
private void ApplyHeatToLines(HeatCheckboxItem heatItem)
{
    foreach (var line in TempLines)
    {
        if (string.IsNullOrWhiteSpace(line.Heat) || heatItem.IsChecked)
        {
            line.Heat = heatItem.HeatNumber;
        }
    }
}
```

---

### Step 6: Enter Packages Per Skid

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 6] Enter Packages Per Skid                  │
│                                                     │
│  Part: PART-001  |  PO: 123456                     │
│                                                     │
│  ┌───────────────────────────────────────────────────┐│
│  │ Line │ Qty │ Heat    │ Pkgs │ Wt/Pkg │ Status ││
│  ├───────────────────────────────────────────────────┤│
│  │ 1    │ 500 │ H123456 │ [__] │ TBD    │ [Pend] ││
│  │ 2    │ 300 │ H123456 │ [__] │ TBD    │ [Pend] ││
│  │ 3    │ 250 │ H789012 │ [__] │ TBD    │ [Pend] ││
│  └───────────────────────────────────────────────────┘│
│                                                     │
│  Note: Weight per package = Quantity ÷ Packages    │
│                                                     │
│  [Save & Continue] button                          │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**Controls**:
- `DataGrid` with editable PackagesOnSkid column
- Calculated WeightPerPackage column (read-only)
- `Button` "Save & Continue"

---

### Step 7: Save or Continue Options

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 7] Review & Save                            │
│                                                     │
│  You have entered 3 lines for Part PART-001        │
│  Total Quantity: 1050                              │
│                                                     │
│  What would you like to do?                        │
│                                                     │
│  [Add Another Part/PO]  button                     │
│  (Returns to Step 1)                               │
│                                                     │
│  [Save to CSV & Database]  button                  │
│  (Proceeds to Step 8)                              │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**ViewModel Code**:
```csharp
[RelayCommand]
private void AddAnotherPart()
{
    // Add current temp lines to session
    foreach (var line in TempLines)
    {
        _receivingSession.AddLine(line);
    }

    // Reset for next entry
    TempLines.Clear();
    SelectedPart = null;
    PONumber = 0;
    CurrentStep = 1;
    StatusMessage = "Enter another PO number.";
}

[RelayCommand]
private async Task SaveAndFinishAsync()
{
    // Add current temp lines to session
    foreach (var line in TempLines)
    {
        _receivingSession.AddLine(line);
    }

    // Proceed to save
    CurrentStep = 8;
    await SaveToCsvAndDatabaseAsync();
}
```

---

### Step 8-9: Save to CSV & Database

**UI Layout**:
```
┌─────────────────────────────────────────────────────┐
│  [Step 8-9] Saving...                              │
│                                                     │
│  ┌─────────────────────────────────────────────┐  │
│  │ [████████████████░░░░░░░░░░░░] 60%         │  │
│  └─────────────────────────────────────────────┘  │
│                                                     │
│  ✓ Saved to local CSV (3 lines)                   │
│  ✓ Saved to network CSV (3 lines)                 │
│  ⧗ Saving to MySQL database...                    │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**After Save**:
```
┌─────────────────────────────────────────────────────┐
│  Save Complete!                                     │
│                                                     │
│  ✓ 3 lines saved to CSV files                      │
│  ✓ 3 records saved to MySQL database              │
│                                                     │
│  CSV Locations:                                    │
│  - %APPDATA%\ReceivingData.csv                     │
│  - \\mtmanu-fs01\...\JKOLL\ReceivingData.csv      │
│                                                     │
│  [Start New Entry] button                          │
│  [View History] button                             │
│  [Print Labels] button (opens LabelView)           │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**ViewModel Code**:
```csharp
[RelayCommand]
private async Task SaveToCsvAndDatabaseAsync()
{
    IsLoading = true;
    SaveProgress = 0;
    SaveStatus = "Preparing to save...";

    try
    {
        // Step 1: Save to local CSV
        SaveStatus = "Saving to local CSV...";
        await _csvService.SaveToLocalAsync(_receivingSession.Lines);
        SaveProgress = 33;

        // Step 2: Save to network CSV
        SaveStatus = "Saving to network CSV...";
        await _csvService.SaveToNetworkAsync(_receivingSession.Lines);
        SaveProgress = 66;

        // Step 3: Save to MySQL
        SaveStatus = "Saving to MySQL database...";
        await _databaseService.SaveReceivingLinesAsync(_receivingSession.Lines);
        SaveProgress = 100;

        SaveStatus = $"✓ {_receivingSession.Lines.Count} lines saved successfully!";
        
        await Task.Delay(2000); // Show success message
        
        // Reset session
        _receivingSession.Reset();
        CurrentStep = 0; // Back to start
    }
    catch (Exception ex)
    {
        await ShowErrorAsync($"Save failed: {ex.Message}");
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

## ViewModel Structure

### ReceivingPageViewModel.cs

```csharp
public partial class ReceivingPageViewModel : ObservableObject
{
    private readonly IInforVisualService _inforVisualService;
    private readonly ICsvService _csvService;
    private readonly IDatabaseService _databaseService;
    private readonly Model_ReceivingSession _receivingSession;

    [ObservableProperty] private int currentStep = 0;
    [ObservableProperty] private int poNumber;
    [ObservableProperty] private Model_InforVisualPO? currentPO;
    [ObservableProperty] private ObservableCollection<Model_InforVisualPart> parts = new();
    [ObservableProperty] private Model_InforVisualPart? selectedPart;
    [ObservableProperty] private int numberOfLines = 1;
    [ObservableProperty] private ObservableCollection<Model_ReceivingLine> tempLines = new();
    [ObservableProperty] private string statusMessage = "Ready";
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private int saveProgress;

    // Commands defined above with [RelayCommand]
}
```

---

## Navigation Flow

```
App Startup
    ↓
Step 0: CSV Reset Prompt
    ↓
ReceivingPage (Step 1)
    ↓
Step 1: Enter PO → Load from Infor Visual
    ↓
Step 2: Select Part → User selects from list
    ↓
Step 3: Skid Info → Enter # of lines
    ↓
Step 4: Quantities → Enter quantity per line
    ↓
Step 5: Heat Numbers → Enter/select heat per line
    ↓
Step 6: Packages → Enter packages per skid
    ↓
Step 7: Review → Add another part OR Save
    ↓
Step 8-9: Save → CSV + MySQL
    ↓
Complete → Return to Step 1 or Exit
```

---

**Next**: See [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) for MySQL table definitions and [INFOR_VISUAL_INTEGRATION.md](INFOR_VISUAL_INTEGRATION.md) for SQL Server queries.
