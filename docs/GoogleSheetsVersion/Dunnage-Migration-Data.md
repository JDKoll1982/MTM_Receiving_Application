# Dunnage Parts – Database Migration Data

> Last Updated: 2026-03-19
> Source: `Dunnage-Types-Reference.md` cross-referenced with live schema
>
> This file contains every row needed to seed the three dunnage master tables.
> Execute inserts **in order**: `dunnage_types` first, then `dunnage_parts`,
> then `dunnage_requires_inventory`.

---

## How This Maps to the Database

```
dunnage_types   ← Step 1 — one row per category section below
     ↓ (type_id FK)
dunnage_parts   ← Step 2 — one row per part variant below
     ↓ (part_id FK, optional)
dunnage_requires_inventory  ← Step 3 — only for items flagged "Yes" in Needs Inventory
```

**Stored procedures to call:**

| Table | SP |
|---|---|
| `dunnage_types` | `sp_dunnage_types_insert(p_type_name, p_icon, p_user, OUT p_new_id)` |
| `dunnage_parts` | `sp_Dunnage_Parts_Insert(p_part_id, p_type_id, p_spec_values, p_home_location, p_user, OUT p_new_id)` |
| `dunnage_requires_inventory` | `sp_Dunnage_Inventory_Insert(p_part_id, p_inventory_method, p_notes, p_user, OUT p_new_id)` |

---

## Step 1 – `dunnage_types` Seed Data

> `icon` values are `MaterialIconKind` names. Verify against the icon picker in Dunnage Admin before inserting.

| # | `type_name` | `icon` |
|---|---|---|
| T1 | Pallets / Skids | `ShippingPallet` |
| T2 | Cardboard Sheets / Slip Sheets | `Layers` |
| T3 | Corrugated Boxes | `PackageVariantClosed` |
| T4 | Gaylords / Bulk Bins | `PackageVariant` |
| T5 | Stretch Film / Shrink Wrap | `Autorenew` |
| T6 | Bags | `BagPersonal` |
| T7 | Tape / Strapping / Banding | `Selection` |
| T8 | Edge Protectors | `ShieldOutline` |
| T9 | Foam / Molded Inserts | `LayersOutline` |
| T10 | Returnable Racks – John Deere | `Warehouse` |
| T11 | Returnable Racks – Other | `Forklift` |
| T12 | Returnable Totes | `BoxVariantClosed` |
| T13 | Returnable Baskets / Wire Containers | `BasketOutline` |

> **Volvo V-EMB Series** is managed by `Module_Volvo` — do **not** create a type row here.

---

## Step 2 – `dunnage_parts` Seed Data

Column key:

| Column | Maps to DB Column |
|---|---|
| `part_id` | `dunnage_parts.part_id` (VARCHAR 50, unique) |
| `type_ref` | Type row # from Step 1 (resolves to `type_id`) |
| `spec_values` | `dunnage_parts.spec_values` (JSON) |
| `home_location` | `dunnage_parts.home_location` (VARCHAR 100, nullable) |
| `source` | Informational — not a DB column |
| `needs_inventory` | If Yes → add row to Step 3 table |

---

