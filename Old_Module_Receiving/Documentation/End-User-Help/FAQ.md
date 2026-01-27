# FAQ - Module_Receiving

**Last Updated: 2025-01-15**

Frequently asked questions about receiving operations and the receiving module.

---

## General Questions

### What is receiving?

**A**: Receiving is the process of logging materials that arrive at your facility into the computer system. It creates a record of what came in, how much, and when.

### Why do we need to enter this into the system?

**A**: So that:
- Warehouse knows what materials are available
- Production can plan what to build
- Accounting knows what to pay the supplier for
- Quality can track batches if there's a problem
- Reports show receiving activity

### Can I receive materials without a PO number?

**A**: No. Every receive must have a valid purchase order. If materials arrived without a PO, contact your supervisor—they'll work with purchasing to create one.

---

## Workflow Questions

### Why use the wizard instead of manual entry?

**A**: The wizard guides you through each step and helps prevent mistakes. Use it when:
- You're new to receiving
- The receive is complex (multiple loads, heat/lot tracking)
- You want step-by-step guidance

Use manual entry when:
- You're experienced
- The receive is simple and repetitive
- You need to enter many lines quickly

### What if I make a mistake?

**A**: You have several options:
1. **Before saving**: Click Back to go to previous step and fix
2. **After saving but immediately noticed**: Use Edit Mode to reload and correct
3. **Discovered later**: Use Edit Mode to load from database and update

Don't panic—receives can be edited.

### Can I save partially and come back later?

**A**: Sort of. Your work is saved in a session automatically, so if the app crashes, you can recover. But there's no official "save draft" feature. Best practice is to complete the receive when you have all the information.

### What if I need to stop mid-receive for lunch/break?

**A**: Your session will be preserved. When you come back:
- Don't close the app if possible
- If you do close it, use Session Recovery when you return
- Session stays valid for 24 hours typically

---

## PO and Part Questions

### The PO number I enter says "not found" but I'm looking right at it.

**A**: Try these:
- Remove any dashes or spaces (enter 123456 instead of PO-123456)
- Check for typos (0 vs O, 1 vs I, etc.)
- Try adding/removing leading zeros
- Verify the PO exists in the ERP system (ask supervisor to check)

### PO has 5 parts but only 2 arrived. What do I do?

**A**: On the "Select Parts" screen, check only the 2 parts that actually came. The other 3 will be received later when they arrive.

### Can I receive more than the PO quantity?

**A**: Yes, the system will allow overages. However:
- Make sure the extra quantity actually arrived
- Some facilities require supervisor approval for overages
- Purchasing will need to reconcile with the supplier

### Can I receive less than the PO quantity?

**A**: Yes, this is a partial receive. The system tracks how much of the PO has been received. When the rest arrives, you'll do another receive against the same PO.

---

## Load and Quantity Questions

### What exactly is a "load"?

**A**: A load is a separate physical container or package. Examples:
- 1 pallet = 1 load
- 1 box = 1 load
- 1 crate = 1 load

