---
name: Outside Service Setup ViewModel Tests
description: "Generate comprehensive xUnit tests for ViewModel_OutsideService_Setup covering all commands, computed properties, validation paths, and service interactions"
agent: agent
tools:
  [
    vscode,
    execute,
    read,
    agent,
    edit,
    search,
    web,
    browser,
    "filesystem/*",
    "awesome-copilot/*",
    todo,
  ]
argument-hint: 'Optionally provide a specific test area to focus on (e.g., "validation", "vendor logic", "package count")'
---

# Outside Service Setup ViewModel — Unit Tests

Generate and maintain unit tests for `ViewModel_OutsideService_Setup` in
`MTM_Receiving_Application.Tests/Unit/Module_OutsideService/ViewModels/ViewModel_OutsideService_SetupTests.cs`.

## References

- ViewModel source: `Module_OutsideService/ViewModels/ViewModel_OutsideService_Setup.cs`
- Service contract: `Module_OutsideService/Contracts/IService_OutsideService.cs`
- Models: `Module_OutsideService/Models/`
- Existing tests: `MTM_Receiving_Application.Tests/Unit/Module_OutsideService/ViewModels/ViewModel_OutsideService_SetupTests.cs`
- Test conventions: `.github/instructions/testing/testing-strategy.instructions.md`

## Scope & Preconditions

Read the ViewModel source fully before generating tests.
Do NOT re-generate or duplicate tests that already exist in the file.
Add only the missing cases enumerated in **Test Inventory** below.

## Test Inventory — Implement All Missing Cases

### Region 1 · Constructor

| ID  | Scenario                       | Expected                                                                        |
| --- | ------------------------------ | ------------------------------------------------------------------------------- |
| C1  | `outsideService` arg is `null` | `ArgumentNullException` thrown                                                  |
| C2  | All args valid                 | `Title == "Outside Service Setup"`, `IsBusy == false`, `EditablePackages` empty |

### Region 2 · LoadLineAsync — Phase & Computed Properties

| ID  | Scenario                                                 | Expected                                                                                                              |
| --- | -------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| L1  | Line is `Initialize` phase                               | `IsInitializePhase == true`, `IsSetupPhase == false`, `PrimaryActionText == "Save Setup"`                             |
| L2  | Line is `Setup` phase                                    | `IsInitializePhase == false`, `IsSetupPhase == true`, `PrimaryActionText == "Save Changes"` _(already exists — skip)_ |
| L3  | `CurrentPhaseText` after load                            | Equals `line.LinePhase.ToString()`                                                                                    |
| L4  | BOL, ShippingContact, SetupNotes, CompletionNotes mapped | Each property equals corresponding line field                                                                         |

### Region 3 · LoadLineAsync — Vendor Logic

| ID  | Scenario                                                             | Expected                                                                                                                                                                |
| --- | -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| V1  | Suggestions returned, line has `SetupVendorSource = null`            | `HasVendorSuggestions == true`, `IsCustomVendorForced == false`, `CanToggleCustomVendor == true`, `IsVendorSuggestionPickerVisible == true`, `UseCustomVendor == false` |
| V2  | Suggestions returned, line source is `"custom"`                      | `UseCustomVendor == true`, `CustomVendorName` equals line's vendor name, `SelectedVendorSuggestion == null`                                                             |
| V3  | No suggestions _(already exists — skip)_                             | —                                                                                                                                                                       |
| V4  | Suggestions returned and a suggestion matches line's `SetupVendorId` | `SelectedVendorSuggestion` is the matching suggestion                                                                                                                   |
| V5  | `GetVendorSuggestionsAsync` returns failure                          | `HasVendorSuggestions == false`, `IsCustomVendorForced == true`                                                                                                         |

### Region 4 · LoadLineAsync — Package Mapping

| ID  | Scenario            | Expected                                                                 |
| --- | ------------------- | ------------------------------------------------------------------------ |
| P1  | Line has 2 packages | `EditablePackages.Count == 2`, quantities match `PackageQuantity` values |
| P2  | `PackageCount <= 0` | `PackageCountInputValue == 1`, single editable package row               |

### Region 5 · Package Count Changes

| ID  | Scenario                  | Expected                                             |
| --- | ------------------------- | ---------------------------------------------------- |
| PC1 | Increase count after load | New row added; existing rows retain their quantities |
| PC2 | Decrease count after load | Rows trimmed; surviving rows retain their quantities |
| PC3 | NaN / Infinity input      | Normalizes to 1 package row                          |

### Region 6 · Vendor Toggle

| ID  | Scenario                      | Expected                                           |
| --- | ----------------------------- | -------------------------------------------------- |
| VT1 | Set `UseCustomVendor = true`  | `SelectedVendorSuggestion == null`                 |
| VT2 | Set `UseCustomVendor = false` | `SelectedVendorSuggestion` unchanged (not cleared) |

### Region 7 · SavePrimaryAction — Validation

| ID  | Scenario                    | Expected                                               |
| --- | --------------------------- | ------------------------------------------------------ |
| SV1 | Any package quantity `<= 0` | `SaveSetupAsync` never called, `IsBusy == false` after |
| SV2 | `CurrentLine is null`       | Command no-ops; service never called                   |

### Region 8 · SavePrimaryAction — Success Paths

| ID  | Scenario                                   | Expected                                                                                                |
| --- | ------------------------------------------ | ------------------------------------------------------------------------------------------------------- |
| SS1 | Initialize phase save                      | Service called once with `LinePhase == Setup`; `LineSaved` event raised; `ReturnRequested` event raised |
| SS2 | Setup phase save _(already exists — skip)_ | —                                                                                                       |
| SS3 | Custom vendor used                         | `SetupVendorSource == "custom"`, `SetupVendorName == CustomVendorName.Trim()`, `SetupVendorId == null`  |
| SS4 | Suggested vendor used                      | `SetupVendorSource == "suggested"`, `SetupVendorName` from suggestion, `SetupVendorId` from suggestion  |

### Region 9 · SavePrimaryAction — Failure Path

| ID  | Scenario                         | Expected                                                                      |
| --- | -------------------------------- | ----------------------------------------------------------------------------- |
| SF1 | `SaveSetupAsync` returns failure | `LineSaved` NOT raised, `ReturnRequested` NOT raised, `IsBusy == false` after |

### Region 10 · ReturnToQueue Command

| ID  | Scenario         | Expected                                    |
| --- | ---------------- | ------------------------------------------- |
| RT1 | Command executed | `ReturnRequested` event raised exactly once |

## Workflow

1. Read the full ViewModel source and existing test file.
2. For each test ID not marked _(skip)_, check whether the test already exists. Skip if present.
3. Add all missing tests as individual `[Fact]` methods inside the existing class, grouped by `#region`.
4. Do NOT remove or rewrite any existing test.
5. Build with `dotnet build MTM_Receiving_Application.slnx` and fix any compile errors.
6. Run `dotnet test --filter "FullyQualifiedName~ViewModel_OutsideService_SetupTests"` — all must pass.

## Output Expectations

- Tests added directly to the existing class file.
- All tests carry `[Trait("Category", "Unit")]` and `[Trait("Layer", "ViewModel")]`.
- FluentAssertions used for all assertions.
- Each test is independent (own `CreateViewModel` call, own mock setup).
- Naming pattern: `MethodOrScenario_Should<Result>_When<Condition>`.
