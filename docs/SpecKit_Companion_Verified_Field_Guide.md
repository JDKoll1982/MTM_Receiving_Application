FIELD GUIDE | SOURCE-LEVEL ACCURACY REVIEW 

# **Spec Kit + SpecKit Companion** 

A practical, verified guide to the pipeline, commands, skills, hooks, workflows and scripts 

PRINT EDITION | US LETTER | PORTRAIT 8.5 x 11 IN 

Verification snapshot: 17 November 2026 

Companion Spec Kit extension: 0.27.0 | Spec Kit minimum declared by the extension: 1.2.5.dev0 

Built from the supplied COMPANION-SCENARIOS.md and checked against the public Companion repository, its current extension files and scripts, the Companion documentation and the official Spec Kit extension documentation. 

Prepared as a readable working reference. It explains what to run, what happens automatically, what files are changed, and where the original guide needed updating. 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **HOW TO USE THIS GUIDE** 

## **What was checked - and what the verdict means** 

Read the short verdict first, then use the chapter and index that match the task in front of you. 

##### **Important scope note** 

This is a source-level review of the public repository and published documentation. The attached file did not include the user’s consuming project, its installed extension folder, local configuration, or a reproducible runtime environment. Therefore this guide can verify shipped public behavior, but it cannot certify project-specific settings or claim that every hook was executed successfully in the user’s environment. 

##### **Overall verdict: useful foundation, but it needs a revision** 

The uploaded guide gets the broad model right: Spec Kit creates the feature documents and Companion records what happened. The most important details about release 0.27.0 have changed, however. The replacement guide below uses the current extension manifest, workflow YAML and script behavior as the main authority, and flags places where the public website is inconsistent. 

|**Area**|**Review result**|**Plain-language finding**|
|---|---|---|
|Core model|Confirmed|Companion adds a run record and recovery/diagnostic tools around Spec Kit; it does<br>not replace the underlying authoring system.|
|Change sizing|Update needed|Sizing is performed before the spec is drafted. The result routes the workflow.|
|Simple changes|Update needed|A simple change folds plan and tasks into specify, then moves to implement.|
|Requirement tests|Update needed|Current specify instructions require one test task for each requirement.|
|Checklist file|Outdated|Current specify self-checks the spec in place; it does not create<br>checklists/requirements.md.|
|Hooks and events|Partly wrong|Lifecycle hooks and prompt/stop events are separate registrations.|
|Doctor|Partly wrong|Default mode is informational and returns 0; --strict can return 1, and --fix changes<br>workflow registration.|
|Converge and<br>completion|Needs care|Current shipped workflow ends with converge. A public introduction page describes<br>completion differently, so use installed workflow/script versions.|
|Local project claims|Not verifiable|The supplied file names project-specific specs, risk globs and branch/pointer state, but<br>those files were not supplied.|



#### **A simple rule for trusting this guide** 

For exact behavior, prefer the installed extension manifest, the generated command/skill instructions and the scripts in the version actually installed in your project. Use the website to understand the product, but check it against the shipped files when a page disagrees. Public documentation is useful, but it is not always updated in lockstep with the code. [R1-R5, R8-R10] 

#### **How source references work** 

Square references such as [R2] point to the source list at the end. GitHub links target the current public source files where possible. The original attachment is treated as the starting document, not as proof that its claims are correct. 

Plain-language reference - verify installed versions before relying on exact behavior 

1 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CONTENTS** 

### **Table of contents** 

|What was checked - and what the verdict means|1|
|---|---|
|The big picture|3|
|Two different Companion pieces|4|
|The current pipeline|5|
|How the size decision works|6|
|What specify, plan and tasks produce|7|
|Implementing work without losing the record|8|
|Converge and what “completed” means|9|
|All 19 Companion commands|10|
|Hooks: what happens automatically|12|
|Customizing the pipeline|13|
|Status, Resume and Doctor|14|
|The files and scripts behind the record|16|
|What should I run? A scenario guide|17|
|Bug fixes and other Spec Kit commands|18|
|Work that lands outside the spec's scope|19|
|Accuracy audit of the uploaded document|20|
|A practical operator checklist|22|
|Plain-language definitions|23|
|Find a topic quickly|25|
|Primary sources used in the review|26|



_Back matter: Glossary, index and source register follow the chapters._ 

Plain-language reference - verify installed versions before relying on exact behavior 

2 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 01** 

## **The big picture** 

What Spec Kit owns, what Companion adds, and how the two pieces fit together. 

#### **Spec Kit is the work process** 

Spec Kit is the system that turns a feature request into a written specification, a plan, a task list and working code. The normal steps are specify, plan, tasks and implement. The exact set of extra steps depends on the workflow and extensions installed. [R11] 

#### **Companion is the run record and the helper layer** 

SpecKit Companion sits alongside Spec Kit. Its extension adds commands and scripts that record which steps started and finished, track task progress, help resume interrupted work and check whether the record matches the files on disk. The VS Code extension is a separate user interface that reads the same project files and shows specs, progress and decisions. The two pieces can work separately; they share the spec folder rather than maintaining a second copy. [R8-R10] 

##### **Think of it this way** 

Spec Kit writes the work. Companion records the journey and helps inspect or continue it. The recording layer is useful, but it does not prove that the code is correct by itself. 

#### **The normal Companion workflow** 

For a typical feature, the current shipped workflow is: 

|**Stage**|**What happens**|
|---|---|
|1. Specify|Understand the request, judge its size and write the feature specification.|
|2. Route by size|A simple change skips separate plan and tasks commands because specify already created their lean<br>versions. Normal and oversized changes use the full sequence.|
|3. Plan|Decide how the work will be built and add design documents when needed.|
|4. Tasks|Break the work into ordered tasks and waves.|
|5. Implement|Build the work and record each completed task.|
|6. Converge|Compare what was built with the spec, plan and task list. If work is missing, add new tasks; a clean result<br>closes the run.|



The workflow file explicitly ends with the stock Spec Kit converge command, and the after-converge hook records the finish. [R2] 

Plain-language reference - verify installed versions before relying on exact behavior 

3 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 02** 

