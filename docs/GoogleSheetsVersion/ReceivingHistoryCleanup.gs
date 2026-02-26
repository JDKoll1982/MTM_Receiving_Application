/**
 * MTM Receiving History - Google Sheets Cleanup Script
 *
 * Mirrors the validation and normalisation logic in Import-ReceivingHistory.ps1
 * so problems are fixed (or flagged) before the CSV is exported and imported.
 *
 * Install:
 *   1. Open the "Receiving Data - History" Google Sheet.
 *   2. Extensions > Apps Script > paste this file > Save.
 *   3. Reload the sheet - a new "MTM Receiving" menu will appear.
 *
 * Column layout (1-based, auto-detected from headers):
 *   A=Quantity  B=Material ID  C=PO Number  D=Employee  E=Heat  F=Date  G=Initial Location
 *
 * Resume/batch design:
 *   Dropdowns!C2 stores the next 1-based sheet row to process (2 = start of data).
 *   Each run processes up to BATCH_SIZE rows, writes results, then saves the next
 *   start row back to Dropdowns!C2.
 *   When the last row is reached, C2 is reset to 1 so the next run starts fresh.
 *   Run "Clean & Validate Sheet" repeatedly until it reports "All done!".
 *
 * Per-batch API call count (O(1) regardless of batch size):
 *   Reads  : 4  (headers, values, backgrounds, notes)
 *   Writes : 3  (values, backgrounds, notes)
 *   + 1 write to Dropdowns!C2
 */

// ---------------------------------------------------------------------------
// Constants
// ---------------------------------------------------------------------------

var COL        = { QTY: 1, PART: 2, PO: 3, EMP: 4, HEAT: 5, DATE: 6, LOC: 7 };
var BATCH_SIZE = 1000;

// Rows where the Employee cell matches one of these are repeated header rows
// (same list as $KnownHeaderValues in Import-ReceivingHistory.ps1).
var KNOWN_HEADER_VALUES = ['employee', 'employee number', 'emp', 'emp #', 'emp#', 'label #', 'label#', '#'];

var COLOR_CORRECTED = '#fff2cc'; // yellow - cell was auto-corrected
var COLOR_WARNING   = '#fce5cd'; // orange - row needs human review
var COLOR_ERROR     = '#f4cccc'; // red    - row would be skipped by the importer

// ---------------------------------------------------------------------------
// Menu
// ---------------------------------------------------------------------------

function onOpen() {
  SpreadsheetApp.getUi()
    .createMenu('MTM Receiving')
    .addItem('Clean & Validate Sheet', 'runCleanup')
    .addSeparator()
    .addItem('Reset Progress (start over)', 'resetProgress')
    .addSeparator()
    .addItem('Clear All Highlights', 'clearHighlights')
    .addToUi();
}

// ---------------------------------------------------------------------------
// Main entry point - processes one batch of BATCH_SIZE rows per run
// ---------------------------------------------------------------------------

