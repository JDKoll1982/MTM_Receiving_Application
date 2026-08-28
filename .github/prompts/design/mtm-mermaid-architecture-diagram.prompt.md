<!-- 
[DOC-META-START]
- File Name: mtm-mermaid-architecture-diagram.prompt.md
- Description: Generate a Mermaid class diagram for the MTM Receiving Application from the real workspace structure, emitting only code-supported relationships.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 1-4: # MTM Mermaid Architecture Diagram Prompt
  - Line 5-17: ## Goal
  - Line 18-28: ## Scope Rules
  - Line 29-43: ## Required Anchor Files
  - Line 44-104: ## What To Include
  - Line 105-155: ## Extraction Rules
  - Line 156-173: ## Relationship Rules
  - Line 174-209: ## Output Constraints
  - Line 210-217: ## Accuracy Rules
  - Line 218-221: ## Response Format
  - Line 222-228: ## First Task
- Critical Notes: Emit the diagram in deterministic installments, including only relationships directly evidenced in code.
[DOC-META-END]
-->

# MTM Mermaid Architecture Diagram Prompt

Generate a Mermaid class diagram for the MTM Receiving Application by analyzing the real workspace structure and emitting only relationships you can support from code.

## Goal

Produce a Mermaid `classDiagram` that maps the MTM Receiving Application architecture across:

- Entry points
- Infrastructure
- Shared base types
- Feature modules

The application is a WinUI 3 desktop solution with strict MVVM layering:

`View -> ViewModel -> IService -> Service -> DAO -> Database`

## Scope Rules

Analyze production code only. Exclude:

- `MTM_Receiving_Application.Tests/`
- `bin/`
- `obj/`
- generated artifacts

Use only classes, properties, methods, and relationships that exist in the workspace. Do not invent types, bindings, commands, or registrations.

## Required Anchor Files

Start from these concrete files and expand outward from them:

- `App.xaml.cs`
- `MainWindow.xaml.cs`
- `Infrastructure/DependencyInjection/CoreServiceExtensions.cs`
- `Infrastructure/DependencyInjection/CqrsInfrastructureExtensions.cs`
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`
- `Infrastructure/Configuration/*.cs`
- `Infrastructure/Logging/SerilogConfiguration.cs`
- `Module_Shared/ViewModels/ViewModel_Shared_Base.cs`
- `Module_Core/Models/Core/Model_Dao_Result.cs`
- `Module_Core/Models/Core/Model_Dao_Result_Generic.cs`

## What To Include

### 1. Entry Points

Include:

- `App`
- `MainWindow`

Show responsibilities only when directly visible from code, such as:

- host creation
- DI configuration
- startup lifecycle
- shell navigation

### 2. Infrastructure

Include these namespaces if present:

- `Infrastructure.DependencyInjection`
- `Infrastructure.Configuration`
- `Infrastructure.Logging`

Show registrar classes, configuration models, and logging configuration types.

### 3. Shared Types

Always include:

- `ViewModel_Shared_Base`
- `Model_Dao_Result`
- `Model_Dao_Result<T>`

### 4. Modules

For each of these module families, include the real classes found in the workspace:

- `Module_Shared`
- `Module_Dunnage`
- `Module_OutsideService`
- `Module_Receiving`
- `Module_Reporting`
- `Module_ShipRec_Tools`
- `Module_Volvo`
- `Module_Settings.Core`
- `Module_Settings.DeveloperTools`
- `Module_Settings.Dunnage`
- `Module_Settings.Receiving`
- `Module_Settings.Reporting`
- `Module_Settings.Volvo`

Within each module, include only the relevant public architectural surface:

- Views
- ViewModels
- Models
- Service interfaces
- Service implementations
- DAO classes

## Extraction Rules

### Views

For each View class:

- include the class name
- include the `ViewModel` property if present
- include up to 6 key `x:Bind` targets that are clearly visible in XAML or code-behind
- include up to 4 key commands bound from the ViewModel when clearly visible

If the view has many bindings, keep only the most architecturally important ones.

### ViewModels

For each ViewModel:

- show inheritance from `ViewModel_Shared_Base` when applicable
- include public state surfaced through observable properties
- include key public command properties or `[RelayCommand]`-backed methods
- include only methods that help explain navigation, orchestration, validation, loading, saving, or workflow transitions

Do not dump every helper method.

### Service Interfaces and Implementations

For each service contract and implementation:

- show the interface name and implementation name
- include key method signatures only
- show implementation relationships only when directly supported by constructor injection, inheritance, or DI registration

### DAOs

For each DAO:

- include the class name
- include key public data-access methods
- show `Model_Dao_Result` or `Model_Dao_Result<T>` return types where used

If a data-layer interface exists, include it only when it is materially part of the architecture.

### Models

For each model:

- include the class name
- include only key properties that help explain module data flow

Limit model detail aggressively. The diagram should stay architectural, not exhaustive.

## Relationship Rules

Use these Mermaid relationship patterns when supported by code:

- `View --> ViewModel : x:Bind`
- `ViewModel --> IService : uses`
- `IService <|.. Service : implements`
- `Service --> DAO : calls`
- `DAO --> Model_Dao_Result : returns`
- `ViewModel_Shared_Base <|-- ViewModel : inherits`
- `DependencyInjection --> Service : registers`
- `DependencyInjection --> DAO : registers`
- `App --> MainWindow : creates`
- `App --> DependencyInjection : configures`
- `MainWindow --> View : navigates to`

Also include other Mermaid class relationships only if they are explicitly evidenced in code and materially useful.

## Output Constraints

This solution is too large for a reliable one-shot exhaustive diagram. You MUST emit the result in deterministic installments.

### Installment Order

Emit in this order:

1. `Infrastructure`
2. `Module_Shared`
3. `Module_Dunnage`
4. `Module_OutsideService`
5. `Module_Receiving`
6. `Module_Reporting`
7. `Module_ShipRec_Tools`
8. `Module_Volvo`
9. `Module_Settings.Core`
10. `Module_Settings.DeveloperTools`
11. `Module_Settings.Dunnage`
12. `Module_Settings.Receiving`
13. `Module_Settings.Reporting`
14. `Module_Settings.Volvo`

### Installment Format

For each response:

- output exactly one fenced Mermaid block
- use `classDiagram`
- group classes inside Mermaid `namespace` blocks that match the real module or infrastructure area included in the installment
- include the requested namespace block or blocks for that installment only
- repeat any shared cross-cutting classes that are needed for the relationships in that installment
- keep all Mermaid node IDs unique and stable

If you are continuing from a prior installment, do not re-explain the project. Just emit the next Mermaid block.

## Accuracy Rules

- Prefer architectural accuracy over completeness
- If a relationship is ambiguous, omit it
- If a symbol exists but its bound properties or methods are not cheaply discoverable, include the class without guessing members
- Ignore test-only types
- Ignore private implementation details unless they are necessary to explain a relationship

## Response Format

Return only a fenced Mermaid block for the current installment.

## First Task

Begin with installment 1 only:

- entry points
- `Infrastructure`
- shared cross-cutting types needed by those relationships

Do not continue past installment 1 unless asked.