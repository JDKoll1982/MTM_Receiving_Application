# SQL Server Network Deployment Guide

**Project:** MTM Receiving Application  
**Database:** SQL Server (Module_Receiving)  
**Target:** .NET 8  
**Created:** 2026-01-25

---

## 📋 Overview

This guide explains how to deploy the SQL Server database to a network server for multi-user access, similar to your current MySQL/MAMP setup but using SQL Server.

### Deployment Options

| Option | Cost | Database Size Limit | Best For |
|--------|------|---------------------|----------|
| **SQL Server Express** | Free | 10GB | Small teams (5-10 users) |
| **SQL Server Standard** | Licensed | Unlimited | Medium teams (10-50 users) |
| **SQL Server Enterprise** | Licensed | Unlimited | Large deployments, HA/DR |

**Recommended:** SQL Server Express for initial deployment

---

## 🔧 Step 1: Install SQL Server on Network Server

### Download SQL Server Express

1. **Download:** https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. **Select:** SQL Server Express with Advanced Services
3. **Features Include:**
   - Database Engine
   - Full-Text Search
   - SQL Server Management Tools (optional)

### Installation Configuration

#### Instance Configuration
- **Instance Type:** Named Instance
- **Instance Name:** `MTM_RECEIVING` (or use `MSSQLSERVER` for default)
- **Instance ID:** `MTM_RECEIVING`

#### Server Configuration
- **SQL Server Database Engine:**
  - Startup Type: **Automatic**
  - Service Account: **NT AUTHORITY\NETWORK SERVICE** (default)

#### Database Engine Configuration
- **Authentication Mode:** **Mixed Mode (SQL Server and Windows)**
  - Set **SA password** (strong password required!)
  - Example: `MyStr0ng!P@ssword2026`
- **Specify SQL Server Administrators:**
  - ✅ Add current Windows user
  - ✅ Add domain admin group (e.g., `DOMAIN\SQL_Admins`)

#### Data Directories (Optional)
- **Data root directory:** `C:\Program Files\Microsoft SQL Server\`
- **User database directory:** `D:\SQLData\` (if separate data drive)
- **Backup directory:** `D:\SQLBackups\`

---

## 🌐 Step 2: Enable Network Protocols

### Using SQL Server Configuration Manager

1. **Open SQL Server Configuration Manager**
   - Start Menu → Microsoft SQL Server → SQL Server Configuration Manager
   - Or run: `SQLServerManager16.msc`

2. **Navigate to:**
   ```
   SQL Server Network Configuration
   └── Protocols for [MTM_RECEIVING]  (or MSSQLSERVER)
   ```

3. **Enable Protocols:**
   - ✅ **TCP/IP** → Right-click → **Enable** (REQUIRED)
   - ✅ **Named Pipes** → Right-click → **Enable** (RECOMMENDED)
   - ℹ️ **Shared Memory** → Already enabled (local connections only)

4. **Configure TCP/IP:**
   - Right-click **TCP/IP** → **Properties**
   - **Protocol** tab:
     - Enabled: **Yes**
     - Listen All: **Yes**
   - **IP Addresses** tab:
     - Scroll to **IPAll** section:
       - **TCP Dynamic Ports:** (leave blank)
       - **TCP Port:** `1433` (standard port)

5. **Restart SQL Server Service:**
   ```
   SQL Server Services
   └── SQL Server (MTM_RECEIVING)
       └── Right-click → Restart
   ```

### Using PowerShell (Alternative)

```powershell
# Import SQL Server module
Import-Module SqlServer

# Enable TCP/IP protocol
$wmi = New-Object Microsoft.SqlServer.Management.Smo.Wmi.ManagedComputer
$uri = "ManagedComputer[@Name='SERVERNAME']/ServerInstance[@Name='MTM_RECEIVING']/ServerProtocol[@Name='Tcp']"
$tcp = $wmi.GetSmoObject($uri)
$tcp.IsEnabled = $true
$tcp.Alter()

