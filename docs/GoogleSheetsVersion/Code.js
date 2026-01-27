// ===== MAIN TRIGGER FUNCTIONS =====

function onOpen() {
  var ui = SpreadsheetApp.getUi();
  ui.createMenu('MTM Menu')
    .addItem('Show User Guide', 'showUserGuide')
    
      .addItem('Create Material Inventory Sheet', 'createMaterialInventorySheet')
    .addItem('Transfer Expo to History 2025 Sheet', 'copyExpoToHistory')
    .addItem('Transfer Vits to History 2025 Sheet', 'copyVitsToHistory')
    .addSeparator()
    .addItem('Update Column H in Vits Sheet', 'updateVitsHLabels')
    .addItem('Update Column H in Expo Sheet', 'updateExpoHLabels')
    .addItem('Gather Data for Email', 'groupAndPushHistory2025')
    .addSeparator()
    .addItem('Reset Progress Bars', 'resetProgressBars')
    .addItem('Test Progress Bar', 'testProgressBar')
    .addItem('Check History Table', 'checkHistoryTable')
    .addItem('Test Clearing Logic', 'testClearing')
    .adDataTransferObjectsUi();
      var ui2 = SpreadsheetApp.getUi();
  ui2.createMenu('Coil Tools')
      .addItem('Create Material Inventory Sheet', 'createMaterialInventorySheet')
      .addItem('Parse Descriptions', 'parseCoilDescriptions')
      .adDataTransferObjectsUi();
  
  // Batch calls for efficiency
  fillSelectSheet();
  fillSearchBy();
  fillSearchFor();
  autoSort();

  // Get spreadsheet and sheets
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var expoSheet = ss.getSheetByName("Expo");
  var vitsSheet = ss.getSheetByName("Vits");
  
  // === SET DATE FORMATTING FOR COLUMN F (Row 2 and beyond) ===
  if (expoSheet) {
    // Reset checkboxes and progress bar text
    expoSheet.getRange("I2:I4").setValue(false);
    expoSheet.getRange("J2").setValue("Save to History");
    expoSheet.getRange("J3").setValue("Fill Blank Spaces");
    expoSheet.getRange("J4").setValue("Sort for Printing");
    
    // Set date format for Column F (row 2 to max rows) - MM/DD/YYYY style
    var expoLastRow = Math.max(expoSheet.getLastRow(), 1000); // Ensure we cover enough rows
    var expoDateRange = expoSheet.getRange(2, 6, expoLastRow - 1, 1); // Column F from row 2 onwards
    expoDateRange.setNumberFormat("MM/DD/YYYY");
    Logger.log("Expo Sheet: Set date format MM/DD/YYYY for F2:F" + expoLastRow);
  }
  
  if (vitsSheet) {
    // Reset checkboxes and progress bar text
    vitsSheet.getRange("I2:I4").setValue(false);
    vitsSheet.getRange("J2").setValue("Save to History");
    vitsSheet.getRange("J3").setValue("Fill Blank Spaces");
    vitsSheet.getRange("J4").setValue("Sort for Printing");
    
    // Set date format for Column F (row 2 to max rows) - MM/DD/YYYY style
    var vitsLastRow = Math.max(vitsSheet.getLastRow(), 1000); // Ensure we cover enough rows
    var vitsDateRange = vitsSheet.getRange(2, 6, vitsLastRow - 1, 1); // Column F from row 2 onwards
    vitsDateRange.setNumberFormat("MM/DD/YYYY");
    Logger.log("Vits Sheet: Set date format MM/DD/YYYY for F2:F" + vitsLastRow);
  }
  
  // Force display update
  SpreadsheetApp.flush();
  
  Logger.log("OnOpen completed at 2025-08-25 17:42:01 - Date formatting applied to both sheets");
}

        function autoSort() {
          var ss = SpreadsheetApp.getActiveSpreadsheet();
          var sheet = ss.getSheetByName("History 2025"); // Replace "Sheet1" with your sheet name
          var range = sheet.getRange("A2:G"); // Or specify a smaller range like sheet.getRange("A1:Z")
          range.sort([{column: 6, ascending: false}]); // Sorts by column 2 (B), descending
        }

function showUserGuide() {
  try {
    // This creates the HTML output from your UserGuide.html file
    var htmlContent = HtmlService.createHtmlOutputFromFile('UserGuide')
      .setWidth(1200)
      .setHeight(700)
      .setTitle('MTM Inventory System - User Guide');
    
    // Show it as a modal dialog
    SpreadsheetApp.getUi().showModalDialog(htmlContent, 'MTM User Guide - Last Updated: 2025-08-25 17:59:58');
    
    Logger.log("User Guide displayed successfully at 2025-08-25 17:59:58 UTC for user: Dorotel");
    
  } catch (error) {
    Logger.log("Error displaying User Guide: " + error.toString());
    SpreadsheetApp.getUi().alert("Error loading User Guide. Please contact support.");
  }
}

function createMaterialInventorySheet() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  
  // Delete existing sheet if present
  var existingSheet = ss.getSheetByName('MaterialInventory');
  if (existingSheet) {
    ss.deleteSheet(existingSheet);
  }
  
  // Create new sheet
  var sheet = ss.insertSheet('MaterialInventory');
  
  // Set headers
  var headers = ['Part ID', 'Material Type', 'Gauge/Thickness', 'Width (in)', 'Length (in)', 'Stock UM', 
                 'On Hand (lbs)', 'Thickness (inches)', 'Calculated Volume (in³)', 
                 'Calculated Weight Total (lbs)', 'Calculated Sheets (96")', 'Density (lb/in³)', 'Notes'];
  sheet.getRange(1, 1, 1, headers.length).setValues([headers]);
  sheet.getRange(1, 1, 1, headers.length).setFontWeight('bold').setBackground('#4285f4').setFontColor('white');
  
  // Get data from CoilInformation
  var coilSheet = ss.getSheetByName('CoilInformation');
  var lastRow = coilSheet.getLastRow();
  var sourceData = coilSheet.getRange('A2:J' + lastRow).getValues(); // Get all columns we need
  
  // Process and filter data
  var validRows = [];
  
  for (var i = 0; i < sourceData.length; i++) {
    var rowNum = i + 2; // Actual row number in sheet
    var partId = sourceData[i][0];
    var description = sourceData[i][1];
    var stockUM = sourceData[i][2];
    var onHand = sourceData[i][5];
    
    // Skip if Part ID is empty/null
    if (!partId || partId === '' || partId === null) continue;
    
    // Skip if Description is empty/null
    if (!description || description === '' || description === null) continue;
    
    // Parse the description
    var parsed = parseDescriptionForSheet(partId, description);
    
    // Skip if we couldn't extract basic dimensions
    if (!parsed.width || parsed.width === '' || parsed.width === 0) continue;
    if (!parsed.thickness || parsed.thickness === '' || parsed.thickness === 0) continue;
    
    // Determine density
    var density = parsed.materialType === 'Steel/Galvanized' ? 0.284 : 0.098;
    
    // Calculate volume
    var volume = '';
    var calculatedWeight = '';
    var calculatedSheets = '';
    var notes = '';
    
    // For MMF/MMFCS items (sheets with defined length)
    if (parsed.length && parsed.length > 0) {
      volume = parsed.thickness * parsed.width * parsed.length;
      calculatedWeight = volume * density;
      calculatedSheets = Math.round((parsed.length / 96) * 10) / 10; // Round to 1 decimal
      notes = 'Sheet - Has defined length';
    } 
    // For MMC items (coils - calculate from weight)
    else if (onHand && onHand > 0) {
      volume = onHand / density; // Total volume of coil
      calculatedWeight = onHand;
      // Calculate coil length then sheets
      var coilLength = volume / (parsed.thickness * parsed.width);
      calculatedSheets = Math.round((coilLength / 96) * 10) / 10;
      notes = 'Coil - Length calculated from weight';
    } else {
      // Skip rows with no on-hand quantity and no length
      continue;
    }
    
    // Build the row
    validRows.push([
      partId,
      parsed.materialType,
      parsed.gaugeOrThickness,
      parsed.width,
      parsed.length || '',
      stockUM,
      onHand || 0,
      parsed.thickness,
      volume,
      calculatedWeight,
      calculatedSheets,
      density,
      notes
    ]);
  }
  
  // Write valid rows to sheet
  if (validRows.length > 0) {
    sheet.getRange(2, 1, validRows.length, 13).setValues(validRows);
    
    // Format numbers
    sheet.getRange(2, 8, validRows.length, 1).setNumberFormat('0.0000');  // Thickness
    sheet.getRange(2, 5, validRows.length, 1).setNumberFormat('#,##0.0');  // Length
    sheet.getRange(2, 4, validRows.length, 1).setNumberFormat('#,##0.0');  // Width
    sheet.getRange(2, 9, validRows.length, 1).setNumberFormat('#,##0.00');  // Volume
    sheet.getRange(2, 10, validRows.length, 1).setNumberFormat('#,##0.00');  // Weight
    sheet.getRange(2, 11, validRows.length, 1).setNumberFormat('#,##0.0');  // Sheets available
    sheet.getRange(2, 12, validRows.length, 1).setNumberFormat('0.000');  // Density
    
    // Conditional formatting for MMF items (green)
    var mmfRule = SpreadsheetApp.newConditionalFormatRule()
      .whenFormulaSatisfied('=OR(REGEXMATCH($A2,"^MMF"),REGEXMATCH($A2,"^MMFCS"))')
      .setBackground('#e8f5e9')
      .setRanges([sheet.getRange(2, 1, validRows.length, 13)])
      .build();
    var rules = sheet.getConditionalFormatRules();
    rules.push(mmfRule);
    sheet.setConditionalFormatRules(rules);
  }
  
  // Format
  sheet.autoResizeColumns(1, 13);
  sheet.setFrozenRows(1);
  
  SpreadsheetApp.getUi().alert('MaterialInventory sheet created!\n\n' +
    'Total valid rows: ' + validRows.length + '\n' +
    'MMF/MMFCS items (sheets) are highlighted in green.\n' +
    'Rows with missing Part ID, Description, or dimensions were excluded.');
}

