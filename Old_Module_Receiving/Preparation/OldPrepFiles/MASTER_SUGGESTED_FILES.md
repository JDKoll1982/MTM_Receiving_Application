# Suggested Documentation Files (Module-Agnostic)

## Overview

This document lists non-code documentation files that should exist within any module to support development, troubleshooting, and onboarding. It is derived from Preparation/02_Suggested_Documentation_Files.md and excludes content already covered in the implementation guide or diagram document.

---

## 1. Core Documentation Files (High Priority)

### README.md (Module Root)

**Purpose:** Entry point for module understanding.

**Include:**

- Module purpose and scope.
- Key components and responsibilities.
- Dependencies on shared infrastructure and external systems.
- Quick-start pointer to the module’s preparation workflow.

**Location:** Module_Name/README.md

---

### ARCHITECTURE.md

**Purpose:** Deep dive into module architecture, patterns, and design decisions.

**Include:**

- MVVM usage details and ViewModel boundaries.
- Service responsibilities and orchestration flows.
- DAO pattern summary and result object usage.
- Error handling and validation approach.
- Database interaction patterns and constraints.

**Location:** Module_Name/ARCHITECTURE.md

---

### DATA_MODEL.md

**Purpose:** Database schema reference for the module.

**Include:**

- Entity-relationship diagram (PlantUML).
- Table schemas and constraints.
- Stored procedure catalog.
- Foreign keys and indexing notes.

**Location:** Module_Name/DATA_MODEL.md

---

### WORKFLOWS.md

**Purpose:** Visual and textual description of user workflows.

**Include:**

- Workflow steps and state transitions.
- User interactions per step.
- Data flow between ViewModels.
- Session management notes (if applicable).

**Location:** Module_Name/WORKFLOWS.md

---

## 2. Preparation Files (Planning & Implementation)

### Preparation/03_Clarification_Questions.md

**Purpose:** Questions to answer before implementation begins.

**Include:**

- Architecture decisions.
- Database constraints and migration rules.
- Dependency and package approvals.
- Validation and testing expectations.
- UI/UX constraints and performance requirements.
- Deployment and compatibility expectations.

**Location:** Module_Name/Preparation/03_Clarification_Questions.md

---

### Preparation/04_Implementation_Order.md

**Purpose:** Step-by-step development plan.

**Include:**

- Phased execution order.
- Milestones per phase.
- Dependencies between steps.

**Location:** Module_Name/Preparation/04_Implementation_Order.md

---

### Preparation/05_Task_Checklist.md

**Purpose:** Granular task tracking.

**Include:**

- Setup tasks.
- Model/DAO/Validator tasks.
- ViewModel and navigation tasks.
- Testing tasks.
- Documentation tasks.
- Code review tasks.

**Location:** Module_Name/Preparation/05_Task_Checklist.md

---

### Preparation/06_Schematic_File.md

**Purpose:** Visual diagrams and schematics.

**Include:**

- Folder structure schematic.
- Architecture diagram (PlantUML).
- Workflow state diagram (PlantUML).
- Data flow diagram (PlantUML).

**Location:** Module_Name/Preparation/06_Schematic_File.md

---

### Preparation/07_Research_Archive.md

**Purpose:** Store research notes, links, and investigation findings.

**Include:**

- Library research links.
- Architecture pattern references.
- Testing strategy references.
- Performance and migration research notes.

**Location:** Module_Name/Preparation/07_Research_Archive.md

---

## 3. Runtime Documentation (Developer Experience)

### DEFAULTS.md

**Purpose:** Document default values and configuration settings.

**Include:**

- Default validation rules.
- Default workflow settings.
- Default export settings.

**Location:** Module_Name/DEFAULTS.md

---

### TROUBLESHOOTING.md

**Purpose:** Common issues and solutions.

**Include:**

- Frequent binding and DI errors.
- Common runtime exceptions and resolutions.
- Diagnostic steps and verification commands.

**Location:** Module_Name/TROUBLESHOOTING.md
