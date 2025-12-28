// Wizard State Management
let wizardState = {
    currentStep: 'modeSelection',
    selectedMode: null,
    selectedType: null,
    selectedPart: null,
    quantity: 1,
    poNumber: '',
    location: '',
    specs: {},
    sessionLoads: [],

    // Pagination state
    currentPage: 1,
    typesPerPage: 9,

    // Default mode preference
    defaultMode: localStorage.getItem('defaultDunnageMode') || null
};

// Step Management
const steps = [
    'modeSelection',
    'typeSelection',
    'partSelection',
    'quantityEntry',
    'detailsEntry',
    'reviewSave'
];

function showStep(stepName) {
    // Hide all steps
    steps.forEach(step => {
        const element = document.getElementById(step);
        if (element) {
            element.classList.add('hidden');
        }
    });

    // Show success message if needed
    const successElement = document.getElementById('successMessage');
    if (successElement) {
        successElement.classList.add('hidden');
    }

    // Show current step
    const currentElement = document.getElementById(stepName);
    if (currentElement) {
        currentElement.classList.remove('hidden');
    }

    wizardState.currentStep = stepName;

    // Initialize step if needed
    initializeStep(stepName);
}

function initializeStep(stepName) {
    switch (stepName) {
        case 'typeSelection':
            renderTypeGrid();
            break;
        case 'partSelection':
            populatePartDropdown();
            break;
        case 'quantityEntry':
            updateQuantityContext();
            break;
        case 'detailsEntry':
            generateSpecInputs();
            updateDetailsContext();
            break;
        case 'reviewSave':
            renderReviewTable();
            break;
    }
}

// Mode Selection
function selectMode(mode) {
    wizardState.selectedMode = mode;

    if (mode === 'wizard') {
        showStep('typeSelection');
    } else if (mode === 'manual') {
        window.location.href = 'manual-entry.html';
    } else if (mode === 'edit') {
        window.location.href = 'edit-mode.html';
    }
}

// Set default mode preference
function setDefaultMode(mode, isChecked) {
    if (isChecked) {
        // Uncheck other checkboxes (mutually exclusive)
        document.getElementById('guidedDefault').checked = (mode === 'guided');
        document.getElementById('manualDefault').checked = (mode === 'manual');
        document.getElementById('editDefault').checked = (mode === 'edit');

        // Save preference
        localStorage.setItem('defaultDunnageMode', mode);
        wizardState.defaultMode = mode;
    } else {
        // Clear preference if unchecking
        localStorage.removeItem('defaultDunnageMode');
        wizardState.defaultMode = null;
    }
}

// Type Selection with Pagination
function renderTypeGrid() {
    const grid = document.getElementById('typeGrid');
    if (!grid) return;

    grid.innerHTML = '';

    const totalPages = Math.ceil(dunnageTypes.length / wizardState.typesPerPage);
    const startIndex = (wizardState.currentPage - 1) * wizardState.typesPerPage;
    const endIndex = Math.min(startIndex + wizardState.typesPerPage, dunnageTypes.length);

    const typesToShow = dunnageTypes.slice(startIndex, endIndex);

    typesToShow.forEach(type => {
        const button = document.createElement('button');
        button.className = 'type-button';
        button.textContent = type;
        button.onclick = () => selectType(type);
        grid.appendChild(button);
    });

    // Update pagination controls
    document.getElementById('pageInfo').textContent = `Page ${wizardState.currentPage} of ${totalPages}`;
    document.getElementById('prevPageBtn').disabled = wizardState.currentPage === 1;
    document.getElementById('nextPageBtn').disabled = wizardState.currentPage === totalPages;
}

function selectType(type) {
    wizardState.selectedType = type;
    wizardState.currentPage = 1; // Reset for next time
    showStep('partSelection');
}

function previousPage() {
    if (wizardState.currentPage > 1) {
        wizardState.currentPage--;
        renderTypeGrid();
    }
}

function nextPage() {
    const totalPages = Math.ceil(dunnageTypes.length / wizardState.typesPerPage);
    if (wizardState.currentPage < totalPages) {
        wizardState.currentPage++;
        renderTypeGrid();
    }
}

// Part Selection
function populatePartDropdown() {
    const select = document.getElementById('partSelect');
    if (!select) return;

    select.innerHTML = '<option value="">Select a part...</option>';

    const parts = partsByType[wizardState.selectedType] || [];
    parts.forEach(part => {
        const option = document.createElement('option');
        option.value = part.id;
        option.textContent = part.id;
        option.dataset.part = JSON.stringify(part);
        select.appendChild(option);
    });

    // Reset UI
    document.getElementById('partDetailsPanel').classList.add('hidden');
    document.getElementById('inventoryInfoBar').classList.add('hidden');
    document.getElementById('partNextBtn').disabled = true;
}

