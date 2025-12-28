// Edit Mode State
let editModeState = {
    allRecords: [],
    filteredRecords: [],
    selectedRecords: new Set(),
    currentPage: 1,
    recordsPerPage: 20,
    currentEditId: null
};

// Generate sample historical data
function generateSampleRecords() {
    const records = [];
    const users = ['John Doe', 'Jane Smith', 'Bob Johnson', 'Alice Williams'];
    const locations = ['Warehouse A', 'Warehouse B', 'Dock 1', 'Dock 2', 'Storage'];

    let id = 1;
    const now = new Date();

    // Generate 50 sample records
    for (let i = 0; i < 50; i++) {
        const daysAgo = Math.floor(Math.random() * 60); // Records from last 60 days
        const date = new Date(now);
        date.setDate(date.getDate() - daysAgo);

        const randomType = dunnageTypes[Math.floor(Math.random() * dunnageTypes.length)];
        const typeParts = partsByType[randomType] || [];
        const randomPart = typeParts[Math.floor(Math.random() * typeParts.length)];

        if (!randomPart) continue;

        records.push({
            id: id++,
            date: date,
            type: randomType,
            partId: randomPart.id,
            quantity: Math.floor(Math.random() * 20) + 1,
            poNumber: Math.random() > 0.5 ? `PO${100000 + Math.floor(Math.random() * 900000)}` : '',
            location: locations[Math.floor(Math.random() * locations.length)],
            user: users[Math.floor(Math.random() * users.length)],
            width: randomPart.width || '',
            height: randomPart.height || '',
            depth: randomPart.depth || ''
        });
    }

    // Sort by date descending (newest first)
    records.sort((a, b) => b.date - a.date);

    return records;
}

// Initialize edit mode
function initEditMode() {
    editModeState.allRecords = generateSampleRecords();
    editModeState.filteredRecords = [...editModeState.allRecords];

    // Populate type filter dropdown
    const typeFilter = document.getElementById('typeFilter');
    if (typeFilter) {
        dunnageTypes.forEach(type => {
            const option = document.createElement('option');
            option.value = type;
            option.textContent = type;
            typeFilter.appendChild(option);
        });
    }

    // Populate edit dialog type dropdown
    const editType = document.getElementById('editType');
    if (editType) {
        dunnageTypes.forEach(type => {
            const option = document.createElement('option');
            option.value = type;
            option.textContent = type;
            editType.appendChild(option);
        });
    }

    renderHistoryGrid();
    updateStatistics();
}

// Filter records based on search and filters
function filterRecords() {
    const searchTerm = document.getElementById('searchInput')?.value.toLowerCase() || '';
    const typeFilter = document.getElementById('typeFilter')?.value || '';
    const dateFilter = document.getElementById('dateFilter')?.value || 'all';

    editModeState.filteredRecords = editModeState.allRecords.filter(record => {
        // Search filter
        const matchesSearch = !searchTerm ||
            record.partId.toLowerCase().includes(searchTerm) ||
            record.poNumber.toLowerCase().includes(searchTerm) ||
            record.location.toLowerCase().includes(searchTerm) ||
            record.user.toLowerCase().includes(searchTerm);

        // Type filter
        const matchesType = !typeFilter || record.type === typeFilter;

        // Date filter
        let matchesDate = true;
        if (dateFilter !== 'all') {
            const now = new Date();
            const recordDate = new Date(record.date);

            switch (dateFilter) {
                case 'today':
                    matchesDate = recordDate.toDateString() === now.toDateString();
                    break;
                case 'week':
                    const weekAgo = new Date(now);
                    weekAgo.setDate(weekAgo.getDate() - 7);
                    matchesDate = recordDate >= weekAgo;
                    break;
                case 'month':
                    const monthAgo = new Date(now);
                    monthAgo.setMonth(monthAgo.getMonth() - 1);
                    matchesDate = recordDate >= monthAgo;
                    break;
            }
        }

        return matchesSearch && matchesType && matchesDate;
    });

    editModeState.currentPage = 1;
    renderHistoryGrid();
    updateStatistics();
}

// Clear all filters
function clearFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('typeFilter').value = '';
    document.getElementById('dateFilter').value = 'all';
    filterRecords();
}

