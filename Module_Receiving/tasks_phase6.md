# Phase 6: Views & UI (XAML) - Task List

**Phase:** 6 of 8  
**Status:** ⏳ PENDING  
**Priority:** HIGH - User interface implementation  
**Dependencies:** Phase 5 (Wizard ViewModels) must be complete

---

## 📊 **Phase 6 Overview**

**Goal:** Implement all XAML Views for Hub and Wizard Mode

**View Types:**
- **Pages:** Full-screen views
- **UserControls:** Reusable components
- **Dialogs:** Modal windows
- **Panels:** Side/embedded panels

**Status:**
- ⏳ Hub Views: 0/4 complete
- ⏳ Wizard Orchestration Views: 0/2 complete
- ⏳ Step 1 Views: 0/12 complete
- ⏳ Step 2 Views: 0/16 complete
- ⏳ Step 3 Views: 0/12 complete
- ⏳ Shared Controls: 0/8 complete
- ⏳ Dialog Views: 0/8 complete

**Completion:** 0/62 tasks (0%)

**Estimated Total Time:** 50-60 hours

---

## 🎨 **XAML Standards (CRITICAL)**

### Binding Requirements

**✅ REQUIRED - Compile-time binding:**
```xaml
<!-- ALWAYS use x:Bind with explicit Mode -->
<TextBox Text="{x:Bind ViewModel.PONumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
<Button Content="Save" Command="{x:Bind ViewModel.SaveCommand}" />
<ListView ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}" />
```

**❌ FORBIDDEN - Runtime binding:**
```xaml
<!-- NEVER use {Binding} -->
<TextBox Text="{Binding MyProperty}" />
```

### DataTemplate Requirements

```xaml
<!-- ALWAYS specify x:DataType for templates -->
<DataTemplate x:DataType="models:Model_Receiving_DataTransferObjects_LoadGridRow">
    <TextBlock Text="{x:Bind LoadNumber}" />
</DataTemplate>
```

### Code-Behind Rules

**✅ ALLOWED in .xaml.cs:**
- Window size initialization (WindowHelper_WindowSizeAndStartupLocation)
- Event wire-up (Loaded, Unloaded)
- ViewModel property initialization

**❌ FORBIDDEN in .xaml.cs:**
- Business logic
- Data access
- Service calls
- State management

---

## ⏳ **Hub Views (4 tasks)**

### Task 6.1: View_Receiving_Hub_Orchestration_MainWorkflow

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Orchestration_MainWorkflow.xaml`
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Orchestration_MainWorkflow.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Hub_Orchestration_MainWorkflow  
**Estimated Time:** 3 hours

**UI Layout:**
- Title bar with app logo
- Mode selection panel (3 buttons: Wizard, Manual, Edit)
- Non-PO toggle switch
- Footer with help button

**XAML Structure:**
```xaml
<Page x:Class="MTM_Receiving_Application.Module_Receiving.Views.Hub.View_Receiving_Hub_Orchestration_MainWorkflow">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <!-- Header -->
            <RowDefinition Height="*" />    <!-- Content -->
            <RowDefinition Height="Auto" /> <!-- Footer -->
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <StackPanel Grid.Row="0" Padding="20">
            <TextBlock Text="MTM Receiving Application" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="Select Workflow Mode" Style="{StaticResource SubtitleTextBlockStyle}" />
        </StackPanel>
        
        <!-- Mode Selection Buttons -->
        <StackPanel Grid.Row="1" Spacing="20" HorizontalAlignment="Center" VerticalAlignment="Center">
            <Button 
                Content="Guided Mode (Wizard)" 
                Command="{x:Bind ViewModel.SelectWizardModeCommand}"
                Width="300" Height="80" />
            
            <Button 
                Content="Manual Entry" 
                Command="{x:Bind ViewModel.SelectManualModeCommand}"
                Width="300" Height="80" />
            
            <Button 
                Content="Edit Mode" 
                Command="{x:Bind ViewModel.SelectEditModeCommand}"
                Width="300" Height="80" />
            
            <ToggleSwitch 
                Header="Non-PO Receiving"
                IsOn="{x:Bind ViewModel.IsNonPO, Mode=TwoWay}"
                OnContent="Non-PO Mode" 
                OffContent="PO Mode" />
        </StackPanel>
        
        <!-- Footer -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" Padding="20">
            <Button Content="Help" Command="{x:Bind ViewModel.ShowHelpCommand}" />
        </StackPanel>
    </Grid>
</Page>
```

**Code-Behind:**
```csharp
public sealed partial class View_Receiving_Hub_Orchestration_MainWorkflow : Page
{
    public ViewModel_Receiving_Hub_Orchestration_MainWorkflow ViewModel { get; }
    
