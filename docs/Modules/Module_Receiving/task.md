# Manual Mode Part and PO Task List

Last Updated: 2026-03-24

- [x] Update intended Mermaid workflow with vendor requirement for PO fuzzy-search results.
- [x] Create implementation plan with affected files, methods, models, and guardrails.
- [x] Add read-only Infor Visual PO-by-part search support.
- [x] Implement Manual Mode part-first row resolution.
- [x] Implement Manual Mode PO-first row resolution when Part ID is blank.
- [x] Implement mismatch dialog flow for PO does not contain entered part.
- [x] Ensure unresolved outcomes clear Part ID only for Manual Mode.
- [x] Verify Guided Mode and Edit Mode remain behaviorally unchanged by keeping code changes scoped to Manual Entry handlers and Manual Entry viewmodel/service dependencies only.
- [x] Build and validate the updated flow. Application build passed. Test project remains blocked by an existing WinUI asset-copy issue for `Microsoft.UI.Xaml\\Assets\\map.html` and `NoiseAsset_256X256_PNG.png`.
- [x] Update Module_Receiving metadata after code changes.
