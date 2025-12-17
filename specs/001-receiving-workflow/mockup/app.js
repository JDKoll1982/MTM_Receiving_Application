// Mock Database
let mockDatabase = null;

// Application State
const appState = {
    currentStep: 1,
    selectedPart: null,
    loads: [],
    allLoads: [], // For multiple parts
    currentPO: null,
    isNonPO: false,
    session: {
        parts: []
    }
};

// Load mock data
async function loadMockData() {
    const response = await fetch('mockData.json');
    mockDatabase = await response.json();
}

// Initialize application
document.addEventListener('DOMContentLoaded', async () => {
    await loadMockData();
    
    // Show CSV reset dialog on startup
    showCSVResetDialog();
    
    setupEventListeners();
    updateStepDisplay();
});

function setupEventListeners() {
    // Step 1: PO Entry
    document.getElementById('loadPOBtn').addEventListener('click', loadPO);
    document.getElementById('nonPOBtn').addEventListener('click', showNonPOForm);
    document.getElementById('lookupPartBtn').addEventListener('click', lookupPart);
    document.getElementById('continueFromNonPO').addEventListener('click', continueFromNonPO);
    
    // Step 2: Load Number
    document.getElementById('createLoadsBtn').addEventListener('click', createLoads);
    
    // Step 3: Weight/Quantity
    document.getElementById('continueToHeatBtn').addEventListener('click', () => goToStep(4));
    
    // Step 4: Heat/Lot#
    document.getElementById('continueToPackagesBtn').addEventListener('click', () => goToStep(5));
    
    // Step 5: Package Type
    document.getElementById('continueToReviewBtn').addEventListener('click', () => {
        calculateWeightPerPackage();
        goToStep(6);
        buildReviewGrid();
    });
    
    // Step 6: Review
    document.getElementById('addAnotherPartBtn').addEventListener('click', addAnotherPart);
    document.getElementById('saveDataBtn').addEventListener('click', saveData);
    
    // Step 8: Complete
    document.getElementById('newEntryBtn').addEventListener('click', resetApplication);
    document.getElementById('viewHistoryBtn').addEventListener('click', () => showStatus('View History feature not implemented in mockup', 'info'));
    document.getElementById('printLabelsBtn').addEventListener('click', () => showStatus('Print Labels feature not implemented in mockup', 'info'));
    
    // CSV Reset Dialog
    document.getElementById('resetYesBtn').addEventListener('click', resetCSVFiles);
    document.getElementById('resetNoBtn').addEventListener('click', continueWithExisting);
    
    // PO Number input validation
    document.getElementById('poNumber').addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/\D/g, '').slice(0, 6);
    });
}

function showCSVResetDialog() {
    document.getElementById('csvResetDialog').classList.remove('hidden');
}

function resetCSVFiles() {
    document.getElementById('csvResetDialog').classList.add('hidden');
    showStatus('CSV files reset successfully', 'success');
    console.log('Local and network CSV files deleted');
}

function continueWithExisting() {
    document.getElementById('csvResetDialog').classList.add('hidden');
    showStatus('Continuing with existing CSV files', 'info');
}

function loadPO() {
    const poNumber = document.getElementById('poNumber').value;
    
    if (!poNumber || poNumber.length !== 6) {
        showStatus('Please enter a valid 6-digit PO number', 'error');
        return;
    }
    
    // Find PO in mock database
    const po = mockDatabase.purchaseOrders.find(p => p.poNumber === poNumber);
    
    if (!po) {
        showModal(
            'PO Not Found',
            `PO ${poNumber} not found in Visual. Please verify the PO number and try again, or click "Non-PO Item" to proceed with manual entry.`,
            () => {},
            'OK'
        );
        return;
    }
    
    if (po.parts.length === 0) {
        showStatus(`PO ${poNumber} contains no parts`, 'error');
        return;
    }
    
    appState.currentPO = po;
    displayParts(po);
    showStatus(`PO ${poNumber} loaded. ${po.parts.length} parts found.`, 'success');
}

