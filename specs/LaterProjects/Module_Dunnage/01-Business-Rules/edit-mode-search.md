# Edit Mode Search Criteria

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Edit Mode Specification](../02-Workflow-Modes/003-edit-mode-specification.md)

---

## Rule Definition

Edit Mode Search Criteria defines the filters and search parameters available for locating historical dunnage transactions for viewing and modification.

---

## Available Search Filters

### Filter 1: Transaction Date Range (Required)

**Definition**: Users must specify a date range to search within.

**Configuration**:
- **From Date**: Start of date range (inclusive)
- **To Date**: End of date range (inclusive)
- **Default**: Last 30 days
- **Maximum Range**: 1 year (365 days) - configurable

**Validation**:
```
If From Date > To Date:
    Error: "From date must be before To date"
    Action: Block search

If (To Date - From Date) > 365 days:
    Warning: "Date range exceeds 1 year. This may return many results."
    Action: Allow with confirmation
```

**Rationale**: Prevents unbounded queries that could impact database performance.

---

### Filter 2: Dunnage Type (Optional)

**Definition**: Filter transactions that contain at least one load of the specified type.

**Behavior**:
```
Selection: "Wood Pallet 48x40"

SQL WHERE clause:
WHERE EXISTS (
    SELECT 1 FROM dunnage_loads dl
    WHERE dl.transaction_id = dt.transaction_id
    AND dl.type_id = {selected_type_id}
)
```

**UI**:
```
Dunnage Type: [All Types ▼]
Options:
  - All Types (no filter)
  - Wood Pallet 48x40
  - Cardboard Box
  - Metal Rack
  - ...
```

**Note**: Only filters transactions containing that type; transaction may have other types too.

---

### Filter 3: Part Number (Optional)

**Definition**: Filter transactions that contain at least one load with the specified part.

**Input**: ComboBox with auto-complete

**Behavior**:
```
User types: "TUBE"
Auto-complete shows: TUBE-A123, TUBE-B456, TUBE-C789

User selects: TUBE-A123

SQL WHERE clause:
WHERE EXISTS (
    SELECT 1 FROM dunnage_loads dl
    WHERE dl.transaction_id = dt.transaction_id
    AND dl.part_id = {selected_part_id}
)
```

**Search Modes**:
- **Exact Match**: User selects from dropdown (default)
- **Contains**: User types partial, searches for contains (optional feature)

---

### Filter 4: Load ID (Optional)

**Definition**: Direct lookup by unique load identifier.

**Behavior**:
```
If Load ID provided:
    Ignore all other filters
    Execute direct query:
    SELECT * FROM dunnage_loads WHERE load_id = {load_id}
    
    Return transaction containing that load
```

**Use Case**: User has load ID from label or CSV export, wants to find exact load quickly.

**UI**:
```
Load ID: [___________]
ℹ If provided, other filters are ignored
```

---

### Filter 5: User (Optional)

**Definition**: Filter transactions created by a specific user.

**UI**:
```
Created By: [All Users ▼]
Options:
  - All Users (no filter)
  - jdoe (John Doe)
  - jsmith (Jane Smith)
  - ...
```

**Behavior**:
```
SQL WHERE clause:
WHERE created_by = {selected_user_id}
```

**User List Source**: Only users who have created dunnage transactions appear in dropdown.

---

### Filter 6: Status (Optional)

**Definition**: Filter by transaction modification status.

**UI**:
```
Status: [All ▼]
Options:
  - All (no filter)
  - Active (not modified)
  - Modified (edited after creation)
  - Deleted (soft-deleted)
```

**Behavior**:
```
Active:
WHERE modified_date IS NULL

Modified:
WHERE modified_date IS NOT NULL

Deleted:
WHERE is_deleted = 1
```

**Rationale**: Helps supervisors find transactions that need review or audit.

---

## Search Execution Rules

### Rule 1: Date Range Mandatory

**Definition**: Search cannot execute without date range.

**Validation**:
```
If From Date is NULL OR To Date is NULL:
    Error: "Date range is required"
    Action: Disable "Search" button
```

---

### Rule 2: Load ID Override

**Definition**: If Load ID is provided, all other filters are ignored.

**Behavior**:
```
If Load ID is NOT empty:
    Execute direct load lookup
    Display transaction containing that load
    Ignore: Type, Part, User, Status, Date Range filters
```

---

### Rule 3: Maximum Results

**Definition**: Limit results to prevent UI overload and performance issues.

**Configuration**:
- **Results Per Page**: 25 (configurable)
- **Maximum Total Results**: 1000 (configurable)
- **Pagination**: Required for > 25 results

**Behavior**:
```
If query returns > 1000 results:
    Warning: "Search returned over 1000 results. 
              Showing first 1000. Please narrow your filters."
    Action: Show first 1000, allow pagination
```

---

### Rule 4: Performance Timeout

**Definition**: Queries that exceed timeout are cancelled.

**Configuration**:
- **Query Timeout**: 30 seconds (configurable)

**Behavior**:
```
If query exceeds 30 seconds:
    Error: "Search timed out. Please narrow your search criteria."
    Action: Cancel query, return to search form
```

---

## Search Performance Optimization

### Database Indexes

