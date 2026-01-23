# LeanKit Integration Specification for MTM Waitlist Application

**Document Version:** 1.0  
**Last Updated:** 2026-01-20  
**Status:** Draft

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [System Overview](#system-overview)
3. [Phase-Based Implementation](#phase-based-implementation)
4. [Module Breakdown](#module-breakdown)
5. [Data Flow & Workflow](#data-flow--workflow)
6. [User Interface Specifications](#user-interface-specifications)
7. [Integration Patterns](#integration-patterns)
8. [Security & Configuration](#security--configuration)
9. [Testing Strategy](#testing-strategy)
10. [Deployment Plan](#deployment-plan)

---

## Executive Summary

### Purpose

This specification outlines the integration of LeanKit (a visual project management platform) with the MTM Waitlist Application. The goal is to create a bidirectional synchronization system that allows manufacturing operations teams to manage waitlist entries as LeanKit cards on a kanban board, enabling real-time visibility and workflow automation.

### Key Objectives

- **Bidirectional Sync**: Waitlist entries ↔ LeanKit cards
- **Real-time Updates**: Status changes propagate immediately
- **Workflow Automation**: Lane movements trigger waitlist state transitions
- **Enhanced Visibility**: Manufacturing team sees work in visual kanban format
- **Audit Trail**: All synchronization actions are logged and traceable

### Business Value

- Reduces manual data entry by 80%
- Improves visibility into manufacturing operations
- Enables cross-team collaboration
- Provides real-time status updates to stakeholders
- Maintains single source of truth across systems

---

## System Overview

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                  MTM Waitlist Application                   │
│                                                               │
│  ┌───────────────┐      ┌──────────────┐                    │
│  │  Waitlist UI  │◄────►│  ViewModel   │                    │
│  │   (WinUI 3)   │      │              │                    │
│  └───────────────┘      └──────┬───────┘                    │
│                                 │                             │
│                                 ▼                             │
│  ┌─────────────────────────────────────────────────┐        │
│  │         LeanKit Integration Service             │        │
│  │  ┌─────────────┐  ┌──────────┐  ┌───────────┐ │        │
│  │  │ Sync Engine │  │ Mapping  │  │  Queue    │ │        │
│  │  │             │  │  Logic   │  │ Manager   │ │        │
│  │  └─────────────┘  └──────────┘  └───────────┘ │        │
│  └─────────────────────┬───────────────────────────┘        │
│                        │                                     │
│                        ▼                                     │
│  ┌─────────────────────────────────────────────────┐        │
│  │         LeanKit API Client (DAO Layer)          │        │
│  │  ┌──────────┐  ┌──────────┐  ┌───────────────┐│        │
│  │  │  Board   │  │  Card    │  │ Authentication││        │
│  │  │  Access  │  │  CRUD    │  │  Management   ││        │
│  │  └──────────┘  └──────────┘  └───────────────┘│        │
│  └─────────────────────┬───────────────────────────┘        │
└────────────────────────┼─────────────────────────────────────┘
                         │
                         │ HTTPS/REST
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│              LeanKit Cloud Platform (SaaS)                  │
│                                                               │
│  ┌──────────────────────────────────────────────┐           │
│  │         Board: MTM Waitlist (ID: 1490772554) │           │
│  │                                                │           │
│  │  ┌─────────┐  ┌─────────┐  ┌──────────────┐ │           │
│  │  │ Backlog │  │ In Prog │  │   Complete   │ │           │
│  │  │  Lane   │  │  Lane   │  │    Lane      │ │           │
│  │  │ ┌─────┐ │  │ ┌─────┐ │  │  ┌─────┐     │ │           │
│  │  │ │Card │ │  │ │Card │ │  │  │Card │     │ │           │
│  │  │ └─────┘ │  │ └─────┘ │  │  └─────┘     │ │           │
│  │  └─────────┘  └─────────┘  └──────────────┘ │           │
│  └──────────────────────────────────────────────┘           │
└─────────────────────────────────────────────────────────────┘
```

### Technology Stack

| Component | Technology |
|-----------|-----------|
| **Application Framework** | WinUI 3 (Windows App SDK 1.6+) |
| **Programming Language** | C# 12 (.NET 8) |
| **Architecture Pattern** | MVVM with CommunityToolkit.Mvvm |
| **Database (Write)** | MySQL 8.0 |
| **HTTP Client** | HttpClient with Polly for retry logic |
| **JSON Serialization** | System.Text.Json |
| **Background Processing** | System.Threading.Channels + Timer |
| **Logging** | Existing IService_LoggingUtility |
| **Configuration** | MySQL app_settings table (encrypted) |

### Integration Scope

**In Scope:**
- Bidirectional sync between Waitlist and LeanKit
- Card creation from waitlist entries
- Status synchronization (lane movements ↔ status changes)
- Comment/note synchronization
- Priority mapping
- User assignment mapping
- Retry logic and error handling
- Sync status UI indicators

**Out of Scope (Future Phases):**
- File attachment synchronization
- Board creation/deletion from Waitlist App
- Custom field synchronization beyond basic mapping
- Webhook-based real-time updates (Phase 4)
- Multi-board support (single board only for v1)

---

## Phase-Based Implementation

### Phase 1: Foundation & Read-Only Integration (Weeks 1-2)

**Goal:** Establish secure connection and read LeanKit board data

**Deliverables:**
- Authentication module (token-based)
- Configuration management (encrypted storage)
- LeanKit API client (DAO layer)
- Board structure retrieval
- Card listing and detail view
- UI: Connection status indicator
- UI: Manual sync trigger button
- Logging infrastructure

**Success Criteria:**
- Successfully authenticate with LeanKit API
- Retrieve board structure (lanes, card types)
- Display LeanKit cards in read-only view
- Log all API interactions
- Handle authentication failures gracefully

### Phase 2: One-Way Sync (Waitlist → LeanKit) (Weeks 3-4)

**Goal:** Create LeanKit cards from waitlist entries

**Deliverables:**
- Card creation service
- Data mapping layer (Waitlist → LeanKit)
- Card update service (status, title, description)
- Sync queue manager (handle failures)
- UI: "Sync to LeanKit" button per waitlist entry
- UI: Sync status badge (synced/pending/error)
- Retry logic with exponential backoff

**Success Criteria:**
- Create LeanKit card from waitlist entry
- Map all required fields correctly
- Handle API failures with retry
- Store LeanKit card ID in waitlist entry
- Display sync status in UI
- Support manual retry for failed syncs

### Phase 3: Bidirectional Sync (Weeks 5-7)

**Goal:** Full two-way synchronization with conflict resolution

**Deliverables:**
- Background sync service (polling-based)
- Change detection logic
- Conflict resolution strategy
- Lane movement → status update
- Status update → lane movement
- Comment synchronization
- Timestamp-based conflict resolution
- UI: Auto-sync toggle
- UI: Sync history log viewer
- UI: Conflict resolution dialog

**Success Criteria:**
- Changes in Waitlist reflect in LeanKit
- Changes in LeanKit reflect in Waitlist
- Conflicts are detected and resolved
- Sync runs automatically every 15 minutes
- User can view sync history
- No data loss during sync

### Phase 4: Advanced Features & Webhooks (Weeks 8-10)

**Goal:** Real-time updates and enhanced features

**Deliverables:**
- Webhook endpoint (if LeanKit supports)
- Real-time card updates
- Advanced field mapping (custom fields)
- Batch sync operations
- Performance optimizations
- Analytics dashboard (sync metrics)
- UI: Real-time notification toast
- UI: Sync performance dashboard

**Success Criteria:**
- Near-instantaneous sync (if webhooks available)
- Support for 100+ concurrent syncs
- Detailed analytics on sync operations
- User notifications for important updates
- <2 second sync latency for critical updates

---

## Module Breakdown

### Module 1: Authentication & Configuration

**Purpose:** Manage LeanKit API authentication and secure credential storage

**Components:**

#### Model_LeanKit_AuthConfig
```csharp
public class Model_LeanKit_AuthConfig
{
    public string Account { get; set; } = "mtmfg";
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string ApiBaseUrl { get; set; } = "https://mtmfg.leankit.com/io";
    public bool IsTokenAuth { get; set; } = true;
    public DateTime? TokenExpiresAt { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
```

#### IService_LeanKit_Authentication
```csharp
public interface IService_LeanKit_Authentication
{
    Task<Model_Dao_Result<string>> AuthenticateAsync();
    Task<Model_Dao_Result> ValidateTokenAsync(string token);
    Task<Model_Dao_Result<string>> RefreshTokenAsync();
    Task<Model_Dao_Result> TestConnectionAsync();
    Task<Model_Dao_Result<Model_LeanKit_AuthConfig>> GetAuthConfigAsync();
    Task<Model_Dao_Result> SaveAuthConfigAsync(Model_LeanKit_AuthConfig config);
}
```

**Database Schema (MySQL app_settings):**
```sql
-- Add to existing app_settings table
INSERT IGNORE INTO app_settings (setting_key, setting_value, description, is_encrypted)
VALUES
('LeanKit.Account', 'mtmfg', 'LeanKit account name', 0),
('LeanKit.ApiUrl', 'https://mtmfg.leankit.com/io', 'LeanKit API base URL', 0),
('LeanKit.BoardId', '1490772554', 'MTM Waitlist LeanKit board ID', 0),
('LeanKit.Token', '{ENCRYPTED_TOKEN}', 'LeanKit API authentication token', 1),
('LeanKit.SyncIntervalMinutes', '15', 'Auto-sync frequency', 0),
('LeanKit.EnableSync', 'true', 'Enable/disable auto-sync', 0),
('LeanKit.LastSyncTimestamp', NULL, 'Last successful sync timestamp', 0),
('LeanKit.MaxRetries', '3', 'Max retry attempts for failed operations', 0),
('LeanKit.RetryDelaySeconds', '5', 'Initial retry delay (exponential backoff)', 0);
```

---

### Module 2: LeanKit API Client (DAO Layer)

**Purpose:** Low-level API interaction with LeanKit REST API

**Components:**

#### Dao_LeanKit_Board
```csharp
public class Dao_LeanKit_Board
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    
    public async Task<Model_Dao_Result<Model_LeanKit_Board>> GetBoardAsync(string boardId);
    public async Task<Model_Dao_Result<List<Model_LeanKit_BoardSummary>>> ListBoardsAsync();
    public async Task<Model_Dao_Result<Model_LeanKit_BoardStructure>> GetBoardStructureAsync(string boardId);
}
```

#### Dao_LeanKit_Card
```csharp
public class Dao_LeanKit_Card
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    
    public async Task<Model_Dao_Result<Model_LeanKit_Card>> GetCardAsync(string cardId);
    public async Task<Model_Dao_Result<List<Model_LeanKit_Card>>> ListCardsAsync(string boardId);
    public async Task<Model_Dao_Result<string>> CreateCardAsync(Model_LeanKit_CardCreateRequest request);
    public async Task<Model_Dao_Result> UpdateCardAsync(string cardId, List<Model_LeanKit_PatchOperation> operations);
    public async Task<Model_Dao_Result> DeleteCardAsync(string cardId);
}
```

#### Dao_LeanKit_Comment
```csharp
public class Dao_LeanKit_Comment
{
    public async Task<Model_Dao_Result<List<Model_LeanKit_Comment>>> ListCommentsAsync(string cardId);
    public async Task<Model_Dao_Result<string>> CreateCommentAsync(string cardId, string text);
    public async Task<Model_Dao_Result> UpdateCommentAsync(string cardId, string commentId, string text);
    public async Task<Model_Dao_Result> DeleteCommentAsync(string cardId, string commentId);
}
```

**Models:**

```csharp
public class Model_LeanKit_Board
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Model_LeanKit_Lane> Lanes { get; set; } = new();
    public List<Model_LeanKit_CardType> CardTypes { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

public class Model_LeanKit_Lane
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Index { get; set; }
    public string Type { get; set; } = string.Empty; // "ready", "inProgress", "done"
}

public class Model_LeanKit_CardType
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#0078D4";
}

public class Model_LeanKit_Card
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TypeId { get; set; } = string.Empty;
    public string LaneId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> AssignedUserIds { get; set; } = new();
    public string ExternalCardId { get; set; } = string.Empty; // Maps to WaitlistEntryID
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

public class Model_LeanKit_CardCreateRequest
{
    public string BoardId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TypeId { get; set; } = string.Empty;
    public string LaneId { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string? ExternalCardId { get; set; }
}

public class Model_LeanKit_PatchOperation
{
    public string Op { get; set; } = string.Empty; // "add", "replace", "remove"
    public string Path { get; set; } = string.Empty;
    public object? Value { get; set; }
}
```

---

### Module 3: Data Mapping & Transformation

**Purpose:** Translate between Waitlist and LeanKit data structures

**Components:**

#### IService_LeanKit_Mapper
```csharp
public interface IService_LeanKit_Mapper
{
    Model_LeanKit_CardCreateRequest MapWaitlistEntryToCardCreate(
        Model_WaitlistEntry entry, 
        Model_LeanKit_BoardStructure boardStructure);
    
    List<Model_LeanKit_PatchOperation> MapWaitlistEntryToCardUpdate(
        Model_WaitlistEntry entry, 
        Model_LeanKit_Card existingCard);
    
    Model_WaitlistEntry MapCardToWaitlistEntry(
        Model_LeanKit_Card card, 
        Model_WaitlistEntry? existingEntry = null);
    
    string MapWaitlistStatusToLaneId(
        string waitlistStatus, 
        Model_LeanKit_BoardStructure boardStructure);
    
    string MapLaneIdToWaitlistStatus(
        string laneId, 
        Model_LeanKit_BoardStructure boardStructure);
}
```

**Field Mapping Table:**

| Waitlist Field | LeanKit Card Field | Transformation Logic |
|----------------|-------------------|----------------------|
| `EntryID` | `ExternalCardId` | Direct mapping (string) |
| `CustomerName` | `Title` | Direct mapping |
| `Description` | `Description` | Direct mapping with HTML sanitization |
| `Status` | `LaneId` | Lookup: "Pending" → Backlog Lane, "In Progress" → In Progress Lane, etc. |
| `Priority` | `Priority` | Direct mapping (1-3) |
| `AssignedTo` | `AssignedUserIds[]` | User email → LeanKit User ID lookup |
| `Tags` | `Tags[]` | Direct mapping (string array) |
| `CreatedDate` | `CreatedOn` | Read-only (LeanKit manages) |
| `ModifiedDate` | `UpdatedOn` | Read-only (LeanKit manages) |
| `Notes` | Comments | Create separate comment via Comment API |

**Status Mapping Configuration (stored in app_settings):**
```json
{
  "statusMappings": [
    {
      "waitlistStatus": "Pending",
      "leankitLaneName": "Backlog",
      "leankitLaneId": "lane-123" // Auto-discovered at runtime
    },
    {
      "waitlistStatus": "In Progress",
      "leankitLaneName": "In Progress",
      "leankitLaneId": "lane-456"
    },
    {
      "waitlistStatus": "On Hold",
      "leankitLaneName": "Blocked",
      "leankitLaneId": "lane-789"
    },
    {
      "waitlistStatus": "Completed",
      "leankitLaneName": "Done",
      "leankitLaneId": "lane-999"
    }
  ]
}
```

---

### Module 4: Synchronization Engine

**Purpose:** Orchestrate bidirectional sync operations

**Components:**

#### IService_LeanKit_SyncEngine
```csharp
public interface IService_LeanKit_SyncEngine
{
    Task<Model_Dao_Result<Model_SyncReport>> SyncAllAsync();
    Task<Model_Dao_Result> SyncWaitlistEntryToLeankitAsync(int entryId);
    Task<Model_Dao_Result> SyncLeankitCardToWaitlistAsync(string cardId);
    Task<Model_Dao_Result> StartAutoSyncAsync();
    Task<Model_Dao_Result> StopAutoSyncAsync();
    bool IsAutoSyncRunning { get; }
    Task<Model_Dao_Result<List<Model_SyncHistoryEntry>>> GetSyncHistoryAsync(int limit = 50);
}
```

#### Model_SyncReport
```csharp
public class Model_SyncReport
{
    public DateTime SyncStartedAt { get; set; }
    public DateTime SyncCompletedAt { get; set; }
    public int TotalEntriesProcessed { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public int ConflictsDetected { get; set; }
    public int ConflictsResolved { get; set; }
    public List<Model_SyncError> Errors { get; set; } = new();
    public TimeSpan Duration => SyncCompletedAt - SyncStartedAt;
}

public class Model_SyncError
{
    public int? WaitlistEntryId { get; set; }
    public string? LeankitCardId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty; // "NetworkError", "ValidationError", "Conflict", etc.
    public int RetryCount { get; set; }
}

public class Model_SyncHistoryEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Direction { get; set; } = string.Empty; // "WaitlistToLeankit", "LeankitToWaitlist", "Bidirectional"
    public int? WaitlistEntryId { get; set; }
    public string? LeankitCardId { get; set; }
    public string Action { get; set; } = string.Empty; // "Created", "Updated", "Deleted"
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string PerformedBy { get; set; } = "System";
}
```

**Sync Algorithm (Bidirectional):**

```
FUNCTION SyncAllAsync():
  1. Load sync configuration (enabled, interval, last sync time)
  2. IF auto-sync disabled THEN return
  3. Log sync start
  4. 
  5. // Phase 1: Waitlist → LeanKit
  6. Load all waitlist entries with SyncStatus != "Synced"
  7. FOR EACH entry:
      a. Check if LeanKit card exists (via ExternalCardId)
      b. IF exists THEN
           - Compare timestamps (entry.ModifiedDate vs card.UpdatedOn)
           - IF entry newer THEN update card
           - ELSE skip (LeanKit is source of truth)
      c. ELSE create new card
      d. Mark entry as synced
      e. Log sync action
  
  8. // Phase 2: LeanKit → Waitlist
  9. Load all cards from LeanKit board
  10. FOR EACH card:
      a. Lookup waitlist entry by card.ExternalCardId
      b. IF entry exists THEN
           - Compare timestamps
           - IF card newer THEN update entry
           - ELSE skip (Waitlist is source of truth)
      c. ELSE create new waitlist entry (if card has ExternalCardId)
      d. Log sync action
  
  11. // Phase 3: Conflict Resolution
  12. FOR EACH detected conflict:
      a. Apply resolution strategy (last-write-wins by default)
      b. Log conflict and resolution
      c. Notify user if manual intervention needed
  
  13. Update last sync timestamp
  14. Generate sync report
  15. Return report
END FUNCTION
```

**Database Schema (Sync Tracking):**
```sql
CREATE TABLE IF NOT EXISTS leankit_sync_history (
    id INT AUTO_INCREMENT PRIMARY KEY,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    direction ENUM('WaitlistToLeankit', 'LeankitToWaitlist', 'Bidirectional') NOT NULL,
    waitlist_entry_id INT NULL,
    leankit_card_id VARCHAR(255) NULL,
    action ENUM('Created', 'Updated', 'Deleted') NOT NULL,
    success BOOLEAN NOT NULL DEFAULT TRUE,
    error_message TEXT NULL,
    performed_by VARCHAR(100) DEFAULT 'System',
    sync_duration_ms INT NULL,
    INDEX idx_timestamp (timestamp),
    INDEX idx_waitlist_entry (waitlist_entry_id),
    INDEX idx_leankit_card (leankit_card_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Add columns to existing waitlist_entries table
ALTER TABLE waitlist_entries 
ADD COLUMN leankit_card_id VARCHAR(255) NULL,
ADD COLUMN leankit_sync_status ENUM('NotSynced', 'Synced', 'SyncError', 'SyncPending') DEFAULT 'NotSynced',
ADD COLUMN leankit_last_sync_at DATETIME NULL,
ADD COLUMN leankit_sync_error TEXT NULL,
ADD INDEX idx_leankit_card_id (leankit_card_id),
ADD INDEX idx_sync_status (leankit_sync_status);
```

---

### Module 5: Retry & Error Handling

**Purpose:** Handle network failures and API errors gracefully

**Components:**

#### IService_LeanKit_RetryPolicy
```csharp
public interface IService_LeanKit_RetryPolicy
{
    Task<Model_Dao_Result<T>> ExecuteWithRetryAsync<T>(
        Func<Task<Model_Dao_Result<T>>> operation,
        string operationName);
    
    Task<Model_Dao_Result> ExecuteWithRetryAsync(
        Func<Task<Model_Dao_Result>> operation,
        string operationName);
}
```

**Retry Strategy:**
- **Max Retries:** 3 attempts
- **Backoff:** Exponential (5s, 10s, 20s)
- **Retryable Errors:**
  - Network timeouts
  - HTTP 429 (Rate Limit)
  - HTTP 502/503/504 (Server errors)
- **Non-Retryable Errors:**
  - HTTP 401 (Unauthorized)
  - HTTP 400 (Bad Request)
  - HTTP 404 (Not Found)

**Implementation (Polly-based):**
```csharp
public class Service_LeanKit_RetryPolicy : IService_LeanKit_RetryPolicy
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    public Service_LeanKit_RetryPolicy()
    {
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => 
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode == HttpStatusCode.BadGateway ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == HttpStatusCode.GatewayTimeout)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempt
                });
    }
}
```

---

### Module 6: User Interface Components

**Purpose:** Provide intuitive UI for managing LeanKit integration

**Components:**

#### View_Settings_LeanKitConfiguration
- Connection settings form
- Test connection button
- Board selection dropdown
- Lane mapping configuration
- Auto-sync toggle
- Sync interval setting

#### View_Waitlist_SyncStatus
- Per-entry sync status badge
- "Sync Now" button
- View in LeanKit link
- Sync error details

#### View_LeanKit_SyncDashboard
- Sync history table
- Sync statistics (success rate, errors)
- Manual sync trigger
- Auto-sync status indicator
- Last sync timestamp

#### UserControl_SyncStatusBadge
- Visual indicator (icon + text)
- States: Synced, Pending, Error, Not Synced
- Tooltip with last sync time
- Click to view details

---

## Data Flow & Workflow

### Workflow 1: Create Waitlist Entry → Sync to LeanKit

```
User Action: Create new waitlist entry in MTM Waitlist App
    ↓
1. Entry saved to MySQL (waitlist_entries table)
   - leankit_sync_status = "SyncPending"
    ↓
2. [User clicks "Sync to LeanKit" OR Auto-sync timer triggers]
    ↓
3. Service_LeanKit_SyncEngine.SyncWaitlistEntryToLeankitAsync(entryId)
    ↓
4. Load entry from database
    ↓
5. Service_LeanKit_Mapper.MapWaitlistEntryToCardCreate(entry, boardStructure)
    ↓
6. Dao_LeanKit_Card.CreateCardAsync(cardCreateRequest)
    ↓
7. [API Call to LeanKit: POST /io/card]
    ↓
8. Receive response with new card ID
    ↓
9. Update waitlist entry:
   - leankit_card_id = {cardId}
   - leankit_sync_status = "Synced"
   - leankit_last_sync_at = NOW()
    ↓
10. Log sync history (leankit_sync_history table)
    ↓
11. Update UI: Show "Synced" badge with green checkmark
```

### Workflow 2: Update LeanKit Card → Sync to Waitlist

```
External Action: User moves card to different lane in LeanKit
    ↓
1. [Auto-sync timer triggers OR user clicks "Sync from LeanKit"]
    ↓
2. Service_LeanKit_SyncEngine.SyncAllAsync()
    ↓
3. Dao_LeanKit_Card.ListCardsAsync(boardId)
    ↓
4. [API Call to LeanKit: GET /io/card?boardId={boardId}]
    ↓
5. Receive list of cards
    ↓
6. FOR EACH card:
    ↓
7. Lookup waitlist entry by leankit_card_id
    ↓
8. IF entry found:
    ↓
9. Compare timestamps (entry.ModifiedDate vs card.UpdatedOn)
    ↓
10. IF card.UpdatedOn > entry.ModifiedDate:
    ↓
11. Service_LeanKit_Mapper.MapCardToWaitlistEntry(card, existingEntry)
    ↓
12. Determine new status from card.LaneId
    ↓
13. Update waitlist entry in database
    - Status = MapLaneIdToWaitlistStatus(card.LaneId)
    - leankit_last_sync_at = NOW()
    ↓
14. Log sync history
    ↓
15. Update UI: Refresh waitlist view
```

### Workflow 3: Conflict Detection & Resolution

```
Scenario: Waitlist entry and LeanKit card both modified since last sync

1. Detect conflict during SyncAllAsync():
   - entry.ModifiedDate > last_sync_time
   - card.UpdatedOn > last_sync_time
    ↓
2. Apply conflict resolution strategy:
   a. Last-Write-Wins (default):
      - Compare timestamps
      - Newer change overwrites older
   b. User-Prompt (manual):
      - Show conflict dialog
      - Let user choose which version to keep
    ↓
3. Log conflict and resolution action
    ↓
4. Apply winning change
    ↓
5. Update both systems to match
```

---

## User Interface Specifications

### UI Component 1: LeanKit Configuration Window

**File:** `View_Settings_LeanKitConfiguration.xaml`

**Purpose:** Configure LeanKit API connection and sync settings

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│  LeanKit Integration Settings                    [X]       │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  Connection Settings                                         │
│  ┌────────────────────────────────────────────────────────┐│
│  │ Account Name:  [mtmfg                            ]     ││
│  │ API Base URL:  [https://mtmfg.leankit.com/io    ]     ││
│  │ Board ID:      [1490772554                       ]     ││
│  │                                                         ││
│  │ Authentication Method:  (•) Token  ( ) Email/Password ││
│  │                                                         ││
│  │ API Token:     [********************************]      ││
│  │                [Generate New Token]                    ││
│  │                                                         ││
│  │ Connection Status: ● Connected                         ││
│  │                [Test Connection]                       ││
│  └────────────────────────────────────────────────────────┘│
│                                                              │
│  Synchronization Settings                                   │
│  ┌────────────────────────────────────────────────────────┐│
│  │ [✓] Enable Auto-Sync                                   ││
│  │ Sync Interval:     [15] minutes                        ││
│  │ Last Sync:         2026-01-20 10:45:23 AM              ││
│  │ Next Sync:         2026-01-20 11:00:23 AM              ││
│  │                                                         ││
│  │ Retry Settings:                                         ││
│  │   Max Retries:   [3]                                   ││
│  │   Retry Delay:   [5] seconds (exponential backoff)    ││
│  └────────────────────────────────────────────────────────┘│
│                                                              │
│  Lane Mapping                                                │
│  ┌────────────────────────────────────────────────────────┐│
│  │ Waitlist Status      →  LeanKit Lane                   ││
│  │ ──────────────────────────────────────────────────     ││
│  │ Pending              →  [Backlog          ▼]          ││
│  │ In Progress          →  [In Progress      ▼]          ││
│  │ On Hold              →  [Blocked          ▼]          ││
│  │ Completed            →  [Done             ▼]          ││
│  │                                                         ││
│  │                [Refresh Lanes from LeanKit]            ││
│  └────────────────────────────────────────────────────────┘│
│                                                              │
│                      [Save Settings]  [Cancel]              │
└────────────────────────────────────────────────────────────┘
```

**Interactions:**
- **Test Connection**: Validates API credentials and board access
- **Generate New Token**: Opens LeanKit in browser to create token
- **Refresh Lanes**: Fetches latest board structure from LeanKit
- **Save Settings**: Encrypts and saves configuration to database

---

### UI Component 2: Waitlist Entry with Sync Status

**File:** `View_Waitlist_MainList.xaml` (Enhanced)

**Purpose:** Display waitlist entries with LeanKit sync indicators

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│  MTM Waitlist                                               │
├────────────────────────────────────────────────────────────┤
│ [Search...]                              [+ New Entry]      │
│                                                              │
│ ┌──────────────────────────────────────────────────────────┐
│ │ ID  Customer       Status      Priority  LeanKit  Actions│
│ ├──────────────────────────────────────────────────────────┤
│ │ 101 Acme Corp     In Progress    High    ✓ Synced  [...]││
│ │     PO #12345     Updated 10m ago        10:45 AM        ││
│ │                                                           ││
│ │ 102 GlobalTech    Pending        Med     ⏱ Pending [...]││
│ │     PO #12346     Created 1h ago         -               ││
│ │                                                           ││
│ │ 103 MegaSupply    On Hold        Low     ✗ Error    [...]││
│ │     PO #12347     Updated 2h ago         Click to view   ││
│ │                                                           ││
│ │ 104 TechStart     Completed      High    ✓ Synced  [...]││
│ │     PO #12348     Completed today        11:15 AM        ││
│ └──────────────────────────────────────────────────────────┘
└────────────────────────────────────────────────────────────┘

Legend:
✓ Synced   = Successfully synced with LeanKit
⏱ Pending  = Waiting for next sync
✗ Error    = Sync failed (click for details)
- Not Synced = Never synced to LeanKit
```

**Context Menu on "..." button:**
```
┌────────────────────────┐
│ Edit Entry             │
│ View in LeanKit ↗      │
│ ─────────────────────  │
│ Sync to LeanKit Now    │
│ View Sync History      │
│ ─────────────────────  │
│ Delete Entry           │
└────────────────────────┘
```

---

### UI Component 3: Sync Dashboard

**File:** `View_LeanKit_SyncDashboard.xaml`

**Purpose:** Monitor and manage synchronization operations

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│  LeanKit Sync Dashboard                          [X]       │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─ Sync Status ─────────────────────────────────────────┐ │
│  │ Auto-Sync: ● ENABLED          [Disable Auto-Sync]     │ │
│  │ Last Sync: 5 minutes ago (2026-01-20 10:55:00 AM)     │ │
│  │ Next Sync: in 10 minutes      [Sync Now]              │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ Statistics (Last 24 Hours) ──────────────────────────┐ │
│  │ Total Syncs:        48                                 │ │
│  │ Successful:         46  (95.8%)  ████████████████████  │ │
│  │ Failed:             2   (4.2%)   █                     │ │
│  │ Conflicts Resolved: 1                                  │ │
│  │ Avg Duration:       2.3 seconds                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ Recent Sync History ──────────────────────────────────┐ │
│  │ Time       Entry  Direction     Action   Status  Details││
│  ├────────────────────────────────────────────────────────┤ │
│  │ 10:55 AM   #101   Waitlist→LK   Update   ✓       [View]││
│  │ 10:55 AM   #102   LK→Waitlist   Update   ✓       [View]││
│  │ 10:40 AM   #103   Waitlist→LK   Create   ✗       [View]││
│  │ 10:25 AM   #104   Bidirectional  Update   ⚠       [View]││
│  │ 10:10 AM   #105   Waitlist→LK   Update   ✓       [View]││
│  │ 09:55 AM   #106   LK→Waitlist   Update   ✓       [View]││
│  │                                                          │ │
│  │            [Load More] [Export to CSV] [Clear History] │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ Active Errors ────────────────────────────────────────┐ │
│  │ Entry #103: Network timeout during card creation       │ │
│  │   Last Attempt: 10:40 AM                                │ │
│  │   Retry Count: 2/3                                      │ │
│  │   [Retry Now] [View Details] [Mark Resolved]           │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│                                         [Close]              │
└────────────────────────────────────────────────────────────┘
```

---

### UI Component 4: Sync Error Detail Dialog

**File:** `Dialog_SyncErrorDetail.xaml`

**Purpose:** Display detailed information about sync failures

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│  Sync Error Details - Entry #103                  [X]      │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  Error Information                                           │
│  ┌────────────────────────────────────────────────────────┐│
│  │ Waitlist Entry: #103 - MegaSupply (PO #12347)          ││
│  │ LeanKit Card ID: Not Created                           ││
│  │ Error Type:      NetworkError                          ││
│  │ Occurred At:     2026-01-20 10:40:15 AM                ││
│  │ Retry Attempts:  2 of 3                                ││
│  │                                                         ││
│  │ Error Message:                                          ││
│  │ ┌─────────────────────────────────────────────────────┐││
│  │ │ The operation has timed out after 30 seconds.       │││
│  │ │ Failed to establish connection to LeanKit API.      │││
│  │ │ Endpoint: https://mtmfg.leankit.com/io/card         │││
│  │ │ HTTP Status: 504 Gateway Timeout                    │││
│  │ └─────────────────────────────────────────────────────┘││
│  │                                                         ││
│  │ Stack Trace: [Show/Hide]                               ││
│  └────────────────────────────────────────────────────────┘│
│                                                              │
│  Recommended Actions                                         │
│  ┌────────────────────────────────────────────────────────┐│
│  │ • Check your internet connection                        ││
│  │ • Verify LeanKit service status                        ││
│  │ • Try again in a few minutes                           ││
│  │ • Contact support if problem persists                  ││
│  └────────────────────────────────────────────────────────┘│
│                                                              │
│     [Retry Now] [Copy Error Details] [Dismiss] [Get Help]  │
└────────────────────────────────────────────────────────────┘
```

---

### UI Component 5: Conflict Resolution Dialog

**File:** `Dialog_SyncConflictResolution.xaml`

**Purpose:** Allow user to resolve synchronization conflicts

**Layout:**

```
┌────────────────────────────────────────────────────────────┐
│  Sync Conflict Detected                           [X]      │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  Entry #101 (Acme Corp - PO #12345) has been modified in   │
│  both the Waitlist and LeanKit since the last sync.        │
│                                                              │
│  ┌─ Waitlist Version ────────────────────────────────────┐ │
│  │ Status:       In Progress                              │ │
│  │ Priority:     High                                     │ │
│  │ Description:  Updated with customer feedback          │ │
│  │ Modified:     2026-01-20 10:50 AM (2 minutes ago)     │ │
│  │ Modified By:  John Smith                              │ │
│  │                                                         │ │
│  │             ( ) Keep this version                      │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ LeanKit Version ─────────────────────────────────────┐ │
│  │ Status:       On Hold (Blocked lane)                   │ │
│  │ Priority:     Medium                                   │ │
│  │ Description:  Waiting for material availability       │ │
│  │ Modified:     2026-01-20 10:48 AM (4 minutes ago)     │ │
│  │ Modified By:  Jane Doe (via LeanKit)                  │ │
│  │                                                         │ │
│  │             (•) Keep this version                      │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─ Resolution Options ──────────────────────────────────┐ │
│  │ [ ] Merge changes (keep newest data for each field)   │ │
│  │ [ ] Remember my choice for future conflicts            │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│                [Resolve Conflict]  [Cancel]  [View Diff]    │
└────────────────────────────────────────────────────────────┘
```

---

## Integration Patterns

### Pattern 1: Polling-Based Sync

**Used in:** Phase 2 & 3

**Mechanism:**
```csharp
public class Service_LeanKit_SyncEngine
{
    private System.Threading.Timer? _syncTimer;
    
    public async Task StartAutoSyncAsync()
    {
        var intervalMinutes = await _configService.GetSyncIntervalAsync();
        var intervalMs = intervalMinutes * 60 * 1000;
        
        _syncTimer = new Timer(
            async _ => await SyncAllAsync(),
            null,
            TimeSpan.Zero,      // Start immediately
            TimeSpan.FromMilliseconds(intervalMs)
        );
    }
}
```

**Pros:**
- Simple to implement
- No server-side dependencies
- Works with any LeanKit subscription

**Cons:**
- Delayed updates (up to sync interval)
- Higher API usage
- Not truly real-time

---

### Pattern 2: Webhook-Based Sync (Future - Phase 4)

**Mechanism:**
```
LeanKit Board Change → Webhook Trigger → MTM API Endpoint → Immediate Sync
```

**Requirements:**
- LeanKit Enterprise subscription (verify availability)
- Public-facing webhook endpoint
- HTTPS with valid certificate
- Webhook signature validation

**Implementation (if available):**
```csharp
[HttpPost]
[Route("api/webhook/leankit")]
public async Task<IActionResult> ReceiveLeankitWebhook(
    [FromBody] Model_LeanKit_WebhookPayload payload,
    [FromHeader("X-LeanKit-Signature")] string signature)
{
    // Validate signature
    if (!_webhookValidator.IsValid(payload, signature))
        return Unauthorized();
    
    // Process webhook
    await _syncEngine.ProcessWebhookAsync(payload);
    return Ok();
}
```

---

### Pattern 3: Optimistic Locking

**Purpose:** Prevent data loss during concurrent modifications

**Implementation:**
```csharp
public async Task<Model_Dao_Result> UpdateWaitlistEntryAsync(
    Model_WaitlistEntry entry, 
    DateTime expectedLastModified)
{
    var sql = @"
        UPDATE waitlist_entries 
        SET status = @Status, 
            modified_date = NOW(),
            leankit_sync_status = 'SyncPending'
        WHERE id = @Id 
        AND modified_date = @ExpectedLastModified";
    
    var rowsAffected = await _db.ExecuteAsync(sql, new
    {
        entry.Status,
        entry.Id,
        ExpectedLastModified = expectedLastModified
    });
    
    if (rowsAffected == 0)
    {
        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = "Entry was modified by another process",
            Severity = Enum_ErrorSeverity.Warning
        };
    }
    
    return new Model_Dao_Result { Success = true };
}
```

---

## Security & Configuration

### Credential Management

**Storage:**
- API tokens stored in MySQL `app_settings` table
- Encrypted using AES-256
- Encryption key stored in Windows Credential Manager (per-user)

**Encryption Service:**
```csharp
public interface IService_Encryption
{
    Task<string> EncryptAsync(string plainText);
    Task<string> DecryptAsync(string cipherText);
}

// Windows Credential Manager integration
public class Service_WindowsCredentialEncryption : IService_Encryption
{
    private const string CREDENTIAL_TARGET = "MTM_Waitlist_LeanKit_EncryptionKey";
    
    public async Task<string> EncryptAsync(string plainText)
    {
        var key = GetOrCreateEncryptionKey();
        using var aes = Aes.Create();
        aes.Key = key;
        // ... encryption logic
    }
    
    private byte[] GetOrCreateEncryptionKey()
    {
        using var cred = new Credential();
        cred.Target = CREDENTIAL_TARGET;
        
        if (!cred.Load())
        {
            // Generate new key
            var key = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            
            cred.Password = Convert.ToBase64String(key);
            cred.Type = CredentialType.Generic;
            cred.PersistanceType = PersistanceType.LocalComputer;
            cred.Save();
        }
        
        return Convert.FromBase64String(cred.Password);
    }
}
```

### API Rate Limiting

**LeanKit Rate Limits (typical):**
- 100 requests per minute per account
- 500 requests per hour per account

**Mitigation Strategy:**
```csharp
public class Service_LeanKit_RateLimiter
{
    private readonly SemaphoreSlim _rateLimitSemaphore = new(10); // Max 10 concurrent requests
    private readonly Queue<DateTime> _requestTimestamps = new();
    
    public async Task<bool> AcquirePermitAsync()
    {
        await _rateLimitSemaphore.WaitAsync();
        
        try
        {
            // Remove timestamps older than 1 minute
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            while (_requestTimestamps.Count > 0 && 
                   _requestTimestamps.Peek() < oneMinuteAgo)
            {
                _requestTimestamps.Dequeue();
            }
            
            // Check if under rate limit
            if (_requestTimestamps.Count >= 90) // Leave buffer
            {
                var oldestRequest = _requestTimestamps.Peek();
                var waitTime = oldestRequest.AddMinutes(1) - DateTime.UtcNow;
                if (waitTime > TimeSpan.Zero)
                {
                    await Task.Delay(waitTime);
                }
            }
            
            _requestTimestamps.Enqueue(DateTime.UtcNow);
            return true;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }
}
```

---

## Testing Strategy

### Unit Tests

**Test Coverage:**
- Data mapping logic (100% coverage)
- Authentication flow
- Retry logic
- Conflict resolution algorithms

**Example Test:**
```csharp
[Fact]
public async Task MapWaitlistEntryToCardCreate_ShouldMapAllFields_WhenEntryValid()
{
    // Arrange
    var entry = new Model_WaitlistEntry
    {
        EntryID = 101,
        CustomerName = "Acme Corp",
        Description = "Test order",
        Status = "In Progress",
        Priority = 2
    };
    
    var boardStructure = new Model_LeanKit_BoardStructure
    {
        Lanes = new List<Model_LeanKit_Lane>
        {
            new() { Id = "lane-123", Name = "In Progress" }
        },
        CardTypes = new List<Model_LeanKit_CardType>
        {
            new() { Id = "type-456", Name = "Order" }
        }
    };
    
    var mapper = new Service_LeanKit_Mapper();
    
    // Act
    var result = mapper.MapWaitlistEntryToCardCreate(entry, boardStructure);
    
    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be("Acme Corp");
    result.Description.Should().Be("Test order");
    result.LaneId.Should().Be("lane-123");
    result.ExternalCardId.Should().Be("101");
}
```

### Integration Tests

**Test Scenarios:**
- Full round-trip sync (create entry → sync to LeanKit → sync back)
- Conflict detection and resolution
- Network failure simulation
- API authentication failure

**Example Integration Test:**
```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task SyncWaitlistToLeankit_ShouldCreateCard_WhenEntryNew()
{
    // Arrange
    var entry = await CreateTestWaitlistEntry();
    var syncEngine = GetIntegrationTestSyncEngine();
    
    // Act
    var result = await syncEngine.SyncWaitlistEntryToLeankitAsync(entry.EntryID);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    
    // Verify in LeanKit
    var leankitDao = GetLeankitDao();
    var cards = await leankitDao.ListCardsAsync(_testBoardId);
    cards.Data.Should().Contain(c => c.ExternalCardId == entry.EntryID.ToString());
}
```

### Manual Test Plan

**Test Case 1: Initial Setup**
1. Open LeanKit Configuration window
2. Enter API token
3. Click "Test Connection"
4. Verify successful connection message
5. Select board from dropdown
6. Configure lane mappings
7. Save settings
8. Verify settings persisted

**Test Case 2: Manual Sync**
1. Create new waitlist entry
2. Click "Sync to LeanKit" button
3. Verify sync status changes to "Pending"
4. Wait for sync completion
5. Verify status changes to "Synced"
6. Open LeanKit board in browser
7. Verify card exists with correct data

**Test Case 3: Auto-Sync**
1. Enable auto-sync (15-minute interval)
2. Create 5 new waitlist entries
3. Wait for sync timer to trigger
4. Verify all entries synced
5. Modify entry in LeanKit (move to different lane)
6. Wait for next sync cycle
7. Verify waitlist entry status updated

**Test Case 4: Error Handling**
1. Disconnect network
2. Create waitlist entry
3. Attempt manual sync
4. Verify error message displayed
5. Reconnect network
6. Click "Retry"
7. Verify successful sync

---

## Deployment Plan

### Phase 1 Deployment (Weeks 1-2)

**Prerequisites:**
- LeanKit API token generated
- Board ID confirmed
- MySQL database updated with new schema

**Deployment Steps:**
1. Database migration (add leankit_* columns)
2. Deploy application update
3. Configure LeanKit settings (admin only)
4. Test connection
5. Enable read-only mode
6. Train users on new UI elements

### Phase 2 Deployment (Weeks 3-4)

**Prerequisites:**
- Phase 1 stable for 1 week
- No critical bugs reported

**Deployment Steps:**
1. Database migration (add sync_history table)
2. Deploy application update
3. Enable one-way sync (Waitlist → LeanKit)
4. Monitor sync operations for 3 days
5. Address any issues
6. Full production rollout

### Phase 3 Deployment (Weeks 5-7)

**Prerequisites:**
- Phase 2 stable with >95% sync success rate
- Conflict resolution strategy approved

**Deployment Steps:**
1. Deploy bidirectional sync update
2. Enable auto-sync with 60-minute interval (conservative)
3. Monitor for conflicts
4. Gradually reduce interval to 15 minutes
5. Full production rollout

### Phase 4 Deployment (Weeks 8-10)

**Prerequisites:**
- LeanKit webhooks available (verify with LeanKit support)
- Public webhook endpoint configured

**Deployment Steps:**
1. Deploy webhook endpoint
2. Configure webhook in LeanKit
3. Test webhook delivery
4. Enable real-time sync
5. Disable polling (keep as fallback)
6. Monitor webhook reliability

---

## Appendix: HTML Mockups

The following HTML mockups demonstrate the complete LeanKit integration workflow in the MTM Waitlist Application. These can be opened in a browser and navigated interactively.

### File Structure
```
docs/MTM_Waitlist_Application/Mockups/LeanKit_Integration/
├── index.html (Main navigation)
├── 1_leankit_configuration.html
├── 2_waitlist_with_sync_status.html
├── 3_sync_dashboard.html
├── 4_sync_error_detail.html
├── 5_conflict_resolution.html
├── assets/
│   ├── styles.css
│   └── scripts.js
```

**Note:** See separate files in the `Mockups/` directory for full interactive HTML prototypes with mock data and navigation.

---

## Document Control

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-20 | AI Assistant | Initial specification document |

---

**End of Specification Document**