    public View_Receiving_Hub_Orchestration_MainWorkflow()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Hub_Orchestration_MainWorkflow>();
        InitializeComponent();
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 800, 600);
    }
}
```

**Acceptance Criteria:**
- [ ] Three mode selection buttons
- [ ] Non-PO toggle switch
- [ ] All buttons bound to commands
- [ ] Window sized appropriately
- [ ] ViewModel property set

---

### Task 6.2: View_Receiving_Hub_Display_ModeSelection

**Priority:** P2 - MEDIUM  
**Files:**
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Display_ModeSelection.xaml`
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Display_ModeSelection.xaml.cs`  
**Estimated Time:** 2 hours

**UI Layout:**
- Mode selection cards with icons
- Description text for each mode
- Visual selection indicator

**Acceptance Criteria:**
- [ ] Three mode cards
- [ ] Icons/visuals for each mode
- [ ] Selection highlights

---

### Task 6.3: View_Receiving_Hub_Dialog_ModeHelpDialog

**Priority:** P3 - LOW  
**Files:**
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Dialog_ModeHelpDialog.xaml`
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Dialog_ModeHelpDialog.xaml.cs`  
**Estimated Time:** 1.5 hours

**UI Layout:**
- ContentDialog with help content
- Explains each workflow mode

**Acceptance Criteria:**
- [ ] ContentDialog format
- [ ] Describes all modes
- [ ] Close button

---

### Task 6.4: View_Receiving_Hub_Display_RecentTransactions

**Priority:** P2 - MEDIUM  
**Files:**
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Display_RecentTransactions.xaml`
- `Module_Receiving/Views/Hub/View_Receiving_Hub_Display_RecentTransactions.xaml.cs`  
**Estimated Time:** 2 hours

**UI Layout:**
- ListView showing last 10 transactions
- Click to view details

**Acceptance Criteria:**
- [ ] Shows recent transactions
- [ ] Click to navigate

---

## ⏳ **Wizard Orchestration Views (2 tasks)**

### Task 6.5: View_Receiving_Wizard_Orchestration_MainWorkflow

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Orchestration/View_Receiving_Wizard_Orchestration_MainWorkflow.xaml`
- `Module_Receiving/Views/Wizard/Orchestration/View_Receiving_Wizard_Orchestration_MainWorkflow.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Orchestration_MainWorkflow  
**Estimated Time:** 5 hours

**UI Layout:**
- Step indicator (1 → 2 → 3)
- Frame for step content navigation
- Previous/Next navigation buttons
- Cancel workflow button

**XAML Structure:**
```xaml
<Page x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Orchestration.View_Receiving_Wizard_Orchestration_MainWorkflow">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <!-- Step Indicator -->
            <RowDefinition Height="*" />    <!-- Step Content Frame -->
            <RowDefinition Height="Auto" /> <!-- Navigation Buttons -->
        </Grid.RowDefinitions>
        
        <!-- Step Indicator -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Padding="20">
            <ItemsControl ItemsSource="{x:Bind ViewModel.StepIndicators, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_Receiving_DataTransferObjects_StepIndicator">
                        <StackPanel Orientation="Horizontal" Spacing="10">
                            <Border 
                                Background="{x:Bind IsActive, Mode=OneWay, Converter={StaticResource StepActiveColorConverter}}"
                                CornerRadius="20" Width="40" Height="40">
                                <TextBlock 
                                    Text="{x:Bind StepNumber}" 
                                    HorizontalAlignment="Center" 
                                    VerticalAlignment="Center" />
                            </Border>
                            <TextBlock Text="{x:Bind StepTitle}" VerticalAlignment="Center" />
                        </StackPanel>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
        
        <!-- Step Content Frame -->
        <Frame x:Name="StepContentFrame" Grid.Row="1" Padding="20" />
        
        <!-- Navigation Buttons -->
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right" Padding="20" Spacing="10">
            <Button 
                Content="Cancel" 
                Command="{x:Bind ViewModel.CancelWorkflowCommand}" />
            
            <Button 
                Content="Previous" 
                Command="{x:Bind ViewModel.NavigateToPreviousStepCommand}"
                IsEnabled="{x:Bind ViewModel.CanNavigatePrevious, Mode=OneWay}" />
            
            <Button 
                Content="Next" 
                Command="{x:Bind ViewModel.NavigateToNextStepCommand}"
                IsEnabled="{x:Bind ViewModel.CanNavigateNext, Mode=OneWay}"
                Style="{StaticResource AccentButtonStyle}" />
        </StackPanel>
    </Grid>
</Page>
```

**Code-Behind Navigation Logic:**
```csharp
public sealed partial class View_Receiving_Wizard_Orchestration_MainWorkflow : Page
{
    public ViewModel_Receiving_Wizard_Orchestration_MainWorkflow ViewModel { get; }
    
    public View_Receiving_Wizard_Orchestration_MainWorkflow()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();
        InitializeComponent();
        
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1200, 800);
    }
    
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.CurrentStepNumber))
        {
            NavigateToCurrentStep();
        }
    }
    
    private void NavigateToCurrentStep()
    {
        switch (ViewModel.CurrentStepNumber)
        {
            case 1:
                StepContentFrame.Navigate(typeof(View_Receiving_Wizard_Display_Step1Container));
                break;
            case 2:
                StepContentFrame.Navigate(typeof(View_Receiving_Wizard_Display_Step2Container));
                break;
            case 3:
                StepContentFrame.Navigate(typeof(View_Receiving_Wizard_Display_Step3Container));
                break;
        }
    }
}
```

