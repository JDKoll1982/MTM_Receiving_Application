---
name: Repomix Configuration Setup
description: Configures Repomix for packing your repository into an AI-friendly format with optimized settings for LLM context
argument-hint: "Specify your project needs (token limits, file types, git history, output format)"
agent: agent
---

# Repomix Configuration Setup Guide

This guide will help you configure Repomix for your project. Follow the sections below to optimize your configuration based on your specific needs.

## 🎯 Purpose

Use this guide to create or enhance your `repomix.config.json` file. Repomix is a tool that packs your repository into a single, AI-friendly file for use with Large Language Models (LLMs) and other AI tools.

---

## 📋 Configuration Schema Reference

Always include the schema reference for IDE autocomplete and validation:

```json
{
  "$schema": "https://repomix.com/schemas/latest/schema.json"
}
```

---

## 🔧 Core Configuration Sections

### 1. **Input Settings** (`input`)

Controls which files are processed and size limitations.

**Available Options:**

- `maxFileSize` (number, optional)
  - **Description**: Maximum file size in bytes.  Files larger than this are skipped.
  - **Default**: `50000000` (50MB)
  - **Use Case**: Exclude large binary files or data files
  - **Example**: `10485760` (10MB)

```json
{
  "input":  {
    "maxFileSize":  10485760
  }
}
```

---

### 2. **Output Settings** (`output`)

Controls the format, content, and features of the generated output file.

#### 2.1 Basic Output Options

**`filePath`** (string, optional)

- **Description**: Name of the output file
- **Default**: `"repomix-output.xml"` (varies by style)
- **Example**: `"output-code-bundle.xml"`

**`style`** (string, optional)

- **Description**: Format of the output file
- **Options**: `"xml"`, `"markdown"`, `"json"`, `"plain"`
- **Default**: `"xml"`
- **Use Case**:
  - `xml` - Best for Claude and most LLMs
  - `markdown` - Human-readable
  - `json` - Programmatic processing
  - `plain` - Simple text format

**`parsableStyle`** (boolean, optional)

- **Description**:  Escape output based on chosen style schema
- **Default**: `false`
- **Use Case**: Enable for better parsing, but may increase token count

**`compress`** (boolean, optional)

- **Description**: Intelligent code extraction using Tree-sitter
- **Default**: `false`
- **Use Case**: Reduce token count while preserving structure

```json
{
  "output":  {
    "filePath": "ai-context. xml",
    "style": "xml",
    "parsableStyle": false,
    "compress": false
  }
}
```

#### 2.2 Content Control Options

**`headerText`** (string, optional)

- **Description**: Custom text in the file header
- **Default**: `null`
- **Use Case**: Provide context or instructions for AI tools

**`instructionFilePath`** (string, optional)

- **Description**: Path to file with detailed custom instructions. Omit the property if not used (must be a string when present).
- **Use Case**: Reference external instruction files

**`fileSummary`** (boolean, optional)

- **Description**: Include summary section with metrics
- **Default**: `true`
- **Use Case**: Show file counts, sizes, and statistics

**`directoryStructure`** (boolean, optional)

- **Description**: Include directory tree in output
- **Default**: `true`
- **Use Case**: Help AI understand project organization

**`files`** (boolean, optional)

- **Description**: Include file contents in output
- **Default**: `true`
- **Use Case**: Set to `false` for structure/metadata only

```json
{
  "output": {
    "headerText": "Project X - Core Implementation Files",
    "instructionFilePath": ".github/prompts/repomix-instructions.md",
    "fileSummary": true,
    "directoryStructure": true,
    "files": true
  }
}
```

#### 2.3 Code Processing Options

**`removeComments`** (boolean, optional)

- **Description**: Remove comments from supported file types
- **Default**: `false`
- **Use Case**: Reduce noise and token count

**`removeEmptyLines`** (boolean, optional)

- **Description**: Remove empty lines from output
- **Default**: `false`
- **Use Case**: Further reduce token count

**`showLineNumbers`** (boolean, optional)

- **Description**: Add line numbers to each line
- **Default**: `false`
- **Use Case**: Reference specific code locations

**`truncateBase64`** (boolean, optional)

- **Description**: Truncate long base64 data strings
- **Default**: `false`
- **Use Case**: Reduce token count from embedded data

```json
{
  "output": {
    "removeComments": true,
    "removeEmptyLines": true,
    "showLineNumbers": false,
    "truncateBase64": true
  }
}
```

#### 2.4 Advanced Output Options

**`topFilesLength`** (number, optional)

- **Description**: Number of top files to display in summary
- **Default**: `5`
- **Use Case**: Set to `0` to disable summary

**`copyToClipboard`** (boolean, optional)

- **Description**: Copy output to clipboard
- **Default**: `false`

**`includeEmptyDirectories`** (boolean, optional)

- **Description**: Include empty directories in structure
- **Default**: `false`

**`includeFullDirectoryStructure`** (boolean, optional)

