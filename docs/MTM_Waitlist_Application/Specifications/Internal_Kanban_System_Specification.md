# Internal Kanban System Specification for MTM Waitlist Application

**Document Version:** 1.0  
**Last Updated:** 2026-01-20  
**Status:** Draft

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [System Overview](#system-overview)
3. [Phase-Based Implementation](#phase-based-implementation)
4. [Module Breakdown](#module-breakdown)
5. [User Interface Specifications](#user-interface-specifications)
6. [Data Model](#data-model)
7. [Workflow & Business Rules](#workflow--business-rules)
8. [Technical Implementation](#technical-implementation)
9. [Testing Strategy](#testing-strategy)
10. [Deployment Plan](#deployment-plan)

---

## Executive Summary

### Purpose

This specification outlines the design and implementation of a fully-integrated kanban board system within the MTM Waitlist Application. The system will provide visual workflow management, allowing users to manage waitlist entries through an intuitive drag-and-drop interface with customizable lanes, swimlanes, and card properties.

### Key Objectives

- **Visual Workflow Management**: Drag-and-drop kanban board interface
- **Flexible Board Configuration**: Customizable lanes, colors, and WIP limits
- **Multiple Views**: Board view, list view, calendar view
- **Real-time Updates**: Changes reflect immediately across all views
- **Advanced Features**: Swimlanes, card filtering, quick actions
- **Team Collaboration**: Comments, assignments, activity tracking

### Business Value

- **30% faster workflow management** through visual interface
- **Reduce status update time** by 80% with drag-and-drop
- **Improve visibility** with at-a-glance board overview
- **Better resource allocation** with WIP limits and assignments
- **Enhanced collaboration** with real-time updates and comments

---

## System Overview

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                  MTM Waitlist Application                       │
│                                                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Presentation Layer                       │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │ │
│  │  │ Kanban Board │  │  List View   │  │ Calendar View│    │ │
│  │  │     View     │  │              │  │              │    │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                           ▼                                       │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    ViewModel Layer                          │ │
│  │  • ViewModel_Kanban_Board                                  │ │
│  │  • ViewModel_Kanban_Card                                   │ │
│  │  • ViewModel_Kanban_BoardConfiguration                     │ │
│  └────────────────────────────────────────────────────────────┘ │
│                           ▼                                       │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Service Layer                            │ │
│  │  • IService_Kanban_Board                                   │ │
│  │  • IService_Kanban_Card                                    │ │
│  │  • IService_Kanban_Activity                                │ │
│  └────────────────────────────────────────────────────────────┘ │
│                           ▼                                       │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Data Access Layer                        │ │
│  │  • Dao_Kanban_Board                                        │ │
│  │  • Dao_Kanban_Lane                                         │ │
│  │  • Dao_Kanban_Card (extends waitlist_entries)             │ │
│  └────────────────────────────────────────────────────────────┘ │
│                           ▼                                       │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                 MySQL Database                              │ │
│  │  • kanban_boards                                           │ │
│  │  • kanban_lanes                                            │ │
│  │  • kanban_swimlanes                                        │ │
│  │  • waitlist_entries (cards)                                │ │
│  │  • kanban_card_activity                                    │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### Core Features

#### 1. Kanban Board View
- **Horizontal Lanes**: Backlog → In Progress → Review → Completed
- **Vertical Swimlanes**: Group by Priority, Customer, Assigned User
- **Card Display**: Visual cards showing key information
- **Drag & Drop**: Move cards between lanes with smooth animations
- **WIP Limits**: Visual warnings when lane limits exceeded
- **Quick Actions**: Archive, delete, edit directly from card

#### 2. Card Management
- **Rich Card Details**: All waitlist entry information
- **Comments & Activity**: Discussion threads and history
- **Attachments**: Files associated with entries
- **Labels/Tags**: Visual categorization
- **Due Dates**: Calendar integration
- **Checklists**: Task breakdown within cards

#### 3. Board Configuration
- **Custom Lanes**: Add, remove, reorder lanes
- **Lane Properties**: Name, color, WIP limit, position
- **Swimlane Options**: Configure grouping criteria
- **Card Templates**: Default card layouts
- **Automation Rules**: Auto-move cards based on conditions

#### 4. Multiple Views
- **Board View**: Visual kanban board (primary)
- **List View**: Traditional table/grid
- **Calendar View**: Due date focused
- **Timeline View**: Gantt-style timeline
- **Activity View**: Recent changes feed

---

## Phase-Based Implementation

### Phase 1: Core Board Foundation (Weeks 1-3)

**Goal:** Basic kanban board with fixed lanes and drag-drop

**Deliverables:**
- Database schema for boards, lanes, cards
- Board configuration table
- Basic board view UI (4 fixed lanes)
- Card component with basic info
- Drag-and-drop between lanes
- Lane headers with counts
- UI: Board switcher/selector

**Success Criteria:**
- Display waitlist entries as cards on board
- Drag cards between lanes
- Status updates persist to database
- Card counts update in real-time
- No data loss during drag operations

### Phase 2: Enhanced Card Features (Weeks 4-5)

**Goal:** Rich card details and interactions

**Deliverables:**
- Card detail modal/flyout
- Comments system
- Activity/history tracking
- Card labels/tags
- Quick edit capabilities
- Card filtering
- Search functionality
- UI: Card detail panel
- UI: Comment thread

**Success Criteria:**
- View full card details
- Add/edit/delete comments
- View complete activity history
- Filter cards by multiple criteria
- Search across all cards
- Quick edit without modal

### Phase 3: Advanced Board Features (Weeks 6-8)

**Goal:** Customizable boards and advanced features

**Deliverables:**
- Custom lane management (add/remove/reorder)
- Swimlane support (horizontal grouping)
- WIP limits with visual warnings
- Multiple board support
- Board templates
- Card assignments
- Due date tracking
- UI: Board configuration screen
- UI: Swimlane selector
- UI: WIP limit indicators

**Success Criteria:**
- Create custom boards with any lanes
- Group cards by swimlanes
- Enforce WIP limits (warnings)
- Switch between multiple boards
- Assign users to cards
- Track and warn on due dates

### Phase 4: Automation & Analytics (Weeks 9-10)

**Goal:** Workflow automation and insights

**Deliverables:**
- Automation rules engine
- Card aging indicators
- Cycle time metrics
- Cumulative flow diagram
- Lead time tracking
- Bottleneck detection
- Export capabilities
- UI: Analytics dashboard
- UI: Automation rules editor

**Success Criteria:**
- Auto-move cards based on rules
- Display card age visually
- Calculate cycle/lead times
- Show flow diagram
- Identify bottlenecks
- Export board data

---

## Module Breakdown

### Module 1: Board Management

**Purpose:** Create and manage kanban boards

#### Model_Kanban_Board
```csharp
public class Model_Kanban_Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<Model_Kanban_Lane> Lanes { get; set; } = new();
    public Model_Kanban_BoardSettings Settings { get; set; } = new();
}

public class Model_Kanban_BoardSettings
{
    public bool EnableSwimla­nes { get; set; }
    public string SwimlaneGroupBy { get; set; } = "None"; // Priority, Customer, AssignedUser
    public bool EnableWipLimits { get; set; }
    public bool ShowCardIds { get; set; } = true;
    public bool ShowCardAge { get; set; }
    public int CardAgeWarningDays { get; set; } = 7;
    public string DefaultCardColor { get; set; } = "#0078D4";
}
```

#### IService_Kanban_Board
```csharp
public interface IService_Kanban_Board
{
    Task<Model_Dao_Result<List<Model_Kanban_Board>>> GetAllBoardsAsync();
    Task<Model_Dao_Result<Model_Kanban_Board>> GetBoardByIdAsync(int boardId);
    Task<Model_Dao_Result<Model_Kanban_Board>> GetDefaultBoardAsync();
    Task<Model_Dao_Result<int>> CreateBoardAsync(Model_Kanban_Board board);
    Task<Model_Dao_Result> UpdateBoardAsync(Model_Kanban_Board board);
    Task<Model_Dao_Result> DeleteBoardAsync(int boardId);
    Task<Model_Dao_Result> SetDefaultBoardAsync(int boardId);
}
```

---

### Module 2: Lane Management

**Purpose:** Manage board lanes (columns)

#### Model_Kanban_Lane
```csharp
public class Model_Kanban_Lane
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#0078D4";
    public int Position { get; set; }
    public int? WipLimit { get; set; }
    public bool IsCollapsed { get; set; }
    public string LaneType { get; set; } = "active"; // backlog, active, done
    public List<Model_Kanban_Card> Cards { get; set; } = new();
}
```

#### IService_Kanban_Lane
```csharp
public interface IService_Kanban_Lane
{
    Task<Model_Dao_Result<List<Model_Kanban_Lane>>> GetLanesByBoardAsync(int boardId);
    Task<Model_Dao_Result<int>> CreateLaneAsync(Model_Kanban_Lane lane);
    Task<Model_Dao_Result> UpdateLaneAsync(Model_Kanban_Lane lane);
    Task<Model_Dao_Result> DeleteLaneAsync(int laneId);
    Task<Model_Dao_Result> ReorderLanesAsync(int boardId, List<int> laneIds);
    Task<Model_Dao_Result<int>> GetCardCountInLaneAsync(int laneId);
}
```

---

### Module 3: Card Management

**Purpose:** Manage kanban cards (waitlist entries)

#### Model_Kanban_Card
```csharp
public class Model_Kanban_Card
{
    public int Id { get; set; } // Same as WaitlistEntryId
    public int BoardId { get; set; }
    public int LaneId { get; set; }
    public int Position { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Pending";
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = new();
    public string ColorHex { get; set; } = "#FFFFFF";
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int DaysInCurrentLane { get; set; }
    
    // Extended from waitlist entry
    public string CustomerName { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
}
```

#### IService_Kanban_Card
```csharp
public interface IService_Kanban_Card
{
    Task<Model_Dao_Result<List<Model_Kanban_Card>>> GetCardsByLaneAsync(int laneId);
    Task<Model_Dao_Result<Model_Kanban_Card>> GetCardByIdAsync(int cardId);
    Task<Model_Dao_Result<int>> CreateCardAsync(Model_Kanban_Card card);
    Task<Model_Dao_Result> UpdateCardAsync(Model_Kanban_Card card);
    Task<Model_Dao_Result> MoveCardAsync(int cardId, int targetLaneId, int position);
    Task<Model_Dao_Result> DeleteCardAsync(int cardId);
    Task<Model_Dao_Result> ArchiveCardAsync(int cardId);
}
```

---

### Module 4: Activity & Comments

**Purpose:** Track changes and enable collaboration

#### Model_Kanban_Activity
```csharp
public class Model_Kanban_Activity
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // Comment, Move, Edit, Create, etc.
    public string UserName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Model_Kanban_Comment
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
```

---

### Module 5: Drag-and-Drop Engine

**Purpose:** Handle card movement with validation

#### IService_Kanban_DragDrop
```csharp
public interface IService_Kanban_DragDrop
{
    Task<Model_Dao_Result<Model_DragDropValidation>> ValidateMoveAsync(
        int cardId, int sourceLaneId, int targetLaneId);
    
    Task<Model_Dao_Result> ExecuteMoveAsync(
        int cardId, int targetLaneId, int position);
    
    bool CheckWipLimit(int laneId);
    Task LogCardMoveAsync(int cardId, int fromLaneId, int toLaneId, string userName);
}

public class Model_DragDropValidation
{
    public bool IsValid { get; set; }
    public bool WipLimitExceeded { get; set; }
    public string? WarningMessage { get; set; }
    public bool RequiresConfirmation { get; set; }
}
```

---

## User Interface Specifications

### UI Component 1: Kanban Board View (Main Screen)

**File:** `View_Kanban_Board.xaml`

**Layout Description:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│  ☰ MTM Waitlist - Kanban Board                    [Board: Main ▼] [⚙]   │
├──────────────────────────────────────────────────────────────────────────┤
│  [🔍 Search...] [Filter ▼] [Group: Priority ▼]  [@Assigned] [+ New Card]│
├──────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌──────────────┬──────────────┬──────────────┬──────────────┐          │
│  │   BACKLOG    │ IN PROGRESS  │    REVIEW    │  COMPLETED   │          │
│  │   12 cards   │  8 cards ⚠  │   5 cards    │   23 cards   │          │
│  │  Limit: ∞    │  Limit: 10   │  Limit: 10   │  Limit: ∞    │          │
│  ├──────────────┼──────────────┼──────────────┼──────────────┤          │
│  │ ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐ │          │
│  │ │ #101     │ │ │ #105     │ │ │ #108     │ │ │ #95      │ │          │
│  │ │ Acme Corp│ │ │ GlobalTch│ │ │ TechStart│ │ │ InnovatCo│ │          │
│  │ │ PO-12345 │ │ │ PO-12349 │ │ │ PO-12352 │ │ │ PO-12290 │ │          │
│  │ │          │ │ │ 🔴 HIGH  │ │ │          │ │ │          │ │          │
│  │ │ 💬 3 📎 1│ │ │ 👤 John  │ │ │ 💬 1     │ │ │ ✓        │ │          │
│  │ │ 2 days   │ │ │ 5 days ⚠│ │ │ 1 day    │ │ │          │ │          │
│  │ └──────────┘ │ └──────────┘ │ └──────────┘ │ └──────────┘ │          │
│  │              │              │              │              │          │
│  │ ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐ │          │
│  │ │ #102     │ │ │ #106     │ │ │ #109     │ │ │ #96      │ │          │
│  │ │ MegaSply │ │ │ CoreSys  │ │ │ FastProd │ │ │ QuickMfg │ │          │
│  │ └──────────┘ │ └──────────┘ │ └──────────┘ │ └──────────┘ │          │
│  │      ⋮       │      ⋮       │      ⋮       │      ⋮       │          │
│  └──────────────┴──────────────┴──────────────┴──────────────┘          │
│                                                                            │
│  [List View] [Calendar] [Timeline] [Analytics]                           │
└──────────────────────────────────────────────────────────────────────────┘

Legend:
💬 - Comments count
📎 - Attachments count
👤 - Assigned user
🔴 - Priority indicator
⚠ - Warning (WIP limit or age)
✓ - Completed
```

**Card Visual Design:**

```
┌─────────────────────────────┐
│ #101                    [⋮] │ ← Card ID + Menu
├─────────────────────────────┤
│ Acme Corporation            │ ← Customer Name (bold)
│ PO #12345                   │ ← PO Number
├─────────────────────────────┤
│ 🔴 HIGH                     │ ← Priority badge
│ 👤 John Smith              │ ← Assignment
│ 📅 Due: Jan 25             │ ← Due date (if set)
├─────────────────────────────┤
│ 💬 3  📎 1  ✓ 2/5          │ ← Counts footer
│ 🕐 2 days in lane          │ ← Time in current lane
└─────────────────────────────┘
```

**Swimlane Example (Grouped by Priority):**

```
┌──────────────────────────────────────────────────────────────┐
│ 🔴 HIGH PRIORITY                                   (5 cards) │
├──────────────┬──────────────┬──────────────┬────────────────┤
│   BACKLOG    │ IN PROGRESS  │    REVIEW    │   COMPLETED    │
│ ┌──────────┐ │ ┌──────────┐ │              │                │
│ │ #101     │ │ │ #105     │ │              │                │
│ └──────────┘ │ └──────────┘ │              │                │
└──────────────┴──────────────┴──────────────┴────────────────┘

┌──────────────────────────────────────────────────────────────┐
│ 🟡 MEDIUM PRIORITY                                 (8 cards) │
├──────────────┬──────────────┬──────────────┬────────────────┤
│   BACKLOG    │ IN PROGRESS  │    REVIEW    │   COMPLETED    │
│ ┌──────────┐ │ ┌──────────┐ │ ┌──────────┐ │                │
│ │ #102     │ │ │ #106     │ │ │ #108     │ │                │
│ └──────────┘ │ └──────────┘ │ └──────────┘ │                │
└──────────────┴──────────────┴──────────────┴────────────────┘
```

---

### UI Component 2: Card Detail Panel

**File:** `UserControl_Kanban_CardDetail.xaml`

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│ Card #101 - Acme Corporation                      [×] Close │
├────────────────────────────────────────────────────────────┤
│ ┌─ Details ──────────────────────────────────────────────┐ │
│ │ Customer:    [Acme Corporation                      ] │ │
│ │ PO Number:   [PO-12345                              ] │ │
│ │ Status:      [In Progress            ▼]              │ │
│ │ Priority:    [● High  ○ Medium  ○ Low]               │ │
│ │ Assigned To: [John Smith             ▼]              │ │
│ │ Due Date:    [📅 01/25/2026          ]              │ │
│ │                                                       │ │
│ │ Description:                                          │ │
│ │ ┌───────────────────────────────────────────────────┐ │ │
│ │ │ Customer requires expedited delivery...          │ │ │
│ │ │                                                   │ │ │
│ │ └───────────────────────────────────────────────────┘ │ │
│ │                                                       │ │
│ │ Labels: [High Priority] [Customer Request] [+ Add]  │ │
│ └───────────────────────────────────────────────────────┘ │
│                                                            │
│ ┌─ Activity & Comments ──────────────────────────────────┐ │
│ │ [All Activity ▼] [Comments Only] [History]            │ │
│ │                                                        │ │
│ │ 👤 John Smith • 2 hours ago                          │ │
│ │ 💬 Updated priority to High based on customer call   │ │
│ │                                                        │ │
│ │ 🔄 System • 5 hours ago                              │ │
│ │ Moved from "Backlog" to "In Progress"                │ │
│ │                                                        │ │
│ │ 👤 Jane Doe • 1 day ago                              │ │
│ │ 💬 Confirmed material availability with supplier     │ │
│ │    [Reply] [Edit] [Delete]                           │ │
│ │                                                        │ │
│ │ ┌───────────────────────────────────────────────────┐ │ │
│ │ │ Write a comment...                               │ │ │
│ │ └───────────────────────────────────────────────────┘ │ │
│ │ [Attach File] [Mention @user]              [Post]    │ │
│ └────────────────────────────────────────────────────────┘ │
│                                                            │
│ ┌─ Attachments (1) ──────────────────────────────────────┐ │
│ │ 📄 customer_specs.pdf          125 KB    [Download]   │ │
│ │    Uploaded by John Smith • 2 days ago                │ │
│ └────────────────────────────────────────────────────────┘ │
│                                                            │
│ [Archive Card]          [Save Changes] [Move to Lane ▼]   │
└────────────────────────────────────────────────────────────┘
```

---

### UI Component 3: Board Configuration

**File:** `View_Kanban_BoardConfiguration.xaml`

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│ Board Configuration - Main Board                   [×]     │
├────────────────────────────────────────────────────────────┤
│ ┌─ General Settings ────────────────────────────────────┐ │
│ │ Board Name:    [Main Waitlist Board              ]   │ │
│ │ Description:   [Primary board for all entries    ]   │ │
│ │ [✓] Set as default board                             │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                            │
│ ┌─ Lanes Configuration ─────────────────────────────────┐ │
│ │ ┌──────────────────────────────────────────────────┐ │ │
│ │ │ #  Lane Name      Color   WIP Limit    Actions  │ │ │
│ │ ├──────────────────────────────────────────────────┤ │ │
│ │ │ 1. Backlog        🔵       ∞         [↕][✎][🗑] │ │ │
│ │ │ 2. In Progress    🟡       10        [↕][✎][🗑] │ │ │
│ │ │ 3. Review         🟠       10        [↕][✎][🗑] │ │ │
│ │ │ 4. Completed      🟢       ∞         [↕][✎][🗑] │ │ │
│ │ └──────────────────────────────────────────────────┘ │ │
│ │ [+ Add Lane]                                         │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                            │
│ ┌─ Display Options ─────────────────────────────────────┐ │
│ │ [✓] Show card IDs                                    │ │
│ │ [✓] Show card age                                    │ │
│ │ [✓] Warn when card age exceeds [7] days             │ │
│ │ [✓] Enable WIP limit warnings                        │ │
│ │ [ ] Collapse empty lanes                             │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                            │
│ ┌─ Swimlanes ───────────────────────────────────────────┐ │
│ │ [✓] Enable swimlanes                                 │ │
│ │ Group by: [Priority            ▼]                    │ │
│ │           Options: Priority, Customer, Assigned User │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                            │
│              [Cancel]  [Save Configuration]                │
└────────────────────────────────────────────────────────────┘
```

---

## Data Model

### Database Schema

```sql
-- Kanban Boards Table
CREATE TABLE IF NOT EXISTS kanban_boards (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    is_archived BOOLEAN DEFAULT FALSE,
    enable_swimlanes BOOLEAN DEFAULT FALSE,
    swimlane_group_by VARCHAR(50) DEFAULT 'None',
    enable_wip_limits BOOLEAN DEFAULT TRUE,
    show_card_ids BOOLEAN DEFAULT TRUE,
    show_card_age BOOLEAN DEFAULT TRUE,
    card_age_warning_days INT DEFAULT 7,
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    INDEX idx_default (is_default),
    INDEX idx_archived (is_archived)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Kanban Lanes Table
CREATE TABLE IF NOT EXISTS kanban_lanes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    board_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    color_hex VARCHAR(7) DEFAULT '#0078D4',
    position INT NOT NULL,
    wip_limit INT NULL,
    is_collapsed BOOLEAN DEFAULT FALSE,
    lane_type ENUM('backlog', 'active', 'done') DEFAULT 'active',
    FOREIGN KEY (board_id) REFERENCES kanban_boards(id) ON DELETE CASCADE,
    INDEX idx_board_position (board_id, position)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Extend waitlist_entries for kanban cards
ALTER TABLE waitlist_entries 
ADD COLUMN kanban_board_id INT NULL,
ADD COLUMN kanban_lane_id INT NULL,
ADD COLUMN kanban_position INT DEFAULT 0,
ADD COLUMN kanban_swimlane VARCHAR(100) NULL,
ADD COLUMN assigned_to VARCHAR(100) NULL,
ADD COLUMN due_date DATE NULL,
ADD COLUMN color_hex VARCHAR(7) DEFAULT '#FFFFFF',
ADD COLUMN days_in_current_lane INT DEFAULT 0,
ADD COLUMN last_moved_date DATETIME NULL,
ADD INDEX idx_kanban_board_lane (kanban_board_id, kanban_lane_id),
ADD INDEX idx_kanban_position (kanban_lane_id, kanban_position),
ADD FOREIGN KEY (kanban_board_id) REFERENCES kanban_boards(id) ON DELETE SET NULL,
ADD FOREIGN KEY (kanban_lane_id) REFERENCES kanban_lanes(id) ON DELETE SET NULL;

-- Kanban Card Labels
CREATE TABLE IF NOT EXISTS kanban_card_labels (
    id INT AUTO_INCREMENT PRIMARY KEY,
    card_id INT NOT NULL,
    label_text VARCHAR(100) NOT NULL,
    color_hex VARCHAR(7) DEFAULT '#0078D4',
    FOREIGN KEY (card_id) REFERENCES waitlist_entries(id) ON DELETE CASCADE,
    INDEX idx_card (card_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Kanban Activity Log
CREATE TABLE IF NOT EXISTS kanban_card_activity (
    id INT AUTO_INCREMENT PRIMARY KEY,
    card_id INT NOT NULL,
    activity_type ENUM('Comment', 'Move', 'Edit', 'Create', 'Assign', 'Archive', 'Delete') NOT NULL,
    user_name VARCHAR(100) NOT NULL,
    description TEXT NOT NULL,
    old_value TEXT NULL,
    new_value TEXT NULL,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (card_id) REFERENCES waitlist_entries(id) ON DELETE CASCADE,
    INDEX idx_card_timestamp (card_id, timestamp DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Kanban Comments
CREATE TABLE IF NOT EXISTS kanban_card_comments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    card_id INT NOT NULL,
    user_name VARCHAR(100) NOT NULL,
    comment_text TEXT NOT NULL,
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_date DATETIME NULL,
    FOREIGN KEY (card_id) REFERENCES waitlist_entries(id) ON DELETE CASCADE,
    INDEX idx_card_created (card_id, created_date DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Initialize default board
INSERT INTO kanban_boards (name, description, is_default, created_by)
VALUES ('Main Waitlist Board', 'Primary board for all waitlist entries', TRUE, 'System');

-- Initialize default lanes
SET @board_id = LAST_INSERT_ID();
INSERT INTO kanban_lanes (board_id, name, color_hex, position, wip_limit, lane_type) VALUES
(@board_id, 'Backlog', '#6B7280', 0, NULL, 'backlog'),
(@board_id, 'In Progress', '#F59E0B', 1, 10, 'active'),
(@board_id, 'Review', '#3B82F6', 2, 10, 'active'),
(@board_id, 'Completed', '#10B981', 3, NULL, 'done');
```

---

## Workflow & Business Rules

### Card Movement Rules

1. **Standard Flow**: Backlog → In Progress → Review → Completed
2. **Allowed Movements**:
   - Forward: Any lane to any lane to the right
   - Backward: Any lane to any lane to the left
   - Skip lanes: Allowed (e.g., Backlog directly to Completed)
3. **WIP Limit Enforcement**:
   - **Soft Limit**: Show warning but allow move
   - **Hard Limit** (optional): Block move until space available

### Automatic Actions

1. **Card Created**: Automatically placed in "Backlog" lane
2. **Card Moved**: Log activity, update `last_moved_date`, reset `days_in_current_lane`
3. **Lane Changed**: Update `status` field to match lane name
4. **Daily Job**: Increment `days_in_current_lane` for all active cards
5. **Age Warning**: Visual indicator when card exceeds `card_age_warning_days`

### Swimlane Grouping Logic

```
IF swimlane_group_by = "Priority" THEN
  Create swimlanes: High, Medium, Low
  
ELSE IF swimlane_group_by = "Customer" THEN
  Create swimlane per unique customer
  
ELSE IF swimlane_group_by = "AssignedUser" THEN
  Create swimlanes: Unassigned, [User 1], [User 2], etc.
  
ELSE
  Show single horizontal board (no swimlanes)
```

---

## Technical Implementation

### Drag-and-Drop Implementation

**Using WinUI 3 Drag-Drop APIs:**

```csharp
// Enable drag on card
private void Card_DragStarting(UIElement sender, DragStartingEventArgs args)
{
    var card = (Model_Kanban_Card)((FrameworkElement)sender).DataContext;
    args.Data.SetData("CardId", card.Id);
    args.Data.SetData("SourceLaneId", card.LaneId);
    args.DragUI.SetContentFromDataPackage();
}

// Accept drop on lane
private void Lane_DragOver(object sender, DragEventArgs e)
{
    e.AcceptedOperation = DataPackageOperation.Move;
    e.DragUIOverride.Caption = "Move card here";
}

// Handle drop
private async void Lane_Drop(object sender, DragEventArgs e)
{
    var cardId = (int)await e.DataView.GetDataAsync("CardId");
    var sourceLaneId = (int)await e.DataView.GetDataAsync("SourceLaneId");
    var targetLane = (Model_Kanban_Lane)((FrameworkElement)sender).DataContext;
    
    // Validate move
    var validation = await _dragDropService.ValidateMoveAsync(
        cardId, sourceLaneId, targetLane.Id);
    
    if (validation.RequiresConfirmation)
    {
        // Show WIP limit warning
        var result = await ShowWipWarningDialog(validation.WarningMessage);
        if (!result) return;
    }
    
    // Execute move
    await _dragDropService.ExecuteMoveAsync(cardId, targetLane.Id, position: 0);
    
    // Refresh board
    await RefreshBoardAsync();
}
```

### Real-Time Updates

**Using SignalR (optional for multi-user):**

```csharp
public class KanbanHub : Hub
{
    public async Task CardMoved(int cardId, int toLaneId)
    {
        await Clients.Others.SendAsync("CardMoved", cardId, toLaneId);
    }
    
    public async Task CardUpdated(int cardId)
    {
        await Clients.Others.SendAsync("CardUpdated", cardId);
    }
}
```

### Performance Optimization

1. **Virtualization**: Use `ItemsRepeater` with virtualization for large card lists
2. **Lazy Loading**: Load card details on-demand
3. **Caching**: Cache board structure in memory
4. **Debouncing**: Debounce search/filter operations
5. **Batch Updates**: Group multiple card moves into single transaction

---

## Testing Strategy

### Unit Tests

```csharp
[Fact]
public async Task MoveCard_ShouldUpdateLaneAndPosition_WhenValidMove()
{
    // Arrange
    var card = new Model_Kanban_Card { Id = 1, LaneId = 1 };
    var targetLaneId = 2;
    
    // Act
    var result = await _dragDropService.ExecuteMoveAsync(card.Id, targetLaneId, 0);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    var updatedCard = await _cardService.GetCardByIdAsync(card.Id);
    updatedCard.Data.LaneId.Should().Be(targetLaneId);
}

[Fact]
public async Task MoveCard_ShouldWarn_WhenWipLimitExceeded()
{
    // Arrange
    var lane = new Model_Kanban_Lane { Id = 2, WipLimit = 5 };
    // Add 5 cards to lane
    
    // Act
    var validation = await _dragDropService.ValidateMoveAsync(6, 1, 2);
    
    // Assert
    validation.WipLimitExceeded.Should().BeTrue();
    validation.RequiresConfirmation.Should().BeTrue();
}
```

### Integration Tests

```csharp
[Fact]
public async Task DragDropCard_ShouldPersistToDatabase_WhenMoved()
{
    // Arrange
    var boardId = await CreateTestBoard();
    var cardId = await CreateTestCard(boardId, laneId: 1);
    
    // Act
    await _dragDropService.ExecuteMoveAsync(cardId, targetLaneId: 2, position: 0);
    
    // Assert
    var card = await _db.QuerySingleAsync<Model_Kanban_Card>(
        "SELECT * FROM waitlist_entries WHERE id = @id", new { id = cardId });
    card.kanban_lane_id.Should().Be(2);
}
```

---

## Deployment Plan

### Phase 1 Deployment

1. Run database migrations (create tables)
2. Initialize default board and lanes
3. Deploy application with board view
4. Train users on drag-drop
5. Monitor for 1 week

### Phase 2 Deployment

1. Deploy card detail enhancements
2. Enable comments and activity
3. Train users on collaboration features

### Phase 3 Deployment

1. Deploy custom board features
2. Train admins on board configuration
3. Allow users to create custom boards

### Phase 4 Deployment

1. Deploy automation and analytics
2. Configure automation rules
3. Review analytics with management

---

## Appendix: HTML Mockup Files

See interactive HTML mockups in:
- `docs/MTM_Waitlist_Application/Mockups/Kanban_System/`

Files include:
1. `kanban_board_view.html` - Main kanban board
2. `kanban_card_detail.html` - Card detail panel
3. `kanban_board_config.html` - Board configuration
4. `kanban_list_view.html` - Alternative list view
5. `kanban_analytics.html` - Analytics dashboard

---

**End of Specification**