**Acceptance Criteria:**
- [ ] Step indicator shows all 3 steps
- [ ] Frame navigates between step views
- [ ] Previous/Next buttons enabled/disabled correctly
- [ ] Cancel button shows confirmation
- [ ] Window sized at 1200x800

---

### Task 6.6: View_Receiving_Wizard_Display_StepIndicator (UserControl)

**Priority:** P2 - MEDIUM  
**Files:**
- `Module_Receiving/Views/Wizard/Orchestration/View_Receiving_Wizard_Display_StepIndicator.xaml`
- `Module_Receiving/Views/Wizard/Orchestration/View_Receiving_Wizard_Display_StepIndicator.xaml.cs`  
**Estimated Time:** 2 hours

**UI Layout:**
- Reusable step indicator component
- Shows: [1] → [2] → [3] with active highlighting

**Acceptance Criteria:**
- [ ] UserControl format
- [ ] Visual step progression
- [ ] Active step highlighted

---

## ⏳ **Step 1 Views (12 tasks)**

### Task 6.7: View_Receiving_Wizard_Display_Step1Container

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_Step1Container.xaml`
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_Step1Container.xaml.cs`  
**Dependencies:** All Step 1 ViewModels  
**Estimated Time:** 4 hours

**UI Layout:**
- Container page for all Step 1 components
- PO Number entry section
- Part selection section
- Load count entry section
- Step 1 summary panel (bottom)

**XAML Structure:**
```xaml
<Page x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_Step1Container">
    <Grid Padding="20">
        <StackPanel Spacing="30">
            <!-- PO Number Section -->
            <StackPanel>
                <TextBlock Text="Purchase Order Number" Style="{StaticResource SubtitleTextBlockStyle}" />
                <local:View_Receiving_Wizard_Display_PONumberEntry x:Name="PONumberEntry" />
            </StackPanel>
            
            <!-- Part Selection Section -->
            <StackPanel>
                <TextBlock Text="Part Selection" Style="{StaticResource SubtitleTextBlockStyle}" />
                <local:View_Receiving_Wizard_Display_PartSelection x:Name="PartSelection" />
            </StackPanel>
            
            <!-- Load Count Section -->
            <StackPanel>
                <TextBlock Text="Number of Loads" Style="{StaticResource SubtitleTextBlockStyle}" />
                <local:View_Receiving_Wizard_Display_LoadCountEntry x:Name="LoadCountEntry" />
            </StackPanel>
            
            <!-- Summary Panel -->
            <Border BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" BorderThickness="1" CornerRadius="4" Padding="15">
                <local:View_Receiving_Wizard_Display_Step1Summary x:Name="Step1Summary" />
            </Border>
        </StackPanel>
    </Grid>
</Page>
```

**Acceptance Criteria:**
- [ ] Contains all Step 1 UserControls
- [ ] Logical vertical layout
- [ ] Summary panel at bottom
- [ ] Proper spacing and padding

---

### Task 6.8: View_Receiving_Wizard_Display_PONumberEntry (UserControl)

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_PONumberEntry.xaml`
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_PONumberEntry.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Display_PONumberEntry  
**Estimated Time:** 3 hours

**UI Layout:**
- TextBox for PO Number input
- Validation indicator (✓ or ✗)
- Non-PO checkbox
- Format help text

**XAML Structure:**
```xaml
<UserControl x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_PONumberEntry">
    <StackPanel Spacing="10">
        <StackPanel Orientation="Horizontal" Spacing="10">
            <TextBox 
                x:Name="PONumberTextBox"
                Text="{x:Bind ViewModel.PONumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="PO-123456"
                Width="200"
                IsEnabled="{x:Bind ViewModel.IsNonPO, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}" />
            
            <FontIcon 
                Glyph="&#xE73E;" 
                Foreground="Green"
                Visibility="{x:Bind ViewModel.IsPOValid, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
            
            <FontIcon 
                Glyph="&#xE711;" 
                Foreground="Red"
                Visibility="{x:Bind ViewModel.IsPOValid, Mode=OneWay, Converter={StaticResource InverseBoolToVisibilityConverter}}" />
            
            <ProgressRing 
                Width="20" Height="20"
                IsActive="{x:Bind ViewModel.IsValidating, Mode=OneWay}" />
        </StackPanel>
        
        <CheckBox 
            Content="Non-PO Receiving"
            IsChecked="{x:Bind ViewModel.IsNonPO, Mode=TwoWay}" />
        
        <TextBlock 
            Text="{x:Bind ViewModel.POValidationMessage, Mode=OneWay}"
            Foreground="{x:Bind ViewModel.IsPOValid, Mode=OneWay, Converter={StaticResource ValidationMessageColorConverter}}"
            TextWrapping="Wrap" />
        
        <TextBlock 
            Text="Format: PO-XXXXXX (6 digits)" 
            Foreground="{ThemeResource TextFillColorSecondaryBrush}"
            FontSize="12" />
    </StackPanel>
