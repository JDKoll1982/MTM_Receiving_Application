# Help System Architecture

## Overview
The MTM Receiving Application implements a centralized help system that replaces all hard-coded tooltips, placeholders, tips, and contextual help content throughout the application.

## Components

### 1. Service Layer
**File**: `Services/Help/Service_Help.cs`

**Interface**: `Contracts/Services/IService_Help.cs`

**Registered as**: Singleton in `App.xaml.cs`

**Key Methods**:
- `ShowHelpAsync(string helpKey)` - Displays help dialog for a specific key
- `ShowContextualHelpAsync(Enum_DunnageWorkflowStep step)` - Shows workflow-specific help for dunnage
- `ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step)` - Shows workflow-specific help for receiving
- `GetHelpContent(string key)` - Retrieves help content by key
- `GetHelpByCategory(string category)` - Gets all help in a category
- `SearchHelp(string searchTerm)` - Full-text search across help content
- `IsDismissedAsync(string helpKey)` - Checks if user dismissed a tip
- `SetDismissedAsync(string helpKey, bool isDismissed)` - Marks tip as dismissed

**Legacy Methods** (for backward compatibility):
- `GetTooltip(string elementName)` - Returns tooltip text
- `GetPlaceholder(string fieldName)` - Returns placeholder text
- `GetTip(string viewName)` - Returns tip text
- `GetInfoBarMessage(string messageKey)` - Returns InfoBar message text

### 2. Model Layer
**File**: `Models/Core/Model_HelpContent.cs`

**Properties**:
- `Key` (string) - Unique identifier (e.g., "Dunnage.PartSelection")
- `Title` (string) - Display title for help dialog
- `Content` (string) - Main help text (supports markdown formatting)
- `Category` (string) - Grouping category (e.g., "Dunnage Workflow", "Admin")
- `HelpType` (Enum_HelpType) - Type classification
- `Icon` (string) - Material Design icon name
- `Severity` (Enum_HelpSeverity) - Importance level
- `RelatedKeys` (List<string>) - Links to related help topics

### 3. Enums
**File**: `Models/Enums/Enum_HelpType.cs`
- `Tip` - Quick tips and hints
- `Info` - Informational content
- `Warning` - Warnings and cautions
- `Tutorial` - Step-by-step guides
- `Reference` - Reference documentation

**File**: `Models/Enums/Enum_HelpSeverity.cs`
- `Info` - Standard information
- `Warning` - Important warnings
- `Critical` - Critical information requiring attention

### 4. View Layer
**File**: `Views/Shared/Shared_HelpDialog.xaml` and `.xaml.cs`

**ViewModel**: `ViewModels/Shared/Shared_HelpDialogViewModel.cs`

**Registered as**: Transient (new instance per dialog display)

**Features**:
- Dynamic content rendering with icon and title
- Scrollable content area with RichTextBlock
- Related topics section with clickable links
- Copy content to clipboard functionality
- "Don't show again" checkbox for dismissible tips
- Material Design icons for visual feedback

## Content Organization

### Help Content Keys (Hierarchical Structure)

#### Dunnage Workflow
- `Dunnage.ModeSelection` - Entry mode selection
- `Dunnage.TypeSelection` - Dunnage type selection
- `Dunnage.PartSelection` - Part/spec selection
- `Dunnage.QuantityEntry` - Quantity input
- `Dunnage.DetailsEntry` - PO and location entry
- `Dunnage.Review` - Review and save
- `Dunnage.ManualEntry` - Bulk entry mode
- `Dunnage.EditMode` - Historical data editing

#### Receiving Workflow
- `Receiving.ModeSelection` - Entry mode selection
- `Receiving.POEntry` - PO number entry
- `Receiving.PartSelection` - Part selection from PO
- `Receiving.LoadEntry` - Load number entry
- `Receiving.WeightQuantity` - Weight vs quantity selection
- `Receiving.HeatLot` - Heat/lot number entry
- `Receiving.PackageType` - Package type selection
- `Receiving.Review` - Review and label generation
- `Receiving.ManualEntry` - Bulk entry mode
- `Receiving.EditMode` - Historical data editing

#### Admin
- `Admin.Types` - Dunnage type management
- `Admin.Parts` - Part management
- `Admin.Inventory` - Inventory management

#### Tooltips (Prefix: `Tooltip.`)
Format: `Tooltip.Button.<ActionName>` or `Tooltip.Field.<FieldName>`

Examples:
- `Tooltip.Button.QuickGuidedWizard` - Quick access button
- `Tooltip.Button.AddMultipleRows` - Manual entry action
- `Tooltip.Field.TypeName` - Admin form field

