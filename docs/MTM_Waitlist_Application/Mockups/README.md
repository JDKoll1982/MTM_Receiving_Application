# MTM Waitlist Mockups (HTML)

These are **interactive HTML mockups** for design validation of the MTM Waitlist WinUI3 desktop application.

## Important: Not a Web App

These HTML mockups are **design prototypes** to validate UX flows before building the actual WinUI3 application.  
The final product will be a **Windows desktop application** (WinUI3 on .NET 8), NOT a web application.

### Why HTML Mockups?

- Faster iteration on UX/UI design
- Stakeholder review without building full WinUI3 code
- Easy to demonstrate module separation and role-based workflows
- Can be tested on any device with a browser

## Run Locally

### Option A: No server (recommended)

Open any HTML file directly in your browser:

- **Start here:** `index.html` (overview of all modules)
- **Or login:** `Module_Core/Views/Auth/LoginPage.html` (simulates login flow)
- **Or shell:** `Module_Core/Views/Shell/ShellPage.html` (role-based navigation)

Most pages work from `file://` protocol.

### Option B: With a local server (for full navigation)

From this folder:

```powershell
cd "C:\Users\johnk\source\repos\MTM_Receiving_Application\docs\MTM_Waitlist_Application\Mockups"
python -m http.server 5173
```

Then open:
- `http://localhost:5173/index.html`

## Module Structure

The mockups mirror the actual WinUI3 application architecture per kickoff-revised-core-first.md:

```
Mockups/
├── Module_Core/                    # Foundation services
│   └── Views/
│       ├── Auth/
│       │   └── LoginPage.html      # Badge + PIN authentication
│       └── Shell/
│           └── ShellPage.html      # Role-based navigation shell
│
├── Module_Operator/                 # Operator-specific features
│   └── Views/
│       ├── RequestWizard/          # Guided request creation
│       │   └── RequestWizardPage.html
│       ├── Favorites/              # Favorite/recent requests
│       │   └── FavoritesPage.html
│       └── Waitlist/
│           └── OperatorWaitlistPage.html  # Operator view of queue
│
├── Module_MaterialHandling/         # Material handler workflows
│   └── Views/
│       └── Waitlist/
│           └── MaterialHandlerWaitlistPage.html  # Zone-based queue (view zone only)
│
├── Module_Quality/                  # Quality alerts (stakeholder requirement)
│   └── Views/
│       └── QualityQueue/
│           └── QualityQueuePage.html  # Quality alerts from operators
│
├── Module_SetupTech/                # Setup tech workflows (from transcript)
│   └── Views/
│       ├── JobSetup/
│       │   └── JobSetupPage.html   # Die changes, job setups
│       └── DieManagement/          # Die notes and tracking
│
├── Module_AnalyticsAdmin/           # Lead/admin features
│   └── Views/
│       ├── ZoneAssignment/         # Material Handler Lead only
│       │   └── ZoneAssignmentPage.html
│       ├── Analytics/              # Duration/usage dashboards
│       │   └── AnalyticsDashboardPage.html
│       ├── TimeStandards/          # Manage urgency thresholds
│       │   └── TimeStandardsPage.html
│       ├── UserManagement/         # User administration
│       │   └── UserManagementPage.html
│       └── Waitlist/
│           └── LeadWaitlistPage.html  # Lead view with analytics
│
└── Assets/                          # Shared resources
    ├── styles/
    │   └── app.css                 # Fluent-style CSS
    └── scripts/
        ├── app.js                  # UI logic
        ├── data.js                 # Mock data
        └── waitlist.bundle.js      # Bundled version
```

## File Mapping (HTML → WinUI3)

HTML mockup structure mirrors the target WinUI3 application:

| HTML Mockup | WinUI3 Target |
|-------------|---------------|
| `Module_Core/Views/Auth/LoginPage.html` | `Module_Core/Views/Auth/LoginPage.xaml` |
| `Module_Core/Views/Shell/ShellPage.html` | `Module_Core/Views/Shell/ShellPage.xaml` |
| `Module_MaterialHandling/Views/Waitlist/MaterialHandlerWaitlistPage.html` | `Module_MaterialHandling/Views/Waitlist/MaterialHandlerWaitlistPage.xaml` |
| `Module_Quality/Views/QualityQueue/QualityQueuePage.html` | `Module_Quality/Views/QualityQueue/QualityQueuePage.xaml` |
| `Module_SetupTech/Views/JobSetup/JobSetupPage.html` | `Module_SetupTech/Views/JobSetup/JobSetupPage.xaml` |
| `Module_AnalyticsAdmin/Views/ZoneAssignment/ZoneAssignmentPage.html` | `Module_AnalyticsAdmin/Views/ZoneAssignment/ZoneAssignmentPage.xaml` |
| `Module_AnalyticsAdmin/Views/Waitlist/LeadWaitlistPage.html` | `Module_AnalyticsAdmin/Views/Waitlist/LeadWaitlistPage.xaml` |

