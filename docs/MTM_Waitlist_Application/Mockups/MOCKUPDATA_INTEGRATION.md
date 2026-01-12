# MockupData.md Integration Summary

**Date:** 2026-01-12  
**Status:** ✅ Mock Data Updated | 🔄 Mockup Views Pending  

---

## ✅ COMPLETED: Mock Data Structure (mock-data.js)

### New Request Type Categories (4 Total)

**1. Material Handler Requests (15 types)**
- Coils: Bring to Press / Return to Storage
- Flatstock: Bring to Press / Return to Storage  
- Needs: Component Parts / Dunnage / Finished Goods
- Returns: Component Parts / Dunnage / Finished Goods
- NCM Pickup (⚠️ **SEPARATE TRACKING REQUIRED**)
- Empty Scrap Container (with scrap type selection)
- Dies: Bring to Press / Return to Storage (with die location)
- Other (freeform text)

**2. Setup Technician Requests (6 types)**
- Die Protection (Die Pro) Alarm
- Die Issue: Material Stuck
- Die Issue: Misalignment
- Die Issue: Suspected Damage
- Die Issue: Needs Adjustment
- Other (freeform text)

**3. Quality Control Requests (3 types)** ⭐ NEW
- Inspection Request
- Quality Question
- Other (freeform text)

**4. Production Lead Requests (3 types)**
- Production Question (freeform)
- Report Injury or Safety Concern (🚨 **ALERTS PLANT MANAGER**)
- Other (freeform text)

### Sample Data Added
- **Coil Part Numbers:** MMC0001000, MMC0000651, MMC0000352, MMC0000789
- **Flatstock Part Numbers:** MMF0005500, MMF0001234, MMF0009876
- **Component Part Numbers:** 23-23415-006, 45-67890-123, 67-89012-345
- **Die Numbers with Locations:**
  - FGT-001 (Not Located)
  - FGT-1934 (R-A0-01)
  - FGT-2045 (R-B2-03)
  - FGT-3056 (R-C1-02)
- **Scrap Types:** 5052 Aluminum, 3003 Aluminum, Steel, Galvanized Steel, Stainless Steel, Copper, Other

---

## 🔄 PENDING: Mockup View Updates

### 1. Operator/waitlist.html - New Request Wizard

**Current State:** Single-step form with 6 basic request types  
**Target State:** Multi-step wizard with category selection and conditional fields

#### Wizard Flow:
```
Step 1: Select Category
├─ Material Handler (green icon)
├─ Setup Technician (orange icon)
├─ Quality Control (purple icon)
└─ Production Lead (blue icon)

Step 2: Select Request Type
├─ [Shows filtered list based on category]
└─ [Displays icon + description for each]

Step 3: Fill Details (conditional based on type)
├─ Part Number (if required)
├─ Work Order (if required)
├─ Die Number + Location (if die request)
├─ Scrap Type Dropdown (if scrap container)
└─ Freeform Text (if "Other" or Production Lead)

Step 4: Review & Submit
├─ Summary of selections
├─ Estimated time standard
└─ Submit button
```

#### Required Form Fields:
```javascript
{
  category: "Material Handler | Setup Technician | Quality Control | Production Lead",
  requestTypeId: "mh-coil-to-press | st-die-protection | qc-inspection-request | ...",
  partNumber: "MMC0001000" (optional, conditional),
  workOrder: "WO-12345" (optional, conditional),
  dieNumber: "FGT-2045" (optional, conditional),
  dieLocation: "R-B2-03" (auto-populated from dieNumber),
  scrapType: "5052 Aluminum" (optional, conditional),
  details: "Freeform text" (always available),
  press: "Press 6" (auto-filled from user context),
  priority: "calculated based on type"
}
```

