Last Updated: 2026-04-16

# Module_OutsideService Reinstatement

## What This Module Does

Module_OutsideService provides the outside-service request workflow, including the main request page, request entry, waitlist/setup/history pages, and the related add-line and part-match helper dialogs.

## How To Re-Enable It

1. Remove the `Module_OutsideService` exclusion entries from `MTM_Receiving_Application.csproj`.
2. Restore the `Module_OutsideService` `using` statements, startup call, and registration method in `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`.
3. Restore the Outside Service navigation item in `MainWindow.xaml`.
4. Restore the Outside Service route and search destination entries in `MainWindow.xaml.cs`.
5. Build the solution and confirm the module navigates and resolves through DI.

## Files Changed To Disable The Module

- `MTM_Receiving_Application.csproj`
  Region: primary exclusion `ItemGroup` entries for `Compile Remove="Module_OutsideService\**"` and `Page Remove="Module_OutsideService\**"`.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  Region: top-level `using MTM_Receiving_Application.Module_OutsideService.*` directives removed.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  Region: `AddModuleServices(...)` startup chain comment `MODULE_OUTSIDESERVICE_DISABLED` where `AddOutsideServiceModule(configuration)` was removed.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
  Region: `AddOutsideServiceModule(...)` method removed.
- `MainWindow.xaml`
  Region: `NavigationView.MenuItems` entry with `Tag="OutsideServiceMainPage"` removed.
- `MainWindow.xaml.cs`
  Region: `_navRoutes` dictionary entry for `OutsideServiceMainPage` replaced with `MODULE_OUTSIDESERVICE_DISABLED` comment.
- `MainWindow.xaml.cs`
  Region: `CreateSearchDestinations()` entry for Outside Service replaced with `MODULE_OUTSIDESERVICE_DISABLED` comment.

## Notes

- The source files under `Module_OutsideService/` were not deleted.
- The Ship/Rec Tools history feature remains enabled; only the main Outside Service module was disabled.
