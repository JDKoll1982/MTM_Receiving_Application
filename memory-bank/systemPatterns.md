# System Patterns

Last Updated: 2026-01-24

## Architecture
- WinUI 3 desktop app targeting .NET 8
- MVVM: Views (XAML) bind to ViewModels using `{x:Bind}`
- ViewModels are partial and typically inherit from `ViewModel_Shared_Base`
- Services orchestrate business logic and DAO calls
- DAOs perform data access and return `Model_Dao_Result`

## Key patterns and constraints
- ViewModels never call DAOs directly
- DAOs are instance-based, injected
- MySQL uses stored procedures only
- SQL Server / Infor Visual is read-only
- Error handling: DAOs return failures; services interpret; ViewModels present

## Documentation patterns (Agent Skills guide)
- Static HTML pages with shared `styles.css` + `app.js`
- Each page includes a `Last Updated: YYYY-MM-DD` metadata block
