# GitHub Copilot Agent Prompt for MTM Waitlist Application WinUI3 Mockup Redesign

## **OBJECTIVE**

Create entirely new HTML mockups for the MTM Waitlist Application that visually simulate WinUI3 UI/UX patterns, replacing all existing mockups in the `docs/MTM_Waitlist_Application/Mockups` folder.

---

## **CONTEXT & REQUIREMENTS**

### **1. RESEARCH PHASE - Read These Files First:**

**Planning Documentation (9 files in `docs/MTM_Waitlist_Application/Documentation/Planning/`):**

- `file-structure-breakdown.md`
- `initial-epics. md`
- `kickoff-revised-core-first. md`
- `kickoff-stakeholder-version.md`
- `kickoff. md`
- `meeting-outline.md`
- `meeting-summary.md`
- `meeting-transcript.md`
- `module-breakdown.md`

**Existing Mockups (to understand current structure, then DELETE all files in these directories):**

- `docs/MTM_Waitlist_Application/Mockups/` (entire folder structure)
- Including:  `index.html`, all `Module_*` subdirectories, `Assets/`, and `README.md`

**Reference Architecture:**

- Study the MTM Receiving Application codebase (WinUI3, MVVM pattern, C# backend)
- Understand the existing architectural patterns, naming conventions, and UI component usage

---

### **2. DESIGN REFERENCE - TablesReady-Inspired Interface**

**Study the reference images (current company waitlist system):**

**Image 1 - Main Waitlist View:**

- Clean table-based layout with columns:  POS, LOAD, WAIT, NOTIFY, ACTIONS
- Left navigation sidebar:  Waitlist, Bookings, Floor Plan, Recent, Analytics, Loads, Settings, Support, Logout
- Action buttons per row: Call (phone), Message (chat), Alert (bell), Edit (pencil), Cancel (X), Complete (checkmark)
- Wait time prominently displayed
- Green floating action button (FAB) in bottom-right for adding parties
- Status indicators at bottom showing load counts by party size

**Image 2 - Add Party Modal:**

- Modal overlay with simple form
- Fields: Phone, Name, Notes (dropdown)
- Confirm/Cancel button pair
- Clean, minimal design

**Image 3 - Recent Loads History:**

- Table columns: TIME, LOAD, SIZE, NOTES, PAGING, UPDATE
- Status badges (e.g., "MAT" for material indicator)
- "Undo Served" / "Undo Canceled" action buttons
- Timestamp and date tracking
- Search functionality in top-right

---

### **3. WINUI3 ADAPTATION REQUIREMENTS**

**CRITICAL:  All HTML mockups must:**

✅ Use **only WinUI3-compatible controls and patterns**:  

- `NavigationView` (for sidebar navigation)
- `ListView` / `DataGrid` (for waitlist tables)
- `ContentDialog` (for modals like "Add Party")
- `AppBarButton` (for action buttons)
- `CommandBar` (for top-level actions)
- `InfoBar` (for notifications/alerts)
- `TeachingTip` (for tooltips/guidance)
- `Segmented Control` / `ToggleSwitch` (for mode toggles)
- `NavigationViewItem` (for sidebar menu items)

✅ **Visual Design Language:**

- Acrylic backgrounds and Fluent Design shadows
- WinUI3 color palette (NOT web colors)
- Corner radius consistent with Fluent Design (4px-8px)
- Proper spacing/padding using 4px grid system
- Microsoft Segoe UI Variable font family
- Light/Dark theme support considerations

✅ **Layout Patterns:**

- Use CSS Grid/Flexbox to mimic WinUI3 layout containers
- Responsive design (but prioritize desktop 1920x1080)
- Navigation pane collapsed/expanded states
- Proper z-index layering for modals/dialogs

---

### **4. MODULE STRUCTURE - SEPARATE VIEWS/PAGES**

Create **completely independent HTML mockups** for each module (duplicated code is acceptable):

#### **A. Login Module**

- Login screen with MTM, LLC branding
- Username/Password fields
- Role selection (or auto-detect based on credentials)
- "Remember Me" checkbox
- Clean, centered design

#### **B. Operator Module** (Press Floor Operators)

- **Waitlist View:** Press floor jobs/tasks awaiting operator action
- Table columns: Job #, Part #, Press #, Priority, Wait Time, Actions
- Action buttons: Start Job, Request Help, Complete
- No analytics (worker-level view only)
- Left nav: Waitlist, My Active Jobs, Recent Jobs, Settings, Logout
- **Floating Action Button:** Shown when waitlist has items

#### **C. Material Handler Module**

- **Unified Waitlist:** Aggregated tasks from ALL departments (Receiving, Dunnage, Routing, Press Floor)
- Table columns: Task Type, Location, Load #, Priority, Wait Time, Requester, Actions
- Color-coded task types (e.g., Receiving=Blue, Dunnage=Green, Routing=Orange, Press=Red)
- Action buttons: Accept Task, Mark Complete, Need Help
- Filter by:  Zone, Task Type, Priority
- Left nav: Waitlist, My Tasks, Recent, Floor Plan, Settings
- **Floating Action Button:** Shown when waitlist has items

#### **D.  Material Handler Lead Module**

- **All Material Handler functionality PLUS:**
- **Analytics Panel/Tab:**
  - Real-time dashboard:  Average wait time, tasks completed today, active handlers
  - **Charts:** SVG placeholders for task volume by type, handler performance metrics
- **Lead Controls:**
  - Toggle:  Zone Mode ON/OFF (SegmentedControl or ToggleSwitch)
  - Toggle: Auto-Assign ON/OFF
  - Assign tasks manually (drag-drop or button)
  - View all handler locations/statuses
- Left nav: Add "Analytics" and "Team Management" menu items
- **Floating Action Button:** Shown when waitlist has items

#### **E. Operator Lead Module** (Press Floor Lead)

- **All Operator functionality PLUS:**
- **Press Floor Analytics Panel/Tab:**
  - **Charts (SVG placeholders):** Press utilization trends, Job completion trends
  - **Tables/Charts:** Operator performance analytics
  - Downtime tracking
  - Part/job analytics
- **Lead Controls:**
  - Reassign jobs
  - Override priorities
  - View operator clock-in status
- Left nav: Add "Press Analytics" and "Operator Management"
- **Floating Action Button:** Shown when waitlist has items

#### **F. Plant Manager Module**

- **Unified Admin View:**
  - Access to BOTH Material Handler Lead and Operator Lead analytics
  - Master dashboard showing plant-wide metrics with **SVG placeholder charts**
  - Tabs/sections: Material Handling Analytics, Press Floor Analytics, Combined Reports
  - Override controls for all settings (Zone Mode, Auto-Assign, etc.)
  - User management (view roles, assign permissions)
- Left nav: Dashboard, MH Analytics, Press Analytics, Reports, Settings, User Management
- **Floating Action Button:** Contextual based on active tab/view

---

### **5. KEY FEATURES FROM PLANNING DOCS TO INCORPORATE**

Based on Planning files, ensure mockups include:

- **Zone Mode Toggle** (Material Handler Lead/Manager)
- **Auto-Assign Logic Indicator** (visual cue when enabled)
- **Priority System** (visual badges:  High/Medium/Low)
- **Wait Time Tracking** (real-time timer display)
- **Task Notifications** (toast-style or InfoBar)
- **Recent/History View** (similar to Image 3)
- **Floor Plan View** (visual map of facility - placeholder for now)
- **Search/Filter Bar** (top-right of each table)
- **Floating Action Button** (for quick task creation - only on waitlist views with items)
- **Status Indicators** (color-coded badges for task states)

---

### **6. FILE STRUCTURE FOR NEW MOCKUPS**

```
docs/MTM_Waitlist_Application/Mockups/
│
├── index.html (Navigation hub or Plant Manager view)
├── README.md (Updated documentation)
│
├── Login/
│   └── login.html
│
├── Operator/
│   ├── waitlist.html
│   ├── active-jobs.html
│   └── recent. html
│
├── MaterialHandler/
│   ├── waitlist.html
│   ├── my-tasks.html
│   ├── recent.html
│   └── floor-plan.html
│
├── MaterialHandlerLead/
│   ├── waitlist.html (includes analytics panel)
│   ├── analytics.html
│   ├── team-management.html
│   └── controls.html (Zone/Auto-Assign toggles)
│
├── OperatorLead/
│   ├── waitlist.html
│   ├── press-analytics.html
│   ├── operator-management.html
│   └── trends.html
│
├── PlantManager/
│   ├── dashboard.html
│   ├── mh-analytics.html
│   ├── press-analytics.html
│   ├── reports. html
│   └── user-management.html
│
├── Shared/
│   ├── styles.css (WinUI3-inspired styles)
│   ├── winui3-theme.css (Fluent Design variables)
│   └── mock-data.js (Sample data for tables)
│
└── Assets/
    ├── icons/ (Fluent UI icons - use Fluent UI Icons set)
    └── images/ (logos, placeholders)
```

---

### **7. TECHNICAL SPECIFICATIONS**

**HTML/CSS Requirements:**

- Use semantic HTML5
- CSS Grid for layout (mimic WinUI3's Grid)
- CSS Variables for theming (light/dark mode ready)
- **Fluent UI Icon Font** (Microsoft's official icon set)
- Responsive but optimized for 1920x1080 desktop
- Include `<!-- WinUI3 Equivalent:  [ControlName] -->` comments above each major UI component

**JavaScript (Optional):**

- Simple interactivity:  Modal open/close, toggle switches, tab switching
- Mock data population for tables
- Floating Action Button visibility logic (show only on waitlist views when items exist)
- DO NOT include complex business logic

**Accessibility:**

- ARIA labels on all interactive elements
- Keyboard navigation support (tabindex)
- High contrast mode considerations

**Charts & Visualizations:**

- **Use SVG placeholders** for all charts and graphs
- Include basic shapes (bars, lines, pie slices) with labels
- Add `<!-- Chart: [Description] -->` comments
- Style SVGs to match WinUI3 color palette

---

### **8. STYLING GUIDELINES - TablesReady + WinUI3 Fusion**

**From TablesReady (Reference Images):**

- Clean, minimal table design
- Action button row layout (icons with background)
- Left navigation sidebar style
- Card-based content areas
- Floating action buttons

**From WinUI3:**

- Acrylic material effects (subtle blur/transparency)
- Fluent Design elevation (shadows)
- CommandBar styling for top actions
- NavigationView pane styling
- Card/Panel border radius (8px)
- Button styles (Accent, Default, Subtle)

**Color Palette (WinUI3-inspired):**

```css
: root {
  --accent-color: #0078D4; /* Blue */
  --success-color: #107C10; /* Green */
  --warning-color: #FFC83D; /* Amber */
  --error-color:  #E81123; /* Red */
  --neutral-bg: #F3F3F3; /* Light mode */
  --neutral-bg-dark: #1F1F1F; /* Dark mode */
  --card-bg: #FFFFFF;
  --border-color: #EDEBE9;
}
```

---

### **9. DELIVERABLES CHECKLIST**

Before completing, ensure:

- [ ] All old mockup files deleted (Module_Core, Module_AnalyticsAdmin, etc.)
- [ ] 6 module sets created (Login, Operator, MaterialHandler, MH Lead, Op Lead, Manager)
- [ ] Each mockup includes NavigationView sidebar
- [ ] Tables use DataGrid/ListView styling
- [ ] Modals use ContentDialog styling
- [ ] Action buttons use AppBarButton patterns
- [ ] Analytics views include **SVG placeholder charts**
- [ ] **Fluent UI Icons** used throughout
- [ ] **Floating Action Button** appears only on waitlist views with items
- [ ] README. md updated with navigation instructions
- [ ] Styles are consistent across all modules
- [ ] WinUI3 control comments added to HTML
- [ ] All features from Planning docs represented visually

---

### **10. EXAMPLE CODE SNIPPET - Waitlist Table**

```html
<!-- WinUI3 Equivalent: ListView with GridView ItemTemplate -->
<div class="winui-listview">
  <table class="waitlist-table">
    <thead>
      <tr>
        <th>POS</th>
        <th>LOAD/JOB</th>
        <th>DETAILS</th>
        <th>WAIT</th>
        <th>ACTIONS</th>
      </tr>
    </thead>
    <tbody>
      <tr class="waitlist-item" data-priority="high">
        <td>1</td>
        <td>
          <div class="load-info">
            <span class="load-number">spotweldP</span>
            <span class="badge badge-material">MAT</span>
          </div>
        </td>
        <td><small>A110147 op 19 bule stiker x2</small></td>
        <td><span class="wait-time">41 mins</span></td>
        <td>
          <!-- WinUI3 Equivalent: AppBarButton -->
          <div class="action-buttons">
            <button class="btn-icon btn-call" aria-label="Call">
              <i class="fluent-icon">&#xE717;</i> <!-- Phone icon -->
            </button>
            <button class="btn-icon btn-message" aria-label="Message">
              <i class="fluent-icon">&#xE8BD;</i> <!-- Message icon -->
            </button>
            <button class="btn-icon btn-edit" aria-label="Edit">
              <i class="fluent-icon">&#xE70F;</i> <!-- Edit icon -->
            </button>
            <button class="btn-icon btn-cancel" aria-label="Cancel">
              <i class="fluent-icon">&#xE711;</i> <!-- Cancel icon -->
            </button>
            <button class="btn-icon btn-complete" aria-label="Complete">
              <i class="fluent-icon">&#xE73E;</i> <!-- Checkmark icon -->
            </button>
          </div>
        </td>
      </tr>
    </tbody>
  </table>
</div>

<!-- Floating Action Button - Only shown when waitlist has items -->
<button class="fab" aria-label="Add new item" id="fabAddItem">
  <i class="fluent-icon">&#xE710;</i> <!-- Add icon -->
</button>

<script>
// Show FAB only when table has items
const fabButton = document.getElementById('fabAddItem');
const tableRows = document.querySelectorAll('. waitlist-table tbody tr');
fabButton.style.display = tableRows.length > 0 ? 'flex' : 'none';
</script>
```

### **11. EXAMPLE SVG PLACEHOLDER CHART**

```html
<!-- Chart: Task Volume by Type (Bar Chart) -->
<div class="chart-container">
  <h3>Task Volume by Type</h3>
  <svg viewBox="0 0 400 200" class="chart-svg">
    <!-- X-axis -->
    <line x1="40" y1="180" x2="380" y2="180" stroke="#EDEBE9" stroke-width="2"/>
    <!-- Y-axis -->
    <line x1="40" y1="20" x2="40" y2="180" stroke="#EDEBE9" stroke-width="2"/>
    
    <!-- Bars -->
    <rect x="60" y="80" width="60" height="100" fill="#0078D4" opacity="0.8"/>
    <text x="90" y="195" text-anchor="middle" font-size="12">Receiving</text>
    
    <rect x="140" y="100" width="60" height="80" fill="#107C10" opacity="0.8"/>
    <text x="170" y="195" text-anchor="middle" font-size="12">Dunnage</text>
    
    <rect x="220" y="60" width="60" height="120" fill="#FF8C00" opacity="0.8"/>
    <text x="250" y="195" text-anchor="middle" font-size="12">Routing</text>
    
    <rect x="300" y="120" width="60" height="60" fill="#E81123" opacity="0.8"/>
    <text x="330" y="195" text-anchor="middle" font-size="12">Press</text>
    
    <!-- Y-axis labels -->
    <text x="30" y="25" text-anchor="end" font-size="10">100</text>
    <text x="30" y="105" text-anchor="end" font-size="10">50</text>
    <text x="30" y="185" text-anchor="end" font-size="10">0</text>
  </svg>
</div>
```

---

### **FINAL INSTRUCTIONS**

1. **Delete** all existing files in `docs/MTM_Waitlist_Application/Mockups/`
2. **Read** all 9 Planning MD files to understand business requirements
3. **Study** the MTM Receiving Application architecture for consistency
4. **Create** new HTML mockups following the file structure above
5. **Style** using WinUI3 visual language while maintaining TablesReady's clean UX
6. **Use** Fluent UI Icons throughout
7. **Implement** SVG placeholders for all charts and graphs
8. **Add** Floating Action Buttons only on waitlist views when items are present
9. **Ensure** each module is self-contained (duplicate code is acceptable)
10. **Document** WinUI3 control equivalents in HTML comments
11. **Test** that all mockups are viewable in a modern browser

---

**REFERENCE RESOURCES:**

- Fluent UI Icons:  <https://aka.ms/fluentui-system-icons>
- WinUI3 Controls Gallery: <https://docs.microsoft.com/en-us/windows/apps/design/controls/>
- Fluent Design System: <https://www.microsoft.com/design/fluent/>

---

**PROJECT CONTEXT:**
This mockup redesign is for the MTM Waitlist Application, a companion system to the MTM Receiving Application. The goal is to create a unified task management and workflow system for manufacturing operations at MTM, LLC, covering material handling, press floor operations, and plant management workflows.
