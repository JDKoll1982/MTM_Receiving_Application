# Workflow Diagram Generator
Generates Mermaid-based workflow diagram markdown files from a selected Repomix output file. Output files are created per workflow and stored under .repomix/diagrams/{Type}/Workflow_{Name}.md.
## Location
- Script: .repomix/Scripts/Generate-Workflow-Diagrams.ps1
- Templates: .repomix/diagrams/templates
- Outputs: .repomix/diagrams/{Type}
## Templates
Two templates are available:
- workflow.template.md: Standard workflow layout used by the Volvo examples.
- workflow-dialog.template.md: Dialog-centric workflow layout.
You can add more templates to .repomix/diagrams/templates as needed.
## Usage
### Generate from a JSON config
Use a JSON file that includes the workflow name and template:
- Sample config: .repomix/diagrams/templates/workflow-diagrams.sample.json
Run:
- .repomix/Scripts/Generate-Workflow-Diagrams.ps1 -RepomixOutputPath .repomix/outputs/code-only/repomix-output-code-only.md -Type Volvo -WorkflowConfigPath .repomix/diagrams/templates/workflow-diagrams.sample.json
### Generate from explicit names
- .repomix/Scripts/Generate-Workflow-Diagrams.ps1 -RepomixOutputPath .repomix/outputs/code-only/repomix-output-code-only.md -Type Volvo -WorkflowNames Volvo_ShipmentEntry,Volvo_GenerateLabels,Volvo_PreviewEmail -Template workflow
### Auto-discover names from Repomix output
- .repomix/Scripts/Generate-Workflow-Diagrams.ps1 -RepomixOutputPath .repomix/outputs/code-only/repomix-output-code-only.md -Type Volvo -AutoDiscover
## Parameters
- RepomixOutputPath: Path to the Repomix output markdown file.
- Type: Output folder name under .repomix/diagrams.
- WorkflowNames: Optional list of workflow names to generate.
- WorkflowConfigPath: Optional JSON file for name + template selection.
- Template: Default template name when using WorkflowNames or AutoDiscover.
- TemplateRoot: Template folder path (default: .repomix/diagrams/templates).
- AutoDiscover: Scan Repomix output for workflow candidates.
## Output Path Convention
Each workflow is written to:
- .repomix/diagrams/{Type}/Workflow_{Name}.md
Example:
- .repomix/diagrams/Volvo/Workflow_Volvo_ShipmentEntry.md
## Notes
- The generator only creates template-based stubs. Use the Volvo workflow diagrams as a guide to fill in real logic and steps.
- If AutoDiscover yields too many names, switch to WorkflowNames or WorkflowConfigPath for tighter control.
