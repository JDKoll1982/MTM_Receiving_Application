# Specification Quality Checklist: Heat / Lot Management

**Purpose**: Validate Companion specification completeness before planning
**Created**: 2026-10-14
**Feature**: [heat-lot-management.spec.md](../heat-lot-management.spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed (User Scenarios, Requirements, Success Criteria)

## Requirement Completeness

- [x] Any [NEEDS CLARIFICATION] markers are genuine ambiguities (≤3) deferred to clarify — not unresolved guesses
- [x] Each Functional Requirement is a single, testable MUST/SHOULD statement
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into the specification

## Notes

- Items marked incomplete require spec updates before clarify or plan
- No [NEEDS CLARIFICATION] markers were needed. Every open question from the underlying feature
  document was resolved with the product owner on 2026-10-14 before this spec was drafted.
- The one sanctioned exception to "no implementation detail" is the Verbatim Constraints section,
  which records the exact strings the request pinned (tool name, tool key, column names, the
  `NONPO` group, and the 13 shape and 12 color values). These are user-pinned requirements, so
  dropping or paraphrasing them would be a defect.
