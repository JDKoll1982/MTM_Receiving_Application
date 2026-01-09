
# Kickoff: Module-Based Waitlist App (Stakeholder Version — Transcript Aligned)

## Executive Summary

This project delivers an **in-house, Windows desktop** waitlist system that connects to **MySQL** for application data and reads from **Visual ERP (SQL Server)** for work order validation and job context.

The core approach discussed in the meeting is **module-based design**: each department (Operator, Material Handling, Quality, Leads) has its own “module”, allowing changes for one group without breaking others.

## What We Heard in the Meeting (Key Outcomes)

- The app must stay **on our servers / in-house** and **not be a web app**.

- Visual ERP usage is **read-only** (no writing to Visual).

- The goal is to reduce wasted time caused by **incorrect coils/parts/ops** being requested.

- Operators should **not have to type** most inputs; they should **click from lists**.

- A **guided wizard** was requested for operators.

- Operators need **Favorites/Recents** for repeat requests.

- Leads need **analytics rights** (duration/usage visibility) that operators do not have.

- Time standards should be centrally controlled and refined using real data.

- Material Handling discussed **zone-based assignment** and optional **auto-assign** when tasks become urgent (“red”).

- Quality needs a workflow that does not require operators to leave the press; notifications may help but must respect security and practical adoption.

- Rollout must be controlled: **Version 1.0 should be as close to current Tables Ready as possible**, with enhancements after approval.

## Non‑Negotiables / Guardrails

- **No writes to Visual ERP**.

- **In-house only** (not web-hosted; no phone app).

- **Version 1.0 requires approval** before rollout.

```mermaid

flowchart  TB

A[Work Order data]  -->|Read-only| B[Visual ERP]

B  -->  C[Waitlist App]

C  -->  D[(MySQL - app data)]

  

C  -->  E[No web hosting]

C  -->  F[No phone apps]

C  -->  G[No writes to Visual]

```

## What “Module-Based” Means (Stakeholder View)

A module is the part of the application each group uses. It lets us change one group’s experience without disrupting others.

```mermaid

flowchart  LR

subgraph  Modules["Role Modules"]

Op[Operator Module]

MH[Material Handling Module]

Q[Quality Module]

Lead[Lead Module]

end

  

subgraph  Core["Core Services"]

Auth["Badge + PIN"]

Site["Site Context - device to site_id"]

Offline["Offline Queue"]

Notify["Notifications"]

Print["Label Printing"]

Audit["Audit + Reporting Data"]

end

  

Modules  -->  Core

Op  -->  Print

MH  -->  Audit

Q  -->  Notify

Lead  -->  Audit

```

## Core User Experience (Operator)

The operator experience must be **simple and low-typing**.

### Intended workflow

- Operator selects the **press**.

- The app pulls the relevant job context from Visual (read-only).

- The operator chooses what they need from a list or wizard.

- They can use **Favorites** or **Recents** to avoid repeated steps.

- If they select multiple items (e.g., skids + gaylords), the app creates **separate waitlist items** automatically.

```mermaid

flowchart  TB

A[Operator logs in]  -->  B[Select Press]

B  -->  C["App loads job context from Visual - read only"]

C  -->  D[Choose what you need]

D  -->  E{Use Favorites or Recents?}

E  -->|Yes| F[Pick favorite/recent request]

E  -->|No| G[Pick from guided list]

  

F  -->  H[Select one or more items via checkmarks]

G  -->  H

  

H  -->  I[Enter only what is needed: quantity and notes]

I  -->  J[Send]

J  -->  K[Creates 1..N separate requests on the shared waitlist]

```

## Shared Waitlist + Analytics Rights

- Everyone can **see the waitlist** and create requests.

- Leads have **analytics rights** that operators do not.

- Example: leads can see how long tasks took and trend data.

## Time Standards (“Red” Urgency)

The meeting discussed using time standards per request type and marking items urgent (“red”) as they approach the deadline.

- Time standards are not adjustable by operators.

- Time standards are tuned over time using real data.

```mermaid

flowchart  TB

A[Request created]  -->  B[Timer starts]

B  -->  C[Time standard by request type]

C  -->  D{Near deadline?}

D  -->|No| E[Normal]

D  -->|Yes: 5 minutes  remaining| F[Turns Red]

F  -->  G[Becomes auto-assign candidate if enabled]

```