function parseDescriptionForSheet(partId, description) {
  var result = {
    materialType: '',
    gaugeOrThickness: '',
    thickness: 0,
    width: 0,
    length: 0
  };
  
  // Extract gauge (e.g., "14Ga", "22Ga")
  var gaugeMatch = description.match(/(\d+)Ga/i);
  if (gaugeMatch) {
    result.gaugeOrThickness = gaugeMatch[1] + 'Ga';
    result.thickness = parseFloat(gaugeToThickness(gaugeMatch[1])) || 0;
    result.materialType = 'Steel/Galvanized';
  } else {
    // Extract decimal thickness (e.g., ".250", "(.375)", "0.125")
    var thicknessMatch = description.match(/\(?\.\d+\)?|\(\d+\.\d+\)/);
    if (thicknessMatch) {
      var thicknessStr = thicknessMatch[0].replace(/[()]/g, '');
      result.thickness = parseFloat(thicknessStr) || 0;
      result.gaugeOrThickness = thicknessStr;
      result.materialType = 'Aluminum';
    }
  }
  
  // Extract dimensions - width and length
  var dimensionMatches = description.match(/X\s+(\d+\.?\d*)/g);
  if (dimensionMatches && dimensionMatches.length >= 1) {
    result.width = parseFloat(dimensionMatches[0].replace(/X\s+/, '')) || 0;
    
    // Check if this is MMF/MMFCS (has length)
    if ((partId.indexOf('MMF') === 0 || partId.indexOf('MMFCS') === 0) && dimensionMatches.length >= 2) {
      result.length = parseFloat(dimensionMatches[1].replace(/X\s+/, '')) || 0;
    }
  }
  
  return result;
}

function gaugeToThickness(gauge) {
  var gaugeTable = {
    '30': '0.0120', '29': '0.0140', '28': '0.0149', '27': '0.0164',
    '26': '0.0179', '25': '0.0209', '24': '0.0239', '23': '0.0269',
    '22': '0.0299', '21': '0.0329', '20': '0.0359', '19': '0.0418',
    '18': '0.0478', '17': '0.0538', '16': '0.0598', '15': '0.0673',
    '14': '0.0747', '13': '0.0897', '12': '0.1046', '11': '0.1196',
    '10': '0.1345', '9': '0.1495', '8': '0.1644', '7': '0.1793',
    '6': '0.1943', '5': '0.2092', '4': '0.2242'
  };
  
  return gaugeTable[gauge] || '';
}


function parseCoilDescriptions() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sourceSheet = ss.getSheetByName('CoilInformation');
  var targetSheet = ss.getSheetByName('Parsed Data') || ss.insertSheet('Parsed Data');
  
  // Get all data from source sheet
  var lastRow = sourceSheet.getLastRow();
  var data = sourceSheet.getRange('A2:B' + lastRow).getValues();
  
  // Create headers in target sheet
  var headers = ['Part ID', 'Original Description', 'Item Type', 'Material Type', 'Gauge', 'Thickness (inches)', 'Width (inches)', 'Length (inches)'];
  targetSheet.getRange(1, 1, 1, headers.length).setValues([headers]);
  targetSheet.getRange(1, 1, 1, headers.length).setFontWeight('bold');
  
  var parsedData = [];
  
  for (var i = 0; i < data.length; i++) {
    var partId = data[i][0];
    var description = data[i][1];
    
    // Skip null/empty rows
    if (!partId || !description || partId === '' || description === '') continue;
    
    var parsed = parseDescriptionDetailed(partId, description);
    
    // Skip if we couldn't parse basic info
    if (!parsed.thickness || !parsed.width) continue;
    
    parsedData.push([
      partId,
      description,
      parsed.itemType,
      parsed.materialType,
      parsed.gauge,
      parsed.thickness,
      parsed.width,
      parsed.length
    ]);
  }
  
  // Write parsed data to target sheet
  if (parsedData.length > 0) {
    targetSheet.getRange(2, 1, parsedData.length, headers.length).setValues(parsedData);
  }
  
  // Format the sheet
  targetSheet.autoResizeColumns(1, headers.length);
  targetSheet.setFrozenRows(1);
  
  SpreadsheetApp.getUi().alert('Parsing complete!\n' + parsedData.length + ' valid rows processed.\nRows with missing data were excluded.');
}

function parseDescriptionDetailed(partId, description) {
  var result = {
    itemType: '',
    materialType: '',
    gauge: '',
    thickness: '',
    width: '',
    length: ''
  };
  
  // Determine if Sheet, Coil, Strip, etc.
  if (description.indexOf('Sheet') !== -1) result.itemType = 'Sheet';
  else if (description.indexOf('Coil') !== -1) result.itemType = 'Coil';
  else if (description.indexOf('Strip') !== -1) result.itemType = 'Strip';
  else if (description.indexOf('Blank') !== -1) result.itemType = 'Blank';
  else if (description.indexOf('Plate') !== -1) result.itemType = 'Plate';
  else result.itemType = 'Unknown';
  
  // Extract gauge (e.g., "14Ga", "22Ga")
  var gaugeMatch = description.match(/(\d+)Ga/i);
  if (gaugeMatch) {
    result.gauge = gaugeMatch[1] + 'Ga';
    result.thickness = gaugeToThickness(gaugeMatch[1]);
    result.materialType = 'Steel/Galvanized';
  } else {
    // Extract decimal thickness (e.g., ".250", "(.375)", "0.125")
    var thicknessMatch = description.match(/\(?\.\d+\)?|\(?\d+\.\d+\)?/);
    if (thicknessMatch) {
      result.thickness = thicknessMatch[0].replace(/[()]/g, '');
      result.materialType = 'Aluminum';
    }
  }
  
  // Extract width and length
  var dimensionMatches = description.match(/X\s+(\d+\.?\d*)/g);
  if (dimensionMatches && dimensionMatches.length >= 1) {
    result.width = dimensionMatches[0].replace(/X\s+/, '');
    if (dimensionMatches.length >= 2) {
      result.length = dimensionMatches[1].replace(/X\s+/, '');
    }
  }
  
  return result;
}



