# ?? CRITICAL WORKFLOW CORRECTION REQUIRED

**Created:** 2025-01-30  
**Severity:** HIGH - Current implementation does not match user requirements  
**Impact:** Step 1 workflow (PONumberEntry + PartSelection) must be redesigned

---

## **? Problem Identified**

The current Wizard Step 1 implementation **does not match the Old_Module_Receiving workflow**.

### **Current (WRONG) Implementation:**
```
Step 1A: PONumberEntry
  - User types PO Number
  - Validates format
  
Step 1B: PartSelection  ? WRONG!
  - User TYPES part number manually
  - Auto-pads to format (MMC000001)
  - Validates part existence
  
Step 1C: LoadCountEntry
  - User enters load count
```

### **Correct Workflow (from Old Module):**
```
Step 1A: PONumberEntry
  - User types PO Number
  - Validates format
  - System queries Infor Visual ERP  ? MISSING!
  - Loads list of available parts on PO  ? MISSING!
  
Step 1B: PartSelection  ? CORRECTED
  - User SELECTS from dropdown/list (not typing!)
  - Parts displayed: PartID, Description, Remaining Qty, Line #
  - Package type AUTO-DETECTED from PartID prefix:
    • MMC* ? Coils
    • MMF* ? Sheets
    • Others ? Skids
  
Step 1C: LoadCountEntry
  - User enters load count
```

---

## **?? Evidence from Old Module Code**

### **File:** `Old_Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs`

**Lines 154-200: LoadPOAsync() Method**
```csharp
[RelayCommand]
private async Task LoadPOAsync()
{
    // ...
    var result = await _inforVisualService.GetPOWithPartsAsync(PoNumber);
    if (result.IsSuccess && result.Data != null)
    {
        Parts.Clear();
        
        // Load parts and populate remaining quantity for each
        foreach (var part in result.Data.Parts)
        {
            // Get remaining quantity for this part
            var remainingQtyResult = await _inforVisualService.GetRemainingQuantityAsync(PoNumber, part.PartID);
            if (remainingQtyResult.IsSuccess)
            {
                part.RemainingQuantity = remainingQtyResult.Data;
            }
            
            Parts.Add(part);  // ? Populates dropdown list
        }
    }
}
```

**Lines 380-395: OnSelectedPartChanged()**
```csharp
partial void OnSelectedPartChanged(Model_InforVisualPart? value)
{
    _workflowService.CurrentPart = value;
    
    // Auto-detect package type when a part is selected from PO
    if (value != null && !string.IsNullOrWhiteSpace(value.PartID))
    {
        var upperPart = value.PartID.Trim().ToUpper();
        
        if (upperPart.StartsWith("MMC"))
            PackageType = "Coils";
        else if (upperPart.StartsWith("MMF"))
            PackageType = "Sheets";
        else
            PackageType = "Skids";
    }
}
```

---

## **?? Missing Dependencies**

### **Service Interface Already Exists:**
- ? `IService_InforVisual` - Located in `Module_Core/Contracts/Services/`
- ? Method: `GetPOWithPartsAsync(string poNumber)` - Returns `Model_Dao_Result<List<Model_InforVisualPO>>`
- ? Method: `GetRemainingQuantityAsync(string poNumber, string partID)` - Returns remaining qty

### **Service Implementation Already Exists:**
- ? Service is registered in DI
- ? DAO: `Dao_InforVisualPO` with query support

### **Model Already Exists:**
- ? `Model_InforVisualPO` - Has all required properties:
  - `PartNumber`, `PartDescription`
  - `OrderedQty`, `ReceivedQty`, `RemainingQty`
  - `PoLine`, `UnitOfMeasure`, `DueDate`
  - `VendorCode`, `VendorName`, `PoStatus`

---

## **? Corrective Actions Required**

### **Priority 1: Fix PartSelection ViewModel (IMMEDIATE)**

