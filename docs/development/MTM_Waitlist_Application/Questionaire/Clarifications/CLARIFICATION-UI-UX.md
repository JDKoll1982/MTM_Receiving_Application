# Clarification Questions - UI/UX Design & Navigation

**Date**: January 21, 2026  
**Category**: User Interface & Experience  
**Priority**: Medium

---

## Overview

This document contains questions requiring clarification about the user interface design, navigation patterns, and visual design for the MTM Waitlist Application.

---

## 1. Navigation Architecture

### 1.1 Navigation Pattern

**Question**: What navigation pattern should the application use?

**Options**:

**Option A: NavigationView (Sidebar)**
```
+-------------------+---------------------------+
|  ☰ Waitlist App  |  Task Request - Step 2   |
|                   |                           |
| ⌂ Dashboard       |  [Content Area]          |
| ✚ Create Request  |                           |
| ≡ My Requests     |                           |
| ⚙ Settings        |                           |
|                   |                           |
| [User Info]       |                           |
+-------------------+---------------------------+
```

**Option B: TabView (Top Tabs)**
```
+------------------------------------------------+
| [Dashboard] [Create Request] [My Requests]    |
+------------------------------------------------+
|                                                |
|  [Content Area]                               |
|                                                |
|                                                |
+------------------------------------------------+
```

**Option C: Pivot/Segmented (Material Handler view)**
```
+------------------------------------------------+
| Material Handler Queue                         |
| [Zone 1] [Zone 2] [Zone 3] [Zone 4] [Zone 5] |
+------------------------------------------------+
|                                                |
|  [Task List for Selected Zone]                |
|                                                |
+------------------------------------------------+
```

**Need Clarification**:
- [ ] Should **different roles** use different navigation patterns?
- [ ] Should navigation be **persistent** (always visible) or **collapsible**?
- [ ] Should we use **breadcrumbs** for deep navigation?
- [ ] Should there be a **global search** in navigation bar?

**Impact**: Navigation service design, XAML layout templates

---

### 1.2 Module Access

**Question**: How do users navigate between modules?

**Example User Story**:
```
ProductionLead needs to:
1. View waitlist (WaitList module)
2. Check analytics (Analytics module)
3. Adjust settings (Settings module)

Question: How do they switch between these?
- Top-level navigation menu?
- Module launcher/home screen?
- Context menu?
```

**Need Clarification**:
- [ ] Should modules be **top-level menu items**?
- [ ] Should there be a **module switcher** (dropdown)?
- [ ] Can users have **multiple modules open** simultaneously (tabs/windows)?
- [ ] Should there be a **"Home"** screen with module tiles?

**Impact**: Navigation hierarchy, window management

---

### 1.3 Back Navigation

**Question**: How should back navigation work in wizards?

**Need Clarification**:
- [ ] Should there be a **browser-style back button**?
- [ ] Should wizard steps show **previous/next buttons**?
- [ ] Can users **jump to any completed step**?
- [ ] What happens if user **hits system back** (Backspace)?

**Impact**: Navigation service, wizard control design

---

## 2. Dashboard Design

### 2.1 Operator Dashboard

**Question**: What should operators see on their dashboard?

**Proposed Components**:
- [ ] **Quick Create Button** (large, prominent)
- [ ] **My Active Requests** (in-progress tasks)
- [ ] **Recent Requests** (clickable to re-submit)
- [ ] **Favorites** (saved templates)
- [ ] **Notifications** (task completed, updates)

**Layout Option A: Card-Based**
```
+------------------------------------------------+
| Welcome, John Doe                              |
| [Create New Request] button                    |
+------------------------------------------------+
| My Active Requests (3)        | Recent (5)     |
| - Coils for WO12345          | - Coils        |
| - Flatstock for WO12346      | - Dies         |
| - Inspection request         | - Scrap        |
|                              |                |
+------------------------------------------------+
| Favorites (4)                                  |
| [Coil Request] [Die Request] [Inspection]     |
+------------------------------------------------+
```

**Layout Option B: List-Focused**
```
+------------------------------------------------+
| [Create New Request] button                    |
+------------------------------------------------+
| Active Requests                                |
| ○ Coils for WO12345 - In Progress - Zone 1   |
| ○ Flatstock for WO12346 - Pending - Zone 2   |
+------------------------------------------------+
| Recent | Favorites                            |
| [Tabs with scrollable lists]                  |
+------------------------------------------------+
```

