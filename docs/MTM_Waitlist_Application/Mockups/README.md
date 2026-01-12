# MTM Waitlist Application - WinUI3 HTML Mockups

## Overview

This directory contains HTML mockups of the MTM Waitlist Application that visually simulate WinUI3 UI/UX patterns using web technologies. These mockups are designed for stakeholder review, UX validation, and as a blueprint for WinUI3/XAML implementation.

## Architecture

### Design Principles

1. **WinUI3 Visual Language**: All mockups use Fluent Design System patterns
2. **Compile-Time Binding Simulation**: Demonst rates how `x:Bind` would work in XAML
3. **Role-Based Navigation**: Each module is self-contained for different user roles
4. **Responsive Components**: Optimized for 1920x1080 desktop displays
5. **Accessibility**: ARIA labels, keyboard navigation, high contrast support

### Technology Stack

- **HTML5**: Semantic markup
- **CSS3**: Fluent Design styling with CSS Grid/Flexbox
- **Vanilla JavaScript**: Minimal interactivity, mock data binding
- **Fluent UI Icons**: Microsoft's Segoe Fluent Icons font

## Project Structure

```
Mockups/
│
├── index.html                  # Navigation hub (or Plant Manager dashboard)
├── README.md                   # This file
├── MockupPrompt.md             # Original specification
│
├── Shared/                     # Shared resources
│   ├── styles.css              # Main WinUI3-inspired styles
│   ├── winui3-theme.css        # Theme variables & icons
│   └── mock-data.js            # Sample data for all modules
│
├── Login/                      # Authentication module
│   └── login.html              # Badge + PIN login screen
│
├── Operator/                   # Press operator module
│   ├── waitlist.html           # Main waitlist view
│   ├── active-jobs.html        # Currently active jobs
│   └── recent.html             # Recent completed requests
│
├── MaterialHandler/            # Material handler module
│   ├── waitlist.html           # Unified task waitlist (all departments)
│   ├── my-tasks.html           # Assigned tasks
│   ├── recent.html             # Completed task history
│   └── floor-plan.html         # Facility map (placeholder)
│
├── MaterialHandlerLead/        # MH lead module (+ analytics)
│   ├── waitlist.html           # Waitlist with analytics panel
│   ├── analytics.html          # Full analytics dashboard
│   ├── team-management.html    # Handler assignments & status
│   └── controls.html           # Zone Mode / Auto-Assign toggles
│
├── OperatorLead/               # Operator lead module (+ press analytics)
│   ├── waitlist.html           # Press floor waitlist
│   ├── press-analytics.html    # Press utilization & trends
│   ├── operator-management.html # Operator status & performance
│   └── trends.html             # Historical trends (SVG charts)
│
└── PlantManager/               # Plant manager module (unified admin)
    ├── dashboard.html          # Unified dashboard (MH + Press)
    ├── mh-analytics.html       # Material handling analytics
    ├── press-analytics.html    # Press floor analytics
    ├── reports.html            # Combined reporting
    └── user-management.html    # User roles & permissions
```

## Module Descriptions

### 1. Login Module

**Purpose**: Authenticate users via Badge + PIN

**Features**:
- Barcode scanner support (keyboard wedge simulation)
- Role auto-detection from badge
- Manual role override option
- Remember me functionality
- Error handling with InfoBar

**WinUI3 Controls**: ContentDialog, TextBox, PasswordBox, ComboBox, Button

---

### 2. Operator Module

**Purpose**: Press floor operators request materials and view job status

**Target Users**: Press operators, setup technicians

**Key Features**:
- **Simple workflow**: Minimal typing, dropdown-driven
- **Guided wizard**: Step-by-step request creation
- **Favorites/Recents**: Quick repeat requests (future phase)
- **Work order context**: Auto-filled from Visual ERP (read-only)
- **Request types**: Coils, parts, skids, gaylords, die changes

**Views**:

#### waitlist.html
- Table of operator's active requests
- Columns: Position, Request Type, Details, Wait Time, Status, Actions
- Action buttons: Edit, Cancel
- New Request button → opens wizard modal
- Floating Action Button (FAB) when items exist