- **Description**: Show complete directory tree when using `include` patterns
- **Default**: `false`
- **Use Case**: Display full tree while processing only included files

**`splitOutput`** (integer, optional)

- **Description**: Split output into multiple files by max size per part
- **Minimum**: `1`
- **Example**: `1000000` (~1MB per file)
- **Use Case**: Handle large repositories

**`tokenCountTree`** (boolean|number|string, optional)

- **Description**: Display token count tree
- **Default**: `false`
- **Options**: `true`, `false`, depth number, or string
- **Use Case**: Analyze token distribution across files

```json
{
  "output": {
    "topFilesLength": 10,
    "copyToClipboard": false,
    "includeEmptyDirectories": false,
    "includeFullDirectoryStructure": false,
    "splitOutput": 2000000,
    "tokenCountTree": true
  }
}
```

#### 2.5 Git Integration (`output.git`)

**`sortByChanges`** (boolean, optional)

- **Description**: Sort files by git change count
- **Default**: `true`
- **Use Case**: Files with more changes appear at bottom

**`sortByChangesMaxCommits`** (number, optional)

- **Description**: Max commits to analyze for changes
- **Default**: `100`

**`includeDiffs`** (boolean, optional)

- **Description**: Include git diffs (work tree and staged)
- **Default**: `false`
- **Use Case**: Show pending repository changes

**`includeLogs`** (boolean, optional)

- **Description**: Include git commit history
- **Default**: `false`
- **Use Case**: Help AI understand development patterns

**`includeLogsCount`** (number, optional)

- **Description**: Number of commits to include in logs
- **Default**: `50`

```json
{
  "output": {
    "git": {
      "sortByChanges": true,
      "sortByChangesMaxCommits": 100,
      "includeDiffs": false,
      "includeLogs":  false,
      "includeLogsCount": 50
    }
  }
}
```

---

### 3. **Include Patterns** (`include`)

Specify files to include using glob patterns.