**Need Clarification**:
- [ ] Which layout is preferred?
- [ ] Should dashboard show **task status updates** in real-time?
- [ ] Should dashboard show **estimated completion time**?
- [ ] Should there be **charts/graphs** on operator dashboard?

**Impact**: Dashboard ViewModel complexity, real-time updates

---

### 2.2 Material Handler Dashboard

**Question**: What should material handlers see on their dashboard?

**Proposed Components**:
- [ ] **Assigned Tasks** (tasks assigned to me)
- [ ] **Available Tasks** (queue I can claim)
- [ ] **Zone Filter** (show only my zones)
- [ ] **Quick Add Button** (for non-requested work)
- [ ] **Current Task Timer** (time elapsed on active task)

**Layout**:
```
+------------------------------------------------+
| Material Handler - Zone 1, Zone 2             |
| [Quick Add] button                             |
+------------------------------------------------+
| My Tasks (2)          | Available Tasks (5)   |
| ● Coils - WO12345    | ○ Flatstock - WO12347 |
|   Started 5 min ago  |   Created 2 min ago   |
| ● Dies - WO12346     | ○ Dunnage - WO12348   |
|   Started 15 min ago |   Created 10 min ago  |
+------------------------------------------------+
| [Zone 1] [Zone 2] [All Zones]                 |
+------------------------------------------------+
```

**Need Clarification**:
- [ ] Should **My Tasks** and **Available Tasks** be separate lists or tabs?
- [ ] Should tasks show **priority indicators** (color, icon)?
- [ ] Should tasks show **operator who requested** it?
- [ ] Should there be a **map view** showing task locations?

**Impact**: Dashboard layout, data refresh frequency

---

### 2.3 Production Lead Dashboard

**Question**: What should production leads see on their dashboard?

**Proposed Components**:
- [ ] **Active Tasks Count** by category
- [ ] **Wait Time Metrics** (average, longest)
- [ ] **Handler Status** (available, busy, offline)
- [ ] **Quick Analytics** (charts)
- [ ] **Alerts** (overdue tasks, bottlenecks)

**Layout**:
```
+------------------------------------------------+
| Production Lead Dashboard                      |
+------------------------------------------------+
| Active Tasks                                   |
| Material Handler: 12 (Avg wait: 8 min)        |
| Setup Tech: 3 (Avg wait: 15 min)              |
| Quality: 2 (Avg wait: 5 min)                  |
+------------------------------------------------+
| Alerts (2)                                     |
| ⚠ Zone 3 has no available handlers            |
| ⚠ 3 tasks exceeding SLA                       |
+------------------------------------------------+
| [View Full Analytics] button                   |
+------------------------------------------------+
```

**Need Clarification**:
- [ ] Should lead dashboard be **read-only** or have **action buttons**?
- [ ] Should lead dashboard **auto-refresh**?
- [ ] Should charts be **interactive** (drill-down)?
- [ ] Should dashboard be **customizable** (choose widgets)?

**Impact**: Analytics service, real-time data feeds

---

## 3. List/Grid Views

### 3.1 Task List Display

**Question**: How should task lists be displayed?

**Option A: List View**
```
+------------------------------------------------+
| ● Coils                    WO12345  | Zone 1   |
|   Requested by John Doe    5 min ago          |
|   Part: ABC-123                               |
+------------------------------------------------+
| ○ Flatstock               WO12346  | Zone 2   |
|   Requested by Jane Smith  2 min ago          |
|   Part: XYZ-789                               |
+------------------------------------------------+
```

**Option B: Grid View**
```
+------------------------------------------------+
| Category | Type  | WO     | Zone | Wait | Action |
|----------|-------|--------|------|------|--------|
| MH       | Coils | 12345  | Z1   | 5min | [Claim]|
| MH       | Flat  | 12346  | Z2   | 2min | [Claim]|
+------------------------------------------------+
```

**Option C: Card View**
```
+------------------------------------------------+
| +------------------------+  +------------------+
| | Coils                 |  | Flatstock        |
| | WO: 12345             |  | WO: 12346        |
| | Zone 1 - 5 min        |  | Zone 2 - 2 min   |
| | [Claim] [Details]     |  | [Claim] [Details]|
| +------------------------+  +------------------+
+------------------------------------------------+
```

