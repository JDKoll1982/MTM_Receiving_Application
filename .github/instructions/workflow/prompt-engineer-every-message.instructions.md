---
applyTo: "**"
description: >
  Prompt Engineering Auto-Enhancer — Intercepts every user message, applies
  MasterPrompting.net principles to construct an optimized prompt, presents it
  for approval using vscode_askQuestions BEFORE executing any task, then
  proceeds only with the confirmed prompt.
---

<!-- 
[DOC-META-START]
- File Name: prompt-engineer-every-message.instructions.md
- Description: Prompt Engineering Auto-Enhancer — Intercepts every user message, applies MasterPrompting.net principles to construct an optimized prompt, presents it for approval using vscode_askQuestions BEFORE executing any task, then proceeds only with the confirmed prompt.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 10-16: # Prompt Engineering Auto-Enhancer
  - Line 17-222: ## Mandatory Workflow — Apply to EVERY User Message
  - Line 223-247: ## When to Apply the Full Workflow vs. Lightweight Mode
  - Line 248-295: ## Specialized Pattern — Rewriting Rough Notes Into Implementation Prompts
  - Line 296-345: ## Prompt Engineering Quick Reference (From All Tracks)
  - Line 346-382: ## Example Enhancement (Before vs. After)
  - Line 383-397: ## Implementation Note
[DOC-META-END]
-->

# Prompt Engineering Auto-Enhancer

**Source:** Principles synthesized from all 69 lessons across Beginner, Intermediate, Advanced,
AI Agents, Risks & Safety, and Claude Code tracks at https://masterprompting.net/learn

---

## Mandatory Workflow — Apply to EVERY User Message

**BEFORE taking any action on any user request, you MUST complete this three-step workflow.**

### Step 1 — Analyze the Raw Input

Silently evaluate the user's message against the following checklist:

**The Four Elements (Beginner Track — Lesson 10)**
- [ ] **Instruction** — Is there a clear action verb + target? (not "help me with X" but "fix X", "write X", "explain X")
- [ ] **Context** — Does it include who the user is, what the goal is behind the task, and relevant background?
- [ ] **Input Data** — If the task requires processing content, is it clearly delimited?
- [ ] **Output Format** — Is structure, length, tone, and any exclusions specified?

**Clarity & Specificity Check (Beginner Track — Lesson 2)**
- [ ] Output format explicitly defined?
- [ ] Length specified (word count, sentence count, or "brief/detailed")?
- [ ] Target audience named?
- [ ] Negative constraints stated (what NOT to do)?
- [ ] Example of desired output provided (if tone/style matters)?

**Context Completeness Check (Beginner Track — Lesson 6)**
- [ ] Who the user is (or who output is for)?
- [ ] The real goal behind the surface task?
- [ ] Constraints and boundaries?
- [ ] Relevant background the model needs but wouldn't know?

**Common Mistake Scan (Beginner Track — Lesson 8)**
- Avoid: one-word or one-line tasks with no context
- Avoid: asking for multiple unrelated things in one prompt
- Avoid: vague quality words ("better", "more professional", "improve") without specifying HOW
- Avoid: missing length specification when it matters
- Avoid: missing format instructions

**Implementation-Handoff Gap Check**
- [ ] If the user wants a rough note, bug list, or change log turned into an implementation-ready prompt, have they specified whether clarifications should be gathered first?
- [ ] Does the task need exact codebase grounding such as owning files, method names, tests, or architectural seams before the prompt can be trusted?
- [ ] Are there likely scope mistakes or unrelated files in the user's raw list that should be confirmed or pruned before rewriting the prompt?
- [ ] Does the final artifact need implementation order, validation expectations, or explicit non-scope items so a later coding pass can proceed without rediscovery?

---

### Step 2 — Construct the Enhanced Prompt

Using only what the user provided plus reasonable inference from workspace context,
build an enhanced version of their prompt by applying:

**Role Assignment (Beginner Track — Lesson 3)**
- Prefix with a relevant expert role when domain expertise, tone, or audience matters
- Template: `You are a [role] with [experience level] in [domain]. Your communication style is [tone]. You are writing for [audience].`
- Skip for trivial single-step tasks

**XML Structure for Complex Prompts (Intermediate Track — Lesson 2)**
- When the prompt mixes instructions + data + context + examples, use XML tags:
  ```
  <instructions>...</instructions>
  <context>...</context>
  <input>...</input>
  <output_format>...</output_format>
  ```
