# AI Agent Requirements - MTM Receiving Application

## User Personas
- **Warehouse Manager**: Needs to track incoming material by location and part number
- **Planner**: Requires scheduling information for material runs
- **Procurement Specialist**: Manages PO status and delivery tracking

## Development Phases

### Phase 1: Data Retrieval & Card Generation
- User enters a Location
- System generates a separate card for each unique Part ID in that Location
- Display all locations with >0 quantity of Part Number (formatted as whole numbers)
When the material is schedualed to be ran (the cards need to be sorted in order of closest to run -> furthest away). Use CSV files found in docs\InforVisual\DatabaseCSVFiles to get schema information on how to gather this information
When the matiral is next due to come in and by what PO, the PO returned must be open or firmed, not closed or cancled,
Include the following Information for material upcoming shipments:
1) Material Due to come in in the next 30 Days
2) For POs with multiple lines that use the part number prescribed by the card, combine them into 1 sum (Qty Receivied / Qty Ordered / % Complete)
3) Dates of Upcoming Shipments

Use the following guidelines on how to get the most accurate shipment date for incoming material:
Guidelines:
When material is ordered: What screens/fields indicate the order has been placed, and what information is available at that stage?
When a PO is created on the header there is a Status field.  If that field = Released, that means the PO has been sent to the supplier.  At that point you can see our desired delivery date on the header.  If the PO has multiple lines with different delivery dates that will be in the body of the PO on each line under the RecvDate column.  Once the PO is confirmed, most often we will attach the confirmation to the paper clip at the top of the header.  At that point we will also include a date in the Promise Delivery Date field on the header so we know when the order should arrive.  Jose will also include a sales order number if available in the Sales Order ID field on the header.
When the vendor confirms a delivery date: Where is the confirmed date entered/updated, and does it update at the PO header level, the line level, or both?
The confirmed delivery date is entered in the Promise Delivery Date field on the header.  We typically do not update the dates on the lines within the body of the PO.  Note, if there are multiple delivery dates in the body of the PO on each line, we enter the latest date as our Desired Recv Date on the header.
Blanket/Bulk order delivery dates (for example MMC0000364 and MMC0000366): How are delivery dates handled for blanket/bulk orders, and how do we identify what is expected to arrive on a specific date?
Typically in the FOB field on the header, we will put BLANKET so that we know the order is a blanket order, meaning there will be several deliveries off the order before it is closed.  The Desired Recv Date field has the final delivery date or the date we plan to have all material consumed by.  We do not break out a delivery schedule in the body of the order because we make releases off the planning teams requests.  Each release has different quantities and dates that we do not know at the time of PO creation.
Carrier tracking numbers (UPS, USPS, FedEx): I understand VISUAL does not store tracking numbers, but I want to confirm our current process:
Do we already keep a spreadsheet or log of tracking numbers (UPS/USPS/FedEx), possibly on the Expo drive?  - NO
If we do, where is it located, and who is responsible for updating it?
If we don’t, would it be useful to have a simple tool or shared spreadsheet where tracking numbers can be entered as soon as they’re received (ideally tied back to PO and line numbers)? - NO
 