function onEdit(e) {
  var sheet = e.source.getActiveSheet();
  var sheetName = sheet.getName();
  var range = e.range;
  var row = range.getRow();
  var editedCol = range.getColumn();

  // ====== MAIN TRIGGERS FOR CHECKBOXES (unchanged) ======
  if ((sheetName === "Expo" || sheetName === "Vits") &&
      editedCol === 9 && (row === 2 || row === 3 || row === 4)) {
    var val = range.getValue();
    if (row === 2 && val === true) {
      if (sheetName === "Expo") {
        copyExpoToHistory();
      } else if (sheetName === "Vits") {
        copyVitsToHistory();
      }
      return;
    }
    if (row === 3 && val === true) {
      sheet.getRange("J3").setValue("Fill Blank Spaces");
      autoFill();
      range.setValue(false);
      return;
    }
    if (row === 4 && val === true) {
      var lastRow = sheet.getLastRow();
      if (lastRow >= 2) {
        sheet.getRange("J4").setValue("Sort for Printing");
        if (sheetName === "Vits") {
          sortVitsByBEA();
        } else if (sheetName === "Expo") {
          sortExpoByBEA();
        }
        sheet.getRange(4, 9).setValue(false);
      }
      return;
    }
  }

  // ====== YOUR NEW LOGIC FOR EXPO SHEET EDITS ======
  if ((sheetName === "Expo" || sheetName === "Vits") && editedCol === 2 && row >= 2) {
    var partNumberValue = range.getValue();
    var partNumberText = partNumberValue ? partNumberValue.toString() : "";
    var isRestrictedPart = /MMFSR|MMCSR/i.test(partNumberText);
    var rowRange = sheet.getRange(row, 1, 1, 8);
    if (isRestrictedPart) {
      SpreadsheetApp.getUi().alert(
        "Quality Hold Required",
        "You must contact quality immediately and quality MUST accept the load before any paperwork is signed.",
        SpreadsheetApp.getUi().ButtonSet.OK
      );
      rowRange.setFontColor("red");
    } else {
      rowRange.setFontColor("black");
    }
  }


  // ====== Old logic (OPTIONAL: you can keep or remove) ======
  if (sheetName === "Expo" || sheetName === "History" || sheetName === "Vits") {
    handleEdit(sheet, sheetName, row);
  }
  if (sheetName === "Search" && (range.getA1Notation() === "E1" || range.getA1Notation() === "C1")) {
    fillSearchFor();
  }
  padMMCMMFCode(e);
  formatPOColumn(e);
  updateSummaryTables(sheetName);

  // ====== FINAL: Always update H column (labels) if Expo ======
  if (sheetName === "Expo") {
    updateHLabels(sheet);
  }
}

function checkHistoryTable() {
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var currentYear = new Date().getFullYear();
  var historySheet = spreadsheet.getSheetByName("History " + currentYear);
  
  if (!historySheet) {
    historySheet = spreadsheet.getSheetByName("History " + (currentYear - 1));
  }
  
  if (!historySheet) {
    Logger.log("No history sheet found!");
    SpreadsheetApp.getUi().alert("No history sheet found!");
    return;
  }
  
  Logger.log("History sheet name: " + historySheet.getName());
  Logger.log("History sheet last row: " + historySheet.getLastRow());
  Logger.log("History sheet last column: " + historySheet.getLastColumn());
  
  // Check header row
  if (historySheet.getLastRow() >= 1) {
    var headers = historySheet.getRange(1, 1, 1, 7).getValues()[0];
    Logger.log("Headers: " + headers.join(", "));
  }
  
  // Check for any existing data
  if (historySheet.getLastRow() >= 2) {
    var sampleData = historySheet.getRange(2, 1, Math.min(3, historySheet.getLastRow() - 1), 7).getValues();
    Logger.log("Sample data rows: " + sampleData.length);
    for (var i = 0; i < sampleData.length; i++) {
      Logger.log("Row " + (i + 2) + ": " + sampleData[i].join(", "));
    }
  }
  
  // Test writing a single row to see if there are permission issues
  try {
    var testRow = ["TEST", "TEST", "TEST", "Dorotel", "TEST", "2025-08-25 17:03:54", "TEST"];
    var lastRow = historySheet.getLastRow();
    var testRange = historySheet.getRange(lastRow + 1, 1, 1, 7);
    Logger.log("Test range: " + testRange.getA1Notation());
    
    testRange.setValues([testRow]);
    Logger.log("✅ Test write successful!");
    
    // Clean up test row
    testRange.clearContent();
    Logger.log("✅ Test cleanup successful!");
    
  } catch (error) {
    Logger.log("❌ Test write failed: " + error.toString());
  }
  
  SpreadsheetApp.getUi().alert("Diagnostic complete - check execution log for details");
}

// ===== AUTOFILL FUNCTION =====

function autoFill() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var sheetName = sheet.getName();
  if (sheetName !== "Expo" && sheetName !== "Vits") {
    return;
  }

  // Start progress tracking in J3
  updateProgressBar(sheet, "Starting...", "fill");

  var columnsToFill = [1, 2, 3, 4, 5, 6, 7, 8];
  var lastRow = sheet.getLastRow();
  if (lastRow < 3) {
    updateProgressBar(sheet, "No data", "fill");
    Utilities.sleep(250);
    updateProgressBar(sheet, "Fill Blank Spaces", "fill");
    return;
  }

  updateProgressBar(sheet, "Reading...", "fill");

  var colCount = Math.max(...columnsToFill);
  var data = sheet.getRange(1, 1, lastRow, colCount).getValues();

  var lastDataRow = 1;
  for (var row = data.length - 1; row >= 0; row--) {
    if (data[row].some(cell => cell !== "" && cell !== null)) {
      lastDataRow = row + 1;
      break;
    }
  }
  if (lastDataRow < 3) {
    updateProgressBar(sheet, "No data", "fill");
    Utilities.sleep(250);
    updateProgressBar(sheet, "Fill Blank Spaces", "fill");
    return;
  }

  updateProgressBar(sheet, "Preparing...", "fill");

  var valueDFromK1 = sheet.getRange("K1").getValue();
  var currentDate = "2025-08-25"; // Current date only

  updateProgressBar(sheet, "Filling...", "fill");

  columnsToFill.forEach(function(colNum, index) {
    // Show progress for each column
    var progress = "Fill " + (index + 1) + "/" + columnsToFill.length;
    updateProgressBar(sheet, progress, "fill");
    
    var colIdx = colNum - 1;
    var skip = (data[1][colIdx] === "" || data[1][colIdx] === null);
    var lastFilledRow = 1;
    
    for (var row = 2; row < lastDataRow; row++) {
      if (skip) {
        if (data[row][colIdx] !== "" && data[row][colIdx] !== null) {
          skip = false;
          lastFilledRow = row;
        }
        continue;
      }
      
      if (colNum === 4) { // Column D (Employee)
        if (data[row][colIdx] === "" && valueDFromK1 !== "" && valueDFromK1 !== null) {
          data[row][colIdx] = valueDFromK1;
        }
      } else if (colNum === 6) { // Column F (Date)
        if (data[row][colIdx] === "" || data[row][colIdx] === null) {
          data[row][colIdx] = currentDate;
        }
      } else {
        if (data[row][colIdx] === "" && data[lastFilledRow][colIdx] !== "" && lastFilledRow > 0) {
          data[row][colIdx] = data[lastFilledRow][colIdx];
        }
      }
      
      if (data[row][colIdx] !== "" && data[row][colIdx] !== null) {
        lastFilledRow = row;
      }
    }
    
    // Small delay to show progress
    Utilities.sleep(250);
  });

  updateProgressBar(sheet, "Writing...", "fill");

  sheet.getRange(1, 1, lastDataRow, colCount).setValues(data.slice(0, lastDataRow));

  updateProgressBar(sheet, "Labels...", "fill");

  // Update H labels automatically
  if (sheetName === "Vits") {
    updateVitsHLabels();
  } else if (sheetName === "Expo") {
    updateExpoHLabels();
  }

  updateProgressBar(sheet, "Complete!", "fill");
  Utilities.sleep(250);
  updateProgressBar(sheet, "Fill Blank Spaces", "fill");
}