# Set TCP port
$ipall = $tcp.IPAddresses['IPAll']
$ipall.IPAddressProperties['TcpPort'].Value = '1433'
$tcp.Alter()

# Restart SQL Server service
Restart-Service 'MSSQL$MTM_RECEIVING'
```

---

## 🛡️ Step 3: Configure Windows Firewall

### Option A: Firewall GUI

1. **Open Windows Firewall:**
   - Control Panel → System and Security → Windows Defender Firewall
   - Click "Advanced settings"

2. **Create Inbound Rule:**
   - **Inbound Rules** → **New Rule...**
   - Rule Type: **Port**
   - Protocol: **TCP**
   - Port: **1433** (or your custom port)
   - Action: **Allow the connection**
   - Profile: ✅ Domain, ✅ Private, ✅ Public (or restrict as needed)
   - Name: `SQL Server - MTM Receiving`

3. **Create SQL Browser Rule (for named instances):**
   - **New Rule...**
   - Protocol: **UDP**
   - Port: **1434**
   - Name: `SQL Browser - MTM Receiving`

### Option B: PowerShell Commands

```powershell
# Allow SQL Server port (TCP 1433)
New-NetFirewallRule -DisplayName "SQL Server - MTM Receiving" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 1433 `
    -Action Allow `
    -Profile Domain,Private

# Allow SQL Browser (UDP 1434) for named instances
New-NetFirewallRule -DisplayName "SQL Browser - MTM Receiving" `
    -Direction Inbound `
    -Protocol UDP `
    -LocalPort 1434 `
    -Action Allow `
    -Profile Domain,Private
```

### Test Firewall

```powershell
# From client machine, test connectivity:
Test-NetConnection -ComputerName SERVERNAME -Port 1433

# Should return:
# TcpTestSucceeded : True
```

---

## 💾 Step 4: Create Database and Deploy Schema

### Method 1: Using Visual Studio Database Project (Recommended)

1. **Open Database Project:**
   - `Module_Databases/Module_Receiving_Database/Module_Receiving_Database.sqlproj`

2. **Publish to Network Server:**
   - Right-click project → **Publish...**
   - **Edit** connection:
     - Server name: `SERVERNAME\MTM_RECEIVING`
     - Authentication: **Windows Authentication** (or SQL Server)
     - Database name: `MTM_Receiving`
     - Test Connection → ✅ Succeeded
   - Click **Publish**

3. **Run Post-Deployment Scripts:**
   - Scripts automatically run if configured in `.sqlproj`
   - Or manually run seed scripts (see next section)

### Method 2: Using SSMS (SQL Server Management Studio)

1. **Connect to Server:**
   - Open SSMS
   - Server name: `SERVERNAME\MTM_RECEIVING`
   - Authentication: **Windows Authentication**

2. **Create Database:**
   ```sql
   CREATE DATABASE [MTM_Receiving];
   GO
   
   USE [MTM_Receiving];
   GO
   ```

3. **Execute Migration Script:**
   - Open `Scripts/Migration/001_InitialSchema_SQLCMD.sql`
   - Enable **SQLCMD Mode**: Query menu → SQLCMD Mode
   - Execute (F5)

4. **Run Seed Scripts:**
   ```sql
   -- Execute in order:
   :r dbo\Scripts\Seed\SeedPartTypes.sql
   :r dbo\Scripts\Seed\SeedPackageTypes.sql
   :r dbo\Scripts\Seed\SeedDefaultSettings.sql
   ```

### Method 3: Using SQLCMD Command Line