#### active-jobs.html
- Currently running jobs on operator's press
- Work order details from Visual ERP
- Material requirements display

#### recent.html
- Completed request history
- Filter by date range
- Completion time tracking

**WinUI3 Equivalents**:
- NavigationView (sidebar)
- ListView with GridView (tables)
- CommandBar (top actions)
- ContentDialog (modals)
- AppBarButton (action icons)

---

### 3. Material Handler Module

**Purpose**: Unified task management across ALL departments

**Target Users**: Material handlers, forklift operators

**Key Features**:
- **Unified waitlist**: Tasks from Receiving, Dunnage, Routing, Press Floor
- **Color-coded task types**: Blue (Receiving), Green (Dunnage), Orange (Routing), Red (Press)
- **Zone filtering**: View tasks by assigned zone
- **Quick Add**: Manual task logging for credit tracking
- **Task acceptance**: Pick tasks from queue
- **Floor plan view**: Visual facility map (placeholder)

**Views**:

#### waitlist.html
- Unified task list from all departments
- Columns: Task Type, Location, Load #, Priority, Wait Time, Requester, Actions
- Task type badges with color coding
- Zone filter dropdown
- Action buttons: Accept Task, Need Help

#### my-tasks.html
- Currently assigned tasks
- Task timer tracking
- Mark complete button

#### recent.html
- Completed task log
- Duration tracking
- Handler credit accounting

#### floor-plan.html
- Facility map with zone overlays
- Active task locations highlighted
- Handler current positions (future: real-time)

**Unique Features**:
- Aggregates from 4 different department workflows
- Zone-based task filtering
- Priority escalation (green → yellow → red)

---

### 4. Material Handler Lead Module

**Purpose**: All MH functionality + team analytics and controls

**Target Users**: Material handling leads, supervisors

**Key Features** (extends Material Handler):
- **Analytics dashboard**: Real-time team performance metrics
- **SVG placeholder charts**: Task volume, handler performance, wait times
- **Zone Mode toggle**: Enable/disable zone-based assignment
- **Auto-Assign toggle**: Automatically assign urgent tasks
- **Team management**: View all handler statuses and locations
- **Manual assignment**: Drag-drop or button-based task assignment

**Views**:

#### waitlist.html
- Same as Material Handler waitlist
- **PLUS**: Analytics panel (collapsible sidebar or split view)
- Shows: Avg wait time, tasks completed today, active handlers

#### analytics.html
- Full-screen analytics dashboard
- Charts (SVG placeholders):
  - Task volume by type (bar chart)
  - Handler performance comparison (bar chart)
  - Wait time trends (line chart)
  - Zone distribution (pie chart)
- Filter by date range, shift, zone

#### team-management.html
- Table of all material handlers
- Columns: Name, Zone, Status, Tasks Today, Last Activity
- Status indicators: Active, On Break, Offline
- Reassign zone button

#### controls.html
- System settings (lead-only access)
- **Zone Mode**: ON/OFF toggle with description
- **Auto-Assign**: ON/OFF toggle with urgency threshold slider
- Time standard adjustments (requires approval)
- Notification preferences

**SVG Chart Example** (Task Volume by Type):
```html
<svg viewBox="0 0 400 200" class="chart-svg">
  <!-- Bars representing Receiving, Dunnage, Routing, Press -->
  <rect x="60" y="80" width="60" height="100" fill="#0078D4"/>
  <text x="90" y="195" text-anchor="middle">Receiving</text>
  <!-- ... more bars -->
</svg>
```

---

### 5. Operator Lead Module

**Purpose**: All Operator functionality + press floor analytics

**Target Users**: Production leads, press floor supervisors

**Key Features** (extends Operator):
- **Press analytics dashboard**: Utilization, downtime, job completion
- **SVG placeholder charts**: Press performance, operator metrics
- **Operator management**: View all operator statuses
- **Job reassignment**: Override assignments
- **Priority overrides**: Manually escalate requests

**Views**:

#### waitlist.html
- Press floor waitlist (all presses, not just one operator)
- Columns: Press, Operator, Request Type, Wait Time, Priority, Status
- Filter by press, shift, priority

#### press-analytics.html
- Press utilization charts (SVG):
  - Uptime vs downtime (pie chart)
  - Jobs completed per press (bar chart)
  - Average cycle time (line chart)
- Filter by date range, press

#### operator-management.html
- Table of all press operators
- Columns: Name, Press, Status, Jobs Today, Current Job
- Status: Running, Setup, Break, Offline
- Clock-in/out times

#### trends.html
- Historical trend analysis
- Charts (SVG):
  - Job completion trends (line chart)
  - Downtime reasons (stacked bar chart)
  - Part/job analytics

---

### 6. Plant Manager Module

**Purpose**: Unified admin view of entire plant operations

**Target Users**: Plant manager, operations director

**Key Features**:
- **Unified dashboard**: Both MH and Press analytics combined
- **Master override controls**: All lead functions accessible
- **User management**: Assign roles, manage permissions
- **Cross-department reports**: Combined MH + Press metrics
- **Approval workflows**: Time standard changes, system updates

**Views**:

#### dashboard.html
- Tabbed interface:
  - **Overview**: Plant-wide KPIs
  - **Material Handling**: MH metrics summary
  - **Press Floor**: Press metrics summary
- Charts (SVG):
  - Plant-wide task volume (combined)
  - Department comparison (side-by-side bars)
  - Overall efficiency score

#### mh-analytics.html
- Same as Material Handler Lead analytics
- Additional: Cross-site comparison (Expo vs VITS)

#### press-analytics.html
- Same as Operator Lead analytics
- Additional: Multi-shift trends

#### reports.html
- Report generation interface
- Filters: Date range, department, site
- Export options: CSV, PDF (mock buttons)
- Scheduled reports configuration

#### user-management.html
- Table of all users
- Columns: Badge ID, Name, Role, Site, Status, Last Login
- Actions: Edit role, Reset PIN, Deactivate
- Audit log viewer

---

## WinUI3 Control Mapping

This table shows how HTML/CSS mockup elements map to actual WinUI3 XAML controls:

| HTML Element | CSS Class | WinUI3 XAML Control |
|--------------|-----------|---------------------|
| `<nav class="nav-pane">` | `.nav-pane` | `NavigationView` |
| `<a class="nav-item-link">` | `.nav-item-link` | `NavigationViewItem` |
| `<div class="command-bar">` | `.command-bar` | `CommandBar` |
| `<button class="btn">` | `.btn` | `Button` |
| `<button class="btn-icon">` | `.btn-icon` | `AppBarButton` |
| `<table class="winui-table">` | `.winui-table` | `ListView` with `GridView` |
| `<div class="card">` | `.card` | `Card` / `Expander` |
| `<div class="dialog-overlay">` | `.dialog` | `ContentDialog` |
| `<div class="search-box">` | `.search-box` | `AutoSuggestBox` |
| `<div class="info-bar">` | `.info-bar` | `InfoBar` |
| `<button class="fab">` | `.fab` | Custom `Button` (circular) |
| `<label class="toggle-switch">` | `.toggle-switch` | `ToggleSwitch` |
| `<select class="form-select">` | `.form-select` | `ComboBox` |
| `<input class="form-input">` | `.form-input` | `TextBox` |
| `<textarea class="form-textarea">` | `.form-textarea` | `TextBox` (multiline) |

---

## Shared Components

### Shared/styles.css

**Purpose**: Main stylesheet with WinUI3-inspired component styles

**Key Sections**:
1. **App Shell**: NavigationView layout grid
2. **Navigation Pane**: Sidebar menu styles
3. **Content Area**: Main page container
4. **Command Bar**: Top action bar
5. **Tables**: ListView/GridView simulation
6. **Buttons**: Standard, Accent, Icon variants
7. **Cards & Panels**: Container styles
8. **Forms**: Input controls
9. **Modals**: ContentDialog overlay
10. **Floating Action Button**: FAB styles
11. **Status Bar**: Bottom info bar
12. **Utility Classes**: Spacing, flex, visibility