// ===== SUMMARY TABLE FUNCTION =====

function updateSummaryTables(sheetName) {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  if (sheetName !== "Expo" && sheetName !== "Vits") return;

  var sheet = ss.getSheetByName(sheetName);
  if (!sheet) return;

  var lastDataRow = sheet.getLastRow();

  // Read data A2:G only if present
  var dataA2G = [];
  if (lastDataRow > 1) {
    dataA2G = sheet.getRange(2, 1, lastDataRow - 1, 7).getValues();
  }

  var isEmpty =
    dataA2G.length === 0 ||
    dataA2G.every(function (row) {
      return row.every(function (cell) {
        return cell === "" || cell === null;
      });
    });

  // Layout constants (matches your screenshot)
  // I9:K9 is "Total Item Weights" header (merged). Do NOT alter its borders except re-assert bottom.
  var weightsHeaderRow = 9;
  var summaryStartRow = 10; // first data row under the header
  var colI = 9, numCols = 3;

  // Helper: keep the bottom border visible on I9:K9
  function restoreHeaderBottomBorder() {
    var headerRange = sheet.getRange(weightsHeaderRow, colI, 1, numCols);
    headerRange.setBorder(
      null, null, true, null, null, null,
      '#000000',
      SpreadsheetApp.BorderStyle.SOLID_MEDIUM
    );
  }

  // Build totals and skid counts per unique Material ID using stable order
  var partOrder = [];
  var totals = Object.create(null);     // weight totals (Column A)
  var skidCounts = Object.create(null); // row counts per Material ID

  for (var i = 0; i < dataA2G.length; i++) {
    var partNum = dataA2G[i][1]; // Column B
    var qty = dataA2G[i][0];     // Column A

    if (
      partNum &&
      partNum !== "" &&
      partNum !== null &&
      partNum !== "Material ID" &&
      partNum !== "TTT" &&
      String(partNum).toUpperCase() !== "MATERIAL ID"
    ) {
      if (totals[partNum] === undefined) {
        partOrder.push(partNum);
        totals[partNum] = 0;
        skidCounts[partNum] = 0;
      }
      var qtyNum = isNaN(qty) ? 0 : Number(qty);
      totals[partNum] += qtyNum;
      skidCounts[partNum] += 1;
    }
  }

  // Prepare rows to write
  var weightRows = partOrder.map(function (partNum, idx) {
    return [idx + 1, partNum, "Total: " + totals[partNum]];
  });
  var skidRows = partOrder.map(function (partNum, idx) {
    return [idx + 1, partNum, "Total: " + skidCounts[partNum]];
  });

  // Compute how much area we will touch (for clearing and styling)
  var neededRows = (isEmpty ? 0 : (weightRows.length + 1 /*skids header*/ + skidRows.length));
  var scanCap = Math.min(Math.max(neededRows + 50, 200), 5000); // bounded
  var valuesIJK = sheet.getRange(summaryStartRow, colI, scanCap, numCols).getValues();
  var lastUsedOffset = -1;
  for (var r = valuesIJK.length - 1; r >= 0; r--) {
    var rowVals = valuesIJK[r];
    if (rowVals[0] !== "" || rowVals[1] !== "" || rowVals[2] !== "") {
      lastUsedOffset = r;
      break;
    }
  }
  var previouslyUsedRows = lastUsedOffset >= 0 ? lastUsedOffset + 1 : 0;
  var rowsToPrepare = Math.max(previouslyUsedRows, neededRows + 10); // small buffer
  rowsToPrepare = Math.min(rowsToPrepare, scanCap);

  // 1) RESET PHASE: Clear contents in I:K, remove borders in I:K, normalize bold/underline for ALL columns in those rows
  if (rowsToPrepare > 0) {
    // Clear/Unmerge only summary columns (I:K)
    var prepRange = sheet.getRange(summaryStartRow, colI, rowsToPrepare, numCols);
    prepRange.breakApart();
    prepRange.clearContent();
    prepRange.setBorder(false, false, false, false, false, false);

    // Normalize styles across ALL columns for those rows (not bold, no underline)
    var normalizeRange = sheet.getRange(summaryStartRow, 1, rowsToPrepare, sheet.getMaxColumns());
    normalizeRange.setFontWeight('normal').setFontLine('none');
  }

  // Always keep the header bottom border intact
  restoreHeaderBottomBorder();

  if (isEmpty) {
    // Nothing to write; exit after reset phase
    return;
  }

  // 2) WRITE PHASE: Write weights, skids header, then skids data
  // Write weights block
  if (weightRows.length > 0) {
    sheet.getRange(summaryStartRow, colI, weightRows.length, numCols).setValues(weightRows);
  }

  // Skids header (merged I:J:K)
  var skidsHeaderRow = summaryStartRow + weightRows.length;
  var skidsHeaderRange = sheet.getRange(skidsHeaderRow, colI, 1, numCols);
  skidsHeaderRange.merge();
  skidsHeaderRange
    .setValue("Total Item Skids")
    .setFontFamily("Roboto")
    .setFontSize(12)
    .setFontWeight("bold")
    .setFontLine("underline")
    .setHorizontalAlignment("center")
    .setVerticalAlignment("middle");

  // Skids data
  if (skidRows.length > 0) {
    sheet.getRange(skidsHeaderRow + 1, colI, skidRows.length, numCols).setValues(skidRows);
  }

  // 3) STYLE PHASE: Outside borders for (weights block), (skids header), (skids block)
  // Weights outside border
  if (weightRows.length > 0) {
    var weightsTop = summaryStartRow;
    var weightsBottom = summaryStartRow + weightRows.length - 1;
    var weightsRange = sheet.getRange(weightsTop, colI, weightsBottom - weightsTop + 1, numCols);
    weightsRange.setBorder(true, true, true, true, false, false, '#000000', SpreadsheetApp.BorderStyle.SOLID);
  }

  // Skids header outside border
  skidsHeaderRange.setBorder(true, true, true, true, false, false, '#000000', SpreadsheetApp.BorderStyle.SOLID);

  // Skids data outside border
  if (skidRows.length > 0) {
    var skidsTop = skidsHeaderRow + 1;
    var skidsBottom = skidsTop + skidRows.length - 1;
    var skidsRange = sheet.getRange(skidsTop, colI, skidsBottom - skidsTop + 1, numCols);
    skidsRange.setBorder(true, true, true, true, false, false, '#000000', SpreadsheetApp.BorderStyle.SOLID);
  }
}

// ===== LABEL UPDATE FUNCTIONS =====

function updateVitsHLabels() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheet = ss.getSheetByName("Vits");
  if (!sheet) return;
  updateHLabelsForSheet(sheet);
}

function updateExpoHLabels() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheet = ss.getSheetByName("Expo");
  if (!sheet) return;
  updateHLabelsForSheet(sheet);
}

