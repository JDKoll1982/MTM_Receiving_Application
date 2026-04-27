# Role-Based Request Visibility Matrix

**Date:** 2026-01-12  
**Status:** ✅ Complete - 4 Categories Implemented

---

## Request Categories (4 Total)

### 1. Material Handler (15 request types)

**Fulfiller:** Material Handler  
**Examples:** Coils, Flatstock, Component Parts, Dunnage, Dies, Scrap, NCM

### 2. Setup Technician (6 request types)

**Fulfiller:** Setup Technician  
**Examples:** Die Protection Alarm, Die Issues (Stuck, Misalignment, Damage, Adjustment)

### 3. Quality Control (3 request types) ⭐ NEW

**Fulfiller:** Quality Control  
**Examples:** Inspection Request, Quality Question, Other QC Requests

### 4. Production Lead (3 request types)

**Fulfiller:** Production Lead  
**Examples:** Production Question, Safety/Injury Report, Other PL Requests

---

## Role Visibility Matrix

| User Role | Can See Categories | Filter/Toggle Options |
|-----------|-------------------|----------------------|
| **Operator** | All 4 categories | N/A (creates requests for all) |
| **Material Handler** | Material Handler ONLY | None (fixed) |
| **Material Handler Lead** | Material Handler + Quality Control | Toggle: MH / QC / All |
| **Setup Technician** | Setup Technician ONLY | None (fixed) |
| **Quality Control** | Quality Control ONLY | None (fixed) |
| **Production Lead** | Production Lead + Material Handler + Quality Control | Toggle: PL / MH / QC / All |
| **Operator Lead** | Production Lead + Material Handler + Quality Control | Toggle: PL / MH / QC / All |
| **Plant Manager** | All 4 categories | Toggle: MH / ST / QC / PL / All |

---

## Detailed Visibility Rules

### Operator (Can Request All)

**Categories Visible in Wizard:** All 4

- ✅ Material Handler
- ✅ Setup Technician
- ✅ Quality Control
- ✅ Production Lead

**Reasoning:** Operators need to be able to create requests for any type of assistance.

---

### Material Handler (MH Tasks Only)

**Categories Visible in Waitlist:** Material Handler ONLY

- ✅ Material Handler
- ❌ Setup Technician
- ❌ Quality Control
- ❌ Production Lead

**Filter Options:** None (always shows MH tasks)

**Reasoning:** Material Handlers should focus solely on material handling tasks. They should not see setup, quality, or production lead requests as those are not their responsibility.

---

### Material Handler Lead (MH + Quality)

**Categories Visible in Waitlist:** Material Handler + Quality Control

- ✅ Material Handler
- ❌ Setup Technician
- ✅ Quality Control
- ❌ Production Lead

**Filter Options:**

- Material Handler (default)
- Quality Control
- All (shows both MH + QC)

**Reasoning:** MH Leads oversee material handling operations and coordinate with quality control for NCM and inspection requests. They don't need to see setup tech or production lead requests.

---

### Setup Technician (Setup Tasks Only)

**Categories Visible in Waitlist:** Setup Technician ONLY

- ❌ Material Handler
- ✅ Setup Technician
- ❌ Quality Control
- ❌ Production Lead

**Filter Options:** None (always shows Setup Tech tasks)

**Reasoning:** Setup Technicians should focus solely on die-related issues and setup tasks.

---

### Quality Control (QC Tasks Only)

**Categories Visible in Waitlist:** Quality Control ONLY

- ❌ Material Handler
- ❌ Setup Technician
- ✅ Quality Control
- ❌ Production Lead

**Filter Options:** None (always shows QC tasks)

**Reasoning:** Quality Control should focus solely on inspection and quality-related requests. They should NOT see material handler, setup tech, or production lead requests.

---

### Production Lead / Operator Lead (PL + MH + QC)

**Categories Visible in Waitlist:** Production Lead + Material Handler + Quality Control

- ✅ Material Handler
- ❌ Setup Technician
- ✅ Quality Control
- ✅ Production Lead

**Filter Options:**

- Production Lead (default)
- Material Handler
- Quality Control
- All (shows PL + MH + QC)

