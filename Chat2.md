User: in the devolopment workflow of one of my modules, what documentation per module do you think would be mandatory to include inside a Documentation folder inside each Module folder, then broken down into sub catagory folders.  think AI editing, bug fixing, creating end user documentaiton, major module refactoring.  create a md file stating your thoughts, keep it clean, code agnostic, formatted in a non developer jargon format.

GitHub Copilot: Searched for files matching `**/memory-bank/**/*.md`, no matches

Searched for files matching `**/_memory/**/*.md`, 41 matches

Read [](file:///c%3A/Users/jkoll/source/repos/MTM_Receiving_Application/Copilot-Processing.md)

Using "Apply Patch"


```

```


Created [](file:///c%3A/Users/jkoll/source/repos/MTM_Receiving_Application/docs/module-documentation-template.md)

Using "Apply Patch"


```

```


Using "Apply Patch"


```

```
- Added a plain-language module documentation starter at module-documentation-template.md with a per-module folder layout covering overview, how-to guides, support/fixes, change/decision notes, big-change plans, AI handoff briefs, end-user help, and reusable templates.
- No tests were run (documentation-only change).

Added final summary to Copilot-Processing.md. Please review and remove the file when you’re done so it isn’t committed.

Made changes.

User: #file:instructions.instructions.md #file:prompt.instructions.md create an instruciton file and create a prompt that will either update or genereate the required documents in a given module either by the one(s) the user references or if none are referenced first find one without documentation and do that one otherwise find one that has outdated documents.  a log of when module documents where last updated should be kept in each docuemtn so copilot can easily see when the last time a file was updated.

GitHub Copilot: Created [](file:///c%3A/Users/jkoll/source/repos/MTM_Receiving_Application/.github/instructions/module-doc-maintenance.instructions.md)

Created [](file:///c%3A/Users/jkoll/source/repos/MTM_Receiving_Application/.github/prompts/module-doc-maintainer.prompt.md)

- Added documentation maintenance instructions at module-doc-maintenance.instructions.md covering required per-module doc sets, freshness logging, selection rules, and validation.
- Added a reusable prompt at module-doc-maintainer.prompt.md to auto-create or refresh module docs, insert `Last Updated` stamps, and log changes/decisions.

If you want, I can run the prompt against a specific module next.

Made changes.