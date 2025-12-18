# Documentation Reorganization Prompt for Local Copilot

Copy and paste this prompt into your local Copilot chat: 

---

I need to reorganize the `Documentation/` folder in my repository `JDKoll1982/MTM_Receiving_Application`. Here's what needs to be done:

## Objective
Reorganize all documentation files into a structured hierarchy with categorized subfolders, rename files to use camelCase with dashes, and create three guide types (UserGuide, DeveloperGuide, CopilotGuide) for each implemented feature.

## Current Files in Documentation/
1.  AUTHENTICATION.md (implemented feature)
2. CONFIGURABLE_SETTINGS.md (implemented feature)
3. DATABASE_ADMIN. md (implemented feature)
4. WINDOW_SIZING_STRATEGY.md (implemented feature)
5. REUSABLE_SERVICES.md (implemented feature)
6. REUSABLE_SERVICES_SETUP.md (implemented feature)
7. NEW_APP_DATA_MODEL.md (future plan)
8. NEW_APP_LABEL_TYPES.md (future plan)
9. NEW_APP_MOCKUP.html (future plan)
10. NEW_APP_OVERVIEW.md (future plan)
11. NEW_APP_USER_WORKFLOW.md (future plan)
12. DUNNAGELABELLOGIC-GOOGLEAPPSCRIPTS.md (future plan)
13. RECEIVINGLABELLOGIC-GOOGLEAPPSCRIPTS.MD (future plan)
14. UPSFEDEXLABELLOGIC-GOOGLEAPPSCRIPTS.md (future plan)

## Target Structure
```
Documentation/
├── README.md (NEW - explains directory layout)
├── Features/
│   ├── Authentication/
│   │   ├── Authentication-UserGuide.md
│   │   ├── Authentication-DeveloperGuide.md
│   │   └── Authentication-CopilotGuide.md
│   ├── ConfigurableSettings/
│   │   ├── ConfigurableSettings-UserGuide.md
│   │   ├── ConfigurableSettings-DeveloperGuide. md
│   │   └── ConfigurableSettings-CopilotGuide.md
│   ├── DatabaseAdmin/
│   │   ├── DatabaseAdmin-UserGuide.md
│   │   ├── DatabaseAdmin-DeveloperGuide.md
│   │   └── DatabaseAdmin-CopilotGuide.md
│   ├── WindowSizing/
│   │   ├── WindowSizing-UserGuide.md
│   │   ├── WindowSizing-DeveloperGuide.md
│   │   └── WindowSizing-CopilotGuide.md
│   └── ReusableServices/
│       ├── ReusableServices-UserGuide.md
│       ├── ReusableServices-DeveloperGuide.md
│       └── ReusableServices-CopilotGuide. md
├── FuturePlans/
│   ├── ReceivingLabels/
│   │   ├── ReceivingLabels-DataModel.md
│   │   ├── ReceivingLabels-LabelTypes.md
│   │   ├── ReceivingLabels-UserWorkflow.md
│   │   ├── ReceivingLabels-Overview.md
│   │   └── ReceivingLabels-UIMockup.html
│   ├── DunnageLabels/
│   │   ├── DunnageLabels-BusinessLogic.md
│   │   ├── DunnageLabels-UserGuide.md
│   │   └── DunnageLabels-DeveloperGuide.md
│   ├── RoutingLabels/
│   │   ├── RoutingLabels-BusinessLogic.md
│   │   ├── RoutingLabels-UserGuide.md
│   │   └── RoutingLabels-DeveloperGuide.md
│   └── GoogleSheetsReplacement/
│       └── GoogleSheets-FunctionalOverview. md
├── API/
│   └── README.md (placeholder)
├── Deployment/
│   └── README.md (placeholder)
├── Troubleshooting/
│   └── README.md (placeholder)
└── Architecture/
    └── README.md (placeholder)
```

## Tasks to Complete

### 1. Review & Fix Existing Documentation
- Read through each existing file
- Fix any technical inaccuracies, typos, or outdated information
- Ensure consistency in formatting (headings, code blocks, tables)
- Validate SQL queries, C# code examples, and file paths

