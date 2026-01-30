# Module_Receiving Specifications - Navigation Index

**Version:** 2.0 (Reorganized)  
**Last Updated:** 2026-01-25  
**Purpose:** Central navigation guide for AI agents to quickly locate relevant specifications

---

## 📋 Quick Reference

This index provides a structured navigation system for all Module_Receiving specifications. Each section is categorized by purpose and includes direct links to detailed documentation.

---

## 🎯 Getting Started

**New to Module_Receiving?** Start here:

1. **[Implementation Blueprint](./03-Implementation-Blueprint/index.md)** ⭐ **START HERE** for from-scratch rebuild
   - [Naming Conventions Extended](./03-Implementation-Blueprint/naming-conventions-extended.md) - 5-part method naming
   - [File Structure](./03-Implementation-Blueprint/file-structure.md) - 228 preset file names
   - [Implementation Summary](./03-Implementation-Blueprint/IMPLEMENTATION-SUMMARY.md) - Quick reference
2. **[Purpose and Overview](./00-Core/purpose-and-overview.md)** - Understand what Module_Receiving does and why
3. **[Data Flow](./00-Core/data-flow.md)** - See how data moves through the system
4. **[Workflow Modes Overview](#workflow-modes)** - Choose the right mode specification

---

## 📁 Directory Structure

```
specs/Module_Receiving/
├── index.md (this file)
├── 00-Core/
│   ├── purpose-and-overview.md
│   └── data-flow.md
├── 01-Business-Rules/
│   ├── load-number-dynamics.md
│   ├── load-composition-rules.md
│   ├── part-number-dynamics.md
│   ├── po-number-dynamics.md
│   ├── receiving-location-dynamics.md
│   ├── default-part-types.md
│   ├── quality-hold.md
│   ├── bulk-copy-operations.md
│   ├── non-po-receiving.md
│   └── workflow-navigation-and-in-step-editing.md
├── 02-Workflow-Modes/
│   ├── 001-wizardmode-specification.md
│   ├── 002-editmode-specification.md
│   ├── 003-manual-mode-specification.md
│   └── 004-hub-orchestration-specification.md
├── 03-Implementation-Blueprint/ ⭐ NEW
│   ├── index.md
│   ├── naming-conventions-extended.md (5-part method naming)
│   ├── file-structure.md (228 preset file names)
│   └── IMPLEMENTATION-SUMMARY.md
└── 99-Archive/
    ├── Module_Receiving_SpecSheet.md (original)
    └── README.md (original)
```

---

## 🎨 Navigation by Category

### Core Concepts

**Foundational understanding of Module_Receiving**

| Document | Purpose | Key Topics |
|----------|---------|------------|
| [Purpose and Overview](./00-Core/purpose-and-overview.md) | System overview and core functionality | Wizard/Manual/Edit modes, User personas, Integration points |
| [Data Flow](./00-Core/data-flow.md) | Complete data architecture | Transaction creation, Validation, Persistence, Edit workflow |

**When to use:**
- New to the project → Start with Purpose and Overview
- Understanding system architecture → Read Data Flow
- Planning integrations → Review both documents

---

### Business Rules

**Detailed rules governing data entry and validation**

| Document | Purpose | Key Topics |
|----------|---------|------------|
| [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md) | How loads are split and calculated | Auto-calculate behavior, Manual override, Recalculation triggers |
| [Load Composition Rules](./01-Business-Rules/load-composition-rules.md) | Pieces per load calculations | Equal pieces, Uneven division, Diameter-based distribution |
| [Part Number Dynamics](./01-Business-Rules/part-number-dynamics.md) | Part number format and validation | Prefixes (MMC, MMF, MMCCS, etc.), Format validation, ERP integration, Auto-padding |
| [P.O. Number Dynamics](./01-Business-Rules/po-number-dynamics.md) | Purchase order number rules | Standard format (PO-XXXXXX), Auto-standardization, Validation |
| [Receiving Location Dynamics](./01-Business-Rules/receiving-location-dynamics.md) | Warehouse location logic | Auto-populate, Session overrides, Settings integration |
| [Default Part Types](./01-Business-Rules/default-part-types.md) | Part categorization | 10 part types, Prefix-based assignment, Settings configuration |
| [Quality Hold](./01-Business-Rules/quality-hold.md) | Quality inspection procedures | Pre-receive workflow, Delivery procedures, System prompts |
| [Bulk Copy Operations](./01-Business-Rules/bulk-copy-operations.md) | Copy to empty cells only | Copy all fields, Copy specific field, Copy source selection, Clear auto-filled data |
| [Non-PO Receiving](./01-Business-Rules/non-po-receiving.md) | Receiving without Purchase Orders | Non-PO mode activation, Unrestricted part search, Non-PO validation |
| [Workflow Navigation & In-Step Editing](./01-Business-Rules/workflow-navigation-and-in-step-editing.md) | Edit from Review, Backward navigation | Edit Mode, Return to Review, Load count changes in Edit Mode |

**When to use:**
- Implementing validation logic → Reference specific rule document
- Understanding calculations → Load Number Dynamics or Load Composition Rules
- Configuring part defaults → Default Part Types, Receiving Location Dynamics
- Handling special procedures → Quality Hold, Non-PO Receiving
- Implementing bulk operations → Bulk Copy Operations
- Building navigation flow → Workflow Navigation & In-Step Editing

---

### Workflow Modes

**Complete specifications for each user-facing workflow**

| Document | Purpose | User Persona | Key Features |
|----------|---------|--------------|--------------|
| [Hub Orchestration](./02-Workflow-Modes/004-hub-orchestration-specification.md) | Mode selection and navigation | All users | Mode selection UI, Session management, Progress indication |
| [Guided Mode (Wizard)](./02-Workflow-Modes/001-wizardmode-specification.md) | 3-step wizard workflow | Standard clerks, new users | Step-by-step guidance, Validation feedback, Bulk copy, Non-PO support, In-step editing |
| [Manual Entry Mode](./02-Workflow-Modes/003-manual-mode-specification.md) | Spreadsheet-style bulk entry | Power users, high volume | Grid UI, Keyboard shortcuts, Bulk operations, Virtual scrolling |
| [Edit Mode](./02-Workflow-Modes/002-editmode-specification.md) | Historical data modification | Supervisors, auditors | Transaction search, Audit trail, Re-export |

**When to use:**
- Building mode selection UI → Hub Orchestration
- Implementing Wizard workflow → Guided Mode (001-wizardmode-specification.md)
- Building grid interface → Manual Entry Mode
- Implementing historical edits → Edit Mode

**Cross-Reference to Business Rules:**
- Bulk Copy in Guided Mode Step 2 → [Bulk Copy Operations](./01-Business-Rules/bulk-copy-operations.md)
- Non-PO checkbox in Guided Mode Step 1 → [Non-PO Receiving](./01-Business-Rules/non-po-receiving.md)
- Edit buttons on Guided Mode Step 3 → [Workflow Navigation & In-Step Editing](./01-Business-Rules/workflow-navigation-and-in-step-editing.md)

---

## 🔍 Navigation by Use Case

### Use Case: Implementing Data Validation

**Relevant Documents:**
1. [P.O. Number Dynamics](./01-Business-Rules/po-number-dynamics.md) - PO format validation
2. [Part Number Dynamics](./01-Business-Rules/part-number-dynamics.md) - Part number validation
3. [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md) - Load count validation
4. [Load Composition Rules](./01-Business-Rules/load-composition-rules.md) - Weight distribution validation

**Implementation Order:**
1. Read all four documents
2. Build shared validation utility for PO numbers
3. Implement part number format validation
4. Add load number and composition validators
5. Integrate into ViewModels/Services

---

### Use Case: Building Guided Mode UI

**Relevant Documents:**
1. [Hub Orchestration](./02-Workflow-Modes/004-hub-orchestration-specification.md) - Mode selection and navigation
2. Archive: `Module_Receiving_SpecSheet.md` (contains original Wizard specifications)
3. [Data Flow](./00-Core/data-flow.md) - Step transitions and data persistence
4. All Business Rules documents - Validation requirements per step

**Implementation Order:**
1. Build Hub with mode selection
2. Implement Step 1 (PO/Part entry) with validation
3. Implement Step 2 (Load details) with bulk copy
4. Implement Step 3 (Review/Save)
5. Integrate session management

---

### Use Case: Implementing Auto-Calculate Logic

**Relevant Documents:**
1. [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md) - Primary auto-calculate rules
2. [Load Composition Rules](./01-Business-Rules/load-composition-rules.md) - Pieces-per-load calculations
3. [Data Flow](./00-Core/data-flow.md) - When calculations occur in workflow

**Implementation Order:**
1. Read Load Number Dynamics thoroughly
2. Understand one-time calculation trigger
3. Implement recalculation confirmation logic
4. Add uneven division handling (Load Composition Rules)
5. Test edge cases (uneven division, manual overrides)

---

### Use Case: ERP Integration (Infor Visual)

**Relevant Documents:**
1. [Part Number Dynamics](./01-Business-Rules/part-number-dynamics.md) - Part data retrieval
2. [Receiving Location Dynamics](./01-Business-Rules/receiving-location-dynamics.md) - Location auto-pull
3. [Data Flow](./00-Core/data-flow.md) - Read-only access requirements
4. [Purpose and Overview](./00-Core/purpose-and-overview.md) - Integration points

**Key Points:**
- **Read-only access ONLY** - NO write operations
- Connection string MUST include `ApplicationIntent=ReadOnly`
- Query: Part Master, Location Master
- Fallback gracefully when data not found

---

### Use Case: Building Manual Entry Grid

**Relevant Documents:**
1. [Manual Entry Mode](./02-Workflow-Modes/003-manual-mode-specification.md) - Complete grid specification
2. [Data Flow](./00-Core/data-flow.md) - Validation and save flow
3. All Business Rules - Field-level validation requirements

**Implementation Order:**
1. Read Manual Entry Mode spec completely
2. Implement grid with virtual scrolling
3. Add toolbar with quick actions
4. Implement keyboard navigation
5. Add bulk copy operations
6. Integrate validation

---

### Use Case: Implementing Quality Hold

**Relevant Documents:**
1. [Quality Hold](./01-Business-Rules/quality-hold.md) - Complete procedure specification
2. [Part Number Dynamics](./01-Business-Rules/part-number-dynamics.md) - Quality Hold check trigger
3. [Data Flow](./00-Core/data-flow.md) - Where in workflow to check

**Implementation Order:**
1. Read Quality Hold procedures
2. Implement Settings flag configuration
3. Build Quality Hold dialog UI
4. Add check logic after Part Number entry
5. Implement acknowledgment logging

---

## 📊 Cross-Reference Matrix

**Dependencies Between Specifications**

| Document | Depends On | Referenced By |
|----------|-----------|---------------|
| Purpose and Overview | (Foundation) | All other documents |
| Data Flow | Purpose and Overview | All workflow modes |
| Load Number Dynamics | Data Flow | Load Composition Rules, Workflow modes |
| Load Composition Rules | Load Number Dynamics | Workflow modes |
| Part Number Dynamics | Default Part Types, Quality Hold | All workflow modes |
| P.O. Number Dynamics | Data Flow | All workflow modes |
| Receiving Location Dynamics | Part Number Dynamics | All workflow modes |
| Default Part Types | Part Number Dynamics | Load Composition Rules |
| Quality Hold | Part Number Dynamics | All workflow modes |
| Hub Orchestration | Data Flow, All Business Rules | (Entry point) |
| Manual Entry Mode | Hub Orchestration, All Business Rules | Data Flow |
| Edit Mode | Hub Orchestration, All Business Rules | Data Flow |

---

## 🏷️ Tags for Quick Search

**Search by topic:**

**Auto-Calculate:**
- [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md#auto-calculate-behavior)
- [Load Composition Rules](./01-Business-Rules/load-composition-rules.md#auto-calculate-pieces)

**Validation:**
- [P.O. Number Dynamics](./01-Business-Rules/po-number-dynamics.md#validation-and-standardization-rules)
- [Part Number Dynamics](./01-Business-Rules/part-number-dynamics.md#validation-rules)
- [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md#validation-rules)

**Settings Integration:**
- [Default Part Types](./01-Business-Rules/default-part-types.md#settings-integration)
- [Receiving Location Dynamics](./01-Business-Rules/receiving-location-dynamics.md#settings-integration)
- [Quality Hold](./01-Business-Rules/quality-hold.md#configuration)

**Uneven Division:**
- [Load Composition Rules](./01-Business-Rules/load-composition-rules.md#uneven-division-handling)
- [Load Composition Rules - Overage Distribution](./01-Business-Rules/load-composition-rules.md#overage-rounded-units--coils--accurate-distribution-rules)

**Session Management:**
- [Hub Orchestration](./02-Workflow-Modes/004-hub-orchestration-specification.md#user-story-4---session-state-management-priority-p1)
- [Receiving Location Dynamics](./01-Business-Rules/receiving-location-dynamics.md#session-based-override-persistence)

**CSV Export:**
- [Data Flow](./00-Core/data-flow.md#csv-output-format)
- [Edit Mode](./02-Workflow-Modes/002-editmode-specification.md#user-story-5---re-export-modified-data-priority-p2)

**Audit Trail:**
- [Data Flow](./00-Core/data-flow.md#database-schema)
- [Edit Mode](./02-Workflow-Modes/002-editmode-specification.md#user-story-3---maintain-audit-trail-priority-p1)

---

## 🚀 Implementation Roadmap

**Recommended reading order for development:**

### Phase 1: Foundation (Week 1)
1. [Purpose and Overview](./00-Core/purpose-and-overview.md)
2. [Data Flow](./00-Core/data-flow.md)
3. [Hub Orchestration](./02-Workflow-Modes/004-hub-orchestration-specification.md)

### Phase 2: Business Rules (Week 2)
4. [P.O. Number Dynamics](./01-Business-Rules/po-number-dynamics.md)
5. [Part Number Dynamics](./01-Business-Rules/part-number-dynamics.md)
6. [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md)
7. [Default Part Types](./01-Business-Rules/default-part-types.md)

### Phase 3: Advanced Rules (Week 3)
8. [Load Composition Rules](./01-Business-Rules/load-composition-rules.md)
9. [Receiving Location Dynamics](./01-Business-Rules/receiving-location-dynamics.md)
10. [Quality Hold](./01-Business-Rules/quality-hold.md)

### Phase 4: Workflow Modes (Week 4+)
11. Guided Mode (from archive or new spec)
12. [Manual Entry Mode](./02-Workflow-Modes/003-manual-mode-specification.md)
13. [Edit Mode](./02-Workflow-Modes/002-editmode-specification.md)

---

## 📝 Document Status

| Document | Status | Completeness | Last Updated |
|----------|--------|--------------|--------------|
| Purpose and Overview | ✅ Complete | 100% | 2026-01-25 |
| Data Flow | ✅ Complete | 100% | 2026-01-25 |
| Load Number Dynamics | ✅ Complete | 100% | 2026-01-25 |
| Load Composition Rules | ✅ Complete | 100% | 2026-01-25 |
| Part Number Dynamics | ✅ Complete | 100% | 2026-01-25 |
| P.O. Number Dynamics | ✅ Complete | 100% | 2026-01-25 |
| Receiving Location Dynamics | ✅ Complete | 100% | 2026-01-25 |
| Default Part Types | ✅ Complete | 100% | 2026-01-25 |
| Quality Hold | ✅ Complete | 100% | 2026-01-25 |
| Hub Orchestration | ✅ Complete | 100% | 2026-01-25 |
| Manual Entry Mode | ✅ Complete | 100% | 2026-01-25 |
| Edit Mode | ✅ Complete | 100% | 2026-01-25 |
| Guided Mode (Wizard) | ⚠️ In Archive | See archive | - |

---

## 🔗 External References

**Related Documentation:**
- **CONSTITUTION.md** - Immutable architecture rules
- **AGENTS.md** - AI agent definitions
- **Memory Bank** - Project context and task tracking
- **Instruction Files** - Development guidelines

**Integration Documentation:**
- Infor Visual (ERP) - Read-only integration
- CSV Export Format - Label printing system
- Settings Module - Configuration management

---

## ❓ Frequently Asked Questions

**Q: Which spec should I read first?**  
A: Start with [Purpose and Overview](./00-Core/purpose-and-overview.md) for context, then [Data Flow](./00-Core/data-flow.md) for architecture.

**Q: Where is the Guided Mode (Wizard) specification?**  
A: The original specification is in the archive (`99-Archive/Module_Receiving_SpecSheet.md`). A standalone Guided Mode spec may be created in the future.

**Q: How do I find validation rules for a specific field?**  
A: Use the [Navigation by Use Case](#use-case-implementing-data-validation) section or search the Business Rules documents by field name.

**Q: What's the difference between Load Number and Load Composition?**  
A: **Load Number** = How many loads total. **Load Composition** = Pieces per load and their weight distribution. See [Load Number Dynamics](./01-Business-Rules/load-number-dynamics.md) and [Load Composition Rules](./01-Business-Rules/load-composition-rules.md).

**Q: Where can I find the uneven division calculations?**  
A: [Load Composition Rules - Overage Distribution](./01-Business-Rules/load-composition-rules.md#overage-rounded-units--coils--accurate-distribution-rules)

**Q: How do session overrides work?**  
A: See [Receiving Location Dynamics - Session-Based Override Persistence](./01-Business-Rules/receiving-location-dynamics.md#session-based-override-persistence)

**Q: Where is the CSV export format defined?**  
A: [Data Flow - CSV Output Format](./00-Core/data-flow.md#csv-output-format)

---

## 📞 Support

**For specification questions or clarifications:**
- Review this index for relevant documents
- Check cross-references and dependencies
- Refer to archived original specifications if needed

**For implementation guidance:**
- Follow the recommended implementation roadmap
- Use the cross-reference matrix to understand dependencies
- Reference the CONSTITUTION.md for immutable architecture rules

---

**Last Updated:** 2026-01-25  
**Index Version:** 2.0  
**Total Documents:** 12 active specifications + 2 archived

---

## 🎯 Summary

This navigation index provides a complete guide to all Module_Receiving specifications. Use the category-based navigation for browsing, the use-case navigation for specific implementation tasks, and the cross-reference matrix to understand document relationships.

**All specifications are complete and ready for development use.**
