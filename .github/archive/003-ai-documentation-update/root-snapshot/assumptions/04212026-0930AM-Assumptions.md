Last Updated: 2026-04-21

# Assumptions Requiring Confirmation

1. Assumption: `volvo_label_data` should stop being the active shipment-header table and should instead become the per-label row table for LabelView generation.
Why this assumption is needed: the current code and schema use `volvo_label_data` as the shipment header table, while your requested behavior describes it as a label-row table containing one row per skid with part-level label payload.
Potential impact if this is wrong: changing this incorrectly would break the current shipment save, pending queue, history, completion, and archive flows across Volvo because several DAOs, stored procedures, and views currently depend on `volvo_label_data` being the shipment header source.
Alternative interpretations considered: keep `volvo_label_data` as shipment headers and create a new dedicated label-row table; keep the current tables and instead store the per-skid label rows in a different existing table; use `volvo_line_data` for per-skid rows and leave `volvo_label_data` as headers.

2. Assumption: the shipment-header concept should remain in the system, but it should move to a different table or schema design once `volvo_label_data` becomes label rows.
Why this assumption is needed: the current Volvo workflow still requires shipment date, shipment number, PO number, receiver number, status, notes, and employee ownership for pending, completed, and history views.
Potential impact if this is wrong: if shipment headers are not preserved somewhere, the current history screen, pending restore, completion flow, and archived shipment detail model will lose their backing records.
Alternative interpretations considered: keep the current shipment header schema untouched and introduce a separate label queue table; flatten shipment-header fields into every label row; redesign the history screens to read only from line-level data.

3. Assumption: `volvo_label_history` should also be repurposed to store archived per-label rows rather than archived shipment headers, because you asked for the new modal to clear the label-data table and move its contents to `volvo_label_history`.
Why this assumption is needed: the current schema and stored procedures archive shipment headers into `volvo_label_history` and shipment lines into `volvo_line_history`, which conflicts with the requested label-row ownership.
Potential impact if this is wrong: repurposing only the active table without repurposing the history table would leave the archive flow inconsistent and could corrupt the current Volvo history feature set.
Alternative interpretations considered: archive new per-label rows into a new `volvo_label_row_history` table while leaving `volvo_label_history` alone; continue using `volvo_label_history` for shipment headers and create a separate label-row history table.

4. Assumption: the new modal should display the live contents of the label-row storage table only, and the existing shipment-entry UI should continue to operate from the current shipment-card data without surfacing those label rows directly in the main page.
Why this assumption is needed: you requested no shipment-entry UI change other than an additional button and a new modal.
Potential impact if this is wrong: if the main page is also supposed to become label-row aware, there is more UI and ViewModel behavior to update than the request currently states.
Alternative interpretations considered: keep the modal as a read-only operational view over generated labels; also expose label-row counts or status on the shipment cards; replace the current shipment cards entirely with the label-row model.

5. Assumption: removing the clear/archive behavior from Complete Shipment means completion should only finalize shipment metadata and shipment lines, while label-row archival becomes exclusively user-driven from the new modal.
Why this assumption is needed: the current completion procedure and history pipeline are tightly coupled to archival behavior.
Potential impact if this is wrong: removing the archive step from completion without a replacement path could leave completed shipment data stranded in active tables or make history inconsistent.
Alternative interpretations considered: keep shipment archive on completion but stop archiving label rows; keep completion behavior unchanged and only add the modal for manual review; move both shipment and label archival to the new modal workflow.

Please confirm, correct, or clarify these assumptions before implementation continues. The current schema evidence that created this ambiguity is in [Database/Database_Deployment/Sql_Files/Schemas/28_Table_volvo_label_data.sql](Database/Database_Deployment/Sql_Files/Schemas/28_Table_volvo_label_data.sql), [Database/Database_Deployment/Sql_Files/Schemas/29_Table_volvo_line_data.sql](Database/Database_Deployment/Sql_Files/Schemas/29_Table_volvo_line_data.sql), [Database/Database_Deployment/Sql_Files/Schemas/31_Table_volvo_label_history.sql](Database/Database_Deployment/Sql_Files/Schemas/31_Table_volvo_label_history.sql), [Database/Database_Deployment/Sql_Files/Schemas/31_Table_volvo_line_history.sql](Database/Database_Deployment/Sql_Files/Schemas/31_Table_volvo_line_history.sql), and [Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_volvo_shipment_complete.sql](Database/Database_Deployment/Sql_Files/StoredProcedures/Volvo/sp_volvo_shipment_complete.sql).