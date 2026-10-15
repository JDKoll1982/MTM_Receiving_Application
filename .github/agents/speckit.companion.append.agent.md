---
description: Append a new capability to the active spec without regenerating its task list, preserving every completed task
---

# Append to the Active Spec

Add scope to a spec that is already in flight. This command edits the active feature's spec in
place and extends `tasks.md` by appending, so tasks already completed keep their ids, their
checkboxes and their journal entries. It never mints a feature directory and never changes the
spec's lifecycle status.

<!-- speckit-companion:part command-spelling -->
## Name every command the way this project registers it

Commands are named in dot form throughout this body, `speckit.companion.plan`, because that is their canonical id, and without a leading slash, because the spelling a host actually registers is not always this one. Claude Code installs `/speckit-companion-plan`. Look at how the commands are installed in this project, under the agent's own commands or skills directory, and use that spelling every time you name one to the developer or dispatch one yourself. A dotted name typed into a host that registered dashes resolves to nothing at all.
<!-- /speckit-companion:part command-spelling -->

<!-- speckit-companion:part concise -->
## The smallest thing that works

**Before building anything, stop at the first rung that holds:** does it need to exist at all; does this codebase already have it; does the standard library, the platform, or an installed dependency do it; can it be one line; only then, the minimum code that works. Fix the cause where every caller passes through, not the symptom one caller reported. Delete rather than add, boring rather than clever: no interface with one implementation, no factory for one product, no scaffolding for later. Lean governs what you build and write, not how the work is split: sending work to a worker where a step says to is not extra.

**Write it the way you would say it.** One idea per sentence. No em-dashes: a full stop, a comma or a colon says it. Say what happens, not what the system "shall be capable of". Never a section that exists to say "N/A": remove it instead.

**The same test governs what you write, and again once it is written.** A section nobody acts on is removed, not filled in. No requirement for what a type or a test already enforces. A third scenario has to cover a failure the first two miss. Then reread and cut: the sentence restating the one before it, the example longer than its rule, the clause defending a choice nobody challenged, the prose repeating a table. Each reads as thoroughness and is what makes a body too long to follow.

**Never simplify away** validation at a trust boundary, error handling that prevents data loss, security, accessibility, or anything the spec asks for. **A corner cut on purpose** carries `// simplified: <ceiling>, <what to do when it binds>` in the code and one `concerns` entry in this step's capture.
<!-- /speckit-companion:part concise -->

## Outline

Append a user story, its requirements and its tasks to the active spec, keep every completed task
intact, then hand back to `speckit.companion.implement`.

## Prerequisites

Verify Python is available by running `python3 --version`. If it is not, still make the spec and
task edits and say the capture was skipped. Never fail the host command.

## Execution

<!-- speckit-companion:phase gather -->
<!-- speckit-companion:node append-guard -->
1. **Resolve the active feature, and refuse the three cases this command must not handle.** Read
   `.specify/feature.json` for the feature directory. This command only ever edits the spec that is
   already active, so it never mints a directory. Stop, naming the reason, when any of these holds:

   - No `feature.json`, or the directory it names is missing → tell the user to run
     `speckit.companion.specify`.
   - `.spec-context.json` says `status: completed` → the spec is finished; a new capability belongs
     in a new spec, so send them to `speckit.companion.specify`.
   - The addition is genuinely separable and would ship on its own → say so and send them to
     `speckit.companion.specify`. One spec is one feature. Folding an independent capability in
     makes both harder to verify and delays completing either.

<!-- speckit-companion:node append-scope -->
2. **Fix the scope before editing anything.** Take the capability from the request. When it is
   ambiguous enough that two different implementations would satisfy it, ask one question and stop.
   Otherwise proceed on an informed default and record it under Assumptions, the rule the specify
   step already follows.

