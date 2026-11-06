# Repository Performance Optimizer

A reusable prompt. Paste it into any repository's agent chat to audit and improve how an
AI coding agent performs in that repository.

## Goal

Reduce what the agent spends on every request, and increase how many claims it can verify
before acting. Work in this priority order:

1. Context weight — what is loaded on every single turn.
2. Instruction accuracy — whether the repository's rules match reality.
3. Feedback-loop speed — how fast the agent learns that it broke something.

Success is a measurable reduction in always-loaded instructions, plus a list of verified
corrections the maintainer approves. Do not refactor application code. Do not redesign
architecture. This is an agent-configuration audit.

## Context discovery

Establish these before scanning, and state plainly anything you cannot determine:

- Stack, language and runtime versions, test framework, and the build, test, and lint
  commands. Read the solution or workspace manifests, lockfiles, build property files,
  CI configuration, and package manifests.
- Which agent customization files exist: root instruction files, scoped instruction
  files, agent definitions, skills, prompt files, MCP configuration, hook configuration,
  and any global overlays.
- Whether a symbol-aware or documentation MCP server is connected, and — critically —
  which of its tools are actually **active** rather than merely **available**.
- OS, shell, and repository size: number of tracked files, and counts of the primary
  source file types.

## Scan scope

Read the entire repository folder, not only tracked files. Use the discovery command that
fits the platform:

- `git ls-files`
- `git ls-files --others --ignored --exclude-standard`
- `rg --files --no-ignore --hidden`
- `Get-ChildItem -Recurse -Force` (Windows)

Then run a content pass over manifests, lockfiles, build and CI configuration, linter and
test configuration, and documentation.

Skip these except to read their manifests and lockfiles: `node_modules`, `dist`, `build`,
`out`, `bin`, `obj`, `target`, `.venv`, `venv`, `__pycache__`, `vendor`, generated assets,
and `.git` internals.

Never read or echo the values of `.env` files, keys, tokens, or credentials. Note only that
such a file exists, and flag any that is committed to version control. Never reproduce a
secret in your output.

## Workflow

Work through these steps in order. Steps 1 to 7 produce findings; do not edit anything
until Step 8.

**Step 1 — Establish a safe baseline.** Run `git status` and `git diff --stat`. Record any
uncommitted or untracked work, including deleted tracked files. If the working tree is
dirty, say so and account for it in every later step. Recommend a checkpoint commit or
branch before any edit.

**Step 2 — Measure always-loaded weight.** Inventory every instruction source. For each,
record its scope, whether it applies to every file, its size, and its purpose. Compute the
total that loads on every request and rank sources by size. Flag files that match every
file in the repository, and flag any whose scoping pattern matches nothing at all — those
never load, so they are dead weight that creates false confidence.

**Step 3 — Reality-check the claims.** This is the highest-value step: incorrect
instructions cause confident wrong actions. Verify, with evidence:

- Every file path, directory, and command named in an instruction exists.
- Every documented naming or formatting convention matches the actual codebase. Count real
  occurrences instead of trusting a sample.
- Every documented module, package, or area map matches the real directory layout.
- Every version claim — language, runtime, framework — matches the project files, and all
  instruction files agree with each other.
- Every MCP server, extension, CLI tool, or script an instruction tells the agent to use is
  actually configured, installed, and reachable. For MCP servers, distinguish tools that
  are *available* from tools that are *active*, and record the real call path when the
  documented names differ from the callable ones.
- Every suggested workflow step is still supported by the current tooling.

**Step 4 — Find contradictions and duplication.** For each core rule, find whether it is
stated once or several times, and whether the statements agree. Two different strengths of
the same rule are worse than no rule at all. Also detect the same guidance duplicated
across two or three different customization mechanisms.

**Step 5 — Check customization hygiene.** Flag instruction-loaded files that carry
agent-only metadata, agent definitions that do not restrict their tool list, definitions
duplicated across mechanisms, and any customization migration left uncommitted.

**Step 6 — Assess feedback-loop speed.** Determine the single cheapest command that proves
a change did not break anything, and whether the instructions actually tell the agent to
use it. Check that the primary build entry point includes the test projects. Confirm the
instructions bound terminal output, disable pagers, and prefer reading compiler and lint
diagnostics from the editor over re-running builds.

**Step 7 — Mine history for context.** Use read-only git commands to find the files with
the highest churn, the dominant commit-message convention, and the provenance of suspicious
code. Report which history techniques would replace broad file reading for common
questions.

**Step 8 — Propose, get approval, then apply.** Present findings as a ranked table with
evidence, then present a separate approval request for each independent change: one change
per item, each independently reversible, each stating what it touches and how to undo it.
Apply only approved items, smallest scope first. Prefer a narrow file or folder scope over
a global one; a global change needs its own explicit approval.

## Guardrails

- Inspect status and diff before touching anything. Never overwrite, revert, or discard
  uncommitted work.
- Never amend, rebase, rewrite history, reset, clean, or force-push.
- Propose a plan and get approval before editing. Make a checkpoint first.
- Keep each change small and reversible. Change one concern at a time.
- Do not install packages, extensions, or MCP servers, and do not change global or
  user-level configuration, without explicit approval.
- Never push.
- Run the repository's build, tests, and lint after changes. If validation fails, stop and
  explain. Do not substitute a different change to make the failure go away.
- Summarize the resulting diff.
- If something is impossible or not discoverable, say so plainly rather than substituting
  something that merely appears to work.

## Output format

Report in this order:

1. **Environment** — stack, versions, sizes, and anything unknown.
2. **Working well** — what needs no change. Do not manufacture problems.
3. **Findings table** — `# | Finding | Evidence (path:line) | Confidence | Gain | Effort`.
4. **Approval requests** — one per independent change, each with scope, what it touches,
   and how to undo it.
5. **Applied changes** — what changed, plus the diff summary.
6. **Verification** — the exact commands run and their outcome.
7. **Not done** — deferred and rejected items with the reason.

## Done criteria

Stop when all of these hold:

- Every instruction source is inventoried with a measured size and a verdict.
- Every Step 3 claim has been checked and is either confirmed or listed as a defect.
- The ranked findings table is delivered with evidence for every row.
- Every change is either approved and applied with verification, or explicitly listed as
  not done.
- The build, tests, and lint pass, or their failure is explained rather than hidden.
- The diff summary is delivered.

Do not continue past these criteria into unrelated refactoring.