**Design System Adherence**:
- 4px spacing grid
- Fluent Design corner radius (4px, 8px, 12px)
- Acrylic backgrounds with blur
- Elevation shadows (card, flyout, dialog)
- Smooth transitions (0.1s - 0.2s)

### Shared/winui3-theme.css

**Purpose**: CSS custom properties for Fluent Design theme

**Variables**:
- **Accent Colors**: Default, Secondary, Tertiary, Disabled
- **Status Colors**: Success, Warning, Error, Info
- **Task Type Colors**: Receiving (Blue), Dunnage (Green), Routing (Orange), Press (Red)
- **Neutral Colors**: Light/Dark mode backgrounds, text, borders
- **Shadows**: Card, Flyout, Dialog elevations
- **Typography**: Segoe UI Variable font, size scale
- **Spacing**: XS to XXL (4px to 24px)
- **Corner Radius**: Small, Medium, Large

**Dark Mode Support**:
```css
@media (prefers-color-scheme: dark) {
  /* Override variables for dark theme */
}
```

**Fluent UI Icons**:
- Unicode character mappings
- Common icon classes (`.icon-add`, `.icon-edit`, etc.)
- Font: Segoe Fluent Icons / Segoe MDL2 Assets

### Shared/mock-data.js

**Purpose**: Sample data for populating mockups

**Data Objects**:
1. **currentUser**: Session context (id, name, role, site)
2. **sites**: Site list (Expo Drive, VITS Drive)
3. **presses**: Press equipment list
4. **zones**: Material handling zones
5. **taskTypes**: MH task categories with colors
6. **requestTypes**: Operator request types with time standards
7. **operatorWaitlist**: Sample operator requests
8. **materialHandlerWaitlist**: Unified MH task list
9. **recentTasks**: Completed task history
10. **analytics**: Lead/manager metrics (tasks, times, performance)
11. **materialHandlers**: Team roster
12. **pressOperators**: Operator roster
13. **settings**: System configuration flags

**Utility Functions** (`MockUtils`):
- `getWaitTimeStatus(waitTime, standard)`: Calculate urgency (normal/warning/critical)
- `formatWaitTime(minutes)`: Display format (e.g., "41 min", "1h 23m")
- `formatTimestamp(dateString)`: Localized date/time
- `getPriorityClass(priority)`: CSS class for priority badges
- `filterByZone(waitlist, zone)`: Zone-based filtering
- `filterByTaskType(waitlist, type)`: Task type filtering
- `updateFABVisibility(length)`: Show/hide FAB based on waitlist count

---

## Key Features by Module

### Floating Action Button (FAB)

**Show When**: Waitlist views have items
**Hide When**: Waitlist is empty
**Action**: Open "New Request" wizard

```javascript
// In waitlist.html
MockUtils.updateFABVisibility(waitlistItems.length);
```

**CSS**:
```css
.fab {
  position: fixed;
  bottom: 24px;
  right: 24px;
  background-color: var(--fab-background);
  /* ... */
}
```

### SVG Placeholder Charts

**Used in**: Lead and Manager modules

**Example** (Bar Chart - Task Volume):
```html
<div class="chart-container">
  <h3>Task Volume by Type</h3>
  <svg viewBox="0 0 400 200" class="chart-svg">
    <!-- Axes -->
    <line x1="40" y1="180" x2="380" y2="180" stroke="#EDEBE9" stroke-width="2"/>
    <line x1="40" y1="20" x2="40" y2="180" stroke="#EDEBE9" stroke-width="2"/>
    
    <!-- Bars -->
    <rect x="60" y="80" width="60" height="100" fill="#0078D4" opacity="0.8"/>
    <text x="90" y="195" text-anchor="middle" font-size="12">Receiving</text>
    
    <rect x="140" y="100" width="60" height="80" fill="#107C10" opacity="0.8"/>
    <text x="170" y="195" text-anchor="middle" font-size="12">Dunnage</text>
    
    <!-- ... more bars -->
  </svg>
</div>
```