</UserControl>
```

**Acceptance Criteria:**
- [ ] TextBox with format validation
- [ ] Real-time validation indicators
- [ ] Non-PO checkbox disables PO input
- [ ] Help text shows format
- [ ] Validation message displays

---

### Task 6.9: View_Receiving_Wizard_Display_PartSelection (UserControl)

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_PartSelection.xaml`
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_PartSelection.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Display_PartSelection  
**Estimated Time:** 4 hours

**UI Layout:**
- ComboBox for part selection (with autocomplete)
- TextBox for manual entry (Non-PO mode)
- Part description display
- Part type indicator

**XAML Structure:**
```xaml
<UserControl x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_PartSelection">
    <StackPanel Spacing="10">
        <!-- ComboBox for PO mode -->
        <ComboBox 
            ItemsSource="{x:Bind ViewModel.AvailableParts, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
            DisplayMemberPath="PartNumber"
            PlaceholderText="Select or type part number..."
            Width="300"
            IsEnabled="{x:Bind ViewModel.IsManualEntryMode, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}"
            IsEditable="True">
            
            <ComboBox.ItemTemplate>
                <DataTemplate x:DataType="models:Model_Receiving_DataTransferObjects_PartDetails">
                    <StackPanel>
                        <TextBlock Text="{x:Bind PartNumber}" FontWeight="Bold" />
                        <TextBlock Text="{x:Bind Description}" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                    </StackPanel>
                </DataTemplate>
            </ComboBox.ItemTemplate>
        </ComboBox>
        
        <!-- Manual entry TextBox (Non-PO mode) -->
        <TextBox 
            Text="{x:Bind ViewModel.ManualPartNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
            PlaceholderText="Enter part number..."
            Width="300"
            Visibility="{x:Bind ViewModel.IsManualEntryMode, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
        
        <!-- Part Details Display -->
        <StackPanel Visibility="{x:Bind ViewModel.SelectedPart, Mode=OneWay, Converter={StaticResource NullToVisibilityConverter}}">
            <TextBlock>
                <Run Text="Description: " FontWeight="SemiBold" />
                <Run Text="{x:Bind ViewModel.PartDescription, Mode=OneWay}" />
            </TextBlock>
            <TextBlock>
                <Run Text="Type: " FontWeight="SemiBold" />
                <Run Text="{x:Bind ViewModel.SelectedPart.PartType, Mode=OneWay}" />
            </TextBlock>
        </StackPanel>
    </StackPanel>
</UserControl>
```

**Acceptance Criteria:**
- [ ] ComboBox with autocomplete
- [ ] Shows part description
- [ ] Manual entry mode for Non-PO
- [ ] Part details display
- [ ] Loads parts from PO

---

### Task 6.10: View_Receiving_Wizard_Display_LoadCountEntry (UserControl)

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_LoadCountEntry.xaml`
- `Module_Receiving/Views/Wizard/Step1/View_Receiving_Wizard_Display_LoadCountEntry.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Display_LoadCountEntry  
**Estimated Time:** 2 hours

**UI Layout:**
- NumberBox for load count (1-999)
- Validation message
- Load initialization button

**XAML Structure:**
```xaml
<UserControl x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_LoadCountEntry">
    <StackPanel Spacing="10">
        <NumberBox 
            Value="{x:Bind ViewModel.LoadCount, Mode=TwoWay}"
            Minimum="1"
            Maximum="999"
            SpinButtonPlacementMode="Inline"
            Width="150" />
        
        <TextBlock 
            Text="{x:Bind ViewModel.LoadCountValidationMessage, Mode=OneWay}"
            Foreground="{x:Bind ViewModel.IsLoadCountValid, Mode=OneWay, Converter={StaticResource ValidationMessageColorConverter}}" />
        
        <TextBlock 
            Text="Enter the number of loads to receive (1-999)" 
            Foreground="{ThemeResource TextFillColorSecondaryBrush}"
            FontSize="12" />
    </StackPanel>
</UserControl>
```

**Acceptance Criteria:**
- [ ] NumberBox with range 1-999
- [ ] Validation message
- [ ] Help text

---

### Tasks 6.11-6.18: Additional Step 1 Views

**Summary:**
- Task 6.11: View_Receiving_Wizard_Display_Step1Summary (UserControl) - 1.5 hours
- Task 6.12: View_Receiving_Wizard_Dialog_Step1HelpDialog - 1 hour
- Task 6.13: View_Receiving_Wizard_Display_POValidationIndicator (UserControl) - 1 hour
- Task 6.14: View_Receiving_Wizard_Display_PartDetailsPanel (UserControl) - 2 hours
- Task 6.15: View_Receiving_Wizard_Dialog_ManualPartEntryHelp - 1 hour
- Task 6.16: View_Receiving_Wizard_Display_NonPOModeToggle (UserControl) - 1 hour

**Total Step 1 Views Time:** ~20 hours