function runCleanup() {
  var ss      = SpreadsheetApp.getActiveSpreadsheet();
  var sheet   = ss.getActiveSheet();
  var lastRow = sheet.getLastRow();
  var lastCol = sheet.getLastColumn();

  if (lastRow < 2) {
    SpreadsheetApp.getUi().alert('No data rows found (expected headers in row 1).');
    return;
  }

  // -------------------------------------------------------------------------
  // Read resume position from Dropdowns!C2
  //   1 or blank -> start from the beginning (row 2)
  //   N >= 2     -> resume from row N
  // -------------------------------------------------------------------------
  var dropdownsSheet = ss.getSheetByName('Dropdowns');
  if (!dropdownsSheet) {
    SpreadsheetApp.getUi().alert(
      'Cannot find a sheet named "Dropdowns".\n' +
      'Please create one (or rename an existing sheet) and re-run.'
    );
    return;
  }

  var storedVal = dropdownsSheet.getRange('C2').getValue();
  var startRow  = parseInt(storedVal, 10);
  if (isNaN(startRow) || startRow < 2) startRow = 2;

  if (startRow > lastRow) {
    // C2 is beyond the data - treat as complete, reset and tell the user
    dropdownsSheet.getRange('C2').setValue(1);
    SpreadsheetApp.getUi().alert('Nothing to process - sheet already fully cleaned.\nProgress has been reset to row 2 for the next run.');
    return;
  }

  // -------------------------------------------------------------------------
  // Determine the row window for this batch
  // -------------------------------------------------------------------------
  var endRow     = Math.min(startRow + BATCH_SIZE - 1, lastRow);
  var batchCount = endRow - startRow + 1;
  var isLastBatch = (endRow >= lastRow);

  // -------------------------------------------------------------------------
  // BATCH READ
  // -------------------------------------------------------------------------

  // Headers (row 1) - needed to derive column positions
  var headerRow = sheet.getRange(1, 1, 1, lastCol).getValues()[0];
  var headers   = headerRow.map(function(h) { return String(h).trim().toLowerCase(); });
  var col       = deriveColumns(headers);

  // Data window only
  var batchRange = sheet.getRange(startRow, 1, batchCount, lastCol);
  var values     = batchRange.getValues();
  var bgs        = batchRange.getBackgrounds();
  var notes      = batchRange.getNotes();

  // -------------------------------------------------------------------------
  // Process batch in memory - ZERO API calls inside the loop
  // -------------------------------------------------------------------------
  var stats = {
    poCorrected: 0,
    heatCleaned: 0,
    qtyCeiled:   0,
    whitespace:  0,
    headerRows:  0,
    errorRows:   0,
    warningRows: 0
  };

  // 1-based sheet rows to delete; processed after the bulk write.
  var rowsToDelete = [];

  for (var i = 0; i < batchCount; i++) {
    var r        = values[i];    // direct ref - mutations update the array in place
    var sheetRow = startRow + i; // 1-based actual sheet row (used for rowsToDelete)

    var qty    = cellStr(r[col.QTY]);
    var partID = cellStr(r[col.PART]);
    var po     = cellStr(r[col.PO]);
    var emp    = cellStr(r[col.EMP]);
    var heat   = cellStr(r[col.HEAT]);
    var date   = cellStr(r[col.DATE]);

    // -----------------------------------------------------------------------
    // 1. Repeated Google-Sheets header rows - mark for deletion, skip rest
    // -----------------------------------------------------------------------
    if (KNOWN_HEADER_VALUES.indexOf(emp.toLowerCase()) !== -1) {
      rowsToDelete.push(sheetRow);
      stats.headerRows++;
      continue;
    }

    // -----------------------------------------------------------------------
    // 2. Fully blank separator rows - leave untouched
    // -----------------------------------------------------------------------
    if (qty === '' && partID === '' && date === '') continue;

    // -----------------------------------------------------------------------
    // 3. Reset any previous cleanup highlights / notes for this row
    // -----------------------------------------------------------------------
    for (var c = 0; c < lastCol; c++) {
      bgs[i][c]   = null;
      notes[i][c] = '';
    }

    // -----------------------------------------------------------------------
    // 4. Trim whitespace from every string cell in the row
    // -----------------------------------------------------------------------
    for (var c2 = 0; c2 < r.length; c2++) {
      if (typeof r[c2] === 'string') {
        var trimmed = r[c2].trim();
        if (trimmed !== r[c2]) {
          r[c2] = trimmed;
          stats.whitespace++;
        }
      }
    }

    // Re-read after trim so downstream checks use clean values
    po   = cellStr(r[col.PO]);
    heat = cellStr(r[col.HEAT]);

    // -----------------------------------------------------------------------
    // 5. PO number: format to canonical PO-NNNNNN
    // -----------------------------------------------------------------------
    if (po !== '') {
      var formattedPO = formatPoNumber(po);
      if (formattedPO !== po) {
        r[col.PO]      = formattedPO;
        bgs[i][col.PO] = COLOR_CORRECTED;
        stats.poCorrected++;
      }
    }

    // -----------------------------------------------------------------------
    // 6. Heat: blank out literal "NONE"
    // -----------------------------------------------------------------------
    if (heat.toUpperCase() === 'NONE') {
      r[col.HEAT]      = '';
      bgs[i][col.HEAT] = COLOR_CORRECTED;
      stats.heatCleaned++;
    }

    // -----------------------------------------------------------------------
    // 7. Validation
    // -----------------------------------------------------------------------
    var rowErrors   = [];
    var rowWarnings = [];

    // Quantity: accept decimals by rounding up to the nearest integer
    var qtyFloat = parseFloat(qty);
    if (qty === '' || isNaN(qtyFloat) || qtyFloat <= 0) {
      rowErrors.push('Invalid quantity: "' + qty + '"');
    } else {
      var qtyInt = Math.ceil(qtyFloat);
      if (qtyInt !== qtyFloat) {
        r[col.QTY]      = qtyInt;
        bgs[i][col.QTY] = COLOR_CORRECTED;
        stats.qtyCeiled++;
      }
    }

    if (partID === '') {
      rowErrors.push('Blank Material ID');
    } else if (partID.length > 50) {
      rowWarnings.push('Material ID longer than 50 chars - importer will truncate');
    }

    var empInt = parseInt(emp, 10);
    if (emp === '' || isNaN(empInt) || String(empInt) !== emp) {
      rowErrors.push('Non-numeric employee: "' + emp + '"');
    }

    if (date !== '' && !isValidDate(date)) {
      rowErrors.push('Unrecognised date format: "' + date + '"');
    }

    // -----------------------------------------------------------------------
    // 8. Record highlight and note into in-memory arrays
    // -----------------------------------------------------------------------
    if (rowErrors.length > 0) {
      for (var ce = 0; ce < lastCol; ce++) bgs[i][ce] = COLOR_ERROR;
      notes[i][col.QTY] = 'Import will skip this row:\n' + rowErrors.join('\n');
      stats.errorRows++;
    } else if (rowWarnings.length > 0) {
      for (var cw = 0; cw < lastCol; cw++) bgs[i][cw] = COLOR_WARNING;
      notes[i][col.QTY] = 'Review needed:\n' + rowWarnings.join('\n');
      stats.warningRows++;
    }
  }

  // -------------------------------------------------------------------------
  // BATCH WRITE: three calls cover the entire batch window
  // -------------------------------------------------------------------------
  batchRange.setValues(values);
  batchRange.setBackgrounds(bgs);
  batchRange.setNotes(notes);

  // -------------------------------------------------------------------------
  // Delete header rows bottom-to-top so live row indices do not shift mid-loop
  // Each deletion shifts all rows below it up by 1; doing it in reverse order
  // means each deletion targets the correct row number.
  // -------------------------------------------------------------------------
  if (rowsToDelete.length > 0) {
    rowsToDelete.sort(function(a, b) { return b - a; });
    for (var d = 0; d < rowsToDelete.length; d++) {
      sheet.deleteRow(rowsToDelete[d]);
    }
  }

  // -------------------------------------------------------------------------
  // Update Dropdowns!C2 with the next start row (or reset to 1 if done)
  // After deleting N rows within [startRow, endRow], the row that was originally
  // at (endRow + 1) is now at (endRow + 1 - rowsToDelete.length).
  // -------------------------------------------------------------------------
  var nextStart = isLastBatch ? 1 : (endRow + 1 - rowsToDelete.length);
  dropdownsSheet.getRange('C2').setValue(nextStart);

  // -------------------------------------------------------------------------
  // Summary dialog
  // -------------------------------------------------------------------------
  var processedRows = batchCount - rowsToDelete.length;
  var msg;

  if (isLastBatch) {
    msg  = 'All done! Sheet fully cleaned.\n';
    msg += 'Progress has been reset - next run will start from row 2.\n\n';
  } else {
    msg  = 'Batch complete. Run "Clean & Validate Sheet" again to continue.\n';
    msg += 'Next batch starts at sheet row ' + nextStart + '.\n\n';
  }

  msg += '  Rows processed       : ' + processedRows + ' (rows ' + startRow + '-' + endRow + ')\n';
  msg += '  PO numbers formatted : ' + stats.poCorrected + '\n';
  msg += '  Heat "NONE" cleared  : ' + stats.heatCleaned + '\n';
  msg += '  Qty decimals rounded : ' + stats.qtyCeiled   + '\n';
  msg += '  Whitespace trimmed   : ' + stats.whitespace   + ' cells\n';
  msg += '  Header rows removed  : ' + stats.headerRows   + '\n';
  if (stats.errorRows   > 0) msg += '\nRows importer will skip : ' + stats.errorRows;
  if (stats.warningRows > 0) msg += '\nRows with warnings      : ' + stats.warningRows;

  SpreadsheetApp.getUi().alert('MTM Receiving Cleanup', msg, SpreadsheetApp.getUi().ButtonSet.OK);
}

