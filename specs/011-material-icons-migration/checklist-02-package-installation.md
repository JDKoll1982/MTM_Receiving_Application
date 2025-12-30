# Package Installation & Configuration Checklist: Material Icons Migration (Phase 2)

**Purpose**: Install Material.Icons.WinUI3 NuGet package and configure application-wide access
**Created**: 2025-12-29
**Feature**: [Icon Migration Plan](plan.md)
**Phase**: 2 of 9 - Package Installation & Configuration

---

## NuGet Package Installation

- [x] CHK008-01 Check latest stable version of Material.Icons.WinUI3 on NuGet.org
- [x] CHK008-02 Open terminal in project root directory
- [x] CHK008-03 Run command: `dotnet add package Material.Icons.WinUI3`
- [x] CHK008-04 Verify package installation succeeded (check output for errors)
- [x] CHK008-05 Verify package reference added to MTM_Receiving_Application.csproj
- [x] CHK008-06 Run `dotnet restore` to ensure all dependencies resolved
- [x] CHK008-07 Document installed version number in plan.md

---

## App.xaml Resource Registration

- [x] CHK009-01 Open App.xaml file
- [x] CHK009-02 Locate `<Application.Resources>` section
- [x] CHK009-03 Locate `<ResourceDictionary.MergedDictionaries>` element
- [x] CHK009-04 Add new line AFTER existing dictionaries: `<ResourceDictionary Source="ms-appx:///Material.Icons.WinUI3/Themes/Generic.xaml"/>`
- [x] CHK009-05 Verify XML syntax is correct (no missing closing tags)
- [x] CHK009-06 Save App.xaml

---

## Namespace Registration (Test Implementation)

Create test XAML page to verify package accessibility:

- [x] CHK010-01 Create test file: Views/Shared/TestMaterialIconsPage.xaml
- [x] CHK010-02 Add namespace to test page: `xmlns:materialIcons="using:Material.Icons.WinUI3"`
- [x] CHK010-03 Add test icon to page: `<materialIcons:MaterialIcon Kind="TestTube" Width="48" Height="48"/>`
- [x] CHK010-04 Create corresponding code-behind: TestMaterialIconsPage.xaml.cs (standard pattern)
- [x] CHK010-05 Build project with command: `dotnet build /p:Platform=x64 /p:Configuration=Debug`
- [x] CHK010-06 Verify build succeeds with no errors
- [x] CHK010-07 Run application and navigate to test page (if possible)
- [x] CHK010-08 Verify test icon displays correctly
- [x] CHK010-09 **MANDATORY**: Delete test file after successful verification

---

## Build Verification

- [x] CHK011-01 Clean solution: `dotnet clean`
- [x] CHK011-02 Rebuild solution: `dotnet build /p:Platform=x64 /p:Configuration=Debug`
- [x] CHK011-03 Verify zero build errors
- [x] CHK011-04 Verify zero build warnings related to Material.Icons
- [x] CHK011-05 Check bin/ folder for Material.Icons.WinUI3.dll presence
- [x] CHK011-06 Check obj/ folder for NuGet cache entries

---

## Documentation Updates

- [x] CHK012-01 Add Material.Icons.WinUI3 to project README.md dependencies section
- [x] CHK012-02 Update .github/copilot-instructions.md with Material Icons reference
- [x] CHK012-03 Create specs/011-material-icons-migration/PACKAGE_VERSION.md with installation details

---

## Constitution Compliance Review

**Required before proceeding to next phase**

### Core Principles
- [x] CONST001 Package installed via NuGet, not manual DLL reference
- [x] CONST002 App.xaml resource registration follows existing pattern
- [x] CONST003 No breaking changes to existing functionality

### Critical Constraints
- [x] CONST004 Build succeeds on x64 Debug configuration
- [x] CONST005 No new runtime dependencies added beyond Material.Icons.WinUI3

### Forbidden Practices
- [x] CONST006 No hardcoded paths to Material.Icons DLLs
- [x] CONST007 No modification of WinUI 3 theme resources (only merge)

---

## Rollback Instructions (If Issues Occur)

**Only execute if critical issues arise:**

- [ ] ROLLBACK-01 Run: `dotnet remove package Material.Icons.WinUI3`
- [ ] ROLLBACK-02 Remove `<ResourceDictionary Source="ms-appx:///Material.Icons.WinUI3/Themes/Generic.xaml"/>` from App.xaml
- [ ] ROLLBACK-03 Run: `dotnet clean`
- [ ] ROLLBACK-04 Run: `dotnet build /p:Platform=x64 /p:Configuration=Debug`
- [ ] ROLLBACK-05 Verify application builds without errors
- [ ] ROLLBACK-06 Document rollback reason in plan.md

---

## Completion Criteria

- [ ] COMPLETE Material.Icons.WinUI3 package successfully installed
- [ ] COMPLETE App.xaml updated with resource dictionary merge
- [ ] COMPLETE Test page created, verified, and **deleted**
- [ ] COMPLETE Build succeeds with zero errors
- [ ] COMPLETE Documentation updated with package details
- [ ] COMPLETE Constitution compliance verified

---

## Estimated Time

**15 minutes**

---

## Notes

- Material.Icons.WinUI3 current stable version: **Check NuGet.org**
- Package size: ~2.5 MB
- Total icon count available: **5000+ Material Design icons**
- Zero additional runtime dependencies required

---

## Next Phase

Once package installation is complete and verified, proceed to [checklist-03-icon-mapping.md](checklist-03-icon-mapping.md)
