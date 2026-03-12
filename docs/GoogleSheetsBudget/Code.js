function UpdateDate() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var name = ss.getSheetByName("01) Budget/Bill Breakdown");
  var name2 = ss.getSheetByName("08) Drop Down Menus");
  var date = name2.getRange(3, 5).getValue().toString();
  var time = name2.getRange(3, 4).getValue().toString();
  name.getRange(1, 7).setValue(date + " @ " + time);
}

function clearAllFormattingFromSheet(sheetName) {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const sheet = ss.getSheetByName(sheetName);
  if (!sheet) {
    throw new Error('No such sheet: ' + sheetName);
  }
  sheet.getDataRange().clearFormat();
  Logger.log('Cleared all formatting from: ' + sheetName);
}

function runClearFormatting() {
  clearAllFormattingFromSheet("Budget 2025");
}

/**
 * Clears all cells in column E that display a date (even if set by formula),
 * unless the corresponding cell in column B equals "Income: Paycheck".
 */
function clearColumnEDatesUnlessIncomePaycheck() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getSheetByName("01) Budget/Bill Breakdown");
  var lastRow = sheet.getLastRow();
  
  // Get displayed values in columns B and E
  var columnBValues = sheet.getRange(1, 2, lastRow, 1).getDisplayValues();
  var columnEValues = sheet.getRange(1, 5, lastRow, 1).getDisplayValues();

  for (var i = 0; i < lastRow; i++) {
    var valueB = columnBValues[i][0];
    var valueE = columnEValues[i][0];
    // Check if column E displays a date and column B is NOT "Income: Paycheck"
    if (isLikelyDate(valueE) && valueB !== "Income: Paycheck") {
      sheet.getRange(i + 1, 5).clearContent();
    }
  }
}

/**
 * Detects if a string is likely a date (YYYY-MM-DD, MM/DD/YYYY, etc).
 */
function isLikelyDate(val) {
  var parsed = Date.parse(val);
  return val && !isNaN(parsed);
}

/**
 * Adds a custom menu to allow downloading all sheets (tabs) as individual CSV files in a single zipped file.
 * The ZIP will be created in your Google Drive, and a download link will be shown.
 * This version adds Logger.log statements for debugging.
 */
function onOpen() {
  const spreadsheet = SpreadsheetApp.getActive();
  const menuItems = [
    { name: "Sort Sheets A ➜ Z", functionName: "sortSheetsAsc" },
    { name: "Sort Sheets Z ➜ A", functionName: "sortSheetsDesc" },
    { name: "Hide All Sheets", functionName: "hideSheets" },
    { name: "Show All Sheets", functionName: "showSheets" },
    { name: "Randomize Sheet Order 🎲", functionName: "sortSheetsRandom" },
    { name: "Reverse Number", functionName: "toggleSign" },
    null,
    { name: "Open/Refresh Print Report", functionName: "openOrRefreshPrintReport" },
    null,
    { name: "Download All Tabs (CSV ZIP)", functionName: "downloadAllTabsAsZip" }
  ];
  spreadsheet.addMenu("Sheet Tools", menuItems);

  // Create + set up report sheet at startup (safe to run every open)
  openOrRefreshPrintReport();
}

function compileAll2025Expenses() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const yearPrefix = "2025 Expences -";
  const targetSheetName = "Expences 2025 Compilation";

  // Find all sheets with the pattern
  const sheets = ss.getSheets().filter(sheet => sheet.getName().startsWith(yearPrefix));
  Logger.log(`[START] Found ${sheets.length} sheets starting with "${yearPrefix}"`);

  // Prepare or clear the destination
  let targetSheet = ss.getSheetByName(targetSheetName);
  if (!targetSheet) {
    Logger.log("[INIT] Output sheet not found, creating new: " + targetSheetName);
    targetSheet = ss.insertSheet(targetSheetName);
  } else {
    Logger.log("[INIT] Output sheet exists, clearing all contents.");
    targetSheet.clearContents();
  }

  let allRows = [];
  let headerAdded = false;
  let totalAdded = 0, totalProcessed = 0, totalSkipped = 0;

  sheets.forEach(sheet => {
    const sheetName = sheet.getName();
    Logger.log(`[SHEET] Processing: ${sheetName}`);

    // C4:C33 (Expence), F4:F33 (Date), G4:G33 (Catagory), H4:H33 (Cost)
    const expenceVals = sheet.getRange('C4:C33').getValues();
    const dateVals    = sheet.getRange('F4:F33').getValues();
    const catVals     = sheet.getRange('G4:G33').getValues();
    const costVals    = sheet.getRange('H4:H33').getValues();

    // Get header row from C3, F3, G3, H3
    let header = [
      "Source Sheet",
      sheet.getRange("C3").getValue() || "Expence",
      sheet.getRange("F3").getValue() || "Date",
      sheet.getRange("G3").getValue() || "Catagory",
      sheet.getRange("H3").getValue() || "Cost"
    ];
    if (!headerAdded) {
      allRows.push(header);
      headerAdded = true;
      Logger.log(`[HEADER] Set header: ${header.join(", ")}`);
    }

    for (let i = 0; i < expenceVals.length; i++) {
      totalProcessed++;
      const expence  = (expenceVals[i][0] || "").toString().trim();
      // Only output row if Expence/Name not blank
      if (!expence) {
        Logger.log(`[SKIP:${sheetName}] Row ${i+4}: Expence blank.`);
        totalSkipped++;
        continue;
      }

      const date = dateVals[i][0];
      const cat  = catVals[i][0];
      const cost = costVals[i][0];
      allRows.push([sheetName, expence, date, cat, cost]);
      Logger.log(`[ADD:${sheetName}] Row ${i+4}: Added row: [${sheetName}, ${expence}, ${date}, ${cat}, ${cost}]`);
      totalAdded++;
    }
    Logger.log(`[SHEET] Finished: ${sheetName}. Added: ${totalAdded} rows; Skipped: ${totalSkipped} so far.`);
  });

  // Write to output
  if (allRows.length > 1) {
    Logger.log(`[WRITE] Writing ${allRows.length-1} data rows (+header) to "${targetSheetName}".`);
    targetSheet.getRange(1, 1, allRows.length, allRows[0].length).setValues(allRows);
    targetSheet.autoResizeColumns(1, allRows[0].length);
    Logger.log(`[DONE] Wrote ${allRows.length-1} rows from ${sheets.length} sheets to "${targetSheetName}".`);
  } else {
    Logger.log("[DONE] No data found in any 2025 expenses sheets, nothing written.");
  }

  Logger.log(`[SUMMARY] Processed ${totalProcessed} rows, added ${totalAdded}, skipped ${totalSkipped}.`);
}