**Need Clarification**:
- [ ] Which view is default?
- [ ] Can users **toggle between views** (list/grid/card)?
- [ ] Should view preference be **saved per user**?
- [ ] What **sort options** should be available?

**Impact**: List control templates, user preferences storage

---

### 3.2 List Interactions

**Question**: How should users interact with task lists?

**Need Clarification**:
- [ ] Should clicking a row **open details** or **select the row**?
- [ ] Should there be **quick actions** (swipe gestures, right-click menu)?
- [ ] Can users **select multiple tasks** for bulk operations?
- [ ] Should there be **inline editing** (change priority in list)?
- [ ] Should lists support **drag-and-drop** (reorder, reassign)?

**Impact**: Event handlers, multi-selection UI

---

### 3.3 Infinite Scroll vs Pagination

**Question**: How should long lists be handled?

**Options**:

**Option A: Infinite Scroll**
- Load more as user scrolls
- Seamless experience
- Risk: performance with thousands of items

**Option B: Pagination**
- Fixed page size (e.g., 20 items)
- Clear "Next/Previous" buttons
- Better performance

**Option C: Virtual Scrolling**
- Render only visible items
- Best performance
- More complex implementation

**Need Clarification**:
- [ ] Which approach is preferred?
- [ ] What **page size** (if pagination)?
- [ ] Should there be **"Jump to page"** functionality?

**Impact**: Performance, UI framework choice

---

## 4. Forms & Input Controls

### 4.1 Dropdown vs Autocomplete

**Question**: How should users select categories, types, zones?

**Option A: Standard Dropdown**
```
Category: [Material Handler ▼]
```

**Option B: Autocomplete/Typeahead**
```
Category: [Material Handler_____]
          Dropdown appears as you type:
          - Material Handler
          - Setup Technician
```

**Option C: Button Group (for few options)**
```
Category: [Material Handler] [Setup Tech] [Quality] [Prod Lead]
```

**Need Clarification**:
- [ ] Which control for **category selection**?
- [ ] Which control for **request type selection**?
- [ ] Which control for **zone selection**?
- [ ] Should autocomplete support **fuzzy matching**?

**Impact**: User experience, input validation

---

### 4.2 Work Order Entry

**Question**: How should work order input work?

**Need Clarification**:
- [ ] Should there be **barcode scanning support** (USB scanner)?
- [ ] Should there be **autocomplete from recent work orders**?
- [ ] Should there be **validation feedback** (green checkmark if valid)?
- [ ] Should invalid work orders be **highlighted in red**?
- [ ] Should system **suggest similar work orders** if exact match not found?

**Proposed UI**:
```
Work Order: [WO12345_______] [🔍 Scan]

✓ Valid work order
  Part: ABC-123 - Widget Assembly
  Status: Active
  Due Date: 2026-01-25
```

**Impact**: Validation service, barcode integration

---

### 4.3 Multi-Line Text Input

**Question**: How should description/notes fields work?

**Need Clarification**:
- [ ] Should descriptions have **character limit**?
- [ ] Should there be **word count** indicator?
- [ ] Should text box **expand** as user types?
- [ ] Should there be **formatting options** (bold, bullet points)?
- [ ] Should there be **spell check**?

**Impact**: Text control features, validation

---

## 5. Visual Design

### 5.1 Color Scheme

**Question**: What color scheme should the app use?

**Need Clarification**:
- [ ] Should we match **MTM Receiving Application** colors?
- [ ] Should we use **company brand colors**?
- [ ] Should there be **dark mode** support?
- [ ] Should priority levels have **specific colors** (red=critical)?
- [ ] Should categories have **color coding**?

**Proposed Color Usage**:
```
Priority Colors:
- Critical: Red (#DC3545)
- Urgent: Orange (#FD7E14)
- Normal: Blue (#0D6EFD)

Category Colors:
- Material Handler: Green
- Setup Tech: Yellow
- Quality: Purple
- Production Lead: Orange

Status Colors:
- Pending: Gray
- In Progress: Blue
- Completed: Green
- Cancelled: Red
```

**Impact**: Theme resources, accessibility (color blindness)

---

### 5.2 Icons

**Question**: What icon style should be used?