### T1 – Pallets / Skids

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 1 | `Pallet 20x20` | T1 | `{"dimensions":"20x20"}` | *(TBD)* | PO'd | No |
| 2 | `Pallet 32x30` | T1 | `{"dimensions":"32x30"}` | *(TBD)* | PO'd | No |
| 3 | `Pallet 40x48` | T1 | `{"dimensions":"40x48"}` | *(TBD)* | PO'd | No |
| 4 | `Pallet 42x42` | T1 | `{"dimensions":"42x42"}` | *(TBD)* | PO'd | No |
| 5 | `Pallet 42x54` | T1 | `{"dimensions":"42x54"}` | *(TBD)* | PO'd | No |
| 6 | `Pallet 45x48` | T1 | `{"dimensions":"45x48"}` | *(TBD)* | PO'd | No |
| 7 | `Pallet 47x52` | T1 | `{"dimensions":"47x52"}` | *(TBD)* | PO'd | No |
| 8 | `Pallet 48x48` | T1 | `{"dimensions":"48x48"}` | *(TBD)* | PO'd | No |
| 9 | `Pallet 52x47` | T1 | `{"dimensions":"52x47"}` | *(TBD)* | PO'd | No |
| 10 | `Pallet 54x42` | T1 | `{"dimensions":"54x42"}` | *(TBD)* | PO'd | No |
| 11 | `Pallet 74x40` | T1 | `{"dimensions":"74x40"}` | *(TBD)* | PO'd | No |
| 12 | `Pallet 85x40` | T1 | `{"dimensions":"85x40"}` | *(TBD)* | PO'd | No |
| 13 | `Pallet 100x40` | T1 | `{"dimensions":"100x40"}` | *(TBD)* | PO'd | No |
| 14 | `Pallet 40x48 John Deere` | T1 | `{"dimensions":"40x48","customer":"John Deere"}` | *(TBD)* | Customer Supplied | **Yes** |
| 15 | `Pallet Styberg` | T1 | `{"customer":"Styberg"}` | *(TBD)* | Customer Supplied | **Yes** |

---

### T2 – Cardboard Sheets / Slip Sheets

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 16 | `Cardboard Sheet 16x38.5` | T2 | `{"dimensions":"16x38.5"}` | *(TBD)* | PO'd | No |
| 17 | `Cardboard Sheet 39x39` | T2 | `{"dimensions":"39x39"}` | *(TBD)* | PO'd | No |
| 18 | `Cardboard Sheet 40x48` | T2 | `{"dimensions":"40x48"}` | Home Location | PO'd | No |
| 19 | `Cardboard Sheet 47x52` | T2 | `{"dimensions":"47x52"}` | *(TBD)* | PO'd | No |
| 20 | `Cardboard Sheet 48x40` | T2 | `{"dimensions":"48x40"}` | *(TBD)* | PO'd | No |
| 21 | `Cardboard Sheet 52x47` | T2 | `{"dimensions":"52x47"}` | *(TBD)* | PO'd | No |
| 22 | `Cardboard Sheet John Deere` | T2 | `{"customer":"John Deere"}` | S-E Racking | Customer Supplied | **Yes** |

---

### T3 – Corrugated Boxes

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 23 | `Corrugated Box 8x5x5` | T3 | `{"length":"8","width":"5","height":"5"}` | *(TBD)* | PO'd | No |
| 24 | `Corrugated Box 12x12x6 Single Wall` | T3 | `{"length":"12","width":"12","height":"6","wall_type":"Single Wall"}` | *(TBD)* | PO'd | No |
| 25 | `Corrugated Box 12x12x6 Double Wall` | T3 | `{"length":"12","width":"12","height":"6","wall_type":"Double Wall"}` | *(TBD)* | PO'd | No |
| 26 | `Corrugated Box 12x12x12 Single Wall` | T3 | `{"length":"12","width":"12","height":"12","wall_type":"Single Wall"}` | *(TBD)* | PO'd | No |
| 27 | `Corrugated Box 12x12x12 Double Wall` | T3 | `{"length":"12","width":"12","height":"12","wall_type":"Double Wall"}` | *(TBD)* | PO'd | No |
| 28 | `Corrugated Box 16x12x8` | T3 | `{"length":"16","width":"12","height":"8"}` | *(TBD)* | PO'd / Customer Supplied | No |
| 29 | `Corrugated Box 24x16x8 Volvo` | T3 | `{"length":"24","width":"16","height":"8","customer":"Volvo"}` | *(TBD)* | Customer Supplied | No |
| 30 | `MPB0000005` | T3 | `{"length":"25","width":"16","height":"18","part_number":"MPB0000005"}` | *(TBD)* | PO'd | No |
| 31 | `Corrugated Box 26x26x26` | T3 | `{"length":"26","width":"26","height":"26"}` | *(TBD)* | PO'd | No |
| 32 | `Corrugated Box 28x16x17` | T3 | `{"length":"28","width":"16","height":"17"}` | *(TBD)* | PO'd | No |
| 33 | `Corrugated Box 32x30x15 Electrolux` | T3 | `{"length":"32","width":"30","height":"15","customer":"Electrolux"}` | Floor - In front of V-N Racking | PO'd | No |
| 34 | `Corrugated Box 32x30x25 Electrolux` | T3 | `{"length":"32","width":"30","height":"25","customer":"Electrolux"}` | *(TBD)* | PO'd | No |
| 35 | `Corrugated Box 42x24x30` | T3 | `{"length":"42","width":"24","height":"30"}` | *(TBD)* | PO'd | No |
| 36 | `Corrugated Box Lennox` | T3 | `{"customer":"Lennox"}` | *(TBD)* | PO'd | No |