#### Wizard UI Enhancements Needed:
1. **Category Selection Cards** (Step 1)
   ```html
   <div class="wizard-step" id="step1-category">
     <div class="category-cards">
       <button class="category-card" data-category="Material Handler">
         <span class="fluent-icon" style="font-size: 48px;">&#xE7C8;</span>
         <h4>Material Handler</h4>
         <p>Coils, parts, dies, dunnage, scrap</p>
       </button>
       <button class="category-card" data-category="Setup Technician">
         <span class="fluent-icon" style="font-size: 48px;">&#xE73E;</span>
         <h4>Setup Technician</h4>
         <p>Die protection, die issues</p>
       </button>
       <button class="category-card" data-category="Quality Control">
         <span class="fluent-icon" style="font-size: 48px;">&#xE8B2;</span>
         <h4>Quality Control</h4>
         <p>Inspections, quality questions</p>
       </button>
       <button class="category-card" data-category="Production Lead">
         <span class="fluent-icon" style="font-size: 48px;">&#xE716;</span>
         <h4>Production Lead</h4>
         <p>Questions, safety concerns</p>
       </button>
     </div>
   </div>
   ```

2. **Request Type List** (Step 2)
   ```html
   <div class="wizard-step hidden" id="step2-type">
     <div class="request-type-list">
       <!-- Dynamically populated from MockData -->
       <div class="request-type-item" data-type-id="mh-coil-to-press">
         <input type="radio" name="requestType" value="mh-coil-to-press" />
         <div class="request-type-content">
           <span class="fluent-icon">&#xE7C8;</span>
           <div>
             <strong>Coils - Bring to Press</strong>
             <p>Request coil to be brought to press (25 min standard)</p>
           </div>
         </div>
       </div>
       <!-- ... more items ... -->
     </div>
   </div>
   ```

3. **Conditional Details Form** (Step 3)
   ```html
   <div class="wizard-step hidden" id="step3-details">
     <!-- Part Number (conditional) -->
     <div class="form-group" id="field-partNumber" style="display:none;">
       <label class="form-label">Part Number <span class="required">*</span></label>
       <input type="text" class="form-input" list="partNumbers" />
       <datalist id="partNumbers">
         <option value="MMC0001000">Coil</option>
         <option value="23-23415-006">Component</option>
       </datalist>
     </div>

     <!-- Work Order (conditional) -->
     <div class="form-group" id="field-workOrder" style="display:none;">
       <label class="form-label">Work Order <span class="required">*</span></label>
       <input type="text" class="form-input" placeholder="WO-12345" />
     </div>

     <!-- Die Number (conditional) -->
     <div class="form-group" id="field-dieNumber" style="display:none;">
       <label class="form-label">Die Number <span class="required">*</span></label>
       <select class="form-select" id="dieNumberSelect">
         <option value="">Select die...</option>
         <option value="FGT-1934">FGT-1934 (Location: R-A0-01)</option>
         <option value="FGT-2045">FGT-2045 (Location: R-B2-03)</option>
       </select>
     </div>

     <!-- Scrap Type (conditional) -->
     <div class="form-group" id="field-scrapType" style="display:none;">
       <label class="form-label">Scrap Type <span class="required">*</span></label>
       <select class="form-select">
         <option value="">Select scrap type...</option>
         <option value="5052 Aluminum">5052 Aluminum</option>
         <option value="3003 Aluminum">3003 Aluminum</option>
         <option value="Steel">Steel</option>
         <option value="Galvanized Steel">Galvanized Steel</option>
         <option value="Stainless Steel">Stainless Steel</option>
         <option value="Copper">Copper</option>
         <option value="Other">Other</option>
       </select>
     </div>

     <!-- Freeform Details (always shown) -->
     <div class="form-group">
       <label class="form-label">Additional Details</label>
       <textarea class="form-textarea" placeholder="Add notes..."></textarea>
     </div>
   </div>
   ```

