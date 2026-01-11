/*
  Mock data + persistence.
  Everything is localStorage-backed so the mockup is "fully functional".
*/

const STORAGE_KEY = "mtm_waitlist_mock_v1";

export function nowIso() {
    return new Date().toISOString();
}

export function minutesAgo(minutes) {
    return new Date(Date.now() - minutes * 60 * 1000).toISOString();
}

export function defaultState() {
    return {
        meta: {
            role: "Operator", // Operator | MaterialHandler | Lead
            site: "Expo",
            online: true,
            viewScope: "My", // My | Shared
            showMode: "Active", // Active | Recent
            lastFilters: {
                search: "",
                press: "",
                type: "",
                recentRange: "2h" // 2h | 8h | 24h | 7d
            }
        },
        tasks: seedTasks()
    };
}

export function seedTasks() {
    return [
        {
            id: cryptoId(),
            createdAt: minutesAgo(8),
            createdBy: "jdoe",
            site: "Expo",
            scopeOwner: "jdoe",
            status: "Active", // Active | Completed
            sync: "Synced", // Synced | Queued | Failed
            requestType: "Parts Pickup",
            press: "Press 12",
            qty: 1,
            notes: "Need 2x bushings for setup",
            urgency: "Normal", // Normal | Red
            assignedTo: "",
            completedAt: "",
            completedBy: "",
            reopenedAt: "",
            reopenedBy: "",
            audit: [{ at: minutesAgo(8), by: "jdoe", action: "Created" }]
        },
        {
            id: cryptoId(),
            createdAt: minutesAgo(42),
            createdBy: "jdoe",
            site: "Expo",
            scopeOwner: "jdoe",
            status: "Active",
            sync: "Queued",
            requestType: "Coil",
            press: "Press 7",
            qty: 1,
            notes: "Coil swap. Line is waiting.",
            urgency: "Red",
            assignedTo: "",
            completedAt: "",
            completedBy: "",
            reopenedAt: "",
            reopenedBy: "",
            audit: [{ at: minutesAgo(42), by: "jdoe", action: "Created (Offline)" }]
        },
        {
            id: cryptoId(),
            createdAt: minutesAgo(180),
            createdBy: "asmith",
            site: "Expo",
            scopeOwner: "asmith",
            status: "Completed",
            sync: "Synced",
            requestType: "Skids",
            press: "Press 3",
            qty: 4,
            notes: "Bring 4 empty skids",
            urgency: "Normal",
            assignedTo: "mhandler1",
            completedAt: minutesAgo(120),
            completedBy: "mhandler1",
            reopenedAt: "",
            reopenedBy: "",
            audit: [
                { at: minutesAgo(180), by: "asmith", action: "Created" },
                { at: minutesAgo(120), by: "mhandler1", action: "Completed" }
            ]
        },
        {
            id: cryptoId(),
            createdAt: minutesAgo(70),
            createdBy: "asmith",
            site: "Expo",
            scopeOwner: "asmith",
            status: "Completed",
            sync: "Failed",
            requestType: "Gaylords",
            press: "Press 5",
            qty: 2,
            notes: "Need 2 gaylords for scrap",
            urgency: "Normal",
            assignedTo: "mhandler2",
            completedAt: minutesAgo(55),
            completedBy: "mhandler2",
            reopenedAt: "",
            reopenedBy: "",
            audit: [
                { at: minutesAgo(70), by: "asmith", action: "Created" },
                { at: minutesAgo(55), by: "mhandler2", action: "Completed" },
                { at: minutesAgo(54), by: "system", action: "Sync Failed" }
            ]
        }
    ];
}

export function loadState() {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return defaultState();
        const parsed = JSON.parse(raw);
        // basic schema guard
        if (!parsed || !parsed.meta || !Array.isArray(parsed.tasks)) return defaultState();
        return parsed;
    } catch {
        return defaultState();
    }
}

export function saveState(state) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

export function resetState() {
    localStorage.removeItem(STORAGE_KEY);
}

export function cryptoId() {
    if (typeof crypto !== "undefined" && crypto.randomUUID) return crypto.randomUUID();
    return "id_" + Math.random().toString(16).slice(2) + "_" + Date.now();
}

export function ageMinutes(iso) {
    const ms = Date.now() - new Date(iso).getTime();
    return Math.max(0, Math.round(ms / 60000));
}

export const RequestTypes = [
    "Coil",
    "Parts Pickup",
    "Skids",
    "Gaylords",
    "Dies",
    "Finished Goods Pickup",
    "Other"
];

export const Presses = [
    "Press 1",
    "Press 2",
    "Press 3",
    "Press 4",
    "Press 5",
    "Press 6",
    "Press 7",
    "Press 8",
    "Press 9",
    "Press 10",
    "Press 11",
    "Press 12"
];

export function roleDefaults(role) {
    if (role === "Operator") return { viewScope: "My", showMode: "Active" };
    return { viewScope: "Shared", showMode: "Active" };
}