function handlePartSelection() {
    const select = document.getElementById('partSelect');
    const selectedOption = select.options[select.selectedIndex];

    if (!selectedOption.value) {
        document.getElementById('partDetailsPanel').classList.add('hidden');
        document.getElementById('inventoryInfoBar').classList.add('hidden');
        document.getElementById('partNextBtn').disabled = true;
        return;
    }

    const part = JSON.parse(selectedOption.dataset.part);
    wizardState.selectedPart = part;

    // Update part details panel
    document.getElementById('partType').textContent = wizardState.selectedType;
    document.getElementById('partWidth').textContent = `${part.width} inches`;
    document.getElementById('partHeight').textContent = `${part.height} inches`;
    document.getElementById('partDepth').textContent = `${part.depth} inches`;
    document.getElementById('partInventoried').textContent = part.inventoried ? 'Yes' : 'No';
    document.getElementById('partDetailsPanel').classList.remove('hidden');

    // Show inventory notification if part is inventoried
    if (part.inventoried) {
        document.getElementById('inventoryMethod').textContent = 'Adjust In';
        document.getElementById('inventoryInfoBar').classList.remove('hidden');
    } else {
        document.getElementById('inventoryInfoBar').classList.add('hidden');
    }

    // Enable next button
    document.getElementById('partNextBtn').disabled = false;
}

// Quantity Entry
function updateQuantityContext() {
    document.getElementById('qtyType').textContent = wizardState.selectedType;
    document.getElementById('qtyPart').textContent = wizardState.selectedPart.id;
    document.getElementById('quantityInput').value = wizardState.quantity;
}

function validateQuantity() {
    const input = document.getElementById('quantityInput');
    const value = parseInt(input.value);
    const error = document.getElementById('quantityError');
    const nextBtn = document.getElementById('quantityNextBtn');

    if (isNaN(value) || value <= 0) {
        error.classList.remove('hidden');
        nextBtn.disabled = true;
    } else {
        error.classList.add('hidden');
        nextBtn.disabled = false;
        wizardState.quantity = value;
    }
}

// Details Entry
function updateDetailsContext() {
    // Show inventory info if part is inventoried
    if (wizardState.selectedPart && wizardState.selectedPart.inventoried) {
        const method = wizardState.poNumber ? 'Receive In' : 'Adjust In';
        document.getElementById('detailsInventoryMethod').textContent = method;
        document.getElementById('detailsInfoBar').classList.remove('hidden');
    } else {
        document.getElementById('detailsInfoBar').classList.add('hidden');
    }

    // Set values
    document.getElementById('poNumberInput').value = wizardState.poNumber;
    document.getElementById('locationInput').value = wizardState.location;
}

function handlePOChange() {
    const poInput = document.getElementById('poNumberInput');
    wizardState.poNumber = poInput.value;

    // Update inventory method if part is inventoried
    if (wizardState.selectedPart && wizardState.selectedPart.inventoried) {
        const method = wizardState.poNumber ? 'Receive In' : 'Adjust In';
        document.getElementById('detailsInventoryMethod').textContent = method;
    }
}

function generateSpecInputs() {
    const container = document.getElementById('specInputsContainer');
    if (!container) return;

    container.innerHTML = '';

    const specs = typeSpecs[wizardState.selectedType] || [];

    specs.forEach(spec => {
        const group = document.createElement('div');
        group.className = 'form-group';

        const label = document.createElement('label');
        label.className = 'form-label';
        label.textContent = spec.name + (spec.required ? ' *' : '');
        if (spec.unit) {
            label.textContent += ` (${spec.unit})`;
        }

        let input;
        if (spec.type === 'number') {
            input = document.createElement('input');
            input.type = 'number';
            input.className = 'numberbox';
            input.step = '0.01';
        } else if (spec.type === 'boolean') {
            const checkboxContainer = document.createElement('div');
            checkboxContainer.className = 'checkbox';
            input = document.createElement('input');
            input.type = 'checkbox';
            const checkLabel = document.createElement('label');
            checkLabel.textContent = 'Yes';
            checkboxContainer.appendChild(input);
            checkboxContainer.appendChild(checkLabel);
            group.appendChild(label);
            group.appendChild(checkboxContainer);
            container.appendChild(group);
            return;
        } else {
            input = document.createElement('input');
            input.type = 'text';
            input.className = 'textbox';
        }

        input.id = `spec_${spec.name}`;
        input.dataset.specName = spec.name;

        // Pre-fill with part data if available
        if (wizardState.selectedPart && wizardState.selectedPart[spec.name.toLowerCase()]) {
            input.value = wizardState.selectedPart[spec.name.toLowerCase()];
        }

        group.appendChild(label);
        group.appendChild(input);
        container.appendChild(group);
    });
}