// ---------------------------------------------------------------------------
// resetProgress - manually resets Dropdowns!C2 back to 1
// ---------------------------------------------------------------------------

function resetProgress() {
  var ss             = SpreadsheetApp.getActiveSpreadsheet();
  var dropdownsSheet = ss.getSheetByName('Dropdowns');
  if (!dropdownsSheet) {
    SpreadsheetApp.getUi().alert('Cannot find a sheet named "Dropdowns".');
    return;
  }
  dropdownsSheet.getRange('C2').setValue(1);
  SpreadsheetApp.getUi().alert('Progress reset. Next run will start from row 2.');
}

// ---------------------------------------------------------------------------
// clearHighlights - single-range batch clear
// ---------------------------------------------------------------------------

function clearHighlights() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var last  = sheet.getLastRow();
  if (last < 2) return;

  var range = sheet.getRange(2, 1, last - 1, sheet.getLastColumn());
  range.setBackground(null);
  range.clearNote();

  SpreadsheetApp.getUi().alert('All highlights and notes cleared.');
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/**
 * Safe coercion of a cell value to a trimmed string.
 * Handles Date objects (Google Sheets returns typed Date cells as JS Date),
 * numbers, null, and undefined.
 */
function cellStr(v) {
  if (v === null || v === undefined) return '';
  if (v instanceof Date) {
    var mm = String(v.getMonth() + 1).padStart(2, '0');
    var dd = String(v.getDate()).padStart(2, '0');
    return mm + '/' + dd + '/' + v.getFullYear();
  }
  return String(v).trim();
}

