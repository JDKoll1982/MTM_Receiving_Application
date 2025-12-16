# Test Project Setup for MTM Receiving Application

## Overview

Due to WinUI 3 architectural constraints, unit tests cannot be included directly in the main application project. This document explains the test project structure and how to set it up.

## Problem

WinUI 3 projects (`net8.0-windows10.0.19041.0`) have the following limitations:
- XAML Compiler interference with test frameworks
- Multiple entry point conflicts (WinUI app + test runner)
- Package compatibility issues between WinUI and xUnit/Moq

## Solution

Tests are organized in the `/Tests` folder within the main project, but should be moved to a separate test project when needed.

---

## Manual Test Files (Current Implementation)

The `/Tests` folder currently contains manual verification test files that can be executed from debug console or temporary UI buttons:

### Phase1_Manual_Tests.cs
**Purpose**: Verifies database connectivity and DAO operations from Phase 1 Infrastructure

**Tests Included**:
1. `Test_InsertReceivingLine_ValidData()` - Verifies successful database insert
   - SUCCESS CRITERIA: `Model_Dao_Result.Success = true`, `AffectedRows = 1`
   
2. `Test_InsertReceivingLine_DatabaseUnavailable()` - Verifies error handling when database is down
   - SUCCESS CRITERIA: `Model_Dao_Result.Success = false` with descriptive `ErrorMessage`
   - NOTE: Requires manual setup (stop MySQL service before running)

**How to Run**:
```csharp
// From debug console, UI button, or startup event
await Phase1_Manual_Tests.RunAllTests();

// Or run individual tests
await Phase1_Manual_Tests.Test_InsertReceivingLine_ValidData();
```

### Phase5_Model_Verification.cs
**Purpose**: Verifies model property calculations and default values

**Tests Included**:
1. `Test_ReceivingLine_LabelText()` - Verifies `LabelText` property formats as "{LabelNumber} / {TotalLabels}"
   - Test Cases: "3 / 5", "1 / 1", "10 / 10"
   
2. `Test_Model_DefaultValues()` - Verifies default values for all three label models
   - `Model_ReceivingLine`: Date (DateTime.Now), VendorName ("Unknown"), LabelNumber (1), TotalLabels (1)
   - `Model_DunnageLine`: Date (DateTime.Now), VendorName ("Unknown"), LabelNumber (1)
   - `Model_RoutingLabel`: Date (DateTime.Now), LabelNumber (1)

**How to Run**:
```csharp
// From debug console, UI button, or startup event
Phase5_Model_Verification.RunAllTests();

// Or run individual tests
Phase5_Model_Verification.Test_ReceivingLine_LabelText();
Phase5_Model_Verification.Test_Model_DefaultValues();
```

**Note About Future Migration**:
These manual test files are temporary. Once a separate xUnit test project is created (see below), these tests should be migrated to proper unit tests with xUnit test framework.

## Recommended Test Project Structure

```
Solution Root/
├── MTM_Receiving_Application/          # Main WinUI project
│   ├── Services/
│   ├── Models/
│   ├── Tests/                          # Manual test files only
│   │   ├── Phase1_Manual_Tests.cs
│   │   └── Phase5_Model_Verification.cs
│   └── MTM_Receiving_Application.csproj
│
└── MTM_Receiving_Application.Tests/    # Separate test project
    ├── Unit/
    │   ├── AuthenticationServiceTests.cs
    │   └── SessionManagerTests.cs
    ├── Integration/
    └── MTM_Receiving_Application.Tests.csproj
```

## Creating the Test Project

### Step 1: Create Test Project in Separate Directory

```powershell
# From solution root
cd ..
dotnet new xunit -n MTM_Receiving_Application.Tests
cd MTM_Receiving_Application.Tests
```

### Step 2: Configure Test Project (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="MySql.Data" Version="9.4.0" />
  </ItemGroup>

  <!-- Reference main project DLL, not project itself -->
  <ItemGroup>
    <Reference Include="MTM_Receiving_Application">
      <HintPath>../MTM_Receiving_Application/bin/Debug/net8.0-windows10.0.19041.0/win-x64/MTM_Receiving_Application.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

### Step 3: Build Main Project First

```powershell
cd ../MTM_Receiving_Application
dotnet build --configuration Debug --runtime win-x64
```

### Step 4: Build and Run Tests

```powershell
cd ../MTM_Receiving_Application.Tests
dotnet test
```

## Test Files Included

### Unit Tests

**AuthenticationServiceTests.cs**:
- Windows username authentication tests
- PIN authentication tests
- PIN validation (format, uniqueness)
- Workstation detection tests

**SessionManagerTests.cs**:
- Session creation with timeout configuration
- Activity tracking tests
- Timeout detection logic
- Session end and cleanup tests

## Running Tests

### Visual Studio
1. Build main application first
2. Open Test Explorer
3. Run all tests or specific test classes

### VS Code
1. Install .NET Core Test Explorer extension
2. Build main application
3. Click "Run All Tests" in Test Explorer

### Command Line
```powershell
# Build main app first
dotnet build MTM_Receiving_Application.csproj --runtime win-x64

# Run tests
dotnet test MTM_Receiving_Application.Tests.csproj
```

## Test Coverage

### Phase 2 (T078-T082) - Unit Tests
- ✅ AuthenticationServiceTests created
- ✅ SessionManagerTests created  
- ⚠️ Blocked by WinUI project structure
- ✅ Tests ready to run in separate project

### Phase 3 (T098-T102) - Integration Tests
- ⏸️ Not yet implemented
- Requires same separate project setup

## Known Limitations

1. **No In-Project Testing**: Cannot run xUnit tests directly in WinUI project
2. **DLL Reference Required**: Test project must reference compiled DLL, not project
3. **Build Order**: Main app must build before tests
4. **DispatcherQueue Mocking**: UI thread operations difficult to unit test

## Future Improvements

1. Move test project to solution root level
2. Add CI/CD pipeline integration
3. Add code coverage reporting
4. Create integration test harness
5. Add mocking for DispatcherQueue in session tests

## References

- [WinUI 3 Testing Best Practices](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/test-winui3)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
