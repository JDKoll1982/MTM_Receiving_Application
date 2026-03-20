function normalizePO(value) {
  if (value === "" || value === null) return "No PO";
  value = value.toString().trim();

  if (/^PO-0\d{5,6}[A-Za-z]?$/.test(value)) return value;

  let match = /^0?(\d{5,6})([A-Za-z]?)$/.exec(value);
  if (match) {
    let digits = match[1];
    let letter = match[2] || "";
    digits = digits.padStart(6, '0');
    return "PO-" + digits + letter;
  }

  if (/^\d{1,4}$/.test(value)) return "Validate PO";
  return value;
}

function groupAndPushHistory2025() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const historySheet = ss.getSheetByName('History 2025');
  const receivingSheet = ss.getSheetByName('Receiving EoD Emails');

  const startDate = new Date(receivingSheet. getRange('C3').getValue());
  const endDate = new Date(receivingSheet.getRange('E3').getValue());

  const historyData = historySheet. getRange(2, 1, historySheet.getLastRow() - 1, historySheet. getLastColumn()).getValues();

  // Filter + Normalize
  let normalizedRows = historyData.filter(row => {
    const dateVal = new Date(row[5]);
    return dateVal >= startDate && dateVal <= endDate;
  }).map(row => {
    row[2] = normalizePO(row[2]);                // PO
    row[4] = (!row[4] || row[4] === "") ? "No Heat" : row[4]; // Heat
    return row;
  });

  // Group and aggregate
  let groupMap = {};
  normalizedRows. forEach(row => {
    // Group by (B, normalized C, E, G, F)
    let key = [row[1], row[2], row[4], row[6], row[5]].join('|'); // B,C,E,G,F
    if (!groupMap[key]) {
      groupMap[key] = {
        quantity: 0,
        skidCount: 0,  // NEW: Track number of skids/lines
        materialID: row[1],
        po: row[2],
        employee: row[3],
        heat: row[4],
        date: row[5],
        location: row[6]
      };
    }
    groupMap[key].quantity += Number(row[0]);
    groupMap[key].skidCount += 1;  // NEW:  Increment skid count
  });

  // Build array for output and sort (F, B, C, G, E)
  // NEW: Added skidCount as the 8th column
  let finalArray = Object.values(groupMap).map(grp => [
    grp.quantity,
    grp.materialID,
    grp.po,
    grp. employee,
    grp.heat,
    grp.date,
    grp.location,
    grp.skidCount  // NEW:  Skid count column
  ]);

  finalArray.sort((a, b) => {
    // date
    const dA = new Date(a[5]), dB = new Date(b[5]);
    if (dA - dB !== 0) return dA - dB;
    // materialID
    if (a[1] !== b[1]) return a[1] > b[1] ?  1 : -1;
    // PO
    if (a[2] !== b[2]) return a[2] > b[2] ? 1 :  -1;
    // Initial Location
    if (a[6] !== b[6]) return a[6] > b[6] ?  1 : -1;
    // Heat
    if (a[4] !== b[4]) return a[4] > b[4] ? 1 : -1;
    return 0;
  });

  // Remove all rows below header (row 6)
  const headerRow = 6;
  const firstDataRow = 7;
  const numRows = receivingSheet.getMaxRows();
  if (numRows > headerRow) {
    receivingSheet.deleteRows(firstDataRow, numRows - headerRow);
  }

  // Insert required number of rows, set data in B7
  if (finalArray.length > 0) {
    receivingSheet.insertRowsAfter(headerRow, finalArray.length);
    receivingSheet.getRange(firstDataRow, 2, finalArray.length, finalArray[0].length).setValues(finalArray);
  }
}