---

### T4 – Gaylords / Bulk Bins

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 37 | `Gaylord Short` | T4 | `{"height_type":"Short"}` | *(TBD)* | PO'd | No |
| 38 | `Gaylord Tall` | T4 | `{"height_type":"Tall"}` | *(TBD)* | PO'd | No |

---

### T5 – Stretch Film / Shrink Wrap

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 39 | `Stretch Film Hand-Held` | T5 | `{"application":"Hand-Held"}` | T - Bay | PO'd | No |
| 40 | `Stretch Film Auto-Wrapper` | T5 | `{"application":"Auto-Wrapper"}` | Rack by 100-15 | PO'd | No |

---

### T6 – Bags

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 41 | `Bag 40x48 Lay Flat` | T6 | `{"length":"40","width":"48","mil":"4","style":"Lay Flat"}` | *(TBD)* | PO'd | No |
| 42 | `Bag 40x48 Gaylord` | T6 | `{"length":"40","width":"48","mil":"4","style":"Gaylord"}` | *(TBD)* | PO'd | No |

---

### T7 – Tape / Strapping / Banding

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 43 | `Tape 2 Inch` | T7 | `{"width":"2 inch","material":"Tape"}` | T - Bay | PO'd | No |
| 44 | `Banding 3/4 Inch Steel` | T7 | `{"width":"3/4 inch","material":"Steel","style":"Banding"}` | T - Bay | PO'd | No |
| 45 | `Strapping 1-1/4 Inch Steel` | T7 | `{"width":"1-1/4 inch","material":"Steel","style":"Strapping"}` | T - Bay | PO'd | No |
| 46 | `Banding Nylon` | T7 | `{"material":"Nylon","style":"Banding"}` | T - Bay | PO'd | No |

---

### T8 – Edge Protectors

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 47 | `Edge Protector 3x3x6` | T8 | `{"width":"3","depth":"3","length":"6"}` | *(TBD)* | PO'd | No |

---

### T9 – Foam / Molded Inserts

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 48 | `Genfoam 1` | T9 | `{"type":"Genfoam","variant":"1"}` | *(TBD)* | PO'd | No |
| 49 | `Genfoam 2` | T9 | `{"type":"Genfoam","variant":"2"}` | *(TBD)* | PO'd | No |
| 50 | `Genfoam 3` | T9 | `{"type":"Genfoam","variant":"3"}` | *(TBD)* | PO'd | No |
| 51 | `Genfoam 4` | T9 | `{"type":"Genfoam","variant":"4"}` | *(TBD)* | PO'd | No |
| 52 | `Gen Walls` | T9 | `{"type":"Gen Walls"}` | *(TBD)* | PO'd | No |
| 53 | `MPB0000017 Cover` | T9 | `{"part_family":"MPB0000017","piece":"Top / Cover"}` | *(TBD)* | PO'd | No |
| 54 | `MPB0000017 Box` | T9 | `{"part_family":"MPB0000017","piece":"Bottom / Box"}` | *(TBD)* | PO'd | No |

