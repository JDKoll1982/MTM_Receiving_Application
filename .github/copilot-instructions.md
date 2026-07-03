# MTM Receiving Application Development Guide

- Repository-level source of truth for coding agents in this workspace.
- Use this file for global rules.
- Use category-specific files under `.github/instructions/` for task-specific guidance.

## Core Response Rules

- Default to code-only or bullet-only replies unless the user asks for explanation.
- Prefer bullets over paragraphs.
- Keep responses brief and direct.
- Do not add explanations unless asked.
- This rule applies to normal user-facing replies.
- Keep required workflow preambles, approval prompts, and tool-intent statements as short as needed.
- Do not let this rule override higher-priority task instructions or safety requirements.

## Start Here

- Read `README.md` for the project overview and repository layout.
- Read `.github/README.md` for the active AI-documentation taxonomy.
- Read this file for global rules that apply everywhere.
- Read only the instruction files that match the files you are changing or the task you are doing.
- Treat `.github/archive/` as historical reference only.

## Non-Negotiable Rules

- Follow the MVVM flow: View → ViewModel → Service → DAO → Database.
- Do not let ViewModels call DAOs or `Helper_Database_*` classes directly.
- Use `x:Bind` in XAML. Do not introduce runtime `{Binding}`.
- Keep DAOs instance-based and return `Model_Dao_Result` or `Model_Dao_Result<T>` for expected failures.
- Use stored procedures for MySQL work. Do not write raw MySQL SQL in C#.
- Treat the Infor Visual SQL Server connection as read only. No `INSERT`, `UPDATE`, or `DELETE`.
- Keep business logic out of `.xaml.cs` code-behind.
- End async methods with `Async`.
- When rebuilding a full `ObservableCollection`, prefer replacing the collection instance instead of `Clear()` plus repeated `Add()` calls.

## Major Assumptions

- Pause and ask the user if implementation requires a major assumption.
- Examples: choosing between valid implementations, inferring missing stored procedures or contracts, deciding ownership between modules, or changing workflow scope without explicit direction.
- Use the chat-facing `vscode_askQuestions` approval flow.
- Do not create an assumption file by default.

## Required Workflow

1. Read the smallest relevant instruction set for the current task.
2. Inspect the owning code path before editing.
3. Make the smallest grounded change that tests the current hypothesis.
4. Run the narrowest validation available after the first substantive edit.
5. Update documentation or metadata when the change invalidates an active source-of-truth file.

## Instruction Map

- `.github/instructions/architecture/` — MVVM, DAO, CQRS, dialogs, settings, converters, and WinUI 3 design constraints.
- `.github/instructions/database/` — MySQL stored procedures and Infor Visual query rules.
- `.github/instructions/documentation/` — Markdown, spec slices, doc maintenance, and doc updates.
- `.github/instructions/languages/` — C#, PowerShell, Python, shell, and related file-specific conventions.
- `.github/instructions/quality/` — review rules, security, performance, code quality, comments.
- `.github/instructions/testing/` — test strategy and test-writing expectations.
- `.github/instructions/tooling/` — MCP, WinApp, skill authoring, and Serena subfolder guidance.
- `.github/instructions/workflow/` — prompt engineering, research, agent authoring, and process.
- `.github/instructions/copilotforms/` — CopilotForms export-specific workflows and handlers.

## Recommended Starting Points

- C# or XAML changes: `.github/instructions/languages/csharp.instructions.md`
- MVVM or service-layer work: `.github/instructions/architecture/mvvm-pattern.instructions.md`
- DAO changes: `.github/instructions/architecture/dao-pattern.instructions.md`
- MySQL or Infor Visual work: `.github/instructions/database/sql-sp-generation.instructions.md` and `.github/instructions/database/infor-visual-query-authoring.instructions.md`
- Tests: `.github/instructions/testing/testing-strategy.instructions.md`
- Tooling-heavy agent work: `.github/instructions/tooling/mcp-tooling.instructions.md`

## Documentation And Metadata Maintenance

- If you change active repository guidance, update the matching index or source-of-truth file.
- If you change `.github` structure, update `.github/README.md` and relevant category READMEs.
- If code changes affect CopilotForms workflow understanding, update `docs/CopilotForms/data/copilot-forms.config.json` or the split module metadata files.
- Prefer one authoritative file per topic. Delete, archive, or merge duplicates instead of maintaining parallel guidance.

## Validation Expectations

- Use the narrowest executable validation available for the changed slice.
- Prefer targeted tests or focused build/test tasks over full-solution checks when possible.
- If no executable validation exists for documentation-only work, validate by checking links, paths, and workspace configuration consistency.

## Related Files

- `README.md` — project overview and onboarding entry point.
- `.github/README.md` — AI customization taxonomy and maintenance map.
- `AGENTS.md` — repository agent contract and execution profile.
- `.github/prompts/README.md` — active prompt taxonomy and usage.
- `.github/instructions/README.md` — active instruction taxonomy and starting points.

## Code Examples