## **Two different Companion pieces** 

The word Companion can refer to the editor interface or to the extension installed in the Spec Kit project. 

#### **The VS Code extension** 

This is the part you see in the editor: the sidebar, the spec viewer, line comments, progress display and buttons for moving to the next step. The latest VS Code extension release listed on November 12, 2026 is 0.39.0. It is not the same version number as the Spec Kit extension. [R8, R9] 

#### **The Spec Kit extension** 

This is installed into the project through Spec Kit. It provides the Companion commands, workflow definition, command instructions and scripts. The current public manifest identifies it as version 0.27.0 and declares a minimum Spec Kit version of 1.2.5.dev0. [R1] 

|**Piece**|**Where it lives**|**What it provides**|
|---|---|---|
|VS Code extension|Your editor|Sidebar, viewer, progress and user controls.|
|Spec Kit extension|Your project, under|Commands, hooks, workflow and scripts that record and|
||.specify/extensions/companion/ after<br>installation|manage runs.|



#### **Where the source repository keeps the files** 

The public Companion repository moved the extension source to apps/speckit-extension/. That is the development/source location in GitHub. Once installed in a project, its runtime files are copied under .specify/extensions/companion/. Do not confuse the repository source path with the installed project path. [R1, R12] 

##### **Before copying a command name** 

The manifest uses canonical dotted IDs such as speckit.companion.specify. The name available in your AI tool can look different. For example, Claude Code may expose /speckit-companion-specify. Always use the command spelling registered by the agent integration in that project. [R3] 

Plain-language reference - verify installed versions before relying on exact behavior 

4 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 03** 

## **The current pipeline** 

The routing decision happens near the start. The size of the change determines how much planning is needed. 

#### **Simple changes** 

A simple change is one where the desired result and the files to change are already clear, the risks are contained, and another developer could continue from the request and the code difference alone. In this case, specify writes the specification plus a short plan and task list. The workflow then goes straight to implement. [R2-R4] 

#### **Normal changes** 

A normal change uses the full path: specify, review the spec, plan, review the plan, tasks, implement and converge. If the size cannot be read or the result is uncertain, the shipped workflow takes this full path rather than silently skipping steps. [R2] 

#### **Oversized changes** 

An oversized change also uses the full path. The workflow displays a warning and keeps both review gates. Being large does not grant permission to skip review. [R2] 

|**Size result**|**What happens next**|**Review pauses**|
|---|---|---|
|simple|Specify creates a short plan and task list; go to implement,<br>then converge.|No review gate in the shipped simple route.|
|normal|Plan, tasks, implement, then converge.|Review the spec and the plan.|
|oversized|Same full sequence, with a visible warning.|Review the spec and the plan.|
|missing or<br>unknown|Safely defaults to the full sequence.|Review the spec and the plan.|



##### **Why this matters** 

A small change does not have to produce three heavyweight planning documents. A large or uncertain change should not be rushed through a shortcut. The classifier chooses the route; the human still reviews the important outputs. 

The current workflow YAML explicitly maps the simple result to implement and sends every other or unknown value through the full path. [R2] 

Plain-language reference - verify installed versions before relying on exact behavior 

5 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 04** 

## **How the size decision works** 

The classifier looks at clarity and risk, not just the number of files. 

#### **All three tests must pass for simple** 

- **1** Clear and understood: the outcome is clear, the likely files and changes are known, and no major product or design decision is still open. 

- **2** Low risk: the change does not alter stored data or file formats, security or permissions, a contract used by others, concurrency behavior, releases or dependencies, or behavior that tests would not catch. 

- **3** Easy to continue: if the current work session stopped, another developer could finish from the original request and the code difference. 

The guide gives approximate size markers: up to about five files and ten tasks can still be simple when all three tests pass. More than about ten files or twenty tasks, or a change spanning several subsystems, points toward oversized. These are guideposts, not automatic rules. A tiny permission change can still be normal because it is risky. [R3] 

#### **The decision is made before the spec is drafted** 

The current specify node order is: resolve the feature folder, classify the size, save the size result, draft the spec, quality-check it, create the branch and finish the step. This is an important correction to the attached file, which described classification as happening after spec creation. [R3, R4] 

#### **The three labels in everyday language** 

|**Label**|**Meaning**|**Typical response**|
|---|---|---|
|simple|Clear, contained and easy to resume.|Use the short route; do not re-run plan or tasks after specify has<br>already created them.|
|normal|Needs ordinary planning or has one or more<br>risks.|Use full planning and review.|
|oversized|Large enough to span several areas or exceed<br>a comfortable single run.|Use the full process and expect review checkpoints.|



##### **Do not use file count as the only test** 

One line that changes access permissions can have higher risk than a 12-file text-only update. The classifier is designed to consider impact, not just size. 

Plain-language reference - verify installed versions before relying on exact behavior 

6 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 05** 

## **What specify, plan and tasks produce** 

These documents are different views of the same change: the need, the design and the work list. 

#### **Specify: describe the need** 

The spec should explain what a user needs and why. It contains prioritized user stories, acceptance scenarios, edge cases, testable functional requirements and measurable success criteria. It should record assumptions and preserve exact names or settings that the user requires. It should not turn into an implementation recipe too early. [R3] 

#### **A test task for every requirement** 

A key change in version 0.27.0 is that the task list plans one test for each requirement. The test should exercise the feature through the visible screen or public interface under the condition named in the requirement. This connects the promise in the spec to something that can be checked. [R8] 

#### **The old requirements checklist file is no longer created** 

Current specify instructions say to review the spec once, fix it in place and keep genuine open questions visible. They do not write checklists/requirements.md. The attached guide did not emphasize that file, but any related older instructions should be updated. [R3, R8] 

#### **Plan: explain how the work fits together** 

For a normal or oversized change, plan writes the overall technical approach and adds design information when needed, such as research, a data model and interface contracts. The amount of detail should match the size and risk of the change. A simple change gets a short plan from specify instead. [R2-R4] 

#### **Tasks: make the work buildable** 

Tasks are grouped into ordered waves. A wave is a group of tasks that can be done before the next group begins. Dependencies are respected, and a file should have one owning story phase so two workers are not independently editing the same file. [R9] 