```cmd
REM Create database
sqlcmd -S SERVERNAME\MTM_RECEIVING -E -Q "CREATE DATABASE [MTM_Receiving]"

REM Run migration (from project root)
sqlcmd -S SERVERNAME\MTM_RECEIVING -E -d MTM_Receiving -i "Module_Databases\Module_Receiving_Database\Scripts\Migration\001_InitialSchema_SQLCMD.sql"

REM Run seed scripts
sqlcmd -S SERVERNAME\MTM_RECEIVING -E -d MTM_Receiving -i "Module_Databases\Module_Receiving_Database\dbo\Scripts\Seed\SeedPartTypes.sql"
sqlcmd -S SERVERNAME\MTM_RECEIVING -E -d MTM_Receiving -i "Module_Databases\Module_Receiving_Database\dbo\Scripts\Seed\SeedPackageTypes.sql"
sqlcmd -S SERVERNAME\MTM_RECEIVING -E -d MTM_Receiving -i "Module_Databases\Module_Receiving_Database\dbo\Scripts\Seed\SeedDefaultSettings.sql"
```

### Verify Deployment

```sql
-- Check tables created
SELECT name FROM sys.tables WHERE name LIKE 'tbl_Receiving_%' ORDER BY name;
-- Should return 10 tables

-- Check seed data
SELECT COUNT(*) AS PartTypes FROM tbl_Receiving_PartType;           -- Should be 4
SELECT COUNT(*) AS PackageTypes FROM tbl_Receiving_PackageType;     -- Should be 6
SELECT COUNT(*) AS Settings FROM tbl_Receiving_Settings;            -- Should be 6
```

---

## 🔐 Step 5: Configure Authentication & Security

### Option A: Windows Authentication (Recommended)

**Pros:**
- ✅ No passwords in connection strings
- ✅ Uses Active Directory credentials
- ✅ Centralized security management
- ✅ More secure (Kerberos authentication)

**Setup:**

```sql
USE [MTM_Receiving];
GO

-- Grant access to individual user
CREATE LOGIN [DOMAIN\jkollman] FROM WINDOWS;
CREATE USER [DOMAIN\jkollman] FOR LOGIN [DOMAIN\jkollman];

-- Grant access to Active Directory group (RECOMMENDED)
CREATE LOGIN [DOMAIN\Receiving_Users] FROM WINDOWS;
CREATE USER [DOMAIN\Receiving_Users] FOR LOGIN [DOMAIN\Receiving_Users];

-- Grant permissions to group
ALTER ROLE db_datareader ADD MEMBER [DOMAIN\Receiving_Users];
ALTER ROLE db_datawriter ADD MEMBER [DOMAIN\Receiving_Users];
ALTER ROLE db_executor ADD MEMBER [DOMAIN\Receiving_Users];
```

**Connection String:**
```csharp
"Server=SERVERNAME\\MTM_RECEIVING;Database=MTM_Receiving;Integrated Security=true;TrustServerCertificate=true;"
```

### Option B: SQL Server Authentication

**Pros:**
- ✅ Works across domains/workgroups
- ✅ Easier for testing

**Cons:**
- ❌ Password must be stored/encrypted
- ❌ Less secure

**Setup:**

```sql
USE [master];
GO

-- Create SQL login
CREATE LOGIN [MTM_App_User] WITH PASSWORD = 'StrongP@ssw0rd123!';
GO

USE [MTM_Receiving];
GO

-- Create database user
CREATE USER [MTM_App_User] FOR LOGIN [MTM_App_User];

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER [MTM_App_User];
ALTER ROLE db_datawriter ADD MEMBER [MTM_App_User];
ALTER ROLE db_executor ADD MEMBER [MTM_App_User];
```

**Connection String:**
```csharp
"Server=SERVERNAME\\MTM_RECEIVING;Database=MTM_Receiving;User ID=MTM_App_User;Password=StrongP@ssw0rd123!;TrustServerCertificate=true;"
```

### Create Application-Specific Role (Best Practice)

```sql
USE [MTM_Receiving];
GO

-- Create custom role
CREATE ROLE [MTM_Receiving_App];

-- Grant only required permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [MTM_Receiving_App];
GRANT EXECUTE ON SCHEMA::dbo TO [MTM_Receiving_App];

-- Add users/logins to role
ALTER ROLE [MTM_Receiving_App] ADD MEMBER [DOMAIN\Receiving_Users];
-- Or for SQL Auth:
ALTER ROLE [MTM_Receiving_App] ADD MEMBER [MTM_App_User];
```