function createNormalizedVendorTable() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const yearPrefix = "2025 Expences -";
  const outputSheetName = "Normalized Vendors 2025";
  Logger.log("Starting vendor gathering and categorization for 2025...");

  const sheets = ss.getSheets().filter(s => s.getName().startsWith(yearPrefix));
  let allVendorEntries = [];
  const skipPatterns = [
    /^(false|null|n\/a)?$/i,
    /^total/i,
    /^expence grand/i,
    /^remaining/i,
    /^shopping/i,
    /^tax/i,
    /^item$/i,
    /^cc$/i,
    /^paycheck/i,
    /^catagory/i,
    /^amount$/i
  ];

  function norm(str) {
    return str.toLowerCase().replace(/[\s\-._]/g, '').replace(/[^\w]/g, '');
  }

  // Step 1: Collect every (vendor, normalized-vendor, category) sequence from all sheets.
  sheets.forEach((sheet, sheetIndex) => {
    const vendors = sheet.getRange("C4:C33").getValues();
    const categories = sheet.getRange("G4:G33").getValues();
    for (let i = 0; i < vendors.length; i++) {
      const vendor = (vendors[i][0] || '').toString().trim();
      const category = (categories[i][0] || '').toString().trim();
      if (
        !vendor ||
        skipPatterns.some(re => re.test(vendor)) ||
        /^\$|^=/.test(vendor) ||
        vendor.length < 2
      ) continue;
      allVendorEntries.push({
        vendor: vendor,
        norm: norm(vendor),
        category: category
      });
    }
  });

  // Step 2: Build clusters of normalized vendor names, grouping by Levenshtein and initial letter.
  const THRESHOLD = 2;
  let normValues = Array.from(new Set(allVendorEntries.map(e => e.norm)));
  let groups = [];
  let used = Array(normValues.length).fill(false);

  for (let i = 0; i < normValues.length; i++) {
    if (used[i]) continue;
    let groupNorms = [normValues[i]];
    used[i] = true;
    for (let j = i + 1; j < normValues.length; j++) {
      if (used[j]) continue;
      if (
        levenshteinDistance(normValues[i], normValues[j]) <= THRESHOLD &&
        normValues[i][0] === normValues[j][0]
      ) {
        groupNorms.push(normValues[j]);
        used[j] = true;
      }
    }
    groups.push(groupNorms);
  }

  // Step 3: For each group, split by category to allow multiple rows for same normalized cluster but different categories
  // Structure: [{category, names[]}]
  let outputRowsData = [];
  groups.forEach(groupNorms => {
    // All entries in these normalized names
    let entries = allVendorEntries.filter(e => groupNorms.includes(e.norm));
    // Build: mapping {category -> Set(names)}
    let byCategory = {};
    entries.forEach(e => {
      if (!byCategory[e.category]) byCategory[e.category] = new Set();
      byCategory[e.category].add(e.vendor);
    });
    for (let cat in byCategory) {
      // For output: sort names by length then alphabetically, so master is "simplest"
      let nameList = Array.from(byCategory[cat]);
      nameList.sort((a, b) => a.length - b.length || a.localeCompare(b));
      outputRowsData.push({category: cat || "Other", names: nameList});
    }
  });

  let maxSim = Math.max(0, ...outputRowsData.map(g => g.names.length - 1));
  let header = ["Category", "Vendor", "Description", "Master"];
  for (let i = 0; i < maxSim; i++) header.push(`Sim ${i + 1}`);

  // Optional: use Master as first vendor string, Description as same as Master (can be enhanced!)
  let tableRows = [];
  for (let group of outputRowsData) {
    let master = group.names[0];
    let row = [group.category, master.split(" ")[0], master, master];
    for (let i = 1; i < group.names.length; i++) row.push(group.names[i]);
    while (row.length < header.length) row.push("");
    tableRows.push(row);
  }

  let outSheet = ss.getSheetByName(outputSheetName);
  if (!outSheet) outSheet = ss.insertSheet(outputSheetName);
  else outSheet.clearContents();

  outSheet.appendRow(header);
  if (tableRows.length > 0) {
    outSheet.getRange(2, 1, tableRows.length, tableRows[0].length).setValues(tableRows);
    outSheet.autoResizeColumns(1, header.length);
    Logger.log(`Output done: ${tableRows.length} rows written.`);
  } else {
    Logger.log("No data to output.");
  }
}