4. **Review Step** (Step 4)
   ```html
   <div class="wizard-step hidden" id="step4-review">
     <div class="review-summary">
       <h4>Review Your Request</h4>
       <dl class="review-list">
         <dt>Category:</dt>
         <dd id="review-category">Material Handler</dd>
         
         <dt>Request Type:</dt>
         <dd id="review-type">Coils - Bring to Press</dd>
         
         <dt>Part Number:</dt>
         <dd id="review-partNumber">MMC0001000</dd>
         
         <dt>Estimated Wait:</dt>
         <dd id="review-timeStandard">25 minutes</dd>
       </dl>
     </div>
   </div>
   ```

#### JavaScript Logic Needed:
```javascript
let wizardState = {
  currentStep: 1,
  category: null,
  requestTypeId: null,
  requestTypeDetails: null,
  formData: {}
};

function selectCategory(category) {
  wizardState.category = category;
  // Load request types for this category
  const types = MockUtils.getRequestTypesByCategory(category);
  // Show step 2 with filtered types
  showWizardStep(2);
}

function selectRequestType(typeId) {
  wizardState.requestTypeId = typeId;
  wizardState.requestTypeDetails = MockUtils.getRequestTypeById(typeId);
  
  // Show/hide conditional fields
  toggleField('partNumber', wizardState.requestTypeDetails.requiresPartNumber);
  toggleField('workOrder', wizardState.requestTypeDetails.requiresWorkOrder);
  toggleField('dieNumber', wizardState.requestTypeDetails.requiresDieNumber);
  toggleField('scrapType', wizardState.requestTypeDetails.requiresScrapType);
  
  showWizardStep(3);
}

function toggleField(fieldId, shouldShow) {
  const field = document.getElementById(`field-${fieldId}`);
  if (field) {
    field.style.display = shouldShow ? 'block' : 'none';
    const input = field.querySelector('input, select, textarea');
    if (input) {
      input.required = shouldShow;
    }
  }
}
```

---

### 2. Material Handler/waitlist.html - Updated Categories

**Changes Needed:**

1. **Add NCM Filter** (separate from standard MH tasks)
   ```html
   <select class="form-select" id="categoryFilter">
     <option value="All">All Tasks</option>
     <option value="Material Handler">Material Handling</option>
     <option value="NCM">NCM Tasks (Separate Tracking)</option>
     <option value="Setup Technician">Setup Tech Requests</option>
     <option value="Production Lead">Production Lead Requests</option>
   </select>
   ```

2. **NCM Visual Indicator**
   ```html
   <!-- For NCM tasks, add warning badge -->
   <tr class="ncm-task">
     <td>
       <span class="task-badge task-ncm">
         <span class="fluent-icon">&#xE7BA;</span>
         NCM Pickup
       </span>
       <span class="priority-badge priority-high">SEPARATE TRACKING</span>
     </td>
   </tr>
   ```

3. **Safety Alert Banner** (for Production Lead safety reports)
   ```html
   <div class="info-bar info-bar-error mb-lg" id="safetyAlert" style="display:none;">
     <span class="info-bar-icon fluent-icon">&#xE7BA;</span>
     <div class="info-bar-content">
       <div class="info-bar-title">🚨 SAFETY CONCERN REPORTED</div>
       <div class="info-bar-message">
         Press 3 - Operator reported safety issue. IMMEDIATE ATTENTION REQUIRED.
         <a href="#" style="color: #FFF; text-decoration: underline;">View Details</a>
       </div>
     </div>
   </div>
   ```

4. **Die Location Display** (for die requests)
   ```html
   <td>
     <strong>Die FGT-2045</strong>
     <div style="font-size: 12px; color: var(--text-secondary);">
       <span class="fluent-icon">&#xE707;</span> Location: R-B2-03
     </div>
   </td>
   ```

5. **Scrap Type Display** (for scrap container requests)
   ```html
   <td>
     <strong>Empty Scrap Container</strong>
     <div style="font-size: 12px; color: var(--text-secondary);">
       Scrap Type: 5052 Aluminum
     </div>
   </td>
   ```

