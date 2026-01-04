# Execute Specific BMad Workflow

## Purpose
Run a specific BMad workflow by name. Provide the workflow name and any required parameters.

## When to Use
You know the workflow you want to run and need to execute it.

## Related Agent
`@setup-bmad-master`

## What This Does
- Locates workflow file in `_bmad/` structure
- Loads workflow configuration (YAML)
- Executes workflow steps in sequence
- Collects and saves outputs
- Reports workflow completion status

## Expected Outcome
Specified workflow executed successfully with outputs generated.