function updateHLabelsForSheet(sheet) {
  var lastRow = sheet.getLastRow();
  if (lastRow < 2) return;

  // Read all data in A2:H
  var data = sheet.getRange(2, 1, lastRow - 1, 8).getValues(); // A2:H

  // Find qualifying rows (A-G not blank & Material ID present)
  var qualifyingRows = [];
  for (var r = 0; r < data.length; r++) {
    var materialID = data[r][1];
    var hValue = data[r][7]; // H column (index 7)
    if (
      materialID && materialID !== "" && materialID !== null &&
      data[r].slice(0, 7).some(cell => cell !== "" && cell !== null)
    ) {
      qualifyingRows.push(r);
    }
  }

  // Group qualifying rows by Material ID (column B = index 1)
  var materialGroups = {};
  qualifyingRows.forEach(function(rowIdx) {
    var materialID = data[rowIdx][1];
    if (!materialGroups[materialID]) materialGroups[materialID] = [];
    materialGroups[materialID].push(rowIdx);
  });

  // For each Material ID group, label appropriately using value in H
  Object.keys(materialGroups).forEach(function(materialID) {
    var group = materialGroups[materialID];
    var total = group.length;
    var kind = materialID.toUpperCase().startsWith("MMC") ? "Coils" : "Parts";
    for (var i = 0; i < total; i++) {
      var r = group[i];
      var rowNumber = r + 2;
      var labelNum = i + 1;
      var value = data[r][7]; // original value in H
      // Only update if H is not already a label string
      if (!(typeof value === "string" && value.trim().startsWith("Label"))) {
        var newVal = "Label: " + labelNum + " of " + total + " - " + kind + ": " + value;
        sheet.getRange(rowNumber, 8).setValue(newVal);
      }
    }
  });
}
// ===== SORTING FUNCTIONS =====

function sortVitsByBEA() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheet = ss.getSheetByName("Vits");
  if (!sheet) return;
  sortSheetByBEA(sheet);
}

function sortExpoByBEA() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var sheet = ss.getSheetByName("Expo");
  if (!sheet) return;
  sortSheetByBEA(sheet);
}

function sortSheetByBEA(sheet) {
  var sheetName = sheet.getName();
  
  // Start progress tracking in J4
  updateProgressBar(sheet, "Starting...", "sort");
  
  var lastRow = sheet.getLastRow();
  if (lastRow <= 2) {
    updateProgressBar(sheet, "No data", "sort");
    Utilities.sleep(250);
    updateProgressBar(sheet, "Sort for Printing", "sort");
    return;
  }

  updateProgressBar(sheet, "Reading...", "sort");

  var data = sheet.getRange(2, 1, lastRow - 1, 8).getValues();

  updateProgressBar(sheet, "Analyzing...", "sort");

  // Build frequency map
  var freqMap = {};
  data.forEach(function(row) {
    var matID = row[1];
    var heat = row[4];
    var key = matID + "|" + heat;
    freqMap[key] = (freqMap[key] || 0) + 1;
  });

  updateProgressBar(sheet, "Sorting...", "sort");

  // Sort with BEA logic
  data.sort(function(a, b) {
    // Primary: B (Material ID)
    if (a[1] < b[1]) return -1;
    if (a[1] > b[1]) return 1;

    // Secondary: by frequency of Heat within same Material ID (descending)
    var freqA = freqMap[a[1] + "|" + a[4]];
    var freqB = freqMap[b[1] + "|" + b[4]];
    if (freqA > freqB) return -1;
    if (freqA < freqB) return 1;

    // Tertiary: E (Heat) alphabetically
    if (a[4] < b[4]) return -1;
    if (a[4] > b[4]) return 1;

    // Break ties by Quantity (A) descending
    return (b[0] || 0) - (a[0] || 0);
  });

  updateProgressBar(sheet, "Writing...", "sort");

  // Write sorted data back
  sheet.getRange(2, 1, data.length, 8).setValues(data);

  updateProgressBar(sheet, "Complete!", "sort");
  Utilities.sleep(250);
  updateProgressBar(sheet, "Sort for Printing", "sort");
}


// ===== MAIN COPY TO HISTORY FUNCTIONS =====

function copyExpoToHistory() {
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var expoSheet = spreadsheet.getSheetByName("Expo");

  var ui = SpreadsheetApp.getUi();
  var response = ui.alert(
    'Confirmation - Transfer expo to History',
    'You will no longer be able to print labels for the Material entered in this sheet.\n\n' +
    'IMPORTANT: After clicking YES, watch the "Save to History" text for progress updates. ' +
    'Do NOT do anything until the checkbox disappears.\n\n' +
    'Are you sure you wish to continue?',
    ui.ButtonSet.YES_NO
  );
  if (response != ui.Button.YES) return;

  if (expoSheet) {
    expoSheet.getRange("I2").setValue(true);
    updateProgressBar(expoSheet, "Starting...");
  }

  try {
    copySheetToHistoryTable(expoSheet, "Expo");
  } catch (error) {
    
  } finally {
    if (expoSheet) {
      expoSheet.getRange("I2").setValue(false);
      updateProgressBar(expoSheet, "Save to History");
    }
  }
}

function copyVitsToHistory() {
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var vitsSheet = spreadsheet.getSheetByName("Vits");

  var ui = SpreadsheetApp.getUi();
  var response = ui.alert(
    'Confirmation - Transfer Vits to History',
    'You will no longer be able to print labels for the Material entered in this sheet.\n\n' +
    'IMPORTANT: After clicking YES, watch the "Save to History" text for progress updates. ' +
    'Do NOT do anything until the checkbox disappears.\n\n' +
    'Are you sure you wish to continue?',
    ui.ButtonSet.YES_NO
  );
  if (response != ui.Button.YES) return;

  if (vitsSheet) {
    vitsSheet.getRange("I2").setValue(true);
    updateProgressBar(vitsSheet, "Starting...");
  }

  try {
    copySheetToHistoryTable(vitsSheet, "Vits");
  } catch (error) {

  } finally {
    if (vitsSheet) {
      vitsSheet.getRange("I2").setValue(false);
      updateProgressBar(vitsSheet, "Save to History");
    }
  }
}

// ===== MAIN COPY FUNCTION WITH ENHANCED ERROR HANDLING =====

