# AI Integration Specification

Last Updated: 2026-03-20
Status: Draft — For Review

---

## ⛔ MAJOR RULE — AI Data Privacy

> **This rule is non-negotiable and overrides every other guideline in this document.**

The AI assistant (GitHub Copilot or any model accessed through it) is a **stateless, session-only tool**.
It **must not**, under any circumstances, retain, store, learn from, or transmit any proprietary company
information outside of the current active session.

This includes but is not limited to:

- Purchase order numbers, quantities, and vendor names
- Part numbers, descriptions, or inventory levels
- Supplier pricing, lead times, or contract terms
- Customer names, delivery addresses, or order history
- Employee names, usernames, or login credentials
- Any data visible in the Infor Visual (MTMFG) ERP database
- Any data stored in the MySQL receiving database
- Internal business rules, thresholds, or workflows described to the AI during a session

**How this is enforced in practice:**

| Rule                          | What It Means                                                                                                      |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| No fine-tuning                | The app will never use a fine-tuning or training API. Models are used as-is, read-only.                            |
| No persistent memory features | GitHub Copilot memory / conversation history features must not be enabled for this application.                    |
| Context-only prompts          | Any data sent to the AI is limited to what is needed to answer the immediate question — no bulk data dumps.        |
| No third-party AI logging     | The selected model and API pathway must not log prompts to any system outside of GitHub's standard privacy policy. |
| User acknowledgement          | The first time a user enables AI features, they must acknowledge this privacy rule before proceeding.              |

> **On implementation:** The developer implementing any AI feature is responsible for reviewing the
> GitHub Copilot API privacy documentation and confirming that no additional data retention options
> are inadvertently enabled. Any doubt defaults to **do not send the data**.

---

## 1. Purpose and Scope

This document describes the requirements, user experience, and module structure for
integrating AI assistance into the MTM Receiving Application. It is intentionally
code-agnostic — no implementation details are prescribed here so that the spec can
be reviewed, challenged, and refined before any code is written.

AI capabilities surface primarily inside **Module_Receiving** (the place users spend
the most time). Configuration and account management live in a new
**Module_Settings.AI** that follows the same hub-and-spoke pattern already established
by **Module_Settings.Receiving** and **Module_Settings.Core**.

---

## 2. Core Requirements

