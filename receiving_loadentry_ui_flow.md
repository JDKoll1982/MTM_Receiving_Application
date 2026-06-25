<style>
  /* Define Landscape Page Layout */
  @page {
    size: 11in 8.5in;
    margin: 0.4in;
  }
  
  @media print {
    html, body {
      width: 10.2in !important;
      margin: 0 !important;
      padding: 0 !important;
    }
    
    /* Container that fills the entirety of Page 1 and isolates it */
    .first-page-container {
      width: 100% !important;
      height: 7.4in !important; /* Maximize height on a 8.5" page with margins */
      display: flex !important;
      flex-direction: column !important;
      page-break-after: always !important;
      break-after: page !important;
    }

    /* Force the title and diagram to stretch to the container boundaries */
    .first-page-container h2 {
      margin-top: 0 !important;
      margin-bottom: 10px !important;
    }

    .first-page-container .mermaid, 
    .first-page-container .mermaid svg {
      width: 100% !important;
      height: 100% !important; /* Scales up the internal vector assets to fill space */
      max-width: 100% !important;
    }
    
    /* Clean formatting for the notes on Page 2 */
    .second-page-container {
      page-break-before: always !important;
      break-before: page !important;
      padding-top: 0.2in !important;
    }
  }
</style>

<div class="first-page-container">

## View -> ViewModel -> Service flow for `SelectedPartId`

```mermaid
%%{init: {'flowchart': {'useMaxWidth': true, 'htmlLabels': true, 'curve': 'basis'}}}%%
flowchart LR

  %% Column 1: Core Triggers & Interfaces
  subgraph Inputs ["Triggers & Interfaces"]
    H["<b>PO Entry VM</b><br>POEntry.cs (L625)<br>OnSelectedPartChanged<br>Sets CurrentPart"]
    E["<b>Service Interface</b><br>IService_Workflow.cs (L49)<br>Declares CurrentPart"]
  end

  %% Column 2: Service Core State & Models
  subgraph ServiceImpl ["Service Implementation"]
    G["<b>Workflow Service</b><br>Workflow.cs (L77)<br>Property: CurrentPart"]
    F["<b>State Engine</b><br>Workflow.cs (L61)<br>CurrentStep Setter<br>Raises StepChanged"]
    I["<b>Data Model</b><br>Model_InforVisualPart<br>Fields: PartID, Desc,<br>POLine, UOM, Qty"]
  end

  %% Column 3: Consumers & View Presentation
  subgraph Consumer ["Load Entry Consumer"]
    C["<b>Load Entry VM</b><br>LoadEntry.cs (L151)<br>OnStepChanged()<br>Reads CurrentPart"]
    D["<b>Update Routine</b><br>LoadEntry.cs (L259)<br>UpdatePartInfo()<br>Sets SelectedPartId"]
    B["<b>Backing State</b><br>LoadEntry.cs (L34)<br>Property: SelectedPartId"]
    A["<b>View Layer</b><br>LoadEntry.xaml (L48)<br>x:Bind SelectedPartId"]
  end

  %% Data Flow & Dependencies
  H -->|Updates State| G
  E -.->|Implements| G
  G ---|Schema Type| I
  
  G -->|Provides Data| C
  F -->|Triggers Event| C
  
  C -->|Executes| D
  D -->|Mutates| B
  A -->|OneWay Bind| B

  %% Precise Visual Accents
  style A fill:#f9f,stroke:#333,stroke-width:1px
  style B fill:#bbf,stroke:#333,stroke-width:1px
  style C fill:#bbf,stroke:#333,stroke-width:1px
  style D fill:#bbf,stroke:#333,stroke-width:1px
  style E fill:#bfb,stroke:#333,stroke-width:1px
  style F fill:#bfb,stroke:#333,stroke-width:1px
  style G fill:#bfb,stroke:#333,stroke-width:1px
  style H fill:#ffd,stroke:#333,stroke-width:1px
```

</div>