---

## ⏳ **Step 2 Views (16 tasks)**

### Task 6.19: View_Receiving_Wizard_Display_Step2Container

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step2/View_Receiving_Wizard_Display_Step2Container.xaml`
- `Module_Receiving/Views/Wizard/Step2/View_Receiving_Wizard_Display_Step2Container.xaml.cs`  
**Dependencies:** All Step 2 ViewModels  
**Estimated Time:** 5 hours

**UI Layout:**
- Container page for Step 2 components
- Load details DataGrid (main component)
- Bulk operation toolbar (top)
- Progress indicator (bottom)

**XAML Structure:**
```xaml
<Page x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step2.View_Receiving_Wizard_Display_Step2Container">
    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <!-- Toolbar -->
            <RowDefinition Height="*" />    <!-- DataGrid -->
            <RowDefinition Height="Auto" /> <!-- Progress -->
        </Grid.RowDefinitions>
        
        <!-- Bulk Operations Toolbar -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="10" Padding="0,0,0,10">
            <Button Content="Bulk Copy Fields" Command="{x:Bind ViewModel.ShowBulkCopyDialogCommand}" />
            <Button Content="Clear Auto-Filled" Command="{x:Bind ViewModel.ClearAutoFilledFieldsCommand}" />
            <Button Content="Help" Command="{x:Bind ViewModel.ShowHelpCommand}" />
        </StackPanel>
        
        <!-- Load Details DataGrid -->
        <local:View_Receiving_Wizard_Display_LoadDetailsGrid 
            Grid.Row="1" 
            x:Name="LoadDetailsGrid" />
        
        <!-- Progress Indicator -->
        <local:View_Receiving_Wizard_Display_Step2Progress 
            Grid.Row="2" 
            x:Name="ProgressIndicator" />
    </Grid>
</Page>
```

**Acceptance Criteria:**
- [ ] Contains LoadDetailsGrid UserControl
- [ ] Bulk operation buttons
- [ ] Progress indicator
- [ ] Proper layout structure

---

### Task 6.20: View_Receiving_Wizard_Display_LoadDetailsGrid (UserControl)

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step2/View_Receiving_Wizard_Display_LoadDetailsGrid.xaml`
- `Module_Receiving/Views/Wizard/Step2/View_Receiving_Wizard_Display_LoadDetailsGrid.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Display_LoadDetailsGrid  
**Estimated Time:** 8 hours (COMPLEX)

**UI Layout:**
- DataGrid with all loads
- Columns: LoadNumber, PartId, Quantity, HeatLot, PackageType, PackagesPerLoad, WeightPerPackage, Location
- Editable cells
- ComboBox columns for PackageType, Location
- Validation indicators per cell

**XAML Structure:**
```xaml
<UserControl x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step2.View_Receiving_Wizard_Display_LoadDetailsGrid">
    <DataGrid 
        ItemsSource="{x:Bind ViewModel.LoadRows, Mode=OneWay}"
        SelectedItem="{x:Bind ViewModel.SelectedLoadRow, Mode=TwoWay}"
        AutoGenerateColumns="False"
        CanUserReorderColumns="False"
        CanUserSortColumns="True"
        IsReadOnly="False"
        SelectionMode="Extended">
        
        <!-- Load Number Column (Read-Only) -->
        <DataGrid.Columns>
            <DataGridTextColumn 
                Header="Load #" 
                Binding="{Binding LoadNumber}"
                IsReadOnly="True"
                Width="60" />
            
            <!-- Part ID Column (Read-Only) -->
            <DataGridTextColumn 
                Header="Part ID" 
                Binding="{Binding PartId}"
                IsReadOnly="True"
                Width="120" />
            
            <!-- Quantity Column (Editable) -->
            <DataGridTextColumn 
                Header="Quantity" 
                Binding="{Binding Quantity, Mode=TwoWay}"
                Width="100" />
            
            <!-- Heat/Lot Column (Editable) -->
            <DataGridTextColumn 
                Header="Heat/Lot" 
                Binding="{Binding HeatLot, Mode=TwoWay}"
                Width="150" />
            
            <!-- Package Type Column (ComboBox) -->
            <DataGridComboBoxColumn 
                Header="Package Type"
                SelectedItemBinding="{Binding PackageType, Mode=TwoWay}"
                ItemsSource="{x:Bind ViewModel.PackageTypes, Mode=OneWay}"
                DisplayMemberPath="TypeName"
                Width="120" />
            
            <!-- Packages Per Load Column (Editable) -->
            <DataGridTextColumn 
                Header="Packages/Load" 
                Binding="{Binding PackagesPerLoad, Mode=TwoWay}"
                Width="120" />
            
            <!-- Weight Per Package Column (Read-Only, Calculated) -->
            <DataGridTextColumn 
                Header="Weight/Package" 
                Binding="{Binding WeightPerPackage}"
                IsReadOnly="True"
                Width="120" />
            
            <!-- Location Column (ComboBox) -->
            <DataGridComboBoxColumn 
                Header="Location"
                SelectedItemBinding="{Binding ReceivingLocation, Mode=TwoWay}"
                ItemsSource="{x:Bind ViewModel.Locations, Mode=OneWay}"
                DisplayMemberPath="LocationName"
                Width="100" />
        </DataGrid.Columns>
    </DataGrid>