<!-- speckit-companion:phase author -->
<!-- speckit-companion:node append-spec -->
3. **Append to the spec, continuing every numbering.** Open
   `<feature_directory>/<short-name>.spec.md` and add in place, keeping the existing section order:

   - a new `### User Story N - <title> (Priority: P#)` with its paragraph, **Why this priority**,
     **Independent Test** and numbered **Acceptance Scenarios**. `N` continues from the highest
     story already present.
   - new `FR-0xx` entries continuing the highest number in use, each one testable MUST or SHOULD.
   - new `SC-0xx` entries continuing the highest number in use, measurable and technology agnostic.
   - new **Edge Cases**, and new **Verbatim Constraints** only where the request pins an exact
     string.
   - new **Assumptions** wherever the addition rested on a default.

   Edit surgically. An append that renumbers or rewords what is already there breaks the trace from
   requirement to task, and the trace is the only reason the task list can be trusted.

4. **Log the new requirements in one batch**, so the coverage map stays answerable and the
   requirement is captured as readable content rather than a bare id:

   ```bash
   python3 .specify/extensions/companion/scripts/write-context.py --feature-dir <feature_directory> --step <currentStep> --batch '{
     "coverage": [
       {"req": "FR-0xx", "title": "<the requirement one-line text>"}
     ],
     "set": {"intent": "<the added capability in one line>"}
   }'
   ```

   Read `<currentStep>` from `.spec-context.json` and pass that value: append is not a lifecycle
   step, and the writer only accepts the steps it knows. That records the capture under whichever
   step is live, which is where it happened.

<!-- speckit-companion:node append-tasks -->
5. **Append to the task list. Never regenerate it.** `tasks.md` is where implement keeps its
   progress, so a regenerated list loses the one thing this command exists to protect. Read the
   file, find the highest `T###` in use, and add the new work after the last existing task, in the
   same line format:

   `- [ ] **T###** [P?] [US#] <description> · <exact/file/path>`

   - Continue the `T###` sequence from the highest id present. Never reuse, renumber or reorder one.
   - Give the new story its own phase section, following the shape already in the file: a `Files`
     line, then waves headed as independent groups with `[P]`, then explicit join lines. Implement
     reads that shape to decide what can run in parallel, so a flat dump of new tasks costs it the
     ability to fan out.
   - Keep every existing line byte for byte. A checked box, or a task carrying a journal entry,
     must survive untouched.
   - Every file still needs exactly one owning phase. If the addition edits a file another phase
     already owns, put the work in that phase rather than creating a second owner, and say which
     phase you chose.

6. **Record the requirement-to-task map**, one batch, the same call shape the tasks step uses,
   including the new tasks against the requirements they cover.

<!-- speckit-companion:node append-checklist -->
7. **Re-check the plan only if the design moved.** When the addition introduces a new table, a new
   dependency, or a new subsystem, say so and offer `speckit.companion.plan`. Do not run it
   unasked: it rewrites the design artifacts and would invalidate the work this command just
   preserved.

<!-- speckit-companion:phase wrap-up -->
<!-- speckit-companion:node append-handoff -->
8. **Hand back to implement, and never write a status.** Report which story, requirements and tasks
   were added with their ids, and how many completed tasks were preserved. Then dispatch
   `speckit.companion.implement` to continue at the next unchecked task. Do not dispatch
   `speckit.companion.specify`, `plan` or `tasks`: each mints or regenerates, which destroys
   precisely what this command protects.

   Record the boundary with `--finish`, or with the `--batch` call above, and never with
   `--advance`. Append owns no status, so it must not move the pipeline; the step machine is the
   business of the step that is already running.

## Output

```text
Appended to <name> at <currentStep> (<status>).
Added: US<n> <title>
  FR-0xx <one line>
  T0xx <description>
Preserved: <k> completed tasks, ids unchanged.
Next: Continue implementation  →  dispatching speckit.companion.implement
```

## Guardrails

- Never mint a feature directory, and never edit a spec other than the active one.
- Never renumber or reword an existing story, requirement, criterion or task.
- Never run `specify`, `plan` or `tasks` to "refresh" the files this command just edited.
- A completed spec is not appended to. Send that case to `speckit.companion.specify`.

## Graceful Degradation

Best-effort throughout. If `python3` is missing, still make the edits and report that the capture
was skipped. If the feature cannot be resolved, stop with the reason. Never fail the host command.
