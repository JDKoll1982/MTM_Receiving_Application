# Feature Specification: CSV Export and LabelView Integration

**Feature Branch**: `008-csv-export-integration`  
**Created**: 2025-12-26  
**Status**: Ready for Implementation  
**Parent Feature**:  Dunnage Receiving System V2  
**Depends On**: 004-services-layer

## Overview

Finalize the CSV export functionality and ensure seamless integration with LabelView label printing system.  This includes validating dynamic column generation, file path management, network share handling, and label template compatibility.

**Architecture Principle**: CSV export is the bridge between receiving workflow and physical label production. Export must be reliable, flexible (dynamic columns), and compatible with existing LabelView infrastructure.

## User Scenarios & Testing

### User Story 1 - Dynamic CSV Column Generation (Priority: P1)

As a **LabelView template designer**, I need CSV exports to include all possible specification columns (union of all spec keys across all types) so that label templates can reference any spec field without modification.

**Why this priority**: Dynamic columns eliminate the need to update label templates when new spec fields are added. Critical for system maintainability.

**Independent Test**: Can be tested by creating types with different specs (Type A:  Width/Height, Type B: Width/Material), exporting loads of both types, and verifying CSV has all unique spec columns (Width, Height, Material).

**Acceptance Scenarios**: 

1. **Given** database has types with varying specs, **When** `GetAllSpecKeysAsync()` is called, **Then** union of all unique spec keys is returned
2. **Given** union of spec keys is ["Width", "Height", "Depth", "Material", "IsInventoriedToVisual"], **When** CSV header is generated, **Then** columns are:  DunnageType, PartID, Quantity, PONumber, ReceivedDate, Employee, Depth, Height, IsInventoriedToVisual, Material, Width (alphabetical after fixed)
3. **Given** load with Type "Pallet" (has Width/Height/Depth), **When** CSV row is written, **Then** Width/Height/Depth cells are populated, Material cell is blank
4. **Given** load with Type "Foam" (has Material), **When** CSV row is written, **Then** Material cell is populated, Width/Height/Depth cells are blank
5. **Given** new spec "Color" added to Type "Crate", **When** next export occurs, **Then** "Color" column appears in CSV for all loads

---

### User Story 2 - Dual-Path File Writing (Priority: P1)

As a **receiving user**, I need CSV files written to both local AppData (guaranteed) and network share (best-effort) so that labels are accessible even when network is unavailable.

**Why this priority**:  Network reliability is unpredictable. Local fallback ensures workflow continuity. Network path enables centralized label management.

**Independent Test**: Can be tested by disconnecting network, exporting loads, verifying local file created, reconnecting network, exporting again, and verifying both local and network files exist.

**Acceptance Scenarios**:

1. **Given** CSV export is triggered, **When** local path write succeeds, **Then** file is created at `%APPDATA%\MTM_Receiving_Application\DunnageData.csv`
2. **Given** network is available, **When** network path write succeeds, **Then** file is created at `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv`
3. **Given** network share is unavailable, **When** export attempts, **Then** local file succeeds, network fails with logged error, user sees "Labels saved locally (network unavailable)"
4. **Given** user folder doesn't exist on network share, **When** export attempts, **Then** directory is created automatically before file write
5. **Given** both writes succeed, **When** export completes, **Then** user sees "Labels saved successfully to local and network locations"

---

### User Story 3 - RFC 4180 CSV Formatting (Priority: P1)

As a **LabelView integration**, I need CSV files to properly escape special characters (commas, quotes, newlines) per RFC 4180 standard so that data parses correctly in label templates.

**Why this priority**:  Improper escaping causes label printing errors or data corruption. Standards compliance is non-negotiable. 

**Independent Test**: Can be tested by creating loads with special characters (PartID with comma, PO with quotes, Location with newline), exporting, and verifying cell escaping is correct.

**Acceptance Scenarios**:

1. **Given** PartID contains comma "PALLET-48,40", **When** CSV is written, **Then** cell is quoted:  `"PALLET-48,40"`
2. **Given** PONumber contains quotes `PO "12345"`, **When** CSV is written, **Then** quotes are doubled and cell is quoted: `"PO ""12345"""`
3. **Given** Location contains newline "Dock A\nBay 2", **When** CSV is written, **Then** cell is quoted with preserved newline: `"Dock A\nBay 2"`
4. **Given** spec value is numeric 48, **When** CSV is written, **Then** cell is unquoted: `48`
5. **Given** cell value is null/empty, **When** CSV is written, **Then** cell is blank (no quotes): ``