---

### 3. MaterialHandlerLead/waitlist.html - Updated Analytics

**Changes Needed:**

1. **NCM Task Count Card**
   ```html
   <div class="card">
     <div class="card-content">
       <div style="font-size: 32px; font-weight: 600; color: var(--error-color);">3</div>
       <div style="font-size: 12px; color: var(--text-secondary);">NCM Tasks (Separate)</div>
     </div>
   </div>
   ```

2. **Category Breakdown Chart**
   ```html
   <!-- Replace task type chart with category breakdown -->
   <div class="chart-container">
     <h3>Requests by Category</h3>
     <!-- Bar chart showing Material Handler, Setup Tech, Production Lead counts -->
   </div>
   ```

---

### 4. PlantManager/dashboard.html - Safety Alerts

**Changes Needed:**

1. **Critical Safety Alert Banner** (top of dashboard)
   ```html
   <div class="info-bar info-bar-error mb-lg" id="plantManagerSafetyAlert">
     <span class="info-bar-icon fluent-icon" style="font-size: 24px;">&#xE7BA;</span>
     <div class="info-bar-content">
       <div class="info-bar-title">🚨 CRITICAL: SAFETY CONCERN REPORTED</div>
       <div class="info-bar-message">
         Press 3 - John Operator reported injury. Reported 5 minutes ago.
         <button class="btn" style="margin-left: 12px;">Acknowledge & View</button>
       </div>
     </div>
   </div>
   ```

2. **Safety Dashboard Card**
   ```html
   <div class="card" style="border-left: 4px solid var(--error-color);">
     <div class="card-header">
       <h3 class="card-title">Safety Incidents & Concerns</h3>
     </div>
     <div class="card-content">
       <table class="winui-table">
         <thead>
           <tr>
             <th>TIME</th>
             <th>LOCATION</th>
             <th>REPORTED BY</th>
             <th>DESCRIPTION</th>
             <th>STATUS</th>
           </tr>
         </thead>
         <tbody>
           <tr>
             <td><strong>5 min ago</strong></td>
             <td>Press 3</td>
             <td>John Operator</td>
             <td>Minor hand injury - first aid applied</td>
             <td><span class="status-badge priority-high">OPEN</span></td>
           </tr>
         </tbody>
       </table>
     </div>
   </div>
   ```

---

## 🎨 UI/UX Enhancements Required

### New CSS Classes (add to styles.css)

```css
/* NCM Task Type */
.task-ncm {
  background-color: var(--error-color);
  color: #FFFFFF;
}

.ncm-task {
  background-color: rgba(196, 43, 28, 0.1);
  border-left: 4px solid var(--error-color);
}

/* Setup Tech Task Type */
.task-setup-tech {
  background-color: #F7630C;
  color: #FFFFFF;
}

/* Production Lead Task Type */
.task-production-lead {
  background-color: #0078D4;
  color: #FFFFFF;
}

/* Quality Control Task Type */
.task-quality-control {
  background-color: #6f42c1;
  color: #FFFFFF;
}

/* Category Selection Cards */
.category-cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin: 24px 0;
}

.category-card {
  padding: 32px;
  border: 2px solid var(--divider-stroke);
  border-radius: 8px;
  background-color: var(--background-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: center;
}

.category-card:hover {
  border-color: var(--accent-default);
  background-color: var(--background-tertiary);
  transform: translateY(-4px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.category-card.selected {
  border-color: var(--accent-default);
  background-color: var(--accent-default);
  color: #FFFFFF;
}

/* Request Type List */
.request-type-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 400px;
  overflow-y: auto;
}

.request-type-item {
  display: flex;
  align-items: center;
  padding: 16px;
  border: 2px solid var(--divider-stroke);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.request-type-item:hover {
  border-color: var(--accent-light);
  background-color: var(--background-tertiary);
}

.request-type-item input[type="radio"] {
  margin-right: 12px;
}

.request-type-item input[type="radio"]:checked + .request-type-content {
  color: var(--accent-default);
  font-weight: 600;
}

.request-type-content {
  display: flex;
  align-items: center;
  gap: 16px;
}

.request-type-content .fluent-icon {
  font-size: 24px;
}

/* Review Summary */
.review-summary {
  padding: 24px;
  background-color: var(--background-secondary);
  border-radius: 8px;
}

.review-list {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 12px;
  margin-top: 16px;
}

.review-list dt {
  font-weight: 600;
  color: var(--text-secondary);
}

.review-list dd {
  margin: 0;
  color: var(--text-primary);
}

/* Wizard Navigation */
.wizard-navigation {
  display: flex;
  justify-content: space-between;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--divider-stroke);
}

/* Required Field Indicator */
.required {
  color: var(--error-color);
  font-weight: bold;
}
```

