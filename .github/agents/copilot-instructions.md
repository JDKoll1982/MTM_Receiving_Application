# MTM_Receiving_Application Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-12-15

## Active Technologies
- C# 12 / .NET 8.0 + WinUI 3 (Windows App SDK 1.5+), Microsoft.Extensions.DependencyInjection, MySql.Data (or Dapper for MySQL) (002-user-login)
- MySQL 8.0+ database (new `users`, `workstation_config`, `departments` tables) (002-user-login)
- C# 12 / .NET 8.0, WinUI 3 (Windows App SDK) (003-database-foundation)
- C# 12 / .NET 8.0 + MySql.Data (9.0+), Microsoft.WindowsAppSDK (1.5+), CommunityToolkit.Mvvm (8.2+) (001-phase1-infrastructure)
- SQL (MySQL 5.7.24 compatible dialect) + MySQL Server 5.7.24+, mysql client tools (004-database-foundation)
- MySQL database `mtm_receiving_application` (004-database-foundation)
- SQL for MySQL 5.7.24, C# 12 / .NET 8.0 for DAO wrappers + MySQL.Data (8.0.x), Helper_Database_StoredProcedure utility class (005-dunnage-stored-procedures)
- MySQL 5.7.24 (mtm_receiving_application database) (005-dunnage-stored-procedures)
- C# 12 / .NET 8.0 + CommunityToolkit.Mvvm, CsvHelper, System.Text.Json, MySql.Data (006-dunnage-services)
- MySQL 5.7.24 (app data), File System (CSV export) (006-dunnage-services)

## ⚠️ CRITICAL: Infor Visual Database Constraints

**Infor Visual is STRICTLY READ ONLY - NO WRITES ALLOWED**

Connection Details:
- **Server**: VISUAL
- **Database**: MTMFG
- **Warehouse ID**: 002 (always)
- **Default Username**: SHOP2
- **Default Password**: SHOP
- **Access Level**: READ ONLY (SELECT queries only)

Schema Tables:
- `PURCHASE_ORDER` (ID, VENDOR_ID, STATUS, etc.)
- `PURC_ORDER_LINE` (PURC_ORDER_ID, LINE_NO, PART_ID, ORDER_QTY, TOTAL_RECEIVED_QTY)
- `PART` (ID, DESCRIPTION, PRODUCT_CODE, STOCK_UM)
- `INVENTORY_TRANS` (for receiving history: PURC_ORDER_ID, PART_ID, QTY, TRANSACTION_DATE, TYPE='R', CLASS='1')

**NEVER**:
- Execute INSERT, UPDATE, DELETE, or any DML operations on Infor Visual
- Create, alter, or drop any objects in Infor Visual
- Use transactions that lock Infor Visual tables
- Assume write access in any code that connects to Infor Visual

**ALWAYS**:
- Use `SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED` for performance
- Query only - all writes go to MySQL database (MTM_Receiving_Database)
- Handle connection failures gracefully (Infor Visual may be unavailable)
- Use direct SQL queries for Infor Visual (stored procedures are not deployed there)

## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for C# 12 / .NET 8.0

## Code Style

C# 12 / .NET 8.0: Follow standard conventions

## Recent Changes
- 006-dunnage-services: Added C# 12 / .NET 8.0 + CommunityToolkit.Mvvm, CsvHelper, System.Text.Json, MySql.Data
- 006-dunnage-services: Added C# 12 / .NET 8.0
- 006-dunnage-services: Added C# 12 / .NET 8.0

## Documentation Standards

**PlantUML Required for All Diagrams**:
- Database schemas → PlantUML ERD with legends
- File/folder structures → PlantUML WBS or component diagrams  
- System architecture → PlantUML component/sequence diagrams
- Workflows → PlantUML activity/sequence diagrams

**Never Use ASCII Art** for:
- Entity relationship diagrams
- Directory tree structures  
- Architecture visualizations
- Process flows

**Why PlantUML**:
- Easier for AI to parse (structured syntax vs ambiguous spacing)
- Professional rendering for humans
- Better version control (meaningful diffs)
- IDE/GitHub support with inline preview

See [markdown-documentation.instructions.md](../instructions/markdown-documentation.instructions.md) for complete standards.

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