### 2. Split Implemented Features into Three Guide Types

For each implemented feature (Authentication, ConfigurableSettings, DatabaseAdmin, WindowSizing, ReusableServices), create: 

**UserGuide**:  
- How to use the feature from an end-user perspective
- Step-by-step instructions with screenshots/examples
- Troubleshooting common issues
- No technical implementation details

**DeveloperGuide**:
- Technical implementation details
- Code architecture and design patterns
- API references and method signatures
- Database schemas and stored procedures
- How to extend or modify the feature

**CopilotGuide**:
- Context for AI assistants (like GitHub Copilot)
- Key classes, methods, and patterns used
- Common tasks and how to implement them
- Code generation hints and examples
- Testing strategies

### 3. Organize Future Plans

Move future-related files to `FuturePlans/` and categorize by feature area: 
- **ReceivingLabels**: NEW_APP_DATA_MODEL.md, NEW_APP_LABEL_TYPES.md, NEW_APP_MOCKUP.html, NEW_APP_OVERVIEW.md, NEW_APP_USER_WORKFLOW.md
- **DunnageLabels**:  DUNNAGELABELLOGIC-GOOGLEAPPSCRIPTS.md
- **RoutingLabels**:  UPSFEDEXLABELLOGIC-GOOGLEAPPSCRIPTS.md
- **GoogleSheetsReplacement**: RECEIVINGLABELLOGIC-GOOGLEAPPSCRIPTS.MD

Rename these files to match the pattern: `FeatureName-DocumentType.md`

### 4. Create Documentation README. md

Create `Documentation/README.md` with:
- Overview of the documentation structure
- Description of each folder and its purpose
- Guide types explanation (User/Developer/Copilot)
- Quick links to commonly accessed docs
- Contributing guidelines for documentation

### 5. Create Placeholder Folders

Add placeholder README.md files in: 
- `API/` - "API documentation will be added when REST APIs are implemented"
- `Deployment/` - "Deployment guides will be added for production rollout"
- `Troubleshooting/` - "Common issues and solutions will be documented here"
- `Architecture/` - "System architecture diagrams and design documents"

## Example File Splitting

Take `AUTHENTICATION.md` and split it into: 

**Authentication-UserGuide.md**: 
- How to log in (PIN vs Windows Auth)
- Creating a new user account
- Session timeout behavior
- What to do if you can't log in
- FAQ for end users

**Authentication-DeveloperGuide.md**:
- Authentication architecture (IService_Authentication interface)
- Database schema (users, workstation_config, departments tables)
- Stored procedures (sp_ValidateUserPin, sp_CreateNewUser)
- Session management implementation
- Security considerations
- How to add new authentication methods

**Authentication-CopilotGuide.md**:
```markdown
# Authentication System - Copilot Context

## Key Classes
- `IService_Authentication` - Main authentication service
- `Service_SessionManager` - Session timeout handling
- `Model_UserSession` - Session data model
- `Model_WorkstationConfig` - Workstation type detection

## Common Tasks

### Authenticate a user by PIN
[code example]

### Create a new user account
[code example]

### Check if session is timed out
[code example]

## Database Queries
[Common queries for authentication operations]

## Testing
[Test patterns and example test cases]
```

## Validation Rules

Before finalizing: 
- [ ] All files use correct naming:  `FeatureName-GuideType.md` (camelCase with dashes)
- [ ] No duplicate content across files
- [ ] All internal links updated to new paths
- [ ] All code examples are syntactically correct
- [ ] All SQL queries are valid MySQL syntax
- [ ] All file paths reference correct locations
- [ ] README.md includes all folders and their purposes
- [ ] Each folder has at least one meaningful file

## Output Format

Provide me with: 
1. A file-by-file change summary (what was moved, renamed, split)
2. The complete content of the new `Documentation/README.md`
3. Confirmation that all validation rules passed
4. Any issues found and how they were fixed

---

**Start by analyzing the current files and proposing the exact file reorganization plan before making any changes.**