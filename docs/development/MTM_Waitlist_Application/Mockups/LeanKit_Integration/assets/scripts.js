// MTM Waitlist - LeanKit Integration Mockup Scripts

// Mock Data Storage
const mockData = {
    auth: {
        account: 'mtmfg',
        token: '2a0ec41e4fca6a10727a33je4f409545870c0ce199fd8cde287a027acdf671d73da3f5af1ee983441d1993dc3a2181302690885c0b46692e39b6c9e29bd132eb',
        apiUrl: 'https://mtmfg.leankit.com/io',
        boardId: '1490772554',
        connected: true
    },
    waitlistEntries: [
        { id: 101, customer: 'Acme Corp', po: 'PO-12345', status: 'In Progress', priority: 'High', leankitCardId: 'card-789', syncStatus: 'synced', lastSync: '10:45 AM' },
        { id: 102, customer: 'GlobalTech', po: 'PO-12346', status: 'Pending', priority: 'Medium', leankitCardId: null, syncStatus: 'pending', lastSync: null },
        { id: 103, customer: 'MegaSupply', po: 'PO-12347', status: 'On Hold', priority: 'Low', leankitCardId: 'card-456', syncStatus: 'error', lastSync: '9:30 AM' },
        { id: 104, customer: 'TechStart', po: 'PO-12348', status: 'Completed', priority: 'High', leankitCardId: 'card-123', syncStatus: 'synced', lastSync: '11:15 AM' }
    ],
    lanes: [
        { id: 'lane-1', name: 'Backlog' },
        { id: 'lane-2', name: 'In Progress' },
        { id: 'lane-3', name: 'Blocked' },
        { id: 'lane-4', name: 'Done' }
    ],
    syncHistory: [
        { time: '10:55 AM', entry: '#101', direction: 'Waitlist→LK', action: 'Update', status: 'success' },
        { time: '10:55 AM', entry: '#102', direction: 'LK→Waitlist', action: 'Update', status: 'success' },
        { time: '10:40 AM', entry: '#103', direction: 'Waitlist→LK', action: 'Create', status: 'error' },
        { time: '10:25 AM', entry: '#104', direction: 'Bidirectional', action: 'Update', status: 'warning' },
        { time: '10:10 AM', entry: '#105', direction: 'Waitlist→LK', action: 'Update', status: 'success' },
        { time: '09:55 AM', entry: '#106', direction: 'LK→Waitlist', action: 'Update', status: 'success' }
    ]
};

// Utility Functions
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        background: ${type === 'success' ? '#107C10' : type === 'error' ? '#E74856' : '#0078D4'};
        color: white;
        border-radius: 4px;
        box-shadow: 0 6.4px 14.4px rgba(0, 0, 0, 0.13);
        z-index: 10000;
        animation: slideIn 0.3s ease;
    `;
    document.body.appendChild(toast);
    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

function formatTime(date) {
    return new Intl.DateTimeFormat('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    }).format(date);
}

// Test Connection
function testConnection() {
    const statusElement = document.getElementById('connection-status');
    const testBtn = document.getElementById('test-connection-btn');
    
    if (!statusElement || !testBtn) return;
    
    testBtn.disabled = true;
    testBtn.textContent = 'Testing...';
    statusElement.className = 'connection-status';
    statusElement.textContent = 'Testing...';
    
    setTimeout(() => {
        mockData.auth.connected = true;
        statusElement.className = 'connection-status connected';
        statusElement.textContent = '● Connected';
        testBtn.disabled = false;
        testBtn.textContent = 'Test Connection';
        showToast('Successfully connected to LeanKit!', 'success');
    }, 1500);
}

// Save Configuration
function saveConfiguration() {
    const account = document.getElementById('account-name')?.value;
    const apiUrl = document.getElementById('api-url')?.value;
    const boardId = document.getElementById('board-id')?.value;
    const token = document.getElementById('api-token')?.value;
    
    if (account) mockData.auth.account = account;
    if (apiUrl) mockData.auth.apiUrl = apiUrl;
    if (boardId) mockData.auth.boardId = boardId;
    if (token) mockData.auth.token = token;
    
    showToast('Settings saved successfully!', 'success');
    setTimeout(() => {
        window.location.href = 'index.html';
    }, 1500);
}

// Sync Single Entry
function syncEntry(entryId) {
    const entry = mockData.waitlistEntries.find(e => e.id === entryId);
    if (!entry) return;
    
    showToast(`Syncing entry #${entryId} to LeanKit...`, 'info');
    
    setTimeout(() => {
        entry.syncStatus = 'synced';
        entry.lastSync = formatTime(new Date());
        entry.leankitCardId = `card-${Math.floor(Math.random() * 1000)}`;
        
        // Update UI if on waitlist page
        const statusBadge = document.getElementById(`sync-status-${entryId}`);
        if (statusBadge) {
            statusBadge.className = 'badge badge-success';
            statusBadge.innerHTML = '<span class="status-icon status-icon-success"></span> Synced';
        }
        
        const lastSyncCell = document.getElementById(`last-sync-${entryId}`);
        if (lastSyncCell) {
            lastSyncCell.textContent = entry.lastSync;
        }
        
        showToast(`Entry #${entryId} synced successfully!`, 'success');
    }, 2000);
}