```csharp
// ❌ FORBIDDEN - ViewModel calling DAO directly
public partial class ViewModel_Bad : ViewModel_Shared_Base
{
	private async Task LoadAsync()
	{
		var result = await Dao_ReceivingLine.GetLinesAsync(loadId);
	}
}
```

### Service Layer Pattern

```csharp
// ✅ CORRECT - Service provides business logic abstraction
public interface IService_MySQL_ReceivingLine
{
	Task<Model_Dao_Result> InsertLineAsync(Model_ReceivingLine line);
	Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetLinesByLoadAsync(int loadId);
}

public class Service_MySQL_ReceivingLine : IService_MySQL_ReceivingLine
{
	private readonly Dao_ReceivingLine _dao;
	private readonly IService_LoggingUtility _logger;

	public Service_MySQL_ReceivingLine(
		Dao_ReceivingLine dao,
		IService_LoggingUtility logger)
	{
		_dao = dao;
		_logger = logger;
	}

	public async Task<Model_Dao_Result> InsertLineAsync(Model_ReceivingLine line)
	{
		_logger.LogInfo($"Inserting receiving line for PO: {line.PONumber}");
		return await _dao.InsertReceivingLineAsync(line);
	}
}
```

### DAO Pattern

- Instance-based DAOs only.
- Use stored procedures only.
- Return `Model_Dao_Result` or `Model_Dao_Result<T>`.
- Never throw exceptions for expected operational failures.

```csharp
public class Dao_ReceivingLine
{
	private readonly string _connectionString;

	public Dao_ReceivingLine(string connectionString)
	{
		ArgumentNullException.ThrowIfNull(connectionString);
		_connectionString = connectionString;
	}
}
```

### XAML Binding Pattern

```xaml
<!-- ✅ CORRECT - Using x:Bind with proper mode -->
<TextBox Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay}" />

<!-- ❌ FORBIDDEN - Runtime binding -->
<TextBox Text="{Binding MyProperty}" />
```

### Dependency Injection Registration

- Register services in `Infrastructure/DependencyInjection/` extension methods.
- Do not register app services directly in `App.xaml.cs`.

## User Interface Guidelines

- For the Core Settings user management page, user card hover should show a grey underline below the card.
- Selected cards should show a blue underline, not a full-card highlight.

## Code Quality Standards

- Always use braces.
- Use explicit accessibility modifiers.
- Prefer null-conditional operators and nullable annotations.
- Use `Order()` for simple sorting instead of `OrderBy(x => x)`.
- Follow `.editorconfig` formatting rules.
- Prefer file-scoped namespaces.
- Use `nameof` instead of string literals for member names.
- Create XML doc comments for public APIs with `<summary>`, `<param>`, and `<returns>`.

## Testing Standards

- Validator (FluentValidation) → unit test.
- ViewModel with IMediator → unit test.
- ViewModel with concrete services → integration test.
- Handler with concrete DAOs → integration test.
- Handler with interfaces only → unit test.
- DAO → integration test.
- Use FluentAssertions.
- Use `MethodName_Should<Result>_When<Condition>` naming.
- Integration tests use `IAsyncLifetime` for setup and cleanup.
- Prefix test data with `TEST-`.

## Database Access

- MySQL: use stored procedures exclusively.
- SQL Server / Infor Visual: read only.
- Never write raw SQL in C# for MySQL work.
- Never write to Infor Visual.

## Error Handling

- DAOs return failure results instead of throwing for expected failures.
- ViewModels catch and handle service errors through the error handler.

## Module Structure

- `Module_Core` - Shared infrastructure, helpers, base classes.
- `Module_Shared` - Shared ViewModels, Views, models.
- `Module_Receiving` - Receiving workflow and label generation.
- `Module_Dunnage` - Dunnage management.
- `Module_Reporting` - Report generation.
- `Module_Settings` - Configuration UI.
- `Module_Volvo` - Volvo-specific integration.

## Debugging Checklist

- DI registration exists in `Infrastructure/DependencyInjection/`.
- ViewModel is `partial`.
- ViewModel inherits from `ViewModel_Shared_Base`.
- XAML uses `x:Bind`.
- No ViewModel→DAO calls.
- DAOs return `Model_Dao_Result`.
- MySQL uses stored procedures only.
- No writes to SQL Server.
- Async methods end with `Async`.
- Error handling is in place.
- Check XAML binding errors in Output window.

## Validation

- Build: `dotnet build MTM_Receiving_Application.sln`
- Test: `dotnet test MTM_Receiving_Application.sln`
- Check for architecture violations:
  - ViewModel calling `Dao_` directly.
  - Static DAO classes.
  - Raw SQL in C#.
  - `INSERT` / `UPDATE` / `DELETE` against SQL Server.

## User Environment Consideration

- Avoid using terminal commands for validation when they lock up the terminal.
- Prefer non-terminal tooling for validation when practical.

## XLSX Creation For Multi-User Access

- Use `FileShare.ReadWrite` or shared access strategies when creating XLSX files.

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.