function displayParts(po) {
    const tbody = document.getElementById('partsTableBody');
    tbody.innerHTML = '';
    
    po.parts.forEach((part, index) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td><button class="btn btn-primary select-part-btn" data-index="${index}">Select</button></td>
            <td>${part.partID}</td>
            <td>${part.poLineNumber}</td>
            <td>${part.partType}</td>
            <td>${part.qtyOrdered}</td>
            <td>${part.description}</td>
        `;
        tbody.appendChild(row);
    });
    
    // Add event listeners to select buttons
    document.querySelectorAll('.select-part-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const index = parseInt(this.dataset.index);
            selectPart(index);
        });
    });
    
    document.getElementById('poResults').classList.remove('hidden');
}

function showNonPOForm() {
    document.getElementById('nonPOForm').classList.remove('hidden');
    document.getElementById('poResults').classList.add('hidden');
    appState.isNonPO = true;
}

function lookupPart() {
    const partID = document.getElementById('partID').value.trim();
    
    if (!partID) {
        showStatus('Please enter a Part ID', 'error');
        return;
    }
    
    // Search for part in mock database
    const part = mockDatabase.parts.find(p => p.partID === partID);
    
    if (!part) {
        showStatus(`Part ID ${partID} not found in Visual. Please verify and try again.`, 'error');
        return;
    }
    
    // Display part details
    document.getElementById('partType').textContent = part.partType;
    document.getElementById('partDescription').textContent = part.description;
    document.getElementById('partDetails').classList.remove('hidden');
    
    appState.selectedPart = {
        partID: part.partID,
        partType: part.partType,
        description: part.description,
        poLineNumber: 'N/A',
        qtyOrdered: null
    };
    
    showStatus('Part found in Visual database', 'success');
}

function continueFromNonPO() {
    if (!appState.selectedPart) {
        showStatus('Please look up a part first', 'error');
        return;
    }
    
    goToStep(2);
    displaySelectedPartInfo();
}

function selectPart(index) {
    const part = appState.currentPO.parts[index];
    appState.selectedPart = {
        ...part,
        poNumber: appState.currentPO.poNumber
    };
    appState.isNonPO = false;
    
    goToStep(2);
    displaySelectedPartInfo();
}

function displaySelectedPartInfo() {
    document.getElementById('selectedPartID').textContent = appState.selectedPart.partID;
    document.getElementById('selectedPOLine').textContent = appState.selectedPart.poLineNumber;
}

function createLoads() {
    const numLoads = parseInt(document.getElementById('numLoads').value);
    
    if (!numLoads || numLoads < 1 || numLoads > 99) {
        showStatus('Number of loads must be between 1 and 99', 'error');
        return;
    }
    
    // Create load entries
    appState.loads = [];
    for (let i = 1; i <= numLoads; i++) {
        appState.loads.push({
            loadNumber: i,
            partID: appState.selectedPart.partID,
            partType: appState.selectedPart.partType,
            poNumber: appState.selectedPart.poNumber || null,
            poLineNumber: appState.selectedPart.poLineNumber,
            weightQuantity: 0,
            heatLotNumber: '',
            packagesPerLoad: 0,
            packageType: getDefaultPackageType(appState.selectedPart.partID),
            weightPerPackage: 0
        });
    }
    
    goToStep(3);
    buildWeightQuantityForm();
    showStatus(`Created ${numLoads} load entries`, 'success');
}

function getDefaultPackageType(partID) {
    if (partID.startsWith('MMC')) return 'Coils';
    if (partID.startsWith('MMF')) return 'Sheets';
    return '';
}

function buildWeightQuantityForm() {
    const container = document.getElementById('weightQuantityForm');
    container.innerHTML = '';
    
    appState.loads.forEach((load, index) => {
        const div = document.createElement('div');
        div.className = 'load-entry';
        div.innerHTML = `
            <h4>Load ${load.loadNumber}</h4>
            <label>Weight/Quantity (lbs):</label>
            <input type="number" class="weight-input" data-index="${index}" min="0" step="0.01" value="${load.weightQuantity}">
        `;
        container.appendChild(div);
    });
    
    // Add event listeners
    document.querySelectorAll('.weight-input').forEach(input => {
        input.addEventListener('input', validateWeights);
    });
}

function validateWeights() {
    const inputs = document.querySelectorAll('.weight-input');
    let allValid = true;
    let totalWeight = 0;
    
    inputs.forEach(input => {
        const index = input.dataset.index;
        const value = parseFloat(input.value) || 0;
        appState.loads[index].weightQuantity = value;
        totalWeight += value;
        
        if (value <= 0) {
            allValid = false;
        }
    });
    
    // Validate against PO ordered quantity (if PO item)
    if (!appState.isNonPO && appState.selectedPart.qtyOrdered) {
        if (totalWeight > appState.selectedPart.qtyOrdered) {
            showModal(
                'Quantity Warning',
                `Total quantity (${totalWeight} lbs) exceeds PO ordered quantity (${appState.selectedPart.qtyOrdered} lbs) for part ${appState.selectedPart.partID}. Do you want to continue?`,
                () => {},
                'Yes, Continue',
                'No, Revise'
            );
        }
    }
    
    document.getElementById('continueToHeatBtn').disabled = !allValid;
    
    if (allValid) {
        showStatus('All weights/quantities entered', 'success');
    }
}

function buildHeatLotForm() {
    const container = document.getElementById('heatLotForm');
    container.innerHTML = '';
    
    appState.loads.forEach((load, index) => {
        const div = document.createElement('div');
        div.className = 'load-entry';
        div.innerHTML = `
            <h4>Load ${load.loadNumber}</h4>
            <label>Heat/Lot Number:</label>
            <input type="text" class="heat-input" data-index="${index}" value="${load.heatLotNumber}">
        `;
        container.appendChild(div);
    });
    
    // Add event listeners
    document.querySelectorAll('.heat-input').forEach(input => {
        input.addEventListener('input', updateHeatNumbers);
    });
    
    buildQuickSelectList();
}

function updateHeatNumbers() {
    const inputs = document.querySelectorAll('.heat-input');
    
    inputs.forEach(input => {
        const index = input.dataset.index;
        appState.loads[index].heatLotNumber = input.value;
    });
    
    buildQuickSelectList();
}

function buildQuickSelectList() {
    const container = document.getElementById('quickSelectList');
    container.innerHTML = '';
    
    // Get unique heat numbers
    const uniqueHeats = [...new Set(appState.loads.map(l => l.heatLotNumber).filter(h => h))];
    
    if (uniqueHeats.length === 0) {
        container.innerHTML = '<p style="color: #7f8c8d;">Enter a heat/lot number to see quick select options</p>';
        return;
    }
    
    uniqueHeats.forEach(heat => {
        const div = document.createElement('div');
        div.className = 'quick-select-item';
        div.innerHTML = `
            <input type="checkbox" id="heat-${heat}" onchange="applyHeatToAll('${heat}', this.checked)">
            <label for="heat-${heat}">${heat}</label>
        `;
        container.appendChild(div);
    });
}

window.applyHeatToAll = function(heatNumber, apply) {
    if (!apply) return;
    
    appState.loads.forEach((load, index) => {
        if (!load.heatLotNumber) {
            load.heatLotNumber = heatNumber;
            const input = document.querySelector(`.heat-input[data-index="${index}"]`);
            if (input) input.value = heatNumber;
        }
    });
    
    showStatus(`Applied heat/lot# ${heatNumber} to empty loads`, 'success');
}

function buildPackageForm() {
    const container = document.getElementById('packageForm');
    container.innerHTML = '';
    
    appState.loads.forEach((load, index) => {
        const div = document.createElement('div');
        div.className = 'load-entry';
        div.innerHTML = `
            <h4>Load ${load.loadNumber}</h4>
            <label>Package Type:</label>
            <select class="package-type-select" data-index="${index}">
                <option value="Coils" ${load.packageType === 'Coils' ? 'selected' : ''}>Coils</option>
                <option value="Sheets" ${load.packageType === 'Sheets' ? 'selected' : ''}>Sheets</option>
                <option value="Boxes" ${load.packageType === 'Boxes' ? 'selected' : ''}>Boxes</option>
                <option value="Custom" ${load.packageType === 'Custom' ? 'selected' : ''}>Custom</option>
            </select>
            <div class="custom-package-input" style="display: ${load.packageType === 'Custom' ? 'block' : 'none'}; margin-top: 10px;">
                <label>Custom Package Type:</label>
                <input type="text" class="custom-package-name" data-index="${index}" placeholder="Enter custom type">
            </div>
            <label style="margin-top: 15px;">Packages per Load:</label>
            <input type="number" class="packages-input" data-index="${index}" min="1" value="${load.packagesPerLoad || ''}">
        `;
        container.appendChild(div);
    });
    
    // Add event listeners
    document.querySelectorAll('.package-type-select').forEach(select => {
        select.addEventListener('change', handlePackageTypeChange);
    });
    
    document.querySelectorAll('.packages-input').forEach(input => {
        input.addEventListener('input', updatePackages);
    });
}

function handlePackageTypeChange(e) {
    const index = e.target.dataset.index;
    const value = e.target.value;
    appState.loads[index].packageType = value;
    
    const customInput = e.target.parentElement.querySelector('.custom-package-input');
    if (value === 'Custom') {
        customInput.style.display = 'block';
    } else {
        customInput.style.display = 'none';
    }
}

function updatePackages() {
    const inputs = document.querySelectorAll('.packages-input');
    
    inputs.forEach(input => {
        const index = input.dataset.index;
        appState.loads[index].packagesPerLoad = parseInt(input.value) || 0;
    });
}

function calculateWeightPerPackage() {
    appState.loads.forEach(load => {
        if (load.packagesPerLoad > 0) {
            load.weightPerPackage = (load.weightQuantity / load.packagesPerLoad).toFixed(2);
        } else {
            load.weightPerPackage = 0;
        }
    });
}

function buildReviewGrid() {
    const tbody = document.getElementById('reviewGridBody');
    tbody.innerHTML = '';
    
    // Include all loads from session if adding multiple parts
    const allLoads = appState.allLoads.length > 0 
        ? [...appState.allLoads, ...appState.loads]
        : appState.loads;
    
    allLoads.forEach((load, index) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td contenteditable="true" data-field="partID" data-index="${index}">${load.partID}</td>
            <td contenteditable="true" data-field="poNumber" data-index="${index}">${load.poNumber || 'N/A'}</td>
            <td>${load.poLineNumber}</td>
            <td>${load.loadNumber}</td>
            <td contenteditable="true" data-field="weightQuantity" data-index="${index}">${load.weightQuantity}</td>
            <td contenteditable="true" data-field="heatLotNumber" data-index="${index}">${load.heatLotNumber}</td>
            <td contenteditable="true" data-field="packagesPerLoad" data-index="${index}">${load.packagesPerLoad}</td>
            <td contenteditable="true" data-field="packageType" data-index="${index}">${load.packageType}</td>
            <td>${load.weightPerPackage} lbs per ${load.packageType}</td>
        `;
        tbody.appendChild(row);
    });
    
    // Add event listeners for editable cells
    document.querySelectorAll('#reviewGridBody td[contenteditable]').forEach(cell => {
        cell.addEventListener('blur', handleCellEdit);
    });
}

function handleCellEdit(e) {
    const field = e.target.dataset.field;
    const index = parseInt(e.target.dataset.index);
    const newValue = e.target.textContent.trim();
    const allLoads = appState.allLoads.length > 0 ? [...appState.allLoads, ...appState.loads] : appState.loads;
    const load = allLoads[index];
    const oldValue = load[field];
    
    // Cascading updates
    if (field === 'partID') {
        // Update all loads with the same old part ID
        const oldPartID = load.partID;
        allLoads.forEach(l => {
            if (l.partID === oldPartID) {
                l.partID = newValue;
            }
        });
        showStatus(`Updated part ID from ${oldPartID} to ${newValue} for all matching loads`, 'success');
        buildReviewGrid();
    } else if (field === 'poNumber') {
        // Update all loads with the same part ID
        const partID = load.partID;
        allLoads.forEach(l => {
            if (l.partID === partID) {
                l.poNumber = newValue;
            }
        });
        showStatus(`Updated PO number to ${newValue} for all loads of part ${partID}`, 'success');
        buildReviewGrid();
    } else {
        // Regular field update
        load[field] = newValue;
        
        // Recalculate weight per package if relevant
        if (field === 'weightQuantity' || field === 'packagesPerLoad') {
            if (load.packagesPerLoad > 0) {
                load.weightPerPackage = (parseFloat(load.weightQuantity) / parseInt(load.packagesPerLoad)).toFixed(2);
            }
            buildReviewGrid();
        }
    }
}

function addAnotherPart() {
    // Add current loads to session
    appState.allLoads.push(...appState.loads);
    appState.session.parts.push({
        partID: appState.selectedPart.partID,
        loads: [...appState.loads]
    });
    
    // Reset for next part
    appState.loads = [];
    appState.selectedPart = null;
    appState.currentStep = 1;
    
    // Reset forms
    document.getElementById('poNumber').value = '';
    document.getElementById('partID').value = '';
    document.getElementById('poResults').classList.add('hidden');
    document.getElementById('nonPOForm').classList.add('hidden');
    document.getElementById('partDetails').classList.add('hidden');
    
    goToStep(1);
    showStatus('Part saved to session. Enter next part or PO.', 'info');
}

async function saveData() {
    goToStep(7);
    
    const allLoads = appState.allLoads.length > 0 ? [...appState.allLoads, ...appState.loads] : appState.loads;
    
    // Simulate saving to local CSV
    await simulateSave('Local CSV', 'progressLocal', 'statusLocal');
    
    // Simulate saving to network CSV
    await simulateSave('Network CSV', 'progressNetwork', 'statusNetwork');
    
    // Simulate saving to database
    await simulateSave('MySQL Database', 'progressDatabase', 'statusDatabase');
    
    // Show completion
    document.getElementById('totalLoads').textContent = allLoads.length;
    goToStep(8);
    
    // Save to mock JSON (in real app, this would be backend call)
    console.log('Saved data:', allLoads);
}

function simulateSave(name, progressId, statusId) {
    return new Promise((resolve) => {
        const progressBar = document.getElementById(progressId);
        const statusText = document.getElementById(statusId);
        
        statusText.textContent = 'In progress...';
        statusText.style.color = '#3498db';
        
        let progress = 0;
        const interval = setInterval(() => {
            progress += 10;
            progressBar.style.width = progress + '%';
            
            if (progress >= 100) {
                clearInterval(interval);
                statusText.textContent = 'Complete ✓';
                statusText.style.color = '#2ecc71';
                resolve();
            }
        }, 200);
    });
}

function resetApplication() {
    appState.currentStep = 1;
    appState.selectedPart = null;
    appState.loads = [];
    appState.allLoads = [];
    appState.currentPO = null;
    appState.isNonPO = false;
    appState.session = { parts: [] };
    
    // Reset all forms
    document.getElementById('poNumber').value = '';
    document.getElementById('numLoads').value = '1';
    document.getElementById('partID').value = '';
    
    goToStep(1);
    showStatus('Application reset. Starting new entry.', 'info');
}

function goToStep(stepNumber) {
    // Hide all steps
    document.querySelectorAll('.step-container').forEach(container => {
        container.classList.remove('active');
        container.classList.add('hidden');
    });
    
    // Show target step
    const targetStep = document.getElementById(`step${stepNumber}`);
    targetStep.classList.remove('hidden');
    targetStep.classList.add('active');
    appState.currentStep = stepNumber;
    
    // Update step display
    updateStepDisplay();
    
    // Build forms for specific steps
    if (stepNumber === 4) buildHeatLotForm();
    if (stepNumber === 5) buildPackageForm();
    
    // Scroll to top
    window.scrollTo(0, 0);
}

function updateStepDisplay() {
    const stepNames = [
        'PO Entry',
        'Part/PO-Line#',
        'Load Number/Skid Amount',
        'Weight/Quantity',
        'Heat/Lot#',
        'Package Type per Load',
        'Review & Edit',
        'Saving Data',
        'Complete'
    ];
    
    document.getElementById('currentStep').textContent = 
        `Step ${appState.currentStep}: ${stepNames[appState.currentStep - 1] || ''}`;
}

function showStatus(message, type = 'info') {
    const container = document.getElementById('statusMessages');
    const div = document.createElement('div');
    div.className = `status-message ${type}`;
    div.textContent = message;
    
    container.appendChild(div);
    
    // Auto-remove after 4 seconds
    setTimeout(() => {
        div.remove();
    }, 4000);
}

function showModal(title, message, onConfirm, confirmText = 'Confirm', cancelText = 'Cancel') {
    const modal = document.getElementById('modal');
    document.getElementById('modalTitle').textContent = title;
    document.getElementById('modalMessage').textContent = message;
    
    const confirmBtn = document.getElementById('modalConfirm');
    const cancelBtn = document.getElementById('modalCancel');
    
    confirmBtn.textContent = confirmText;
    cancelBtn.textContent = cancelText;
    
    confirmBtn.onclick = () => {
        modal.classList.add('hidden');
        if (onConfirm) onConfirm();
    };
    
    cancelBtn.onclick = () => {
        modal.classList.add('hidden');
    };
    
    modal.classList.remove('hidden');
}
