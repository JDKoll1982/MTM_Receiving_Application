# Coil Inventory Summary Report — Planning Document

**Last Updated:** 2026-03-06

---

## 1. Objective

Create a comprehensive view of all coil inventory currently in stock, showing how many unique coils exist, where they are physically located, and their typical size characteristics (weight and quantity per location).

---

## 2. Purpose & Business Value

### What Problem Does This Solve?

- Warehouse staff need to quickly identify what coils are available and where to find them
- Management needs visibility into average coil sizes to plan for storage and handling
- Operations needs to match available inventory with customer orders efficiently

### Who Will Use This?

- Warehouse personnel locating specific coils
- Inventory planners assessing available stock
- Customer service representatives checking availability
- Supervisors monitoring inventory levels and distribution

---

## 3. Data Sources

### Source 1: Receiving Label History (MTM Application Database)

**What it contains:**

- Records of all coils received into the facility
- Original identification details for each coil
- Initial receipt information and characteristics

**Why we need it:**

- Provides the "birth certificate" for each coil
- Establishes the unique identity of each coil
- Contains original specifications and properties

### Source 2: Current Inventory (Infor Visual Database)

**What it contains:**

- Real-time snapshot of what is physically on hand
- Current warehouse locations
- Current quantities available

**Why we need it:**

- Shows which coils are still in the building
- Confirms current storage locations
- Verifies what is actually available for use or sale

### Source 3: Transaction History (Infor Visual Database)

**What it contains:**

- Movement records (receipts, issues, transfers)
- Changes in quantity over time
- Location movement history

**Why we need it:**

- Validates current status against past activities
- Helps track coil movement patterns
- Provides audit trail for inventory changes

---

## 4. Information to Display

For each unique coil, show:

| Information | Description | Business Use |
|---|---|---|
| **Coil Identifier** | Unique code or number identifying the specific coil | Locate and reference specific inventory |
| **Current Quantity** | How many units (feet, pounds, pieces) of this coil remain | Know what's available to allocate |
| **Storage Location** | Warehouse zone, aisle, bin, or coordinates | Find the physical coil quickly |
| **Average Weight** | Typical weight of coils in this group | Plan for equipment and handling needs |
| **Average Coil Count** | Typical number of individual coils per location | Understand storage density and space usage |

---

## 5. Processing Logic

### Step 1: Identify Active Coils

- Start with the Receiving Label History to get all coils that have entered the facility
- Cross-reference with Current Inventory to filter only coils still on hand
- **Result:** List of coils that exist in the building right now

### Step 2: Determine Current Locations

- For each active coil, look up its current warehouse location
- If a coil appears in multiple locations, show each location separately
- **Result:** Each coil mapped to its physical location(s)

### Step 3: Calculate Current Quantities

- For each coil-location combination, determine the quantity on hand
- Account for partial usage (coils that have been partially consumed)
- **Result:** Exact quantity available at each location

### Step 4: Compute Averages

**Average Weight Calculation:**

- Group coils by relevant characteristics (size, grade, type)
- Calculate the mean weight across the group
- **Result:** Representative weight for planning purposes

**Average Coil Count Calculation:**

- Group by location or coil type
- Calculate mean number of individual coils
- **Result:** Typical storage pattern information

---

## 6. Business Rules & Considerations

### What Counts as "In Stock"?

- Coils with quantity greater than zero
- Coils in available locations (not blocked or quarantined)
- Coils not allocated to pending shipments (if applicable)

### How to Handle Partial Coils?

- Include any coil with remaining quantity > 0
- Report actual remaining quantity, not original quantity
- Flag if significantly different from original size (optional enhancement)

### What Defines "Unique"?

- Each receiving label represents a unique coil
- Same material from different receipts = different coils
- Splitting a coil creates a new unique identifier

### How to Group for Averages?

- By material specification
- By physical characteristics (gauge, width, etc.)
- By storage location type
- User may need to define grouping criteria

---

## 7. Expected Output Format

### Summary List View

```
Coil ID     | Quantity  | Location  | Avg Weight | Avg Count
-------------------------------------------------------------
COIL-12345  | 2,500 lbs | A-12-03   | 2,450 lbs  | 1.2 coils
COIL-12346  | 1,800 lbs | B-05-11   | 2,450 lbs  | 1.2 coils
COIL-12347  | 3,200 lbs | A-12-05   | 2,450 lbs  | 1.2 coils
```

### Detailed Information (Expandable / Drill-Down)

- Historical receipt information
- Movement history for the coil
- Related transactions
- Material specifications

---

## 8. User Workflow

### How Users Will Access This Information

1. Navigate to "Inventory Reports" or "Coil Summary" section
2. *(Optional)* Apply filters (date range, location, material type)
3. System generates report from the three data sources
4. View results in sortable, filterable list
5. *(Optional)* Export to spreadsheet for further analysis
6. *(Optional)* Print for physical inventory tasks

### Refresh Frequency

| Mode | Behaviour |
|---|---|
| **On-demand** | User clicks "Refresh" to get latest data |
| **Automatic** | Updates every X minutes if kept open |
| **Scheduled** | Could generate overnight for morning review |

---

## 9. Performance Considerations

### Data Volume Expectations

| Source | Estimated Size |
|---|---|
| Receiving Label History | All historical receipts (potentially thousands) |
| Current Inventory | Subset of active items (hundreds to thousands) |
| Transaction History | All movements (potentially tens of thousands) |

### Processing Approach

- **Filter early:** Start with Current Inventory to reduce dataset
- **Use indexed lookups:** Match on coil identifiers efficiently
- **Calculate averages in batches:** Group similar coils together
- **Consider caching:** Store averages if recalculating is slow

---

## 10. Future Enhancements (Not in Initial Scope)

| Enhancement | Description |
|---|---|
| **Aging analysis** | How long has each coil been in stock? |
| **Usage trends** | Are coils being consumed evenly or sitting idle? |
| **Alert thresholds** | Notify when coil counts drop below minimums |
| **Visual mapping** | Show warehouse layout with coil locations |
| **Forecasting** | Predict when coils will be depleted based on usage patterns |

---

## 11. Success Criteria

This feature will be considered successful when:

- [ ] Users can view all current coils with their locations in under 5 seconds
- [ ] Quantities match physical inventory counts (within tolerance)
- [ ] Average calculations provide useful planning information
- [ ] Report can be filtered and exported for offline use
- [ ] Information is accurate and trustworthy for decision-making

---

## Next Steps

1. **Validate assumptions** — Confirm data availability and structure in all three sources
2. **Define filters** — Determine what filtering options users need most
3. **Design interface** — Create mockup of how information will be displayed
4. **Plan technical implementation** — Map to MVVM architecture components
5. **Identify test scenarios** — Define edge cases and validation criteria