---

## ⚙️ Step 6: Update Application Configuration

### Current Configuration (LocalDB)

**File:** `Module_Shared/Helpers/Helper_Database_Variables.cs`

```csharp
public static class Helper_Database_Variables
{
    public static string GetConnectionString()
    {
        return "Server=(localdb)\\MSSQLLocalDB;Database=MTM_Receiving;Integrated Security=true;";
    }
}
```

### Updated Configuration (Network Server)

#### Option 1: Hardcoded (Simple)

```csharp
public static class Helper_Database_Variables
{
    public static string GetConnectionString()
    {
        // PRODUCTION: Network SQL Server
        string server = "SQLSERVER01\\MTM_RECEIVING";  // Replace with your server
        string database = "MTM_Receiving";
        
        return $"Server={server};Database={database};Integrated Security=true;TrustServerCertificate=true;";
    }
}
```

#### Option 2: Configuration File (Recommended)

**App.config:**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <connectionStrings>
    <!-- LocalDB (Development) -->
    <add name="MTM_Receiving_Dev" 
         connectionString="Server=(localdb)\MSSQLLocalDB;Database=MTM_Receiving;Integrated Security=true;" 
         providerName="System.Data.SqlClient" />
    
    <!-- Network Server (Production) -->
    <add name="MTM_Receiving" 
         connectionString="Server=SQLSERVER01\MTM_RECEIVING;Database=MTM_Receiving;Integrated Security=true;TrustServerCertificate=true;" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

**Helper_Database_Variables.cs:**
```csharp
using System.Configuration;

public static class Helper_Database_Variables
{
    public static string GetConnectionString()
    {
        // Read from App.config
        return ConfigurationManager.ConnectionStrings["MTM_Receiving"].ConnectionString;
    }
}
```

#### Option 3: Environment-Based (Advanced)

```csharp
public static class Helper_Database_Variables
{
    public static string GetConnectionString()
    {
        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        
        return environment switch
        {
            "Development" => "Server=(localdb)\\MSSQLLocalDB;Database=MTM_Receiving;Integrated Security=true;",
            "Production" => "Server=SQLSERVER01\\MTM_RECEIVING;Database=MTM_Receiving;Integrated Security=true;TrustServerCertificate=true;",
            _ => throw new InvalidOperationException($"Unknown environment: {environment}")
        };
    }
}
```

---

## 📊 Comparison: MySQL vs SQL Server

| Aspect | MySQL (Current - MAMP) | SQL Server (New) |
|--------|----------------------|------------------|
| **Server Software** | MySQL Server / MAMP | SQL Server Express/Standard |
| **Default Port** | 3306 | 1433 |
| **Authentication** | MySQL username/password | Windows Auth or SQL Auth |
| **Management Tool** | MySQL Workbench, phpMyAdmin | SSMS, Azure Data Studio |
| **Connection String** | `Server=localhost;Database=db;User=root;Password=...` | `Server=SERVER;Database=db;Integrated Security=true;` |
| **Protocol Setup** | TCP/IP enabled by default | Must manually enable TCP/IP |
| **Firewall** | Allow TCP 3306 | Allow TCP 1433 + UDP 1434 |
| **Stored Procedures** | MySQL syntax | T-SQL syntax |
| **Network Discovery** | Direct IP/hostname | SQL Browser service (UDP 1434) |

---

## 🧪 Testing Network Connectivity

### From Client Machine

#### Test 1: Port Connectivity
```powershell
# Test SQL Server port
Test-NetConnection -ComputerName SQLSERVER01 -Port 1433

# Expected output:
# TcpTestSucceeded : True
```

#### Test 2: SQL Server Connection
```powershell
# Using SQLCMD
sqlcmd -S SQLSERVER01\MTM_RECEIVING -E -Q "SELECT @@VERSION"

# Should return SQL Server version info
```

#### Test 3: Application Connection
```csharp
// Quick test in C#
using System.Data.SqlClient;

string connString = "Server=SQLSERVER01\\MTM_RECEIVING;Database=MTM_Receiving;Integrated Security=true;";
using (SqlConnection conn = new SqlConnection(connString))
{
    conn.Open();
    Console.WriteLine("✓ Connection successful!");
    Console.WriteLine($"Database: {conn.Database}");
    Console.WriteLine($"Server: {conn.DataSource}");
}
```

### From SQL Server (Test Outbound)

```sql
-- Check if SQL Browser is running
EXEC xp_servicecontrol 'QueryState', 'SQLBrowser'
-- Should return: Running.

-- Check network configuration
EXEC sp_configure 'remote access'
-- Should show: run_value = 1

-- List active connections
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    client_interface_name
FROM sys.dm_exec_sessions
WHERE is_user_process = 1;
```

---

## 🚀 Deployment Checklist

### Server Setup
- [ ] SQL Server Express/Standard installed on network server
- [ ] Named instance configured: `MTM_RECEIVING`
- [ ] Mixed Mode authentication enabled
- [ ] SA password set and documented
- [ ] SQL Server service set to Automatic startup
- [ ] TCP/IP protocol enabled
- [ ] TCP port configured (1433 or custom)
- [ ] SQL Browser service running (for named instances)
- [ ] Windows Firewall rules created (TCP 1433, UDP 1434)
- [ ] Server restarted after configuration

### Database Deployment
- [ ] Database `MTM_Receiving` created
- [ ] Schema deployed (10 tables, indexes, constraints)
- [ ] Seed data loaded (PartTypes, PackageTypes, Settings)
- [ ] Stored procedures deployed (29 procedures)
- [ ] Views created (if applicable)
- [ ] Functions created (if applicable)
- [ ] Verification queries successful

### Security Configuration
- [ ] Windows logins created for users/groups
- [ ] Database users created
- [ ] Permissions granted (db_datareader, db_datawriter, db_executor)
- [ ] Custom role created: `MTM_Receiving_App`
- [ ] Test user connections from client machines

### Application Configuration
- [ ] Connection string updated in `Helper_Database_Variables.cs`
- [ ] App.config created with connection string (if using)
- [ ] Development vs Production configurations
- [ ] Connection string tested from development machine
- [ ] All DAOs tested with network connection
- [ ] Error handling verified

### Client Deployment
- [ ] Application deployed to client machines
- [ ] .NET 8 Runtime installed on client machines
- [ ] Network connectivity verified
- [ ] All CRUD operations tested
- [ ] Performance tested with multiple users
- [ ] Error logging configured

### Backup & Maintenance
- [ ] Backup strategy defined
- [ ] Scheduled backups configured
- [ ] Backup location secured
- [ ] Recovery procedures documented
- [ ] Maintenance plan created

---

## 🔧 Troubleshooting

### Issue: Cannot Connect to Server

**Error:** `A network-related or instance-specific error occurred`

**Solutions:**
1. ✅ Verify SQL Server service is running
2. ✅ Check TCP/IP is enabled
3. ✅ Confirm firewall rules allow port 1433
4. ✅ Test port connectivity: `Test-NetConnection SERVER -Port 1433`
5. ✅ Try SQL Browser service: `net start SQLBrowser`

### Issue: Login Failed for User

**Error:** `Login failed for user 'DOMAIN\username'`

**Solutions:**
1. ✅ Verify login exists: `SELECT * FROM sys.server_principals WHERE name = 'DOMAIN\username'`
2. ✅ Check database user exists: `SELECT * FROM sys.database_principals`
3. ✅ Verify permissions: `EXEC sp_helprotec`
4. ✅ Check SQL Server authentication mode (Mixed Mode required for SQL Auth)

### Issue: Named Instance Not Found