// --- Levenshtein ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

// --- Levenshtein ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

// --- Levenshtein ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

/**
 * Replace any sim/variant in C4:C33 columns of each 2025 Expences sheet with its master name from the normalization table.
 */
function syncSimColumnsToMaster() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const normSheet = ss.getSheetByName("Normalized Vendors 2025");
  // Load normalization table
  const table = normSheet.getDataRange().getValues();
  // header: Category, Vendor, Description, Master, Sim 1, ...
  let simToMaster = {};
  for (let i = 1; i < table.length; i++) {
    const master = table[i][3];
    if (!master) continue;
    for (let j = 3; j < table[i].length; j++) {
      if (table[i][j] && table[i][j] !== master) {
        simToMaster[table[i][j]] = master;
      }
    }
  }
  const sheets = ss.getSheets().filter(s => s.getName().startsWith("2025 Expences -"));
  sheets.forEach(sheet => {
    const range = sheet.getRange("C4:C33");
    const values = range.getValues();
    let changed = false;
    for (let i = 0; i < values.length; i++) {
      const val = values[i][0];
      if (simToMaster[val]) {
        values[i][0] = simToMaster[val];
        changed = true;
      }
    }
    if (changed) range.setValues(values);
  });
  Logger.log("Synced all sim/variant names in 2025 sheets to master values.");
}

// --- Levenshtein ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

// --- Levenshtein ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

// --- Levenshtein as before ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

// --- Levenshtein as before ---
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, 
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

/**
 * Calculate the Levenshtein distance between two strings.
 */
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  for (var i = 0; i <= blen; i++) matrix[i] = [i];
  for (var j = 0; j <= alen; j++) matrix[0][j] = j;
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, // substitution
          Math.min(matrix[i][j - 1] + 1, matrix[i - 1][j] + 1)
        );
      }
    }
  }
  return matrix[blen][alen];
}

function normalizeVendorName(name, canonicalList, threshold) {
  if (!name || typeof name !== 'string') return ""; // Handle undefined/null/non-string
  name = name.trim();
  for (var i = 0; i < canonicalList.length; i++) {
    if (levenshteinDistance(name.toLowerCase(), canonicalList[i].toLowerCase()) <= threshold) {
      return canonicalList[i];
    }
  }
  canonicalList.push(name);
  return name;
}

/**
 * Calculate the Levenshtein distance between two strings.
 * Lower values = closer match.
 */
function levenshteinDistance(a, b) {
  if (a === b) return 0;
  var alen = a.length, blen = b.length;
  if (alen === 0) return blen;
  if (blen === 0) return alen;
  var matrix = [];
  // increment along the first column of each row
  for (var i = 0; i <= blen; i++) {
    matrix[i] = [i];
  }
  // increment each column in the first row
  for (var j = 0; j <= alen; j++) {
    matrix[0][j] = j;
  }
  // Fill in
  for (var i = 1; i <= blen; i++) {
    for (var j = 1; j <= alen; j++) {
      if (b.charAt(i - 1) == a.charAt(j - 1)) {
        matrix[i][j] = matrix[i - 1][j - 1];
      } else {
        matrix[i][j] = Math.min(
          matrix[i - 1][j - 1] + 1, // substitution
          Math.min(
            matrix[i][j - 1] + 1, // insertion
            matrix[i - 1][j] + 1 // deletion
          )
        );
      }
    }
  }
  return matrix[blen][alen];
}

/**
 * Download all sheets in the spreadsheet as individual CSV files, zipped together.
 * The zip will be saved in your Google Drive and a link will be displayed.
 * Includes debugging logs.
 */
