# PO Line Specs Search - GUI Version
# This script searches Infor Visual ERP for PO lines containing specific text in specification fields
#
# Features:
# - Search across ALL POs (leave PO Number blank) or filter to specific PO
# - Filter by PO Status: Firmed (F), Released (R), Closed (C), Cancelled (X)
#   * None selected = All statuses (no filter)
#   * Select one or more to filter results
# - Case-insensitive wildcard search in specification text
# - Read-only connection to Infor Visual database
# - Auto-formats PO numbers with "PO-" prefix
#
# Usage:
# 1. Enter database credentials (Server, Database, Username, Password)
# 2. Enter search text (required) - e.g., "MMFSR", "quality hold", "sheet"
# 3. Optionally enter specific PO Number to narrow search
# 4. Select PO Status filters (default = all checked = all statuses)
# 5. Click Search to find matching PO lines

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Data

# XAML for the search window
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="PO Line Specs Search - Infor Visual ERP" 
        Height="700" Width="1200"
        WindowStartupLocation="CenterScreen"
        Background="#F5F5F5">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Margin="0,0,0,20">
            <TextBlock Text="PO Line Specifications Search" 
                       FontSize="24" 
                       FontWeight="Bold" 
                       Foreground="#2196F3"/>
            <TextBlock Text="Search Infor Visual ERP for PO lines containing specific text" 
                       FontSize="14" 
                       Foreground="#666"/>
        </StackPanel>

        <!-- Search Parameters -->
        <Border Grid.Row="1" 
                Background="White" 
                BorderBrush="#DDD" 
                BorderThickness="1" 
                CornerRadius="5" 
                Padding="15" 
                Margin="0,0,0,20">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="120"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="120"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Row 0: Server and Database -->
                <TextBlock Grid.Row="0" Grid.Column="0" Text="Server:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,5"/>
                <TextBox Grid.Row="0" Grid.Column="1" Name="ServerTextBox" Margin="10,5" Padding="5" Text="VISUAL"/>

                <TextBlock Grid.Row="0" Grid.Column="2" Text="Database:" FontWeight="Bold" VerticalAlignment="Center" Margin="20,5,0,5"/>
                <TextBox Grid.Row="0" Grid.Column="3" Name="DatabaseTextBox" Margin="10,5" Padding="5" Text="MTMFG"/>

                <!-- Row 1: Username and Password -->
                <TextBlock Grid.Row="1" Grid.Column="0" Text="Username:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,5"/>
                <TextBox Grid.Row="1" Grid.Column="1" Name="UsernameTextBox" Margin="10,5" Padding="5" Text="SHOP2"/>

                <TextBlock Grid.Row="1" Grid.Column="2" Text="Password:" FontWeight="Bold" VerticalAlignment="Center" Margin="20,5,0,5"/>
                <PasswordBox Grid.Row="1" Grid.Column="3" Name="PasswordBox" Margin="10,5" Padding="5"/>

                <!-- Row 2: PO Number and Search String -->
                <TextBlock Grid.Row="2" Grid.Column="0" Text="PO Number:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,5"/>
                <TextBox Grid.Row="2" Grid.Column="1" Name="PONumberTextBox" Margin="10,5" Padding="5">
                    <TextBox.ToolTip>
                        <ToolTip Content="Optional - Leave blank to search all POs"/>
                    </TextBox.ToolTip>
                </TextBox>

                <TextBlock Grid.Row="2" Grid.Column="2" Text="Search Text:" FontWeight="Bold" VerticalAlignment="Center" Margin="20,5,0,5"/>
                <TextBox Grid.Row="2" Grid.Column="3" Name="SearchTextBox" Margin="10,5" Padding="5"/>

                <!-- Row 3: PO Status Filters -->
                <TextBlock Grid.Row="3" Grid.Column="0" Text="PO Status:" FontWeight="Bold" VerticalAlignment="Center" Margin="0,5"/>
                <StackPanel Grid.Row="3" Grid.Column="1" Grid.ColumnSpan="3" Orientation="Horizontal" Margin="10,5">
                    <CheckBox Name="StatusFirmedCheckBox" Content="Firmed" Margin="0,0,15,0" IsChecked="True">
                        <CheckBox.ToolTip>
                            <ToolTip Content="Status = F"/>
                        </CheckBox.ToolTip>
                    </CheckBox>
                    <CheckBox Name="StatusReleasedCheckBox" Content="Released" Margin="0,0,15,0" IsChecked="True">
                        <CheckBox.ToolTip>
                            <ToolTip Content="Status = R"/>
                        </CheckBox.ToolTip>
                    </CheckBox>
                    <CheckBox Name="StatusClosedCheckBox" Content="Closed" Margin="0,0,15,0" IsChecked="True">
                        <CheckBox.ToolTip>
                            <ToolTip Content="Status = C"/>
                        </CheckBox.ToolTip>
                    </CheckBox>
                    <CheckBox Name="StatusCancelledCheckBox" Content="Cancelled" Margin="0,0,15,0" IsChecked="True">
                        <CheckBox.ToolTip>
                            <ToolTip Content="Status = X"/>
                        </CheckBox.ToolTip>
                    </CheckBox>
                    <TextBlock Text="(None selected = All)" Foreground="#999" VerticalAlignment="Center" Margin="10,0,0,0" FontStyle="Italic"/>
                </StackPanel>

                <!-- Row 4: Search Button -->
                <Button Grid.Row="4" Grid.Column="0" Grid.ColumnSpan="4" 
                        Name="SearchButton" 
                        Content="Search" 
                        Width="120" 
                        Height="35" 
                        Margin="0,10,0,0"
                        HorizontalAlignment="Right"
                        Background="#2196F3" 
                        Foreground="White" 
                        BorderThickness="0" 
                        FontWeight="Bold"
                        Cursor="Hand"/>
            </Grid>
        </Border>

        <!-- Results Section -->
        <Border Grid.Row="2" 
                Background="White" 
                BorderBrush="#DDD" 
                BorderThickness="1" 
                CornerRadius="5" 
                Padding="10">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>

                <!-- Status Bar -->
                <Grid Grid.Row="0" Margin="5,5,5,10">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Grid.Column="0" Name="StatusText" Text="Enter search criteria above and click Search" Foreground="#666"/>
                    <ProgressBar Grid.Column="1" Name="ProgressBar" Width="100" Height="20" IsIndeterminate="True" Visibility="Collapsed"/>
                </Grid>

                <!-- DataGrid -->
                <DataGrid Grid.Row="1" 
                          Name="ResultsGrid" 
                          AutoGenerateColumns="False"
                          IsReadOnly="True"
                          AlternatingRowBackground="#F9F9F9"
                          GridLinesVisibility="Horizontal"
                          HeadersVisibility="Column"
                          CanUserResizeRows="False"
                          SelectionMode="Single">
                    <DataGrid.Columns>
                        <DataGridTextColumn Header="PO Number" Binding="{Binding PO_Number}" Width="100"/>
                        <DataGridTextColumn Header="Line" Binding="{Binding Line_Number}" Width="50"/>
                        <DataGridTextColumn Header="Part Number" Binding="{Binding Part_Number}" Width="120"/>
                        <DataGridTextColumn Header="Vendor Part" Binding="{Binding Vendor_Part_Number}" Width="120"/>
                        <DataGridTextColumn Header="Qty" Binding="{Binding Ordered_Qty}" Width="60"/>
                        <DataGridTextColumn Header="UM" Binding="{Binding Unit_Of_Measure}" Width="50"/>
                        <DataGridTextColumn Header="Recv Qty" Binding="{Binding Received_Qty}" Width="70"/>
                        <DataGridTextColumn Header="Status" Binding="{Binding Line_Status}" Width="50"/>
                        <DataGridTextColumn Header="Specification Text" Binding="{Binding Spec_Text}" Width="*" MinWidth="200">
                            <DataGridTextColumn.ElementStyle>
                                <Style TargetType="TextBlock">
                                    <Setter Property="TextWrapping" Value="Wrap"/>
                                    <Setter Property="Padding" Value="5"/>
                                </Style>
                            </DataGridTextColumn.ElementStyle>
                        </DataGridTextColumn>
                    </DataGrid.Columns>
                </DataGrid>
            </Grid>
        </Border>

        <!-- Error Display -->
        <Border Grid.Row="3" 
                Name="ErrorBorder"
                Background="#FFEBEE" 
                BorderBrush="#F44336" 
                BorderThickness="1" 
                CornerRadius="3" 
                Padding="10" 
                Margin="0,10,0,0"
                Visibility="Collapsed">
            <TextBlock Name="ErrorText" 
                       Foreground="#C62828" 
                       TextWrapping="Wrap"/>
        </Border>
    </Grid>
