---
applyTo: "**/*.ps1,**/*.psm1"
---

<!-- 
[DOC-META-START]
- File Name: powershell-scripting-ai.instructions.md
- Description: PowerShell script creation (AI-assisted) guidelines covering core structure, safe defaults, AI integration, error handling, output quality, security, and naming.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 5-8: # PowerShell Script Creation (AI-Assisted)
  - Line 9-17: ## Core Script Structure
  - Line 18-23: ## Safe Defaults and Paths
  - Line 24-56: ## AI Integration (Optional)
  - Line 57-62: ## Error Handling
  - Line 63-67: ## Output Quality
  - Line 68-72: ## Security
  - Line 73-76: ## Naming
- Critical Notes: Never hardcode tokens or secrets; read them from environment variables or secure stores.
[DOC-META-END]
-->

# PowerShell Script Creation (AI-Assisted)

Purpose: Provide a consistent approach for creating reliable .ps1 scripts with optional AI integration. Adapted from the NetNerds AI-for-PowerShell guidance.

## Core Script Structure

- Use `CmdletBinding()` and a `param()` block for all inputs.
- Validate parameters with `ValidateSet`, `ValidatePattern`, and `Mandatory` where appropriate.
- Use `Set-StrictMode -Version Latest` in utility scripts.
- Prefer returning objects (PSCustomObject) instead of formatted strings.
- Use `Write-Host` only for user-facing progress, not for data output.
- Favor small, composable functions and pipeline-friendly patterns.

## Safe Defaults and Paths

- Resolve paths relative to the script location when possible.
- If a parameter is omitted, fall back to safe defaults (e.g., outputs folder).
- Normalize paths with `Join-Path` and `GetFullPath` to avoid CWD issues.

## AI Integration (Optional)

### Recommended Setup

- Use a GitHub Personal Access Token (PAT) for authentication.
- Install the PSOpenAI module:
  - `Install-Module -Name PSOpenAI -Scope CurrentUser`
- Set environment variables for GitHub Models:
  - `OPENAI_API_KEY` = your PAT
  - `OPENAI_API_BASE` = `https://models.inference.ai.azure.com`

### Basic Call Pattern

- Use `Request-ChatCompletion` with a clear `SystemMessage` such as:
  - “You are a PowerShell expert.”
- Keep prompts short, specific, and scoped to the task.

### Structured Output

- Prefer JSON schema output for automation.
- Define a schema in PowerShell, convert to JSON, and pass it as `JsonSchema`.
- Parse with `ConvertFrom-Json` to return clean objects.

Example flow:
1. Define schema
2. Call the model with `Format = "json_schema"`
3. Parse result to a structured object

### Rate Limits

- GitHub Models has free rate limits; avoid loops that spam calls.
- Cache outputs when possible.

## Error Handling

- Validate inputs early and fail fast with clear errors.
- Wrap external calls in `try/catch` and return actionable errors.
- Avoid unhandled exceptions for user-facing scripts.

## Output Quality

- If the script emits files, ensure filenames are deterministic.
- Log generated files and summarize actions at the end.

## Security

- Never hardcode tokens or secrets in scripts.
- Read tokens from environment variables or secure stores.

## Naming

- Use approved verbs for functions (e.g., `Get-`, `Set-`, `New-`, `Invoke-`).
- Use `PascalCase` for function names.
