@echo off
setlocal
set ScriptPath=%~dp0Generate-Workflow-Diagrams.ps1
pwsh -NoProfile -ExecutionPolicy Bypass -File "%ScriptPath%" %*
endlocal