---

### T10 – Returnable Racks – John Deere

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 55 | `Rack John Deere AKK543` | T10 | `{"rack_number":"AKK543","customer":"John Deere"}` | *(TBD)* | Customer Supplied | **Yes** |
| 56 | `Rack John Deere AKK46582` | T10 | `{"rack_number":"AKK46582","customer":"John Deere"}` | *(TBD)* | Customer Supplied | **Yes** |
| 57 | `Rack John Deere AKK46543` | T10 | `{"rack_number":"AKK46543","customer":"John Deere"}` | *(TBD)* | Customer Supplied | **Yes** |
| 58 | `Rack John Deere AKK546` | T10 | `{"rack_number":"AKK546","customer":"John Deere"}` | *(TBD)* | Customer Supplied | **Yes** |
| 59 | `Rack John Deere AKK419925` | T10 | `{"rack_number":"AKK419925","customer":"John Deere"}` | *(TBD)* | Customer Supplied | **Yes** |

---

### T11 – Returnable Racks – Other

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 60 | `Rack Daimler Jackies` | T11 | `{"customer":"Daimler","style":"Jackies"}` | *(TBD)* | Customer Supplied | **Yes** |
| 61 | `Rack Crenlo` | T11 | `{"customer":"Crenlo"}` | *(TBD)* | Customer Supplied | **Yes** |
| 62 | `Crate MTM` | T11 | `{"owner":"MTM"}` | Outside Door 5 | Returns | **Yes** |

---

### T12 – Returnable Totes

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 63 | `Tote Kawasaki 14x12x7` | T12 | `{"length":"14","width":"12","height":"7","customer":"Kawasaki"}` | WC | Customer Supplied | **Yes** |
| 64 | `Tote Kawasaki 15x12x7` | T12 | `{"length":"15","width":"12","height":"7","customer":"Kawasaki"}` | *(TBD)* | Customer Supplied | **Yes** |
| 65 | `Tote Kawasaki 24x14x7` | T12 | `{"length":"24","width":"14","height":"7","customer":"Kawasaki"}` | *(TBD)* | Customer Supplied | **Yes** |
| 66 | `Tote Allison` | T12 | `{"customer":"Allison"}` | RECV | Customer Supplied | **Yes** |
| 67 | `Tote Ariens` | T12 | `{"customer":"Ariens"}` | *(TBD)* | Customer Supplied | **Yes** |
| 68 | `Tote Lennox` | T12 | `{"customer":"Lennox"}` | *(TBD)* | Customer Supplied | **Yes** |
| 69 | `Tote ZF` | T12 | `{"customer":"ZF"}` | *(TBD)* | Customer Supplied | **Yes** |

---

### T13 – Returnable Baskets / Wire Containers

| # | `part_id` | `type_ref` | `spec_values` | `home_location` | `source` | `needs_inventory` |
|---|---|---|---|---|---|---|
| 70 | `Basket Insinkerator Short` | T13 | `{"customer":"Insinkerator","height_type":"Short"}` | *(TBD)* | Customer Supplied | **Yes** |
| 71 | `Basket Insinkerator Tall` | T13 | `{"customer":"Insinkerator","height_type":"Tall"}` | *(TBD)* | Customer Supplied | **Yes** |
| 72 | `Crate Kohler Short` | T13 | `{"customer":"Kohler","height_type":"Short"}` | *(TBD)* | Customer Supplied | **Yes** |
| 73 | `Crate Kohler Tall` | T13 | `{"customer":"Kohler","height_type":"Tall"}` | *(TBD)* | Customer Supplied | **Yes** |
| 74 | `Crate Kohler Half` | T13 | `{"customer":"Kohler","height_type":"Half"}` | *(TBD)* | Customer Supplied | **Yes** |
| 75 | `Jarkies Lennox` | T13 | `{"customer":"Lennox","style":"Jarkies"}` | *(TBD)* | Customer Supplied | **Yes** |

