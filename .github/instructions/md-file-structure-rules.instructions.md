# Markdown File Structure Rules

**Purpose:** Establish consistent markdown formatting standards for all documentation files  
**Applies To:** All `.md` files in the repository  
**Based On:** markdownlint rules and markdown best practices

---

## Core Principles

1. **Consistency** - Use the same formatting patterns throughout all documents
2. **Readability** - Optimize for human reading in both raw and rendered forms
3. **Compatibility** - Ensure files render correctly across all markdown parsers (GitHub, VS Code, etc.)
4. **Accessibility** - Structure content for screen readers and assistive technologies

---

## Rule Categories

### 1. Headers

**MD001 - Header Levels Increment**
- Header levels should only increment by one level at a time
- ? CORRECT: `# H1` ? `## H2` ? `### H3`
- ? WRONG: `# H1` ? `### H3` (skipped H2)

**MD002 - First Header Level**
- First header should be a top-level header (`# H1`)
- Document should start with `# Title`

**MD003 - Header Style Consistency**
- Use ATX-style headers (with `#` symbols) consistently
- ? CORRECT: `# Header`
- ? WRONG: Mixing ATX (`#`) with Setext (underlined) styles

**MD018 - No Space After Hash**
- Include space after hash in ATX-style headers
- ? CORRECT: `# Header`
- ? WRONG: `#Header`

**MD019 - Multiple Spaces After Hash**
- Use only one space after hash in ATX-style headers
- ? CORRECT: `# Header`
- ? WRONG: `#  Header` (two spaces)

**MD020 - No Space Inside Hashes (Closed ATX)**
- Use spaces inside closing hashes if using closed ATX style
- ? CORRECT: `# Header #`
- ? WRONG: `# Header#`

**MD021 - Multiple Spaces Inside Hashes**
- Use only one space inside closing hashes
- ? CORRECT: `# Header #`
- ? WRONG: `# Header  #`

**MD022 - Headers Surrounded by Blank Lines**
- Headers should be surrounded by blank lines
- Blank line before and after every header (except at document start)

**MD023 - Headers Start at Beginning of Line**
- Headers should start at the beginning of the line
- ? CORRECT: `# Header`
- ? WRONG: `  # Header` (indented)

**MD024 - Multiple Headers with Same Content**
- Avoid using multiple headers with the same content
- Use unique header text or add context to differentiate

**MD025 - Single Top-Level Header**
- Document should have only one top-level header (`# H1`)
- Use `## H2` and below for all other sections

**MD041 - First Line in File is Top-Level Header**
- First line of file should be a top-level header
- Document should start with `# Title`

**MD043 - Required Header Structure**
- Headers should follow required structure if defined
- Follow project-specific header organization patterns

---

### 2. Lists

**MD004 - Unordered List Style**
- Use consistent marker style for unordered lists
- Choose one: `-`, `*`, or `+` and use consistently
- Recommended: Use `-` for unordered lists

**MD005 - Inconsistent Indentation for List Items**
- List items at the same level should have the same indentation
- Use consistent spacing (typically 2 or 4 spaces)

**MD006 - List Item at Wrong Indentation Level**
- Consider starting bulleted lists at the beginning of the line
- Avoid indenting first-level list items

**MD007 - Unordered List Indentation**
- Use consistent indentation for nested list items
- Recommended: 2 spaces per level
- ? CORRECT:
  ```markdown
  - Level 1
    - Level 2
      - Level 3
  ```

**MD029 - Ordered List Item Prefix**
- Use consistent numbering style for ordered lists
- Options: sequential (`1. 2. 3.`) or all ones (`1. 1. 1.`)
- Recommended: Sequential numbering

**MD030 - Spaces After List Markers**
- Use consistent number of spaces after list markers
- Recommended: 1 space after marker
- ? CORRECT: `- Item`
- ? WRONG: `-  Item` (two spaces)

**MD032 - Lists Surrounded by Blank Lines**
- Lists should be surrounded by blank lines
- Blank line before and after every list

---

### 3. Code

**MD014 - Dollar Signs in Shell Commands**
- Do not include dollar signs (`$`) in shell command examples
- ? CORRECT: `npm install`
- ? WRONG: `$ npm install`

**MD031 - Fenced Code Blocks Surrounded by Blank Lines**
- Fenced code blocks should be surrounded by blank lines
- Blank line before and after code fences