##### **Simple-change warning** 

When specify says the change is simple, it has already created the short plan and task list and recorded that those steps are complete. Do not run plan or tasks again; that can overwrite the lean files and waste the shortcut. [R2-R4] 

Plain-language reference - verify installed versions before relying on exact behavior 

7 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 06** 

## **Implementing work without losing the record** 

Implementation is where code changes, tests run and completed tasks are recorded. 

#### **Implement follows the task order** 

Implement works through the dependency-ordered task list. It should not treat a checked box as proof that the code works; the task should be built and its named checks should run. The after-implement recording step synchronizes progress from tasks.md. [R1, R6] 

#### **When work is split across workers** 

The current Companion guide uses workers for substantial independent work. The published overview describes sending large story phases to workers when a phase owns five or more files, while foundational work is grouped into waves. Smaller work stays with the main agent because starting a worker can cost more than doing the task. [R9] 

#### **Two workers must not fold the same run record at the same time** 

Workers can append their individual task-finish events to the shared event log. The main agent then folds those events into the main run record with the materialize command, one at a time. This is important because updating the final record means reading it, changing it and writing it back. Two simultaneous updates can overwrite each other. [R6] 

|**Safe pattern**|**Why**|
|---|---|
|Workers append their own task-finish events.|Each worker writes a separate event rather than editing the shared main record.|
|The main agent materializes one at a time.|Only one process performs the shared read-change-write operation.|
|Run a final materialize as a backstop.|It folds any remaining recorded events into the main record.|
|Do not run two implementations against one<br>spec at once.|Two runs can both read the same record and one can erase the other update.|



#### **Keep each file with one owner** 

The simplest way to prevent merge conflicts is to give each file one owner during a run. If different workers need the same file, the task list should make one worker responsible for that file and have others provide changes or requirements to that owner. [R9] 

##### **What the progress record proves - and what it does not** 

The record can show that a task was marked finished and that a test or check was recorded. It cannot replace reading the code, reviewing the test quality or checking that the test covers the real requirement. 

Plain-language reference - verify installed versions before relying on exact behavior 

8 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

**CHAPTER 07** 

## **Converge and what “completed” means** 

Converge is the final comparison between what the feature promised and what was actually built. 

#### **What Converge checks** 

Converge compares the implementation with the spec, plan and task list. If it finds missing work, it appends new tasks in a new Convergence phase rather than rewriting old tasks. If nothing is missing, it reports that the work has converged. [R2, R8] 

#### **Current Companion workflow: converge is the final step** 

The current shipped workflow YAML includes the stock speckit.converge command as its terminal step. The after-converge hook records the finish, and the current release notes say specs finish at Completed. The auto command also states that implement itself stops at Implemented and converge is what completes the run. [R2, R4, R8] 

#### **Auto gets one correction round, not an endless loop** 

If the first converge appends tasks, Auto builds those appended tasks once and runs converge again. If that second converge still finds open work, Auto records a concern and stops at Implemented. It does not run a third pass and it must not dispatch mark-complete to hide the remaining work. [R4] 

##### **Important public-doc conflict** 

The current Companion changelog, installed workflow YAML and Auto command say converge completes the Companion workflow. However, the public Introduction page still says implement marks Completed and describes converge as optional and status-neutral. Those statements disagree. For a project using extension 0.27.0, follow the installed workflow and command instructions, and check the exact installed version before relying on the web page. [R2, R4, R8, R9] 

#### **Do not reopen a completed spec for unrelated new work** 

A completed spec should remain a truthful record of the work it covered. A genuinely new feature or a later change should normally get a new spec folder and link back to the earlier work where useful. That keeps the history understandable. A repair that belongs to the same change is different from a new request; check the current project’s supported reactivation behavior before deciding. [R8, R10] 

#### **Manual completion is a recovery action** 

The mark-complete command is a manual recovery tool, not a routine shortcut for skipping checks. The run record’s completion writer protects status changes and may record a concern when completion is forced without evidence of verification. Use it only when the normal completion hook could not finish and the work has actually been reviewed. [R7] 

Plain-language reference - verify installed versions before relying on exact behavior 

9 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 08** 

## **All 19 Companion commands** 

A command inventory, grouped by the job it performs. 

The 0.27.0 extension manifest lists 19 command entries. Some are meant for the person or agent to run; others are called automatically by Spec Kit. The canonical IDs below use dotted names. Your AI tool may show them with dashes or a slash prefix. [R1, R3] 

#### **A. The four authoring commands** 

|**Command ID**|**Use it for**|
|---|---|
|speckit.companion.specify|Start a new feature request and write spec.md. It also classifies the change.|
|speckit.companion.plan|Write the plan and any needed design documents.|
|speckit.companion.tasks|Create the wave-ordered task list.|
|speckit.companion.implement|Build the tasks in order and record progress.|



#### **B. The two run-control commands** 

|**Command ID**|**Use it for**|
|---|---|
|speckit.companion.status|Read where the active spec stands without changing it.|
|speckit.companion.resume|Find the next step and dispatch it. Mid-implementation, continue with the next<br>unchecked task.|



#### **C. The three diagnosis and size commands** 

|**Command ID**|**Use it for**|
|---|---|
|speckit.companion.classify|Print the size result for the active spec.|
|speckit.companion.doctor|Recheck the run record and report mismatches or missing evidence.|
|speckit.companion.mark-complete|Manually set the spec to Completed as a recovery action.|



#### **D. Hands-off command** 

|**Command ID**|**Use it for**|
|---|---|
|speckit.companion.auto|Run the Companion pipeline unattended, with the size-based route and one<br>correction round.|



Plain-language reference - verify installed versions before relying on exact behavior 

10 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

#### **E. Seven Spec Kit lifecycle hook commands** 

|**Command ID**|**When it runs**|
|---|---|
|speckit.companion.before-step|Before specify, plan, tasks and implement; records the step starting.|
|speckit.companion.after-specify|After specify; records the specify result.|
|speckit.companion.after-plan|After plan; records the plan finish.|
|speckit.companion.after-tasks|After tasks; records the task-list finish.|
|speckit.companion.after-implement|After implement; syncs task progress from tasks.md.|
|speckit.companion.before-converge|Before converge; records the start.|
|speckit.companion.after-converge|After converge; records the result and performs the completion path.|



