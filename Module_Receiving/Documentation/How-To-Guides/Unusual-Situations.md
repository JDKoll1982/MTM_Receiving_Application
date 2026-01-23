# Unusual Situations - Module_Receiving

**Last Updated: 2025-01-15**

## Receiving Without a PO Number

**Situation**: Material arrived but there's no purchase order in the system.

**Steps**:
1. **Don't proceed with normal receiving**—it will fail validation
2. Contact your supervisor or purchasing department
3. Options they might choose:
   - Create emergency PO in ERP (then you can proceed)
   - Use "non-stock" receiving process (if your facility has one)
   - Hold material until PO is created
4. If you receive before PO exists, it won't appear in reports or routing

**Workaround (if authorized)**:
- Some facilities allow using a generic "emergency PO" number
- Check with supervisor first
- Document actual vendor and part in notes field

## PO Number Valid But Parts Don't Match

**Situation**: PO looks correct in ERP but the parts that arrived are different.

**Steps**:
1. Verify packing slip against what physically arrived
2. Re-check PO number you entered (typo?)
3. If mismatch is real:
   - **Do not force the receive**
   - Contact purchasing: "PO shows Part A, but Part B arrived"
   - Purchasing will either:
     - Issue a new PO for the correct part
     - Have supplier correct the shipment
     - Create a return

**Common Causes**:
- Vendor shipped wrong parts
- PO was changed after shipment
- Packing slip has wrong PO number

## Receiving Partial Shipment

**Situation**: PO is for 1000 pieces, but only 500 arrived today.

**Steps**:
1. **Proceed normally** with the wizard
2. On the Quantity entry screen:
   - Enter 500 (what actually arrived)
   - **Not** 1000 (what was ordered)
3. System tracks this as a partial receive
4. When the remaining 500 arrive:
   - Start a new receive session
   - Use the same PO number
   - Enter the additional 500

**Important**: 
- Each physical arrival = one receiving session
- System tracks cumulative received quantity per PO
- Reports show received vs. ordered

## One PO, Many Loads, Mixed Package Types

**Situation**: One shipment with 3 pallets, 5 boxes, and 2 crates.

**Steps**:
1. On "Number of Loads" screen, enter **10** (total containers)
2. On Load Details screen:
   - Loads 1-3: Select "Pallet" from dropdown
   - Loads 4-8: Select "Box" from dropdown
   - Loads 9-10: Select "Crate" from dropdown
3. Each load gets its own label with correct package type

**Why this matters**: 
- Warehouse routing depends on package type
- Labels must match physical container
- Forklift routes differ from manual carry items

## Heat/Lot Number Missing or Unclear

**Situation**: Packing slip is supposed to have a heat/lot code but it's unreadable or missing.

**Steps**:
1. **Check multiple places** on packaging:
   - Packing slip
   - Stamped on parts
   - Stickers on containers
   - Supplier certificate (sometimes included)
2. If truly missing:
   - Contact Quality Control **before** receiving
   - They will decide:
     - **Acceptable**: Receive without heat/lot (leave field blank)
     - **Quality hold**: Quarantine shipment, don't receive yet
     - **Reject**: Return to supplier
3. **Never guess or make up a heat/lot number**

**Documentation**:
- If you receive without heat/lot, note why in comments (if available)
- QC may require a non-conformance report

## Network CSV Path Unavailable

**Situation**: Save shows "Network CSV path failed" warning.

**What it means**:
- Local CSV was created successfully
- Network location (shared drive) is down or disconnected
- Your receive **is** saved to the database
- Labels may not print automatically

**Immediate actions**:
1. **Verify save was successful**: Look for "Saved to database" message
2. **Check local CSV**: Verify file was created on your computer
3. **Label workaround**:
   - Manually copy local CSV to network path when network is back
   - Or email CSV to label system operator
   - Or manually create labels from database records

**Report to IT**:
- Note the time and specific error
- Check if other users have same issue
- IT will verify/fix network path

## System Crashes Mid-Receive

**Situation**: App closes unexpectedly while you're entering loads.

