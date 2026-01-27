# About Module_Receiving

**Last Updated: 2025-01-15**

## Purpose

Module_Receiving handles all receiving operations for manufacturing—the process of logging incoming materials, parts, and shipments into the system. It's the main entry point for receiving floor workers to record what arrived, verify quantities and details, and generate the necessary labels for tracking.

## What Problems It Solves

1. **Streamlined Data Entry**: Eliminates manual paperwork by guiding users through a step-by-step receiving wizard
2. **Accuracy**: Validates purchase orders, part numbers, and quantities against ERP data before saving
3. **Flexibility**: Supports both guided workflows for complex receives and quick bulk entry for simple ones
4. **Label Generation**: Automatically creates CSV files for label printing systems
5. **Error Recovery**: Maintains session state so work isn't lost if something goes wrong

## Who Uses This Module

- **Receiving Clerks**: Primary users who log incoming shipments daily
- **Supervisors**: Review receiving history and correct errors
- **Quality Control**: Access heat/lot information for traceability
- **IT Support**: Troubleshoot receiving issues and manage CSV file paths

## Core Capabilities

### Guided Receiving Wizard
A step-by-step workflow that walks users through:
1. **Mode Selection**: Choose wizard, manual entry, or edit mode
2. **PO Entry**: Enter purchase order number and validate with ERP
3. **Part Selection**: Choose which parts from the PO to receive (if multiple)
4. **Load Entry**: Specify how many loads/containers
5. **Weight & Quantity**: Enter weight and quantity per load
6. **Heat/Lot**: Add traceability information (optional)
7. **Package Type**: Select container/package type
8. **Review & Save**: Confirm details and generate labels

### Manual Entry Mode
A grid-based interface for experienced users to:
- Enter multiple lines at once
- Bypass the wizard for simple receives
- Copy/paste data from spreadsheets

### Edit Mode
Modify existing receiving records from:
- Current session (before saving)
- CSV files (reload and edit)
- Database history (load past receives)

## How It Fits in the Application

Module_Receiving is one of the main modules accessed from the application's home screen. It works alongside:
- **Module_Routing**: Routes completed receives to appropriate destinations
- **Module_Dunnage**: Manages reusable container tracking
- **Module_Volvo**: Handles special Volvo supplier requirements
- **Module_Reporting**: Generates reports from receiving data

## Key Data Flow

```
User Input → Validation (ERP Check) → Session Save → CSV Generation → Database Save → Label Print
```

## Technical Owner

- **Module**: Module_Receiving
- **Primary Developers**: Manufacturing IT team
- **Data Owner**: Operations team (MySQL receiving database)
- **Integration Owner**: ERP team (Infor Visual read-only access)