#### **F. Two event commands** 

|**Command ID**|**When it runs**|
|---|---|
|speckit.companion.on-prompt|On Spec Kit’s user_prompt_submit event.|
|speckit.companion.on-stop|On Spec Kit’s stop event.|



The command names, purpose descriptions, lifecycle hook entries and separate prompt/stop event registrations are all visible in extension.yml. [R1] 

Plain-language reference - verify installed versions before relying on exact behavior 

11 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 09** 

## **Hooks: what happens automatically** 

There are separate systems for whole-step hooks, runtime events and custom Companion hooks inside a step. 

#### **1. Spec Kit lifecycle hooks** 

These are attached to whole steps such as specify, plan, tasks, implement and converge. The Companion extension manifest registers them under its hooks: section. The registered Companion lifecycle hooks are marked optional: false, meaning they are required for the declared lifecycle point when the extension is installed and the host follows Spec Kit’s hook mechanism. [R1, R11] 

#### **2. Prompt and stop events** 

The user_prompt_submit and stop handlers are registered separately under events: in extension.yml. They are not additional before/after step hooks in .specify/extensions.yml. Each has a ten-second timeout. The original guide put these in the same list as the lifecycle hooks; that wording should be corrected. [R1] 

|**System**|**Configured in**|**What it responds to**|
|---|---|---|
|Spec Kit lifecycle hooks|.specify/extensions.yml, with handlers<br>supplied by the extension|Before or after an entire step.|
|Prompt/stop events|Extension manifest, under events:|A user submits a prompt or an agent turn ends.|
|Companion node hooks|.specify/companion.yml|Before or after a node, phase or whole step inside a<br>Companion command.|



#### **What the word “automatic” should mean** 

A hook can be configured to run automatically, but its reliability depends on the host integration, its installation and the exact event mechanism. A prompt/stop event is not proof that every step boundary has been captured. Use Doctor to identify gaps in the record; do not assume the record is complete simply because the hook is configured. [R1, R5] 

##### **The crucial correction** 

The seven before/after lifecycle handlers and the two prompt/stop events are all real extension commands. But they are not one single list of hooks, and they are not all configured in the same file. 

Plain-language reference - verify installed versions before relying on exact behavior 

12 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 10** 

## **Customizing the pipeline** 

Use Companion configuration to add team-specific steps, checks and instructions - but understand what is truly enforced. 

#### **The configuration file** 

Project-level Companion changes are described in .specify/companion.yml and the files beside it. If the file is absent, the shipped defaults run. The Workflow Builder edits these files and a build writes the generated command files that the AI assistant reads. Preview build lists changes without writing them. [R10] 

#### **Four kinds of custom hook** 

|**Type**|**Plain meaning**|**Example**|
|---|---|---|
|skill|Ask the assistant to load an existing skill.|Use the team’s code-review skill before implement ends.|
|prompt|Give the assistant an instruction at that<br>point.|Ask it to confirm the changelog was updated.|
|command|Run a shell command, when a terminal is<br>available.|Run npm test.|
|node|Load a reusable instruction file.|Run a shared security-review instruction.|



#### **Hooks attach to a place in the step** 

A custom hook can run before or after an individual node, a phase (a group of nodes), or an entire step. The hook runs at the point where it is attached. A missing anchor is warned about and skipped; a referenced node file that does not exist is a real configuration error. [R10] 

#### **A custom hook is not automatically a hard safety gate** 

This is one of the most important limitations in the current documentation: nothing independently executes companion.yml. The assistant reads it while carrying out the step. Therefore a prompt or command written there is not the same as a hard rule enforced by the build system. For a check that must never be skipped, put it in Continuous Integration (CI) or another system that can actually block a merge or release. [R10] 

#### **Background hooks** 

A hook can run in the background, which is useful for tests or notifications that do not need to block the assistant. Do not background a hook that writes .spec-context.json or otherwise changes the shared run record. Two overlapping reads and writes can lose an update. [R10] 

#### **Configuration changes must be built** 

Changing .specify/companion.yml does not automatically refresh the instructions the assistant uses. Build the workflow after editing. The build writes generated commands under .specify/extensions/companion/commands; do not edit those generated files by hand because a later build can replace them. [R10] 

Plain-language reference - verify installed versions before relying on exact behavior 

13 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 11** 

## **Status, Resume and Doctor** 

These three commands answer different questions: where am I, what is next, and does the run record make sense? 

#### **Status: show me where I am** 

Status is the safe first command when returning to a run. It reads the current spec, status, decisions and likely next action. It does not dispatch the next step. If the record file is missing or malformed, status can derive a best-effort state from the spec documents on disk. [R12] 

#### **Resume: carry on from here** 

Resume resolves the next action and dispatches it. During implement, it continues from the next unchecked task. If the pipeline is already complete, it reports that there is nothing to resume. If there is no usable spec to continue, it reports that there is nothing to resume. [R12] 

#### **Doctor: check whether the record matches the work** 

Doctor reads the run record, documents and trace, then recomputes its findings instead of trusting the previous status. It separates a clean result from a check that could not run. If one check crashes, the other checks can still run. [R5] 

#### **What Doctor checks** 

|**Check area**|**Plain-language question**|
|---|---|
|Record and triage|Did steps start and finish in a sensible order? Do the saved records agree?|
|Bleed|Did one step do work that belongs to another step?|
|Completion and verification|Was the run marked complete correctly? Was any actual check recorded?|
|Artifacts and template|Did the promised files appear? Does the task list still follow the expected structure?|
|Built commands and agents|Are generated instructions and required agent command files present?|
|Workflow and numbering|Is the workflow registered? Are spec or task numbers duplicated?|
|Retired settings and<br>persistence|Are old settings still present? Does the record match the current history model?|
|Dispatch and trace|Were capture commands called correctly? Did they fail or repeatedly rewrite files?|
|Chat (optional)|What does the session transcript show about retries, stops and wasted work?|