**Error:** `Cannot connect to SERVERNAME\MTM_RECEIVING`

**Solutions:**
1. ✅ Ensure SQL Browser service is running
2. ✅ Check firewall allows UDP 1434
3. ✅ Try using explicit port: `SERVERNAME,1433` (comma, not backslash)
4. ✅ Verify instance name: `SELECT @@SERVERNAME`

### Issue: Timeout Expired

**Error:** `Timeout expired. The timeout period elapsed...`

**Solutions:**
1. ✅ Increase connection timeout in connection string:
   ```csharp
   "Server=...;Connection Timeout=30;" // Default is 15
   ```
2. ✅ Check network latency
3. ✅ Verify server not overloaded
4. ✅ Check for slow queries: `sp_who2`

---

## 📈 Performance Tuning

### SQL Server Configuration

```sql
-- Set max memory (prevent SQL Server from consuming all RAM)
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'max server memory (MB)', 4096; -- Adjust based on server
RECONFIGURE;

-- Enable backup compression (save disk space)
EXEC sp_configure 'backup compression default', 1;
RECONFIGURE;
```

### Index Maintenance

```sql
-- Rebuild fragmented indexes (run weekly)
EXEC sp_MSforeachtable 'ALTER INDEX ALL ON ? REBUILD';

-- Update statistics (run weekly)
EXEC sp_updatestats;
```

### Monitor Performance

```sql
-- View top 10 slowest queries
SELECT TOP 10
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time,
    qs.execution_count,
    SUBSTRING(qt.text, qs.statement_start_offset/2 + 1,
        (CASE WHEN qs.statement_end_offset = -1
            THEN LEN(CONVERT(NVARCHAR(MAX), qt.text)) * 2
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) qt
ORDER BY avg_elapsed_time DESC;
```

---

## 💾 Backup Strategy

### Full Backup (Recommended Daily)

```sql
-- Create full backup
BACKUP DATABASE [MTM_Receiving]
TO DISK = 'D:\SQLBackups\MTM_Receiving_Full.bak'
WITH FORMAT, INIT,
NAME = 'MTM_Receiving-Full Database Backup',
COMPRESSION;
```

### Automated Backup Script

```powershell
# PowerShell script for scheduled backup
$server = "SQLSERVER01\MTM_RECEIVING"
$database = "MTM_Receiving"
$backupPath = "D:\SQLBackups"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "$backupPath\MTM_Receiving_Full_$timestamp.bak"

# Create backup
$query = @"
BACKUP DATABASE [$database]
TO DISK = '$backupFile'
WITH FORMAT, INIT,
NAME = 'MTM_Receiving-Full Database Backup',
COMPRESSION;
"@

Invoke-Sqlcmd -ServerInstance $server -Query $query

# Delete backups older than 7 days
Get-ChildItem $backupPath -Filter "MTM_Receiving_*.bak" | 
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-7)} | 
    Remove-Item
```

**Schedule with Task Scheduler:**
- Trigger: Daily at 2:00 AM
- Action: `PowerShell.exe -File "C:\Scripts\Backup-MTMReceiving.ps1"`

---

## 📚 Additional Resources

### Documentation
- **SQL Server Documentation:** https://learn.microsoft.com/en-us/sql/
- **Connection Strings:** https://www.connectionstrings.com/sql-server/
- **Security Best Practices:** https://learn.microsoft.com/en-us/sql/relational-databases/security/

### Tools
- **SQL Server Management Studio (SSMS):** https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
- **Azure Data Studio:** https://learn.microsoft.com/en-us/azure-data-studio/download-azure-data-studio
- **SQL Server Configuration Manager:** Included with SQL Server installation

### Support
- **GitHub Repository:** https://github.com/JDKoll1982/MTM_Receiving_Application
- **Project Documentation:** `docs/` folder
- **Database Schema:** `Module_Databases/Module_Receiving_Database/README.md`

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Next Review:** After initial deployment
