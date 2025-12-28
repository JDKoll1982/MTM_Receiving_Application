// Manual Entry Mode State
let manualEntryState = {
    rows: [],
    nextRowId: 1
};

// Initialize manual entry grid
function initManualEntry() {
    // Add initial rows
    for (let i = 0; i < 5; i++) {
        addRow();
    }
    updateRowStats();
}

// Add a new row to the grid
function addRow() {
    const row = {
        id: manualEntryState.nextRowId++,
        type: '',
        partId: '',
        quantity: 1,
        poNumber: '',
        location: '',
        width: '',
        height: '',
        depth: ''
    };

    manualEntryState.rows.push(row);
    renderManualEntryGrid();
    updateRowStats();
}

// Remove a row
function removeRow(rowId) {
    const index = manualEntryState.rows.findIndex(r => r.id === rowId);
    if (index > -1) {
        manualEntryState.rows.splice(index, 1);
        renderManualEntryGrid();
        updateRowStats();
    }
}

// Render the manual entry grid
function renderManualEntryGrid() {
    const tbody = document.getElementById('manualEntryBody');
    if (!tbody) return;

    tbody.innerHTML = '';

    manualEntryState.rows.forEach((row, index) => {
        const tr = document.createElement('tr');

        // Row number
        const tdNum = document.createElement('td');
        tdNum.textContent = index + 1;
        tdNum.className = 'text-secondary';
        tr.appendChild(tdNum);

        // Type dropdown
        const tdType = document.createElement('td');
        const typeSelect = document.createElement('select');
        typeSelect.className = 'textbox';
        typeSelect.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        typeSelect.innerHTML = '<option value="">Select Type...</option>';
        dunnageTypes.forEach(type => {
            const option = document.createElement('option');
            option.value = type;
            option.textContent = type;
            option.selected = row.type === type;
            typeSelect.appendChild(option);
        });
        typeSelect.onchange = (e) => {
            row.type = e.target.value;
            updatePartDropdown(row.id, e.target.value);
            updateRowStats();
        };
        tdType.appendChild(typeSelect);
        tr.appendChild(tdType);

        // Part ID dropdown
        const tdPart = document.createElement('td');
        const partSelect = document.createElement('select');
        partSelect.className = 'textbox';
        partSelect.id = `partSelect_${row.id}`;
        partSelect.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        partSelect.innerHTML = '<option value="">Select Part...</option>';
        if (row.type && partsByType[row.type]) {
            partsByType[row.type].forEach(part => {
                const option = document.createElement('option');
                option.value = part.id;
                option.textContent = part.id;
                option.selected = row.partId === part.id;
                partSelect.appendChild(option);
            });
        }
        partSelect.onchange = (e) => {
            row.partId = e.target.value;
            autoFillSpecs(row.id);
            updateRowStats();
        };
        tdPart.appendChild(partSelect);
        tr.appendChild(tdPart);

        // Quantity
        const tdQty = document.createElement('td');
        const qtyInput = document.createElement('input');
        qtyInput.type = 'number';
        qtyInput.className = 'numberbox';
        qtyInput.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        qtyInput.value = row.quantity;
        qtyInput.min = 1;
        qtyInput.oninput = (e) => {
            row.quantity = parseInt(e.target.value) || 0;
            updateRowStats();
        };
        tdQty.appendChild(qtyInput);
        tr.appendChild(tdQty);

        // PO Number
        const tdPO = document.createElement('td');
        const poInput = document.createElement('input');
        poInput.type = 'text';
        poInput.className = 'textbox';
        poInput.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        poInput.value = row.poNumber;
        poInput.oninput = (e) => row.poNumber = e.target.value;
        tdPO.appendChild(poInput);
        tr.appendChild(tdPO);

        // Location
        const tdLoc = document.createElement('td');
        const locInput = document.createElement('input');
        locInput.type = 'text';
        locInput.className = 'textbox';
        locInput.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        locInput.value = row.location;
        locInput.oninput = (e) => row.location = e.target.value;
        tdLoc.appendChild(locInput);
        tr.appendChild(tdLoc);

        // Width
        const tdWidth = document.createElement('td');
        const widthInput = document.createElement('input');
        widthInput.type = 'number';
        widthInput.className = 'numberbox';
        widthInput.id = `width_${row.id}`;
        widthInput.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        widthInput.value = row.width;
        widthInput.step = '0.1';
        widthInput.oninput = (e) => row.width = e.target.value;
        tdWidth.appendChild(widthInput);
        tr.appendChild(tdWidth);

        // Height
        const tdHeight = document.createElement('td');
        const heightInput = document.createElement('input');
        heightInput.type = 'number';
        heightInput.className = 'numberbox';
        heightInput.id = `height_${row.id}`;
        heightInput.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        heightInput.value = row.height;
        heightInput.step = '0.1';
        heightInput.oninput = (e) => row.height = e.target.value;
        tdHeight.appendChild(heightInput);
        tr.appendChild(tdHeight);

        // Depth
        const tdDepth = document.createElement('td');
        const depthInput = document.createElement('input');
        depthInput.type = 'number';
        depthInput.className = 'numberbox';
        depthInput.id = `depth_${row.id}`;
        depthInput.style.cssText = 'width: 100%; padding: 4px 8px; font-size: 13px;';
        depthInput.value = row.depth;
        depthInput.step = '0.1';
        depthInput.oninput = (e) => row.depth = e.target.value;
        tdDepth.appendChild(depthInput);
        tr.appendChild(tdDepth);

        // Actions
        const tdActions = document.createElement('td');
        const removeBtn = document.createElement('button');
        removeBtn.className = 'button';
        removeBtn.style.cssText = 'padding: 4px 8px; font-size: 12px;';
        removeBtn.textContent = '🗑️';
        removeBtn.onclick = () => removeRow(row.id);
        tdActions.appendChild(removeBtn);
        tr.appendChild(tdActions);

        // Highlight invalid rows
        if (!isRowValid(row)) {
            tr.style.background = 'rgba(255, 140, 0, 0.1)';
        }

        tbody.appendChild(tr);
    });
}