function downloadAllTabsAsZip() {
  try {
    var ss = SpreadsheetApp.getActiveSpreadsheet();
    var sheets = ss.getSheets();
    Logger.log("Found " + sheets.length + " sheets in the spreadsheet.");

    var csvBlobs = [];
    for (var i = 0; i < sheets.length; i++) {
      var sheet = sheets[i];
      Logger.log("Processing sheet: " + sheet.getName());

      var csv = sheetToCsv_(sheet);
      Logger.log("CSV generated for: " + sheet.getName() + " (Length: " + csv.length + ")");

      var sheetName = sheet.getName().replace(/[\\\/:\*\?"<>\|]/g, '_'); // sanitize
      var blob = Utilities.newBlob(csv, 'text/csv', sheetName + '.csv');
      csvBlobs.push(blob);
    }

    Logger.log("Total CSV blobs to zip: " + csvBlobs.length);

    var zippedBlob = Utilities.zip(csvBlobs, ss.getName() + '_sheets.zip');
    Logger.log("ZIP file created. Size: " + zippedBlob.getBytes().length + " bytes.");

    var file = DriveApp.createFile(zippedBlob);
    Logger.log("ZIP file saved to Drive. File ID: " + file.getId());

    var url = file.getUrl();
    Logger.log("ZIP file accessible at: " + url);

    SpreadsheetApp.getUi().alert(
      'All sheets have been exported as CSV and zipped!\n\n' +
      'Find the ZIP file in your Google Drive.\n\n' +
      'Download link: ' + url
    );
  } catch (e) {
    Logger.log("Error in downloadAllTabsAsZip: " + e);
    SpreadsheetApp.getUi().alert('Error exporting sheets: ' + e.message);
  }
}

/**
 * Converts a sheet to CSV format.
 * @param {Sheet} sheet 
 * @returns {string} CSV string
 */
function sheetToCsv_(sheet) {
  try {
    var values = sheet.getDataRange().getValues();
    var formulas = sheet.getDataRange().getFormulas();
    Logger.log("sheetToCsv_: got " + values.length + " rows for sheet: " + sheet.getName());

    return values.map(function(row, i) {
      return row.map(function(cell, j) {
        var formula = formulas[i][j];
        var output = formula && formula !== "" ? formula : cell;
        if (output === null) output = '';
        var str = output.toString();
        // Escape quotes
        str = str.replace(/"/g, '""');
        // If cell contains comma, quote or newline, wrap in quotes
        if (str.search(/("|,|\n)/g) >= 0) {
          str = '"' + str + '"';
        }
        return str;
      }).join(',');
    }).join('\r\n');
  } catch (e) {
    Logger.log("Error in sheetToCsv_ for sheet " + sheet.getName() + ": " + e);
    return '';
  }
}

/**
 * Converts a sheet to CSV format.
 * @param {Sheet} sheet 
 * @returns {string} CSV string
 */
function sheetToCsv_(sheet) {
  var data = sheet.getDataRange().getValues();
  return data.map(row => row.map(cell => {
    if (cell === null) return '';
    var str = cell.toString();
    // Escape quotes
    str = str.replace(/"/g, '""');
    // If cell contains comma, quote or newline, wrap in quotes
    if (str.search(/("|,|\n)/g) >= 0) {
      str = '"' + str + '"';
    }
    return str;
  }).join(',')).join('\r\n');
}

function setB2WithSheetDate() {
  // Get the active sheet
  var sheet = SpreadsheetApp.getActiveSheet();
  var sheetName = sheet.getName();
  
  // Extract the date portion after the last ' - '
  var datePart = sheetName.split(' - ').pop();
  
  // Parse datePart into a Date object (assuming MM/dd/yyyy format)
  var parts = datePart.split('/');
  var dateObj = new Date(parts[2], parts[0] - 1, parts[1]); // year, monthIndex, day

  // Add 13 days
  dateObj.setDate(dateObj.getDate() + 13);

  // Format the new date as MM/dd/yyyy
  var formattedEndDate = Utilities.formatDate(dateObj, Session.getScriptTimeZone(), "MM/dd/yyyy");

  // Set B2 with the desired text
  sheet.getRange('B2').setValue('Expences for ' + datePart + " - " + formattedEndDate);
}

function copyFilteredData2022() {
  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  
  // Get the sheets
  var sourceSheet = ss.getSheetByName('10) 2022 Budget/Bill Breakdown');
  var targetSheet = ss.getSheetByName('All Data');
  
  // Get the data from the source sheet
  var data = sourceSheet.getDataRange().getValues();
  
  // Initialize an array to hold the filtered data
  var filteredData = [];
  
  // Loop through the data and filter rows where Column B, E, and G contain data
  for (var i = 1; i < data.length; i++) {
    if (data[i][1] && data[i][2] && data[i][3] && data[i][2] != "N/A") {
      filteredData.push([data[i][1], data[i][4], data[i][2], data[i][3], data[i][6]]);
    }
  }
  
  // Get the next empty row in the target sheet
  var lastRow = targetSheet.getLastRow();
  var startRow = lastRow + 1;
  
  // Place the filtered data into the target sheet
  if (filteredData.length > 0) {
    targetSheet.getRange(startRow, 1, filteredData.length, filteredData[0].length).setValues(filteredData);
  }
}

function copyFilteredData2023() {
  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  
  // Get the sheets
  var sourceSheet = ss.getSheetByName('11) 2023 Budget/Bill Breakdown');
  var targetSheet = ss.getSheetByName('All Data');
  
  // Get the data from the source sheet
  var data = sourceSheet.getDataRange().getValues();
  
  // Initialize an array to hold the filtered data
  var filteredData = [];
  
  // Loop through the data and filter rows where Column B, E, and G contain data
  for (var i = 1; i < data.length; i++) {
    if (data[i][1] && data[i][4] && data[i][5]) {
      filteredData.push([data[i][1], data[i][2], data[i][3], data[i][4], data[i][5]]);
    }
  }
  
  // Get the next empty row in the target sheet
  var lastRow = targetSheet.getLastRow();
  var startRow = lastRow + 1;
  
  // Place the filtered data into the target sheet
  if (filteredData.length > 0) {
    targetSheet.getRange(startRow, 1, filteredData.length, filteredData[0].length).setValues(filteredData);
  }
}

function copyFilteredData2024() {
  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  
  // Get the sheets
  var sourceSheet = ss.getSheetByName('01) Budget/Bill Breakdown');
  var targetSheet = ss.getSheetByName('All Data');
  
  // Get the data from the source sheet
  var data = sourceSheet.getDataRange().getValues();
  
  // Initialize an array to hold the filtered data
  var filteredData = [];
  
  // Loop through the data and filter rows where Column B, E, and G contain data
  for (var i = 1; i < data.length; i++) {
    if (data[i][1] && data[i][4] && data[i][6]) {
      filteredData.push([data[i][1], data[i][3], data[i][4], data[i][5], data[i][6]]);
    }
  }
  
  // Get the next empty row in the target sheet
  var lastRow = targetSheet.getLastRow();
  var startRow = lastRow + 1;
  
  // Place the filtered data into the target sheet
  if (filteredData.length > 0) {
    targetSheet.getRange(startRow, 1, filteredData.length, filteredData[0].length).setValues(filteredData);
  }
}

function processAllData() {
  console.log("Script started");

  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  console.log("Spreadsheet opened");

  // Get the 'All Data' sheet
  var sheet = ss.getSheetByName('All Data');
  console.log("'All Data' sheet retrieved");

  // Get all data from the sheet
  var data = sheet.getDataRange().getValues();
  console.log("Data retrieved from 'All Data' sheet");

  // Initialize an array to hold the processed data
  var processedData = [];
  var skipRows = false;

  // Loop through the data and process it
  for (var i = 0; i < data.length; i++) {
    var row = data[i];

    // Check if the row should be skipped
    if (skipRows) {
      if (row[0] === 'Catagory') {
        skipRows = false;
      }
      continue;
    }

    // Check if the row matches the first condition
    if (row[0] === 'Cost of One' && row[1] === 'Item' && row[2] === 'Amount' && row[4] === 'Total Cost') {
      skipRows = true;
      continue;
    }

    // Check if the row matches the second condition
    if (row[0] === 'Catagory' && row[1] === 'Expence' && row[2] === 'Date' && row[4] === 'Cost') {
      continue;
    }

    // Add the row to the processed data array
    processedData.push(row);
  }
  console.log("Data processed");



  // Re-add the processed data to the 'All Data' sheet
  if (processedData.length > 0) {
    sheet.getRange(1, 1, processedData.length, processedData[0].length).setValues(processedData);
    console.log("Processed data re-added to 'All Data' sheet");
  } else {
    console.log("No data to re-add to 'All Data' sheet");
  }

  console.log("Script finished");
}

function copyExpensesData() {
  console.log("Script started");

  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  console.log("Spreadsheet opened");

  // Get all sheets
  var sheets = ss.getSheets();
  console.log("All sheets retrieved");

  // Initialize an array to hold the filtered data
  var filteredData = [];
  console.log("Filtered data array initialized");

  // Loop through all sheets
  for (var i = 0; i < sheets.length; i++) {
    var sheet = sheets[i];
    
    // Check if the sheet name contains " Expences - "
    if (sheet.getName().includes(" Expences - ")) {
      console.log("Processing sheet: " + sheet.getName());

      // Get the data from the sheet
      var data = sheet.getDataRange().getValues();
      console.log("Data retrieved from sheet: " + sheet.getName());

      // Loop through the data and filter rows where Column C, F, G, and H contain data
      for (var j = 1; j < data.length; j++) {
        if (data[j][2] && data[j][5] && data[j][6] && data[j][7]) {
          filteredData.push([data[j][6], data[j][2], data[j][5], data[j][7]]);
        }
      }
      console.log("Filtered data from sheet: " + sheet.getName());
    }
  }

  // Reverse the sign of the data in column H if it is a number
  for (var k = 0; k < filteredData.length; k++) {
    if (typeof filteredData[k][3] === 'number') {
      filteredData[k][3] = -filteredData[k][3];
    }
  }
  console.log("Reversed the sign of the data in column H if it is a number");

  // Get the target sheet
  var targetSheet = ss.getSheetByName('All Data');
  console.log("Target sheet 'All Data' retrieved");

  // Get the next empty row in the target sheet
  var lastRow = targetSheet.getLastRow();
  var startRow = lastRow + 1;
  console.log("Next empty row in 'All Data' is: " + startRow);

  // Place the filtered data into the target sheet
  if (filteredData.length > 0) {
    targetSheet.getRange(startRow, 1, filteredData.length, filteredData[0].length).setValues(filteredData);
    console.log("Filtered data placed into 'All Data'");
  } else {
    console.log("No filtered data to place into 'All Data'");
  }

  console.log("Script finished");
}

function addNewMonthToBudget() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var billDataRange = sheet.getRange("A2:J15"); // Adjust the range as per your Bill Data
  var budgetDataRange = sheet.getRange("A17:J100"); // Adjust the range as per your Budget Data

  var billData = billDataRange.getValues();
  var budgetData = budgetDataRange.getValues();

  // Calculate the new month dates
  var lastDate = new Date(budgetData[budgetData.length - 1][0]);
  var newStartDate = new Date(lastDate);
  newStartDate.setMonth(newStartDate.getMonth() + 1);
  var newEndDate = new Date(newStartDate);
  newEndDate.setMonth(newEndDate.getMonth() + 1);
  newEndDate.setDate(newEndDate.getDate() - 1);

  // Append new month Bill Data to Budget Data
  for (var i = 0; i < billData.length; i++) {
    var newRow = [];
    newRow.push(newStartDate.toLocaleDateString()); // Start Date
    newRow.push(newEndDate.toLocaleDateString()); // End Date
    newRow.push(billData[i][1]); // Category
    newRow.push(billData[i][2]); // Keyword
    newRow.push(billData[i][3]); // Bill Type
    newRow.push(billData[i][4]); // Payment Type
    newRow.push(billData[i][5]); // Day of Month
    newRow.push(billData[i][6]); // Amount
    newRow.push(billData[i][7]); // Added
    newRow.push(billData[i][8]); // Added Name

    budgetData.push(newRow);
  }

  // Write the updated Budget Data back to the sheet
  sheet.getRange("A17:J" + (16 + budgetData.length)).setValues(budgetData);
}

function toggleSign() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var range = sheet.getActiveRange();
  var values = range.getValues();

  for (var i = 0; i < values.length; i++) {
    for (var j = 0; j < values[i].length; j++) {
      if (typeof values[i][j] === 'number') {
        values[i][j] = -values[i][j];
      }
    }
  }

  range.setValues(values);
}

function onEdit(e) {
  // Get the active sheet
  var sheet = e.source.getActiveSheet();

  // Check if the active sheet is 'All Data' and the edited cell is H2
  if (sheet.getName() === 'All Data' && e.range.getA1Notation() === 'H2') {
    // Get the value of H2
    var targetValue = e.range.getValue();
    
    // Check if the value in H2 is not empty
    if (targetValue) {
      // Get all data from columns E and F
      var data = sheet.getRange(2, 5, sheet.getLastRow() - 1, 2).getValues();
      
      // Initialize the sum
      var sum = 0;
      
      // Loop through the data and sum the values in column E where column F matches H2
      for (var i = 0; i < data.length; i++) {
        if (data[i][1] === targetValue) {
          sum += parseFloat(data[i][0]) || 0;
        }
      }
      
      // Place the sum in cell H3
      sheet.getRange('H3').setValue(sum);
      

    }
  }
}






function processAllDataSheet() {
  console.log("Script started");

  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  console.log("Spreadsheet opened");

  // Get the 'All Data' sheet
  var sheet = ss.getSheetByName('All Data');
  console.log("'All Data' sheet retrieved");

  // Sort the sheet by column C, only columns A to F, starting at row 2
  var range = sheet.getRange(2, 1, sheet.getLastRow() - 1, 6);
  range.sort({ column: 3, ascending: true });
  console.log("Sheet sorted by column C, only columns A to F, starting at row 2");

  // Get all data from columns A and B starting from row 2
  var data = sheet.getRange(2, 1, sheet.getLastRow() - 1, 2).getValues();
  console.log("Data retrieved from columns A and B starting from row 2");

  // Get all unique filters from column G starting from row 2
  var filters = sheet.getRange(2, 7, sheet.getLastRow() - 1, 1).getValues();
  var uniqueFilters = Array.from(new Set(filters.flat().filter(Boolean)));
  console.log("Unique filters retrieved from column G starting from row 2");

  // Initialize an array to hold the new data for column F
  var newData = [];
  console.log("New data array initialized");

  // Loop through the data and process it
  for (var i = 0; i < data.length; i++) {
    var row = data[i];
    var text = '';
    var colA = row[0] ? row[0].toString().toLowerCase() : '';
    var colB = row[1] ? row[1].toString().toLowerCase() : '';

    for (var j = 0; j < uniqueFilters.length; j++) {
      var filter = uniqueFilters[j].toLowerCase();
      if (colA.includes(filter) || colB.includes(filter) || fuzzyMatchStrings(colA, filter) || fuzzyMatchStrings(colB, filter)) {
        text = uniqueFilters[j];
        break;
      }
    }

    newData.push([text]);
  }
  console.log("Data processed");

  // Place the new data into column F of the 'All Data' sheet starting from row 2
  sheet.getRange(2, 6, newData.length, 1).setValues(newData);
  console.log("New data placed into column F of 'All Data' sheet starting from row 2");

  console.log("Script finished");
}

function fuzzyMatchStrings(input, target) {
  var maxDistance = 2;
  return levenshteinDistance(input, target) <= maxDistance;
}

// Function to calculate the Levenshtein distance between two strings
function levenshteinDistance(a, b) {
  var tmp;
  if (a.length === 0) { return b.length; }
  if (b.length === 0) { return a.length; }
  if (a.length > b.length) { tmp = a; a = b; b = tmp; }

  var i, j, res, alen = a.length, blen = b.length, row = Array(alen);
  for (i = 0; i <= alen; i++) { row[i] = i; }

  for (i = 1; i <= blen; i++) {
    res = i;
    for (j = 1; j <= alen; j++) {
      tmp = row[j - 1];
      row[j - 1] = res;
      res = b[i - 1] === a[j - 1] ? tmp : Math.min(tmp + 1, Math.min(res + 1, row[j] + 1));
    }
  }
  return res;
}





function uniqueItemsToColumnG() {
  console.log("Script started");

  // Open the active spreadsheet
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  console.log("Spreadsheet opened");

  // Get the 'All Data' sheet
  var sheet = ss.getSheetByName('All Data');
  console.log("'All Data' sheet retrieved");

  // Get all data from column F
  var data = sheet.getRange(1, 6, sheet.getLastRow(), 1).getValues();
  console.log("Data retrieved from column F");

  // Create a set to hold unique items
  var uniqueItems = new Set();
  console.log("Unique items set initialized");

  // Loop through the data and add unique items to the set
  for (var i = 0; i < data.length; i++) {
    if (data[i][0]) {
      uniqueItems.add(data[i][0]);
    }
  }
  console.log("Unique items collected");

  // Convert the set to an array
  var uniqueArray = Array.from(uniqueItems).map(item => [item]);
  console.log("Unique items array created");

  // Place the unique items array into column G
  sheet.getRange(1, 7, uniqueArray.length, 1).setValues(uniqueArray);
  console.log("Unique items placed into column G");

  console.log("Script finished");
}


function updateBudgetSheet() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var budgetSheet = ss.getSheetByName("01) Budget/Bill Breakdown");
  var dataSheet = ss.getSheetByName("01) Bill Information");
  var testSheet = ss.getSheetByName("Test Sheet");

  console.log("Starting the updateBudgetSheet function");

  // Get data from sheets
  var billData = dataSheet.getDataRange().getValues();

  console.log("Retrieved data from Bill Information Sheet");

  // Update L1 to L1 + 14 days in the Billing Sheet
  var lastAddedDate = new Date(billData[0][11]);
  lastAddedDate.setDate(lastAddedDate.getDate() + 14);
  dataSheet.getRange("L1").setValue(formatDate(lastAddedDate));
  console.log("Updated Last Added Date to: " + formatDate(lastAddedDate));

  // Determine colors based on L2 value
  var headerColor, color1, color2, footerColor;
  if (billData[1][11] == 2) {
    headerColor = "#26a69a";
    color1 = "#ffffff";
    color2 = "#ddf2f0";
    footerColor = "#8cd3cd";
    dataSheet.getRange("L2").setValue(1);
    console.log("Set colors for Color Code = 2");
  } else {
    headerColor = "#8989eb";
    color1 = "#ffffff";
    color2 = "#e8e7fc";
    footerColor = "#c4c3f7";
    dataSheet.getRange("L2").setValue(2);
    console.log("Set colors for Color Code = 1");
  }

  // Create a new budget array
  var budgetArray = [];

  // Add header row
  budgetArray.push(["", "Category", "", "Keyword", "", "Due Date", "Amount", "Status"]);

  // 1. Update columns D and F
  for (var i = 1; i < billData.length; i++) {
    if (billData[i][0] === true) {
      // Handle column F logic
      if (billData[i][5] == 0) {
        var newDate = new Date(lastAddedDate);
        newDate.setDate(newDate.getDate() + 14);
        billData[i][5] = formatDate(newDate);
      } else if (billData[i][5] < lastAddedDate.getDate()) {
        var newDate = new Date(lastAddedDate);
        newDate.setMonth(newDate.getMonth() + 1);
        newDate.setDate(billData[i][5]);
        billData[i][5] = formatDate(newDate);
      } else {
        var newDate = new Date(lastAddedDate.getFullYear(), lastAddedDate.getMonth(), billData[i][5]);
        billData[i][5] = formatDate(newDate);
      }

      // Add a new row to the budget array
      var newRow = [
        "", // Empty column
        billData[i][1], // Category
        "", // Checkbox column
        billData[i][2], // Keyword
        "", // Empty column
        billData[i][5], // Due Date
        billData[i][6], // Amount
        '=ARRAYFORMULA(IF($C' + (i + 1) + '="","N/A",IF($C' + (i + 1) + '=TRUE,"Paid","Pending")))' // Status formula
      ];
      budgetArray.push(newRow);
    }
  }

  // Find the last row in the budget sheet
  var lastRow = budgetSheet.getLastRow();

  // Add the budget array to the budget sheet starting from the last row
  budgetSheet.getRange(lastRow + 1, 1, budgetArray.length, budgetArray[0].length).setValues(budgetArray);

  // Apply alternating colors (excluding column A)
  for (var i = 0; i < budgetArray.length; i++) {
    var color = (i % 2 === 0) ? color1 : color2;
    budgetSheet.getRange(lastRow + 1 + i, 2, 1, budgetArray[0].length - 1).setBackground(color);
  }

  // Apply header and footer colors
  budgetSheet.getRange(lastRow + 1, 1, 1, budgetArray[0].length).setBackground(headerColor);
  budgetSheet.getRange(lastRow + budgetArray.length, 1, 1, budgetArray[0].length).setBackground(footerColor);

  console.log("Added budget data to Budget Sheet with alternating colors");

  // Add data to "Test Sheet"
  testSheet.clear(); // Clear existing data
  testSheet.getRange(1, 1, billData.length, billData[0].length).setValues(billData);
  console.log("Added bill data to Test Sheet");

  // Add color information to "Test Sheet"
  testSheet.getRange(billData.length + 2, 1).setValue("Header Color: " + headerColor);
  testSheet.getRange(billData.length + 3, 1).setValue("Color 1: " + color1);
  testSheet.getRange(billData.length + 4, 1).setValue("Color 2: " + color2);
  testSheet.getRange(billData.length + 5, 1).setValue("Footer Color: " + footerColor);
  console.log("Added color information to Test Sheet");

  console.log("Finished the updateBudgetSheet function");
}

function getNextMonday(date) {
  var day = date.getDay();
  var diff = (day <= 1) ? 1 - day : 8 - day;
  var nextMonday = new Date(date);
  nextMonday.setDate(date.getDate() + diff);
  return formatDate(nextMonday);
}

function promptUser(message) {
  var ui = SpreadsheetApp.getUi();
  var response = ui.prompt(message);
  var amount = parseFloat(response.getResponseText());
  while (isNaN(amount)) {
    response = ui.prompt("Please enter a valid financial number. " + message);
    amount = parseFloat(response.getResponseText());
  }
  return -Math.abs(amount); // Ensure the amount is negative
}

function formatDate(date) {
  return Utilities.formatDate(date, Session.getScriptTimeZone(), "MM/dd/yyyy");
}























function hideSheets() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var sheetNameArray = [];

  for (var i = 1; i < sheets.length; i++) {
    sheetNameArray.push(sheets[i].getName());
  }

  for( var j = 0; j < sheets.length; j++ ) {
    ss.setActiveSheet(ss.getSheetByName(sheetNameArray[j])).hideSheet();
  }
}

function showSheets() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var sheetNameArray = [];

  for (var i = 1; i < sheets.length; i++) {
    sheetNameArray.push(sheets[i].getName());
  }

  for( var j = 0; j < sheets.length; j++ ) {
    ss.setActiveSheet(ss.getSheetByName(sheetNameArray[j])).showSheet();
  }
}

function sortSheetsAsc() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var sheetNameArray = [];

  for (var i = 0; i < sheets.length; i++) {
    sheetNameArray.push(sheets[i].getName());
  }

  sheetNameArray.sort();

  for( var j = 0; j < sheets.length; j++ ) {
    ss.setActiveSheet(ss.getSheetByName(sheetNameArray[j]));
    ss.moveActiveSheet(j + 1);
  }
}

function sortSheetsDesc() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var sheetNameArray = [];

  for (var i = 0; i < sheets.length; i++) {
    sheetNameArray.push(sheets[i].getName());
  }

  sheetNameArray.sort().reverse();

  for( var j = 0; j < sheets.length; j++ ) {
    ss.setActiveSheet(ss.getSheetByName(sheetNameArray[j]));
    ss.moveActiveSheet(j + 1);
  }
}

function sortSheetsRandom() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = ss.getSheets();
  var sheetNameArray = [];

  for (var i = 0; i < sheets.length; i++) {
    sheetNameArray.push(sheets[i].getName());
  }

  sheetNameArray.sort().sort(() => (Math.random() > .5) ? 1 : -1);;

  for( var j = 0; j < sheets.length; j++ ) {
    ss.setActiveSheet(ss.getSheetByName(sheetNameArray[j]));
    ss.moveActiveSheet(j);
  }
}
