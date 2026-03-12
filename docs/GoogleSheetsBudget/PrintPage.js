/***************
 * Print Report
 * Creates/refreshes a print-friendly report sheet driven by user inputs.
 *
 * Source data sheet: "01) Budget/Bill Breakdown"
 * Report sheet:      "Print Report"
 ***********************/

const PRINT_REPORT_SHEET_NAME = "Print Report";
const BUDGET_SHEET_NAME = "01) Budget/Bill Breakdown";

// Column indexes in "01) Budget/Bill Breakdown" based on your CSV header:
// A: (blank)
// B: Bill
// C: (blank / checkbox values sometimes)
// D: Description
// E: CC Available
// F: Due Date
// G: Minimum Due
// H: Notes
// I: Check Balance
// J: Checking Balance
const COL_BILL = 2;
const COL_DESC = 4;
const COL_CC_AVAILABLE = 5;
const COL_DUE_DATE = 6;
const COL_MIN_DUE = 7;
const COL_NOTES = 8;
const COL_CHECK_BAL = 9;
const COL_CHECKING_BAL = 10;


/**
 * Main entry point: create the sheet if missing, ensure layout, and refresh report.
 */
function openOrRefreshPrintReport() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const reportSheet = ensurePrintReportSheet_(ss);
  ensurePrintReportLayout_(reportSheet);
  refreshPrintReport_();
  ss.setActiveSheet(reportSheet);
}

/**
 * Creates report sheet if it doesn't exist.
 */
function ensurePrintReportSheet_(ss) {
  let sheet = ss.getSheetByName(PRINT_REPORT_SHEET_NAME);
  if (!sheet) sheet = ss.insertSheet(PRINT_REPORT_SHEET_NAME);
  return sheet;
}

/**
 * Builds/stabilizes the input panel + static layout + styling.
 * This is idempotent: it overwrites the layout area each time.
 *
 * CHANGE: Do NOT auto-fill (or reset) the date input cells (B4/B5).
 * If they're empty, we leave them empty so the user controls them.
 */
function ensurePrintReportLayout_(sheet) {
  // Preserve user inputs BEFORE clearing
  // (Because sheet.clear() wipes the user's chosen dates/options.)
  const priorInputs = {
    start: safeGetDisplayValue_(sheet, "B4"),
    end: safeGetDisplayValue_(sheet, "B5"),
    status: safeGetDisplayValue_(sheet, "B6"),
    filter: safeGetDisplayValue_(sheet, "B7")
  };

  sheet.clear();

  // Title
  sheet.getRange("A1:H1").merge();
  sheet.getRange("A1").setValue("Budget / Bill Breakdown — Print Report");
  sheet.getRange("A1").setFontSize(16).setFontWeight("bold").setHorizontalAlignment("center");

  // Inputs header
  sheet.getRange("A3:H3").merge().setValue("Report Inputs");
  sheet.getRange("A3").setFontWeight("bold");

  // Input labels
  sheet.getRange("A4").setValue("Start Date (MM/DD/YYYY)");
  sheet.getRange("A5").setValue("End Date (MM/DD/YYYY)");
  sheet.getRange("A6").setValue("Include Status");
  sheet.getRange("A7").setValue("Bill Filter (optional contains text)");

  // Restore prior values (do not overwrite with defaults)
  if (priorInputs.start) sheet.getRange("B4").setValue(priorInputs.start);
  if (priorInputs.end) sheet.getRange("B5").setValue(priorInputs.end);

  // Dropdown for status selection
  const statusOptions = ["All", "Paid only", "Pending only"];
  const rule = SpreadsheetApp.newDataValidation()
    .requireValueInList(statusOptions, true)
    .setAllowInvalid(false)
    .build();
  sheet.getRange("B6").setDataValidation(rule);

  // Restore status/filter (with safe defaults only if blank)
  sheet.getRange("B6").setValue(priorInputs.status || "All");
  sheet.getRange("B7").setValue(priorInputs.filter || "");

  // Button-ish instruction cell
  sheet.getRange("D4:H7").merge()
    .setValue("To refresh report:\nSheet Tools → Open/Refresh Print Report")
    .setHorizontalAlignment("center")
    .setVerticalAlignment("middle")
    .setFontWeight("bold");

  // Style input panel
  sheet.getRange("A3:H7").setBackground("#eef3ff");
  sheet.getRange("A4:A7").setFontWeight("bold");
  sheet.getRange("A3:H3").setBackground("#c4c3f7");
  sheet.getRange("A3:H3").setHorizontalAlignment("center");

  sheet.setFrozenRows(8);

  // Report header row (starts at row 9)
  const headerRow = 9;
  const headers = [
    "Bill",
    "Description",
    "Due Date",
    "Minimum Due",
    "CC Available",
    "Status",
    "Notes",
    "Checking Balance"
  ];
  sheet.getRange(headerRow, 1, 1, headers.length).setValues([headers]);

  sheet.getRange(headerRow, 1, 1, headers.length)
    .setFontWeight("bold")
    .setBackground("#26a69a")
    .setFontColor("#ffffff")
    .setHorizontalAlignment("center");

  // Column widths for print friendliness
  sheet.setColumnWidths(1, 1, 180); // Bill
  sheet.setColumnWidths(2, 1, 260); // Description
  sheet.setColumnWidths(3, 1, 110); // Due Date
  sheet.setColumnWidths(4, 1, 110); // Minimum Due
  sheet.setColumnWidths(5, 1, 110); // CC Available
  sheet.setColumnWidths(6, 1, 90);  // Status
  sheet.setColumnWidths(7, 1, 260); // Notes
  sheet.setColumnWidths(8, 1, 140); // Checking Balance

  // Print settings (best-effort)
  try {
    sheet.setHiddenGridlines(true);
    sheet.getRange("A1:H1").setBackground("#ffffff");
  } catch (e) {
    // ignore
  }

  // Border around input panel
  sheet.getRange("A3:H7").setBorder(true, true, true, true, true, true);

  // Thin border on report area (header + space for rows)
  sheet.getRange("A9:H200").setBorder(true, true, true, true, true, true);
}

