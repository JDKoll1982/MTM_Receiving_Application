# LeanKit Integration for MTM Waitlist Application

## Board Information

**LeanKit Board URL:** <https://mtmfg.leankit.com/board/1490772554>

## Authentication Credentials

- **Username:** <shop2@mantoolmfg.com>
- **Password:** shop2

## Integration Overview

The MTM Waitlist Application will integrate with LeanKit to synchronize task and workflow data. This integration will enable real-time visibility into manufacturing operations and streamline process management.

## API Integration Requirements

### Authentication

- LeanKit uses API token-based authentication
- Credentials must be stored securely using encrypted configuration (never hardcoded)
- Use `IService_Configuration` for credential management

### Endpoints to Integrate

- **Board API:** Retrieve board structure and lane configuration
- **Card API:** Create, update, and query cards (work items)
- **User API:** Validate authentication and retrieve user permissions

### Data Synchronization

**Waitlist → LeanKit:**

- New waitlist entries create LeanKit cards
- Status updates sync to appropriate lanes
- Priority changes update card priority

**LeanKit → Waitlist:**

- Card completions update waitlist status
- Comments sync to waitlist notes
- Lane movements trigger waitlist workflow transitions

## Implementation Considerations

### Service Layer

Create `IService_LeanKit` interface with methods:

- `AuthenticateAsync()` - Validate credentials
- `CreateCardAsync(Model_WaitlistEntry entry)` - Create new card
- `UpdateCardStatusAsync(string cardId, string status)` - Update card
- `GetBoardStructureAsync()` - Retrieve lanes and card types
- `SyncCardsAsync()` - Bi-directional sync operation

### Data Model Mapping

| Waitlist Field | LeanKit Card Field |
|----------------|-------------------|
| EntryID | ExternalCardID |
| CustomerName | Title |
| Priority | Priority |
| Status | Lane Name |
| Notes | Description |
| CreatedDate | CreatedOn |
| AssigneDataTransferObjects | AssignedUsers |

### Error Handling

- Network failures must not block waitlist operations
- Implement retry logic with exponential backoff
- Log all API errors using `IService_LoggingUtility`
- Queue failed sync operations for retry

### Security Best Practices

- ✅ Store credentials in encrypted app settings (MySQL `app_settings` table)
- ✅ Use HTTPS for all API calls
- ✅ Implement token refresh logic
- ❌ Never log credentials or tokens
- ❌ Never commit credentials to source control

## Configuration Settings

Add to `app_settings` table:

```sql
INSERT IGNORE INTO app_settings (setting_key, setting_value, description)
VALUES
('LeanKit.ApiUrl', 'https://mtmfg.leankit.com/api/v1', 'LeanKit API base URL'),
('LeanKit.BoardId', '1490772554', 'MTM Waitlist LeanKit board ID'),
('LeanKit.SyncIntervalMinutes', '15', 'Sync frequency in minutes'),
('LeanKit.EnableSync', 'true', 'Enable/disable LeanKit synchronization');
```

## Testing Strategy

### Unit Tests

- Mock LeanKit API responses
- Test credential validation
- Test data mapping transformations

### Integration Tests

- Test actual API calls against LeanKit sandbox (if available)
- Verify bi-directional sync logic
- Test error handling and retry mechanisms

### Manual Testing Checklist

- [ ] Authenticate with provided credentials
- [ ] Create test card from waitlist entry
- [ ] Update card status and verify sync
- [ ] Delete card and verify waitlist handling
- [ ] Test network failure scenarios

## Rollout Plan

1. **Phase 1:** Read-only integration (query LeanKit data)
2. **Phase 2:** One-way sync (Waitlist → LeanKit)
3. **Phase 3:** Bi-directional sync with conflict resolution
4. **Phase 4:** Real-time webhooks (if LeanKit supports)

## References

- LeanKit API Documentation: <https://support.leankit.com/hc/en-us/articles/360019280651>
- LeanKit .NET SDK: <https://github.com/LeanKit/LeanKit.API.Client.Library>
- Authentication Guide: <https://support.leankit.com/hc/en-us/articles/360019371472>
- LeanKit API: <https://github.com/LeanKit/leankit-node-client>
- Leankit API Documentation: <https://docs.leankit.com/display/LM/LeanKit+API+Documentation>

---

**⚠️ Security Notice:** Credentials in this document are for development/testing only. Production credentials must be managed through secure configuration management and never committed to source control.