The current doctor.py lists 16 check names, with chat as an optional deeper check. Some checks may be skipped when there is no evidence to examine. A skipped check is not the same as a clean check. [R5] 

#### **Three important Doctor options** 

|**Option**|**What it changes**|**Safe description**|
|---|---|---|
|--json|Formats the report for another program.|Useful for automation or dashboards.|
|--strict|Returns a non-zero exit code when problem-level<br>findings exist.|Can be used as a gate in a scripted process.|
|--fix|Registers the shipped Companion workflow with<br>Spec Kit, then exits.|This changes workflow registration; it is not read-only.|



Plain-language reference - verify installed versions before relying on exact behavior 

14 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

##### **Correction to the attached guide** 

Doctor’s default diagnostic mode is designed not to halt work and normally returns exit code 0. The original statement that it “always” exits 0 is too broad: --strict can return 1. Also, --fix performs a change by registering the workflow, so do not describe that mode as read-only. [R5] 

#### **A limit on --all** 

The current --all implementation examines directories under specs/ that contain .spec-context.json. It is not a universal scan of every folder that might contain an old specification. For a particular legacy spec or a spec outside the standard specs/ location, point Doctor at it explicitly with --feature-dir. [R5] 

#### **How to read a finding** 

A finding is something the Doctor could verify and wants you to review. A skipped check means it did not have enough evidence or could not run; it is not a pass. Read the reason shown beside the check before deciding what to do next. [R5] 

#### **A safe troubleshooting order** 

- **1** Start with Status. Confirm which spec folder is active and what Companion believes the next step should be. 

- **2** Run Doctor. Use the default report first. Do not use --fix unless workflow registration is the problem you intend to change. 

- **3** Check the underlying files. Compare the task list and spec documents with the record. Use the approved writer script for supported repairs; do not edit the record JSON directly. 

- **4** Resume only after you understand the finding. If more than one spec is active, pass --feature-dir so the command targets the correct folder. 

##### **Remember** 

Doctor checks whether the run was recorded consistently. It does not replace running the project's tests, reviewing the code or proving that the feature works for users. 

Plain-language reference - verify installed versions before relying on exact behavior 

15 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 12** 

## **The files and scripts behind the record** 

This chapter explains the small set of files that matter most when something looks wrong. 

#### **.spec-context.json: the main run record** 

This file lives inside the feature folder. It stores the current status, step history, recorded decisions and task progress data used by Companion. Treat it as a record maintained by the supplied writer scripts, not as a normal document to edit by hand. A manual edit can make the record disagree with the history or the task file. [R7] 

#### **write-context.py: the approved writer** 

Use write-context.py to record a step start or finish, move the run forward, close tasks, materialize worker events, or perform a supported manual completion. The script has protections against invalid status movement and manages the run history. Avoid direct edits to .spec-context.json. [R7] 

#### **task_sync.py: keeping task checkboxes and events aligned** 

The task synchronizer links finished-task events with the task list. It helps ensure that task completion is recorded as more than a checked box. This is also why task journaling after the fact can cause timing information to be less trustworthy than the task summary. [R6] 

#### **spec_context.py: finding the right feature folder** 

The record helpers resolve the active spec using an explicit --feature-dir first, then environment settings, the project pointer file and, where applicable, the branch name. If more than one spec is being worked on, pass --feature-dir explicitly so an operation cannot accidentally write to the wrong spec. [R7] 

#### **Other useful scripts** 

|**Script**|**What it is for**|
|---|---|
|mint-spec-dir.py|Creates a new feature folder using the project’s numbering rules and helps avoid assigning<br>the same number twice.|
|status-context.py|Reads selected information from the current run record, including the stored size result<br>used by the workflow.|
|dispatch-briefs.py|Builds the worker briefs for the next task wave so work is dispatched consistently.|
|doctor.py|Runs the health checks and formats the findings.|



#### **What if Python is missing?** 

The extension manifest declares python3 as not required for installation. That does not mean recording works without it. The command instructions warn and carry on when Python is unavailable, which means the Spec Kit step may continue while Companion capture is skipped. On Windows, check whether the working command is python or python3 and use the path expected by the generated script instructions. [R1, R3-R5] 

##### **Simple file-safety rules** 

Do not hand-edit .spec-context.json. Do not run two writers against the same spec at the same time. Do not let workers materialize the shared record. Do use --feature-dir when a command could target more than one spec. 

Plain-language reference - verify installed versions before relying on exact behavior 

16 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 13** 

## **What should I run? A scenario guide** 

The fastest way to choose a command is to identify the problem you are trying to solve. 

|**Situation**|**What to use**|**Important note**|
|---|---|---|
|I have a new feature and want<br>to review each stage.|speckit.companion.specify, then<br>plan/tasks/implement as routed, then<br>converge.|Use the manual path and review the<br>documents at each checkpoint.|
|I want the full run with no<br>manual pauses.|speckit.companion.auto.|It creates a fresh spec, routes by size and skips<br>human pauses. It has one correction round<br>only.|
|I want approval before<br>planning.|specify workflow run speckit-companion.|The normal/oversized routes include spec and<br>plan review gates.|
|I have returned after a break.|speckit.companion.status.|Read the current position before dispatching<br>anything.|
|I know the next action and<br>want to continue.|speckit.companion.resume.|It works out the next step and dispatches it.|
|I am unsure whether a change<br>is small enough.|speckit.companion.classify on an existing<br>spec, or specify for new work.|For a new change, specify makes the decision<br>early and records it.|
|Implement stopped part-way<br>through tasks.|speckit.companion.resume.|Continue at the next unchecked task. Do not<br>regenerate tasks.md.|
|Converge added missing work.|Implement the appended tasks, then run<br>converge again.|For Auto, follow its one-correction-round rule.|
|The progress display looks<br>inconsistent.|speckit.companion.doctor.|Use the report to tell a capture issue from a<br>display issue.|
|I need a machine-readable<br>report.|speckit.companion.doctor --json.|Use --strict only when a failing exit code should<br>block the caller.|
|The current workflow is not<br>registered.|speckit.companion.doctor --fix.|This is a mutating repair that registers the<br>shipped workflow.|
|I need to mark a legitimately<br>finished run complete after a<br>failed hook.|speckit.companion.mark-complete.|Treat as a recovery step, not a way to bypass<br>tests or converge.|
|A user reported a broken<br>behavior.|Use the separate Spec Kit bug<br>assessment/fix/test flow if installed.|If the work is actually a new capability, create a<br>feature spec instead.|
|I need to edit how a pipeline<br>step is structured.|Use Workflow Builder or<br>.specify/companion.yml, then build.|Put checks that must never be skipped in CI.|
|Two specs are active in the<br>same repository.|Pass --feature-dir explicitly to any script or<br>command that could choose the wrong one.|Do not rely on the single active-spec pointer for<br>ambiguous work.|