**Recovery steps**:
1. **Restart the application**
2. Navigate back to Receiving module
3. System should show: "Session recovery available"
4. Click "Restore Session"
5. Your data up to the last completed step is recovered
6. **Check carefully**: Review all previously entered data
7. Continue from where you left off

**What's recovered**:
- PO number and part selection
- Number of loads
- All completed load details up to crash point

**What's lost**:
- Anything on the current screen that wasn't saved
- If you were mid-edit on a field, that value is lost

## Receiving Quality Hold Material

**Situation**: Material arrives marked "Quality Hold" or "Inspection Required."

**Steps**:
1. **Do not receive as normal stock**
2. Receiving may have a separate "Quality Hold" workflow:
   - Check with your supervisor
   - May require different PO or special part number suffix
   - Labels must show "QUALITY HOLD" designation
3. **Physical segregation**: Keep quality hold material in designated area
4. **Do not route to production**: It must clear inspection first

**If your system supports it**:
- Look for "Quality Hold" checkbox in receiving
- System flags the receive for QC review
- Labels print with special formatting

## Overweight or Oversized Loads

**Situation**: Load exceeds normal handling limits.

**Steps**:
1. **Note in system**: Enter actual weight even if it's extreme
2. **Package type**: Select "Oversized" or "Special Handling" if available
3. **Alert warehouse**: Notify supervisor that special equipment is needed
4. **Do not split artificially**: One physical pallet = one load record

**Why accurate weight matters**:
- Routing system uses it for forklift assignment
- Incorrect weight can cause safety issues or equipment damage

## Receiving From an Unapproved Supplier

**Situation**: Supplier on packing slip doesn't match PO supplier.

**Steps**:
1. **Stop**—do not receive
2. Verify packing slip and PO carefully
3. Contact purchasing:
   - "PO shows Supplier A, package from Supplier B"
4. Purchasing will investigate:
   - Authorized sub-supplier? (OK to proceed)
   - Vendor error? (hold shipment)
   - Wrong PO? (find correct PO)
5. **Do not bypass**—supplier tracking is critical for quality and compliance

## Damaged or Short Shipment

**Situation**: Boxes are damaged or count doesn't match packing slip.

**Steps**:
1. **Take photos** of damage or shortage
2. **Count what's actually usable** (not what packing slip says)
3. **Receive only what you can verify**:
   - If packing slip says 100 but only 87 are present, receive 87
4. **Notify supervisor and purchasing**:
   - They'll handle supplier claim
   - May need freight carrier claim if damage is shipping-related
5. **Document in system**: Add notes about shortage/damage (if system allows)

**Important**:
- Receiving creates the official count of record
- Insurance claims depend on accurate receiving count
- Don't receive "expected" quantity if it's not there

## Labels Won't Print After Save

**Situation**: Receive saved successfully but no labels appeared.

**Troubleshooting**:
1. **Check CSV files**:
   - Verify local CSV was created (check file timestamp)
   - Verify network CSV (if path is accessible)
2. **Check label printer**:
   - Is it online and ready?
   - Are there print jobs in queue?
   - Does it have paper and ribbon?
3. **Manual reprint**:
   - Locate the CSV file
   - Send to label system manually
   - Or use Edit Mode to reload and re-save (regenerates CSV)

**Escalate to IT if**:
- CSV files are empty or corrupted
- Label system isn't picking up CSVs
- Labels print but data is wrong

## Supplier Sent Wrong Quantity Per Container

**Situation**: PO says 100 pieces per box, but boxes actually have 120.

**Steps**:
1. **Receive what actually arrived**: Enter 120 per load
2. **Notify purchasing**: "PO quantity per package doesn't match actual"
3. **System implications**:
   - Total received quantity will exceed or fall short of PO
   - This creates a variance that purchasing must resolve
4. **Do not adjust to match PO**—accuracy is more important than matching

**Why this matters**:
- Inventory accuracy
- Payment to supplier (are they billing for 100 or 120?)
- Production planning relies on actual quantities