**Required Indexes**:
```sql
-- Primary search index (date range)
CREATE INDEX idx_dunnage_loads_created_date 
ON dunnage_loads(created_date);

-- User filter index
CREATE INDEX idx_dunnage_loads_created_by 
ON dunnage_loads(created_by);

-- Type filter index
CREATE INDEX idx_dunnage_loads_type_id 
ON dunnage_loads(type_id);

-- Part filter index
CREATE INDEX idx_dunnage_loads_part_id 
ON dunnage_loads(part_id);

-- Load ID direct lookup (unique)
CREATE UNIQUE INDEX idx_dunnage_loads_load_id 
ON dunnage_loads(load_id);

-- Composite index for common queries
CREATE INDEX idx_dunnage_loads_date_user 
ON dunnage_loads(created_date, created_by);
```

---

### Query Optimization

**Use Covering Indexes**:
```sql
-- For list view, select only needed columns
SELECT 
    transaction_id,
    created_date,
    created_by,
    COUNT(*) as load_count
FROM dunnage_loads
WHERE created_date BETWEEN @from AND @to
GROUP BY transaction_id, created_date, created_by
ORDER BY created_date DESC
LIMIT 25 OFFSET 0;
```

**Avoid Full Table Scans**:
- Always filter by date range (indexed)
- Use LIMIT/OFFSET for pagination
- Use EXISTS for type/part filters (faster than JOIN)

---

## UI/UX Behavior

### Smart Defaults

**On First Load**:
```
From Date: [Today - 30 days]
To Date:   [Today]
Type:      [All Types]
Part:      [Empty]
Load ID:   [Empty]
User:      [All Users]
Status:    [All]
```

**After Search**:
```
Preserve last search criteria
User can modify and re-search
[Clear Filters] button resets to defaults
```

---

### Search Results Display

**Grid Columns**:
```
| Trans ID | Date       | User    | Type        | Loads | Last Modified |
|----------|------------|---------|-------------|-------|---------------|
| 12345    | 01/25/2026 | jdoe    | Wood Pallet | 10    | 01/25 10:30   |
| 12344    | 01/24/2026 | jsmith  | Cardboard   | 5     | 01/24 14:15   |
```

**Row Actions**:
- Double-click: Open transaction for editing
- Right-click menu: View, Export CSV, View Audit Log, Delete

**Pagination**:
```
[◀ Previous]  Page 1 of 3  [Next ▶]
```

---

### No Results Found

**Message**:
```
ℹ No transactions found matching your criteria.

Suggestions:
  • Widen the date range
  • Remove some filters
  • Check for typos in part number
  • Try searching by Load ID if you have one
```

---

### Error Handling

**Scenario 1: Invalid Date Range**
```
❌ From date must be before To date.
```

**Scenario 2: Date Range Too Large**
```
⚠ Date range exceeds 1 year. This may return many results.

Continue? [Yes] [No]
```

**Scenario 3: No Transactions in Period**
```
ℹ No dunnage transactions found between 01/01/2025 and 01/31/2025.

Try a different date range.
```

**Scenario 4: Query Timeout**
```
❌ Search timed out after 30 seconds.

Please narrow your search:
  • Use a smaller date range
  • Add more specific filters (Type, Part, User)
  
[OK]
```

---

## Code Implementation

```csharp
public class Service_DunnageSearch
{
    public async Task<SearchResult> SearchTransactionsAsync(SearchCriteria criteria)
    {
        // Validation
        if (criteria.FromDate == null || criteria.ToDate == null)
            return SearchResult.Error("Date range is required");
        
        if (criteria.FromDate > criteria.ToDate)
            return SearchResult.Error("From date must be before To date");
        
        if ((criteria.ToDate.Value - criteria.FromDate.Value).TotalDays > 365)
        {
            // Log warning but allow
            _logger.LogWarning($"Large date range: {criteria.FromDate} to {criteria.ToDate}");
        }
        
        // Load ID override
        if (!string.IsNullOrEmpty(criteria.LoadId))
        {
            return await _dao.SearchByLoadIdAsync(criteria.LoadId);
        }
        
        // Build filtered query
        var query = new DatabaseQuery()
            .Where("created_date >= @fromDate", criteria.FromDate)
            .Where("created_date <= @toDate", criteria.ToDate);
        
        if (criteria.TypeId.HasValue)
            query.WhereExists("SELECT 1 FROM dunnage_loads WHERE type_id = @typeId");
        
        if (criteria.PartId.HasValue)
            query.WhereExists("SELECT 1 FROM dunnage_loads WHERE part_id = @partId");
        
        if (criteria.UserId.HasValue)
            query.Where("created_by = @userId", criteria.UserId);
        
        if (!string.IsNullOrEmpty(criteria.Status))
        {
            if (criteria.Status == "Active")
                query.Where("modified_date IS NULL");
            else if (criteria.Status == "Modified")
                query.Where("modified_date IS NOT NULL");
            else if (criteria.Status == "Deleted")
                query.Where("is_deleted = 1");
        }
        
        query.OrderBy("created_date DESC")
             .Limit(criteria.PageSize)
             .Offset(criteria.Page * criteria.PageSize);
        
        return await _dao.ExecuteSearchAsync(query);
    }
}
```

---

## Related Documentation

- [Edit Mode Specification](../02-Workflow-Modes/003-edit-mode-specification.md) - Complete edit workflow
- [Data Flow](../00-Core/data-flow.md) - Transaction data structure

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete
