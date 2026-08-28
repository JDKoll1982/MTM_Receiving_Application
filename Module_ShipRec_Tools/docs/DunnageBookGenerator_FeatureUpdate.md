# Dunnage Book Generator

Last Updated: 2026-08-28

## Overview

The Dunnage Book tool generates a printable 8.5 x 11 portrait "book" of selected dunnage
parts. Employees can include or exclude entire dunnage types or individual parts, add an
optional cover page and table of contents, preview the result in-app, and print or save it
as a PDF through the browser.

## How to Use

1. Open **Ship/Rec Tools** and select **Dunnage Book**.
2. Optionally set the book title and toggle the cover page and table of contents.
3. Use the type filter, search box, and Select all / Clear all to choose the parts to include.
   Checking a type header includes or excludes the entire type.
4. Click **Generate Book** to preview the book.
5. In the preview, click **Print / Save as PDF** to open the browser print dialog.

## Design Decisions

- Reuses the existing `IService_MySQL_Dunnage` read methods; no new stored procedures.
- Cards show the part ID, type, home location, quantity type, and resolved custom UDC fields
  (labels come from `dunnage_custom_fields`).
- Part images are embedded as base64 data URIs; parts without an image use
  `Assets\DunnageBookNoImage.png`. The cover logo uses `Assets\MTMLogo.jpg`.
- The book is a deterministic paged HTML document: each sheet is a fixed `.page` block, so the
  table of contents page numbers and the running `Title — Page X of Y` footer are exact.
- The print pipeline follows the Material Availability Board pattern (HTML to a temp file,
  opened in the browser with `window.print()`).

## Files

- `Module_ShipRec_Tools/Models/Model_Tool_DunnageBook_{Config,FieldValue,Entry,TypeGroup}.cs`
- `Module_ShipRec_Tools/Contracts/IService_Tool_DunnageBook.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_DunnageBook.cs`
- `Module_ShipRec_Tools/Helpers/Helper_ImageDataUri.cs` and `Helper_PrintDocumentHtml.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_DunnageBook.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Dialog_DunnageBookPreview.cs`
- `Module_ShipRec_Tools/Views/View_Tool_DunnageBook.xaml(.cs)`
- `Module_ShipRec_Tools/Dialogs/Dialog_DunnageBookPreview.xaml(.cs)`
- Hub wiring: `Service_ShipRecTools_Navigation`, `ViewModel_ShipRecTools_Main`,
  `View_ShipRecTools_Main`, and `ModuleServicesExtensions`.
- Tests: `MTM_Receiving_Application.Tests/Unit/Module_ShipRec_Tools/{Services,ViewModels,Models}`