// Sync All Entries
function syncAll() {
    const syncBtn = document.getElementById('sync-now-btn');
    if (syncBtn) {
        syncBtn.disabled = true;
        syncBtn.textContent = 'Syncing...';
    }
    
    showToast('Starting full synchronization...', 'info');
    
    let completed = 0;
    const total = mockData.waitlistEntries.filter(e => e.syncStatus !== 'synced').length;
    
    const interval = setInterval(() => {
        if (completed >= total) {
            clearInterval(interval);
            if (syncBtn) {
                syncBtn.disabled = false;
                syncBtn.textContent = 'Sync Now';
            }
            showToast('Synchronization completed!', 'success');
            updateSyncStats();
            return;
        }
        
        const entry = mockData.waitlistEntries.find(e => e.syncStatus !== 'synced');
        if (entry) {
            entry.syncStatus = 'synced';
            entry.lastSync = formatTime(new Date());
            completed++;
        }
    }, 1000);
}

// Update Sync Stats
function updateSyncStats() {
    const totalSyncs = document.getElementById('total-syncs');
    const successfulSyncs = document.getElementById('successful-syncs');
    const failedSyncs = document.getElementById('failed-syncs');
    
    if (totalSyncs) totalSyncs.textContent = '48';
    if (successfulSyncs) successfulSyncs.textContent = '46 (95.8%)';
    if (failedSyncs) failedSyncs.textContent = '2 (4.2%)';
}

// Retry Failed Sync
function retrySync(entryId) {
    showToast(`Retrying sync for entry #${entryId}...`, 'info');
    syncEntry(entryId);
}

// Resolve Conflict
function resolveConflict(choice) {
    showToast(`Conflict resolved using ${choice} version`, 'success');
    setTimeout(() => {
        window.location.href = '3_sync_dashboard.html';
    }, 1500);
}

// View in LeanKit
function viewInLeanKit(cardId) {
    const url = `https://mtmfg.leankit.com/board/${mockData.auth.boardId}/card/${cardId}`;
    showToast('Opening LeanKit in browser...', 'info');
    setTimeout(() => {
        window.open(url, '_blank');
    }, 500);
}

// Enable/Disable Auto-Sync
function toggleAutoSync() {
    const checkbox = document.getElementById('enable-autosync');
    const status = document.getElementById('autosync-status');
    
    if (checkbox && status) {
        if (checkbox.checked) {
            status.textContent = '● ENABLED';
            status.style.color = '#107C10';
            showToast('Auto-sync enabled', 'success');
        } else {
            status.textContent = '○ DISABLED';
            status.style.color = '#A19F9D';
            showToast('Auto-sync disabled', 'info');
        }
    }
}

// Load Sync History
function loadSyncHistory() {
    const tbody = document.getElementById('sync-history-tbody');
    if (!tbody) return;
    
    tbody.innerHTML = '';
    mockData.syncHistory.forEach(record => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${record.time}</td>
            <td>${record.entry}</td>
            <td>${record.direction}</td>
            <td>${record.action}</td>
            <td>
                <span class="badge badge-${record.status === 'success' ? 'success' : record.status === 'error' ? 'error' : 'warning'}">
                    ${record.status === 'success' ? '✓' : record.status === 'error' ? '✗' : '⚠'}
                </span>
            </td>
            <td><button class="btn-link" onclick="viewSyncDetails('${record.entry}')">View</button></td>
        `;
        tbody.appendChild(row);
    });
}

// View Sync Details
function viewSyncDetails(entryId) {
    showToast(`Viewing details for ${entryId}`, 'info');
}

// Refresh Lanes
function refreshLanes() {
    showToast('Refreshing lane data from LeanKit...', 'info');
    setTimeout(() => {
        showToast('Lane data updated successfully!', 'success');
    }, 1500);
}

// Copy Error Details
function copyErrorDetails() {
    const errorText = `
Error Type: NetworkError
Occurred At: 2026-01-20 10:40:15 AM
Entry: #103 - MegaSupply (PO #12347)
Message: The operation has timed out after 30 seconds.
Failed to establish connection to LeanKit API.
Endpoint: https://mtmfg.leankit.com/io/card
HTTP Status: 504 Gateway Timeout
    `.trim();
    
    navigator.clipboard.writeText(errorText).then(() => {
        showToast('Error details copied to clipboard', 'success');
    });
}

// Initialize Page
document.addEventListener('DOMContentLoaded', () => {
    // Load sync history if on dashboard page
    loadSyncHistory();
    
    // Update last sync time
    const lastSyncElements = document.querySelectorAll('.last-sync-time');
    lastSyncElements.forEach(el => {
        el.textContent = formatTime(new Date());
    });
});

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    .btn-link:hover {
        text-decoration: underline;
    }
`;
document.head.appendChild(style);