---

## Step 3 – `dunnage_requires_inventory` Seed Data

Only parts flagged **Yes** in Step 2 need a row here. Default `inventory_method` is `"Adjust In"`.

| Part Row # | `part_id` | `inventory_method` | `notes` |
|---|---|---|---|
| 14 | `Pallet 40x48 John Deere` | Adjust In | Customer-owned returnable |
| 15 | `Pallet Styberg` | Adjust In | Customer-owned returnable |
| 22 | `Cardboard Sheet John Deere` | Adjust In | Customer-supplied cardboard |
| 55 | `Rack John Deere AKK543` | Adjust In | Customer-owned John Deere rack |
| 56 | `Rack John Deere AKK46582` | Adjust In | Customer-owned John Deere rack |
| 57 | `Rack John Deere AKK46543` | Adjust In | Customer-owned John Deere rack |
| 58 | `Rack John Deere AKK546` | Adjust In | Customer-owned John Deere rack |
| 59 | `Rack John Deere AKK419925` | Adjust In | Customer-owned John Deere rack |
| 60 | `Rack Daimler Jackies` | Adjust In | Customer-owned Daimler Jackies |
| 61 | `Rack Crenlo` | Adjust In | Customer-owned Crenlo rack |
| 62 | `Crate MTM` | Adjust In | MTM-owned crates tracked on return |
| 63 | `Tote Kawasaki 14x12x7` | Adjust In | Customer-owned Kawasaki tote |
| 64 | `Tote Kawasaki 15x12x7` | Adjust In | Customer-owned Kawasaki tote |
| 65 | `Tote Kawasaki 24x14x7` | Adjust In | Customer-owned Kawasaki tote |
| 66 | `Tote Allison` | Adjust In | Customer-owned Allison tote |
| 67 | `Tote Ariens` | Adjust In | Customer-owned Ariens tote |
| 68 | `Tote Lennox` | Adjust In | Customer-owned Lennox tote |
| 69 | `Tote ZF` | Adjust In | Customer-owned ZF tote |
| 70 | `Basket Insinkerator Short` | Adjust In | Customer-owned Insinkerator basket |
| 71 | `Basket Insinkerator Tall` | Adjust In | Customer-owned Insinkerator basket |
| 72 | `Crate Kohler Short` | Adjust In | Customer-owned Kohler crate |
| 73 | `Crate Kohler Tall` | Adjust In | Customer-owned Kohler crate |
| 74 | `Crate Kohler Half` | Adjust In | Customer-owned Kohler crate |
| 75 | `Jackies Lennox` | Adjust In | Customer-owned Lennox jackies |

---

## Summary Counts

| Table | Rows to Insert |
|---|---|
| `dunnage_types` | 13 |
| `dunnage_parts` | 75 |
| `dunnage_requires_inventory` | 24 |

---

## Open Items Before Migration

- [ ] Confirm `home_location` values for all *(TBD)* rows — pull from warehouse map or ask floor lead
- [ ] Verify `icon` names against the MaterialIconKind picker in Dunnage Admin (T1–T13)
- [ ] Confirm `MPB0000017-COV` and `MPB0000017-BOX` should be two separate part records, or one
- [ ] Confirm whether `BOX-24X16X8-VOLVO` and `BOX-29-VOLVO` cardboard (row 29) belong here or in Module_Volvo
- [ ] Confirm `CARD-JD` requires inventory — it may be tracked by weight/count rather than by unit
- [ ] Decide if `GAYL-SHORT` / `GAYL-TALL` need further spec detail (dimensions) before insert
- [ ] Confirm spelling: **Jackies** vs **Jarkies** for Daimler and Lennox items (`RACK-DJK`, `JRKY-LNX`)