/**
 * Helper: read a display value safely even if the sheet hasn't been laid out yet.
 */
function safeGetDisplayValue_(sheet, a1) {
  try {
    return String(sheet.getRange(a1).getDisplayValue() || "").trim();
  } catch (e) {
    return "";
  }
}

/**
 * Refreshes the report data based on input cells on the report sheet.
 */
function refreshPrintReport_() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const source = ss.getSheetByName(BUDGET_SHEET_NAME);
  if (!source) throw new Error(`Missing source sheet: "${BUDGET_SHEET_NAME}"`);

  const report = ss.getSheetByName(PRINT_REPORT_SHEET_NAME);
  if (!report) throw new Error(`Missing report sheet: "${PRINT_REPORT_SHEET_NAME}"`);

  // Read inputs
  const startStr = String(report.getRange("B4").getDisplayValue() || "").trim();
  const endStr = String(report.getRange("B5").getDisplayValue() || "").trim();
  const statusMode = String(report.getRange("B6").getDisplayValue() || "All").trim();
  const billFilter = String(report.getRange("B7").getDisplayValue() || "").trim().toLowerCase();

  const startDate = parseDateLoose_(startStr);
  const endDate = parseDateLoose_(endStr);
  if (!startDate || !endDate) {
    throw new Error('Print Report: Start Date / End Date must be valid dates in B4 and B5 (ex: "03/07/2026").');
  }

  // Normalize time range inclusive
  startDate.setHours(0, 0, 0, 0);
  endDate.setHours(23, 59, 59, 999);

  // Load source rows (skip header-ish rows by filtering later)
  const lastRow = source.getLastRow();
  if (lastRow < 2) return;

  const values = source.getRange(1, 1, lastRow, 10).getValues(); // A:J
  const display = source.getRange(1, 1, lastRow, 10).getDisplayValues();

  // Clear old report rows
  const headerRow = 9;
  const outputStart = headerRow + 1;
  report.getRange(outputStart, 1, Math.max(1, report.getMaxRows() - outputStart + 1), 8).clearContent().clearFormat();

  const out = [];
  for (let r = 1; r < values.length; r++) {
    const bill = (display[r][COL_BILL - 1] || "").toString().trim();
    const desc = (display[r][COL_DESC - 1] || "").toString().trim();

    // Skip totally blank lines
    if (!bill && !desc) continue;

    // Skip section headers like "Start of 2024" / date-range rows / "Extra Money Left Over"
    const billLower = bill.toLowerCase();
    if (
      billLower.startsWith("start of") ||
      billLower.includes("extra money left over") ||
      /^\d{1,2}\/\d{1,2}\/\d{4}\s*-\s*\d{1,2}\/\d{1,2}\/\d{4}/.test(bill)
    ) {
      continue;
    }

    // Optional bill filter
    if (billFilter) {
      const hay = (bill + " " + desc).toLowerCase();
      if (!hay.includes(billFilter)) continue;
    }

    const dueDisp = (display[r][COL_DUE_DATE - 1] || "").toString().trim();
    const dueDate = parseDateLoose_(dueDisp);

    // Date filter: include only rows with parsable due date in range
    // (If you want "N/A" included too, tell me and I’ll add an option.)
    if (!dueDate) continue;
    if (dueDate < startDate || dueDate > endDate) continue;

    const minDue = display[r][COL_MIN_DUE - 1];
    const ccAvail = display[r][COL_CC_AVAILABLE - 1];
    const notes = display[r][COL_NOTES - 1];
    const checkingBal = display[r][COL_CHECKING_BAL - 1];

    // Determine status (your sheet uses "Paid"/"Pending"/"N/A" text in Notes or a computed column in other sheets;
    // in this budget sheet, you often write Paid/Pending in Notes column (H) or in a dedicated column depending on layout.
    // We'll interpret "Paid" / "Pending" appearing in the Notes column as status.
    const notesLower = (notes || "").toString().toLowerCase();
    let status = "";
    if (notesLower.includes("paid")) status = "Paid";
    else if (notesLower.includes("pending")) status = "Pending";
    else if (notesLower.includes("n/a")) status = "N/A";

    if (statusMode === "Paid only" && status !== "Paid") continue;
    if (statusMode === "Pending only" && status !== "Pending") continue;

    out.push([bill, desc, dueDisp, minDue, ccAvail, status, notes, checkingBal]);
  }

  // Write rows
  if (out.length) {
    report.getRange(outputStart, 1, out.length, 8).setValues(out);

    // Formatting
    const bodyRange = report.getRange(outputStart, 1, out.length, 8);
    bodyRange.setFontSize(10).setVerticalAlignment("middle");

    // Alignments
    report.getRange(outputStart, 3, out.length, 1).setHorizontalAlignment("center"); // Due Date
    report.getRange(outputStart, 6, out.length, 1).setHorizontalAlignment("center"); // Status
    report.getRange(outputStart, 8, out.length, 1).setHorizontalAlignment("right");  // Checking Bal

    // Alternating row shading
    for (let i = 0; i < out.length; i++) {
      const bg = (i % 2 === 0) ? "#ffffff" : "#ddf2f0";
      report.getRange(outputStart + i, 1, 1, 8).setBackground(bg);
    }

    // Borders
    bodyRange.setBorder(true, true, true, true, true, true);

    // Add a footer summary line
    const footerRow = outputStart + out.length + 1;
    report.getRange(footerRow, 1, 1, 8).merge()
      .setValue(`Rows: ${out.length}   |   Range: ${formatMMDDYYYY_(startDate)} – ${formatMMDDYYYY_(endDate)}`)
      .setFontWeight("bold")
      .setBackground("#8cd3cd");
  } else {
    const msgRow = outputStart;
    report.getRange(msgRow, 1, 1, 8).merge()
      .setValue("No matching rows for the selected inputs.")
      .setHorizontalAlignment("center")
      .setFontWeight("bold")
      .setBackground("#ffe0e0");
  }
}

/**
 * Parses a date from common spreadsheet display formats like:
 * - MM/dd/yyyy
 * - M/d/yyyy
 * - yyyy-MM-dd
 * Returns Date or null.
 */
function parseDateLoose_(val) {
  if (!val) return null;
  const s = String(val).trim();
  if (!s) return null;

  // Try built-in parse first
  const t = Date.parse(s);
  if (!isNaN(t)) return new Date(t);

  // Try MM/dd/yyyy explicitly
  const m = s.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/);
  if (m) return new Date(Number(m[3]), Number(m[1]) - 1, Number(m[2]));

  // Try yyyy-MM-dd explicitly
  const m2 = s.match(/^(\d{4})-(\d{2})-(\d{2})$/);
  if (m2) return new Date(Number(m2[1]), Number(m2[2]) - 1, Number(m2[3]));

  return null;
}

function formatMMDDYYYY_(d) {
  return Utilities.formatDate(d, Session.getScriptTimeZone(), "MM/dd/yyyy");
}
