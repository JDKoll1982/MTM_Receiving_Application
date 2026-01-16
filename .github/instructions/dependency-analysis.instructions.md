# Dependency Analysis Instructions

## Purpose

This document outlines how to perform dependency analysis to ensure architectural compliance.

## Tools

- **Visual Studio Code**: Use "Find in Files" (Ctrl+Shift+F) for quick checks.
- **Graphviz**: Use `class-dependency-graph.dot` (if maintained) to visualize.
- **Roslyn Analyzers**: (Future) Custom analyzers to enforce rules at build time.

## Manual Verification Steps

### 1. Check for ViewModel -> DAO Violations

Search for `using MTM_Receiving_Application.Data` in the `ViewModels` folder.

- **Regex**: `using.*\.Data` in `ViewModels/**/*.cs`
- **Expected Result**: 0 matches (except for `Data.Authentication` if strictly necessary, but preferably wrapped in Service).
- **Action**: If found, refactor to use a Service.

### 2. Check for Service -> Database Violations

Search for `MySqlConnection`, `MySqlCommand`, or `Helper_Database_StoredProcedure` in the `Services` folder.

- **Regex**: `(MySqlConnection|MySqlCommand|Helper_Database_StoredProcedure)` in `Services/**/*.cs`
- **Expected Result**: 0 matches (except for `Service_ErrorHandler` or specialized infrastructure services).
- **Action**: If found, move logic to a DAO.

### 3. Check for Static DAOs

Search for `public static class Dao_` in the `Data` folder.

- **Regex**: `public static class Dao_` in `Data/**/*.cs`
- **Expected Result**: 0 matches.
- **Action**: Convert to instance-based class.

## Automated Analysis (Script)

You can use a PowerShell script to scan for these patterns:

```powershell
# Check ViewModels
$vmViolations = Get-ChildItem -Path "ViewModels" -Recurse -Filter "*.cs" | Select-String "using.*\.Data"
if ($vmViolations) { Write-Host "ViewModel Violations Found!" -ForegroundColor Red; $vmViolations }

# Check Services
$serviceViolations = Get-ChildItem -Path "Services" -Recurse -Filter "*.cs" | Select-String "Helper_Database_StoredProcedure"
if ($serviceViolations) { Write-Host "Service Violations Found!" -ForegroundColor Red; $serviceViolations }
```