---

### User Story 4 - LabelView Template Validation (Priority: P2)

As a **system implementer**, I need documentation and test data to validate that LabelView templates correctly map to CSV columns so that labels print with correct data.

**Why this priority**: Integration validation prevents production issues. Priority P2 because it's testing/documentation, not core functionality.

**Independent Test**: Can be tested by generating sample CSV with all column types, importing to LabelView, creating test template, and verifying all fields map correctly.

**Acceptance Scenarios**:

1. **Given** sample CSV with 20 loads (various types/specs), **When** imported to LabelView, **Then** all rows parse without errors
2. **Given** LabelView template referencing "Width" field, **When** label prints for load with Width=48, **Then** label displays "48"
3. **Given** LabelView template referencing "IsInventoriedToVisual" field, **When** load has IsInventoriedToVisual=true, **Then** label displays "True" or custom formatted value
4. **Given** load missing optional spec (Material blank), **When** label template references Material, **Then** label displays blank or default value (no error)
5. **Given** CSV with 100 loads, **When** batch printed in LabelView, **Then** all 100 labels print correctly without manual intervention

---

### User Story 5 - Export Error Handling and Logging (Priority: P2)

As a **system administrator**, I need detailed error logging for CSV export failures so that I can diagnose issues with file permissions, network paths, or data formatting.

**Why this priority**: Export failures block workflow.  Detailed logging enables fast resolution.  Priority P2 because basic export works (P1), this is enhanced diagnostics.

**Independent Test**: Can be tested by inducing failures (remove directory write permissions, disconnect network mid-write, corrupt data), and verifying error logs contain actionable information.

**Acceptance Scenarios**:

1. **Given** local directory is write-protected, **When** export attempts, **Then** error is logged with message "Failed to write local CSV:  Access denied to {path}"
2. **Given** network share path is malformed, **When** export attempts, **Then** error is logged with message "Invalid network path: {path}"
3. **Given** load contains invalid character encoding, **When** CSV write attempts, **Then** error is logged with load ID and problematic field
4. **Given** disk is full, **When** export attempts, **Then** error is logged "Insufficient disk space for CSV export"
5. **Given** any export error, **When** logged, **Then** log includes: timestamp, username, load count, file paths attempted, exception details

---

### Edge Cases