**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PartSelection.cs`

**Changes:**
1. **Remove** manual part entry logic:
   - Remove `PartNumber` property (user doesn't type it)
   - Remove `AutoPadPartNumber()` method
   - Remove part number regex validation
   
2. **Add** part list selection logic:
   - Add `ObservableCollection<Model_InforVisualPO> AvailablePartsOnPO`
   - Add `Model_InforVisualPO? SelectedPartFromPO` property
   - Add `OnSelectedPartFromPOChanged()` handler for package type detection
   
3. **Update** Quality Hold detection:
   - Trigger on `SelectedPartFromPO` change (not manual entry)
   - Use `SelectedPartFromPO.PartNumber` for detection

### **Priority 2: Enhance PONumberEntry ViewModel**

**File:** `Module_Receiving/ViewModels/Wizard/Step1/ViewModel_Receiving_Wizard_Display_PONumberEntry.cs`

**Changes:**
1. **Add** Infor Visual service dependency:
   ```csharp
   private readonly IService_InforVisual _inforVisualService;
   ```

2. **Add** parts collection:
   ```csharp
   [ObservableProperty]
   private ObservableCollection<Model_InforVisualPO> _availablePartsOnPO = new();
   ```

3. **Update** ValidatePoNumberAsync():
   - After validation succeeds ? call `LoadPartsFromPOAsync()`
   - Populate `AvailablePartsOnPO` collection

4. **Add** new method:
   ```csharp
   private async Task LoadPartsFromPOAsync()
   {
       var result = await _inforVisualService.GetPOWithPartsAsync(PoNumber);
       if (result.IsSuccess && result.Data != null)
       {
           AvailablePartsOnPO.Clear();
           foreach (var part in result.Data)
           {
               // Load remaining qty for each part
               var qtyResult = await _inforVisualService.GetRemainingQuantityAsync(PoNumber, part.PartNumber);
               if (qtyResult.IsSuccess)
               {
                   part.RemainingQty = qtyResult.Data;
               }
               AvailablePartsOnPO.Add(part);
           }
           StatusMessage = $"Loaded {AvailablePartsOnPO.Count} parts from PO";
       }
   }
   ```

### **Priority 3: Update Orchestration**

**File:** `Module_Receiving/ViewModels/Wizard/Orchestration/ViewModel_Receiving_Wizard_Orchestration_MainWorkflow.cs`

**Changes:**
- Pass `AvailablePartsOnPO` from PONumberEntry to PartSelection
- Update step transition to carry part list data

---

## **?? Impact Assessment**

### **Tasks Affected:**
- ? Task 4.6: PONumberEntry ViewModel - **NEEDS REWORK**
- ? Task 4.7: PartSelection ViewModel - **NEEDS COMPLETE REDESIGN**
- Task 5.5-5.6: PONumberEntry View - **NEEDS UPDATE** (show parts list)
- Task 5.7-5.8: PartSelection View - **NEEDS UPDATE** (ComboBox not TextBox)

### **Estimated Rework Effort:**
- PONumberEntry ViewModel: **+2 hours** (add Infor Visual integration)
- PartSelection ViewModel: **+3 hours** (redesign from manual entry to selection)
- Views (Phase 5): **+2 hours** (update XAML for list display)
- **Total:** ~7 hours additional work

---

## **?? Next Steps**

1. **Document this correction** in memory bank
2. **Update User Approved Plan** if needed
3. **Pause CSV Export implementation** until workflow is corrected
4. **Fix PartSelection ViewModel** to use list selection
5. **Enhance PONumberEntry** to load parts from Infor Visual
6. **Update task inventory** with rework items

---

## **?? User Confirmation Required**

**Questions for User:**
1. ? Confirm: Part selection should be from **dropdown list** (not manual entry)?
2. ? Confirm: PO query should load parts from **Infor Visual ERP**?
3. ? Confirm: Package type should **auto-detect** from part prefix (MMC/MMF)?
4. Should we support **Non-PO mode** (manual part entry without PO)?

**This correction is CRITICAL for matching the old app's UX and business logic.**

---

**End of Correction Document**