#### Placeholders (Prefix: `Placeholder.`)
Format: `Placeholder.Field.<FieldName>`

Examples:
- `Placeholder.Field.PONumber` - PO number input
- `Placeholder.Field.Location` - Location input
- `Placeholder.Field.StartDate` - Date picker

#### Tips (Prefix: `Tip.`)
Format: `Tip.<Workflow>.<ViewName>`

Examples:
- `Tip.Dunnage.QuantityEntry` - Contextual tip
- `Tip.Receiving.POEntry` - Workflow hint

#### InfoBar Messages (Prefix: `InfoBar.`)
Format: `InfoBar.<MessageKey>`

Examples:
- `InfoBar.InventoryWarning` - Inventory status warning
- `InfoBar.SaveSuccess` - Success confirmation

## Integration Patterns

### ViewModel Integration
All ViewModels inject `IService_Help` via constructor:

```csharp
public partial class MyViewModel : BaseViewModel
{
    private readonly IService_Help _helpService;
    
    public MyViewModel(
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _helpService = helpService;
    }
    
    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowContextualHelpAsync(CurrentStep);
    }
}
```

### XAML Binding Patterns

**Method 1: Direct Binding (for tooltips, placeholders)**
```xml
<TextBox PlaceholderText="{x:Bind ViewModel.HelpService.GetPlaceholder('Field.PONumber'), Mode=OneTime}"/>
<Button ToolTipService.ToolTip="{x:Bind ViewModel.HelpService.GetTooltip('Button.Save'), Mode=OneTime}"/>
```

**Method 2: Command Binding (for help dialogs)**
```xml
<Button Command="{x:Bind ViewModel.ShowHelpCommand}" ToolTipService.ToolTip="Click for help"/>
```

**Method 3: Computed Property (for tips)**
```csharp
public string CurrentTip => _helpService.GetTip($"{WorkflowName}.{CurrentStepName}");
```

```xml
<TextBlock Text="{x:Bind ViewModel.CurrentTip, Mode=OneWay}"/>
```

## Data Flow

1. **Initialization**: Service loads all help content into in-memory cache on application startup
2. **Retrieval**: ViewModels request content via service methods
3. **Display**: 
   - Static content (tooltips, placeholders) binds directly to service methods
   - Dynamic content (dialogs) shown via `ShowHelpAsync()` methods
4. **Dialog Display**: 
   - Service creates dialog instance from DI container
   - Sets XamlRoot from window service
   - Populates content and displays asynchronously
5. **Dismissal Tracking**: User preference stored in HashSet (in-memory, session-scoped)

## Threading Model
- All dialog display operations execute on UI thread via `IService_Dispatcher`
- Content retrieval is synchronous (cached data)
- Dialog display is asynchronous (awaits ContentDialog.ShowAsync())

## Performance Considerations
- **Cache-First**: All content loaded once at startup, O(1) dictionary lookups
- **No Database Calls**: Content is code-defined, no runtime queries
- **Lazy Dialog Creation**: Dialogs created only when needed via DI
- **Efficient Binding**: x:Bind (compile-time) preferred over Binding (runtime)

## Extensibility

### Adding New Help Content
1. Add help key constant to relevant area in `Service_Help.cs`
2. Create `Model_HelpContent` with all properties
3. Call `AddHelpContent()` in appropriate initialization method
4. Use key in ViewModel or XAML binding

### Adding New Content Types
1. Extend `Enum_HelpType` with new type
2. Update dialog rendering logic if custom styling needed
3. Add initialization method for new content category

### Adding Search/Filter
- Service already includes `SearchHelp()` and `GetHelpByCategory()`
- Can be exposed through a dedicated search UI if needed

## Testing Strategy
- **Unit Tests**: Test help content retrieval, search, categorization
- **Integration Tests**: Verify dialog display, ViewModel integration
- **Manual Tests**: Verify all tooltips, placeholders, and help dialogs render correctly

## Migration Path (From Hard-Coded to Service)
1. Identify hard-coded string (e.g., `PlaceholderText="Enter PO..."`)
2. Create help content key (e.g., `Placeholder.Field.PONumber`)
3. Add content to service initialization
4. Update XAML to bind to service method
5. Test rendering and verify content displays correctly

## Known Limitations
- Dismissal persistence is session-only (resets on app restart)
- Content is code-defined (not externally editable)
- Related topics navigation limited to single-level depth
- No multilingual support (English only)

## Future Enhancements
- Persistent dismissal tracking (save to database or settings)
- External content file (JSON/XML) for easier editing
- Multi-language support with resource files
- F1 keyboard shortcut for contextual help
- Help search dialog with fuzzy matching
- Help history/recently viewed topics
- Embedded video tutorials via links