##### **Safest starting point when unsure** 

Run Status first. It is read-only and gives a baseline. If the status seems inconsistent with the files, run Doctor next. Use Resume only when you are ready for the next step to be dispatched. 

Plain-language reference - verify installed versions before relying on exact behavior 

17 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 14** 

## **Bug fixes and other Spec Kit commands** 

Not every task belongs in the feature pipeline, and not every command in the original quick reference is still the right one. 

#### **Bug assessment is a separate flow** 

The bug assessment, bug fix and bug test commands are a separate Spec Kit extension flow. They are not the same thing as the Companion specify-plan-tasks-implement-converge workflow. The Companion VS Code interface can still display bugfix runs; the November 8, 2026 changelog says the Bugs pane reads Spec Kit bugfix runs and shows their step timing. [R8] 

#### **When a bug is really a feature** 

If the reported problem turns out to require a new capability, stop treating it as a small repair and write a feature spec. If the bug fix changes behavior that an existing spec promised, keep the spec and code aligned: correct the documented requirement as part of the same work rather than silently changing the code only. 

#### **Other useful Spec Kit commands** 

|**Command or capability**|**When it helps**|
|---|---|
|clarify|Ask focused questions where the specification has unresolved ambiguity.|
|analyze|Check whether the spec, plan and tasks agree with each other.|
|checklist|Create or review a requirements checklist when that command is part of the installed Spec Kit<br>setup. This is different from the removed Companion specify checklist file.|
|GitHub task-to-issues|Create GitHub issues from the task list when the GitHub extension is installed and configured.|



#### **Task-to-issues command name has changed in current Spec Kit docs** 

The uploaded quick-reference table lists taskstoissues as if it were a generic built-in command. Current Spec Kit documentation for version 1.4 says to install the GitHub extension and use /speckit-github-taskstoissues in GitHub Copilot chat. Do not assume the older bare taskstoissues name is present in every project. [R11] 

##### **Do not force every small fix through a feature process** 

The right tool depends on what the work is. A quick, isolated repair may belong in a bug-fix flow. A new capability, a risky change or work that needs a clear requirements record belongs in a feature spec. Use the workflow that creates the evidence you actually need. 

Plain-language reference - verify installed versions before relying on exact behavior 

18 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 15** 

## **Work that lands outside the spec's scope** 

What to do when a branch, a working tree or a conversation has already changed things the active spec never asked for. 

#### **Sort the changes before you edit any document** 

Every unplanned change fits one of three buckets, and each bucket has a different correct home. 

|**Bucket**|**What it looks like**|**Where it belongs**|
|---|---|---|
|In scope, not yet built|The spec promised it and the task list does not cover it|Converge appends a Convergence task; implement it, then converge again. [R2, R8]|
|A correction to something the spec promised|The spec describes one behavior and the correct behavior is another|Amend the spec text and the code in the same change, so the document stops describing the old behavior.|
|Genuinely new or separate work|A different feature, module or user need|A new spec folder that links back to this one. Do not bury new work in a finished spec. [R8, R10]|

#### **Amending the spec is a document edit, and that is allowed** 

Rewrite spec.md, plan.md or tasks.md so they describe what was really built. That is an ordinary document edit. Rewriting .spec-context.json to say the same thing is not: the record is maintained by the writer scripts, and a hand edit leaves it disagreeing with its own history. Change the documents, then let the capture commands record the change. [R7] 

#### **Record the change honestly** 

- Say in the spec what changed and why, in the words a reader would use. 
- Do not tick a checkbox for work that was never dispatched. The task journal is what makes a tick more than a checked box, and back-filling it destroys that evidence. [R6] 
- If the change shipped without a task, add the task as complete with a one-line note that it was folded in, rather than leaving it out silently. 
- If the change reversed an earlier decision, record the reversal where decisions live, not only in a code comment. 

#### **Expect Doctor to notice, and read it as a prompt rather than a verdict** 

Doctor's bleed check asks whether one step did work that belongs to another step, and unplanned changes trip it often. The finding asks you to review; it is not proof that something is broken. Close it by making the documents match the code, not by editing the record. [R5] 

#### **A finished spec is a record, not a folder that keeps growing** 

A spec marked Completed should stay a truthful account of the work it covered. A repair that belongs to the same change can stay with it. A new capability belongs in a new spec, so each history stays readable and each one can be verified on its own. Some projects install a local command that appends scope to a spec that is still in flight. Use it while the spec is still open; leave a completed record alone. [R8, R10] 

Plain-language reference - verify installed versions before relying on exact behavior 

19 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 16** 

## **Accuracy audit of the uploaded document** 

A direct list of what was confirmed, what should be rewritten and what could not be checked from the supplied material. 

#### **Claims that are broadly correct** 

- Companion records the progress of a Spec Kit run; it is an extension layer rather than a replacement for Spec Kit. 

- The four normal authoring steps are specify, plan, tasks and implement; Converge checks whether the build matches the documents. 

- A simple change can fold plan and tasks into specify and proceed to implement. 

- The current extension provides 19 command entries, including automatic lifecycle and event handlers. 

- Status is read-only; Resume dispatches the next step; Doctor recomputes findings from files rather than trusting a prior report. 

- Do not hand-edit .spec-context.json; use the supported writer scripts. 

- Workers should append their task-finish events, while the main agent materializes the shared record one at a time. 

