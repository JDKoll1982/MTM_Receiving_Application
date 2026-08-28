<!-- 
[DOC-META-START]
- File Name: winui-input-json-build-lock.md
- Description: Recovery steps for WinUI XAML input.json build-lock failures.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-17: # WinUI input.json Build Lock
- Critical Notes: Rebuild with -nodeReuse:false -m:1 after deleting the stale input.json.
[DOC-META-END]
-->

# WinUI input.json Build Lock

- Symptom: `dotnet build MTM_Receiving_Application.slnx` can fail in WinUI XAML compilation because `obj/x64/Debug/net10.0-windows10.0.22621.0/input.json` is locked.
- Working recovery: stop `VBCSCompiler.exe` if running, delete the stale `obj/.../input.json`, then rebuild with `dotnet build .\MTM_Receiving_Application.slnx /property:GenerateFullPaths=true "/consoleloggerparameters:NoSummary;ForceNoAlign" /nodeReuse:false /m:1`.
- Result observed: solution build and tests succeeded after using the one-shot build without node reuse.
- Bash-safe command: `dotnet build MTM_Receiving_Application.slnx -p:GenerateFullPaths=true -clp:NoSummary\;ForceNoAlign -nodeReuse:false -m:1` because slash-style MSBuild switches can be mangled by bash on Windows.
