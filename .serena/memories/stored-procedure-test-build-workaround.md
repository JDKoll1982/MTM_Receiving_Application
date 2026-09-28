<!-- 
[DOC-META-START]
- File Name: stored-procedure-test-build-workaround.md
- Description: Workaround for dotnet test output-lock failures while the app is running.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-16: # Stored Procedure Test Build Workaround
- Critical Notes: Use isolated BaseOutputPath to avoid stopping the running app.
[DOC-META-END]
-->

# Stored Procedure Test Build Workaround

- When the WinUI app or VS debugger is running, `dotnet test` can fail because `MTM_Receiving_Application.dll` / `.exe` in the default output folder is locked.
- Working validation workaround for test batches: run `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~StoredProcedureInitialCoverageIntegrationTests" -p:BaseOutputPath=$PWD/artifacts/sp-test-build/` from the repo root.
- This isolated output path avoids stopping the running app while validating new stored-procedure integration tests.
