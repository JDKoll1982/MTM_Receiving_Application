---
applyTo: "**"
description: >
  Prompt Engineering Auto-Enhancer — Intercepts every user message, applies
  MasterPrompting.net principles to construct an optimized prompt, presents it
  for approval using vscode_askQuestions BEFORE executing any task, then
  proceeds only with the confirmed prompt.
---

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

The goal is not longer prompts — it is **complete** prompts that give the AI no room to
misinterpret. A well-formed prompt of 5 sentences beats a vague paragraph every time.
