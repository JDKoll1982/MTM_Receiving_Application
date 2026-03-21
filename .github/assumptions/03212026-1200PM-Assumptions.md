# CopilotForms Form-Usability Assumptions

1. I plan to keep the existing Copilot request-form experience as the primary user experience, and improve it so users type less.
   Why this assumption is needed:
   You clarified that the point of gathering the module information was to make the request forms themselves more intuitive, not primarily to build a workflow-document library.
   Potential impact if wrong:
   If the real priority is documentation browsing first, I would over-invest in form usability and under-invest in standalone workflow pages.
   Alternative interpretations considered:

- Make the main deliverable workflow reference pages.
- Split effort evenly between documentation pages and smarter input controls.
- Prioritize request-form usability first and treat workflow pages as supporting material.

2. I plan to preserve the current top-level `forms` array because it already powers the existing Copilot request pages.
   Why this assumption is needed:
   The current browser experience already depends on that structure. Replacing it would create extra risk and slow down work on the actual usability improvements.
   Potential impact if wrong:
   If you wanted a full redesign of the config structure, keeping `forms` stable would limit how aggressively I could remodel the data.
   Alternative interpretations considered:

- Replace the current `forms` array entirely.
- Move current request forms under another key and build a brand-new request runtime.
- Keep `forms` stable and enrich it with smarter metadata-driven behavior.

3. I plan to add supporting metadata that helps the existing request forms auto-populate choices, defaults, and likely answers.
   Why this assumption is needed:
   To reduce typing, the forms need structured data they can use for controls like checkboxes, combo boxes, list pickers, chips, repeatable rows, and data-grid-like selectors.
   Potential impact if wrong:
   If you wanted mostly free-text forms with only minor enhancements, this would add more metadata than necessary.
   Alternative interpretations considered:

- Keep only the current static fields and placeholders.
- Add minimal dropdown data only.
- Add a broader metadata layer for modules, workflows, screens, fields, validations, and likely files.

4. I plan to use the gathered module data only for the modules we have already documented: `Module_Dunnage`, `Module_Receiving`, `Module_Reporting`, and `Module_Volvo`.
   Why this assumption is needed:
   You explicitly said to begin only with modules where data has already been gathered.
   Potential impact if wrong:
   If you expected the improved request forms to span the full application immediately, the first pass would feel intentionally incomplete.
   Alternative interpretations considered:

- Apply the improvements repo-wide immediately.
- Restrict improvements only to `Module_Receiving`.
- Start with the four analyzed modules and expand later.

5. I plan to make structured controls the default wherever the code and docs already tell us the likely answer set.
   Why this assumption is needed:
   The main usability win comes from replacing unnecessary typing with guided selection.
   Potential impact if wrong:
   If users actually need more freedom than the current docs suggest, overly structured controls could feel restrictive.
   Alternative interpretations considered:

- Leave most fields as text areas.
- Use structured controls only for simple priorities and risk levels.
- Use structured controls whenever the answer space is reasonably known, with text fallback when it is not.

6. I plan to map known data types to intuitive control types like this:
   Why this assumption is needed:
   The form design needs a consistent rule set so we can reduce typing without making the forms confusing.
   Potential impact if wrong:
   If a given field type needs a different control style, some screens may feel awkward until refined.
   Alternative interpretations considered:

- Booleans and yes/no options → checkboxes or toggle switches
- Small known option sets (modes, audiences, scopes, workflows, severity) → combo boxes / selects
- Known multi-select choices (affected screens, files, concerns, validations, modules) → checkbox lists or addable chip lists
- Repeatable structured rows (scenarios, rule sets, verification steps, related files) → add-row lists or simple data-grid-like editors
- Unknown or explanatory input → text boxes / text areas

7. I plan to keep free-text fields only where human explanation is genuinely needed.
   Why this assumption is needed:
   You still want users to explain intent, but only after the form has already collected as much structured information as possible.
   Potential impact if wrong:
   If users need more narrative room in earlier sections, the form could feel too rigid.
   Alternative interpretations considered:

- Keep the current amount of free-text input.
- Aggressively remove most text fields.
- Keep text fields for intent, edge cases, exceptions, and desired outcomes only.

8. I plan to make the form adapt based on module and workflow selection so users only see relevant questions.
   Why this assumption is needed:
   A single large generic form increases typing and confusion. Conditional visibility is one of the best ways to make the experience feel intuitive.
   Potential impact if wrong:
   If you wanted every field visible at all times for transparency, dynamic forms could feel too hidden.
   Alternative interpretations considered:

- Keep every field visible all the time.
- Group fields by collapsible sections only.
- Dynamically narrow the visible inputs based on the selected module, feature, workflow, and request type.

9. I plan to use the module docs we created as the source of truth for the first usability pass rather than re-reading the code.
   Why this assumption is needed:
   You explicitly said there should be no need to re-read the code because the updated/generated docs now house the needed information.
   Potential impact if wrong:
   If a doc entry is stale or incomplete, the first pass of form improvements could inherit that gap.
   Alternative interpretations considered:

- Re-scan the source code before each form change.
- Use only the updated docs as the planning source for the first pass.
- Use docs first, and only re-open code later if a gap becomes obvious.

10. I plan to treat workflow HTML pages as secondary helpers, not the main deliverable, for this phase.
    Why this assumption is needed:
    Your clarification makes the priority clear: the Copilot request forms themselves should become easier and faster to complete.
    Potential impact if wrong:
    If you still want equal emphasis on standalone documentation pages, the implementation would need to split effort more evenly.
    Alternative interpretations considered:

- Keep workflow HTML generation as a major parallel track.
- Pause workflow pages entirely until forms are improved.
- Keep workflow pages as supporting context while prioritizing request-form usability.

Please confirm, correct, or refine these assumptions before I proceed with the CopilotForms usability changes.
