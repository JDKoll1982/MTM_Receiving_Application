# List All Available BMad Tasks

## Purpose
Display a comprehensive list of all available BMad tasks from the task manifest with descriptions and usage information.

## When to Use
Discovering what BMad tasks are available to execute.

## Related Agent
`@setup-bmad-master`

## What This Does
- Reads `_bmad/_config/task-manifest.csv`
- Displays all tasks with descriptions
- Shows task categories and workflow associations
- Provides usage examples for each task using the current codebase as well as generic examples if applicable

## Expected Outcome
Complete inventory of available BMad tasks for reference.
