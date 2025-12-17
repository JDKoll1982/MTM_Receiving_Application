# Suggested Commands

**Build:**
```powershell
dotnet build
```

**Test:**
```powershell
dotnet test
```
*Note: Tests are in a separate project `MTM_Receiving_Application.Tests`.*

**Run:**
- Use Visual Studio "Start Debugging" (F5) or "Start Without Debugging" (Ctrl+F5).
- Or `dotnet run --project MTM_Receiving_Application` (might require specific launch profile for WinUI).

**Database Deployment:**
- PowerShell script available: `Database/Deploy/Deploy-Database.ps1`
