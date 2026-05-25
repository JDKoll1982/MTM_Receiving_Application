# Customer Pull n' Pack Phase 2 Assumptions

Created: 2026-05-24 06:40 PM

The mock-data foundation for Customer Pull n' Pack is complete. The live Infor Visual query blocker has been resolved using the verified CSV exports under `docs/development/InforVisual/DatabaseCSVFiles`, and the MTM-managed MySQL persistence shape was approved by the user and implemented in the Ship/Rec Tools schema and stored procedures.

## 1. Resolved: The live Infor Visual demand query was authored against `CUSTOMER_ORDER`, `CUST_ORDER_LINE`, `CUST_BOOK_DEL`, `CUSTOMER`, `PART`, and `CR_PART_LOCATION` using the verified CSV exports.

Outcome:
- The CSV exports were provided and verified.
- The live query was added at `Database/InforVisualScripts/Queries/CustomerPullPack/01_GetCustomerPullPackDemand.sql`.
- The startup project still builds after adding the new query resource.

## 2. Resolved: The MTM-managed MySQL persistence layer now uses new Customer Pull n' Pack tables owned by Ship/Rec Tools, with table and procedure names derived from the current task plan.

Outcome:
- Added Ship/Rec-owned schema files for `customer_pull_pack_waitlist`, `customer_pull_pack_waitlist_location`, and `customer_pull_pack_user_defaults`.
- Added `sp_CustomerPullPack_Waitlist_Upsert`, `sp_CustomerPullPack_Waitlist_GetQueue`, and `sp_CustomerPullPack_UserDefaults_Upsert`.
- The waitlist schema enforces one open entry per source line while still allowing historical closed records.
- The queue procedure defaults to Requested, Accepted, and Problem items when no explicit status set is passed.

## 3. Resolved: Phase 2 foundational SQL work can continue on the approved schema direction.

Outcome:
- The mock-data slice, live query slice, and T010 persistence slice are all implemented.
- Startup-project build validation still passes after the SQL additions.

No further confirmation is required for the approved foundational SQL direction recorded in this file.