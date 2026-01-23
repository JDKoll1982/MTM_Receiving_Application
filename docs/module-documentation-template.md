# Module Documentation Starter Kit

A simple, repeatable set of docs every module should keep in its own `Documentation` folder. Written for anyone who needs to touch the module: builders, testers, support, writers, or AI helpers.

## Default Folder Layout (per module)

```
Documentation/
├─ Overview/
│  ├─ About-This-Module.md        # What it does, why it matters, who owns it
│  └─ How-It-Works-at-a-Glance.md # Short, plain-language flow of the module
├─ How-To-Guides/
│  ├─ Daily-Tasks.md              # Step-by-step for routine work
│  └─ Unusual-Situations.md       # Rare but important procedures
├─ Support-and-Fixes/
│  ├─ Common-Issues.md            # Symptoms, quick fixes, when to escalate
│  └─ Checks-and-Health.md        # Simple checks to confirm the module is healthy
├─ Changes-and-Decisions/
│  ├─ Change-Log.md               # What changed, when, and why it was done
│  └─ Decisions.md                # One-pagers capturing key choices and tradeoffs
├─ Big-Changes/
│  ├─ Refactor-Plan.md            # Plan, risks, roll-back steps for major rewrites
│  └─ Impact-Map.md               # What parts of the product are touched
├─ AI-Handoff/
│  ├─ Editing-Brief.md            # What AI or new editors must know before touching the module
│  └─ Guardrails.md               # Boundaries, do/do-not rules, safety checks
├─ End-User-Help/
│  ├─ Quick-Start.md              # Short guide to start using the module
│  └─ FAQ.md                      # Friendly answers to common questions
└─ Templates/
   └─ README.md                   # A copy of this structure as a starter
```

## What goes in each area

- **Overview**: A two-page max story of what the module solves and the main steps it runs through.
- **How-To-Guides**: Plain checklists for day-to-day actions and rare edge cases; keep screenshots or short clips if helpful.
- **Support and Fixes**: Fast triage tips, common error messages and their fixes, and a small "when to call for help" section.
- **Changes and Decisions**: A running log of updates plus brief decision notes that explain the reason behind meaningful changes.
- **Big Changes**: Plans for large cleanups or redesigns, with clear risk lists, roll-back steps, and success criteria.
- **AI Handoff**: One-page briefs that orient AI or new editors—what to avoid, sensitive data notes, and how to validate a change safely.
- **End-User Help**: Friendly guidance for people using the module; avoid technical terms and show the happy path.
- **Templates**: Reusable skeletons so every module starts from the same playbook.

## Maintenance expectations

- Keep summaries short and current; archive stale details instead of piling on edits.
- When a change ships, update the Change-Log and, if it involved a decision or risk, add a short Decision note.
- For big refactors, fill in the Refactor-Plan and Impact-Map before starting work.
- When AI or new contributors join, refresh the Editing-Brief and Guardrails first.
- Treat End-User-Help as the single source for customer-facing steps; test them with a new person occasionally.