</UserControl>
```

**Acceptance Criteria:**
- [ ] DataGrid with all columns
- [ ] Editable cells
- [ ] ComboBox columns for PackageType, Location
- [ ] Read-only calculated columns
- [ ] Cell validation indicators
- [ ] Row selection support
- [ ] Auto-save on cell edit

---

### Tasks 6.21-6.34: Additional Step 2 Views

**Summary:**
- Task 6.21: View_Receiving_Wizard_Dialog_BulkCopyPreviewDialog - 3 hours
- Task 6.22: View_Receiving_Wizard_Dialog_CopyTypeSelectionDialog - 2 hours
- Task 6.23: View_Receiving_Wizard_Display_Step2Progress (UserControl) - 2 hours
- Task 6.24: View_Receiving_Wizard_Display_BulkOperationsToolbar (UserControl) - 2 hours
- Task 6.25: View_Receiving_Wizard_Dialog_Step2HelpDialog - 1.5 hours
- Task 6.26: View_Receiving_Wizard_Display_CellValidationIndicator (UserControl) - 1.5 hours
- Task 6.27-6.34: Additional Step 2 supporting views

**Total Step 2 Views Time:** ~25 hours

---

## ⏳ **Step 3 Views (12 tasks)**

### Task 6.35: View_Receiving_Wizard_Display_Step3Container

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_Step3Container.xaml`
- `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_Step3Container.xaml.cs`  
**Dependencies:** All Step 3 ViewModels  
**Estimated Time:** 4 hours

**UI Layout:**
- Container page for Step 3 components
- Summary header
- Review DataGrid (read-only)
- Totals panel
- Save button (prominent)

**Acceptance Criteria:**
- [ ] Contains all Step 3 UserControls
- [ ] Save button prominent
- [ ] Read-only data display

---

### Task 6.36: View_Receiving_Wizard_Display_ReviewSummary (UserControl)

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_ReviewSummary.xaml`
- `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_ReviewSummary.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Display_ReviewSummary  
**Estimated Time:** 4 hours

**UI Layout:**
- Read-only DataGrid with all loads
- All columns visible
- No editing allowed
- Summary totals at bottom

**XAML Structure:**
```xaml
<UserControl x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step3.View_Receiving_Wizard_Display_ReviewSummary">
    <StackPanel Spacing="15">
        <!-- Summary Header -->
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            
            <StackPanel Grid.Column="0">
                <TextBlock Text="Purchase Order:" FontWeight="SemiBold" />
                <TextBlock Text="{x:Bind ViewModel.PONumberDisplay, Mode=OneWay}" />
            </StackPanel>
            
            <StackPanel Grid.Column="1">
                <TextBlock Text="Part Number:" FontWeight="SemiBold" />
                <TextBlock Text="{x:Bind ViewModel.PartNumberDisplay, Mode=OneWay}" />
            </StackPanel>
        </Grid>
        
        <!-- Read-Only DataGrid -->
        <DataGrid 
            ItemsSource="{x:Bind ViewModel.LoadRows, Mode=OneWay}"
            AutoGenerateColumns="False"
            IsReadOnly="True"
            Height="400">
            
            <DataGrid.Columns>
                <DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" Width="60" />
                <DataGridTextColumn Header="Quantity" Binding="{Binding Quantity}" Width="100" />
                <DataGridTextColumn Header="Heat/Lot" Binding="{Binding HeatLot}" Width="150" />
                <DataGridTextColumn Header="Package Type" Binding="{Binding PackageType}" Width="120" />
                <DataGridTextColumn Header="Packages" Binding="{Binding PackagesPerLoad}" Width="100" />
                <DataGridTextColumn Header="Weight/Pkg" Binding="{Binding WeightPerPackage}" Width="120" />
                <DataGridTextColumn Header="Location" Binding="{Binding ReceivingLocation}" Width="100" />
            </DataGrid.Columns>
        </DataGrid>
        
        <!-- Totals Panel -->
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            
            <TextBlock Grid.Column="0">
                <Run Text="Total Weight: " FontWeight="SemiBold" />
                <Run Text="{x:Bind ViewModel.TotalWeight, Mode=OneWay}" />
                <Run Text=" LBS" />
            </TextBlock>
            
            <TextBlock Grid.Column="1">
                <Run Text="Total Packages: " FontWeight="SemiBold" />
                <Run Text="{x:Bind ViewModel.TotalPackages, Mode=OneWay}" />
            </TextBlock>
        </Grid>
    </StackPanel>
</UserControl>
```

**Acceptance Criteria:**
- [ ] Read-only DataGrid
- [ ] All load data visible
- [ ] Totals calculated and displayed
- [ ] Step 1 summary shown

---