**Chart Types**:
- **Bar Chart**: Task volume, handler performance
- **Line Chart**: Wait time trends, completion trends
- **Pie Chart**: Press utilization, zone distribution
- **Stacked Bar**: Downtime reasons, shift comparison

---

## Navigation Flow

```
Login (login.html)
    ↓ (Role Detection)
    ├─→ Operator → Operator/waitlist.html
    ├─→ Material Handler → MaterialHandler/waitlist.html
    ├─→ Material Handler Lead → MaterialHandlerLead/waitlist.html
    ├─→ Operator Lead → OperatorLead/waitlist.html
    ├─→ Quality → (Same as Operator with quality-specific queue)
    └─→ Plant Manager → PlantManager/dashboard.html
```

**Session Storage**:
```javascript
sessionStorage.setItem('currentUser', JSON.stringify({
  badgeId, role, loginTime
}));
```

---

## Responsive Behavior

**Target Resolution**: 1920x1080 (primary)
**Minimum**: 1366x768 (degraded but functional)

**Breakpoints** (if needed for smaller screens):
```css
@media (max-width: 1366px) {
  .nav-pane { width: 280px; }
  .search-box { width: 200px; }
}
```

---

## Accessibility

**ARIA Labels**:
- All icon buttons have `aria-label`
- Tables have `role="table"` attributes
- Modals have `role="dialog"` and `aria-labelledby`

**Keyboard Navigation**:
- Tab order follows visual flow
- Enter key submits forms
- Escape key closes modals

**High Contrast Mode**:
- Borders remain visible
- Focus indicators prominent
- Color not sole indicator

---

## Usage Instructions

### Viewing Mockups

1. **Start at Login**:
   ```
   file:///path/to/Mockups/Login/login.html
   ```

2. **Login Credentials** (mock):
   - Badge ID: `MTM001` (any value works)
   - PIN: `1234` (any 4 digits work)

3. **Role Selection**:
   - Choose role from dropdown or use "Auto-Detect"

4. **Navigate**:
   - Use left sidebar navigation
   - Click module-specific views

### Testing Scenarios

**Operator Workflow**:
1. Login as Operator
2. View waitlist → Click "New Request"
3. Fill wizard → Submit
4. View request in table
5. Cancel request

**Material Handler Workflow**:
1. Login as Material Handler
2. View unified waitlist (all task types)
3. Filter by zone
4. Accept task → Move to "My Tasks"
5. Mark complete

**Lead Analytics Workflow**:
1. Login as Material Handler Lead or Operator Lead
2. View waitlist with analytics panel
3. Navigate to full analytics dashboard
4. Review charts (SVG placeholders)
5. Adjust controls (Zone Mode toggle)

**Plant Manager Workflow**:
1. Login as Plant Manager
2. View unified dashboard (both departments)
3. Navigate between MH and Press analytics tabs
4. Review user management
5. Generate reports (mock)

---

## Implementation Notes for XAML Conversion

### 1. Data Binding Patterns

**HTML (Mock)**:
```html
<span id="userName"></span>
<script>
  document.getElementById('userName').textContent = currentUser.name;
</script>
```

**XAML (WinUI3)**:
```xml
<TextBlock Text="{x:Bind ViewModel.CurrentUser.Name, Mode=OneWay}" />
```

### 2. Navigation

**HTML**:
```html
<a href="waitlist.html" class="nav-item-link">Waitlist</a>
```

**XAML**:
```xml
<NavigationViewItem Content="Waitlist" Tag="WaitlistPage">
  <NavigationViewItem.Icon>
    <SymbolIcon Symbol="Home" />
  </NavigationViewItem.Icon>
</NavigationViewItem>
```

### 3. Commands

**HTML**:
```html
<button onclick="submitRequest()">Submit</button>
```

**XAML**:
```xml
<Button Content="Submit" Command="{x:Bind ViewModel.SubmitRequestCommand}" />
```

### 4. Tables/Lists

**HTML**:
```html
<table class="winui-table">
  <tr><td>Item 1</td></tr>
</table>
```