- Reference tags by name in instructions: "Using only the information in `<context>`..."
- Use triple backticks for code content, markdown headers for multi-section prompts
- **EXCEPTION — spec and docs `.md` files:** Do NOT use XML tags when writing or editing any
  file under `specs/**/*.md` or `Module_*/docs/**/*.md`. Those files are governed by
  `spec-slice-format.instructions.md`, which requires `##` Markdown headings instead of XML
  tags. XML breaks Markdown preview and printing. This exception takes precedence over the XML
  structure guidance above.

**Chain of Thought for Reasoning Tasks (Intermediate Track — Lesson 3)**
- Add "Think step by step." for: math, multi-step logic, analysis, debugging, planning, decisions
- Use guided CoT when the problem structure is known (numbered reasoning steps)
- Use `<thinking>` / `<recommendation>` XML blocks for high-stakes analysis
- Skip CoT for: simple lookups, translations, creative writing, formatting-only tasks

**Few-Shot Examples when Style/Format Matters (Intermediate Track — Lesson 1)**
- If the user implies a specific tone or format not easily described, ask for or infer an example
- Include 2–5 examples when structure consistency is critical
- Focus on formatting consistency in examples more than content correctness

**Output Format Controls (Beginner Track — Lesson 4)**
The enhanced prompt MUST specify:
1. **Structure** — bullets / numbered list / table / JSON / markdown / prose / headers
2. **Length** — word count, sentence count, or relative (e.g., "under 200 words", "one paragraph")
3. **Style** — tone, vocabulary level, voice (if relevant to the task)
4. **Exclusions** — "Do NOT include X", "No preamble", "Output only Y"

**Context Enrichment (Beginner Track — Lesson 6)**
Prepend the five context types if absent and inferrable:
1. Who the user is (infer from workspace: WinUI 3 developer working on MTM Receiving Application)
2. Who the output is for
3. The goal behind the task
4. Constraints and boundaries
5. Relevant background (use workspace knowledge if the task is code-related)

**Repository-Grounded Prompt Construction (Required for implementation-ready prompts)**

When the user asks you to rewrite rough notes, specs, change requests, issue bullets, or a scratchpad into a prompt that will later drive real code changes, do not stop at generic prompt enhancement. Build a repo-grounded implementation handoff.

Use this workflow inside Step 2 before presenting the approval prompt:

1. **Identify the owning surfaces**
  - Inspect the smallest set of nearby files needed to determine which views, viewmodels, services, controls, tests, and docs actually own the requested behavior.
  - Prefer the direct behavior owner over broad repo mapping.

2. **Collect targeted clarifications**
  - Ask only the questions needed to resolve implementation-affecting ambiguity.
  - Focus on behavior rules, scope splits, display semantics, required calculations, and explicit non-scope items.
  - Resolve ambiguous file lists by checking the repo instead of preserving bad guesses.

2a. **Run additional clarification passes when needed**
  - If the first rewritten prompt is still missing implementation-critical decisions, or the user explicitly asks for more clarification before coding, run another targeted clarification pass instead of guessing.
  - Group questions by theme when possible: behavior rules, visual semantics, selection rules, navigation, validation, and testing.
  - If the user asks for a minimum number of questions, satisfy that minimum while still keeping the questions implementation-relevant.
  - Prefer a single batched clarification pass over many tiny interruptions when the ambiguities are known and related.

3. **Ground the prompt in real code anchors**
  - Name the actual files that should change.
  - Include method names, view/control anchors, test files, and documentation files when they are discoverable and useful.
  - If exact line numbers are unstable or not requested, use method or symbol anchors instead of inventing precision.

4. **Separate primary scope from conditional scope**
  - Distinguish between files that almost certainly must change and files that are verify-only or conditional.
  - Call out explicit non-scope items when the user's raw list includes likely unrelated surfaces.

5. **Convert the result into an implementation-ready handoff**
  - The final enhanced prompt should include, when relevant:
    - personas or roles
    - confirmed clarifications
    - required outcome
    - constraints and guardrails
    - files grouped by primary / conditional / non-scope
    - code anchors such as methods, controls, tests, or handlers
    - ordered implementation plan
    - checkbox task list
    - validation expectations for the later coding pass

6. **Optimize for no-rediscovery later**
  - The rewritten prompt should be self-contained enough that a later implementation pass does not need another broad discovery round.
  - Add only the context that prevents likely misexecution; do not pad the prompt with generic commentary.

7. **Reintegrate later clarifications into the artifact**
  - After any follow-up clarification pass, update the rewritten prompt/spec itself rather than leaving the answers only in chat.
  - Merge the new answers into the correct sections such as Confirmed Clarifications, Required Outcome, Constraints And Guardrails, Files That Should Change, Ordered Implementation Plan, Task List, and Validation Expectations.
  - Keep the artifact internally consistent after reintegration. Do not leave stale earlier assumptions in place once clarified answers supersede them.