| #    | Requirement                                                                                                                                                       |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R-01 | A user **must** have a GitHub account to unlock any AI feature.                                                                                                   |
| R-02 | The application **must** detect the user's GitHub Copilot subscription tier and surface it clearly.                                                               |
| R-03 | Available AI models **must** be filtered to only those permitted by the detected tier.                                                                            |
| R-04 | When the selected model has a monthly quota, the app **must** display a real-time usage gauge (like Visual Studio's Copilot usage bar).                           |
| R-05 | A global AI on/off toggle must exist in settings. When off, no AI surfaces anywhere in the UI and no GitHub API calls are made.                                   |
| R-06 | Individual AI features within Module_Receiving can each be toggled independently, subject to the global switch.                                                   |
| R-07 | The GitHub Personal Access Token (PAT) or OAuth credential must **never** be logged or displayed in plain text after initial entry.                               |
| R-08 | The connection to GitHub must be validated before any AI feature activates.                                                                                       |
| R-09 | AI-generated suggestions must be clearly labelled so users know they are AI-produced.                                                                             |
| R-10 | The application must degrade gracefully when offline or when the GitHub API is unreachable — AI features hide, but all core receiving functions continue to work. |

---

## 3. GitHub Account and Copilot Tier Model

### 3.1 Authentication Method

The user provides a **GitHub Personal Access Token (PAT)** with the minimum required scopes.
The app stores the token in encrypted local settings (same mechanism used by
`Service_SettingsEncryptionService` in Module_Settings.Core for other sensitive values).

An alternative OAuth Device Flow path could be considered in a future revision but is
out of scope for this initial implementation.

### 3.2 Tier Detection

After a valid PAT is saved, the application checks the GitHub Copilot Billing API to
determine the user's subscription level. The result is cached locally and refreshed
on app startup and whenever the user manually re-validates.

| Detected Tier          | What It Means                                                                          |
| ---------------------- | -------------------------------------------------------------------------------------- |
| **None / No Copilot**  | GitHub account exists but has no active Copilot subscription.                          |
| **Copilot Free**       | The free tier bundled with public GitHub accounts. Limited monthly quota.              |
| **Copilot Individual** | Paid individual subscription. Higher or no quota. Full model selection.                |
| **Copilot Business**   | Organisation-managed subscription. Model availability may be restricted by org policy. |
| **Copilot Enterprise** | Enterprise-managed. Same as Business with advanced features.                           |
| **Unknown**            | API responded but tier could not be mapped. Treated conservatively as None.            |

### 3.3 Model Availability Matrix

The model picker in settings only shows models compatible with the detected tier. Attempting to use a higher-tier model than the account allows produces a clear "Upgrade your GitHub Copilot plan to use this model" message — not a cryptic API error.

On Free and Student plans, premium models may only be accessible in "Auto" mode (Copilot selects the model); manual selection of those models requires a paid plan.

#### Quota Definitions

| Plan             | Premium Requests/Month                  | Code Completions/Month |
| ---------------- | --------------------------------------- | ---------------------- |
| Free             | 50                                      | 2,000                  |
| Student          | 300 (Auto mode only for premium models) | Unlimited              |
| Individual / Pro | 300 (+ $0.04/extra)                     | Unlimited              |
| Pro+             | 1,500                                   | Unlimited              |
| Business         | 300 / user                              | Unlimited              |
| Enterprise       | 1,000 / user                            | Unlimited              |

> **"Premium Requests"** are consumed by Copilot Chat, agent mode, code review, and manual model selection. Inline code completions do not consume premium requests on paid tiers.

#### Model Availability & Quota Multipliers

##### 🚀 Fast and Cost-Efficient

| Model                    | Free (50 req/mo)  | Pro (300 req/mo) | Pro+ (1,500 req/mo) | Business   | Enterprise | Quota Multiplier |
| ------------------------ | ----------------- | ---------------- | ------------------- | ---------- | ---------- | ---------------- |
| GPT-5.4 mini             | ✓ (quota, manual) | ✓                | ✓                   | Org policy | Org policy | Low              |
| GPT-5 mini               | ✓ (quota, manual) | ✓                | ✓                   | Org policy | Org policy | Low              |
| Grok Code Fast 1         | ✓ (quota, manual) | ✓                | ✓                   | Org policy | Org policy | Low              |
| Gemini 3 Flash (Preview) | ✓ (quota, Auto)   | ✓                | ✓                   | Org policy | Org policy | 0.25×            |

##### 🧠 Versatile and Highly Intelligent

| Model             | Free (50 req/mo)  | Pro (300 req/mo) | Pro+ (1,500 req/mo) | Business   | Enterprise | Quota Multiplier |
| ----------------- | ----------------- | ---------------- | ------------------- | ---------- | ---------- | ---------------- |
| Claude Sonnet 4.6 | Auto only         | ✓                | ✓                   | Org policy | Org policy | 1×               |
| GPT-5.1           | Auto only         | ✓                | ✓                   | Org policy | Org policy | 1×               |
| Claude Sonnet 4   | Auto only         | ✓                | ✓                   | Org policy | Org policy | 1×               |
| Claude Sonnet 4.5 | Auto only         | ✓                | ✓                   | Org policy | Org policy | 1×               |
| Claude Haiku 4.5  | ✓ (quota, manual) | ✓                | ✓                   | Org policy | Org policy | Low              |
| GPT-5.2           | Auto only         | ✓                | ✓                   | Org policy | Org policy | 1×               |
| GPT-4.1           | ✓ (quota, manual) | ✓                | ✓                   | Org policy | Org policy | Low              |
| GPT-4o            | ✓ (quota, manual) | ✓                | ✓                   | Org policy | Org policy | Low              |

##### 💡 Most Powerful at Complex Tasks

| Model                    | Free (50 req/mo) | Pro (300 req/mo) | Pro+ (1,500 req/mo) | Business   | Enterprise | Quota Multiplier |
| ------------------------ | ---------------- | ---------------- | ------------------- | ---------- | ---------- | ---------------- |
| Claude Opus 4.6          | Auto only        | Pro+ only        | ✓                   | Org policy | Org policy | 10×              |
| Gemini 3.1 Pro (Preview) | Auto only        | Auto only        | ✓                   | Org policy | Org policy | High             |
| GPT-5.2-Codex            | Auto only        | ✓                | ✓                   | Org policy | Org policy | High             |
| GPT-5.3-Codex            | Auto only        | Pro+ only        | ✓                   | Org policy | Org policy | High             |
| GPT-5.4                  | Auto only        | Pro+ only        | ✓                   | Org policy | Org policy | High             |
| GPT-5.1-Codex-Max        | Auto only        | Pro+ only        | ✓                   | Org policy | Org policy | High             |
| Claude Opus 4.5          | Auto only        | Pro+ only        | ✓                   | Org policy | Org policy | 10×              |
| Gemini 3 Pro (Preview)   | Auto only        | Auto only        | ✓                   | Org policy | Org policy | High             |
| Gemini 2.5 Pro           | Auto only        | ✓                | ✓                   | Org policy | Org policy | High             |

#### Legend

**Status Indicators:**

- **✓ (quota, manual)** — Available on this tier; consumes from monthly premium request quota; model can be manually selected.
- **Auto only** — Copilot may route requests to this model automatically, but the user cannot manually select it on this tier.
- **Pro+ only** — Available for manual selection on Pro+ tier and above only.
- **Org policy** — Availability controlled by the organization admin (can be enabled or disabled per model).
- **(Preview)** — Model is in public preview; availability and behavior may change without notice.

#### Important Notes

- **Configuration-driven model list** — Model availability changes frequently. The model list, tier access, and quota multipliers must be driven by a configuration value (e.g., `appsettings.json`) rather than hard-coded, so they can be updated without a code change.

- **Quota multipliers** — Heavier models deplete the monthly budget faster. For example: a Pro user with 300 requests/month gets ~30 Claude Opus calls (10×) vs. ~1,200 Gemini Flash calls (0.25×).

- **Top-up purchases** — Free users cannot purchase additional premium requests; paid users can top up at $0.04 per extra request.

- **Preview models** — Models marked (Preview) are subject to change or removal at any time.

- **Authoritative source** — For the current live list, refer to the [GitHub Copilot Supported Models documentation](https://docs.github.com/en/copilot/supported-models) and the model picker in your IDE.

---

## 4. Usage Tracking (Quota Gauge)

### 4.1 What Gets Tracked

For tiers that have a monthly quota (primarily Free), the app shows a gauge identical
in spirit to the Copilot usage bar in Visual Studio:

```
GitHub Copilot Usage — March 2026
[████████████░░░░░░░░]  62%   1,240 / 2,000 completions used
[██████████░░░░░░░░░░]  47%      24 / 50 chat messages used
Quota resets in 11 days
```

For paid tiers with no hard quota, the gauge is replaced by a smaller informational
line showing approximate requests made this month (informational only, no cap warning).

### 4.2 Refresh Behaviour

- Quota is fetched from the GitHub API once per app session on startup.
- A manual **Refresh** button is available on the Usage settings page.
- After every AI interaction inside the app, the local counters are incremented
  optimistically. The next background refresh corrects any drift.
- When a user reaches 80 % of their quota, a non-blocking amber banner appears at the
  top of any page that triggers an AI call.
- At 100 %, the banner turns red and AI features are disabled until the quota resets or
  the user upgrades.

### 4.3 Quota Warning Thresholds

| Threshold  | Indicator                                                    |
| ---------- | ------------------------------------------------------------ |
| Below 80 % | No warning. Gauge visible only in settings.                  |
| 80 – 94 %  | Amber banner on AI-enabled pages. Gauge turns amber.         |
| 95 – 99 %  | Red banner. Gauge turns red.                                 |
| 100 %      | AI features hidden. Persistent red notice with Upgrade link. |

---

## 5. Module_Settings.AI — Settings Structure

Modelled after Module_Settings.Receiving (hub-and-spoke navigation, one focused page
per concern). Accessible from the main Settings window via a new **AI** section in the
navigation hub.

### 5.1 Navigation Hub

Identical in shape to `View_Settings_Receiving_WorkflowHub` — a list of cards that
link to each settings sub-page.

| Card            | Purpose                                                                |
| --------------- | ---------------------------------------------------------------------- |
| GitHub Account  | Connect or disconnect a GitHub account. Shows connection status badge. |
| AI Features     | Global on/off toggle and per-feature toggles.                          |
| Model Selection | Choose the default AI model. Shows tier badge and quota remaining.     |
| Usage           | Full quota gauge, request history summary, and manual refresh.         |

### 5.2 GitHub Account Page

Fields and controls:

| Element               | Type                           | Notes                                                                                      |
| --------------------- | ------------------------------ | ------------------------------------------------------------------------------------------ |
| Authenticated As      | Read-only display              | GitHub username + avatar, shown after valid connection.                                    |
| Personal Access Token | Masked text input              | Stored encrypted. Displays only a redacted preview (e.g. `ghp_••••••••••1a2b`) after save. |
| Required Scopes       | Info label                     | Describes minimum PAT scopes needed (read Copilot billing, read user profile).             |
| Connect / Re-validate | Primary button                 | Calls GitHub API to verify token and detect tier.                                          |
| Disconnect            | Secondary / destructive button | Clears token and disables all AI features.                                                 |
| Connection Status     | Status badge                   | Idle / Checking / Connected (tier name) / Error (reason).                                  |
| Last Validated        | Read-only                      | Timestamp of last successful API validation.                                               |

### 5.3 AI Features Page

| Element                     | Type   | Default | Notes                                                                                                 |
| --------------------------- | ------ | ------- | ----------------------------------------------------------------------------------------------------- |
| Enable AI Features (global) | Toggle | Off     | Master switch. When off, all sub-toggles are greyed out and no AI calls are made anywhere in the app. |
| AI in Receiving Workflow    | Toggle | Off     | Enables AI assistance inside Module_Receiving. Dependent on global toggle.                            |
| Smart PO Lookup             | Toggle | Off     | AI-assisted purchase order search and disambiguation.                                                 |
| Anomaly Detection           | Toggle | Off     | Flags receiving quantities or part numbers that look unusual based on historical patterns.            |
| Auto-Fill Suggestions       | Toggle | Off     | Suggests field values based on recent similar receipts.                                               |
| Natural Language Search     | Toggle | Off     | Allows searching POs and parts using plain English.                                                   |

> Sub-toggles within a module should only be visible when the parent module toggle is on,
> keeping the page uncluttered for users who enable only one feature area.

Permission level required to change these settings: **Supervisor** (aligns with the
`Enum_SettingsPermissionLevel` established in Module_Settings.Core).

### 5.4 Model Selection Page

| Element                | Type               | Notes                                                                                                               |
| ---------------------- | ------------------ | ------------------------------------------------------------------------------------------------------------------- |
| Subscription Tier      | Read-only badge    | Colour-coded: grey = None, green = Free, blue = Individual, gold = Business/Enterprise.                             |
| Default Model          | Dropdown           | Only shows models permitted by the detected tier.                                                                   |
| Model Card (per model) | Info card          | Displays model name, provider, strengths (speed vs reasoning), and a "Requires [tier]" lock badge if not available. |
| About Selected Model   | Expandable section | Short one-paragraph description of the model's characteristics.                                                     |

### 5.5 Usage Page

| Element                             | Type              | Notes                                                                                                           |
| ----------------------------------- | ----------------- | --------------------------------------------------------------------------------------------------------------- |
| Monthly Quota Gauge — Completions   | Progress bar      | Labelled with absolute numbers and %. Colour follows threshold rules in §4.3. Only shown when tier has a quota. |
| Monthly Quota Gauge — Chat Messages | Progress bar      | Same as above but for chat/question quota.                                                                      |
| Quota Period                        | Read-only         | "Resets on April 1, 2026 (12 days)".                                                                            |
| Requests This Session               | Read-only counter | How many AI calls were made in the current app session.                                                         |
| Requests This Month (All Sessions)  | Read-only counter | Best-effort local count; corrected by API refresh.                                                              |
| Refresh                             | Secondary button  | Fetches latest quota data from GitHub API immediately.                                                          |
| Upgrade Plan                        | Link              | Opens the GitHub Copilot plans page in the user's browser. Shown only when tier is Free or None.                |

---

## 6. AI Feature Integration — Module_Receiving

This section describes where and how AI surfaces during the receiving workflow.
All features respect the toggles defined in §5.3.

### 6.1 Smart PO Lookup

**Where:** The purchase order search field at the start of the receiving workflow.

**What it does:** When the user types a partial or ambiguous PO number (or types text
that doesn't match any PO exactly), the AI compares the input against recent PO history
and suggests the most likely match with a confidence indicator.

**UI pattern:** A small "AI Suggestion" chip appears below the search field with the
suggested PO number and a brief reason ("Matches vendor MTM-SUP-001, received
3 times in the last 30 days"). The user can accept or ignore it.

**Trigger:** Activated after the user pauses typing for ~800 ms with no exact match found.

### 6.2 Anomaly Detection

**Where:** After the user enters a received quantity and confirms a receiving line.

**What it does:** Compares the entered quantity against the approved quantity on the PO
line and against historical received quantities for the same part. Flags entries that
deviate significantly from the norm.

**UI pattern:** A non-blocking amber information card slides in below the quantity field:
"This quantity (500) is 4× the historical average for Part X3-44B. Please confirm this
is correct before proceeding." User can acknowledge and continue or cancel and correct.

**Trigger:** Activated when a receiving line is confirmed (before the final save).

### 6.3 Auto-Fill Suggestions

**Where:** Any free-text field in the receiving form that has historical data behind it
(carrier name, container ID format, etc.).

**What it does:** Offers autocomplete suggestions based on the most recent entries for
the same vendor or part number.

**UI pattern:** Standard autocomplete dropdown labelled with a small "AI" badge in the
corner to distinguish it from hardcoded picklists.

### 6.4 Natural Language Search

**Where:** A dedicated search bar added to the receiving dashboard / PO list view.

**What it does:** Lets users type plain English queries instead of exact PO numbers or
part IDs. Examples: "all receipts for Volvo parts this week", "open POs over $10,000
from vendor ABC".

**UI pattern:** Separate search bar clearly labelled "Ask AI" with a model indicator
badge. Results are presented as a filtered grid with an AI summary above it.

---

## 7. Settings Inventory (Key Registry)

Follows the same column format as `SETTABLE_OBJECTS_INVENTORY.md` in Module_Settings.Core.

| Setting Key                                 | Data Type          | Scope  | Permission | Default             | Notes                                                                                            |
| ------------------------------------------- | ------------------ | ------ | ---------- | ------------------- | ------------------------------------------------------------------------------------------------ |
| AI:GitHub:PersonalAccessToken               | String (encrypted) | User   | Supervisor | _(empty)_           | Stored using `Service_SettingsEncryptionService`. Never logged or displayed unmasked after save. |
| AI:GitHub:Username                          | String             | User   | Read-only  | _(empty)_           | Populated automatically after successful validation. Not user-editable.                          |
| AI:GitHub:LastValidatedUtc                  | DateTime           | User   | Read-only  | _(null)_            | UTC timestamp of last successful GitHub API validation.                                          |
| AI:GitHub:DetectedTier                      | String             | User   | Read-only  | `None`              | One of: `None`, `Free`, `Individual`, `Business`, `Enterprise`, `Unknown`.                       |
| AI:Features:GlobalEnabled                   | Boolean            | User   | Supervisor | `false`             | Master switch for all AI capabilities.                                                           |
| AI:Features:Receiving:Enabled               | Boolean            | User   | Supervisor | `false`             | Enables AI in Module_Receiving. Requires `GlobalEnabled = true`.                                 |
| AI:Features:Receiving:SmartPOLookup         | Boolean            | User   | Supervisor | `false`             | Smart PO lookup (§6.1).                                                                          |
| AI:Features:Receiving:AnomalyDetection      | Boolean            | User   | Supervisor | `false`             | Anomaly detection on receiving lines (§6.2).                                                     |
| AI:Features:Receiving:AutoFill              | Boolean            | User   | Supervisor | `false`             | Auto-fill suggestions on free-text fields (§6.3).                                                |
| AI:Features:Receiving:NaturalLanguageSearch | Boolean            | User   | Supervisor | `false`             | Plain-English PO/part search (§6.4).                                                             |
| AI:Model:DefaultModel                       | String             | User   | Supervisor | `claude-3-5-sonnet` | The model identifier to use for all AI calls. Validated against tier before save.                |
| AI:Usage:CompletionsUsed                    | Int32              | User   | Read-only  | `0`                 | Local cached count. Corrected by next API refresh.                                               |
| AI:Usage:CompletionsLimit                   | Int32              | User   | Read-only  | `0`                 | `0` means no hard cap (paid tier). Set from API.                                                 |
| AI:Usage:ChatMessagesUsed                   | Int32              | User   | Read-only  | `0`                 | Local cached count for chat-type interactions.                                                   |
| AI:Usage:ChatMessagesLimit                  | Int32              | User   | Read-only  | `0`                 | `0` means no hard cap.                                                                           |
| AI:Usage:PeriodResetDateUtc                 | DateTime           | User   | Read-only  | _(null)_            | When the current quota period resets. Fetched from API.                                          |
| AI:Usage:LastRefreshedUtc                   | DateTime           | User   | Read-only  | _(null)_            | When usage data was last fetched from GitHub.                                                    |
| AI:Usage:WarnThresholdPercent               | Int32              | System | Developer  | `80`                | Amber warning threshold (§4.3). Configurable via `appsettings.json`.                             |
| AI:Usage:BlockThresholdPercent              | Int32              | System | Developer  | `100`               | Red / block threshold. Set to 100 to match quota hard limit.                                     |
| AI:Model:AvailableModels                    | String[]           | System | Developer  | _(see appsettings)_ | JSON array of model descriptors loaded from `appsettings.json`. Not stored in settings DB.       |

---

## 8. Data Flow Summary

```
User fills GitHub PAT
        │
        ▼
GitHub Account Settings Page
  → Validate PAT via GitHub API
  → Detect Copilot tier
  → Cache tier + username locally (encrypted settings)
        │
        ▼
AI Features Settings Page
  → User enables Global + module-level toggles
  → Saved to settings store
        │
        ▼
Model Selection Settings Page
  → Tier-filtered model list presented
  → User picks default model
  → Saved to settings store
        │
        ▼
Module_Receiving (runtime)
  → On load: checks GlobalEnabled + Receiving.Enabled
  → If enabled: reads DefaultModel and quota remaining
  → If quota OK: AI entry points become visible in the UI
  → User interaction triggers AI call via GitHub Copilot API
  → Response rendered with "AI" label
  → Local usage counters incremented
        │
        ▼
Usage Settings Page (on demand)
  → Reads local counters + last-refreshed timestamp
  → Refresh button fetches fresh quota from GitHub API
```

---

## 9. Module Structure (Proposed)

Inspired by the hub-and-spoke shape of Module_Settings.Receiving and the
infrastructure patterns of Module_Settings.Core.

```
Module_Settings.AI/
  Contracts/
    Services/              ← Interfaces for GitHub auth, tier detection, usage service
  Data/                    ← DAO / API client wrappers for GitHub API calls
  Defaults/                ← Default values (models list, thresholds)
  Enums/                   ← Tier enumeration, model availability flags
  Models/                  ← GitHubAccount, CopilotTier, UsageSnapshot, ModelDescriptor
  Services/                ← Concrete implementations (auth, tier, usage, model registry)
  Settings/                ← Setting key constants, defaults class
  ViewModels/
    ViewModel_Settings_AI_NavigationHub
    ViewModel_Settings_AI_GitHubAccount
    ViewModel_Settings_AI_Features
    ViewModel_Settings_AI_ModelSelection
    ViewModel_Settings_AI_Usage
  Views/
    View_Settings_AI_NavigationHub
    View_Settings_AI_GitHubAccount
    View_Settings_AI_Features
    View_Settings_AI_ModelSelection
    View_Settings_AI_Usage
```

Module_Receiving changes are confined to existing Views and ViewModels — new AI
entry points are added as optional, toggle-gated expansions rather than rewrites.

---

## 10. Non-Goals (Out of Scope for This Revision)

- Custom fine-tuned models or locally-hosted LLMs.
- AI assistance outside Module_Receiving (Dunnage, Reporting, etc.) — can be added
  incrementally using the same framework.
- OAuth Device Flow authentication (PAT only for now).
- Bulk or background AI processing (all calls are on-demand, user-initiated).
- Usage analytics sent from the app to any MTM server.
- AI-generated receiving decisions — the AI only suggests; a human always confirms.

---

## 11. Open Questions

These are plain-language questions that need answers before development starts.
Each one is labelled with who is best placed to decide.

| #     | Plain-English Question                                                                                                                                                                                                                                                                                                        | Why It Matters                                                                                                           | Who Decides                                       |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------- |
| OQ-01 | **What password-level access does the app need to ask GitHub for?** When a user connects their GitHub account they give the app a "token" — a kind of password that only works for specific things. We need to confirm the exact minimum list of permissions that token needs so we don't ask for more access than necessary. | Security — asking for too many permissions is bad practice. Asking for too few means the feature won't work.             | Developer (needs to check GitHub docs)            |
| OQ-02 | **If a user's GitHub account is paid for by a company (Business/Enterprise tier), can they still pick which AI model to use?** Company GitHub plans sometimes lock employees to a specific AI model chosen by IT. If that's the case here, the model-picker screen may not be needed for those users.                         | Affects whether we build and show the model selection screen to all users or only personal-account users.                | IT Admin / whoever manages the company GitHub org |
| OQ-03 | **Should the GitHub login be tied to one specific Windows user on a PC, or to the whole machine?** If two people share the same Windows PC (e.g. a shared receiving terminal), do they each need their own GitHub account connected, or does one connection cover everyone who logs in to that PC?                            | Changes where and how we store the connection securely. Shared terminals are already a concept in the app's settings.    | Product Owner                                     |
| OQ-04 | **When a user types a plain-English search (e.g. "all open Volvo POs this week"), what should it be able to search across?** Options: just purchase orders, or also parts, vendors, and historical receipts. The broader the scope, the more data gets sent to the AI per search.                                             | Directly affects the privacy rule above — we need to agree on the minimum useful scope before building it.               | Product Owner                                     |
| OQ-05 | **When the app flags a received quantity as unusual, should the "unusual" threshold be a fixed rule or something a supervisor can adjust in settings?** For example: "flag anything more than 3× the historical average" — is 3× the right number for all parts, or should it vary?                                           | If it needs to be adjustable, a new settings page is required. If fixed, it's simpler to build.                          | Product Owner                                     |
| OQ-06 | **Should the app keep a log of every AI suggestion — what it suggested and whether the user accepted or ignored it?** For example: "On March 5, AI suggested PO-1234 for this receipt. User accepted." This would let a supervisor review AI influence on receiving decisions.                                                | Audit / quality control. Adds storage and UI work. Needed before going live if traceability is a compliance requirement. | Product Owner                                     |

---

## 12. Revision History

| Date       | Change                                                                                |
| ---------- | ------------------------------------------------------------------------------------- |
| 2026-03-20 | Initial draft created for review.                                                     |
| 2026-03-20 | Added MAJOR RULE — AI Data Privacy section. Rewrote Open Questions in plain language. |