**XAML**:
```xml
<ListView ItemsSource="{x:Bind ViewModel.WaitlistItems, Mode=OneWay}">
  <ListView.ItemTemplate>
    <DataTemplate x:DataType="models:WaitlistItem">
      <TextBlock Text="{x:Bind Description}" />
    </DataTemplate>
  </ListView.ItemTemplate>
</ListView>
```

### 5. Modals

**HTML**:
```html
<div class="dialog-overlay">
  <div class="dialog">...</div>
</div>
```

**XAML**:
```xml
<ContentDialog Title="New Request">
  <StackPanel>...</StackPanel>
</ContentDialog>
```

---

## Future Enhancements (Post-MVP)

### Phase 2 Features

1. **Operator Module**:
   - Guided wizard with step validation
   - Favorites/Recents quick add
   - Work order lookup from Visual ERP

2. **Material Handler Module**:
   - Auto-assign logic when tasks turn red
   - Real-time floor plan with handler GPS
   - Project tracking (log in/out of special projects)

3. **Quality Module** (Standalone):
   - Quality-only queue separated
   - Automated email/Teams alerts
   - Intercom integration (security approval required)

4. **Training Module**:
   - In-app training workflows
   - Step-by-step operator onboarding
   - Video tutorials embedded

5. **Advanced Analytics**:
   - Custom date range reports
   - Export to CSV/PDF
   - Scheduled report delivery
   - Predictive analytics (bottleneck detection)

### Technical Enhancements

1. **Real-Time Updates**: SignalR for live waitlist changes
2. **Offline Support**: Service Worker for offline queueing
3. **Print Integration**: Label printing via LabelView 2022 API
4. **Barcode Scanning**: USB scanner direct integration
5. **Mobile Companion**: Read-only mobile view (future)

---

## Validation Checklist

Before converting to XAML, verify these mockup features:

- [ ] All modules have left NavigationView sidebar
- [ ] All tables use GridView-style column headers
- [ ] All action buttons use Fluent UI icon classes
- [ ] All modals use ContentDialog styling
- [ ] FAB appears only on waitlist views with items
- [ ] All forms validate before submission
- [ ] Search boxes use AutoSuggestBox styling
- [ ] Status bars show real-time updates
- [ ] Charts use SVG with proper legends
- [ ] Toggle switches have ON/OFF states
- [ ] Priority badges color-coded correctly
- [ ] Task type badges match department colors
- [ ] Wait time indicators change based on threshold
- [ ] Empty states show when no data
- [ ] Loading states indicated (future)
- [ ] Error handling with InfoBar
- [ ] Accessibility ARIA labels present
- [ ] Keyboard navigation functional
- [ ] High contrast mode compatible
- [ ] Responsive layout (1920x1080 primary)
- [ ] All WinUI3 control comments in HTML

---

## Contact & Support

**Project Owner**: John Kollodge  
**Business Stakeholders**:
- Nick Wunsch (Production Lead) - NWunsch@mantoolmfg.com
- Cristofer Muchowski (Production Lead) - CMuchowski@mantoolmfg.com
- Brett Lusk (Production Lead) - blusk@mantoolmfg.com
- Dan Smith (IT/Operations) - DSmith@mantoolmfg.com

**Version**: 1.0.0  
**Last Updated**: 2026-01-11  
**Status**: Mockup Phase - Stakeholder Review

---

## References

- **Fluent UI Icons**: https://aka.ms/fluentui-system-icons
- **WinUI3 Controls Gallery**: https://docs.microsoft.com/en-us/windows/apps/design/controls/
- **Fluent Design System**: https://www.microsoft.com/design/fluent/
- **Planning Documentation**: `docs/MTM_Waitlist_Application/Documentation/Planning/`
- **Mockup Prompt**: `MockupPrompt.md`

---

**Note**: These mockups are for visualization and stakeholder feedback only. They are not production code and should not be deployed to users. All business logic, security, and database operations must be implemented in the actual WinUI3 application per the architecture guidelines in the planning documentation.