/**
 * Formats a PO number to canonical PO-NNNNNN (6-digit zero-padded).
 * Mirrors Format-PoNumber in Import-ReceivingHistory.ps1.
 *
 *   "66868"      -> "PO-066868"
 *   "64489B"     -> "PO-064489B"   (digits + B suffix)
 *   "64489b"     -> "PO-064489B"   (B uppercased)
 *   "PO-66868"   -> "PO-066868"
 *   "po-66868"   -> "PO-066868"
 *   "PO-064489B" -> "PO-064489B"   (already correct, unchanged)
 *   "MISC-001"   -> "MISC-001"     (unrecognised - returned as-is)
 */
function formatPoNumber(po) {
  var t = po.trim();
  if (t === '') return '';

  var prefixMatch = t.match(/^[Pp][Oo]-(.+)$/);
  if (prefixMatch) {
    var n = prefixMatch[1];
    if (/^\d{1,6}$/.test(n)) {
      return 'PO-' + n.padStart(6, '0');
    }
    var nbMatch = n.match(/^(\d{1,6})([Bb])$/);
    if (nbMatch) {
      return 'PO-' + nbMatch[1].padStart(6, '0') + 'B';
    }
    // Other suffix - normalise prefix casing only
    return 'PO-' + n;
  }

  // No prefix - add it only if the value is a bare number or number+B
  if (/^\d{1,6}$/.test(t)) {
    return 'PO-' + t.padStart(6, '0');
  }
  var bareNbMatch = t.match(/^(\d{1,6})([Bb])$/);
  if (bareNbMatch) {
    return 'PO-' + bareNbMatch[1].padStart(6, '0') + 'B';
  }

  return t; // unrecognised format - return as-is
}

/**
 * Returns true if the string is a valid MM/dd/yyyy date.
 */
function isValidDate(s) {
  var m = s.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
  if (!m) return false;
  var month = parseInt(m[1], 10);
  var day   = parseInt(m[2], 10);
  var year  = parseInt(m[3], 10);
  var d = new Date(year, month - 1, day);
  return d.getFullYear() === year && d.getMonth() === month - 1 && d.getDate() === day;
}

/**
 * Derives 0-based column indices from the actual header row values,
 * with fallback to the default column positions defined in COL.
 */
function deriveColumns(headers) {
  function find(candidates) {
    for (var i = 0; i < headers.length; i++) {
      if (candidates.indexOf(headers[i]) !== -1) return i;
    }
    return -1;
  }
  function col(candidates, defaultOneBased) {
    var found = find(candidates);
    return found !== -1 ? found : defaultOneBased - 1;
  }
  return {
    QTY  : col(['quantity', 'qty'],                              COL.QTY),
    PART : col(['material id', 'material_id', 'part id'],        COL.PART),
    PO   : col(['po number', 'po_number', 'po #', 'po#'],        COL.PO),
    EMP  : col(['employee', 'employee number', 'emp', 'emp #'],  COL.EMP),
    HEAT : col(['heat', 'heat number', 'lot'],                   COL.HEAT),
    DATE : col(['date', 'transaction date'],                     COL.DATE),
    LOC  : col(['initial location', 'location', 'loc'],          COL.LOC)
  };
}
