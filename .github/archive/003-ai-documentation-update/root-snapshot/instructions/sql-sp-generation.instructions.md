---
applyTo: "Database/StoredProcedures/**/*.sql,Database/Scripts/**/*.sql,Database/Deploy/**/*.sql"
---

# SQL Development

## SQL Coding Style

- use uppercase for SQL keywords (SELECT, FROM, WHERE)
- use consistent indentation for nested queries and conditions
- include comments to explain complex logic
- break long queries into multiple lines for readability
- organize clauses consistently (SELECT, FROM, JOIN, WHERE, GROUP BY, HAVING, ORDER BY)

## SQL Query Structure

- use explicit column names in SELECT statements instead of SELECT \*
- qualify column names with table name or alias when using multiple tables
- limit the use of subqueries when joins can be used instead
- include LIMIT/TOP clauses to restrict result sets
- use appropriate indexing for frequently queried columns
- avoid using functions on indexed columns in WHERE clauses

## Stored Procedure Naming Conventions

- prefix stored procedure names with 'usp\_'
- use PascalCase for stored procedure names
- use descriptive names that indicate purpose (e.g., usp_GetCustomerOrders)
- include plural noun when returning multiple records (e.g., usp_GetProducts)
- include singular noun when returning single record (e.g., usp_GetProduct)

## Parameter Handling

- prefix parameters with '@'
- use camelCase for parameter names
- provide default values for optional parameters
- validate parameter values before use
- document parameters with comments
- arrange parameters consistently (required first, optional later)

## Stored Procedure Structure

- include header comment block with description, parameters, and return values
- return standardized error codes/messages
- return result sets with consistent column order
- use OUTPUT parameters for returning status information
- prefix temporary tables with 'tmp\_'

## Toggleable Test Blocks

- when a stored procedure includes inline manual test cases, format them so `Database/Scripts/SetTestBlockState/Set-SqlTestBlockState.ps1` can enable, disable, or toggle the test bodies safely
- start each toggleable test block with a commented marker line in this exact format: `-- TEST BLOCK START: <Block Name>`
- end each toggleable test block with a commented marker line in this exact format: `-- TEST BLOCK END: <Block Name>`
- keep the start and end marker lines commented at all times; the script uses them to find the block boundaries
- keep every non-blank line inside the block commented by default so the checked-in SQL remains safe to open and review without executing tests accidentally
- use normal SQL indentation inside the block, but keep the comment prefix on each active SQL line, for example `--     'value'`
- blank lines inside a block are allowed and are preserved by the script
- do not nest test blocks inside other test blocks
- use a unique block name per block within the file so targeted toggling with `-BlockName` is predictable
- do not place unrelated commented prose inside the block body unless you want the script to uncomment it together with the SQL

Example:

```sql
-- TEST BLOCK START: TEST 1 - Valid user creation
-- SET @error_msg = 'NOT EXECUTED';
-- CALL sp_Auth_User_Create(
--     1001,
--     'DOMAIN\\testuser01',
--     'Test User One',
--     '1234',
--     'Receiving',
--     '1st Shift',
--     'SYSTEM_ADMIN',
--     '',
--     '',
--     @error_msg
-- );
-- SELECT
--     'TEST 1 - Valid Creation' AS TestName,
--     @error_msg AS Result;
-- TEST BLOCK END: TEST 1 - Valid user creation
```

Recommended note immediately above the test area:

```sql
-- NOTE: Use Database/Scripts/SetTestBlockState/Set-SqlTestBlockState.ps1 to enable, disable, or
--       toggle the test bodies below while preserving the block markers.
```

## SQL Security Best Practices

- parameterize all queries to prevent SQL injection
- use prepared statements when executing dynamic SQL
- avoid embedding credentials in SQL scripts
- implement proper error handling without exposing system details
- avoid using dynamic SQL within stored procedures

## Transaction Management

- explicitly begin and commit transactions
- use appropriate isolation levels based on requirements
- avoid long-running transactions that lock tables
- use batch processing for large data operations
- include SET NOCOUNT ON for stored procedures that modify data