---

### Step 3 — Present and Get Approval BEFORE Acting

**This step is NON-NEGOTIABLE. Do not execute the task before approval.**

Use the `vscode_askQuestions` tool with the following structure:

- **header:** `"Enhanced Prompt Ready"`
- **question:** `"I've applied prompt engineering techniques to your request. Does the summary below capture what you want?"`
- **message:** Build the message using this exact readable structure — do NOT paste the raw enhanced prompt as a monospace code block, as that renders as an unreadable wall of text in VS Code dialogs.

  Format the message as clean markdown with these labelled sections:

  ```
  **🎯 Task**
  [One sentence: action verb + what is being done + target artifact]

  **👤 Role assigned**
  [e.g., "Senior C# / WinUI 3 developer" — or "None (simple task)"]

  **📋 Key constraints**
  - [Constraint 1]
  - [Constraint 2]
  - [Constraint 3 — max 5 bullets; omit trivial ones]

  **📄 Output format**
  - Structure: [bullets / table / code blocks / prose]
  - Length: [e.g., "one code file per class" / "under 300 words"]
  - Exclusions: [e.g., "No preamble" / "No code comments on obvious lines"]

  **➕ What was added vs. your original message**
  - [Key addition 1 — e.g., "Added MVVM architecture constraints"]
  - [Key addition 2 — e.g., "Specified FluentAssertions for assertions"]
  ```

For implementation-ready prompt rewrites, also summarize whichever of these were added:

- clarified behavior rules
- real file or method anchors
- primary vs conditional scope
- explicit non-scope items
- ordered plan
- validation/test expectations

- **options:**
  - `"✅ Yes — use this prompt"` *(recommended)*
  - `"✏️ Let me adjust it first"`
  - `"⏭️ Skip — use my original message as-is"`
  - `"🔄 Regenerate with different approach"`
- **allowFreeformInput:** `true`

**Handle each response:**
- **"✅ Yes — use this prompt"** → Proceed using the enhanced prompt exactly as presented
- **"✏️ Let me adjust it first"** → Wait for the user's edits, then re-present the adjusted version for a second approval (one level deep only)
- **"⏭️ Skip — use my original message as-is"** → Proceed using the user's original raw input
- **"🔄 Regenerate with different approach"** → Apply a different technique (e.g., fewer constraints, different role, more CoT) and re-present
- **Freeform text response** → Treat the freeform text as corrections, incorporate them into the enhanced prompt, re-present for final approval

---

## When to Apply the Full Workflow vs. Lightweight Mode

### Full Workflow (Steps 1 + 2 + 3)
Apply when the user's message is:
- A task request (write, fix, build, analyze, explain, create, refactor, debug)
- Vague, incomplete, or missing format/context
- Complex enough that a poorly formed prompt would produce mediocre output
- A first message in a new conversation about a new topic
- A request to turn rough notes or a scratchpad into an implementation-ready prompt or spec for a later coding pass

### Lightweight Mode (Step 3 only — ask with minimal enhancement)
Apply when:
- The user's message is already well-specified (has role, context, format, constraints)
- A follow-up message that builds on prior approved context
- A short clarification question mid-task ("what does X mean?")

In lightweight mode, still present the prompt but note "Your prompt looks well-formed already" and offer the same approval options with lighter enhancement.

### Skip Entirely (proceed without enhancement)
- Purely conversational greetings or acknowledgements ("thanks", "got it")
- Explicit user instruction: "Just do it, skip the prompt review"
- Follow-up iterations where the user already approved a base prompt and is refining

---

## Specialized Pattern — Rewriting Rough Notes Into Implementation Prompts

Use this pattern when the raw input is a change list, markdown scratchpad, copied issue notes, or a rough feature idea that will later be used to drive code changes.

### Required deliverable shape

When appropriate, shape the rewritten prompt/spec with sections like:

- Audience
- Personas
- Confirmed Clarifications
- Required Outcome
- Constraints And Guardrails
- Primary Change Areas In Implementation Order
- Files That Should Change
- Focused Tests That Should Change
- Documentation Files That Should Change
- Verify-Only / Conditional Files
- Explicit Non-Scope Items
- Ordered Implementation Plan
- Task List
- Validation Expectations For The Later Implementation Pass

### Clarification priorities

When deciding what to ask before rewriting, prioritize:

1. ambiguous behavior rules that would change implementation
2. scope splits such as separate page vs dialog, replace vs coexist, count vs sum, style vs logic
3. questionable or unrelated files in the user's source list
4. the level of precision desired for anchors: exact lines, methods, or best-effort symbols

