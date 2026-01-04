# MTM Receiving Application - UI Component Inventory

## Overview
WinUI 3 desktop application with modular MVVM architecture. All views use `x:Bind` for compile-time binding to ViewModels.

## Application Shell

### Main Window
- **MainWindow.xaml**: Root application window
- **ViewModel_Shared_MainWindow**: Main window coordination

### Core Pages
- **Main_ReceivingLabelPage**: Receiving workflow entry point
- **Main_DunnageLabelPage**: Dunnage workflow entry point
- **Main_CarrierDeliveryLabelPage**: Carrier label workflow

## Module_Shared Components

### Authentication & Startup
- **View_Shared_SplashScreenWindow**: Splash screen with branding
- **View_Shared_SharedTerminalLoginDialog**: PIN-based authentication for shared terminals
- **View_Shared_NewUserSetupDialog**: First-time user configuration

### Utility Components
- **View_Shared_HelpDialog**: Context-sensitive help system
- **View_Shared_IconSelectorWindow**: Material Design icon picker

## Module_Receiving Components

### Workflow Views (Linear Wizard)
- **View_Receiving_Workflow**: Main workflow container with navigation
- **View_Receiving_ModeSelection**: Choose between Standard/Manual/Edit modes
- **View_Receiving_POEntry**: Purchase Order validation and entry
- **View_Receiving_LoadEntry**: Carrier and packing slip details
- **View_Receiving_WeightQuantity**: Weight and quantity capture
- **View_Receiving_HeatLot**: Heat number and lot number entry
- **View_Receiving_PackageType**: Package configuration selection
- **View_Receiving_Review**: Final review before commit

### Alternate Modes
- **View_Receiving_ManualEntry**: Manual data entry without PO validation
- **View_Receiving_EditMode**: Edit existing receiving records

## Module_Dunnage Components

### User Workflow Views
- **View_Dunnage_WorkflowView**: Main dunnage workflow container
- **View_Dunnage_ModeSelectionView**: Select Incoming/Outgoing/Transfer/Count
- **View_Dunnage_TypeSelectionView**: Choose packaging type (bins/racks/pallets)
- **View_Dunnage_PartSelectionView**: Select specific part number
- **View_Dunnage_QuantityEntryView**: Enter quantity with large touch targets
- **View_Dunnage_DetailsEntryView**: Capture custom field data
- **View_Dunnage_ReviewView**: Final review before commit

### Admin Dashboard Views
- **View_Dunnage_AdminMainView**: Admin dashboard landing page
- **View_Dunnage_AdminTypesView**: Manage dunnage types
- **View_Dunnage_AdminPartsView**: Manage parts and home locations
- **View_Dunnage_AdminInventoryView**: View/edit inventory levels

### Dialogs & Controls
- **View_Dunnage_Dialog_AddTypeDialog**: Quick-add new dunnage type
- **View_Dunnage_QuickAddPartDialog**: Quick-add new part
- **View_Dunnage_QuickAddTypeDialog**: Alternate type creation dialog
- **View_Dunnage_Dialog_AddToInventoriedListDialog**: Add to tracked inventory
- **View_Dunnage_Dialog_AddMultipleRowsDialog**: Batch entry
- **View_Dunnage_Control_IconPickerControl**: Icon selection control

### Alternate Modes
- **View_Dunnage_ManualEntryView**: Manual transaction entry
- **View_Dunnage_EditModeView**: Edit existing transactions

## Module_Settings Components

### Configuration Views
- **View_Settings_Workflow**: Settings workflow container
- **View_Settings_ModeSelection**: Choose settings category
- **View_Settings_DunnageMode**: Configure default dunnage behavior
- **View_Settings_Placeholder**: Placeholder for future settings

## Design System

### Theming
- **Themes/** folder in Module_Core
- Material Design Icons via `Material.Icons.WinUI3`
- Custom converters in `Converters/` folder

### UI Patterns
- **Large Touch Targets**: Optimized for touchscreen terminals
- **Step-by-Step Wizards**: Linear navigation with clear progress
- **Input Validation**: Real-time feedback with error messages
- **Modal Dialogs**: For quick actions and confirmations
- **Admin Dashboards**: Data grids with CRUD operations

### Common Controls
- **TextBox**: Input fields with validation
- **Button**: Primary and secondary actions
- **ListView/DataGrid**: Data display and selection
- **ComboBox**: Dropdown selections
- **NumberBox**: Numeric input
- **ProgressRing**: Loading indicators
- **InfoBar**: Status messages and errors

## Navigation Architecture
- **Workflow-based**: Each module has a workflow container
- **Step Navigation**: Forward/Back through wizard steps
- **Modal Overlays**: Dialogs for quick actions
- **Menu-based**: Main menu for module selection

## Accessibility
- Keyboard navigation support
- High contrast theme compatibility
- Large font scaling support
- Screen reader friendly (XAML automation)