- Two concurrent runs against the same spec can overwrite the shared run record and should be avoided. 

Plain-language reference - verify installed versions before relying on exact behavior 

20 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

#### **Claims that need correction or more exact wording** 

|**Topic**|**Original guide**|**Verified update**|
|---|---|---|
|Classification<br>timing|The source says the size decision follows<br>spec creation.|It now happens before spec drafting and is saved for later<br>routing.|
|Requirements<br>checklist|The source omits the latest change.|Current specify self-checks the spec in place and does not<br>write checklists/requirements.md.|
|Test coverage|The source does not explain this newer<br>requirement.|Every requirement should get a corresponding test task.|
|Hook registration|The source lists prompt/stop alongside<br>lifecycle hooks under extensions.yml.|The seven lifecycle hooks are in the hooks section;<br>user_prompt_submit and stop are separate events in<br>extension.yml.|
|Doctor exit code|“Always exits 0” is too broad.|Default mode returns 0; --strict may return 1. --fix registers<br>the workflow and is not read-only.|
|Doctor --all|The source implies a repository-wide<br>complete sweep.|Current code checks specs/ folders that have<br>.spec-context.json; use --feature-dir for a specific legacy<br>folder.|
|Completion<br>model|The source presents after-converge and<br>mark-complete without noting public-doc<br>conflict.|Current workflow YAML ends with converge; use installed<br>workflow/command files over conflicting overview prose.|
|Auto retries|The source says there is never a third pass,<br>which is correct, but needs a clear<br>explanation.|The command allows one correction round: after a second<br>converge still leaves work, it records a concern and stops.|
|Command<br>spelling|The source uses dash-style names<br>throughout.|Canonical IDs are dotted; the available spelling depends on<br>the host integration.|
|Custom hooks|The source describes hooks as<br>configuration but may imply enforcement.|The assistant reads companion.yml; a must-not-skip check<br>belongs in CI.|
|Task-to-issues|The quick-reference name is too generic for<br>current Spec Kit.|Current docs say use the GitHub extension command<br>/speckit-github-taskstoissues when installed.|
|Completed specs|The source says a completed spec should<br>not be reopened, without nuance.|A new change should get a new spec; same-change<br>recovery/reactivation is a separate supported case to check<br>in the installed version.|



#### **Public documentation conflict that must remain visible** 

The current 0.27.0 changelog, workflow YAML and Auto command say the Companion workflow reaches Completed after Converge. The public Introduction page instead says Implement marks Completed and Converge is optional and does not change status. This is not a wording preference; it changes which command a user should run. The safe resolution is to follow the installed extension version and its generated workflow/command files, and verify them locally when the project behaves differently. [R2, R4, R8, R9] 

#### **Project-specific statements not verifiable from the attachment** 

What could not be verified: The attachment makes claims about the consuming repository, including specs 000-011, later phases in spec 003, an empty commands block in .specify/companion.yml, MTM-specific risk globs and a possible mismatch between the active spec pointer and branch. Only COMPANION-SCENARIOS.md was supplied, not that repository, so these points remain unverified. Audit conclusion: The guide explains the intended operating model but should not be used unchanged as a current procedure. Before adopting it, compare the installed extension and local .specify configuration with the exact version being run. 

Plain-language reference - verify installed versions before relying on exact behavior 

21 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **CHAPTER 17** 

## **A practical operator checklist** 

Use this short sequence to keep a run easy to understand and safe to recover. 

#### **Before starting** 

- **1** Confirm the installed Spec Kit and Companion versions. The public Companion Spec Kit extension manifest checked for this guide is 0.27.0 and requires Spec Kit >=1.2.5.dev0. 

- **2** Make sure Python is available under the name the project scripts expect. 

- **3** Decide whether this is a new feature, a bug repair or a very small change that does not need a spec-driven run. 

- **4** For more than one active spec, identify the exact feature folder and pass --feature-dir when the script supports it. 

#### **While working** 

- **1** Let specify classify the change before drafting. Review the spec before planning when the workflow offers that gate. 

- **2** For a simple change, do not rerun plan or tasks after specify has folded them. 

- **3** Check that each requirement has a corresponding test task. 

- **4** Keep file ownership clear when workers are used. Have workers append task-finish events; let the main agent materialize the shared record one at a time. 

- **5** Do not run two pipelines against the same spec at the same time. 

#### **Before calling it done** 

- **1** Run the requested tests and checks; do not rely only on checked task boxes. 

- **2** Run Converge when using the current Companion workflow. Build newly appended tasks and follow the current Auto correction limit. 

- **3** Run Doctor if the displayed status, task list or run record does not make sense. 

- **4** Use mark-complete only as a genuine recovery action, not as a shortcut around missing work. 

- **5** Keep the project-specific configuration and release version alongside this guide so future readers know which behavior was installed. 

##### **A safe default order** 

Status to see where you are. Doctor to diagnose inconsistencies. Resume to continue when you are ready to dispatch the next step. 

Plain-language reference - verify installed versions before relying on exact behavior 

22 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **GLOSSARY** 

## **Plain-language definitions** 

Terms are defined the way they are used in this guide. 

###### **Acceptance scenario** 

A short example that shows what should happen in a given situation. 

###### **Agent integration** 

The connection between Spec Kit commands and an AI tool such as Claude Code or GitHub Copilot. 

###### **Artifact** 

A file created or updated by a step, such as spec.md, plan.md or tasks.md. 

###### **Auto** 

The Companion command that runs the feature workflow without pausing for normal human review gates. 

###### **Branch** 

A separate line of work in Git, used to keep a change isolated from other work. 

###### **CI (Continuous Integration)** 

An automated system that runs checks when code is pushed or a pull request is opened. It can block a change when a required check fails. 

###### **Command ID** 

The official internal name of a command. A host tool may display a different spelling. 

###### **Converge** 

The final comparison of the built code with the spec, plan and tasks. Missing work is added as new tasks. 

###### **Feature folder** 

The directory that contains the documents and run record for one feature. 

###### **Hook** 

An instruction or command attached to a point in a step so it can run before or after that point. 

