Enter Load Info Page:
-----------------------------------------------------------------------
- Part Number: {Part Number}    PO Number: {PONumber}   {Description} -
-                               {divider line}                        -
- Number Of Labels (1-99)                    Location (Optional)      -
- {TextBox}                                  {TextBox}                -
-----------------------------------------------------------------------
* Add PO Number in between Part # and Description
* Add divider line between part number row and 2nd Row
* Change Number of Loads (1-99) text to Number of Labels (1-99)

Enter Weight Page:
-----------------------------------------------------------------------
- Part Number: {Part Number}    PO Number: {PONumber}   {Description} -
-                               {divider line}                        -
- Labels: {TotalLabels} Current Total: {CurrentTotal}  {ReinderText}  -
-----------------------------------------------------------------------
* Add PO Number in between Part # and Description
* Add divider line between part number row and 2nd Row
* Change Loads text in 2nd row to Labels

Enter Heat Page:
-----------------------------------------------------------------------
- Part Number: {Part Number}    PO Number: {PONumber}   {Description} -
-                               {divider line}                        -
- Preset Filters {ComboBox} {FilterButton}           {AutoFillButton} -
-----------------------------------------------------------------------
* Add PO Number in between Part # and Description
* Add divider line between part number row and 2nd Row
* Move Preset Fillers Combobox , filler button to 2nd row (in between 1st row and divider line) Left Justified
** Move Auto-Fill button to 2nd row (in between 1st row and divider line) Right Justified
* Change Loads text in 2nd row to Labels

Enter Package Type Page
-----------------------------------------------------------------------
- Part Number: {Part Number}    PO Number: {PONumber}   {Description} -
-                               {divider line}                        -
-                     {Current 1st row, just pushed down}             -
-----------------------------------------------------------------------
* Add PO Number in between Part # and Description
* Add divider line between part number row and 2nd Row
* Move {Icon} "Package Type" {Combobox} {Textbox} {Checkbox} to bottom row of card.
* Remove "(Applied to all loads)" text from Package Type label, add ":" to the end of Package Type label.


Implementation Status (2026-09-15)
-----------------------------------------------------------------------
Applied to all four step views:
* PO Number added between Part Number and Description (segment hides when there is no PO)
* 1px divider line added between the first row and the second row
* Enter Load Info: header text is now "Number of Labels (1-99)"
* Enter Weight: "Loads:" renamed to "Labels:"
* Enter Heat: Preset Fillers + apply button left justified below the divider, Auto-Fill right justified
* Enter Package Type: card gained the header row + divider; the icon/label/Combo/TextBox/CheckBox row
  moved below the divider and the label reads "Package Type:"

Notes:
* Heat page divider sits above the preset row, matching the sketch. The "Change Loads text in 2nd row
  to Labels" bullet was skipped - that page has no Loads text to rename.
* The step view models gained a PO number property so the new cell binds, and ViewModel_Receiving_
  PackageType gained part id/description because that page had no part identity before.
* Wording stored in settings was updated in three places: the C# defaults, settings.manifest.json,
  and the live settings_universal rows (Database/Scripts/GuidedReceivingCardWording).
* Not changed (out of scope): workflow step titles such as "Receiving - Enter Number of Loads".