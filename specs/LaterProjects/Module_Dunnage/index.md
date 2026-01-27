# Module_Dunnage Specifications - Navigation Index

**Version:** 1.0  
**Last Updated:** 2026-01-25  
**Purpose:** Central navigation guide for Module_Dunnage specifications

---

## 📋 Quick Reference

Module_Dunnage manages the receiving and tracking of dunnage materials (shipping/packing materials and containers) used in manufacturing operations. It provides workflow modes for data entry, CSV label generation, and inventory management.

**Key Features**:
- 3 workflow modes (Guided/Manual/Edit)
- Configurable dunnage types with custom specification fields
- Part-type associations
- CSV label generation
- Inventory tracking
- Admin management interface

---

## 🎯 Getting Started

**New to Module_Dunnage?** Start here:

1. **[Purpose and Overview](./00-Core/purpose-and-overview.md)** - Understand what Module_Dunnage does
2. **[Data Flow](./00-Core/data-flow.md)** - See how data moves through the system
3. **[Business Rules Overview](#business-rules)** - Core rules and validation
4. **[Workflow Modes Overview](#workflow-modes)** - Guided, Manual, and Edit modes

---

## 📁 Directory Structure

```
specs/Module_Dunnage/
├── index.md (this file)
├── CLARIFICATIONS.md (Edge cases and questions)
├── 00-Core/
│   ├── purpose-and-overview.md
│   └── data-flow.md
├── 01-Business-Rules/
│   ├── dunnage-type-configuration.md
│   ├── dynamic-specification-fields.md
│   ├── part-type-associations.md
│   ├── workflow-mode-selection.md
│   ├── quantity-validation.md
│   ├── csv-export-paths.md
│   ├── inventory-tracking.md
│   ├── custom-field-persistence.md
│   ├── edit-mode-search.md
│   └── multi-load-entry.md
├── 02-Workflow-Modes/
│   ├── 001-guided-mode-specification.md
│   ├── 002-manual-entry-mode-specification.md
│   ├── 003-edit-mode-specification.md
│   └── 004-admin-mode-specification.md
├── 03-Implementation-Blueprint/
│   ├── index.md
│   ├── file-structure.md
│   └── naming-conventions-extended.md
└── 99-Archive/
```

---

## 🎨 Navigation by Category

### Core Concepts

| Document | Purpose | Key Topics |
|----------|---------|------------|
| [Purpose and Overview](./00-Core/purpose-and-overview.md) | System overview and functionality | Workflow modes, User personas, Integration points |
| [Data Flow](./00-Core/data-flow.md) | Complete data architecture | Transaction flow, Validation, Persistence |

---

### Business Rules

| Document | Purpose | Key Topics |
|----------|---------|------------|
| [Dunnage Type Configuration](./01-Business-Rules/dunnage-type-configuration.md) | Type management rules | Type creation, Icons, Active status |
| [Dynamic Specification Fields](./01-Business-Rules/dynamic-specification-fields.md) | Custom field system | Field types, Required fields, Validation |
| [Part-Type Associations](./01-Business-Rules/part-type-associations.md) | Part-type relationships | Association management, Multi-type support |
| [Workflow Mode Selection](./01-Business-Rules/workflow-mode-selection.md) | Mode selection logic | Default mode, Mode switching, User preferences |
| [Quantity Validation](./01-Business-Rules/quantity-validation.md) | Quantity entry rules | Minimum values, Zero/negative handling |
| [CSV Export Paths](./01-Business-Rules/csv-export-paths.md) | Export configuration | Local/network paths, Fallback behavior |
| [Inventory Tracking](./01-Business-Rules/inventory-tracking.md) | Inventoried dunnage | Quick-add lists, Priority ordering |
| [Custom Field Persistence](./01-Business-Rules/custom-field-persistence.md) | Spec value storage | Data format, Required field enforcement |
| [Edit Mode Search](./01-Business-Rules/edit-mode-search.md) | Transaction search | Search filters, Date ranges |
| [Multi-Load Entry](./01-Business-Rules/multi-load-entry.md) | Bulk load addition | Add multiple dialog, Quantity multipliers |

---

### Workflow Modes

| Document | Purpose | Key Topics |
|----------|---------|------------|
| [Guided Mode](./02-Workflow-Modes/001-guided-mode-specification.md) | 5-step wizard workflow | Type selection, Part selection, Quantity, Details, Review |
| [Manual Entry Mode](./02-Workflow-Modes/002-manual-entry-mode-specification.md) | Grid-based bulk entry | High-volume receiving, Keyboard shortcuts |
| [Edit Mode](./02-Workflow-Modes/003-edit-mode-specification.md) | Historical data editing | Search, Modify, Re-export |
| [Admin Mode](./02-Workflow-Modes/004-admin-mode-specification.md) | Configuration management | Type/Part/Spec management, Inventory |

---

### Implementation Blueprint

| Document | Purpose | Content |
|----------|---------|---------|
| [Implementation Blueprint Index](./03-Implementation-Blueprint/index.md) | Blueprint overview | Architecture, Development phases |
| [File Structure](./03-Implementation-Blueprint/file-structure.md) | Complete file listing | All files with naming conventions |
| [Naming Conventions](./03-Implementation-Blueprint/naming-conventions-extended.md) | Naming standards | 5-part naming for ViewModels, Views, Services, Models, DAOs |

---

## 🔗 Related Documentation

### Module_Settings.Dunnage Specifications
- [Module_Settings.Dunnage Index](../Module_Settings.Dunnage/index.md) - Settings configuration
- [Settings Categories](../Module_Settings.Dunnage/01-Settings-Categories/) - Configurable settings

### Core Architecture
- [Project Constitution](../../.github/CONSTITUTION.md) - Immutable architecture rules
- [Development Instructions](../../.github/copilot-instructions.md) - Coding standards

---

## 📝 Document Update Guidelines

When updating these specifications:

1. **Version Control**: Update "Last Updated" date in modified files
2. **Cross-References**: Verify all internal links still work
3. **Consistency**: Maintain naming conventions and structure
4. **Clarifications**: Add edge cases to CLARIFICATIONS.md
5. **Index Update**: Update this index.md when adding new files

---

**Last Updated:** 2026-01-25  
**Status:** Phase 1 Complete, Phase 2 In Progress