- What happens when username contains invalid path characters (e.g., `\`)?  (Sanitize username for file path:  replace with underscore)
- What happens when CSV file is locked by another process (LabelView has it open)? (Retry with incremented filename:  DunnageData_1.csv, log warning)
- What happens when spec value is very long (>32,000 characters)? (Truncate with warning in log, CSV cell limit)
- What happens when exporting 10,000 loads in one batch? (Stream write to avoid memory issues, show progress indicator)
- What happens when spec key contains spaces or special characters? (Sanitize column header: replace spaces with underscores, remove special chars)

## Requirements

### Functional Requirements - CSV Writer Service

#### Service_DunnageCSVWriter (Enhanced from FR-038 to FR-046 in spec 004)
- **FR-001**: Service MUST call `GetAllSpecKeysAsync()` to retrieve union of all spec keys before writing CSV
- **FR-002**: CSV headers MUST be ordered:  Fixed columns first (DunnageType, PartID, Quantity, PONumber, ReceivedDate, Employee), then spec keys alphabetically
- **FR-003**:  Service MUST write to local path:  `%APPDATA%\MTM_Receiving_Application\DunnageData.csv`
- **FR-004**: Service MUST write to network path: `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv`
- **FR-005**: Service MUST create user subdirectory on network share if it doesn't exist
- **FR-006**: Service MUST sanitize username for valid file path (replace `\/: *?"<>|` with underscore)
- **FR-007**: Service MUST escape CSV cells per RFC 4180 (quote cells with comma/quote/newline, double internal quotes)
- **FR-008**: Service MUST handle null/empty spec values as blank cells (no quotes)
- **FR-009**: Service MUST log errors separately for local and network write failures
- **FR-010**:  Service MUST return `CSVWriteResult` with LocalSuccess, NetworkSuccess, ErrorMessage, LocalFilePath, NetworkFilePath

### Functional Requirements - Dynamic Column Handling

- **FR-011**: `GetAllSpecKeysAsync()` MUST query all records from `dunnage_specs` table
- **FR-012**: `GetAllSpecKeysAsync()` MUST deserialize each record's `DunnageSpecs` JSON
- **FR-013**: `GetAllSpecKeysAsync()` MUST extract all unique keys from all spec schemas
- **FR-014**: `GetAllSpecKeysAsync()` MUST return keys sorted alphabetically
- **FR-015**: For each load in export, spec values MUST be populated from load's part's `DunnageSpecValues` JSON
- **FR-016**: If spec key exists in union but not in load's part, cell MUST be blank
- **FR-017**:  Spec value data types MUST be preserved (number unquoted, string quoted if contains special chars, boolean as "True"/"False")

### Functional Requirements - File Path Management

- **FR-018**: Service MUST expand `%APPDATA%` environment variable for local path
- **FR-019**:  Service MUST replace `{Username}` placeholder with `Environment.UserName` for network path
- **FR-020**: Service MUST validate network path is reachable before attempting write (timeout:  3 seconds)
- **FR-021**: If network validation fails, MUST skip network write and log warning (don't block local write)
- **FR-022**: Service MUST overwrite existing CSV files (no append, always replace)
- **FR-023**: If file is locked, MUST attempt retry with incremented filename (DunnageData_1.csv, _2.csv, etc., max 5 attempts)

### Functional Requirements - Error Handling

- **FR-024**: Local write failure MUST return `CSVWriteResult` with LocalSuccess=false, ErrorMessage populated
- **FR-025**: Network write failure MUST NOT fail entire operation if local succeeds (return LocalSuccess=true, NetworkSuccess=false)
- **FR-026**: Both write failures MUST return LocalSuccess=false, NetworkSuccess=false, ErrorMessage with both error details
- **FR-027**: All exceptions MUST be caught, logged via ILoggingService, and wrapped in result
- **FR-028**:  Encoding errors MUST log problematic load ID and field name
- **FR-029**: Disk space errors MUST log required space vs available space

### Functional Requirements - LabelView Integration

- **FR-030**: CSV encoding MUST be UTF-8 with BOM (for Excel/LabelView compatibility)
- **FR-031**: Line endings MUST be CRLF (`\r\n`) per RFC 4180
- **FR-032**: Boolean spec values MUST serialize as "True"/"False" (LabelView string fields)
- **FR-033**: Numeric spec values MUST use invariant culture formatting (period decimal separator, no thousands separator)
- **FR-034**: Date fields MUST format as `yyyy-MM-dd HH:mm:ss` (sortable, LabelView compatible)
- **FR-035**: Empty columns MUST NOT be omitted (preserve column count across all rows for LabelView column mapping)

### Performance Requirements

- **FR-036**:  CSV write MUST handle 1,000 loads in under 5 seconds
- **FR-037**: CSV write MUST stream rows (not load all into memory) to support 10,000+ loads
- **FR-038**:  `GetAllSpecKeysAsync()` result SHOULD be cached per export operation (called once, not per load)
- **FR-039**: Network path reachability check MUST timeout after 3 seconds (don't block indefinitely)

## Success Criteria

### Measurable Outcomes

- **SC-001**:  CSV files written to both local and network paths when network is available
- **SC-002**: Local CSV always succeeds even when network fails (99.9% success rate)
- **SC-003**: CSV headers include all spec keys from union (verified with 10 types, 30 unique specs)
- **SC-004**: CSV escaping correctly handles 100% of test cases (commas, quotes, newlines, edge chars)
- **SC-005**: LabelView imports CSV without errors for 100 test loads
- **SC-006**:  Export of 1,000 loads completes in under 5 seconds
- **SC-007**: Export errors are logged with sufficient detail for diagnosis (100% of failures include actionable info)
- **SC-008**: File locking handled gracefully with retry (95% success rate on locked file scenarios)

## Non-Functional Requirements

- **NFR-001**: CSV writer MUST use `CsvHelper` library for RFC 4180 compliance
- **NFR-002**: All file I/O MUST use `using` statements for proper disposal
- **NFR-003**: Network write failures MUST NOT show error dialog (log only, inform user in status message)
- **NFR-004**: CSV file size MUST be monitored (warn if >50MB, could indicate data issue)
- **NFR-005**: All paths MUST be configurable via application settings (not hardcoded)

## Out of Scope

- ❌ CSV import functionality (export only)
- ❌ Custom CSV column ordering (fixed + alphabetical only)
- ❌ CSV compression/archival (raw CSV only)
- ❌ Email delivery of CSV files (manual retrieval only)
- ❌ LabelView template creation (assumes templates exist)
- ❌ Barcode generation in CSV (LabelView generates barcodes)

## Dependencies

- 004-services-layer (IService_MySQL_Dunnage. GetAllSpecKeysAsync, models)
- NuGet:  CsvHelper (for RFC 4180 CSV writing)
- Configuration: Local and network file paths in app settings
- Infrastructure: Network share `\\mtmanu-fs01\Expo Drive` with user write permissions
- External: LabelView label printing software

## Files to be Created/Modified

### Enhanced Files (from spec 004)
- `Services/Receiving/Service_DunnageCSVWriter. cs` - Add enhanced error handling, retry logic, streaming
- `Contracts/Services/IService_DunnageCSVWriter. cs` - Update return type to include network success flag

### New Files
- `Models/Results/CSVWriteResult.cs` - Result wrapper with LocalSuccess, NetworkSuccess, FilePaths, ErrorMessage

### Configuration
- `appsettings.json` - Add DunnageCSV section with LocalPath, NetworkPathTemplate, RetryCount, RetryDelayMs

### Documentation
- `Docs/DunnageCSVFormat.md` - CSV column specification for LabelView integration
- `Docs/DunnageLabelViewSetup.md` - Instructions for mapping CSV columns in LabelView templates

## Review & Acceptance Checklist

### Requirement Completeness
- [x] Dynamic column generation logic is fully specified (union of spec keys)
- [x] Dual-path writing is detailed (local + network with failure handling)
- [x] RFC 4180 compliance is explicit (escaping rules enumerated)
- [x] LabelView integration requirements are clear (encoding, formatting, column preservation)
- [x] Error handling and retry logic are specified

### Clarity & Unambiguity
- [x] File paths are verbatim specified (local AppData, network UNC)
- [x] Column ordering is explicit (fixed first, alphabetical specs)
- [x] Escaping rules are detailed per data type
- [x] Error messages are specified for common failure scenarios
- [x] Performance targets are measurable (1000 loads < 5s)

### Testability
- [x] Each user story has independent test approach
- [x] Success criteria are measurable (success rates, timing, error logs)
- [x] Edge cases define specific failure scenarios with expected behavior
- [x] LabelView integration can be validated with sample data

### Integration Quality
- [x] Builds on Service_DunnageCSVWriter from spec 004 (extends, not replaces)
- [x] Uses IService_MySQL_Dunnage.GetAllSpecKeysAsync from spec 004
- [x] Follows project error handling patterns (ILoggingService)
- [x] Compatible with existing LabelView infrastructure

## Clarifications

### Resolved Questions

**Q1**: Should CSV include deleted/soft-deleted loads?   
**A1**: No soft deletes in system.  Only active loads are exported.  If load is deleted, it doesn't exist. 

**Q2**: Should network write block user or happen asynchronously?  
**A2**:  Synchronous.  User waits for export completion.  Shows progress indicator for large exports.

**Q3**:  Should CSV overwrite or append to existing file?  
**A3**:  Overwrite always. LabelView reads latest export, not historical appends.

**Q4**: Should column headers be configurable or fixed?  
**A4**:  Fixed pattern (Fixed + Alphabetical specs). No configurability.  LabelView templates adapt to this pattern.

**Q5**: Should empty spec columns be omitted from CSV to reduce file size?  
**A5**:  No.  Preserve all columns for consistent LabelView column mapping.  Empty cells are small overhead.

**Q6**: Should CSV include metadata row with export date/time/user?  
**A6**: No. LabelView expects data-only CSV (headers + data rows). Metadata in filename or log only. 