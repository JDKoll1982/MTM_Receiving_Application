Last Updated: 2026-04-08

# Assumptions For Receiving Recommended Locations And Vendor Aggregation

1. Assumption: The existing receiving user preference stored at `Receiving.UserPreferences.IgnoredReconciliationLocationsJson` should also drive the new recommended-locations ignore behavior.
   Why this assumption is needed: The request asks for a new location ignore list for recommended locations, but the codebase already contains a persisted receiving ignored-location list with fuzzy add/remove behavior.
   Potential impact if wrong: Users could see one screen but have the wrong ignore list applied, or the application could end up with two overlapping location preference stores.
   Alternative interpretations considered: Create a second dedicated recommended-locations preference key; hardcode a separate non-editable ignore list only for the card.

2. Assumption: The default ignored locations `WC`, `NCM`, `FG`, and `RECV` should be treated as default exclusions for recommendations but still remain user-manageable rather than permanently forced.
   Why this assumption is needed: The request says to exclude them by default, but does not specify whether users can opt them back in.
   Potential impact if wrong: A permanent exclusion could hide desired locations; a removable exclusion could surface locations the business intended to suppress globally.
   Alternative interpretations considered: Always exclude these four regardless of preferences; seed them once and let users remove them later.

3. Assumption: The recommended-locations card should appear on the Enter Load Information step once a part has already been resolved in Guided Mode.
   Why this assumption is needed: The request places the card on Enter Load Information, but does not specify whether the card should render before part resolution or how it should behave when part context is missing.
   Potential impact if wrong: The card could appear empty or confusing, or it could be delayed longer than the user expects.
   Alternative interpretations considered: Show the card only when part context exists; show a placeholder when part context is not yet available.

4. Assumption: The Outside Service History vendor aggregation checkbox should collapse duplicate vendor-name search candidates before drill-in, not merge final history rows from distinct vendor IDs into a synthetic result set.
   Why this assumption is needed: The request says to reduce clutter from duplicate vendor results, but does not define whether aggregation applies to the fuzzy-selection stage or the final history grid.
   Potential impact if wrong: Aggregating final rows could hide vendor-ID distinctions and mix unrelated history; aggregating only search candidates may not reduce clutter in every place the user expects.
   Alternative interpretations considered: Aggregate search candidates only; aggregate final result rows by vendor name; load all matching vendor IDs into one combined history set.

5. Assumption: When a user selects an aggregated vendor-name result, the app should continue using the existing vendor-to-part drill-in flow and pick one matching vendor key behind the scenes only after the user chooses a specific candidate.
   Why this assumption is needed: The current tool is keyed by vendor ID, while the request asks for vendor-name aggregation.
   Potential impact if wrong: The tool may lose precision, fetch incomplete records, or require a different interaction model than users expect.
   Alternative interpretations considered: Prompt again for exact vendor ID after vendor-name aggregation; merge all vendor IDs sharing the same name; replace vendor-ID drill-in entirely.

Please confirm, correct, or clarify these assumptions before implementation continues on the recommended-locations and vendor-aggregation features. Phase 1 focus changes can proceed safely without these decisions.