</Window>
"@

# Load XAML
$reader = [System.Xml.XmlNodeReader]::new([xml]$xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

# Get controls
$serverTextBox = $window.FindName("ServerTextBox")
$databaseTextBox = $window.FindName("DatabaseTextBox")
$usernameTextBox = $window.FindName("UsernameTextBox")
$passwordBox = $window.FindName("PasswordBox")
$poNumberTextBox = $window.FindName("PONumberTextBox")
$searchTextBox = $window.FindName("SearchTextBox")
$statusFirmedCheckBox = $window.FindName("StatusFirmedCheckBox")
$statusReleasedCheckBox = $window.FindName("StatusReleasedCheckBox")
$statusClosedCheckBox = $window.FindName("StatusClosedCheckBox")
$statusCancelledCheckBox = $window.FindName("StatusCancelledCheckBox")
$searchButton = $window.FindName("SearchButton")
$resultsGrid = $window.FindName("ResultsGrid")
$statusText = $window.FindName("StatusText")
$progressBar = $window.FindName("ProgressBar")
$errorBorder = $window.FindName("ErrorBorder")
$errorText = $window.FindName("ErrorText")

# Set default password
$passwordBox.Password = "SHOP"

# PO Number auto-formatting (like View_Receiving_POEntry.xaml)
$poNumberTextBox.Add_LostFocus({
        $text = $poNumberTextBox.Text.Trim().ToUpper()
    
        if ([string]::IsNullOrWhiteSpace($text)) {
            return
        }
    
        # Remove any existing "PO-" prefix
        if ($text.StartsWith("PO-")) {
            $text = $text.Substring(3)
        }
    
        # Add "PO-" prefix
        $poNumberTextBox.Text = "PO-$text"
    })

# Function to show error
function Show-Error {
    param([string]$Message)
    
    $errorText.Text = $Message
    $errorBorder.Visibility = "Visible"
    $statusText.Text = "Error occurred"
    $progressBar.Visibility = "Collapsed"
    $searchButton.IsEnabled = $true
}

# Function to hide error
function Hide-Error {
    $errorBorder.Visibility = "Collapsed"
}

# Search button click handler
$searchButton.Add_Click({
        try {
            Hide-Error
        
            # Validate inputs
            $server = $serverTextBox.Text.Trim()
            $database = $databaseTextBox.Text.Trim()
            $username = $usernameTextBox.Text.Trim()
            $password = $passwordBox.Password
            $poNumber = $poNumberTextBox.Text.Trim()
            $searchText = $searchTextBox.Text.Trim()
        
            if ([string]::IsNullOrWhiteSpace($server)) {
                Show-Error "Server name is required"
                return
            }
        
            if ([string]::IsNullOrWhiteSpace($database)) {
                Show-Error "Database name is required"
                return
            }
        
            if ([string]::IsNullOrWhiteSpace($username)) {
                Show-Error "Username is required"
                return
            }
        
            if ([string]::IsNullOrWhiteSpace($password)) {
                Show-Error "Password is required"
                return
            }
        
            if ([string]::IsNullOrWhiteSpace($searchText)) {
                Show-Error "Search text is required"
                return
            }
        
            # Disable search button and show progress
            $searchButton.IsEnabled = $false
            $progressBar.Visibility = "Visible"
            $statusText.Text = "Searching..."
            $resultsGrid.ItemsSource = $null
        
            # Build connection string
            $connectionString = "Server=$server;Database=$database;User Id=$username;Password=$password;ApplicationIntent=ReadOnly;TrustServerCertificate=True;Connection Timeout=30;"
        
            # SQL Query - PO Number is optional filter
            $sqlQuery = @"
SELECT 
    po.ID AS PO_Number,
    pol.LINE_NO AS Line_Number,
    pol.PART_ID AS Part_Number,
    pol.VENDOR_PART_ID AS Vendor_Part_Number,
    pol.USER_ORDER_QTY AS Ordered_Qty,
    pol.PURCHASE_UM AS Unit_Of_Measure,
    pol.TOTAL_RECEIVED_QTY AS Received_Qty,
    pol.LINE_STATUS AS Line_Status,
    pol.DESIRED_RECV_DATE AS Desired_Recv_Date,
    pol.LAST_RECEIVED_DATE AS Last_Recv_Date,
    CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), plb.BITS)) AS Spec_Text,
    po.VENDOR_ID AS Vendor_ID,
    po.STATUS AS PO_Status,
    po.ORDER_DATE AS PO_Date,
    po.SITE_ID AS Site_ID
FROM 
    dbo.PURC_ORDER_LINE pol
    INNER JOIN dbo.PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID
    INNER JOIN dbo.PURC_LINE_BINARY plb ON pol.PURC_ORDER_ID = plb.PURC_ORDER_ID 
        AND pol.LINE_NO = plb.PURC_ORDER_LINE_NO
        AND plb.TYPE = 'D'
WHERE 
    CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), plb.BITS)) LIKE @SearchPattern
