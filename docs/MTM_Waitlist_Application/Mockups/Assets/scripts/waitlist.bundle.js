/* MTM Waitlist Mockups - self-contained bundle (no server required)
   This file intentionally avoids ES module imports so the mockups work when opened via file://.
*/

(function () {
    "use strict";

    // -----------------------------
    // data.js (inlined)
    // -----------------------------

    const STORAGE_KEY = "mtm_waitlist_mock_v1";

    function nowIso() {
        return new Date().toISOString();
    }

    function minutesAgo(minutes) {
        return new Date(Date.now() - minutes * 60 * 1000).toISOString();
    }

    function cryptoId() {
        if (typeof crypto !== "undefined" && crypto.randomUUID) return crypto.randomUUID();
        return "id_" + Math.random().toString(16).slice(2) + "_" + Date.now();
    }

    function seedTasks() {
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

    function defaultState() {
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
                },
                selectedId: ""
            },
            tasks: seedTasks()
        };
    }

    function loadState() {
        try {
            const raw = localStorage.getItem(STORAGE_KEY);
            if (!raw) return defaultState();
            const parsed = JSON.parse(raw);
            if (!parsed || !parsed.meta || !Array.isArray(parsed.tasks)) return defaultState();
            parsed.meta.selectedId = parsed.meta.selectedId || "";
            parsed.meta.lastFilters = parsed.meta.lastFilters || { search: "", press: "", type: "", recentRange: "2h" };
            return parsed;
        } catch {
            return defaultState();
        }
    }

    function saveState(state) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    }

    function resetState() {
        localStorage.removeItem(STORAGE_KEY);
    }

    function ageMinutes(iso) {
        const ms = Date.now() - new Date(iso).getTime();
        return Math.max(0, Math.round(ms / 60000));
    }

    const RequestTypes = [
        "Coil",
        "Parts Pickup",
        "Skids",
        "Gaylords",
        "Dies",
        "Finished Goods Pickup",
        "Other"
    ];

    const Presses = [
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

    function roleDefaults(role) {
        if (role === "Operator") return { viewScope: "My", showMode: "Active" };
        return { viewScope: "Shared", showMode: "Active" };
    }

    // -----------------------------
    // app.js (inlined)
    // -----------------------------

    function $(sel, root = document) {
        return root.querySelector(sel);
    }

    function $all(sel, root = document) {
        return Array.from(root.querySelectorAll(sel));
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
        if (state.meta.role === "Operator") return "jdoe";
        if (state.meta.role === "MaterialHandler") return "mhandler1";
        return "lead1";
    }

    function effectiveColumnSet(role) {
        if (role === "Operator") return ["Urgency", "Age", "Press", "Request", "Qty", "Notes", "Status"];
        if (role === "MaterialHandler") return ["Urgency", "Age", "Press", "Request", "Qty", "Notes", "Assigned", "Status"];
        return ["Urgency", "Age", "Press", "Request", "Qty", "Notes", "CreatedBy", "Status"];
    }

    function statusChipHtml(task) {
        if (task.status === "Completed") {
            if (task.sync === "Failed") return `<span class="chip chip--bad">Failed sync</span>`;
            return `<span class="chip chip--good">Completed</span>`;
        }
        if (task.sync === "Queued") return `<span class="chip chip--warn">Queued for sync</span>`;
        if (task.sync === "Failed") return `<span class="chip chip--bad">Sync failed</span>`;
        return `<span class="chip">Active</span>`;
    }

    function syncChip(meta) {
        if (!meta.online) return { cls: "chip--warn", text: "Offline" };
        return { cls: "chip--good", text: "Online" };
    }

    function filterTasks(state) {
        const { site, viewScope, showMode, lastFilters } = state.meta;
        const user = currentUser(state);

        let tasks = state.tasks.filter(t => t.site === site);

        if (viewScope === "My") {
            tasks = tasks.filter(t => t.scopeOwner === user || t.createdBy === user);
        }

        if (showMode === "Active") tasks = tasks.filter(t => t.status === "Active");
        if (showMode === "Recent") tasks = tasks.filter(t => t.status === "Completed" && withinRange(t.completedAt || t.createdAt, lastFilters.recentRange));

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

        tasks.sort((a, b) => {
            const au = a.urgency === "Red" ? 0 : 1;
            const bu = b.urgency === "Red" ? 0 : 1;
            if (au !== bu) return au - bu;
            return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        });

        return tasks;
    }

    function renderDetails(state, task) {
        $("#detailsTitle").textContent = `${task.requestType} • ${task.press}`;
        $("#detailsSubtitle").textContent = task.status === "Active" ? "Active task" : "Completed history";

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

        if (showComplete) $("#actComplete").addEventListener("click", () => openCompleteModal(state, task));
        if (showReopen) $("#actReopen").addEventListener("click", () => openReopenModal(state, task));
        if (showTemplate) $("#actTemplate").addEventListener("click", () => openNewRequestModal(state, task));

        if (task.sync === "Failed") {
            $("#actRetrySync").addEventListener("click", () => {
                task.audit.push({ at: nowIso(), by: "system", action: "Retry Sync" });
                task.sync = state.meta.online ? "Synced" : "Queued";
                toast($("#toastHost"), {
                    title: "Sync retry",
                    message: task.sync === "Synced" ? "Synced successfully." : "Queued for sync (Offline).",
                    kind: task.sync === "Synced" ? "good" : "warn"
                });
                persistAndRender(state);
                renderDetails(state, task);
            });
        }
    }

    function render(state) {
        $("#roleSelect").value = state.meta.role;
        $("#siteSelect").value = state.meta.site;

        const sync = syncChip(state.meta);
        const syncEl = $("#syncChip");
        syncEl.className = `chip ${sync.cls}`;
        syncEl.textContent = sync.text;

        setSegment($("#segScope"), state.meta.viewScope);
        setSegment($("#segMode"), state.meta.showMode);

        $("#searchInput").value = state.meta.lastFilters.search;

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

        const cta = $("#primaryCta");
        if (state.meta.role === "Operator") cta.textContent = "New Request";
        else if (state.meta.role === "MaterialHandler") cta.textContent = "Complete Selected";
        else cta.textContent = "Reopen Selected";

        const cols = effectiveColumnSet(state.meta.role);
        const tasks = filterTasks(state);
        $("#resultCount").textContent = `${tasks.length} item${tasks.length === 1 ? "" : "s"}`;

        $("#queueThead").innerHTML = `<tr>${cols.map(c => `<th>${escapeHtml(c)}</th>`).join("")}</tr>`;

        $("#queueTbody").innerHTML = tasks.map(t => {
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

        $("#emptyState").style.display = tasks.length ? "none" : "block";

        if ($("#detailsDrawer").getAttribute("aria-hidden") === "false") {
            const t = state.tasks.find(x => x.id === state.meta.selectedId);
            if (t) renderDetails(state, t);
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

    function openNewRequestModal(state, prefillFromTask) {
        const typeSel = $("#nrType");
        const pressSel = $("#nrPress");
        const urgencySel = $("#nrUrgency");

        typeSel.innerHTML = RequestTypes.map(t => `<option value="${escapeHtml(t)}">${escapeHtml(t)}</option>`).join("");
        pressSel.innerHTML = Presses.map(p => `<option value="${escapeHtml(p)}">${escapeHtml(p)}</option>`).join("");

        const prefill = prefillFromTask
            ? {
                qty: prefillFromTask.qty,
                notes: prefillFromTask.notes,
                requestType: prefillFromTask.requestType,
                press: prefillFromTask.press,
                urgency: prefillFromTask.urgency
            }
            : null;

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
                id: cryptoId(),
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

            toast($("#toastHost"), {
                title: "Request created",
                message: task.sync === "Synced" ? "Material handling will see it immediately." : "Queued for sync (Offline).",
                kind: task.sync === "Synced" ? "good" : "warn"
            });

            onCancel();

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

            if (!state.meta.online) {
                task.sync = "Queued";
                task.audit.push({ at: nowIso(), by: "system", action: "Queued for sync (Offline)" });
            }

            toast($("#toastHost"), {
                title: "Completed",
                message: state.meta.online ? "Moved to Recent (Completed)." : "Queued for sync (Offline).",
                kind: state.meta.online ? "good" : "warn"
            });

            onCancel();
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

            toast($("#toastHost"), {
                title: "Reopened",
                message: "Returned to Active with timer reset.",
                kind: "warn"
            });

            onCancel();
            persistAndRender(state);
            renderDetails(state, task);
        };

        cancel.addEventListener("click", onCancel);
        closeX.addEventListener("click", onCancel);
        ok.addEventListener("click", onOk);
    }

    function bind(state) {
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

        $("#roleSelect").addEventListener("change", () => {
            state.meta.role = $("#roleSelect").value;
            const def = roleDefaults(state.meta.role);
            state.meta.viewScope = def.viewScope;
            state.meta.showMode = def.showMode;
            state.meta.selectedId = "";
            closeDrawer();
            persistAndRender(state);
        });

        $("#siteSelect").addEventListener("change", () => {
            state.meta.site = $("#siteSelect").value;
            state.meta.selectedId = "";
            closeDrawer();
            persistAndRender(state);
        });

        $("#toggleOnline").addEventListener("click", () => {
            state.meta.online = !state.meta.online;
            toast($("#toastHost"), {
                title: state.meta.online ? "Online" : "Offline",
                message: state.meta.online ? "Sync is available." : "Actions will be queued for sync.",
                kind: state.meta.online ? "good" : "warn"
            });

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

        $("#searchInput").addEventListener("input", () => {
            state.meta.lastFilters.search = $("#searchInput").value;
            persistAndRender(state);
        });

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

        $("#primaryCta").addEventListener("click", () => {
            const selected = state.tasks.find(t => t.id === state.meta.selectedId);

            if (state.meta.role === "Operator") {
                openNewRequestModal(state);
                return;
            }

            if (!selected) {
                toast($("#toastHost"), { title: "Select a row", message: "Pick an item from the table first.", kind: "warn" });
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

            if (selected.status !== "Completed") {
                toast($("#toastHost"), { title: "Not completed", message: "Reopen is available from Recent (Completed).", kind: "warn" });
                return;
            }
            openReopenModal(state, selected);
        });

        $("#resetDemo").addEventListener("click", () => {
            resetState();
            const fresh = loadState();
            state.meta = fresh.meta;
            state.tasks = fresh.tasks;
            closeDrawer();
            toast($("#toastHost"), { title: "Reset", message: "Demo data reset.", kind: "good" });
            persistAndRender(state);
        });

        $("#queueTbody").addEventListener("click", (e) => {
            const tr = e.target.closest("tr[data-id]");
            if (!tr) return;
            selectRow(state, tr.dataset.id);
        });

        $("#queueTbody").addEventListener("keydown", (e) => {
            const tr = e.target.closest("tr[data-id]");
            if (!tr) return;
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                selectRow(state, tr.dataset.id);
            }
        });

        $("#drawerClose").addEventListener("click", closeDrawer);

        document.addEventListener("keydown", (e) => {
            if (e.key !== "Escape") return;
            ["#modalNewRequest", "#modalConfirm"].forEach(id => {
                const el = $(id);
                if (el && el.getAttribute("aria-hidden") === "false") closeModal(id);
            });
            closeDrawer();
        });
    }

    // -----------------------------
    // Public boot
    // -----------------------------

    window.bootWaitlistPage = function bootWaitlistPage() {
        const state = loadState();
        bind(state);
        render(state);
    };

})();