###### **Lifecycle hook** 

A hook that runs before or after a whole Spec Kit step. 

###### **Materialize** 

Fold worker task-finish events into the main run record. 

###### **Node** 

One part of a command’s instructions, such as classifying size or drafting the spec. 

###### **Node hook** 

A custom instruction attached before or after a single node, a phase or a step. 

###### **Phase** 

A named group of nodes within a step. 

###### **Requirement** 

A testable statement of what the feature must do. 

###### **Run record** 

The data file that tracks step history, status, decisions and task progress. 

###### **Simple / normal / oversized** 

The three size results used to choose the amount of planning and review. 

###### **Spec** 

The written description of what users need and why. 

###### **Spec Kit** 

The framework that guides specification, planning, task creation and implementation. 

###### **Task** 

A specific piece of work in tasks.md, normally with an owner and a way to check it. 

Plain-language reference - verify installed versions before relying on exact behavior 

23 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **Task journal** 

The recorded event that says a task was completed and captures supporting details. 

###### **Unattended** 

A run setting used by Auto to record checkpoints and continue rather than pause for a person. 

###### **Verification** 

Evidence that a requirement or task was checked, for example by running a test. 

###### **Wave** 

A group of tasks that can be finished before the next dependency group begins. 

###### **Workflow** 

The ordered sequence of steps, gates and routing rules used for a run. 

_Tip: When a technical term appears in a command or a file name, keep its spelling exactly as shown even when the explanation uses simpler words._ 

Plain-language reference - verify installed versions before relying on exact behavior 

24 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **INDEX** 

## **Find a topic quickly** 

The index points to printed page numbers. Related entries are grouped under the same topic where useful. 

|**acceptance scenario**|23|**agent integration**|23|
|---|---|---|---|
|**artifact**|23|**audit findings**|21|
|**auto**|23|**auto command**|11|
|**background hook**|13|**branch**|23|
|**build**|13|**CI**|13|
|**ci (continuous integration)**|23|**command id**|23|
|**commands**|11|**Companion extension**|3|
|**companion.yml**|13|**completed status**|9|
|**converge**|9, 23|**convergence tasks**|9|
|**doctor command**|11|**feature folder**|23|
|**hook**|23|**implemented status**|9|
|**lifecycle hook**|23|**mark-complete**|9|
|**materialize**|23|**node**|23|
|**node hook**|13, 23|**normal change**|5|
|**not verified**|21|**oversized change**|5|
|**phase**|23|**project-specific configuration**|21|
|**public documentation conflict**|21|**requirement**|23|
|**resume command**|11|**review gate**|5|
|**run record**|23|**simple / normal / oversized**|23|
|**simple change**|5|**spec**|23|
|**Spec Kit**|3, 23|**status command**|11|
|**task**|23|**task journal**|24|
|**unattended**|24|**verification**|24|
|**wave**|24|**workflow**|3, 24|**out-of-scope change**|19|**scope change, mid-run**|19|



Plain-language reference - verify installed versions before relying on exact behavior 

25 

**SPEC KIT + COMPANION  |  VERIFIED FIELD GUIDE** 

Checked 17 November 2026 

###### **SOURCE REGISTER** 

## **Primary sources used in the review** 

All source checks below were made on 17 November 2026. The Companion code links point to the public main branch; main can change after this date. For a permanent audit trail, save the commit hash or tag used by your project when you install the extension. 

###### **[R1] Companion Spec Kit extension manifest - version 0.27.0; declared Spec Kit minimum; all 19 command entries; lifecycle hooks and separate events.** 

https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/extension.yml 

**[R2] Shipped Companion workflow - size routing, review gates, simple fast path and final converge step.** https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/workflows/speckit-companion.workflow.yml 

**[R3] Current Companion specify command - size rules, order of operations, specification contents and in-place quality check.** 

https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/commands/speckit.companion.specify.md 

**[R4] Current Companion Auto command - unattended flow, correction round, one-shot environment behavior and completion handling.** 

https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/commands/speckit.companion.auto.md 

###### **[R5] Doctor script - checks, default behavior, --all, --strict and --fix options.** 

https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/scripts/doctor.py 

**[R6] Task synchronization script - task-finish events and materialization behavior.** 

https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/scripts/task_sync.py 

###### **[R7] Run-record writer - status changes, history, safe writes and supported recording operations.** 

https://raw.githubusercontent.com/alfredoperez/speckit-companion/main/apps/speckit-extension/scripts/write-context.py 

**[R8] Companion changelog - Spec Kit extension 0.27.0 and VS Code extension 0.39.0 release notes, dated November 12, 2026.** 

https://speckit-companion.dev/changelog/ 

###### **[R9] Companion Introduction - product overview and one-run explanation. It contains a completion description that conflicts with the current 0.27.0 workflow and changelog.** 

https://speckit-companion.dev/docs/ 

**[R10] Companion Workflow Builder and hooks - custom hook types, build behavior, configuration files and the CI enforcement limitation.** 

https://speckit-companion.dev/docs/customize/workflow-builder/ 

###### **[R11] Official Spec Kit Extensions Overview - how extensions, commands, hooks and installation work; current GitHub task-to-issues command guidance.** 

https://github.github.io/spec-kit/extensions/overview.html 

###### **[R12] Companion status and resume command instructions - locating a spec, deriving state and dispatching the next step.** 

https://github.com/alfredoperez/speckit-companion/tree/main/apps/speckit-extension/commands 

###### **[R13] Companion extension source location - current source tree is under apps/speckit-extension.** 

https://github.com/alfredoperez/speckit-companion/tree/main/apps/speckit-extension 

###### **[R14] Supplied source document - COMPANION-SCENARIOS.md, user-provided attachment, reviewed as the baseline for corrections.** 

User-provided attachment: COMPANION-SCENARIOS.md 

##### **Version and change control** 

This guide reflects the public Companion repository and documentation visible on 17 November 2026. Because the repository’s main branch can move, the installed extension version and its actual files are the final authority for a specific project. Record the version or Git commit when adopting this guide as an internal procedure. 

Plain-language reference - verify installed versions before relying on exact behavior 

26 