function copySheetToHistoryTable(sourceSheet, sheetName) {
  if (!sourceSheet) return;
  
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var currentYear = new Date().getFullYear();
  
  // Try current year first, then previous year as fallback
  var historySheet = spreadsheet.getSheetByName("History " + currentYear);
  if (!historySheet) {
    historySheet = spreadsheet.getSheetByName("History " + (currentYear - 1));
  }
  
  if (!historySheet) {
    updateProgressBar(sourceSheet, "No History!");
    throw new Error('No History table found. Looking for "History ' + currentYear + '" or "History ' + (currentYear - 1) + '"');
  }

  // *** STEP 1: Quick data check ***
  updateProgressBar(sourceSheet, "Reading...");
  
  var sourceLastRow = sourceSheet.getLastRow();
  if (sourceLastRow <= 1) {
    updateProgressBar(sourceSheet, "No data");
    Utilities.sleep(250);
    return;
  }
  
  var allData = sourceSheet.getRange(2, 1, sourceLastRow - 1, 7).getValues();
  var sourceData = allData.filter(row => row.some(cell => cell !== "" && cell !== null));
  
  if (sourceData.length === 0) {
    updateProgressBar(sourceSheet, "No data");
    Utilities.sleep(250);
    return;
  }

  Logger.log("Will transfer " + sourceData.length + " rows to " + historySheet.getName());

  // *** STEP 2: Prepare data with current date (DATE ONLY) ***
  updateProgressBar(sourceSheet, "Preparing...");
  
  Logger.log("First row prepared: " + sourceData[0].join(", "));

  // *** STEP 3: Direct write for best performance ***
  updateProgressBar(sourceSheet, "Copy 1/1");
  
  try {
    var historyLastRow = historySheet.getLastRow();
    var insertRow = historyLastRow + 1;
    
    Logger.log("Inserting " + sourceData.length + " rows starting at row " + insertRow);
    
    // Write all data at once
    var targetRange = historySheet.getRange(insertRow, 1, sourceData.length, 7);
    targetRange.setValues(sourceData);
    
    Logger.log("✅ All data written successfully");
    SpreadsheetApp.flush();
    
  } catch (error) {
    Logger.log("❌ Direct write failed: " + error.toString());
    updateProgressBar(sourceSheet, "Retry...");
    
    // Fallback to chunked writing
    var chunkSize = 50;
    var totalChunks = Math.ceil(sourceData.length / chunkSize);
    
    for (var i = 0; i < sourceData.length; i += chunkSize) {
      var endIndex = Math.min(i + chunkSize, sourceData.length);
      var chunk = sourceData.slice(i, endIndex);
      var currentChunk = Math.floor(i / chunkSize) + 1;
      
      updateProgressBar(sourceSheet, "Retry " + currentChunk + "/" + totalChunks);
      
      var chunkStartRow = historyLastRow + 1 + i;
      var chunkRange = historySheet.getRange(chunkStartRow, 1, chunk.length, 7);
      chunkRange.setValues(chunk);
      
      if (currentChunk % 3 === 0) {
        SpreadsheetApp.flush();
      }
    }
    
    SpreadsheetApp.flush();
    Logger.log("✅ Chunked write completed");
  }

  // *** STEP 4: Clear source data ***
  updateProgressBar(sourceSheet, "Clearing...");
  
  sourceSheet.getRange(2, 1, sourceLastRow - 1, 8).clearContent();
  sourceSheet.getRange("I3:I4").setValue(false);
  
  if (sourceLastRow > 9) {
    var summaryRowsToClear = Math.min(sourceLastRow - 9, 50);
    sourceSheet.getRange(10, 9, summaryRowsToClear, 3).clearContent();
  }
  
  SpreadsheetApp.flush();

  // *** STEP 5: Finish ***
  updateProgressBar(sourceSheet, "Finishing...");
  
  try {
    updateSummaryTables(sheetName);
  } catch (error) {
    Logger.log("Summary update error: " + error.toString());
  }
  
  Logger.log("✅ Transfer completed: " + sourceData.length + " rows with date " + currentDate);
  
  updateProgressBar(sourceSheet, "Complete!");
  Utilities.sleep(250);
}

// ===== ENHANCED PROGRESS BAR HELPER FUNCTION =====
function updateProgressBar(sheet, message, operation) {
  if (!sheet) return;
  
  var cell;
  // Determine which cell to update based on operation
  if (operation === "history" || !operation) {
    cell = "J2"; // Save to History
  } else if (operation === "fill") {
    cell = "J3"; // Fill Blank Spaces
  } else if (operation === "sort") {
    cell = "J4"; // Sort for Printing
  } else {
    cell = "J2"; // Default fallback
  }
  
  // Set the message in the appropriate cell
  sheet.getRange(cell).setValue(message);
  
  // Force display update
  SpreadsheetApp.flush();
  Utilities.sleep(250);
  
  // Log for debugging
  Logger.log("Progress (" + cell + "): " + message);
}
// ===== UTILITY FUNCTIONS =====
function autoFill() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var sheetName = sheet.getName();
  if (sheetName !== "Expo" && sheetName !== "Vits") {
    return;
  }

  // Start progress tracking in J3
  updateProgressBar(sheet, "Starting...", "fill");

  var columnsToFill = [1, 2, 3, 4, 5, 6, 7, 8];
  var lastRow = sheet.getLastRow();
  if (lastRow < 3) {
    updateProgressBar(sheet, "No data", "fill");
    Utilities.sleep(250);
    updateProgressBar(sheet, "Fill Blank Spaces", "fill");
    return;
  }

  updateProgressBar(sheet, "Reading...", "fill");

  var colCount = Math.max(...columnsToFill);
  var data = sheet.getRange(1, 1, lastRow, colCount).getValues();

  var lastDataRow = 1;
  for (var row = data.length - 1; row >= 0; row--) {
    if (data[row].some(cell => cell !== "" && cell !== null)) {
      lastDataRow = row + 1;
      break;
    }
  }
  if (lastDataRow < 3) {
    updateProgressBar(sheet, "No data", "fill");
    Utilities.sleep(250);
    updateProgressBar(sheet, "Fill Blank Spaces", "fill");
    return;
  }

  updateProgressBar(sheet, "Preparing...", "fill");

  var valueDFromK1 = sheet.getRange("K1").getValue();
  var currentDate = "2025-08-25"; // Current date only

  updateProgressBar(sheet, "Filling...", "fill");

  columnsToFill.forEach(function(colNum, index) {
    // Show progress for each column
    var progress = "Fill " + (index + 1) + "/" + columnsToFill.length;
    updateProgressBar(sheet, progress, "fill");
    
    var colIdx = colNum - 1;
    var skip = (data[1][colIdx] === "" || data[1][colIdx] === null);
    var lastFilledRow = 1;
    
    for (var row = 2; row < lastDataRow; row++) {
      if (skip) {
        if (data[row][colIdx] !== "" && data[row][colIdx] !== null) {
          skip = false;
          lastFilledRow = row;
        }
        continue;
      }
      
      if (colNum === 4) { // Column D (Employee)
        if (data[row][colIdx] === "" && valueDFromK1 !== "" && valueDFromK1 !== null) {
          data[row][colIdx] = valueDFromK1;
        }
      } else if (colNum === 6) { // Column F (Date)
        if (data[row][colIdx] === "" || data[row][colIdx] === null) {
          data[row][colIdx] = currentDate;
        }
      } else {
        if (data[row][colIdx] === "" && data[lastFilledRow][colIdx] !== "" && lastFilledRow > 0) {
          data[row][colIdx] = data[lastFilledRow][colIdx];
        }
      }
      
      if (data[row][colIdx] !== "" && data[row][colIdx] !== null) {
        lastFilledRow = row;
      }
    }
    
    // Small delay to show progress
    Utilities.sleep(250);
  });

  updateProgressBar(sheet, "Writing...", "fill");

  sheet.getRange(1, 1, lastDataRow, colCount).setValues(data.slice(0, lastDataRow));

  updateProgressBar(sheet, "Labels...", "fill");

  // Update H labels automatically
  if (sheetName === "Vits") {
    updateVitsHLabels();
  } else if (sheetName === "Expo") {
    updateExpoHLabels();
  }

  updateProgressBar(sheet, "Complete!", "fill");
  Utilities.sleep(250);

  // Reset to default after completion
  updateProgressBar(sheet, "Fill Blank Spaces", "fill");
}

function testProgressBar() {
  var sheet = SpreadsheetApp.getActiveSheet();
  var sheetName = sheet.getName();
  
  if (sheetName !== "Expo" && sheetName !== "Vits") {
    SpreadsheetApp.getUi().alert("Please run this test from either the Expo or Vits sheet.");
    return;
  }
  
  updateProgressBar(sheet, "Testing...");
  Utilities.sleep(250);
  
  updateProgressBar(sheet, "Step 1/3");
  Utilities.sleep(250);
  
  updateProgressBar(sheet, "Step 2/3");
  Utilities.sleep(250);
  
  updateProgressBar(sheet, "Step 3/3");
  Utilities.sleep(250);
  
  updateProgressBar(sheet, "Complete!");
  Utilities.sleep(250);
  
  updateProgressBar(sheet, "Save to History");
}

// ===== DISABLED FORMATTING FUNCTIONS (TABLE HANDLES FORMATTING) =====
function formatHistoryAfterSave() {
  SpreadsheetApp.getUi().alert("History formatting is now handled by the table structure. No additional formatting needed.");
}

function colorHistory() {
  Logger.log("Color formatting disabled - using table formatting instead");
  SpreadsheetApp.getUi().alert("Color formatting is disabled. The History table handles its own formatting automatically.");
}