**Reasoning:** Production Leads oversee production operations, which includes coordinating with material handlers and quality control. They need visibility into these areas but NOT setup technician tasks (which are maintenance-related).

---

### Plant Manager (All Tasks)

**Categories Visible in Waitlist:** All 4 Categories

- ✅ Material Handler
- ✅ Setup Technician
- ✅ Quality Control
- ✅ Production Lead

**Filter Options:**

- Material Handler
- Setup Technician
- Quality Control
- Production Lead
- All (default - shows all 4)

**Reasoning:** Plant Managers have oversight of all operations and need complete visibility across all request types.

---

## Implementation in MockUtils

### Function: `getCategoriesForRole(role)`

Returns array of category names visible to this role:

```javascript
getCategoriesForRole('MaterialHandler') 
// Returns: ['Material Handler']

getCategoriesForRole('MaterialHandlerLead') 
// Returns: ['Material Handler', 'Quality Control']

getCategoriesForRole('ProductionLead') 
// Returns: ['Production Lead', 'Material Handler', 'Quality Control']

getCategoriesForRole('Quality') 
// Returns: ['Quality Control']

getCategoriesForRole('PlantManager') 
// Returns: ['Material Handler', 'Setup Technician', 'Quality Control', 'Production Lead']
```

### Function: `getRequestTypesForRole(role)`

Returns array of all request type objects visible to this role:

```javascript
getRequestTypesForRole('MaterialHandler')
// Returns: 15 Material Handler request types

getRequestTypesForRole('Quality')
// Returns: 3 Quality Control request types

getRequestTypesForRole('ProductionLead')
// Returns: 3 PL + 15 MH + 3 QC = 21 request types
```

### Function: `filterByCategory(waitlist, category, userRole)`

Filters waitlist based on role and selected category filter:

```javascript
// Quality user tries to see all - only shows QC tasks
filterByCategory(allTasks, 'All', 'Quality')
// Returns: Only Quality Control tasks

// MH Lead selects "All" - shows MH + QC
filterByCategory(allTasks, 'All', 'MaterialHandlerLead')
// Returns: Material Handler + Quality Control tasks

// Production Lead selects "Material Handler" - shows only MH
filterByCategory(allTasks, 'Material Handler', 'ProductionLead')
// Returns: Only Material Handler tasks
```

---

## Mockup Updates Required

### ✅ COMPLETED

1. **mock-data.js**
   - Added `requestTypes_QualityControl` array (3 types)
   - Added `fulfiller` property to all request types
   - Added `getCategoriesForRole()` function
   - Added `getRequestTypesForRole()` function
   - Added `filterByCategory()` function with role awareness

2. **Operator/waitlist.html**
   - Added Quality Control category card (4th card)
   - Changed grid from 3-column to 2x2 for 4 categories

3. **styles.css**
   - Changed `.category-cards` to 2-column grid
   - Added `.task-quality-control` color (green)

### 🔄 PENDING

1. **MaterialHandler/waitlist.html**
   - Should ONLY show Material Handler tasks
   - No filter dropdown (fixed to MH tasks only)

2. **MaterialHandlerLead/waitlist.html**
   - Filter dropdown with options: MH | QC | All
   - Default to "Material Handler"

3. **OperatorLead/waitlist.html** or **ProductionLead/waitlist.html**
   - Filter dropdown with options: Production Lead | Material Handler | Quality Control | All
   - Default to "Production Lead"

