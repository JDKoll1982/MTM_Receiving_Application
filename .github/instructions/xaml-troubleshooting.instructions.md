---
description: Guidelines for troubleshooting XAML compilation errors in WinUI 3 applications
applyTo: '**/*.xaml, **/*.xaml.cs'
---

# XAML Troubleshooting Guide

## The Silent XAML Compiler Problem

### Symptom

The build fails with a cryptic error:

```
error MSB3073: The command "XamlCompiler.exe" exited with code 1
```

No additional details are provided about what's actually wrong with your XAML.

### Why This Happens

The Windows App SDK XAML compiler (`XamlCompiler.exe`) is a .NET Framework 4.7.2 tool that sometimes fails silently when run through the `dotnet` CLI, especially with .NET 8+ SDK. It crashes without outputting error details to the console.

---

## Solution: Use Visual Studio Build Tool

### Prerequisites

- Visual Studio 2022 installed (Community edition is free)
- Includes `.NET Desktop Development` workload
- Includes `Windows App SDK` components

### The Magic Command

Run this in PowerShell from your project directory:

```powershell
# Find Visual Studio installation
$vsPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath

# Build with Visual Studio's build system
& "$vsPath\Common7\IDE\devenv.com" MTM_Receiving_Application.slnx /Rebuild "Debug|x64"
```

### What You'll Get

Instead of:

```
❌ error MSB3073: XamlCompiler.exe exited with code 1
```

You'll see:

```
✅ LoadEntryView.xaml(13,20): error WMC1110: Invalid binding path 'ViewModel.SelectedPartInfo'
   Property 'SelectedPartInfo' not found on type 'LoadEntryViewModel'
```

---

## Common XAML Binding Errors

### 1. Missing StepData in Binding Path

❌ **Wrong:**

```xml
<TextBox Text="{x:Bind ViewModel.NumberOfLoads, Mode=TwoWay}" />
<TextBlock Text="{x:Bind ViewModel.SelectedPartInfo, Mode=OneWay}" />
```

✅ **Correct:**

```xml
<TextBox Text="{x:Bind ViewModel.StepData.NumberOfLoads, Mode=TwoWay}" />
<TextBlock Text="{x:Bind ViewModel.StepData.SelectedPartInfo, Mode=OneWay}" />
```

**Why:** ViewModels that inherit from `BaseStepViewModel<T>` store their data in a `StepData` property.

### 2. Property Name Casing Mismatch

❌ **Wrong:**

```xml
<TextBox Text="{x:Bind ViewModel.StepData.PoNumber}" />
```

✅ **Correct:**

```xml
<TextBox Text="{x:Bind ViewModel.StepData.PONumber}" />
```

**Why:** C# property names are case-sensitive. Check the actual property name in the ViewModel or StepData class.

### 3. Binding to Protected Methods

❌ **Wrong (in code-behind):**

```csharp
private void OnLoaded(object sender, RoutedEventArgs e)
{
    ViewModel.OnNavigatedToAsync(); // ❌ 'OnNavigatedToAsync' is protected!
}
```

✅ **Correct:**

```csharp
// Don't call OnNavigatedToAsync manually - it's called automatically by BaseStepViewModel
private void OnLoaded(object sender, RoutedEventArgs e)
{
    // Leave empty or add only UI-specific initialization
}
```

**Why:** Protected lifecycle methods like `OnNavigatedToAsync()` are called automatically by the workflow service.

### 4. Wrong Collection Property Name

❌ **Wrong:**

```xml
<ComboBox ItemsSource="{x:Bind ViewModel.StepData.PackageTypes}" />
```

✅ **Correct:**

```xml
<ComboBox ItemsSource="{x:Bind ViewModel.StepData.AvailablePackageTypes}" />
```

**Why:** Always verify the exact property name in the StepData class.

---

## Troubleshooting Workflow

### Step 1: Quick VS Code Check

1. Look for red squiggles in XAML files
2. Hover over binding expressions to see IntelliSense warnings
3. Check the Problems panel (Ctrl+Shift+M)

**Note:** VS Code IntelliSense for XAML is not always reliable. If no errors show but build fails, proceed to Step 2.

### Step 2: Build with Visual Studio CLI

```powershell
# Get detailed XAML errors
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
& "$vs\Common7\IDE\devenv.com" MTM_Receiving_Application.slnx /Rebuild "Debug|x64"
```

### Step 3: Read the Error Output

Look for lines like:

```
File.xaml(LINE,COL): XamlCompiler error WMCXXXX: [Description]
```

This tells you:

- **File:** Which XAML file has the problem
- **LINE,COL:** Exact location in the file
- **Error Code:** Type of error (WMC1110 = property not found, etc.)
- **Description:** What's actually wrong

### Step 4: Fix the Binding

1. Open the XAML file mentioned in the error
2. Go to the line number specified
3. Check the binding path against the ViewModel/StepData class
4. Correct the property name or add missing `StepData.` prefix

### Step 5: Verify the Fix

```powershell
# Clean build to ensure all XAML is recompiled
dotnet clean
dotnet build /p:Platform=x64
```

---

## Environment Setup for XAML Debugging

### Required .NET SDK Version

This project uses **.NET 8 SDK** due to compatibility with Windows App SDK 1.8.

Check your SDK version:

```powershell
dotnet --version
```

If you see 10.x or higher, create a `global.json` file:

```json
{
  "sdk": {
    "version": "8.0.416",
    "rollForward": "latestMinor"
  }
}
```

### Install .NET 8 SDK

```powershell
winget install Microsoft.DotNet.SDK.8
```

---

## Quick Reference Commands

### Full Clean and Rebuild

```powershell
# Kill any stuck processes
Get-Process | Where-Object { $_.ProcessName -match 'XamlCompiler|MSBuild' } | Stop-Process -Force -ErrorAction SilentlyContinue

# Clean everything
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue
dotnet clean

# Restore and build
dotnet restore
dotnet build /p:Platform=x64
```

### Build and Show Only Errors

```powershell
dotnet build /p:Platform=x64 2>&1 | Select-String "error"
```

### Visual Studio Rebuild with Filtered Output

```powershell
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
& "$vs\Common7\IDE\devenv.com" MTM_Receiving_Application.slnx /Rebuild "Debug|x64" 2>&1 | Select-String "error|warning" | Select-Object -Last 20
```

---

## Preventing XAML Errors

### 1. Use Strong Typing

Always use `x:Bind` (compile-time binding) instead of `Binding` (runtime binding):

✅ **Good:** Compile-time error if property doesn't exist

```xml
<TextBox Text="{x:Bind ViewModel.StepData.MyProperty, Mode=TwoWay}" />
```

❌ **Bad:** No compile error, fails at runtime

```xml
<TextBox Text="{Binding MyProperty, Mode=TwoWay}" />
```

### 2. Verify ViewModel Structure

Before creating XAML bindings, check the ViewModel:

```csharp
// If ViewModel inherits from BaseStepViewModel<T>
public partial class MyViewModel : BaseStepViewModel<MyStepData>
{
    // Properties are in StepData, not directly on ViewModel
    // Binding: ViewModel.StepData.PropertyName
}

// If ViewModel inherits from BaseViewModel
public partial class MyViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _myProperty = string.Empty;
    
    // Binding: ViewModel.MyProperty (no StepData)
}
```

### 3. Use Find-in-Files for Property Names

Before binding to a property, verify it exists:

```powershell
# In VS Code: Ctrl+Shift+F
# Search for: "public.*MyPropertyName"
```

### 4. Enable IntelliSense in XAML

In VS Code, install:

- **XAML Styler** extension
- **C# Dev Kit** extension

These provide basic XAML IntelliSense, though not as robust as Visual Studio.

---

## Understanding XAML Compiler Exit Codes

| Exit Code | Meaning | Action |
|-----------|---------|--------|
| 0 | Success | Build succeeded |
| 1 | XAML error | Use Visual Studio build to see details |
| -1 | Crash/Exception | Check for file locks, restart VS Code |

---

## Common File Lock Issues

### Symptom

```
error: The process cannot access the file 'File.g.cs' because it is being used by another process
```

### Solution

```powershell
# Kill all related processes
Get-Process | Where-Object { $_.ProcessName -match 'XamlCompiler|MSBuild|devenv|dotnet' } | Stop-Process -Force -ErrorAction SilentlyContinue

# Wait a moment
Start-Sleep -Seconds 2

# Clean and rebuild
dotnet clean
dotnet build /p:Platform=x64
```

---

## When All Else Fails

### Nuclear Option: Complete Reset

```powershell
# 1. Close VS Code
# 2. Kill processes
Get-Process | Where-Object { $_.ProcessName -match 'Code|XamlCompiler|MSBuild|dotnet' } | Stop-Process -Force -ErrorAction SilentlyContinue

# 3. Delete all build artifacts
Remove-Item -Recurse -Force bin, obj, .vs -ErrorAction SilentlyContinue

# 4. Clear NuGet cache (optional, if package issues suspected)
dotnet nuget locals all --clear

# 5. Restore and build fresh
dotnet restore
dotnet build /p:Platform=x64
```

---

## Getting Help

If you're still stuck after trying these steps:

1. **Copy the full error output** from the Visual Studio build command
2. **Note which XAML file and line number** the error references
3. **Check the ViewModel/StepData class** to see what properties actually exist
4. **Compare with working XAML files** in the same project

The error messages from Visual Studio build are extremely precise - they tell you exactly what property is missing and what type it's looking for.