function copySheetToHistoryTableWithVisibleProgress(sourceSheet, sheetName) {
  if (!sourceSheet) return;
  
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var currentYear = new Date().getFullYear();
  
  // Try current year first, then previous year as fallback
  var historySheet = spreadsheet.getSheetByName("History " + currentYear);
  if (!historySheet) {
    historySheet = spreadsheet.getSheetByName("History " + (currentYear - 1));
  }
  
  if (!historySheet) {
    updateProgressBar(sourceSheet, "No History!", "history");
    throw new Error('No History table found. Looking for "History ' + currentYear + '" or "History ' + (currentYear - 1) + '"');
  }

  // *** STEP 1: Quick data check ***
  updateProgressBar(sourceSheet, "Reading...", "history");
  
  var sourceLastRow = sourceSheet.getLastRow();
  Logger.log("Source last row: " + sourceLastRow);
  
  if (sourceLastRow <= 1) {
    updateProgressBar(sourceSheet, "No data", "history");
    Utilities.sleep(250);
    updateProgressBar(sourceSheet, "Save to History", "history");
    return;
  }
  
  var allData = sourceSheet.getRange(2, 1, sourceLastRow - 1, 7).getValues();
  var sourceData = allData.filter(row => row.some(cell => cell !== "" && cell !== null));
  
  if (sourceData.length === 0) {
    updateProgressBar(sourceSheet, "No data", "history");
    Utilities.sleep(250);
    updateProgressBar(sourceSheet, "Save to History", "history");
    return;
  }

  Logger.log("Will transfer " + sourceData.length + " rows to " + historySheet.getName());

  // *** STEP 2: Prepare data with current timestamp ***
  updateProgressBar(sourceSheet, "Preparing...", "history");
  
  var currentUser = "Dorotel";
  var currentDate = "2025-08-25"; // Date only as requested
  
  // Update date and user for all rows
  for (var i = 0; i < sourceData.length; i++) {
    sourceData[i][5] = currentDate; // Column F (Date)
    if (!sourceData[i][3] || sourceData[i][3] === "") {
      sourceData[i][3] = currentUser; // Column D (Employee)
    }
  }

  Logger.log("First row prepared: " + sourceData[0].join(", "));

  // *** STEP 3: Direct write for best performance ***
  updateProgressBar(sourceSheet, "Copy 1/1", "history");
  
  try {
    var historyLastRow = historySheet.getLastRow();
    var insertRow = historyLastRow + 1;
    
    Logger.log("Inserting " + sourceData.length + " rows starting at row " + insertRow);
    
    // Write all data at once
    var targetRange = historySheet.getRange(insertRow, 1, sourceData.length, 7);
    targetRange.setValues(sourceData);
    
    Logger.log("✅ All data written successfully");
    SpreadsheetApp.flush();
    
  } catch (error) {
    Logger.log("❌ Direct write failed: " + error.toString());
    updateProgressBar(sourceSheet, "Retry...", "history");
    
    // Fallback to chunked writing
    var chunkSize = 50;
    var totalChunks = Math.ceil(sourceData.length / chunkSize);
    
    for (var i = 0; i < sourceData.length; i += chunkSize) {
      var endIndex = Math.min(i + chunkSize, sourceData.length);
      var chunk = sourceData.slice(i, endIndex);
      var currentChunk = Math.floor(i / chunkSize) + 1;
      
      updateProgressBar(sourceSheet, "Retry " + currentChunk + "/" + totalChunks, "history");
      
      var chunkStartRow = historyLastRow + 1 + i;
      var chunkRange = historySheet.getRange(chunkStartRow, 1, chunk.length, 7);
      chunkRange.setValues(chunk);
      
      if (currentChunk % 3 === 0) {
        SpreadsheetApp.flush();
      }
    }
    
    SpreadsheetApp.flush();
    Logger.log("✅ Chunked write completed");
  }

  // *** STEP 4: FIXED CLEARING LOGIC ***
  updateProgressBar(sourceSheet, "Clearing...", "history");
  
  try {
    Logger.log("Starting to clear source data from row 2 to " + sourceLastRow);
    
    // Find the actual last row with data in columns A-H
    var actualLastRow = sourceLastRow;
    for (var checkRow = sourceLastRow; checkRow >= 2; checkRow--) {
      var rowData = sourceSheet.getRange(checkRow, 1, 1, 8).getValues()[0];
      if (rowData.some(cell => cell !== "" && cell !== null)) {
        actualLastRow = checkRow;
        break;
      }
    }
    
    Logger.log("Actual last row with data: " + actualLastRow);
    
    // Only clear if there are rows to clear (actualLastRow >= 2)
    if (actualLastRow >= 2) {
      var rowsToClear = actualLastRow - 1; // From row 2 to actualLastRow
      Logger.log("Clearing " + rowsToClear + " rows from A2:H" + actualLastRow);
      
      sourceSheet.getRange(2, 1, rowsToClear, 8).clearContent();
      Logger.log("✅ Data rows cleared successfully");
    } else {
      Logger.log("No data rows to clear");
    }
    
    // Clear checkboxes I3 and I4
    sourceSheet.getRange("I3:I4").setValue(false);
    Logger.log("✅ Checkboxes I3:I4 cleared");
    
    // Clear summary area if it exists
    var summaryStartRow = 10;
    var maxSummaryRows = Math.max(actualLastRow - 9, 20); // At least 20 rows or more
    if (maxSummaryRows > 0) {
      Logger.log("Clearing summary area from I" + summaryStartRow + " to K" + (summaryStartRow + maxSummaryRows - 1));
      sourceSheet.getRange(summaryStartRow, 9, maxSummaryRows, 3).clearContent();
      Logger.log("✅ Summary area cleared");
    }
    
    SpreadsheetApp.flush();
    Logger.log("✅ All clearing operations completed successfully");
    
  } catch (clearError) {
    Logger.log("❌ Error during clearing: " + clearError.toString());
    // Don't throw error here - data was already copied successfully
  }

  // *** STEP 5: Finish ***
  updateProgressBar(sourceSheet, "Finishing...", "history");
  
  try {
    updateSummaryTables(sheetName);
    Logger.log("✅ Summary tables updated");
  } catch (error) {
    Logger.log("❌ Summary update error: " + error.toString());
  }
  
  Logger.log("✅ Transfer completed: " + sourceData.length + " rows with date " + currentDate + " by " + currentUser + " at 2025-08-25 17:25:38");
  
  updateProgressBar(sourceSheet, "Complete!", "history");
  Utilities.sleep(250);
  updateProgressBar(sourceSheet, "Save to History", "history");
}

// ===== HANDLE EDIT FUNCTION =====

function handleEdit(sheet, sheetName, row) {
  var rowRange = sheet.getRange(row, 1, 1, 7);
  var rowData = rowRange.getValues()[0];
  var columnsToCheck = [0, 1, 2, 4, 6]; // A, B, C, E, G

  var allEmpty = columnsToCheck.every(index => rowData[index] === "");

  if (allEmpty) {
    rowRange.clearContent();
    return;
  }

  if (row > 1) {
    var valueD = sheet.getRange("K1").getValue();
    rowData[3] = valueD; // D

    rowData[5] = Utilities.formatDate(new Date(), "UTC", "MM-dd-yyyy"); // F - DATE ONLY

    // Uppercase for B, C, E, G
    [1, 2, 4, 6].forEach(idx => {
      if (rowData[idx]) rowData[idx] = rowData[idx].toString().toUpperCase();
    });
    
    sheet.getRange(row, 1, 1, 7).setValues([rowData]);
  }
}

// ===== SEARCH FUNCTIONS =====

function fillSelectSheet() {
  var spreadsheet = SpreadsheetApp.getActiveSpreadsheet();
  var sheets = spreadsheet.getSheets();
  var sheetNames = [];
  for (var i = 0; i < sheets.length; i++) {
    var sheetName = sheets[i].getName();
    // UNIFIED: Include Expo in search dropdown since it's now identical to Vits
    if (sheetName !== 'Search') sheetNames.push(sheetName);
  }
  var targetSheet = spreadsheet.getSheetByName('Search');
  var cell = targetSheet.getRange('C1');
  var rule = SpreadsheetApp.newDataValidation().requireValueInList(sheetNames).build();
  cell.setDataValidation(rule);
  if (sheetNames.length > 0) cell.setValue(sheetNames[0]);
}