### Method anchors (key methods referenced above)

- View / XAML
  - No methods — UI binds to `ViewModel.SelectedPartId` via `x:Bind`.

- `ViewModel_Receiving_LoadEntry` (file: Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs)
  - `OnStepChanged(object?, EventArgs)` — subscribed to `_workflowService.StepChanged`, updates UI state when `CurrentStep` becomes `LoadEntry`.
  - `UpdatePartInfo(string partId, string description)` — sets `SelectedPartId` and `SelectedPartDescription`.
  - `RefreshRecommendedLocationsAsync()` — asynchronous lookup of recommended locations using `_workflowService.CurrentPart` and `_workflowService.CurrentPONumber`.
  - `CreateLoadsAsync()` — validates `NumberOfLoads` and synchronizes `_workflowService.NumberOfLoads`.
  - `OnNumberOfLoadsChanged(int)` — partial change handler that sets `_workflowService.NumberOfLoads`.
  - `OnLocationChanged(string)` — partial change handler that sets `_workflowService.CurrentLocation`.
  - `ApplyRecommendedLocation(Model_ReceivingRecommendedLocation)` — helper command that sets `Location`.

- `ViewModel_Receiving_POEntry` (file: Module_Receiving/ViewModels/ViewModel_Receiving_POEntry.cs)
  - `OnSelectedPartChanged(Model_InforVisualPart?)` — assigns `_workflowService.CurrentPart = value`, sets `CurrentPODueDate` and `CurrentLocation` in the service, performs quality-hold checks, and notifies workflow buttons.
  - `CheckQualityHoldOnSelectedPartAsync(Model_InforVisualPart)` — warns and clears selection if restricted.
  - `InitializeAsync()` — initialization logic for PO entry (mock data auto-fill, etc.).

- `Service_ReceivingWorkflow` (file: Module_Receiving/Services/Service_ReceivingWorkflow.cs)
  - `StartWorkflowAsync()` — workflow init and step selection logic.
  - `AdvanceToNextStepAsync()` — validation and step transitions (sets `CurrentStep` and relies on `CurrentPart`).
  - `GenerateLoads()` — creates `Model_ReceivingLoad` objects based on `CurrentPart`, `NumberOfLoads`, and `CurrentLocation`.
  - `AddCurrentPartToSessionAsync()` — commits the current batch and resets PO/part inputs.
  - `SaveSessionAsync()` / `SaveToDatabaseOnlyAsync()` — final save workflows that consume `CurrentSession.Loads` produced by `GenerateLoads()`.
  - Property: `Model_InforVisualPart? CurrentPart { get; set; }` — the canonical source-of-truth for the selected part.

These method names are the implementation anchors you can open directly to inspect and follow the runtime flow when a user selects a part in PO entry and moves to LoadEntry.


<div class="second-page-container">

**Notes / explanation of anchors included in the diagram**

- `View_Receiving_LoadEntry.xaml` — TextBlock showing part number bound to `ViewModel.SelectedPartId` (match at file line ~48).
- `ViewModel_Receiving_LoadEntry.cs` — backing field `_selectedPartId` (line 34), `OnStepChanged` subscribes to `_workflowService.StepChanged` (line ~151) and calls `UpdatePartInfo` (line ~259) which sets `SelectedPartId`.
- `IService_ReceivingWorkflow.cs` — declares `CurrentPart` (line 49) used as the upstream source for part info.
- `Service_ReceivingWorkflow.cs` — implements `CurrentPart` (line 77) and raises `StepChanged` when `CurrentStep` changes (setter at line 61).
- `ViewModel_Receiving_POEntry.cs` — when a user selects a part in the PO entry UI, `OnSelectedPartChanged` assigns `_workflowService.CurrentPart = value;` (line ~625), which is the originating action that eventually flows to the LoadEntry view.
- `Model_InforVisualPart` fields (part id, description, etc.) are read by `LoadEntry` to populate UI and recommended locations lookup.

</div>
