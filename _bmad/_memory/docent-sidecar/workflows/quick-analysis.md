---
workflow: quick-analysis
command: QA
description: Fast module overview (1-2 minutes)
version: 1.0.0
---

# Quick Analysis Workflow

**Command:** QA (Quick Analysis)
**Purpose:** Generate fast module overview without deep-dive analysis
**Target Time:** 1-2 minutes

---

## Workflow Execution Steps

### Step 1: Initialize Quick Scan

**Prompt user:**

```
⚡ Docent - Quick Analysis

Module to analyze: [await user input]
Output: Console display (no file written unless requested)
```

---

### Step 2: Rapid Component Count

**Scan directory structure for file counts only:**

```bash
# Don't parse file contents - just count
Views: Count *.xaml files
ViewModels: Count *ViewModel.cs files  
Services: Count files in Services/ directory
DAOs: Count Dao_*.cs files
Models: Count files in Models/ directory
```

**Status:**

```
Scanning {ModuleName}...
✓ Component inventory (file count only)
```

---

### Step 3: Generate Quick Summary

**Output format:**

```markdown
# {ModuleName} - Quick Overview

**Analysis Date:** {CurrentDate}
**Analysis Type:** Quick Scan (file counts only)

## Component Summary

| Component Type | Count |
|----------------|-------|
| Views | {count} |
| ViewModels | {count} |
| Services | {count} |
| DAOs | {count} |
| Models | {count} |
| Converters | {count} |
| Helpers | {count} |

**Total C# Files:** {count}
**Total XAML Files:** {count}

## Module Structure

```

{ModuleName}/
├── Views/            ({count} files)
├── ViewModels/       ({count} files)
├── Services/         ({count} files)
├── Data/             ({count} files)
├── Models/           ({count} files)
└── Contracts/        ({count} files)

```

## Purpose Assessment

Based on file names and structure, {ModuleName} appears to handle:
- {Inferred purpose from directory names and file counts}

## Recommended Next Steps

- **Full Analysis:** Run AM command for complete 7-section documentation
- **View Focus:** Use UV command to analyze specific View/ViewModel pairs
- **Database Focus:** Use DS command for database schema deep-dive

---

**Quick scan complete.** Total files: {count}

For comprehensive analysis with workflows, patterns, and database mapping, run:
`AM {ModuleName}`
```

---

### Step 4: Display Results

**Present to console immediately**

- No file writing (unless user requests save)
- Fast turnaround for rapid context gathering

---

**Workflow Version:** 1.0.0  
**Created:** 2026-01-08  
**Status:** Production Ready
