# LeanKit Integration for MTM Waitlist Application

## Overview

This directory contains the complete specification and interactive mockups for integrating LeanKit (a visual project management platform) with the MTM Waitlist Application.

## Directory Structure

```
LeanKit_Integration/
├── README.md (this file)
├── Specification/
│   └── LeanKit_Integration_Specification.md
└── Mockups/
    ├── index.html (navigation hub)
    ├── 1_leankit_configuration.html
    ├── 2_waitlist_with_sync_status.html
    ├── 3_sync_dashboard.html
    ├── 4_sync_error_detail.html
    ├── 5_conflict_resolution.html
    └── assets/
        ├── styles.css
        └── scripts.js
```

## Quick Start

### View the Specification

Open `../Specifications/LeanKit_Integration_Specification.md` in any Markdown viewer to read the complete technical specification, which includes:

- **Phase-Based Implementation Plan** (4 phases over 10 weeks)
- **Module Breakdown** (6 core modules with detailed APIs)
- **Data Flow Workflows** (3 complete workflow diagrams)
- **Database Schema** (MySQL tables and field mappings)
- **Security Guidelines** (encryption, authentication, rate limiting)
- **Testing Strategy** (unit tests, integration tests, manual test plans)

### Explore the Interactive Mockups

1. Open `Mockups/index.html` in a modern web browser (Chrome, Edge, Firefox)
2. Click on any mockup card to view that screen
3. Interact with buttons and form elements to simulate the workflow
4. All mockups include realistic mock data and working JavaScript

#### Mockup Screens:

1. **LeanKit Configuration** - Set up API connection and sync settings
2. **Waitlist with Sync Status** - Main waitlist view with sync indicators
3. **Sync Dashboard** - Monitor sync operations and statistics
4. **Sync Error Details** - Detailed error information and troubleshooting
5. **Conflict Resolution** - Resolve synchronization conflicts

## Key Features

### Bidirectional Synchronization

- **Waitlist → LeanKit**: Create and update LeanKit cards from waitlist entries
- **LeanKit → Waitlist**: Update waitlist entries when cards change in LeanKit
- **Conflict Detection**: Automatically detect when both systems have changed
- **Conflict Resolution**: User-friendly interface for resolving conflicts

### Real-Time Status Tracking

- Per-entry sync status badges (Synced, Pending, Error, Not Synced)
- Last sync timestamps
- Sync history with detailed logs
- Error tracking and retry management

### Robust Error Handling

- Exponential backoff retry logic
- Network timeout handling
- Rate limit management
- Detailed error diagnostics

### Security

- Token-based authentication (recommended)
- Encrypted credential storage
- HTTPS-only communication
- Audit trail for all sync operations

## Implementation Phases

### Phase 1: Foundation (Weeks 1-2)
- Authentication setup
- Read-only LeanKit integration
- Configuration UI
- Logging infrastructure

### Phase 2: One-Way Sync (Weeks 3-4)
- Waitlist → LeanKit card creation
- Manual sync triggers
- Retry logic
- Sync status indicators

### Phase 3: Bidirectional Sync (Weeks 5-7)
- Full two-way synchronization
- Background sync service
- Conflict detection and resolution
- Automated sync (polling-based)

### Phase 4: Advanced Features (Weeks 8-10)
- Webhook support (if available)
- Real-time notifications
- Performance optimizations
- Analytics dashboard

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | WinUI 3 (Windows App SDK 1.6+) |
| **Language** | C# 12 (.NET 8) |
| **Architecture** | MVVM with CommunityToolkit.Mvvm |
| **Database** | MySQL 8.0 |
| **HTTP Client** | HttpClient with Polly (retry logic) |
| **Testing** | xUnit with FluentAssertions |

## Data Mapping

| Waitlist Field | LeanKit Card Field | Notes |
|----------------|-------------------|-------|
| EntryID | ExternalCardId | Unique identifier link |
| CustomerName | Title | Direct mapping |
| Description | Description | HTML sanitization applied |
| Status | LaneId | Configurable lane mapping |
| Priority | Priority | 1-3 scale |
| AssignedTo | AssignedUserIds[] | User lookup required |
| Notes | Comments | Separate comment API |

## LeanKit Board Information

**Board URL:** https://mtmfg.leankit.com/board/1490772554  
**Account:** mtmfg  
**Authentication:** API Token (stored encrypted)

## Key Services & Interfaces

### Core Services

```csharp
IService_LeanKit_Authentication    // API authentication
IService_LeanKit_SyncEngine        // Orchestrates sync operations
IService_LeanKit_Mapper            // Data transformation
IService_LeanKit_RetryPolicy       // Error handling
```