// Review & Save
function renderReviewTable() {
    const tbody = document.getElementById('reviewTableBody');
    if (!tbody) return;

    tbody.innerHTML = '';

    wizardState.sessionLoads.forEach((load, index) => {
        const row = document.createElement('tr');

        const inventoryMethod = load.part.inventoried
            ? (load.poNumber ? 'Receive In' : 'Adjust In')
            : 'N/A';

        row.innerHTML = `
            <td>${index + 1}</td>
            <td>${load.type}</td>
            <td>${load.part.id}</td>
            <td>${load.quantity}</td>
            <td>${load.poNumber || '-'}</td>
            <td>${load.location || '-'}</td>
            <td>${inventoryMethod}</td>
            <td>
                <button class="button" onclick="removeLoad(${index})" style="padding: 4px 8px; font-size: 12px;">
                    🗑️ Remove
                </button>
            </td>
        `;

        tbody.appendChild(row);
    });

    document.getElementById('loadCount').textContent = wizardState.sessionLoads.length;
}

function addAnother() {
    // Save current load first
    saveCurrentLoad();

    // Reset wizard to type selection but keep session loads
    wizardState.selectedType = null;
    wizardState.selectedPart = null;
    wizardState.quantity = 1;
    wizardState.poNumber = '';
    wizardState.location = '';
    wizardState.specs = {};

    showStep('typeSelection');
}

function saveCurrentLoad() {
    // Gather spec values
    const specs = {};
    const specInputs = document.querySelectorAll('[data-spec-name]');
    specInputs.forEach(input => {
        const name = input.dataset.specName;
        if (input.type === 'checkbox') {
            specs[name] = input.checked;
        } else {
            specs[name] = input.value;
        }
    });

    const load = {
        type: wizardState.selectedType,
        part: wizardState.selectedPart,
        quantity: wizardState.quantity,
        poNumber: document.getElementById('poNumberInput')?.value || '',
        location: document.getElementById('locationInput')?.value || '',
        specs: specs
    };

    wizardState.sessionLoads.push(load);
}

function removeLoad(index) {
    wizardState.sessionLoads.splice(index, 1);
    renderReviewTable();
}

function cancelSession() {
    if (confirm('Are you sure you want to cancel? All unsaved loads will be lost.')) {
        resetWizard();
    }
}

function saveAll() {
    if (wizardState.sessionLoads.length === 0) {
        alert('No loads to save. Please add at least one load.');
        return;
    }

    // Simulate saving
    console.log('Saving loads:', wizardState.sessionLoads);

    // Show success message
    document.getElementById('savedCount').textContent = wizardState.sessionLoads.length;
    showStep('successMessage');
    document.getElementById('successMessage').classList.remove('hidden');
}

function resetWizard() {
    wizardState = {
        currentStep: 'modeSelection',
        selectedMode: null,
        selectedType: null,
        selectedPart: null,
        quantity: 1,
        poNumber: '',
        location: '',
        specs: {},
        sessionLoads: [],
        currentPage: 1,
        typesPerPage: 9
    };

    showStep('modeSelection');
}

// Navigation
function goNext() {
    const currentIndex = steps.indexOf(wizardState.currentStep);

    // Save data before moving to next step
    if (wizardState.currentStep === 'detailsEntry') {
        saveCurrentLoad();
    }

    if (currentIndex < steps.length - 1) {
        showStep(steps[currentIndex + 1]);
    }
}

function goBack() {
    const currentIndex = steps.indexOf(wizardState.currentStep);
    if (currentIndex > 0) {
        showStep(steps[currentIndex - 1]);
    }
}

// Quick Add Dialog (stub)
function showQuickAddDialog(itemType) {
    alert(`Quick Add ${itemType} dialog would open here (not implemented in mockup)`);
}

// Initialize wizard on load
document.addEventListener('DOMContentLoaded', () => {
    // Check for default mode and auto-navigate
    const defaultMode = localStorage.getItem('defaultDunnageMode');
    if (defaultMode) {
        // Set checkbox
        const checkboxId = defaultMode === 'guided' ? 'guidedDefault' :
            defaultMode === 'manual' ? 'manualDefault' : 'editDefault';
        const checkbox = document.getElementById(checkboxId);
        if (checkbox) checkbox.checked = true;

        // Auto-navigate to default mode
        selectMode(defaultMode === 'guided' ? 'wizard' : defaultMode);
    } else {
        // Show mode selection
        showStep('modeSelection');
    }
});