// Update part dropdown when type changes
function updatePartDropdown(rowId, type) {
    const partSelect = document.getElementById(`partSelect_${rowId}`);
    if (!partSelect) return;

    partSelect.innerHTML = '<option value="">Select Part...</option>';

    if (type && partsByType[type]) {
        partsByType[type].forEach(part => {
            const option = document.createElement('option');
            option.value = part.id;
            option.textContent = part.id;
            partSelect.appendChild(option);
        });
    }
}

// Auto-fill specs when part is selected
function autoFillSpecs(rowId) {
    const row = manualEntryState.rows.find(r => r.id === rowId);
    if (!row || !row.type || !row.partId) return;

    const parts = partsByType[row.type];
    if (!parts) return;

    const part = parts.find(p => p.id === row.partId);
    if (!part) return;

    // Auto-fill dimensions
    if (part.width) {
        row.width = part.width;
        const widthInput = document.getElementById(`width_${rowId}`);
        if (widthInput) widthInput.value = part.width;
    }

    if (part.height) {
        row.height = part.height;
        const heightInput = document.getElementById(`height_${rowId}`);
        if (heightInput) heightInput.value = part.height;
    }

    if (part.depth) {
        row.depth = part.depth;
        const depthInput = document.getElementById(`depth_${rowId}`);
        if (depthInput) depthInput.value = part.depth;
    }
}

// Check if row is valid
function isRowValid(row) {
    return row.type && row.partId && row.quantity > 0;
}

// Update row statistics
function updateRowStats() {
    const totalRows = manualEntryState.rows.length;
    const validRows = manualEntryState.rows.filter(isRowValid).length;
    const invalidRows = totalRows - validRows;

    document.getElementById('rowCount').textContent = totalRows;
    document.getElementById('validRowCount').textContent = validRows;
    document.getElementById('invalidRowCount').textContent = invalidRows;
}

// Save manual entries
function saveManualEntries() {
    const validRows = manualEntryState.rows.filter(isRowValid);

    if (validRows.length === 0) {
        alert('No valid rows to save. Please fill in Type, Part ID, and Quantity for at least one row.');
        return;
    }

    console.log('Saving manual entries:', validRows);

    // Simulate save
    alert(`Successfully saved ${validRows.length} row(s)!\n\nIn the real application, this would:\n- Insert into database\n- Export to CSV\n- Show success message`);

    // Reset grid
    manualEntryState.rows = [];
    manualEntryState.nextRowId = 1;
    initManualEntry();
}

// Navigate back to mode selection
function goBackToModeSelection() {
    if (manualEntryState.rows.some(r => r.type || r.partId)) {
        if (!confirm('You have unsaved data. Are you sure you want to go back?')) {
            return;
        }
    }
    window.location.href = 'dunnage-wizard.html';
}

// Add multiple rows
function addMultipleRows() {
    const count = prompt('How many rows would you like to add?', '5');
    if (count && !isNaN(count) && count > 0) {
        for (let i = 0; i < parseInt(count); i++) {
            addRow();
        }
    }
}

// Remove selected rows
function removeSelectedRows() {
    const checkboxes = document.querySelectorAll('#manualEntryBody input[type="checkbox"]:checked');
    if (checkboxes.length === 0) {
        alert('Please select at least one row to remove.');
        return;
    }

    if (confirm(`Remove ${checkboxes.length} selected row(s)?`)) {
        checkboxes.forEach(cb => {
            const rowId = parseInt(cb.dataset.rowId);
            removeRow(rowId);
        });
    }
}

// Auto-fill selected rows
function autoFillSelected() {
    alert('Auto-fill functionality would populate selected rows based on part data (not fully implemented in mockup)');
}

// Initialize on load
document.addEventListener('DOMContentLoaded', () => {
    initManualEntry();
});