---

## 📋 Implementation Checklist

### Phase 1: Mock Data (✅ COMPLETE)
- [x] Add 3 request type categories
- [x] Add sample part numbers
- [x] Add die numbers and locations
- [x] Add scrap types
- [x] Update utility functions

### Phase 2: Operator Waitlist (🔄 IN PROGRESS)
- [ ] Implement multi-step wizard
- [ ] Add category selection (Step 1)
- [ ] Add request type selection (Step 2)
- [ ] Add conditional form fields (Step 3)
- [ ] Add review step (Step 4)
- [ ] Add wizard navigation (Back/Next/Submit)
- [ ] Add field validation logic
- [ ] Update table to show new request types

### Phase 3: Material Handler Waitlist (🔄 PENDING)
- [ ] Add category filter dropdown
- [ ] Add NCM visual indicators
- [ ] Add die location display
- [ ] Add scrap type display
- [ ] Add safety alert banner
- [ ] Update task assignment logic

### Phase 4: Lead Dashboards (🔄 PENDING)
- [ ] Add NCM task count cards
- [ ] Update analytics charts for new categories
- [ ] Add safety incident tracking
- [ ] Update performance metrics

### Phase 5: Plant Manager Dashboard (🔄 PENDING)
- [ ] Add critical safety alert banner
- [ ] Add safety incidents dashboard card
- [ ] Update plant-wide metrics
- [ ] Add NCM tracking summary

### Phase 6: Styling (🔄 PENDING)
- [ ] Add new CSS classes for NCM, Setup Tech, Production Lead
- [ ] Add wizard styling
- [ ] Add category card styling
- [ ] Add review summary styling

---

## 🎯 Next Steps

1. **Implement Operator Wizard** (Highest Priority)
   - This is the primary user entry point
   - Demonstrates all 4 categories
   - Shows conditional field logic

2. **Update Material Handler View**
   - Shows how requests are fulfilled
   - Demonstrates NCM separation
   - Shows die location integration

3. **Update Lead Dashboards**
   - Analytics reflect new categories
   - NCM tracking separated

4. **Update Plant Manager Dashboard**
   - Critical safety alerts visible
   - Plant-wide view of all categories

---

## 📝 Notes for Developers

1. **NCM Tracking:**
   - NCM tasks MUST be kept separate in database
   - Requires separate approval workflow
   - Quality team must be notified

2. **Safety Alerts:**
   - Production Lead safety reports trigger immediate alerts
   - Plant Manager dashboard must show critical banner
   - Email/SMS notifications in real app

3. **Die Locations:**
   - Die number selection auto-populates location
   - Location displayed to material handler
   - Location updated when die moved

4. **Time Standards:**
   - Each request type has specific time standard
   - Approvals required for deviations (see Nick Wunsch/Cristofer Muchowski)

5. **Validation:**
   - Part number format validation (MMC*, MMF*, ##-#####-###)
   - Work order format validation
   - Die number validation against inventory

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-12  
**Author:** GitHub Copilot  
**Status:** Mock data complete, mockup updates documented and pending implementation