**MD040 - Fenced Code Blocks Should Have Language**
- Fenced code blocks should specify a language
- ? CORRECT: ` ```csharp `
- ? WRONG: ` ``` ` (no language)

**MD046 - Code Block Style**
- Use consistent code block style
- Recommended: Fenced code blocks with ` ``` ` (not indented)

**MD048 - Code Fence Style**
- Use consistent code fence style
- Choose backticks (` ``` `) or tildes (`~~~`) and use consistently
- Recommended: Backticks (` ``` `)

---

### 4. Inline Formatting

**MD033 - No Inline HTML**
- Avoid using inline HTML tags
- Use markdown syntax instead of HTML when possible
- Exceptions: Complex tables, specific styling unavailable in markdown

**MD034 - No Bare URLs**
- Do not use bare URLs without link syntax
- ? CORRECT: `[Link](https://example.com)`
- ? WRONG: `https://example.com` (bare URL)

**MD036 - No Emphasis Instead of Headers**
- Do not use emphasis (bold/italic) in place of headers
- ? CORRECT: `## Section Title`
- ? WRONG: `**Section Title**`

**MD037 - No Spaces Inside Emphasis Markers**
- Spaces inside emphasis markers should not be used
- ? CORRECT: `**bold text**`
- ? WRONG: `** bold text **`

**MD038 - No Spaces Inside Code Span**
- Spaces inside code span markers should not be used
- ? CORRECT: `` `code` ``
- ? WRONG: `` ` code ` ``

**MD039 - No Spaces Inside Link Text**
- Spaces inside link text should not be used
- ? CORRECT: `[link text](url)`
- ? WRONG: `[ link text ](url)`

**MD042 - No Empty Links**
- Links should not be empty
- ? CORRECT: `[text](url)`
- ? WRONG: `[](url)` or `[text]()`

**MD044 - Proper Names Should Have Correct Capitalization**
- Use correct capitalization for proper names
- Examples: GitHub (not Github), JavaScript (not Javascript)

**MD045 - Images Should Have Alt Text**
- Images should have alternative text
- ? CORRECT: `![Description](image.png)`
- ? WRONG: `![](image.png)`

**MD047 - Files Should End with Single Newline**
- Files should end with a single newline character
- Ensures compatibility with Unix tools

**MD049 - Emphasis Style**
- Use consistent style for emphasis
- Choose asterisks (`*`) or underscores (`_`) and use consistently
- Recommended: Asterisks for bold (`**`) and italic (`*`)

**MD050 - Strong Style**
- Use consistent style for strong emphasis
- Choose asterisks (`**`) or underscores (`__`) and use consistently
- Recommended: Asterisks (`**`)

---

### 5. Line Length and Spacing

**MD009 - No Trailing Spaces**
- Lines should not have trailing spaces
- Exception: Two trailing spaces for hard line breaks (avoid if possible)

**MD010 - No Hard Tabs**
- Do not use hard tabs for indentation
- Use spaces instead (typically 2 or 4 spaces)

**MD012 - No Multiple Consecutive Blank Lines**
- Avoid multiple consecutive blank lines
- Use single blank lines for separation

**MD013 - Line Length**
- Lines should not exceed a maximum length
- Recommended: 80-120 characters per line for text
- Exception: Long URLs, code blocks, tables

**MD026 - No Trailing Punctuation in Headers**
- Headers should not end with punctuation (`.`, `,`, `:`, `;`)
- ? CORRECT: `## Section Title`
- ? WRONG: `## Section Title.`

**MD027 - No Multiple Spaces After Blockquote Symbol**
- Use single space after blockquote symbol
- ? CORRECT: `> Quote`
- ? WRONG: `>  Quote` (two spaces)

**MD028 - No Blank Line Inside Blockquote**
- Avoid blank lines inside blockquotes
- Use `>` on blank lines to continue blockquote

---

### 6. Links and References

**MD011 - Reversed Link Syntax**
- Link syntax should not be reversed
- ? CORRECT: `[text](url)`
- ? WRONG: `(url)[text]`

**MD052 - Reference Links Should Be Defined**
- Reference-style links should have definitions
- If using `[text][ref]`, must have `[ref]: url` somewhere in document

**MD053 - Link Definitions Should Be Used**
- All link definitions should be referenced
- Do not define links that are never used

---

### 7. Tables

**MD055 - Table Pipe Style**
- Use consistent table pipe style
- All rows should have leading/trailing pipes or none should
- Recommended: Use leading and trailing pipes

**MD056 - Table Column Count**
- Tables should have consistent column count
- All rows should have the same number of columns

---

## Special Characters and Escaping

### Characters That Need Escaping

In markdown, these characters have special meaning and may need escaping with backslash (`\`):

- `\` - Backslash
- `` ` `` - Backtick
- `*` - Asterisk
- `_` - Underscore
- `{}` - Curly braces
- `[]` - Square brackets
- `()` - Parentheses
- `#` - Hash/pound
- `+` - Plus sign
- `-` - Minus/hyphen (in lists)
- `.` - Period (after numbers in lists)
- `!` - Exclamation mark (for images)
- `|` - Pipe (in tables)
- `<>` - Angle brackets

### When to Escape

**DO ESCAPE when:**
- Using literal characters that would otherwise trigger markdown formatting
- Example: `Use asterisks (\*) for emphasis` to show literal asterisks

**DO NOT ESCAPE when:**
- Inside code blocks or code spans
- Inside preformatted text
- When the character is not ambiguous in context

### Common Escaping Scenarios

**Checkbox Syntax:**
- ? CORRECT: `- [ ] Unchecked` or `- [x] Checked`
- Note: Space between brackets is required
- Checkboxes work in task lists (GitHub-flavored markdown)

**Emoji and Unicode:**
- ? CORRECT: `? ? ?? ?? ?? ??` (Unicode characters work directly)
- No escaping needed for emoji/unicode in most parsers
- Use HTML entities if compatibility issues: `&check;` for ?

**Mathematical Symbols:**
- ? CORRECT: `<=` (less than or equal)
- If using in tables: Consider code spans `` `<=` `` for clarity

**Arrows:**
- ? CORRECT: `? ? ? ? ?` (Unicode arrows work directly)
- Alternative: Use HTML entities `&rarr;` for ?

---

## Document Structure Best Practices

### 1. Document Front Matter

Every documentation file should start with:

```markdown
# Document Title

**Purpose:** Brief description of document purpose  
**Last Updated:** YYYY-MM-DD  
**Status:** Draft | In Progress | Complete  
**Applies To:** Scope of application

---
```

### 2. Table of Contents

For long documents (>200 lines), include TOC after front matter:

```markdown
## Table of Contents

- [Section 1](#section-1)
- [Section 2](#section-2)
  - [Subsection 2.1](#subsection-21)
- [Section 3](#section-3)

---
```

### 3. Section Organization

Use consistent header hierarchy:

```markdown
# Top-Level Title (H1) - Only once per document

## Major Section (H2)

### Subsection (H3)

#### Detail Section (H4)

##### Minor Detail (H5) - Use sparingly

###### Rare Detail (H6) - Avoid if possible
```

### 4. Lists and Tables

**For simple lists:**
```markdown
- Item 1
- Item 2
- Item 3
```

**For complex lists with sub-items:**
```markdown
- Parent Item
  - Child item 1
  - Child item 2
    - Grandchild item
```

**For feature lists with checkboxes:**
```markdown
- [ ] Incomplete task
- [x] Completed task
```

**For tables:**
```markdown
| Column 1 | Column 2 | Column 3 |
|----------|----------|----------|
| Data 1   | Data 2   | Data 3   |
| Data 4   | Data 5   | Data 6   |
```

### 5. Code Examples

**Inline code:**
```markdown
Use the `functionName()` method to process data.
```

**Code blocks with language:**
```markdown
\`\`\`csharp
public class Example
{
    public void Method() { }
}
\`\`\`
```

### 6. Emphasis and Strong Emphasis

**Italic (emphasis):**
```markdown
Use *asterisks* for italic text.
```

**Bold (strong emphasis):**
```markdown
Use **double asterisks** for bold text.
```

**Bold Italic:**
```markdown
Use ***triple asterisks*** for bold italic text.
```

---

## VS Code Specific Considerations

### Markdown Preview Issues

**Special Characters Not Rendering:**
- Ensure file encoding is UTF-8
- Use Unicode characters directly (?, ?, etc.) - no escaping needed
- For checkboxes, use `- [ ]` with space between brackets

**Line Breaks:**
- Use blank lines for paragraph breaks
- Avoid hard line breaks with double spaces (causes issues in some parsers)
- Use `<br>` only when absolutely necessary

**Tables Not Aligning:**
- Ensure all rows have same number of pipes
- Use consistent spacing (VS Code auto-formats tables)
- Include header separator row with `---`

---

## Common Mistakes and Fixes

### Mistake 1: Checkbox Not Rendering
```markdown
? WRONG: - [] Task
? WRONG: - [X] Task (uppercase X doesn't work in all parsers)
? CORRECT: - [ ] Task (space between brackets)
? CORRECT: - [x] Task (lowercase x)
```

### Mistake 2: Special Characters Breaking Format
```markdown
? WRONG: Use * for emphasis (interpreted as list item)
? CORRECT: Use \* for emphasis (escaped)
? CORRECT: Use `*` for emphasis (in code span)
```

### Mistake 3: Links Not Working
```markdown
? WRONG: [Link Text] (url)
? WRONG: [Link Text](url with spaces)
? CORRECT: [Link Text](url)
? CORRECT: [Link Text](url%20with%20encoded%20spaces)
```

### Mistake 4: Headers Not Rendering
```markdown
? WRONG: #Header (no space after #)
? WRONG:  # Header (indented header)
? CORRECT: # Header (space after # at line start)
```

### Mistake 5: Code Blocks Not Formatted
```markdown
? WRONG:
\`\`\`
code without language
\`\`\`

? CORRECT:
\`\`\`csharp
public void Method() { }
\`\`\`
```

---

## Recommended Markdown Linting Tools

### VS Code Extensions
- **markdownlint** - Real-time linting with rule violations highlighted
- **Markdown All in One** - Formatting, TOC generation, shortcuts
- **Markdown Preview Enhanced** - Enhanced preview with more features

### Configuration File (.markdownlint.json)

Create in project root to customize rules:

```json
{
  "default": true,
  "MD013": {
    "line_length": 120,
    "code_blocks": false,
    "tables": false
  },
  "MD024": {
    "allow_different_nesting": true
  },
  "MD033": {
    "allowed_elements": ["br", "details", "summary"]
  }
}
```

---

## Project-Specific Rules for MTM_Receiving_Application

### 1. File Naming
- Use lowercase with hyphens: `feature-name.md`
- Exception: `README.md`, `RULES.md` (standard conventions)
- Instruction files: `topic.instructions.md`

### 2. Header Style
- Use ATX-style headers (`# Header`)
- Single H1 per document
- Use sentence case for headers (not Title Case)

### 3. Code Language Tags
- Always specify language: `csharp`, `sql`, `xml`, `json`, `markdown`
- Use `text` for plain text output
- Use `bash` or `powershell` for shell commands

### 4. Tables
- Always include header row
- Use alignment for readability (VS Code auto-formats)
- Include leading and trailing pipes

### 5. Lists
- Use `-` for unordered lists (consistent with project)
- Use sequential numbering for ordered lists
- Use `- [ ]` for task lists/checkboxes

### 6. Special Characters
- Use Unicode directly: ? ? ?? ?? ?? ?? ? ?
- No need to escape emoji/unicode in UTF-8 encoded files
- Escape markdown special chars when showing literal syntax

---

## Validation Checklist

Before committing markdown files:

- [ ] Run markdownlint in VS Code (should show no errors)
- [ ] Preview in VS Code markdown preview (Ctrl+Shift+V)
- [ ] Verify tables render correctly
- [ ] Check all links are valid
- [ ] Ensure code blocks have language specified
- [ ] Verify checkboxes render (if used)
- [ ] Check for trailing whitespace
- [ ] Ensure file ends with single newline
- [ ] Verify Unicode characters display correctly

---

## Resources

- [CommonMark Spec](https://spec.commonmark.org/) - Baseline markdown specification
- [GitHub Flavored Markdown](https://github.github.com/gfm/) - GitHub extensions to CommonMark
- [markdownlint Rules](https://github.com/DavidAnson/markdownlint/blob/main/doc/Rules.md) - Detailed rule documentation
- [Markdown Guide](https://www.markdownguide.org/) - Comprehensive markdown reference

---

**End of Markdown File Structure Rules**