4. **Quality/waitlist.html** (NEW - doesn't exist yet)
   - Should ONLY show Quality Control tasks
   - No filter dropdown (fixed to QC tasks only)
   - Similar layout to Material Handler view

5. **PlantManager/dashboard.html**
   - Filter dropdown with options: Material Handler | Setup Technician | Quality Control | Production Lead | All
   - Default to "All"
   - Shows comprehensive metrics across all categories

---

## Category Filter Dropdown HTML

### For Material Handler Lead (2 options + All)

```html
<select class="form-select" id="categoryFilter" onchange="filterWaitlist()">
  <option value="Material Handler" selected>Material Handler</option>
  <option value="Quality Control">Quality Control</option>
  <option value="All">All Tasks</option>
</select>
```

### For Production Lead (3 options + All)

```html
<select class="form-select" id="categoryFilter" onchange="filterWaitlist()">
  <option value="Production Lead" selected>Production Lead</option>
  <option value="Material Handler">Material Handler</option>
  <option value="Quality Control">Quality Control</option>
  <option value="All">All Tasks</option>
</select>
```

### For Plant Manager (4 options + All)

```html
<select class="form-select" id="categoryFilter" onchange="filterWaitlist()">
  <option value="All" selected>All Categories</option>
  <option value="Material Handler">Material Handler</option>
  <option value="Setup Technician">Setup Technician</option>
  <option value="Quality Control">Quality Control</option>
  <option value="Production Lead">Production Lead</option>
</select>
```

---

## JavaScript Filter Implementation

```javascript
// Get current user role from MockData.currentUser
const userRole = MockData.currentUser.role;

// Get categories this role can see
const visibleCategories = MockUtils.getCategoriesForRole(userRole);

// Populate filter dropdown (if role has multiple categories)
function populateCategoryFilter() {
  const filterSelect = document.getElementById('categoryFilter');
  
  if (!filterSelect) return; // No filter for single-category roles
  
  // Clear existing options
  filterSelect.innerHTML = '';
  
  // Add "All" option if more than one category
  if (visibleCategories.length > 1) {
    const allOption = document.createElement('option');
    allOption.value = 'All';
    allOption.textContent = 'All Tasks';
    filterSelect.appendChild(allOption);
  }
  
  // Add each visible category
  visibleCategories.forEach(category => {
    const option = document.createElement('option');
    option.value = category;
    option.textContent = category;
    filterSelect.appendChild(option);
  });
}

// Filter waitlist based on selected category
function filterWaitlist() {
  const selectedCategory = document.getElementById('categoryFilter')?.value || 'All';
  const filteredWaitlist = MockUtils.filterByCategory(
    MockData.materialHandlerWaitlist, 
    selectedCategory, 
    userRole
  );
  
  // Re-render waitlist table with filtered data
  renderWaitlistTable(filteredWaitlist);
}
```

---

## Color Coding by Category

| Category | Badge Color | Icon | Example |
|----------|-------------|------|---------|
| Material Handler | Blue (#0078D4) | &#xE7C8; (package) | `.task-receiving` |
| Setup Technician | Orange (#F7630C) | &#xE73E; (wrench) | `.task-setup-tech` |
| Quality Control | Green (#107C10) | &#xE9D9; (checkmark shield) | `.task-quality-control` |
| Production Lead | Blue (#0078D4) | &#xE716; (contact) | `.task-production-lead` |

---

## Special Request Types

### NCM (Non-Conforming Material)

- **Category:** Material Handler
- **Special Handling:** Separate tracking required
- **Visual:** Red badge with border
- **Alert:** Quality team must be notified
- **Visibility:** MH, MH Lead, Production Lead, Plant Manager

### Safety Reports

- **Category:** Production Lead
- **Special Handling:** Immediate alert to Plant Manager
- **Visual:** Red critical alert banner
- **Priority:** Critical
- **Visibility:** Production Lead, Plant Manager

---

## Testing Checklist

### Role Isolation Testing

- [ ] Quality user CANNOT see Material Handler tasks
- [ ] Quality user CANNOT see Setup Technician tasks
- [ ] Quality user CANNOT see Production Lead tasks
- [ ] Material Handler user CANNOT see Quality tasks
- [ ] Material Handler user CANNOT see Setup Tech tasks
- [ ] Material Handler user CANNOT see Production Lead tasks

### Multi-Category Role Testing

- [ ] MH Lead can toggle between MH and QC
- [ ] MH Lead CANNOT see Setup Tech or Production Lead
- [ ] Production Lead can toggle between PL, MH, and QC
- [ ] Production Lead CANNOT see Setup Technician tasks
- [ ] Plant Manager can see all 4 categories

### Filter Dropdown Testing

- [ ] Single-category roles (MH, Setup Tech, QC) have NO filter dropdown
- [ ] Multi-category roles (MH Lead, Prod Lead, Plant Mgr) HAVE filter dropdown
- [ ] "All" option shows only categories visible to that role

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-12  
**Author:** GitHub Copilot  
**Status:** Role visibility matrix complete, mockup updates pending