## Material Handling (Zone Assignment + Auto-Assign Concept)

The meeting discussed reducing cherry-picking by enabling zone assignment and optional auto-assign rules.

```mermaid

flowchart  TB

A[Material Handler starts shift]  -->  B[Assign zones: A, B, C]

B  -->  C[Queue shows tasks by site]

C  -->  D{Auto-assign enabled?}

D  -->|No| E[Pick tasks]

D  -->|Yes| F[System assigns next task]

F  -->  G{Red task in zone?}

G  -->|Yes| H[Assign that task first]

G  -->|No| I[Assign best next task by proximity and priority]

```

## Quality Workflow + Alerts

The meeting discussed letting operators create a quality task without leaving the press.

```mermaid

flowchart  TB

A[Operator creates Quality task]  -->  B[Quality queue updates]

B  -->  C[Quality sees task in their module]

  

C  -->  D{Optional notification enabled?}

D  -->|No| E[No notification]

D  -->|Yes| F[Send Email]

D  -->|Yes| G[Send Teams]

  

N["No phone apps: must stay in house"]  -.->  D

```

Important realities from the meeting:

- Email sending can be restricted by security.

- Teams/email may not be monitored in real time; Quality process alignment is required.

- Intercom/phone-based paging was discussed but is not currently approved.

## Dunnage / Notes Reality

The meeting noted Visual does not reliably store dunnage in a structured way.

- We can keep dunnage entry in the workflow.

- Pulling from Visual notes only works if notes are written in a consistent format.

## Site Separation (Expo vs VITS)

- The waitlist must be separated by `site_id`.

- `site_id` refers to what building the Waitlist is being operated from (Expo Drive / Vits Drive).

- `site_id` is determined by the workstation running the app (IP/host mapping).

## Deployment + Release Governance

- Updates cannot be rolled out without leadership consent.

  - Leadership Being:
    - "Nick Wunsch" <NWunsch@mantoolmfg.com>
    - "Cristofer Muchowski" <CMuchowski@mantoolmfg.com>
    - "Brett Lusk" <blusk@mantoolmfg.com>
    - "Dan Smith" <DSmith@mantoolmfg.com>

- Version 1.0 must be approved before shop-floor release.

```mermaid

sequenceDiagram

participant  Stakeholders

participant  Dev

participant  Approvers  as  Nick/Chris/Dan

participant  Loader  as  MTM  Loader

participant  Users

  

Dev->>Stakeholders: Demo candidate build

Stakeholders->>Approvers: Approve for rollout?

alt  Approved

Approvers->>Loader: Publish to Live Folder

Loader-->>Users: Offer update + sync

Users->>Loader: Launch app

else  Not  approved

Approvers-->>Dev: Hold release and collect feedback

end

```

## Rollout Strategy (as discussed)

```mermaid

flowchart  TB

A[Phase 1: Version 1.0]

A  -->  B[Closest possible to current Tables Ready]

B  -->  C[Stabilize workflow + training \n Training Module to be included in Application]

  

A  -->  D[Phase 2: Enhancements]

D  -->  E[Guided wizard]

D  -->  F[Favorites/Recents]

D  -->  G[Zone auto-assign]

D  -->  H[Quality alerts]

D  -->  I[Lead analytics improvements]

```

## Training Expectation

The meeting highlighted that this is not a “checklist only” change.

- We will need a training plan (train-the-trainer, mentors, or structured sessions).

## Decisions Needed From Stakeholders (to move forward)

- Which roles get **analytics rights**, and what is visible.

- What **exact analytics** need to be reported.

- Who can adjust **time standards** and how often. **Time Standards** being the amount of time allocated to each type of request (Coil, Pickup, Die, ...)
  - As of right now we agreed to only:
    - "Cristofer Muchowski" <CMuchowski@mantoolmfg.com>
    - "Nick Wunsch" <NWunsch@mantoolmfg.com>

- Whether **auto-assign** is enabled and how strict it should be.

- Which notification methods are officially approved and monitored (Email, Teams).

- Ownership for maintaining workstation → `site_id` mapping.
