# Product Context

**Last Updated:** 2026-02-19

## Why This Project Exists

Manufacturing facilities need efficient receiving operations to:
- Track incoming materials accurately
- Generate labels for parts and shipments
- Integrate with ERP systems (Infor Visual)
- Maintain compliance and audit trails

## Problems It Solves

1. **Manual Label Generation** - Automates label creation for received parts
2. **ERP Disconnection** - Provides real-time integration with Infor Visual
3. **Workflow Inefficiency** - Streamlines receiving processes with guided workflows
4. **Data Accuracy** - Validates data against ERP before processing
5. **Audit Requirements** - Maintains comprehensive logging and audit trails

## How It Should Work

### User Experience Flow

1. **Receiving Mode Selection** - User chooses receiving workflow type
2. **Data Entry** - Guided entry with validation against ERP
3. **Label Queueing** - Label rows are stored in MySQL active queue for print workflows
4. **ERP Update** - Real-time synchronization with Infor Visual (read-only)
5. **Audit Logging** - All operations logged with user, timestamp, machine
6. **Clear Label Data** - Explicit archive action moves active queue rows to history

### Key Features

- **Multiple Receiving Workflows** - Support for different receiving scenarios
- **Label Customization** - Configurable label formats per customer/part
- **Batch Processing** - Handle multiple items efficiently
- **Error Handling** - Clear error messages with recovery options
- **Offline Capability** - Queue operations when ERP unavailable

## User Personas

1. **Receiving Clerk** - Primary user, processes incoming materials
2. **Supervisor** - Reviews reports, manages settings
3. **IT Administrator** - Maintains configuration, troubleshoots

## Success Metrics

- Time to process receiving reduced by 50%
- Zero label errors
- 100% audit trail completeness
- User satisfaction rating > 4.5/5
