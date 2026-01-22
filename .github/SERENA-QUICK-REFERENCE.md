# Serena Quick Reference for MTM Development

**Keep this open while using Serena. Updated: January 21, 2026**

## One-Line Decision: Should I Use Serena?

> Refactoring 3+ files? Finding usages? Understanding a 400-line class? **YES.** Otherwise: **NO.**

---

## Setup (First Time Only)

```bash
# 1. Install (if needed)
uvx --from git+https://github.com/oraios/serena serena --help

# 2. Create project (in MTM_Receiving_Application directory)
serena project create --language csharp --name "MTM_Receiving_Application" --index

# 3. Start MCP server (in another terminal)
uvx --from git+https://github.com/oraios/serena serena start-mcp-server --project "MTM_Receiving_Application"
```

**Result:** `.serena/` folder created with project config.

---

## Each Conversation: Activate Project

Tell Claude/AI to:
```
Activate project MTM_Receiving_Application
```

Or use CLI:
```bash
serena activate-project "MTM_Receiving_Application"
```

---

## 6 Essential Tools (and When to Use Them)

### 1. Get File Structure (No Full Read!)
```
Get symbols overview for Module_Receiving/Data/Dao_ReceivingLine.cs
```
**When:** First time exploring a file. See all methods in 2 seconds.  
**Saves:** ~95% tokens

---

### 2. Read One Method (Without Reading Entire File)
```
Find symbol "InsertReceivingLineAsync" with body in Module_Receiving/Data/
```
**When:** Need to see how one method works.  
**Saves:** ~90% tokens

---

### 3. Find All Usages (Before Breaking Changes)
```
Find referencing symbols for Service_ReceivingLine/GetLineAsync
```
**When:** About to change method signature. See all callers first.  
**Saves:** ~85% tokens

---

### 4. Replace Method Body (Precisely)
```
Replace body of Dao_ReceivingLine/InsertLineAsync with:
[copy-paste new implementation]
```
**When:** Updating a DAO or Service implementation.  
**Saves:** ~80% tokens (no line-number guessing)

---

### 5. Find Code Violations (Architecture Check)
```
Search for pattern: MessageBox\.Show\(
In: Module_**/*Views/*.cs  (should be here)
Exclude: **/ (should NOT be here)
```
**When:** Validating MVVM compliance.  
**Finds:** Direct SQL in .cs files, hardcoded connections, static DAOs

---

### 6. Rename Symbol Everywhere
```
Rename symbol ViewModel_Receiving/CurrentLineID to CurrentLoadID
```
**When:** Renaming a property/method used in multiple places.  
**Impact:** Changes View bindings, ViewModel property, all callers

---

## Common Workflows (Copy & Paste)

### Explore a New DAO
```
1. Get symbols overview for Module_Receiving/Data/Dao_ReceivingLine.cs
2. Find symbol "Dao_ReceivingLine/[MethodName]" with body
3. Understand pattern, write new method following same approach
```
**Result:** Understand entire DAO structure in <1 minute. Token savings: 90%

---

### Refactor Service Method Signature
```
1. Find referencing symbols for Service_ReceivingLine/GetLineAsync
   → See all 7 callers
2. Replace body of Service_ReceivingLine/GetLineAsync with [new implementation]
3. For each caller: update the call
```
**Result:** Safe refactoring with impact validation.

---

### Validate No MVVM Violations
```
1. Search for "Dao_" in Module_**/*Views/*.cs
   → Should find ZERO (Views don't call DAOs directly)
2. Search for "SELECT\|INSERT\|UPDATE" in Module_**/*.cs excluding StoredProcedures
   → Should find ZERO (no direct SQL)
3. Search for "static.*Dao_" in Module_**/*.cs
   → Should find ZERO (DAOs are instance-based)
```
**Result:** Architectural compliance verified.

---

## Token Savings Cheat Sheet

| Task | Standard | Serena | Savings |
|------|----------|--------|---------|
| Read 1 method from 500-line file | Read all 500 lines | `find_symbol` | 90% |
| Find all usages of method | Grep + read files | `find_referencing_symbols` | 85% |
| Refactor 5 files | Read all 5 fully | Use symbolic tools | 75% |
| Validate architecture | Read 50+ views | `search_for_pattern` | 70% |

**Total per session:** 80-90% token savings on large refactoring tasks.

---

## Troubleshooting (30 Seconds to Fix)

### "Symbol not found"
→ Use `substring_matching: true` for partial name match
```
Find symbol "Insert" with substring_matching in Module_Receiving/Data/
```

### "Tool takes 10+ seconds"
→ Re-index project
```
serena project index
```

### "Language server not responding"
→ Restart it
```
serena restart-language-server
```

---

## Anti-Patterns to AVOID

❌ **Reading entire 500-line file**
→ Use `get_symbols_overview` + targeted `find_symbol`

❌ **Changing method signature without finding usages**
→ Use `find_referencing_symbols` first

❌ **Searching with grep instead of Serena pattern search**
→ Serena is 10x faster, respects C# syntax

❌ **Using Serena for single-line edits**
→ Just edit directly, Serena is overkill

---

## Before You Commit: Validation Checklist

- [ ] Build succeeds: `dotnet build`
- [ ] Tests pass: `dotnet test`
- [ ] No violations: `search_for_pattern` for Dao_, SELECT, MessageBox
- [ ] Git diff clean: `git diff` shows only intended changes
- [ ] Documentation updated: Comments reflect changes

---

## Pro Tips

💡 **Create project memories** after first onboarding:
```
Write memory "mtm_dao_pattern"
[Paste DAO pattern example]
```
Future conversations can read it instantly.

💡 **Search for TODOs** before refactoring:
```
Search for pattern: TODO|FIXME|XXX
```

💡 **Use decision tree** when unsure:
- Single file? Standard tools.
- 3+ files? Serena.
- Refactoring? Serena.
- Architecture check? Serena pattern search.

💡 **Commit after each major change**:
```bash
git add .
git commit -m "Refactored [Service/DAO] with Serena"
```
This creates checkpoints for rollback.

---

## Resources

- **Full Docs:** `.github/instructions/serena-tools.instructions.md`
- **Official Docs:** https://oraios.github.io/serena/
- **Refactoring Summary:** `.github/SERENA-REFACTORING-SUMMARY.md`

---

## TL;DR

1. **Setup once:** `serena project create --language csharp --name "MTM_Receiving_Application" --index`
2. **Each conversation:** Activate project
3. **Use 6 tools:** overview, find_symbol, find_referencing, replace, search, rename
4. **Save 80%+ tokens** on large codebase work
5. **Validate before commit** - build, test, architecture check

**Speed & Efficiency = Serena ✨**