## Mock Users (for testing login)

| Badge ID | PIN | Role(s) | Username |
|----------|-----|---------|----------|
| BADGE001 | 1234 | Operator, Lead | jdoe |
| BADGE002 | 5678 | MaterialHandler | mhandler1 |
| BADGE003 | 9999 | Quality | qtechA |
| BADGE004 | 4321 | SetupTech | setup1 |
| BADGE999 | 0000 | All roles | admin |

## Key Features Demonstrated

### Module Independence
- Each module (Operator, Material Handling, Quality, Setup Tech, Admin) is self-contained
- Changes to one module don't affect others (per meeting transcript requirement)
- Role-based navigation shows/hides modules

### Stakeholder Requirements Met (kickoff-stakeholder-version.md)
- ✅ Badge + PIN authentication
- ✅ Site separation (Expo / VITS Drive)
- ✅ Offline queue with sync status
- ✅ Quality module (operators alert quality without leaving press)
- ✅ Setup Tech module (die changes, job setup tracking)
- ✅ Zone assignment for material handlers (A/B/C zones)
- ✅ Analytics rights for leads only (operators can't see duration/usage)
- ✅ Read-only Visual ERP integration (mocked)
- ✅ Time standards with red urgency (auto-red when deadline approaches)
- ✅ Email/Teams notifications (optional, approval required)
- ✅ In-house only (Windows desktop target, no web/phone apps)
- ✅ Version 1.0 approval workflow (Nick, Chris, Brett, Dan)

### From Meeting Transcript
- ✅ Setup Tech module ("set of texts will have their own module")
- ✅ Quality alerts without leaving press
- ✅ Zone-based material handler assignment
- ✅ Auto-assign concept when tasks go red
- ✅ Recents/favorites for operators
- ✅ Time standards not adjustable by operators

### Not Included (Future Enhancements - Phase 2)
- Full guided wizard (placeholder created)
- Complete favorites implementation (template feature exists)
- Auto-assign logic (UI created, logic not implemented)
- Training tutorials module (placeholder)

## Design Validation Focus

These mockups validate:

1. **Module boundaries** - Can each role operate independently?
2. **Navigation clarity** - Is role-based menu visibility intuitive?
3. **Offline honesty** - Are sync states clear and trustworthy?
4. **Action feedback** - Do users get immediate confidence?
5. **Table-first familiarity** - Does it feel like Tables Ready?

## Technologies Used (Mockup Only)

- **HTML5**: Page structure
- **CSS3**: Fluent-inspired design system
- **Vanilla JavaScript**: No frameworks (keeps mockups simple)
- **localStorage**: Simulates offline persistence

## Final Application Stack (WinUI3)

- **Platform**: WinUI3 on .NET 8 (Windows Desktop)
- **Architecture**: Strict MVVM with CommunityToolkit.Mvvm
- **Databases**: 
  - MySQL (application data - full CRUD via stored procedures)
  - SQL Server (Infor Visual ERP - READ ONLY)
- **Deployment**: MTM Application Loader (copy-folder installer)
- **Printing**: LabelView 2022 integration
- **Offline**: SQLite local queue with sync service

## Next Steps

1. **Stakeholder Review**: Validate UX flows with Nick, Chris, Brett, Dan
2. **Iterate mockups**: Address feedback before WinUI3 build
3. **Begin WinUI3 Module_Core**: Badge/PIN auth, shell, offline queue
4. **Module-by-module implementation**: Operator → Material Handling → Quality → Setup Tech → Admin

---

**Questions?** Review the planning documents:
- `docs/MTM_Waitlist_Application/Documentation/Planning/kickoff-stakeholder-version.md`
- `docs/MTM_Waitlist_Application/Documentation/Planning/kickoff-revised-core-first.md`
- `docs/MTM_Waitlist_Application/Documentation/Planning/meeting-transcript.md`
