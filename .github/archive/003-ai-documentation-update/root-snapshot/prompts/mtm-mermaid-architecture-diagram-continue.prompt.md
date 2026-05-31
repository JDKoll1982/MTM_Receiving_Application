# MTM Mermaid Architecture Diagram Continue Prompt

Continue the Mermaid architecture diagram started with `mtm-mermaid-architecture-diagram.prompt.md`.

## Rules

- Analyze production code only
- Exclude `MTM_Receiving_Application.Tests/`, `bin/`, `obj/`, and generated artifacts
- Use only relationships supported by code
- Do not re-explain the project
- Do not repeat prior installments unless a shared cross-cutting type is required for the next relationships

## Output Format

- Return exactly one fenced Mermaid block
- Use `classDiagram`
- Group classes inside Mermaid `namespace` blocks
- Keep node IDs stable and unique
- Include only the next requested installment

## Installment Order

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

## Relationship Rules

Use these patterns when supported by code:

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

## Continuation Task

Resume from the next unfinished installment after the most recently completed block.

If the conversation does not include a previous installment, stop and ask for the last completed installment number.