**Type**: Array of strings  
**Default**: `[]` (all files)  
**Pattern Syntax**: [fast-glob patterns](<https://github.com/mrmlnc/fast-glob>? tab=readme-ov-file#pattern-syntax)

**Examples:**

- `"**/*.js"` - All JavaScript files
- `"src/**/*"` - All files in src directory
- `"**/*.{ts,tsx}"` - All TypeScript files
- `"src/**/*.test.js"` - All test files in src

```json
{
  "include":  [
    "src/**/*.ts",
    "lib/**/*.js",
    "**/*.md"
  ]
}
```

---

### 4. **Ignore Settings** (`ignore`)

Control which files and directories to exclude.

**`useGitignore`** (boolean, optional)

- **Description**: Use patterns from `.gitignore`
- **Default**: `true`

**`useDotIgnore`** (boolean, optional)

- **Description**: Use patterns from `.ignore` file
- **Default**: `true`

**`useDefaultPatterns`** (boolean, optional)

- **Description**: Use default ignore patterns (node_modules, .git, etc.)
- **Default**: `true`

**`customPatterns`** (array of strings, optional)

- **Description**: Additional patterns to ignore
- **Default**:  `[]`
- **Pattern Syntax**: [fast-glob patterns](<https://github.com/mrmlnc/fast-glob>? tab=readme-ov-file#pattern-syntax)

**Priority Order** (highest to lowest):

1. Custom patterns
2. Ignore files (. repomixignore, .ignore, .gitignore)
3. Default patterns

```json
{
  "ignore": {
    "useGitignore": true,
    "useDotIgnore": true,
    "useDefaultPatterns":  true,
    "customPatterns": [
      "**/bin/**",
      "**/obj/**",
      "**/*. log",
      "dist/**",
      "build/**"
    ]
  }
}
```

---

### 5. **Security Settings** (`security`)

Protect sensitive information.

**`enableSecurityCheck`** (boolean, optional)

- **Description**: Perform security checks using Secretlint
- **Default**: `true`
- **Detects**: API keys, tokens, private keys, passwords

```json
{
  "security":  {
    "enableSecurityCheck": true
  }
}
```

---

### 6. **Token Count Settings** (`tokenCount`)

Configure tokenization for token counting.

**`encoding`** (string, optional)

- **Description**: Token encoding for tiktoken tokenizer
- **Default**: `"o200k_base"`
- **Options**:
  - `"o200k_base"` - GPT-4o, GPT-4o-mini
  - `"cl100k_base"` - GPT-4, GPT-3.5-turbo
  - `"p50k_base"` - Codex models
  - See [tiktoken](https://github.com/openai/tiktoken) for more

```json
{
  "tokenCount": {
    "encoding": "o200k_base"
  }
}
```

---

## 📝 Complete Example Configuration

Here's a comprehensive example combining all options:

```json
{
  "$schema": "https://repomix.com/schemas/latest/schema.json",
  "input": {
    "maxFileSize": 50000000
  },
  "output": {
    "filePath": "repomix-output.xml",
    "style": "xml",
    "parsableStyle": false,
    "compress": false,
    "headerText": "Project Context for AI Analysis",
    "fileSummary": true,
    "directoryStructure": true,
    "files": true,
    "removeComments": false,
    "removeEmptyLines": false,
    "topFilesLength": 5,
    "showLineNumbers": false,
    "truncateBase64": false,
    "copyToClipboard": false,
    "includeEmptyDirectories": false,
    "includeFullDirectoryStructure": false,
    "splitOutput": 2000000,
    "tokenCountTree":  false,
    "git": {
      "sortByChanges":  true,
      "sortByChangesMaxCommits": 100,
      "includeDiffs": false,
      "includeLogs":  false,
      "includeLogsCount": 50
    }
  },
  "include":  [],
  "ignore": {
    "useGitignore": true,
    "useDotIgnore": true,
    "useDefaultPatterns": true,
    "customPatterns": []
  },
  "security":  {
    "enableSecurityCheck": true
  },
  "tokenCount": {
    "encoding": "o200k_base"
  }
}
```

---

## 🎯 Use Case Templates

### Template 1: Minimal Token Count (Code Only)

```json
{
  "$schema": "https://repomix.com/schemas/latest/schema.json",
  "output": {
    "style": "xml",
    "compress": true,
    "removeComments": true,
    "removeEmptyLines": true,
    "truncateBase64": true,
    "fileSummary": false,
    "directoryStructure": false
  }
}
```

### Template 2: Full Context with Git History

```json
{
  "$schema": "https://repomix.com/schemas/latest/schema.json",
  "output":  {
    "style": "xml",
    "fileSummary": true,
    "directoryStructure": true,
    "showLineNumbers": true,
    "git": {
      "sortByChanges": true,
      "includeDiffs": true,
      "includeLogs": true,
      "includeLogsCount": 100
    }
  }
}
```

### Template 3: Specific File Types Only

```json
{
  "$schema": "https://repomix.com/schemas/latest/schema.json",
  "include": [
    "**/*.ts",
    "**/*.tsx",
    "**/*.js",
    "**/*.jsx"
  ],
  "ignore": {
    "customPatterns": [
      "**/*.test.*",
      "**/*.spec.*",
      "**/dist/**",
      "**/node_modules/**"
    ]
  }
}
```

### Template 4: Documentation Focus

```json
{
  "$schema": "https://repomix.com/schemas/latest/schema.json",
  "include": [
    "**/*.md",
    "**/*. mdx",
    "**/README*",
    "**/CHANGELOG*"
  ],
  "output": {
    "style": "markdown",
    "removeComments": false,
    "directoryStructure": true
  }
}
```

---

## 🚀 Quick Start Steps

1. **Initialize Configuration**

   ```bash
   repomix --init
   ```

2. **Review Your Project Needs**
   - What files should be included?
   - Should comments be removed?
   - Do you need git history?
   - What's the target LLM token limit?

3. **Customize Using This Guide**
   - Choose relevant options from each section
   - Reference the templates above
   - Test with small batches first

4. **Test Your Configuration**

   ```bash
   repomix
   ```

5. **Iterate and Refine**
   - Check the output file
   - Adjust token count settings
   - Refine include/ignore patterns

---

## 📚 Additional Resources

- **Official Documentation**: [https://repomix.com](https://repomix.com)
- **Schema Validation**: `https://repomix.com/schemas/latest/schema.json`
- **GitHub Repository**: [yamadashy/repomix](https://github.com/yamadashy/repomix)
- **Glob Pattern Syntax**: [fast-glob documentation](<https://github.com/mrmlnc/fast-glob>? tab=readme-ov-file#pattern-syntax)
- **Default Ignore Patterns**: [defaultIgnore.ts](https://github.com/yamadashy/repomix/blob/main/src/config/defaultIgnore.ts)

---

## ⚠️ Important Notes

1. **All configuration options are optional** - defaults are sensible
2. **Binary files are automatically excluded** from file contents (but listed in structure)
3. **Security checks are enabled by default** - keep them on!
4. **Multiple ignore methods work together** - . gitignore, .ignore, . repomixignore, custom patterns
5. **Include patterns are applied after ignores** - be specific if using both
6. **Git features require a git repository** - commands will skip if not in a repo
7. **Token encoding should match your target LLM** - check your AI tool's requirements

---

## 🎓 Best Practices

1. **Start Simple**: Begin with minimal config, add options as needed
2. **Use Schema Validation**: Always include `$schema` for IDE support
3. **Comment Your Config**: Use JSON5 format to add explanatory comments
4. **Test Incrementally**: Make small changes and verify output
5. **Monitor Token Counts**: Use `tokenCountTree` to identify large files
6. **Secure by Default**: Keep security checks enabled
7. **Version Control**:  Commit your config file to share with team
8. **Project-Specific**: Create different configs for different use cases

---

**Configuration validated against**:  Repomix schema version `latest` (2026-01-16)  
**Source**: [yamadashy/repomix](https://github.com/yamadashy/repomix) - configSchema.ts