### Task 6.37: View_Receiving_Wizard_Display_CompletionScreen

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_CompletionScreen.xaml`
- `Module_Receiving/Views/Wizard/Step3/View_Receiving_Wizard_Display_CompletionScreen.xaml.cs`  
**Dependencies:** ViewModel_Receiving_Wizard_Display_CompletionScreen  
**Estimated Time:** 3 hours

**UI Layout:**
- Success icon/animation
- Transaction ID display
- CSV file paths with copy buttons
- Action buttons (New Transaction, View History, Exit)

**XAML Structure:**
```xaml
<Page x:Class="MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step3.View_Receiving_Wizard_Display_CompletionScreen">
    <Grid Padding="40">
        <StackPanel Spacing="30" HorizontalAlignment="Center" VerticalAlignment="Center" Width="600">
            <!-- Success Icon -->
            <FontIcon 
                Glyph="&#xE73E;" 
                FontSize="80"
                Foreground="Green"
                HorizontalAlignment="Center" />
            
            <!-- Success Message -->
            <TextBlock 
                Text="Transaction Saved Successfully!" 
                Style="{StaticResource TitleTextBlockStyle}"
                HorizontalAlignment="Center" />
            
            <!-- Transaction Details -->
            <StackPanel Spacing="10">
                <TextBlock>
                    <Run Text="Transaction ID: " FontWeight="SemiBold" />
                    <Run Text="{x:Bind ViewModel.TransactionId, Mode=OneWay}" />
                </TextBlock>
                
                <TextBlock>
                    <Run Text="Completed: " FontWeight="SemiBold" />
                    <Run Text="{x:Bind ViewModel.CompletionTimestamp, Mode=OneWay}" />
                </TextBlock>
            </StackPanel>
            
            <!-- CSV File Paths -->
            <StackPanel Spacing="10">
                <TextBlock Text="CSV Files Exported:" FontWeight="SemiBold" />
                
                <StackPanel Orientation="Horizontal" Spacing="10">
                    <TextBlock Text="{x:Bind ViewModel.CSVFilePathLocal, Mode=OneWay}" TextWrapping="Wrap" Width="450" />
                    <Button Content="Copy" Command="{x:Bind ViewModel.CopyLocalPathCommand}" />
                </StackPanel>
                
                <StackPanel Orientation="Horizontal" Spacing="10">
                    <TextBlock Text="{x:Bind ViewModel.CSVFilePathNetwork, Mode=OneWay}" TextWrapping="Wrap" Width="450" />
                    <Button Content="Copy" Command="{x:Bind ViewModel.CopyNetworkPathCommand}" />
                </StackPanel>
            </StackPanel>
            
            <!-- Action Buttons -->
            <StackPanel Orientation="Horizontal" Spacing="15" HorizontalAlignment="Center" Margin="0,20,0,0">
                <Button 
                    Content="New Transaction" 
                    Command="{x:Bind ViewModel.StartNewTransactionCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    Width="180" />
                
                <Button 
                    Content="View History" 
                    Command="{x:Bind ViewModel.ViewTransactionHistoryCommand}"
                    Width="180" />
                
                <Button 
                    Content="Exit" 
                    Command="{x:Bind ViewModel.ExitWorkflowCommand}"
                    Width="180" />
            </StackPanel>
        </StackPanel>
    </Grid>
