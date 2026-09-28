<!-- 
[DOC-META-START]
- File Name: test-coverage-workflow.md
- Description: How coverage is generated via coverlet and Scripts/Generate-TestCoverage.ps1.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-18: # Test Coverage Workflow
- Critical Notes: Coverage Gutters reads coverage/coverage.cobertura.xml.
[DOC-META-END]
-->

# Test Coverage Workflow

- Test project already uses `coverlet.collector` in `MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj`.
- Stable coverage generation is handled by `Scripts/Generate-TestCoverage.ps1`.
- The script runs `dotnet test` with `--collect:XPlat Code Coverage`, writes raw collector output under `artifacts/coverage/raw/`, and copies the newest `coverage.cobertura.xml` to `coverage/coverage.cobertura.xml`.
- Workspace settings in `.vscode/settings.json` point Coverage Gutters at `coverage/coverage.cobertura.xml` and enable `coverage-gutters.watchOnActivate`.
- VS Code task label: `test: coverage`.