**Options**:
- [ ] **Fluent UI Icons** (Microsoft's default for WinUI 3)
- [ ] **Material Icons** (Google's design)
- [ ] **Custom Icons** (company-specific)
- [ ] **Font Awesome** (third-party)

**Need Clarification**:
- [ ] Should icons be **outlined** or **filled**?
- [ ] Should icons have **color** or be monochrome?
- [ ] Should all buttons have **icons + text** or just icons?

**Impact**: Asset management, consistency

---

### 5.3 Typography

**Question**: What fonts and sizes should be used?

**Proposed Typography Scale**:
```
Title: 28pt Segoe UI Semibold
Subtitle: 20pt Segoe UI Semibold
Header: 16pt Segoe UI Semibold
Body: 14pt Segoe UI Regular
Caption: 12pt Segoe UI Regular
```

**Need Clarification**:
- [ ] Should we use **default Windows fonts** (Segoe UI)?
- [ ] Should we use **custom fonts** (company branding)?
- [ ] Should font sizes be **adjustable** (accessibility)?
- [ ] Should there be **different scales for touch** vs mouse?

**Impact**: Accessibility, brand consistency

---

## 6. Notifications & Feedback

### 6.1 In-App Notifications

**Question**: How should in-app notifications appear?

**Options**:

**Option A: Toast Notifications (System)**
```
[Bottom-right corner popup]
✓ Task completed: Coils for WO12345
  [View Details]
```

**Option B: In-App Banner**
```
+------------------------------------------------+
| ℹ Your task has been assigned to John Doe     |
| [Dismiss] [View Details]                       |
+------------------------------------------------+
```

**Option C: Notification Center**
```
[Bell icon in header with badge: 3]
Click to open panel:
- Task completed (2 min ago)
- New task assigned (5 min ago)
- Task cancelled (10 min ago)
```

**Need Clarification**:
- [ ] Which notification style?
- [ ] Should notifications **auto-dismiss** or require user action?
- [ ] Should there be **notification history**?
- [ ] Should notifications be **persistent** across sessions?
- [ ] Should **sound alerts** be optional?

**Impact**: Notification service, user preferences

---

### 6.2 Progress Indicators

**Question**: How should long-running operations be indicated?

**Need Clarification**:
- [ ] Should there be a **global loading spinner**?
- [ ] Should there be **progress bars** for multi-step operations?
- [ ] Should there be **skeleton screens** while loading lists?
- [ ] Should failed operations show **retry button**?

**Impact**: Loading UI patterns, error handling

---

### 6.3 Validation Feedback

**Question**: How should form validation errors be displayed?

**Option A: Inline Errors**
```
Work Order: [_______]
❌ Work order is required
```

**Option B: Summary Panel**
```
⚠ Please fix the following errors:
- Work order is required
- Zone selection is required
```

**Option C: Both**
```
Work Order: [_______]
❌ Work order is required

[Bottom of form]
⚠ 2 errors found. Please review above.
```

**Need Clarification**:
- [ ] Which validation approach?
- [ ] Should errors **prevent navigation** to next step?
- [ ] Should there be **warning vs error** distinction?

**Impact**: Validation service, UX flow

---

## 7. Accessibility

### 7.1 Touch vs Mouse

**Question**: Should the UI optimize for touch or mouse?

**Need Clarification**:
- [ ] Will app be used on **touchscreens**?
- [ ] Should touch targets be **larger** (44x44 pixels minimum)?
- [ ] Should there be **different layouts** for touch vs mouse?
- [ ] Should **swipe gestures** be supported?

**Impact**: Control sizing, interaction patterns

---

### 7.2 Keyboard Navigation

**Question**: How should keyboard navigation work?

**Need Clarification**:
- [ ] Should all controls be **keyboard accessible**?
- [ ] Should there be **keyboard shortcuts** (Ctrl+N for new request)?
- [ ] Should **Tab order** be optimized for efficiency?
- [ ] Should there be **visible focus indicators**?

**Impact**: Accessibility compliance, power user efficiency

---

### 7.3 Screen Reader Support

**Question**: What level of screen reader support is needed?

**Need Clarification**:
- [ ] Are there **visually impaired users**?
- [ ] Should all UI elements have **ARIA labels**?
- [ ] Should images have **alt text**?
- [ ] Should there be **audio cues** for important events?

**Impact**: Accessibility implementation, compliance

---

## 8. Responsive Design

### 8.1 Window Sizing

**Question**: Should the app support different window sizes?

**Need Clarification**:
- [ ] Should app be **full screen** or **windowed**?
- [ ] What is the **minimum window size**?
- [ ] Should layout **adapt** to window size (responsive)?
- [ ] Can users **resize** windows?
- [ ] Should window size/position be **saved per user**?

**Proposed Sizes**:
- Minimum: 1024x768
- Default: 1280x800
- Maximum: Full screen

**Impact**: XAML adaptive layouts, window management

---

### 8.2 Multi-Monitor Support

**Question**: How should multi-monitor setups work?

**Need Clarification**:
- [ ] Can users **open multiple windows** (one per monitor)?
- [ ] Should app **remember monitor placement**?
- [ ] Should there be a **"detach to new window"** feature?

**Impact**: Window management service

---

## 9. Performance & Responsiveness

### 9.1 Real-Time Updates

**Question**: How should data refresh in real-time?

**Need Clarification**:
- [ ] Should lists **auto-refresh** (polling interval)?
- [ ] Should we use **SignalR** for real-time push notifications?
- [ ] Should auto-refresh be **pausable** (when user is interacting)?
- [ ] Should there be **visual indicator** of refresh (loading spinner)?

**Proposed Refresh Strategies**:
```
Dashboard: Auto-refresh every 30 seconds
Task List: Auto-refresh every 15 seconds
Task Details: Auto-refresh every 10 seconds (if task is active)
Analytics: Manual refresh only (expensive queries)
```

**Impact**: SignalR setup, server load, user experience

---

### 9.2 UI Responsiveness

**Question**: How should the UI handle slow operations?

**Need Clarification**:
- [ ] Should buttons be **disabled during operations**?
- [ ] Should there be a **global busy indicator**?
- [ ] Should operations be **cancellable**?
- [ ] What **timeout** for operations before showing error?

**Impact**: Loading states, error handling

---

## 10. Help & Onboarding

### 10.1 In-App Help

**Question**: How should help content be accessed?

**Need Clarification**:
- [ ] Should there be a **Help menu** with topics?
- [ ] Should there be **context-sensitive help** (? icon on forms)?
- [ ] Should there be **tooltips** on all controls?
- [ ] Should there be **video tutorials** embedded?
- [ ] Should help content be **searchable**?

**Impact**: Help content management, UI space

---

### 10.2 First-Time User Experience

**Question**: How should new users be onboarded?

**Options**:

**Option A: Welcome Wizard**
```
On first login:
1. Welcome screen
2. Role selection confirmation
3. Quick tour of key features
4. Create your first request (guided)
```

**Option B: Tooltips & Highlights**
```
On first login:
- Highlight "Create Request" button
- Show tooltip: "Click here to create your first request"
- After first request, show next feature
```

**Option C: No Onboarding**
```
Assume users will be trained
Rely on help documentation
```

**Need Clarification**:
- [ ] Which onboarding approach?
- [ ] Should onboarding be **skippable**?
- [ ] Should onboarding be **replayable** (Help > Show Tour)?

**Impact**: Onboarding service, user training requirements

---

## Action Items

### Critical (Before UI Development)
1. [ ] Confirm navigation pattern (sidebar vs tabs)
2. [ ] Finalize dashboard layouts for each role
3. [ ] Choose list display format (list vs grid vs cards)
4. [ ] Define color scheme and icon set

### High Priority (Before Alpha)
5. [ ] Specify notification styles
6. [ ] Define form validation approach
7. [ ] Determine dropdown vs autocomplete usage
8. [ ] Specify real-time refresh strategy

### Medium Priority (Before Beta)
9. [ ] Accessibility requirements (keyboard, screen reader)
10. [ ] Touch vs mouse optimization
11. [ ] Window sizing and multi-monitor support
12. [ ] In-app help system design

### Low Priority (Before Production)
13. [ ] Onboarding flow
14. [ ] Dark mode support
15. [ ] Custom font choices
16. [ ] Advanced keyboard shortcuts

---

## Next Steps

1. **Review with UX Designer**: Validate design decisions
2. **Create Mockup Refinements**: Update HTML mockups based on decisions
3. **User Testing**: Show mockups to operators/handlers for feedback
4. **Design System Documentation**: Create XAML resource dictionary
5. **Implement Navigation Service**: Code navigation framework
6. **Build UI Templates**: Create reusable control templates

---

**Document Owner**: UX Designer / UI Architect  
**Review Date**: [To Be Scheduled]  
**Status**: Pending Stakeholder Input