</Page>
```

**Acceptance Criteria:**
- [ ] Success icon/animation
- [ ] Transaction ID displayed
- [ ] CSV paths with copy buttons
- [ ] Action buttons (New, History, Exit)
- [ ] Centered layout

---

### Tasks 6.38-6.50: Additional Step 3 Views

**Summary:**
- Task 6.38: View_Receiving_Wizard_Display_ReviewDataGrid (UserControl) - 3 hours
- Task 6.39: View_Receiving_Wizard_Display_ReviewTotalsPanel (UserControl) - 2 hours
- Task 6.40: View_Receiving_Wizard_Display_SaveProgressDialog - 2 hours
- Task 6.41: View_Receiving_Wizard_Dialog_SaveErrorDialog - 2 hours
- Task 6.42: View_Receiving_Wizard_Dialog_Step3HelpDialog - 1 hour
- Task 6.43-6.50: Additional Step 3 supporting views

**Total Step 3 Views Time:** ~20 hours

---

## ⏳ **Shared Controls (8 tasks)**

### Task 6.51: SharedControl_Receiving_Display_ValidationMessage (UserControl)

**Priority:** P1 - HIGH  
**Files:**
- `Module_Receiving/Views/Shared/SharedControl_Receiving_Display_ValidationMessage.xaml`
- `Module_Receiving/Views/Shared/SharedControl_Receiving_Display_ValidationMessage.xaml.cs`  
**Estimated Time:** 1.5 hours

**UI Layout:**
- Reusable validation message component
- Icon + message text
- Configurable severity (Error, Warning, Info)

**Acceptance Criteria:**
- [ ] UserControl format
- [ ] Severity-based styling
- [ ] Icon + message layout

---

### Tasks 6.52-6.58: Additional Shared Controls

**Summary:**
- Task 6.52: SharedControl_Receiving_Display_LoadingIndicator - 1 hour
- Task 6.53: SharedControl_Receiving_Display_ProgressBar - 1 hour
- Task 6.54: SharedControl_Receiving_Display_ErrorPanel - 1.5 hours
- Task 6.55: SharedControl_Receiving_Display_EmptyStateMessage - 1 hour
- Task 6.56: SharedControl_Receiving_Button_PrimaryAction - 1 hour
- Task 6.57: SharedControl_Receiving_Button_SecondaryAction - 1 hour
- Task 6.58: SharedControl_Receiving_Display_TooltipHelper - 1 hour

**Total Shared Controls Time:** ~9 hours

---

## ⏳ **Dialog Views (8 tasks)**

### Task 6.59: View_Receiving_Dialog_ConfirmationDialog

**Priority:** P1 - HIGH  
**Files:**
- `Module_Receiving/Views/Dialogs/View_Receiving_Dialog_ConfirmationDialog.xaml`
- `Module_Receiving/Views/Dialogs/View_Receiving_Dialog_ConfirmationDialog.xaml.cs`  
**Estimated Time:** 2 hours

**UI Layout:**
- ContentDialog with message
- Confirm/Cancel buttons
- Optional warning icon

**XAML Structure:**
```xaml
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Receiving.Views.Dialogs.View_Receiving_Dialog_ConfirmationDialog"
    Title="{x:Bind ViewModel.DialogTitle, Mode=OneWay}"
    PrimaryButtonText="Confirm"
    SecondaryButtonText="Cancel"
    DefaultButton="Secondary">
    
    <StackPanel Spacing="15">
        <FontIcon 
            Glyph="&#xE7BA;" 
            FontSize="48"
            Foreground="{ThemeResource SystemFillColorCautionBrush}"
            HorizontalAlignment="Center"
            Visibility="{x:Bind ViewModel.ShowWarningIcon, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}" />
        
        <TextBlock 
            Text="{x:Bind ViewModel.Message, Mode=OneWay}"
            TextWrapping="Wrap"
            HorizontalAlignment="Center" />
        
        <TextBlock 
            Text="{x:Bind ViewModel.DetailMessage, Mode=OneWay}"
            TextWrapping="Wrap"
            FontSize="12"
            Foreground="{ThemeResource TextFillColorSecondaryBrush}"
            Visibility="{x:Bind ViewModel.DetailMessage, Mode=OneWay, Converter={StaticResource StringToVisibilityConverter}}" />
    </StackPanel>
</ContentDialog>
```

**Acceptance Criteria:**
- [ ] ContentDialog format
- [ ] Configurable title/message
- [ ] Warning icon optional
- [ ] Confirm/Cancel buttons

---

### Tasks 6.60-6.62: Additional Dialog Views

**Summary:**
- Task 6.60: View_Receiving_Dialog_ErrorDialog - 2 hours
- Task 6.61: View_Receiving_Dialog_ProgressDialog - 2 hours
- Task 6.62: View_Receiving_Dialog_InputDialog - 2 hours

**Total Dialog Views Time:** ~8 hours

---

## 📊 **Phase 6 Summary**

**Total Tasks:** 62  
**Total Estimated Time:** 50-60 hours  

**Critical Path Dependencies:**
1. Hub Views (Tasks 6.1-6.4) - Entry point
2. Wizard Orchestration Views (Tasks 6.5-6.6) - Main workflow container
3. Step 1 Views (Tasks 6.7-6.18) - First workflow step
4. Step 2 Views (Tasks 6.19-6.34) - Data entry step (most complex)
5. Step 3 Views (Tasks 6.35-6.50) - Review and save
6. Shared Controls (Tasks 6.51-6.58) - Reusable components
7. Dialogs (Tasks 6.59-6.62) - Modal interactions

**Key Technical Requirements:**
- ✅ All XAML uses `x:Bind` (compile-time binding)
- ✅ All `x:Bind` specifies explicit `Mode` (OneWay, TwoWay, OneTime)
- ✅ All DataTemplates specify `x:DataType`
- ✅ No business logic in .xaml.cs code-behind
- ✅ Window sizing via WindowHelper_WindowSizeAndStartupLocation
- ✅ Resource dictionaries for shared styles
- ✅ Accessibility support (AutomationProperties)

**Common Controls Used:**
- TextBox, ComboBox, NumberBox, CheckBox, ToggleSwitch
- Button, MenuFlyoutItem
- DataGrid, ListView, ItemsControl
- StackPanel, Grid, Border
- ContentDialog, Flyout, TeachingTip
- ProgressRing, ProgressBar
- FontIcon, SymbolIcon

**Styling Resources:**
- `TitleTextBlockStyle`
- `SubtitleTextBlockStyle`
- `AccentButtonStyle`
- `CardStrokeColorDefaultBrush`
- Custom converters (BoolToVisibility, InverseBool, etc.)

---

**Status:** ⏳ PENDING - Awaiting Phase 5 completion