function fillSearchBy() {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getSheetByName('Search');
  var cell = sheet.getRange('E1');
  var rule = SpreadsheetApp.newDataValidation().requireValueInList(['Material ID', 'PO Number', 'Employee', 'Date'], true).build();
  cell.setDataValidation(rule);
  cell.setValue('Material ID');
}

function fillSearchFor() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var searchSheet = ss.getSheetByName("Search");
  var searchCriteria = searchSheet.getRange("E1").getValue();
  var sheetName = searchSheet.getRange("C1").getValue();
  var targetSheet = ss.getSheetByName(sheetName);
  if (!targetSheet) {
    Logger.log("Sheet not found: " + sheetName);
    return;
  }

  var lastTargetRow = targetSheet.getLastRow();
  if (lastTargetRow < 3) return;

  var colMap = {
    "Material ID": 2,
    "PO Number": 3,
    "Employee": 4,
    "Date": 6
  };
  var col = colMap[searchCriteria];
  if (!col) {
    Logger.log("Invalid search criteria: " + searchCriteria);
    return;
  }
  var dataRange = targetSheet.getRange(3, col, lastTargetRow - 2, 1);
  var data = dataRange.getValues();

  var uniqueSet = new Set();
  data.forEach(function(row) {
    if (row[0] !== "") {
      if (searchCriteria === "Date") {
        var d = Utilities.formatDate(row[0], Session.getScriptTimeZone(), "M/d/yyyy");
        uniqueSet.add(d);
      } else {
        uniqueSet.add(row[0]);
      }
    }
  });

  var uniqueValues = Array.from(uniqueSet);
  uniqueValues.sort(descOrder);

  var rule = SpreadsheetApp.newDataValidation().requireValueInList(uniqueValues).build();
  searchSheet.getRange("G1").setDataValidation(rule);

  if (uniqueValues.length > 0) {
    searchSheet.getRange("G1").setValue(uniqueValues[0]);
  }
}

function searchButton() {
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var searchSheet = ss.getSheetByName("Search");

  // Check if C1, E1, and G1 are populated
  var searchCriteria = searchSheet.getRange("E1").getValue();
  var sheetName = searchSheet.getRange("C1").getValue();
  var searchValue = searchSheet.getRange("G1").getValue();
  if (!searchCriteria || !sheetName || !searchValue) {
    Logger.log("C1, E1, and G1 must be populated");
    return;
  }

  // Clear previous results (rows 4 and below)
  var lastRow = searchSheet.getLastRow();
  if (lastRow > 3) {
    searchSheet.getRange(4, 1, lastRow - 3, searchSheet.getLastColumn()).clearContent();
  }

  var targetSheet = ss.getSheetByName(sheetName);
  if (!targetSheet) {
    Logger.log("Sheet not found: " + sheetName);
    return;
  }
  var lastTargetRow = targetSheet.getLastRow();
  if (lastTargetRow < 2) {
    Logger.log("No data found in the selected sheet starting from row 2");
    return;
  }

  // All A-G data, rows 2+
  var data = targetSheet.getRange(2, 1, lastTargetRow - 1, 7).getValues();
  // Filter if needed
  var isCheckboxChecked = searchSheet.getRange("H1").getValue();

  var filteredData = (!isCheckboxChecked)
    ? data.filter(row => row.includes(searchValue))
    : data;

  if (filteredData.length > 0) {
    searchSheet.getRange("A3").offset(0, 0, filteredData.length, filteredData[0].length).setValues(filteredData);
  }
}

function clearButton(){
  var ss = SpreadsheetApp.getActiveSpreadsheet();
  var searchSheet = ss.getSheetByName("Search");
  var lastRow = searchSheet.getLastRow();
  if (lastRow > 2) {
    searchSheet.getRange(3, 1, lastRow - 2, searchSheet.getLastColumn()).clearContent();
  }
}

// ===== UTILITY FUNCTIONS =====

function formatPOColumn(e) {
  var sheet = e.range.getSheet();
  var sheetName = sheet.getName();
  var row = e.range.getRow();
  var col = e.range.getColumn();
  if ((sheetName !== "Expo" && sheetName !== "Vits") || col !== 3 || row < 2) return;

  var value = e.value;
  if (typeof value !== "string") return;

  // If value is only 5 digits and doesn't start with 0, add PO-0
  if (/^\d{5}$/.test(value) && !value.startsWith("0")) {
    sheet.getRange(row, col).setValue("PO-0" + value);
  }
  // If value is 6 digits and starts with 0, add PO-
  else if (/^0\d{5}$/.test(value)) {
    sheet.getRange(row, col).setValue("PO-" + value);
  }
}

function padMMCMMFCode(e) {
  var sheet = e.range.getSheet();
  var sheetName = sheet.getName();
  var row = e.range.getRow();
  var col = e.range.getColumn();
  if ((sheetName !== "Expo" && sheetName !== "Vits") || col !== 2 || row < 2) return;

  var value = e.value;
  if (typeof value !== "string") return;

  // Make check case-insensitive and preserve original casing
  var upperValue = value.toUpperCase();
  var normalizedValue = value;

  if (/^MMC0RS/i.test(value)) {
    normalizedValue = "MMCSR" + value.slice(3);
    upperValue = normalizedValue.toUpperCase();
  } else if (/^MMF0RS/i.test(value)) {
    normalizedValue = "MMFSR" + value.slice(3);
    upperValue = normalizedValue.toUpperCase();
  }

  var prefixes = ["MMCSR", "MMFSR", "MMCCS", "MMFCS", "MMC", "MMF", "MMS"];
  for (var i = 0; i < prefixes.length; i++) {
    var prefix = prefixes[i];
    if (upperValue.indexOf(prefix) === 0 && normalizedValue.length !== 10) {
      var rest = normalizedValue.slice(prefix.length);
      var paddedNumber = rest.padStart(10 - prefix.length, "0");
      var newValue = prefix + paddedNumber;
      if (newValue !== value) {
        sheet.getRange(row, col).setValue(newValue);
      }
      break;
    }
  }
}

function descOrder(element1, element2) {
  if(element1 > element2) return -1;
  if(element1 < element2) return 1;
  return 0;
}

// ===== REMAINING UNCHANGED FUNCTIONS =====

function exportToCSV() {
  var properties = PropertiesService.getUserProperties();
  var lastRun = properties.getProperty('lastRun');
  var now = new Date().getTime();

  if (lastRun && (now - lastRun) < 5000) {
    SpreadsheetApp.getUi().alert('Please wait at least 5 seconds before running this function again.');
    return;
  }
  properties.setProperty('lastRun', now);

  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var lastRow = sheet.getLastRow();
  var lastCol = sheet.getLastColumn();
  if (lastRow < 2) return;

  var range = sheet.getRange(2, 1, lastRow - 1, lastCol);
  var values = range.getValues();
  var csvContent = values.map(rowArray => rowArray.join(",")).join("\r\n") + "\r\n";

  var blob = Utilities.newBlob(csvContent, 'text/csv', 'Receiving Label.csv');
  var base64Data = Utilities.base64Encode(blob.getBytes());
  var downloadLink = 'data:text/csv;base64,' + base64Data;

  var html = '<html><body><a id="downloadLink" href="' + downloadLink + '" download="Receiving Label.csv"></a>' +
             '<script>document.getElementById("downloadLink").click();google.script.host.close();</script>' +
             '</body></html>';

  var userInterface = HtmlService.createHtmlOutput(html).setWidth(1).setHeight(1);
  SpreadsheetApp.getUi().showModelessDialog(userInterface, 'Downloading CSV');
}