### Clarification hardening after the first draft

If the first rewritten prompt/spec is good but still not implementation-safe, or the user asks for a deeper clarification pass:

1. inspect the current draft to identify which sections still contain assumptions
2. ask a focused second-round question set aimed only at those unresolved decisions
3. prefer thematic batches over ad hoc one-off questions
4. update the draft immediately after the answers arrive
5. keep the final artifact as the single source of truth rather than splitting key decisions between the file and the chat

### Research budget rule

Before rewriting the prompt, do enough targeted repo inspection to identify the controlling surfaces and major seams, but do not perform a full implementation investigation. The goal is to create a high-confidence handoff prompt, not to start coding yet.

---

## Prompt Engineering Quick Reference (From All Tracks)

### The Four Elements (Always Check)
| Element | Question to Ask | Fix If Missing |
|---------|----------------|----------------|
| Instruction | What exactly should the AI DO? (action verb + target) | Add clear verb: "write", "analyze", "fix", "summarize" |
| Context | Who is this for? What's the real goal? What domain? | Add 1-2 sentences of background |
| Input Data | Is the content to be processed clearly marked? | Wrap in XML tags or triple backticks |
| Output Format | How should the response be structured/sized? | Specify structure + length + tone + exclusions |

### Role Templates
```
You are a [role] with [N] years of experience in [domain].
Your communication style is [tone/style].
You are addressing [audience].
```

### Specificity Spectrum
- **Vague** → "Make it better"
- **General** → "Make it more professional"
- **Specific** → "Remove contractions, use formal vocabulary, ensure complete sentences"
- **Precise** → "Remove contractions, replace informal words with formal equivalents, ensure every sentence has an explicit subject. Do not change the content."

Always aim for **Specific** or **Precise**.

### Chain of Thought Trigger Phrases
- `Think step by step.` — simple trigger, works for most reasoning tasks
- `Work through each step before giving your final answer.` — guided CoT
- `<thinking>` block → `<recommendation>` block — highest-stakes analysis (especially effective with Claude)

### Format Control Patterns
```
Structure: [bullets / numbered list / table / JSON / prose / markdown sections]
Length: [N words / N sentences / under N words / one paragraph]
Tone: [formal / casual / direct / empathetic / technical / plain]
Exclusions: [No preamble. No explanation. Output only X.]
```

### Context Minimum Viable (For Every Enhanced Prompt)
Before sending, ask: "What would the model get WRONG if I didn't tell it this?"
That's what to include. Nothing else is required.

### Avoiding Hallucinations (Intermediate Track — Lesson 4)
For factual or code-related prompts, add:
- "Only use information from the provided context."
- "If you don't know, say so rather than guessing."
- "Cite which file/section each answer is drawn from."

---

## Example Enhancement (Before vs. After)

**User's raw message:**
> "write some tests for the receiving line dao"

**Enhanced prompt (after applying this instruction):**
```
You are a senior C# engineer working on a WinUI 3 MVVM desktop application
(.NET 10, MySQL 5.7, xUnit, FluentAssertions).

<task>
Write unit and integration tests for the Dao_ReceivingLine class in the
MTM Receiving Application.
</task>

<constraints>
- Unit tests: mock all dependencies; test DAO logic in isolation
- Integration tests: use IAsyncLifetime for setup/cleanup; prefix test data with "TEST-"
- Follow the "Test what you can mock" decision tree from project guidelines
- Test method naming: MethodName_Should<Result>_When<Condition>
- No Arrange/Act/Assert comments (project convention)
- Use FluentAssertions for all assertions
- Verify that DAOs return Model_Dao_Result (never throw exceptions)
</constraints>

<output_format>
- One test class per category (unit / integration)
- Include at least 3 tests per method covering: happy path, null input, and DB error
- Use markdown code blocks with C# syntax highlighting
- Add a brief comment above each test class explaining what it covers
</output_format>

Think step by step through what test cases are most critical before writing any code.
```

---

## Implementation Note

This workflow applies to every session regardless of topic. For non-code tasks (writing,
analysis, planning), adapt the workspace context section to reflect the user's actual
domain. For code tasks in this repository, workspace knowledge (MVVM architecture,
forbidden patterns, DAO conventions) should automatically inform the enhanced context.

When the user asks for a prompt that will later drive implementation work, treat prompt quality as partly a research task: first confirm the behavior rules and repo anchors, then write the prompt so the later coding pass can execute with minimal rediscovery.

The goal is not longer prompts — it is **complete** prompts that give the AI no room to
misinterpret. A well-formed prompt of 5 sentences beats a vague paragraph every time.
