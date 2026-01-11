import {
    loadState,
    saveState,
    resetState,
    nowIso,
    ageMinutes,
    RequestTypes,
    Presses,
    roleDefaults
} from "./data.js";

function $(sel, root = document) {
    return root.querySelector(sel);
}

function $all(sel, root = document) {
    return [...root.querySelectorAll(sel)];
}

function escapeHtml(str) {
    return (str ?? "").toString()
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function fmtTime(iso) {
    if (!iso) return "";
    const d = new Date(iso);
    return d.toLocaleString([], { month: "2-digit", day: "2-digit", hour: "2-digit", minute: "2-digit" });
}

function withinRange(iso, rangeKey) {
    const minutes = ageMinutes(iso);
    if (rangeKey === "2h") return minutes <= 120;
    if (rangeKey === "8h") return minutes <= 480;
    if (rangeKey === "24h") return minutes <= 1440;
    if (rangeKey === "7d") return minutes <= 10080;
    return true;
}

function toast(host, { title, message, kind = "info", actionText, onAction, timeoutMs = 3200 }) {
    const node = document.createElement("div");
    node.className = "toast";

    const icon = document.createElement("div");
    icon.className = "toast__icon";
    icon.style.background = kind === "good" ? "var(--good)" : kind === "warn" ? "var(--warn)" : kind === "bad" ? "var(--bad)" : "var(--accent)";

    const body = document.createElement("div");
    body.innerHTML = `
    <div class="toast__title">${escapeHtml(title)}</div>
    <div class="toast__msg">${escapeHtml(message)}</div>
  `;

    const actions = document.createElement("div");
    actions.className = "toast__actions";

    if (actionText && typeof onAction === "function") {
        const btn = document.createElement("button");
        btn.className = "btn btn--ghost";
        btn.textContent = actionText;
        btn.addEventListener("click", () => {
            try { onAction(); } finally { node.remove(); }
        });
        actions.appendChild(btn);
    }

    const close = document.createElement("button");
    close.className = "btn btn--ghost";
    close.textContent = "Dismiss";
    close.addEventListener("click", () => node.remove());
    actions.appendChild(close);

    node.append(icon, body, actions);
    host.appendChild(node);

    window.setTimeout(() => node.remove(), timeoutMs);
}

function setSegment(segRoot, value) {
    $all("button", segRoot).forEach(btn => {
        btn.setAttribute("aria-pressed", btn.dataset.value === value ? "true" : "false");
    });
}

function getSegment(segRoot) {
    const pressed = $all("button", segRoot).find(b => b.getAttribute("aria-pressed") === "true");
    return pressed?.dataset.value ?? "";
}

function openModal(id) {
    const overlay = $(id);
    overlay.setAttribute("aria-hidden", "false");
    overlay.addEventListener("click", (e) => {
        if (e.target === overlay) closeModal(id);
    }, { once: true });
}

function closeModal(id) {
    const overlay = $(id);
    overlay.setAttribute("aria-hidden", "true");
}

function openDrawer() {
    $("#detailsDrawer").setAttribute("aria-hidden", "false");
}

function closeDrawer() {
    $("#detailsDrawer").setAttribute("aria-hidden", "true");
}

function currentUser(state) {
    // Mock identity for "My" scope
    if (state.meta.role === "Operator") return "jdoe";
    if (state.meta.role === "MaterialHandler") return "mhandler1";
    return "lead1";
}

function effectiveColumnSet(role) {
    // Keep v1.0 table readable. Details panel holds the rest.
    if (role === "Operator") {
        return ["Urgency", "Age", "Press", "Request", "Qty", "Notes", "Status"];
    }
    if (role === "MaterialHandler") {
        return ["Urgency", "Age", "Press", "Request", "Qty", "Notes", "Assigned", "Status"];
    }
    // Lead
    return ["Urgency", "Age", "Press", "Request", "Qty", "Notes", "CreatedBy", "Status"];
}

function statusChipHtml(task) {
    if (task.status === "Completed") {
        if (task.sync === "Failed") return `<span class="chip chip--bad">Failed sync</span>`;
        return `<span class="chip chip--good">Completed</span>`;
    }
    // Active
    if (task.sync === "Queued") return `<span class="chip chip--warn">Queued for sync</span>`;
    if (task.sync === "Failed") return `<span class="chip chip--bad">Sync failed</span>`;
    return `<span class="chip">Active</span>`;
}

function syncChip(meta) {
    if (!meta.online) return { cls: "chip--warn", text: "Offline" };
    return { cls: "chip--good", text: "Online" };
}

function filterTasks(state) {
    const { role, site, viewScope, showMode, lastFilters } = state.meta;
    const user = currentUser(state);

    let tasks = state.tasks.filter(t => t.site === site);

    // scope
    if (viewScope === "My") {
        tasks = tasks.filter(t => t.scopeOwner === user || t.createdBy === user);
    }

    // status
    if (showMode === "Active") tasks = tasks.filter(t => t.status === "Active");
    if (showMode === "Recent") tasks = tasks.filter(t => t.status === "Completed" && withinRange(t.completedAt || t.createdAt, lastFilters.recentRange));

    // filters
    const q = (lastFilters.search || "").trim().toLowerCase();
    if (q) {
        tasks = tasks.filter(t =>
            (t.press || "").toLowerCase().includes(q) ||
            (t.requestType || "").toLowerCase().includes(q) ||
            (t.notes || "").toLowerCase().includes(q) ||
            (t.createdBy || "").toLowerCase().includes(q)
        );
    }

    if (lastFilters.press) tasks = tasks.filter(t => t.press === lastFilters.press);
    if (lastFilters.type) tasks = tasks.filter(t => t.requestType === lastFilters.type);

    // sort: urgency first, then age desc
    tasks.sort((a, b) => {
        const au = a.urgency === "Red" ? 0 : 1;
        const bu = b.urgency === "Red" ? 0 : 1;
        if (au !== bu) return au - bu;
        return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    });

    return tasks;
}

function render(state) {
    // Header controls
    $("#roleSelect").value = state.meta.role;
    $("#siteSelect").value = state.meta.site;
    const sync = syncChip(state.meta);
    const syncEl = $("#syncChip");
    syncEl.className = `chip ${sync.cls}`;
    syncEl.textContent = sync.text;

    setSegment($("#segScope"), state.meta.viewScope);
    setSegment($("#segMode"), state.meta.showMode);

    $("#searchInput").value = state.meta.lastFilters.search;

    // Filter dropdowns
    const pressSel = $("#pressFilter");
    const typeSel = $("#typeFilter");
    pressSel.innerHTML = `<option value="">All Presses</option>` + Presses.map(p => `<option value="${escapeHtml(p)}">${escapeHtml(p)}</option>`).join("");
    typeSel.innerHTML = `<option value="">All Types</option>` + RequestTypes.map(t => `<option value="${escapeHtml(t)}">${escapeHtml(t)}</option>`).join("");
    pressSel.value = state.meta.lastFilters.press;
    typeSel.value = state.meta.lastFilters.type;

    const recentRangeField = $("#recentRangeField");
    const recentRangeSelect = $("#recentRangeSelect");
    recentRangeSelect.value = state.meta.lastFilters.recentRange;
    recentRangeField.style.display = state.meta.showMode === "Recent" ? "inline-flex" : "none";

    // Primary CTA label varies by role
    const cta = $("#primaryCta");
    if (state.meta.role === "Operator") {
        cta.textContent = "New Request";
        cta.className = "btn btn--primary";
    } else if (state.meta.role === "MaterialHandler") {
        cta.textContent = "Complete Selected";
        cta.className = "btn btn--primary";
    } else {
        cta.textContent = "Reopen Selected";
        cta.className = "btn btn--primary";
    }

    // Table
    const cols = effectiveColumnSet(state.meta.role);
    const tasks = filterTasks(state);
    $("#resultCount").textContent = `${tasks.length} item${tasks.length === 1 ? "" : "s"}`;

    const thead = $("#queueThead");
    thead.innerHTML = `<tr>${cols.map(c => `<th>${escapeHtml(c)}</th>`).join("")}</tr>`;

    const tbody = $("#queueTbody");
    tbody.innerHTML = tasks.map(t => {
        const age = ageMinutes(t.createdAt);
        const urgency = t.urgency === "Red" ? `<span class="chip chip--bad">RED</span>` : `<span class="chip">Normal</span>`;
        const assigned = t.assignedTo ? escapeHtml(t.assignedTo) : "-";
        const createdBy = escapeHtml(t.createdBy);
        const notes = t.notes ? escapeHtml(t.notes) : "";
        const status = statusChipHtml(t);

        const cell = (name) => {
            if (name === "Urgency") return `<td>${urgency}</td>`;
            if (name === "Age") return `<td><span class="cell-strong">${age}m</span> <span class="cell-muted">ago</span></td>`;
            if (name === "Press") return `<td class="cell-strong">${escapeHtml(t.press)}</td>`;
            if (name === "Request") return `<td>${escapeHtml(t.requestType)}</td>`;
            if (name === "Qty") return `<td>${escapeHtml(t.qty)}</td>`;
            if (name === "Notes") return `<td>${notes}</td>`;
            if (name === "Assigned") return `<td>${assigned}</td>`;
            if (name === "CreatedBy") return `<td>${createdBy}</td>`;
            if (name === "Status") return `<td>${status}</td>`;
            return `<td></td>`;
        };

        return `
      <tr data-id="${escapeHtml(t.id)}" tabindex="0" aria-selected="${state.meta.selectedId === t.id ? "true" : "false"}">
        ${cols.map(cell).join("")}
      </tr>
    `;
    }).join("");

    // Empty state
    $("#emptyState").style.display = tasks.length ? "none" : "block";

    // Update drawer if open
    if ($("#detailsDrawer").getAttribute("aria-hidden") === "false") {
        const t = state.tasks.find(x => x.id === state.meta.selectedId);
        if (t) renderDetails(state, t);
    }
}

function renderDetails(state, task) {
    $("#detailsTitle").textContent = `${task.requestType} • ${task.press}`;
    $("#detailsSubtitle").textContent = task.status === "Active" ? "Active task" : "Completed history";

    const user = currentUser(state);
    const showReopen = state.meta.role === "Lead" && task.status === "Completed";
    const showComplete = state.meta.role === "MaterialHandler" && task.status === "Active";
    const showTemplate = task.status === "Completed";

    $("#detailsActions").innerHTML = `
    ${showComplete ? `<button class="btn btn--primary" id="actComplete">Complete</button>` : ``}
    ${showReopen ? `<button class="btn btn--primary" id="actReopen">Reopen</button>` : ``}
    ${showTemplate ? `<button class="btn" id="actTemplate">Use as Template</button>` : ``}
    ${task.sync === "Failed" ? `<button class="btn btn--danger" id="actRetrySync">Retry Sync</button>` : ``}
    <button class="btn btn--ghost" id="actClose">Close</button>
  `;

    $("#detailsKv").innerHTML = `
    <div class="kv"><div class="k">Status</div><div class="v">${statusChipHtml(task)}</div></div>
    <div class="kv"><div class="k">Urgency</div><div class="v">${escapeHtml(task.urgency)}</div></div>
    <div class="kv"><div class="k">Created</div><div class="v">${fmtTime(task.createdAt)} by ${escapeHtml(task.createdBy)}</div></div>
    <div class="kv"><div class="k">Notes</div><div class="v">${escapeHtml(task.notes || "")}</div></div>
    <div class="kv"><div class="k">Assigned</div><div class="v">${escapeHtml(task.assignedTo || "-")}</div></div>
    ${task.completedAt ? `<div class="kv"><div class="k">Completed</div><div class="v">${fmtTime(task.completedAt)} by ${escapeHtml(task.completedBy)}</div></div>` : ``}
    ${task.reopenedAt ? `<div class="kv"><div class="k">Reopened</div><div class="v">${fmtTime(task.reopenedAt)} by ${escapeHtml(task.reopenedBy)}</div></div>` : ``}
  `;

    $("#detailsAudit").innerHTML = task.audit
        .slice()
        .reverse()
        .map(a => `<div class="kv"><div class="k">${fmtTime(a.at)}</div><div class="v">${escapeHtml(a.action)} <span class="cell-muted">(${escapeHtml(a.by)})</span></div></div>`)
        .join("");

    $("#actClose").addEventListener("click", closeDrawer);

    if (showComplete) {
        $("#actComplete").addEventListener("click", () => openCompleteModal(state, task));
    }
    if (showReopen) {
        $("#actReopen").addEventListener("click", () => openReopenModal(state, task));
    }
    if (showTemplate) {
        $("#actTemplate").addEventListener("click", () => openTemplateModal(state, task));
    }
    if (task.sync === "Failed") {
        $("#actRetrySync").addEventListener("click", () => {
            task.audit.push({ at: nowIso(), by: "system", action: "Retry Sync" });
            task.sync = state.meta.online ? "Synced" : "Queued";
            const host = $("#toastHost");
            toast(host, {
                title: "Sync retry",
                message: task.sync === "Synced" ? "Synced successfully." : "Queued for sync (Offline).",
                kind: task.sync === "Synced" ? "good" : "warn"
            });
            persistAndRender(state);
            renderDetails(state, task);
        });
    }
}

function persistAndRender(state) {
    saveState(state);
    render(state);
}

function selectRow(state, id) {
    state.meta.selectedId = id;
    persistAndRender(state);
    const task = state.tasks.find(t => t.id === id);
    if (task) {
        renderDetails(state, task);
        openDrawer();
    }
}

function openNewRequestModal(state, prefill = null) {
    const typeSel = $("#nrType");
    const pressSel = $("#nrPress");
    const urgencySel = $("#nrUrgency");

    typeSel.innerHTML = RequestTypes.map(t => `<option value="${escapeHtml(t)}">${escapeHtml(t)}</option>`).join("");
    pressSel.innerHTML = Presses.map(p => `<option value="${escapeHtml(p)}">${escapeHtml(p)}</option>`).join("");

    $("#nrQty").value = prefill?.qty ?? 1;
    $("#nrNotes").value = prefill?.notes ?? "";
    typeSel.value = prefill?.requestType ?? "Parts Pickup";
    pressSel.value = prefill?.press ?? Presses[0];
    urgencySel.value = prefill?.urgency ?? "Normal";

    $("#nrTitle").textContent = prefill ? "New Request (from Template)" : "New Request";
    $("#nrHint").textContent = state.meta.online
        ? "Submit a request and see it appear instantly in the queue."
        : "Offline: your request will be queued for sync.";

    openModal("#modalNewRequest");

    const saveBtn = $("#nrSubmit");
    const cancelBtn = $("#nrCancel");
    const closeXBtn = $("#nrCloseX");

    const onCancel = () => {
        closeModal("#modalNewRequest");
        cancelBtn.removeEventListener("click", onCancel);
        closeXBtn.removeEventListener("click", onCancel);
        saveBtn.removeEventListener("click", onSubmit);
    };

    const onSubmit = () => {
        const task = {
            id: crypto.randomUUID ? crypto.randomUUID() : ("id_" + Math.random().toString(16).slice(2)),
            createdAt: nowIso(),
            createdBy: currentUser(state),
            site: state.meta.site,
            scopeOwner: currentUser(state),
            status: "Active",
            sync: state.meta.online ? "Synced" : "Queued",
            requestType: typeSel.value,
            press: pressSel.value,
            qty: Number($("#nrQty").value || 1),
            notes: $("#nrNotes").value,
            urgency: urgencySel.value,
            assignedTo: "",
            completedAt: "",
            completedBy: "",
            reopenedAt: "",
            reopenedBy: "",
            audit: [{ at: nowIso(), by: currentUser(state), action: state.meta.online ? "Created" : "Created (Offline)" }]
        };

        state.tasks.unshift(task);

        const host = $("#toastHost");
        toast(host, {
            title: "Request created",
            message: task.sync === "Synced" ? "Material handling will see it immediately." : "Queued for sync (Offline).",
            kind: task.sync === "Synced" ? "good" : "warn"
        });

        closeModal("#modalNewRequest");
        cancelBtn.removeEventListener("click", onCancel);
        closeXBtn.removeEventListener("click", onCancel);
        saveBtn.removeEventListener("click", onSubmit);

        // ensure operator sees their own request right away
        if (state.meta.role === "Operator") {
            state.meta.viewScope = "My";
            state.meta.showMode = "Active";
        }

        persistAndRender(state);
    };

    cancelBtn.addEventListener("click", onCancel);
    closeXBtn.addEventListener("click", onCancel);
    saveBtn.addEventListener("click", onSubmit);
}

function openCompleteModal(state, task) {
    $("#cmTitle").textContent = "Complete Task";
    $("#cmHint").textContent = "Completing moves the item to Recent (Completed) and shows a timed confirmation.";
    $("#cmSummary").textContent = `${task.requestType} • ${task.press}`;

    openModal("#modalConfirm");

    const ok = $("#cmOk");
    const cancel = $("#cmCancel");
    const closeX = $("#cmCloseX");

    const onCancel = () => {
        closeModal("#modalConfirm");
        cancel.removeEventListener("click", onCancel);
        closeX.removeEventListener("click", onCancel);
        ok.removeEventListener("click", onOk);
    };

    const onOk = () => {
        task.status = "Completed";
        task.completedAt = nowIso();
        task.completedBy = currentUser(state);
        task.audit.push({ at: nowIso(), by: currentUser(state), action: "Completed" });

        // If offline, completion is queued
        if (!state.meta.online) {
            task.sync = "Queued";
            task.audit.push({ at: nowIso(), by: "system", action: "Queued for sync (Offline)" });
        }

        const host = $("#toastHost");
        toast(host, {
            title: "Completed",
            message: state.meta.online ? "Moved to Recent (Completed)." : "Queued for sync (Offline).",
            kind: state.meta.online ? "good" : "warn"
        });

        closeModal("#modalConfirm");
        cancel.removeEventListener("click", onCancel);
        closeX.removeEventListener("click", onCancel);
        ok.removeEventListener("click", onOk);

        persistAndRender(state);
        renderDetails(state, task);
    };

    cancel.addEventListener("click", onCancel);
    closeX.addEventListener("click", onCancel);
    ok.addEventListener("click", onOk);
}

function openReopenModal(state, task) {
    $("#cmTitle").textContent = "Reopen Task";
    $("#cmHint").textContent = "Reopen returns the item to Active and resets its timer.";
    $("#cmSummary").textContent = `${task.requestType} • ${task.press}`;

    openModal("#modalConfirm");

    const ok = $("#cmOk");
    const cancel = $("#cmCancel");
    const closeX = $("#cmCloseX");

    const onCancel = () => {
        closeModal("#modalConfirm");
        cancel.removeEventListener("click", onCancel);
        closeX.removeEventListener("click", onCancel);
        ok.removeEventListener("click", onOk);
    };

    const onOk = () => {
        task.status = "Active";
        task.reopenedAt = nowIso();
        task.reopenedBy = currentUser(state);
        task.createdAt = nowIso();
        task.audit.push({ at: nowIso(), by: currentUser(state), action: "Reopened (Timer reset)" });

        if (!state.meta.online) {
            task.sync = "Queued";
            task.audit.push({ at: nowIso(), by: "system", action: "Queued for sync (Offline)" });
        } else {
            task.sync = "Synced";
        }

        const host = $("#toastHost");
        toast(host, {
            title: "Reopened",
            message: "Returned to Active with timer reset.",
            kind: "warn"
        });

        closeModal("#modalConfirm");
        cancel.removeEventListener("click", onCancel);
        closeX.removeEventListener("click", onCancel);
        ok.removeEventListener("click", onOk);

        persistAndRender(state);
        renderDetails(state, task);
    };

    cancel.addEventListener("click", onCancel);
    closeX.addEventListener("click", onCancel);
    ok.addEventListener("click", onOk);
}

function openTemplateModal(state, task) {
    // Use-as-template is a fast way to create a new task.
    openNewRequestModal(state, {
        requestType: task.requestType,
        press: task.press,
        qty: task.qty,
        notes: task.notes,
        urgency: task.urgency
    });
}

function bind(state) {
    // Segments
    $("#segScope").addEventListener("click", (e) => {
        const btn = e.target.closest("button[data-value]");
        if (!btn) return;
        state.meta.viewScope = btn.dataset.value;
        persistAndRender(state);
    });

    $("#segMode").addEventListener("click", (e) => {
        const btn = e.target.closest("button[data-value]");
        if (!btn) return;
        state.meta.showMode = btn.dataset.value;
        persistAndRender(state);
    });

    // Role
    $("#roleSelect").addEventListener("change", () => {
        state.meta.role = $("#roleSelect").value;
        const def = roleDefaults(state.meta.role);
        state.meta.viewScope = def.viewScope;
        state.meta.showMode = def.showMode;
        state.meta.selectedId = "";
        closeDrawer();
        persistAndRender(state);
    });

    // Site
    $("#siteSelect").addEventListener("change", () => {
        state.meta.site = $("#siteSelect").value;
        state.meta.selectedId = "";
        closeDrawer();
        persistAndRender(state);
    });

    // Online/offline toggle
    $("#toggleOnline").addEventListener("click", () => {
        state.meta.online = !state.meta.online;
        const host = $("#toastHost");
        toast(host, {
            title: state.meta.online ? "Online" : "Offline",
            message: state.meta.online ? "Sync is available." : "Actions will be queued for sync.",
            kind: state.meta.online ? "good" : "warn"
        });

        // If returning online, auto-resolve queued sync items in demo
        if (state.meta.online) {
            state.tasks.forEach(t => {
                if (t.sync === "Queued") {
                    t.sync = "Synced";
                    t.audit.push({ at: nowIso(), by: "system", action: "Synced" });
                }
            });
        }

        persistAndRender(state);
    });

    // Search
    $("#searchInput").addEventListener("input", () => {
        state.meta.lastFilters.search = $("#searchInput").value;
        persistAndRender(state);
    });

    // Filters
    $("#pressFilter").addEventListener("change", () => {
        state.meta.lastFilters.press = $("#pressFilter").value;
        persistAndRender(state);
    });

    $("#typeFilter").addEventListener("change", () => {
        state.meta.lastFilters.type = $("#typeFilter").value;
        persistAndRender(state);
    });

    $("#recentRangeSelect").addEventListener("change", () => {
        state.meta.lastFilters.recentRange = $("#recentRangeSelect").value;
        persistAndRender(state);
    });

    // Primary CTA
    $("#primaryCta").addEventListener("click", () => {
        const selected = state.tasks.find(t => t.id === state.meta.selectedId);

        if (state.meta.role === "Operator") {
            openNewRequestModal(state);
            return;
        }

        if (!selected) {
            toast($("#toastHost"), {
                title: "Select a row",
                message: "Pick an item from the table first.",
                kind: "warn"
            });
            return;
        }

        if (state.meta.role === "MaterialHandler") {
            if (selected.status !== "Active") {
                toast($("#toastHost"), { title: "Not active", message: "Only Active tasks can be completed.", kind: "warn" });
                return;
            }
            openCompleteModal(state, selected);
            return;
        }

        // Lead
        if (selected.status !== "Completed") {
            toast($("#toastHost"), { title: "Not completed", message: "Reopen is available from Recent (Completed).", kind: "warn" });
            return;
        }
        openReopenModal(state, selected);
    });

    // Reset demo
    $("#resetDemo").addEventListener("click", () => {
        resetState();
        const fresh = loadState();
        Object.assign(state, fresh);
        closeDrawer();
        toast($("#toastHost"), { title: "Reset", message: "Demo data reset.", kind: "good" });
        persistAndRender(state);
    });

    // Table row selection
    $("#queueTbody").addEventListener("click", (e) => {
        const tr = e.target.closest("tr[data-id]");
        if (!tr) return;
        selectRow(state, tr.dataset.id);
    });

    // Keyboard nav
    $("#queueTbody").addEventListener("keydown", (e) => {
        const tr = e.target.closest("tr[data-id]");
        if (!tr) return;
        if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            selectRow(state, tr.dataset.id);
        }
    });

    // Drawer close button
    $("#drawerClose").addEventListener("click", closeDrawer);

    // Global escape closes modals/drawer
    document.addEventListener("keydown", (e) => {
        if (e.key !== "Escape") return;
        // close any open modal
        ["#modalNewRequest", "#modalConfirm"].forEach(id => {
            const el = $(id);
            if (el && el.getAttribute("aria-hidden") === "false") closeModal(id);
        });
        closeDrawer();
    });
}

export function bootWaitlistPage() {
    const state = loadState();
    // Ensure defaults for missing props
    state.meta.selectedId = state.meta.selectedId || "";

    bind(state);
    render(state);
}