// Render history grid with pagination
function renderHistoryGrid() {
    const tbody = document.getElementById('historyTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';

    const totalPages = Math.ceil(editModeState.filteredRecords.length / editModeState.recordsPerPage);
    const startIndex = (editModeState.currentPage - 1) * editModeState.recordsPerPage;
    const endIndex = Math.min(startIndex + editModeState.recordsPerPage, editModeState.filteredRecords.length);

    const recordsToShow = editModeState.filteredRecords.slice(startIndex, endIndex);

    recordsToShow.forEach((record, index) => {
        const tr = document.createElement('tr');

        // Checkbox
        const tdCheck = document.createElement('td');
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.checked = editModeState.selectedRecords.has(record.id);
        checkbox.onchange = (e) => {
            if (e.target.checked) {
                editModeState.selectedRecords.add(record.id);
            } else {
                editModeState.selectedRecords.delete(record.id);
            }
            updateStatistics();
            updateDeleteButton();
        };
        tdCheck.appendChild(checkbox);
        tr.appendChild(tdCheck);

        // Record number
        const tdNum = document.createElement('td');
        tdNum.textContent = record.id;
        tdNum.className = 'text-secondary';
        tr.appendChild(tdNum);

        // Date/Time
        const tdDate = document.createElement('td');
        tdDate.textContent = record.date.toLocaleDateString() + ' ' + record.date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        tdDate.className = 'caption-text';
        tr.appendChild(tdDate);

        // Type
        const tdType = document.createElement('td');
        tdType.textContent = record.type;
        tr.appendChild(tdType);

        // Part ID
        const tdPart = document.createElement('td');
        tdPart.textContent = record.partId;
        tdPart.className = 'body-strong';
        tr.appendChild(tdPart);

        // Quantity
        const tdQty = document.createElement('td');
        tdQty.textContent = record.quantity;
        tr.appendChild(tdQty);

        // PO Number
        const tdPO = document.createElement('td');
        tdPO.textContent = record.poNumber || '-';
        tdPO.className = record.poNumber ? '' : 'text-secondary';
        tr.appendChild(tdPO);

        // Location
        const tdLoc = document.createElement('td');
        tdLoc.textContent = record.location;
        tr.appendChild(tdLoc);

        // User
        const tdUser = document.createElement('td');
        tdUser.textContent = record.user;
        tdUser.className = 'caption-text';
        tr.appendChild(tdUser);

        // Actions
        const tdActions = document.createElement('td');
        const editBtn = document.createElement('button');
        editBtn.className = 'button';
        editBtn.style.cssText = 'padding: 4px 8px; font-size: 12px;';
        editBtn.textContent = '✏️ Edit';
        editBtn.onclick = () => openEditDialog(record.id);
        tdActions.appendChild(editBtn);
        tr.appendChild(tdActions);

        tbody.appendChild(tr);
    });

    // Update pagination
    document.getElementById('resultCount').textContent = editModeState.filteredRecords.length;
    document.getElementById('totalCount').textContent = editModeState.allRecords.length;

    const pageInput = document.getElementById('pageInput');
    const pageTotal = document.getElementById('editPageTotal');
    if (pageInput) pageInput.value = editModeState.currentPage;
    if (pageTotal) pageTotal.textContent = Math.max(1, totalPages);

    document.getElementById('editPrevPageBtn').disabled = editModeState.currentPage === 1;
    document.getElementById('editNextPageBtn').disabled = editModeState.currentPage === totalPages || totalPages === 0;
}

// Toggle select all
function toggleSelectAll() {
    const selectAll = document.getElementById('selectAll');
    const startIndex = (editModeState.currentPage - 1) * editModeState.recordsPerPage;
    const endIndex = Math.min(startIndex + editModeState.recordsPerPage, editModeState.filteredRecords.length);
    const pageRecords = editModeState.filteredRecords.slice(startIndex, endIndex);

    if (selectAll.checked) {
        pageRecords.forEach(record => editModeState.selectedRecords.add(record.id));
    } else {
        pageRecords.forEach(record => editModeState.selectedRecords.delete(record.id));
    }

    renderHistoryGrid();
    updateStatistics();
    updateDeleteButton();
}

// Update delete button state
function updateDeleteButton() {
    const deleteBtn = document.getElementById('deleteBtn');
    if (deleteBtn) {
        deleteBtn.disabled = editModeState.selectedRecords.size === 0;
    }
}

// Update statistics
function updateStatistics() {
    const now = new Date();
    const weekAgo = new Date(now);
    weekAgo.setDate(weekAgo.getDate() - 7);

    const todayRecords = editModeState.allRecords.filter(r =>
        r.date.toDateString() === now.toDateString()
    ).length;

    const weekRecords = editModeState.allRecords.filter(r => r.date >= weekAgo).length;

    document.getElementById('statTotal').textContent = editModeState.allRecords.length;
    document.getElementById('statWeek').textContent = weekRecords;
    document.getElementById('statToday').textContent = todayRecords;
    document.getElementById('statSelected').textContent = editModeState.selectedRecords.size;
}

// Pagination
function previousPageEdit() {
    if (editModeState.currentPage > 1) {
        editModeState.currentPage--;
        renderHistoryGrid();
    }
}

function nextPageEdit() {
    const totalPages = Math.ceil(editModeState.filteredRecords.length / editModeState.recordsPerPage);
    if (editModeState.currentPage < totalPages) {
        editModeState.currentPage++;
        renderHistoryGrid();
    }
}

// Open edit dialog
function openEditDialog(recordId) {
    const record = editModeState.allRecords.find(r => r.id === recordId);
    if (!record) return;

    editModeState.currentEditId = recordId;

    document.getElementById('editType').value = record.type;
    document.getElementById('editPartId').value = record.partId;
    document.getElementById('editQuantity').value = record.quantity;
    document.getElementById('editPO').value = record.poNumber;
    document.getElementById('editLocation').value = record.location;

    document.getElementById('editDialog').classList.remove('hidden');
}

// Close edit dialog
function closeEditDialog() {
    document.getElementById('editDialog').classList.add('hidden');
    editModeState.currentEditId = null;
}

// Save edit
function saveEdit() {
    if (!editModeState.currentEditId) return;

    const record = editModeState.allRecords.find(r => r.id === editModeState.currentEditId);
    if (!record) return;

    record.type = document.getElementById('editType').value;
    record.partId = document.getElementById('editPartId').value;
    record.quantity = parseInt(document.getElementById('editQuantity').value);
    record.poNumber = document.getElementById('editPO').value;
    record.location = document.getElementById('editLocation').value;

    console.log('Updated record:', record);

    closeEditDialog();
    filterRecords(); // Re-apply filters and re-render

    alert('Record updated successfully!');
}

// Delete selected records
function deleteSelected() {
    if (editModeState.selectedRecords.size === 0) return;

    if (!confirm(`Are you sure you want to delete ${editModeState.selectedRecords.size} record(s)? This action cannot be undone.`)) {
        return;
    }

    editModeState.allRecords = editModeState.allRecords.filter(r =>
        !editModeState.selectedRecords.has(r.id)
    );

    editModeState.selectedRecords.clear();
    filterRecords();

    alert('Selected records deleted successfully!');
}

// Export to CSV
function exportToCSV() {
    const records = editModeState.filteredRecords.length > 0
        ? editModeState.filteredRecords
        : editModeState.allRecords;

    let csv = 'ID,Date,Type,Part ID,Quantity,PO Number,Location,User\n';

    records.forEach(record => {
        csv += `${record.id},`;
        csv += `${record.date.toLocaleDateString()},`;
        csv += `"${record.type}",`;
        csv += `"${record.partId}",`;
        csv += `${record.quantity},`;
        csv += `"${record.poNumber || ''}",`;
        csv += `"${record.location}",`;
        csv += `"${record.user}"\n`;
    });

    // Create download
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `dunnage-history-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);

    alert(`Exported ${records.length} record(s) to CSV!`);
}

// Navigate back
function goBackToModeSelection() {
    window.location.href = 'dunnage-wizard.html';
}

// Load data from different sources
function loadFromMemory() {
    alert('Load from Current Memory would load active session data (not implemented in mockup)');
    // In real app: Load from wizardState.sessionLoads
}

function loadFromLabels() {
    alert('Load from Current Labels would import from last CSV export (not implemented in mockup)');
    // In real app: Parse and load from CSV file
}

function loadFromHistory() {
    alert('Load from History would query database with current filters (not implemented in mockup)');
    // In real app: Call IService_MySQL_Dunnage.GetLoadsByDateRangeAsync
}

// Select all records (visible on current page or all filtered)
function selectAllRecords() {
    const startIndex = (editModeState.currentPage - 1) * editModeState.recordsPerPage;
    const endIndex = Math.min(startIndex + editModeState.recordsPerPage, editModeState.filteredRecords.length);
    const pageRecords = editModeState.filteredRecords.slice(startIndex, endIndex);

    pageRecords.forEach(record => editModeState.selectedRecords.add(record.id));
    renderHistoryGrid();
    updateStatistics();
    updateDeleteButton();
}

// Pagination helpers
function firstPageEdit() {
    editModeState.currentPage = 1;
    renderHistoryGrid();
}

function lastPageEdit() {
    const totalPages = Math.ceil(editModeState.filteredRecords.length / editModeState.recordsPerPage);
    editModeState.currentPage = Math.max(1, totalPages);
    renderHistoryGrid();
}

function goToPage() {
    const input = document.getElementById('pageInput');
    const pageNum = parseInt(input.value);
    const totalPages = Math.ceil(editModeState.filteredRecords.length / editModeState.recordsPerPage);

    if (!isNaN(pageNum) && pageNum >= 1 && pageNum <= totalPages) {
        editModeState.currentPage = pageNum;
        renderHistoryGrid();
    } else {
        alert(`Please enter a page number between 1 and ${totalPages}`);
        input.value = editModeState.currentPage;
    }
}

// Save all edits
function saveAllEdits() {
    if (editModeState.selectedRecords.size === 0 && !confirm('No records selected. Save all visible records?')) {
        return;
    }

    alert('Save & Finish would commit all edits to database and export to CSV (not implemented in mockup)');
    // In real app: Call IService_MySQL_Dunnage.UpdateLoadsAsync
}

// Initialize on load
document.addEventListener('DOMContentLoaded', () => {
    initEditMode();
});
