# MTM Waitlist - Internal Kanban System Documentation

## Overview

This directory contains the complete specification and mockups for building an **internal kanban board system** within the MTM Waitlist Application. Unlike the previous LeanKit integration approach, this is a fully self-contained solution that replicates kanban workflow functionality without external dependencies.

## What's Been Created

### ✅ Specification Document
**File:** `../Specifications/Internal_Kanban_System_Specification.md`

**Contents:**
- **Executive Summary** - Business value and objectives
- **4-Phase Implementation** (10 weeks)
  - Phase 1: Core board with drag-drop (Weeks 1-3)
  - Phase 2: Card details and comments (Weeks 4-5)
  - Phase 3: Custom boards and swimlanes (Weeks 6-8)
  - Phase 4: Automation and analytics (Weeks 9-10)
- **6 Core Modules** with complete APIs
- **Complete Database Schema** with MySQL DDL
- **UI Specifications** with ASCII art layouts
- **Workflow Rules** and business logic
- **Technical Implementation** guides
- **Testing Strategy**

### ✅ Mockup Index
**File:** `index.html`

Landing page with navigation to all mockup screens

### 🔨 Mockups To Be Created

The following interactive HTML mockups need to be built based on the specification:

1. **kanban_board_view.html** - Main kanban board
   - Horizontal lanes (Backlog, In Progress, Review, Completed)
   - Draggable cards
   - WIP limit indicators
   - Swimlane toggle
   - Search and filters

2. **kanban_card_detail.html** - Card detail panel
   - Full card information
   - Comments section
   - Activity history
   - Attachments
   - Edit capabilities

3. **kanban_board_config.html** - Board configuration
   - Lane management (add/remove/reorder)
   - WIP limit settings
   - Color customization
   - Swimlane options

4. **kanban_list_view.html** - Alternative list view
   - Table format
   - Sorting and filtering
   - Bulk actions
   - Quick edit

5. **kanban_analytics.html** - Analytics dashboard
   - Cycle time charts
   - Lead time metrics
   - Cumulative flow diagram
   - Bottleneck detection

6. **kanban_calendar_view.html** - Calendar view
   - Monthly/weekly calendar
   - Due date tracking
   - Drag to reschedule

## Key Differences from LeanKit Integration

| Aspect | LeanKit Integration | Internal Kanban |
|--------|-------------------|-----------------|
| **Architecture** | External API calls | Internal database |
| **Sync** | Bidirectional sync | No sync needed |
| **Dependencies** | LeanKit account required | Fully self-contained |
| **Customization** | Limited to LeanKit features | Fully customizable |
| **Cost** | LeanKit subscription | No external costs |
| **Offline** | Requires internet | Works offline |
| **Data Control** | Data in LeanKit cloud | All data internal |

## Core Features

### 1. Visual Kanban Board
```
┌─────────────┬─────────────┬─────────────┬─────────────┐
│   BACKLOG   │ IN PROGRESS │   REVIEW    │  COMPLETED  │
│  12 cards   │  8 cards ⚠ │   5 cards   │  23 cards   │
├─────────────┼─────────────┼─────────────┼─────────────┤
│ ┌─────────┐ │ ┌─────────┐ │ ┌─────────┐ │ ┌─────────┐ │
│ │Card #101│ │ │Card #105│ │ │Card #108│ │ │Card #95 │ │
│ │Acme Corp│ │ │GlobalTch│ │ │TechStart│ │ │InnovatCo│ │
│ │🔴 HIGH  │ │ │👤 John  │ │ │💬 1     │ │ │✓       │ │
│ └─────────┘ │ └─────────┘ │ └─────────┘ │ └─────────┘ │
│     ⋮       │     ⋮       │     ⋮       │     ⋮       │
└─────────────┴─────────────┴─────────────┴─────────────┘
```

### 2. Card Structure
```
┌─────────────────────────────┐
│ #101                    [⋮] │ ← ID + Menu
├─────────────────────────────┤
│ Acme Corporation            │ ← Customer
│ PO #12345                   │ ← PO Number
├─────────────────────────────┤
│ 🔴 HIGH                     │ ← Priority
│ 👤 John Smith              │ ← Assignment
│ 📅 Due: Jan 25             │ ← Due Date
├─────────────────────────────┤
│ 💬 3  📎 1  ✓ 2/5          │ ← Metadata
│ 🕐 2 days in lane          │ ← Age
└─────────────────────────────┘
```

### 3. Swimlanes (Grouped by Priority)
```
🔴 HIGH PRIORITY
├── BACKLOG ─ IN PROGRESS ─ REVIEW ─ COMPLETED
│   [Card]    [Card]

🟡 MEDIUM PRIORITY  
├── BACKLOG ─ IN PROGRESS ─ REVIEW ─ COMPLETED
│   [Card]    [Card]        [Card]

🟢 LOW PRIORITY
├── BACKLOG ─ IN PROGRESS ─ REVIEW ─ COMPLETED
│   [Card]
```

## Database Schema Highlights

