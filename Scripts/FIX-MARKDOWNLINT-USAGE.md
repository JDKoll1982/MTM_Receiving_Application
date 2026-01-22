# Fix-MarkdownLint.ps1 - Usage Guide

Enhanced script for fixing and validating markdown linting errors across the entire repository.

## Overview

The `Fix-MarkdownLint.ps1` script provides three operational modes:

- **fix** - Automatically fix all auto-fixable markdown errors
- **check** - Validate markdown without making changes
- **list** - Display all markdown files that would be processed

## Features

✅ **Comprehensive Linting**

- Fixes all auto-fixable markdown errors (spacing, heading styles, indentation, etc.)
- Validates against 40+ markdownlint rules
- Processes all `.md` files in the repository recursively

✅ **Smart Exclusions**

- Automatically excludes: `.git`, `node_modules`, `.vs`, `obj`, `bin`
- Customizable exclude patterns via `-ExcludePatterns`
- Preserves critical directories

✅ **Three Operational Modes**

- `fix`: Auto-fix all errors
- `check`: Validate without changes (good for CI/CD)
- `list`: Preview which files would be processed

✅ **Professional Error Handling**

- Validates prerequisites (Node.js/npx)
- Provides actionable error messages
- Returns structured objects (not just console output)
- Proper exit codes for automation

✅ **Best Practices**

- Follows PowerShell scripting guidelines
- Uses `CmdletBinding()` for proper pipeline support
- Parameter validation with `ValidateScript` and `ValidateSet`
- Structured error reporting
- Proper resource cleanup (Push-Location/Pop-Location)

---

## Installation

### Prerequisites

1. **Node.js and npm** (for markdownlint-cli)

   ```powershell
   # Check if installed
   npm --version
   npx --version
   
   # If not installed, download from https://nodejs.org/
   ```

2. **markdownlint-cli** (installed automatically via npx)

   ```powershell
   # First run will download and cache
   npx markdownlint-cli --version
   ```

### Setup

The script is ready to use as-is:

```powershell
# Make executable (if needed)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Optional: Add to PATH or create alias
Set-Alias mdfix "C:\Users\johnk\source\repos\MTM_Receiving_Application\Scripts\Fix-MarkdownLint.ps1"
```

---

## Usage Examples

### Mode 1: Fix All Errors (Default)

Automatically fix all auto-fixable markdown errors in the repository:

```powershell
# Fix all markdown in current directory and subdirectories
.\Fix-MarkdownLint.ps1

# Fix all markdown in specific directory
.\Fix-MarkdownLint.ps1 -Path "C:\Users\johnk\source\repos\MTM_Receiving_Application"

# Fix with verbose output
.\Fix-MarkdownLint.ps1 -Verbose
```

**What it fixes:**

