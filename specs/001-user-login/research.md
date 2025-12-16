# Phase 0: Research & Decisions

**Feature**: User Authentication & Login System  
**Date**: December 15, 2025  
**Status**: Complete

## Research Tasks Completed

All technical unknowns were resolved during specification clarification phase. No additional research required as:

1. ✅ **WinUI 3 ContentDialog patterns** - Standard WinUI 3 component, well-documented
2. ✅ **MySQL database schema design** - Defined complete schema with 3 tables and proper indexes
3. ✅ **Windows username detection** - `Environment.UserName` and `Environment.MachineName` (standard .NET APIs)
4. ✅ **Session timeout implementation** - Track UI input events, standard WinUI 3 pattern
5. ✅ **Splash screen progress reporting** - `IProgress<T>` pattern with Dispatcher marshalling
6. ✅ **MVVM patterns for authentication** - Follows existing Phase 1 Infrastructure patterns

## Key Decisions

### 1. PIN Storage Strategy
**Decision**: Store PINs in plain text VARCHAR(4)  
**Rationale**: Shop floor physical security is adequate; simplifies implementation; 4-digit numeric constraint limits attack surface  
**Alternatives Considered**: 
- Hashing (bcrypt/PBKDF2) - Rejected: Unnecessary complexity for 4-digit PINs in physically secure environment
- Encryption (AES) - Rejected: Adds key management complexity without meaningful security benefit

### 2. Session Management
**Decision**: No logout button; close application to end session; timeout closes app completely  
**Rationale**: Simpler UX and implementation; consistent behavior for all users; reduces code complexity  
**Alternatives Considered**:
- Logout button with re-authentication - Rejected: Added UI complexity, unclear benefit
- Timeout shows re-login dialog - Rejected: More complex than close and relaunch

### 3. Department Configuration
**Decision**: Database table (`departments`) with dropdown + "Other" text field option  
**Rationale**: Data consistency, flexibility for new departments, no app redeployment needed  
**Alternatives Considered**:
- Hardcoded enum - Rejected: Requires redeployment for new departments
- Free-text field - Rejected: Data inconsistency issues (typos, variations)

### 4. Workstation Detection
**Decision**: Database configuration table (`workstation_config`) queried at startup  
**Rationale**: Flexible management, central administration, no code changes for new terminals  
**Alternatives Considered**:
- Hardcoded list in Model class - Rejected: Requires redeployment
- App settings file - Rejected: Distributed configuration management challenge

### 5. Concurrent Sessions
**Decision**: Allow across workstation types (personal + shared), prevent duplicates on same type  
**Rationale**: Practical for supervisors, security adequate with timeout enforcement  
**Alternatives Considered**:
- Single session only - Rejected: Too restrictive for supervisors managing shop floor
- Unlimited concurrent - Rejected: Potential security risk, harder to audit

### 6. Database Connection Error Handling
**Decision**: 3 automatic retries (5-second delays), then manual retry dialog  
**Rationale**: Balances user experience with resilience; handles temporary network issues  
**Alternatives Considered**:
- Auto-close on failure - Rejected: Harsh UX, no user control
- Infinite retry - Rejected: Could hang application indefinitely

## Technology Stack Confirmed

| Component | Choice | Version | Justification |
|-----------|--------|---------|---------------|
| Language | C# | 12 | Project standard |
| Framework | .NET | 8.0 | Project standard |
| UI Framework | WinUI 3 | Windows App SDK 1.5+ | Project standard (Phase 1) |
| Database | MySQL | 8.0+ | Project standard |
| DI Container | Microsoft.Extensions.DI | Built-in | Project standard (Phase 1) |
| Data Access | Dapper or MySql.Data | Latest stable | Lightweight, performant |
| Testing | xUnit or MSTest | Latest | Project preference TBD |

## Best Practices Applied

### WinUI 3 ContentDialog
- Use ContentDialog for modal dialogs over splash screen
- Set XamlRoot property for proper z-ordering
- Handle primary/secondary button clicks with async/await
- Implement data binding with x:Bind for performance
- Support keyboard navigation (Tab, Enter, Escape)

### Async Database Operations
- All database queries use async/await pattern
- Never block UI thread (< 100ms rule)
- Use Task.Run for CPU-bound operations if needed
- Proper exception handling with try/catch
- Timeout configuration (10 seconds per query)

### Session Management
- Track last activity timestamp with each UI event
- Use DispatcherTimer for periodic timeout checks (every 60 seconds)
- Subscribe to PointerMoved, KeyDown, and GotFocus events
- Reset timer on any user interaction
- Log timeout events for audit trail

### Progress Reporting
- Use IProgress<(int percentage, string message)> pattern
- Marshal updates to UI thread via Dispatcher
- Update every 200-500ms for smooth animation
- Show pulsing animation during user input wait (45%)
- Proper async handling to avoid blocking

### Error Handling
- Display user-friendly error messages in ContentDialogs
- Include actionable guidance (retry, contact IT)
- Log detailed errors with stack traces
- Use IService_ErrorHandler for consistency
- Prevent error information disclosure (no SQL in UI)

## Integration Points

### Existing Phase 1 Infrastructure
- Uses `ILoggingService` for audit trail logging
- Uses `IService_ErrorHandler` for error dialog display
- Follows existing MVVM pattern (ViewModel → Model → DAO → Database)
- Integrates with `Service_OnStartup_AppLifecycle` for startup orchestration
- Uses existing database helper utilities (`Helper_Database_StoredProcedure`, `Helper_Database_Variables`)

### Startup Sequence
- Phase 1 (0-15%): Environment setup (existing)
- Phase 2 (20-40%): Database connection (existing)
- **Phase 2 (40-60%): Authentication (NEW)** ← Integration point
- Phase 3 (60-100%): MainWindow load (existing)

### MainWindow Updates
- Add user info display in top-right header
- Track user interaction events for session timeout
- No logout button (per design decision)

## Open Questions

None remaining - all ambiguities resolved during specification clarification phase (see spec.md Clarifications section for complete Q&A history).

## Phase 0 Completion Checklist

- [x] All technical unknowns identified and researched
- [x] Technology stack decisions documented
- [x] Best practices for each technology identified
- [x] Integration points with existing code confirmed
- [x] Design decisions documented with rationale
- [x] No blocking unknowns remain

**Status**: ✅ Ready to proceed to Phase 1 (Data Model & Contracts)