"@

            # Build PO Status filter
            $selectedStatuses = @()
            if ($statusFirmedCheckBox.IsChecked) { $selectedStatuses += 'F' }
            if ($statusReleasedCheckBox.IsChecked) { $selectedStatuses += 'R' }
            if ($statusClosedCheckBox.IsChecked) { $selectedStatuses += 'C' }
            if ($statusCancelledCheckBox.IsChecked) { $selectedStatuses += 'X' }

            # If at least one status is selected, add filter
            # If none selected, treat as "all" (no filter)
            if ($selectedStatuses.Count -gt 0) {
                $statusList = ($selectedStatuses | ForEach-Object { "'$_'" }) -join ','
                $sqlQuery += "`n    AND po.STATUS IN ($statusList)"
            }

            # Add PO Number filter if provided
            if (-not [string]::IsNullOrWhiteSpace($poNumber)) {
                $sqlQuery += "`n    AND po.ID = @PONumber"
            }

            $sqlQuery += "`nORDER BY po.ID, pol.LINE_NO"
        
            # Execute query
            $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
            $connection.Open()
        
            $command = $connection.CreateCommand()
            $command.CommandText = $sqlQuery
            $command.CommandTimeout = 60
        
            # Add search pattern parameter
            $command.Parameters.AddWithValue("@SearchPattern", "%$searchText%") | Out-Null
            
            # Add PO Number parameter only if provided
            if (-not [string]::IsNullOrWhiteSpace($poNumber)) {
                $command.Parameters.AddWithValue("@PONumber", $poNumber) | Out-Null
            }
        
            $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
            $dataSet = New-Object System.Data.DataSet
        
            $rowCount = $adapter.Fill($dataSet)
        
            $connection.Close()
        
            # Display results
            if ($rowCount -gt 0) {
                $resultsGrid.ItemsSource = $dataSet.Tables[0].DefaultView
                
                # Build status description
                $poScope = if ([string]::IsNullOrWhiteSpace($poNumber)) { "all POs" } else { "PO $poNumber" }
                
                $statusNames = @()
                if ($statusFirmedCheckBox.IsChecked) { $statusNames += "Firmed" }
                if ($statusReleasedCheckBox.IsChecked) { $statusNames += "Released" }
                if ($statusClosedCheckBox.IsChecked) { $statusNames += "Closed" }
                if ($statusCancelledCheckBox.IsChecked) { $statusNames += "Cancelled" }
                
                if ($statusNames.Count -eq 0 -or $statusNames.Count -eq 4) {
                    $statusScope = "all statuses"
                }
                elseif ($statusNames.Count -eq 1) {
                    $statusScope = "$($statusNames[0]) status"
                }
                else {
                    $statusScope = "$($statusNames -join ', ') statuses"
                }
                
                $statusText.Text = "Found $rowCount matching line(s) in $poScope with $statusScope"
            }
            else {
                $resultsGrid.ItemsSource = $null
                $poScope = if ([string]::IsNullOrWhiteSpace($poNumber)) { "any PO" } else { "PO $poNumber" }
                $statusText.Text = "No results found in $poScope - try different search criteria"
            }
        
            $progressBar.Visibility = "Collapsed"
            $searchButton.IsEnabled = $true
        }
        catch {
            Show-Error "Error: $($_.Exception.Message)"
        
            if ($connection -and $connection.State -eq 'Open') {
                $connection.Close()
            }
        }
    })

# Show the window
$window.ShowDialog() | Out-Null