### Core Tables

**kanban_boards** - Board definitions
```sql
CREATE TABLE kanban_boards (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255),
    is_default BOOLEAN,
    enable_swimlanes BOOLEAN,
    swimlane_group_by VARCHAR(50),
    enable_wip_limits BOOLEAN,
    ...
);
```

**kanban_lanes** - Board lanes/columns
```sql
CREATE TABLE kanban_lanes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    board_id INT,
    name VARCHAR(100),
    color_hex VARCHAR(7),
    position INT,
    wip_limit INT,
    lane_type ENUM('backlog', 'active', 'done'),
    ...
);
```

**waitlist_entries (extended)** - Cards
```sql
ALTER TABLE waitlist_entries ADD (
    kanban_board_id INT,
    kanban_lane_id INT,
    kanban_position INT,
    assigned_to VARCHAR(100),
    due_date DATE,
    days_in_current_lane INT,
    ...
);
```

**kanban_card_activity** - Activity log
```sql
CREATE TABLE kanban_card_activity (
    id INT AUTO_INCREMENT PRIMARY KEY,
    card_id INT,
    activity_type ENUM('Comment', 'Move', 'Edit', ...),
    user_name VARCHAR(100),
    description TEXT,
    timestamp DATETIME,
    ...
);
```

## Implementation Roadmap

### Week 1-3: Phase 1 - Core Board
- [ ] Create database tables
- [ ] Implement basic board view
- [ ] Add drag-and-drop functionality
- [ ] Create card component
- [ ] Display cards in lanes
- [ ] Update lane on card move

### Week 4-5: Phase 2 - Card Details
- [ ] Build card detail panel
- [ ] Implement comments system
- [ ] Add activity tracking
- [ ] Enable card editing
- [ ] Add search and filters

### Week 6-8: Phase 3 - Customization
- [ ] Lane management UI
- [ ] Swimlane support
- [ ] WIP limit enforcement
- [ ] Multiple board support
- [ ] Board templates

### Week 9-10: Phase 4 - Advanced
- [ ] Automation rules
- [ ] Analytics dashboard
- [ ] Performance metrics
- [ ] Export capabilities

## Quick Start for Developers

### 1. Review Specification
Read `Internal_Kanban_System_Specification.md` for complete technical details

### 2. Set Up Database
Run the SQL schema from the specification to create tables

### 3. Implement Core Services
Start with:
- `IService_Kanban_Board`
- `IService_Kanban_Lane`
- `IService_Kanban_Card`

### 4. Build UI Components
- Board view (main screen)
- Card component (reusable)
- Lane header (reusable)

### 5. Add Drag-Drop
Use WinUI 3 drag-drop APIs as shown in specification

## Testing Checklist

- [ ] Drag card between lanes
- [ ] Card persists to correct lane
- [ ] WIP limit warnings display
- [ ] Comments save and display
- [ ] Activity log captures changes
- [ ] Search finds cards
- [ ] Filters work correctly
- [ ] Multiple boards work
- [ ] Swimlanes group correctly
- [ ] Analytics calculate correctly

## Key Advantages

### vs External Kanban Tools
✅ No subscription costs  
✅ Complete data ownership  
✅ Deep integration with waitlist  
✅ Offline capability  
✅ Full customization  
✅ No API rate limits  

### vs Manual List View
✅ Visual workflow representation  
✅ Drag-and-drop simplicity  
✅ At-a-glance status  
✅ WIP limit enforcement  
✅ Team collaboration  
✅ Better metrics  

## Support & Resources

### Documentation
- Specification: `Internal_Kanban_System_Specification.md`
- Database Schema: See specification Section "Data Model"
- API Reference: See specification Section "Module Breakdown"

### Example Code
- Drag-Drop: See specification Section "Technical Implementation"
- Service Layer: See specification "Module Breakdown"
- ViewModel Pattern: Follow existing MTM architecture

### References
- WinUI 3 Drag-Drop: https://learn.microsoft.com/en-us/windows/apps/design/input/drag-and-drop
- Kanban Principles: https://kanbanize.com/kanban-resources/getting-started/what-is-kanban
- MySQL Indexes: Optimize for `kanban_board_id`, `kanban_lane_id`, `position`

## Next Steps

1. **Review the specification document** for complete technical details
2. **Create the HTML mockups** using the layouts provided in the spec
3. **Set up the database schema** from the SQL DDL
4. **Implement Phase 1** (core board with drag-drop)
5. **Test with real data** using existing waitlist entries

## Questions?

For questions about:
- **Architecture**: See specification "System Overview"
- **Database**: See specification "Data Model"
- **UI Design**: See specification "User Interface Specifications"
- **Implementation**: See specification "Technical Implementation"
- **Testing**: See specification "Testing Strategy"

---

**Version:** 1.0  
**Last Updated:** 2026-01-20  
**Status:** Ready for Implementation

---

The specification provides everything needed to build a complete internal kanban system. The HTML mockups will help visualize the final product before coding begins.