- ✅ Trailing spaces
- ✅ Hard tabs → spaces conversion
- ✅ Heading styling (consistent # prefix spacing)
- ✅ List indentation and marker spacing
- ✅ Blank lines around headings and fences
- ✅ URL formatting
- ✅ Code fence language tags
- ✅ Multiple blank lines → single blank line
- ✅ Emphasis spacing
- ✅ Link spacing
- ✅ And 20+ other fixable rules

---

### Mode 2: Validate Only (No Changes)

Check for violations without making changes (useful for CI/CD):

```powershell
# Validate all markdown files
.\Fix-MarkdownLint.ps1 -Mode check

# Validate with verbose output
.\Fix-MarkdownLint.ps1 -Mode check -Verbose

# Validate and capture result
$result = .\Fix-MarkdownLint.ps1 -Mode check
if ($result.Success) {
    Write-Host "All markdown files are valid"
}
else {
    Write-Host "Found $($result.ErrorCount) issues"
}
```

**Use in CI/CD:**

```powershell
# GitHub Actions example
$result = .\Fix-MarkdownLint.ps1 -Mode check
if (-not $result.Success) {
    Write-Error "Markdown validation failed"
    exit 1
}
```

---

### Mode 3: List Files

Preview which markdown files would be processed:

```powershell
# List all markdown files
.\Fix-MarkdownLint.ps1 -Mode list

# List with verbose output
.\Fix-MarkdownLint.ps1 -Mode list -Verbose
```

---

## Parameter Reference

### `-Path`

**Type:** `string`  
**Default:** Current directory  
**Required:** No  
**Validates:** Must be existing directory  
**Description:** Root path for markdown file discovery

```powershell
.\Fix-MarkdownLint.ps1 -Path ".github/instructions"
```

---

### `-Mode`

**Type:** `string`  
**Default:** `fix`  
**Required:** No  
**Allowed Values:** `fix`, `check`, `list`  
**Description:** Operation mode

```powershell
# Fix all errors
.\Fix-MarkdownLint.ps1 -Mode fix

# Validate without changes
.\Fix-MarkdownLint.ps1 -Mode check

# List markdown files
.\Fix-MarkdownLint.ps1 -Mode list
```

---

### `-ExcludePatterns`

**Type:** `string[]`  
**Default:** `@('.git', 'node_modules', '.vs', 'obj', 'bin')`  
**Required:** No  
**Description:** Directory patterns to exclude from processing

```powershell
# Exclude additional directories
.\Fix-MarkdownLint.ps1 -ExcludePatterns @('docs', 'archive', 'node_modules')

# Exclude only node_modules
.\Fix-MarkdownLint.ps1 -ExcludePatterns 'node_modules'
```

---

### `-Verbose`

**Type:** `SwitchParameter`  
**Default:** $false  
**Required:** No  
**Description:** Display detailed diagnostic information

```powershell
# Run with verbose output
.\Fix-MarkdownLint.ps1 -Verbose

# Also works with PowerShell native verbose
.\Fix-MarkdownLint.ps1 -Verbose:$true
```

---

## Output and Return Values

### Success Case (Mode: fix)

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Markdown Linting: fix
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Path:     C:\Users\johnk\source\repos\MTM_Receiving_Application
  Mode:     fix
  Excludes: .git, node_modules, .vs, obj, bin
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[markdownlint output...]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ✓ All markdown files fixed successfully!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

**Returned Object:**

```powershell
Success    : True
Mode       : fix
ExitCode   : 0
Message    : All markdown files fixed successfully!
FixCount   : 42
ErrorCount : 0
```

---

### Partial Success Case (Mode: check, issues found)

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Markdown Linting: check
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Path:     ...
  Mode:     check
  Excludes: ...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

docs/example.md:1:1 MD041/first-line-h1 ...

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ⚠ Markdown validation issues found.

  Run with -Mode 'fix' to auto-fix issues:
  .\Fix-MarkdownLint.ps1 -Path '...' -Mode 'fix'
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

**Returned Object:**

```powershell
Success    : False
Mode       : check
ExitCode   : 1
Message    : Markdown linting completed with issues
FixCount   : 0
ErrorCount : 3
Errors     : @(error descriptions)
```

---

## Common Rules Fixed

| Rule | Name | What It Fixes | Example |
|------|------|--------------|---------|
| MD001 | heading-increment | Skipped heading levels | h1 → h3 |
| MD003 | heading-style | Inconsistent heading markers | # vs ## |
| MD009 | no-trailing-spaces | Trailing whitespace | `text` → `text` |
| MD010 | no-hard-tabs | Hard tabs in content | `\t` → spaces |
| MD012 | no-multiple-blanks | Multiple blank lines | 3 blank lines → 1 |
| MD018 | no-missing-space-atx | Space after # | `#heading` → `# heading` |
| MD022 | blanks-around-headings | Blank lines around headers | Missing lines → Added |
| MD030 | list-marker-space | List marker spacing | `-item` → `- item` |
| MD031 | blanks-around-fences | Blank lines around code blocks | Missing lines → Added |
| MD037 | no-space-in-emphasis | Space in emphasis | `* text *` → `*text*` |
| MD039 | no-space-in-links | Space in links | `[ link ]` → `[link]` |

---

## Integration Examples

### GitHub Actions (CI/CD)

```yaml
name: Markdown Validation

on: [pull_request, push]

jobs:
  markdown:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Validate Markdown
        run: |
          .\Scripts\Fix-MarkdownLint.ps1 -Mode check
```

---

### Git Pre-commit Hook

```bash
#!/bin/bash
# .git/hooks/pre-commit

echo "Checking markdown files..."
pwsh -Command ".\Scripts\Fix-MarkdownLint.ps1 -Mode check"

if [ $? -ne 0 ]; then
    echo "Markdown validation failed. Run: .\Scripts\Fix-MarkdownLint.ps1 -Mode fix"
    exit 1
fi
```

---

### Manual Workflow

```powershell
# 1. Check for issues
$result = .\Fix-MarkdownLint.ps1 -Mode check

# 2. If issues found, fix them
if (-not $result.Success) {
    .\Fix-MarkdownLint.ps1 -Mode fix
}

# 3. Verify fixes
$result = .\Fix-MarkdownLint.ps1 -Mode check
if ($result.Success) {
    Write-Host "All markdown files are now valid! ✓"
}
```

---

## Troubleshooting

### Issue: "npx not found"

**Cause:** Node.js or npm not installed

**Solution:**

```powershell
# Install Node.js from https://nodejs.org/
# Then verify installation
npm --version
npx --version

# Try the script again
.\Fix-MarkdownLint.ps1
```

---

### Issue: "Access Denied" on execution

**Cause:** Execution policy restricts script execution

**Solution:**

```powershell
# Check current policy
Get-ExecutionPolicy

# Allow scripts for current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Then run script
.\Fix-MarkdownLint.ps1
```

---

### Issue: Script finds no markdown files

**Cause:** Path is incorrect or exclude patterns too broad

**Solution:**

```powershell
# Verify path exists
Test-Path "C:\path\to\repo"

# List markdown files that would be processed
.\Fix-MarkdownLint.ps1 -Mode list

# Adjust exclude patterns if needed
.\Fix-MarkdownLint.ps1 -ExcludePatterns @('docs', 'archive')
```

---

### Issue: Some errors won't auto-fix

**Cause:** Some errors require manual intervention (non-fixable rules)

**Examples of non-fixable errors:**

- MD013 (line-length) - Often needs manual rewording
- MD024 (no-duplicate-heading) - Requires intentional heading changes
- MD033 (no-inline-html) - HTML must be removed manually
- MD036 (no-emphasis-as-heading) - Need to convert to real headings

**Solution:**

```powershell
# Check which errors are present
.\Fix-MarkdownLint.ps1 -Mode check -Verbose

# Manually fix non-fixable errors based on output
# Then run again to verify
```

---

## Best Practices

### 1. Regular Validation

```powershell
# Add to your development workflow
.\Fix-MarkdownLint.ps1 -Mode check  # Before committing
.\Fix-MarkdownLint.ps1 -Mode fix    # When issues found
```

### 2. CI/CD Integration

```powershell
# Validate in pull requests
$result = .\Fix-MarkdownLint.ps1 -Mode check
if (-not $result.Success) {
    exit 1  # Fail the build
}
```

### 3. Repository-Wide Cleanup

```powershell
# Fix all markdown issues
cd C:\Users\johnk\source\repos\MTM_Receiving_Application
.\Scripts\Fix-MarkdownLint.ps1 -Mode fix -Verbose

# Verify all fixed
.\Scripts\Fix-MarkdownLint.ps1 -Mode check

# Commit changes
git add *.md
git commit -m "chore: fix markdown linting errors"
```

### 4. Exclude Sensitive Directories

```powershell
# Don't process archived or third-party markdown
.\Fix-MarkdownLint.ps1 `
  -ExcludePatterns @('archive', 'third-party', 'node_modules', '.git')
```

---

## Script Improvements Over Original

| Feature | Original | Enhanced |
|---------|----------|----------|
| **Modes** | fix only | fix, check, list |
| **Error Handling** | Basic | Comprehensive |
| **Return Values** | Exit code only | Structured objects |
| **Exclude Patterns** | Hardcoded | Customizable |
| **Output Quality** | Minimal | Professional with colors |
| **Path Resolution** | Current dir | Smart root detection |
| **Documentation** | None | This guide |
| **Best Practices** | Limited | Full compliance |

---

## Summary

**The improved script:**

- ✅ Fixes all auto-fixable markdown errors across the repository
- ✅ Provides validation mode for CI/CD pipelines
- ✅ Returns structured output for automation
- ✅ Follows PowerShell best practices
- ✅ Includes comprehensive error handling
- ✅ Supports customization via parameters
- ✅ Professional console output
- ✅ Ready for production use

**Quick start:**

```powershell
# Fix all markdown issues
.\Scripts\Fix-MarkdownLint.ps1

# Verify all fixed
.\Scripts\Fix-MarkdownLint.ps1 -Mode check
```