If your shipment has 3 pallets, enter 3 loads (even if they're all the same part).

### Why does each load need its own label?

**A**: Because warehouse needs to track each container separately as it moves through the facility. Each label has a unique ID.

### All my loads are identical. Do I have to enter details for each one?

**A**: In wizard mode, yes. But you can:
- Use Tab key to move quickly between fields
- Copy/paste values when applicable
- Or switch to Manual Entry mode and bulk-enter identical rows

### What if I don't know the weight?

**A**: Leave it blank unless weight is required by your facility's policy. Many receives work fine without weight data.

### Can quantity be in decimals?

**A**: Depends on the part. If the part's unit of measure is "Each" or "Pieces," use whole numbers. If it's measured in pounds, feet, or liters, decimals are fine.

---

## Heat/Lot Questions

### What is a heat/lot number?

**A**: A batch identifier that tracks which production run the parts came from. Used for quality traceability—if there's a problem, you can identify all parts from that batch.

### When do I need to enter heat/lot?

**A**: If:
- The part is flagged as requiring heat/lot tracking (usually shown on screen)
- The packing slip has a heat/lot number
- Quality or regulatory policy requires it

If none of these apply, you can leave it blank.

### Where do I find the heat/lot number?

**A**: Common places:
- Packing slip
- Stamped or etched on the parts themselves
- Sticker on the container
- Certificate of conformance included with shipment

If you can't find it, check with Quality Control before proceeding.

### What if the heat/lot is unreadable or missing?

**A**: **Do not guess.** Contact Quality Control. They'll decide whether to:
- Accept the receive without it
- Put the material on quality hold
- Reject the shipment

---

## Package Type Questions

### Why does package type matter?

**A**: The warehouse uses it to determine:
- What equipment is needed (forklift vs. hand carry)
- Where to store it (pallet rack vs. shelf)
- How to route it through the facility

### What if my package doesn't fit the dropdown options?

**A**: Pick the closest match. If nothing fits:
- Select "Other" if available
- Ask your supervisor for guidance
- IT can add new package types if needed frequently

### Can one load have multiple package types?

**A**: No. If you have materials in different package types, they're separate loads. Example:
- 2 pallets + 1 box = 3 loads (2 marked as Pallet, 1 as Box)

---

## Saving and CSV Questions

### What does "local CSV" and "network CSV" mean?

**A**:
- **Local CSV**: File saved on your computer (always works)
- **Network CSV**: File saved on shared network drive (for label printers to access)

Your receive saves successfully even if network CSV fails.

### "Network CSV path failed" message—is my receive saved?

**A**: Yes! Your receive is saved to:
- Local computer (CSV file)
- Database (permanent record)

The network CSV is optional. Labels might not print automatically, but IT can fix that.

### Where are the CSV files saved?

**A**: Ask IT for the exact paths. Typically:
- Local: Somewhere in your Documents or AppData folder
- Network: Shared drive (e.g., `\\server\receiving\`)

### Why do we create CSV files?

**A**: The label printing system reads CSV files to know what to print. Each row in the CSV = one label.

---

## Label Questions

### Labels didn't print. What do I do?

**A**: Check:
1. Is the label printer on and ready?
2. Does it have paper and ribbon?
3. Are there error lights on the printer?

If the printer is fine:
- The CSV files were created (check local path)
- IT can manually send CSV to printer
- Or they can reload from database and regenerate

### Wrong label printed (shows old data).

**A**: Likely the label printer grabbed an old CSV file from the queue. Solutions:
- Cancel old print jobs in the queue
- Reprint from the newest CSV file
- Contact IT if labels continue showing wrong data

### How do I know which label goes on which load?

**A**: Labels are numbered—Label 1 matches Load 1, Label 2 matches Load 2, etc.

### Can I reprint labels if they get damaged?

**A**: Yes. Use Edit Mode to reload the receive from database, then save again. This regenerates the CSV and reprints labels.

---

## Edit Mode Questions

### When should I use Edit Mode?

**A**: When you need to:
- Fix a mistake in a completed receive
- Add missing information
- Regenerate labels
- View details of a past receive

### How do I find a specific receive in Edit Mode?

**A**: Search by:
- **PO number** (fastest if you know it)
- **Date range** (find all receives from last week)
- **Part number** (find all receives for a specific part)
- **User** (find your own receives)

### Can I edit receives from months ago?

**A**: Yes, if you have the appropriate permissions. Some facilities restrict editing old receives to supervisors.

### What happens when I save an edited receive?

**A**:
- Database record is updated
- New CSV files are generated (replace old ones)
- Labels can be reprinted if needed

---

## Session and Recovery Questions

### What is a session?

**A**: Your work-in-progress receive. The system saves it automatically so you don't lose data if something goes wrong.

### How do I recover a session after a crash?

**A**:
1. Restart the application
2. Go back to Receiving module
3. You should see "Restore Session" option
4. Click it

Your data up to the last completed step will be restored.

### Session recovery didn't work. Why?

**A**: Possible reasons:
- Session file got corrupted in the crash
- Too much time passed (sessions expire after 24 hours typically)
- Session file was deleted

Check with IT—they might be able to recover from CSV backups.

### Can I access my session from a different computer?

**A**: No. Sessions are saved locally on your computer. You must use the same workstation to recover a session.

---

## Error Messages

### "PO Not Found"

**Cause**: PO doesn't exist in ERP or typo in PO number  
**Fix**: Verify PO number, try variations, check with supervisor

### "Access Denied" or "CSV Path Not Found"

**Cause**: Network path is down or you don't have permissions  
**Fix**: Contact IT. Your receive is still saved to database.

### "Database Connection Failed"

**Cause**: Network issue or database server down  
**Fix**: Wait a moment and try again. Contact IT if it persists.

### "Validation Error: Quantity Required"

**Cause**: Required field was left blank  
**Fix**: Fill in the quantity field (must be a positive number)

### "Quality Hold Part"

**Cause**: Part is flagged in ERP as requiring quality inspection  
**Fix**: Contact Quality Control. Do not receive as normal stock.

---

## Process Questions

### Who can I receive materials for?

**A**: Typically anyone can receive any PO, but some facilities restrict by:
- Department
- User role
- Supplier

Check your facility's policy.

### Do I need approval to receive?

**A**: Usually no. Receiving is considered routine. However:
- Large overages might require supervisor approval
- Quality hold items definitely need QC approval
- High-value items might need special handling

### Can someone else edit my receives?

**A**: Yes, if they have appropriate permissions. Edit Mode allows loading any user's receives.

### How long do receives stay in the system?

**A**: Forever (or until archived per company policy). Database records are permanent for audit and compliance.

---

## Best Practices

### What's the fastest way to receive?

**A**:
- Keep packing slip nearby
- Use keyboard shortcuts (Tab, Enter)
- Manual Entry mode for repetitive receives
- Barcode scanner for PO numbers (if available)

### How can I avoid mistakes?

**A**:
- Double-check PO number before proceeding
- Count physical containers carefully for number of loads
- Review screen before clicking Save
- Verify labels against physical containers

### Should I receive as soon as materials arrive?

**A**: Yes, best practice is to receive materials on the same day they arrive. This keeps inventory accurate and unblocks downstream processes.

### What if I'm unsure about something?

**A**: Ask! Better to ask your supervisor than to guess and enter wrong data. Common things to ask about:
- PO number not clear
- Heat/lot requirements
- Package type selection
- Overages or shortages

---

## Need More Help?

**This FAQ didn't answer your question?**

**For process questions**: Ask your supervisor or receiving team lead  
**For system issues**: Contact IT support  
**For quality concerns**: Contact Quality Control  
**For PO/part questions**: Contact Purchasing

**Additional documentation**:
- **Quick Start**: Step-by-step first receive
- **Daily Tasks**: Detailed workflow instructions
- **Unusual Situations**: Edge cases and special scenarios
- **Common Issues**: Troubleshooting guide