### Data Access Objects (DAOs)

```csharp
Dao_LeanKit_Board      // Board operations
Dao_LeanKit_Card       // Card CRUD
Dao_LeanKit_Comment    // Comment management
```

## Database Schema Updates

### New Tables

```sql
leankit_sync_history    // Tracks all sync operations
```

### Existing Table Updates

```sql
ALTER TABLE waitlist_entries ADD (
    leankit_card_id VARCHAR(255),
    leankit_sync_status ENUM(...),
    leankit_last_sync_at DATETIME,
    leankit_sync_error TEXT
);
```

## Configuration Settings

Stored in MySQL `app_settings` table:

- `LeanKit.Account` - Account subdomain
- `LeanKit.ApiUrl` - API base URL
- `LeanKit.BoardId` - Board identifier
- `LeanKit.Token` - API token (encrypted)
- `LeanKit.SyncIntervalMinutes` - Auto-sync frequency
- `LeanKit.EnableSync` - Enable/disable flag
- `LeanKit.MaxRetries` - Retry limit
- `LeanKit.RetryDelaySeconds` - Initial retry delay

## Workflow Examples

### Create and Sync New Entry

```
1. User creates waitlist entry in app
2. Entry saved with sync_status = "SyncPending"
3. User clicks "Sync to LeanKit" (or auto-sync triggers)
4. System maps entry to LeanKit card format
5. API call creates card in LeanKit
6. Entry updated with card_id and sync_status = "Synced"
7. Sync logged in history table
```

### Handle Sync Conflict

```
1. Auto-sync detects both systems changed
2. Conflict dialog displays both versions
3. User selects which version to keep
4. Winning version applied to both systems
5. Conflict resolution logged
```

### Retry Failed Sync

```
1. Network timeout during card creation
2. Error logged with retry count (1/3)
3. Wait 5 seconds (exponential backoff)
4. Retry operation
5. On success: update entry, clear error
6. On failure: increment retry count, wait longer
```

## Testing

### Unit Tests

- Test all mapping logic
- Mock API responses
- Verify retry behavior
- Validate error handling

### Integration Tests

- Test actual LeanKit API calls
- Verify bidirectional sync
- Test conflict resolution
- Validate error scenarios

### Manual Testing

- Follow test plan in specification
- Test all UI interactions
- Verify sync indicators
- Test error recovery

## Deployment Checklist

- [ ] Generate LeanKit API token
- [ ] Run database migrations
- [ ] Configure app settings (encrypted)
- [ ] Test connection to LeanKit
- [ ] Configure lane mappings
- [ ] Enable auto-sync with conservative interval
- [ ] Monitor sync operations for 48 hours
- [ ] Gradually reduce sync interval
- [ ] Train users on new features

## Security Considerations

⚠️ **Critical Security Requirements:**

1. **Never commit credentials to source control**
2. Store API tokens encrypted in database
3. Use HTTPS for all API communication
4. Implement rate limiting to avoid API abuse
5. Log all sync operations for audit trail
6. Validate all user inputs before sync
7. Use least-privilege access for LeanKit API

## Support & Troubleshooting

### Common Issues

**Connection Failures:**
- Verify API token validity
- Check network connectivity
- Confirm LeanKit service status

**Sync Errors:**
- Check sync history for details
- Verify lane mapping configuration
- Ensure entry data is valid

**Conflicts:**
- Review conflict resolution settings
- Check timestamps for accuracy
- Verify user permissions

### Getting Help

- Review error details in Sync Dashboard
- Check LeanKit API documentation
- Contact LeanKit support for API issues
- Review application logs for debugging

## Future Enhancements

- **Webhook Support**: Real-time updates via webhooks
- **Batch Operations**: Bulk sync for performance
- **Custom Field Mapping**: Sync custom fields
- **Advanced Analytics**: Sync performance metrics
- **Multi-Board Support**: Sync across multiple boards
- **Attachment Sync**: Synchronize file attachments

## Resources

- [LeanKit API Documentation](https://docs.leankit.com/display/LM/LeanKit+API+Documentation)
- [LeanKit Node Client](https://github.com/LeanKit/leankit-node-client)
- [MTM Waitlist Application Constitution](../../../../.github/CONSTITUTION.md)
- [Testing Strategy](../../../../.github/instructions/testing-strategy.instructions.md)

## License

Internal use only - MTM Manufacturing

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-20 | AI Assistant | Initial specification and mockups |

---

**Questions or Issues?** Contact the development team or open an issue in the project repository.
