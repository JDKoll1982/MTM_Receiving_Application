/* =============================================================================
   copilot-forms.js — CopilotForms runtime
   Features: dark mode, sticky feature, progress bar, dynamic list widget,
   File System Access API save, recent exports, user defaults, URL pre-fill,
   keyboard shortcuts, output tabs, toast notifications, SPA routing,
   natural-language pre-fill (parse-into-fields).
   ============================================================================= */

// ── Constants ─────────────────────────────────────────────────────────────────
const S = "copilotforms:";
const KEY_DRAFT = (id) => `${S}draft:${id}`;
const KEY_DEFAULTS = `${S}defaults`;
const KEY_THEME = `${S}theme`;
const KEY_STICKY = `${S}lastFeature`;
const KEY_RECENT = `${S}recent`;
const MAX_RECENT = 20;
const ALL_FEATURES_ID = "__all_features__";
const ALL_SUBFEATURES_ID = "__all_subfeatures__";

// ── DOM helper ────────────────────────────────────────────────────────────────
function $(id) {
  return document.getElementById(id);
}

function make(tag, attrs = {}, ...children) {
  const el = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (k === "class") el.className = v;
    else if (k === "text") el.textContent = v;
    else el.setAttribute(k, v);
  }
  children.flat().forEach((c) => {
    if (typeof c === "string") el.appendChild(document.createTextNode(c));
    else if (c) el.appendChild(c);
  });
  return el;
}

function escHtml(s) {
  return String(s ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

// ── Theme ─────────────────────────────────────────────────────────────────────
function getTheme() {
  return localStorage.getItem(KEY_THEME) || "auto";
}
function isDark(theme) {
  const t = theme ?? getTheme();
  return (
    t === "dark" ||
    (t === "auto" && window.matchMedia("(prefers-color-scheme: dark)").matches)
  );
}

function applyTheme(theme) {
  document.documentElement.setAttribute(
    "data-theme",
    isDark(theme) ? "dark" : "light",
  );
  const btn = $("theme-toggle");
  if (btn)
    btn.setAttribute(
      "aria-label",
      isDark(theme) ? "Switch to light" : "Switch to dark",
    );
}

function initTheme() {
  applyTheme(getTheme());
  window
    .matchMedia("(prefers-color-scheme: dark)")
    .addEventListener("change", () => {
      if (getTheme() === "auto") applyTheme("auto");
    });
}

function toggleTheme() {
  const next = isDark() ? "light" : "dark";
  localStorage.setItem(KEY_THEME, next);
  applyTheme(next);
}

// ── Toast ─────────────────────────────────────────────────────────────────────
function showToast(message, type = "success") {
  const c = $("toast-container");
  if (!c) return;
  const t = make(
    "div",
    { class: `toast toast-${type}`, role: "status" },
    message,
  );
  c.appendChild(t);
  requestAnimationFrame(() => t.classList.add("toast-visible"));
  setTimeout(() => {
    t.classList.remove("toast-visible");
    t.addEventListener("transitionend", () => t.remove(), { once: true });
  }, 2500);
}

// ── Defaults ──────────────────────────────────────────────────────────────────
function loadDefaults() {
  try {
    return JSON.parse(localStorage.getItem(KEY_DEFAULTS) || "{}");
  } catch {
    return {};
  }
}
function saveDefaults(obj) {
  localStorage.setItem(KEY_DEFAULTS, JSON.stringify(obj));
}

// ── Recent exports ─────────────────────────────────────────────────────────────
function getRecents() {
  try {
    return JSON.parse(localStorage.getItem(KEY_RECENT) || "[]");
  } catch {
    return [];
  }
}
function addRecent(formId, formTitle, featureName, content) {
  const entry = {
    id: Date.now().toString(36),
    formId,
    formTitle,
    featureName: featureName || "Custom",
    timestamp: new Date().toISOString(),
    preview: content.split("\n").slice(0, 3).join(" ").substring(0, 120),
  };
  const updated = [
    entry,
    ...getRecents().filter(
      (r) => !(r.formId === formId && r.featureName === entry.featureName),
    ),
  ].slice(0, MAX_RECENT);
  localStorage.setItem(KEY_RECENT, JSON.stringify(updated));
}

function formatRelTime(iso) {
  const diff = Date.now() - new Date(iso).getTime();
  const m = Math.floor(diff / 60000);
  if (m < 1) return "just now";
  if (m < 60) return `${m}m ago`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h}h ago`;
  return `${Math.floor(h / 24)}d ago`;
}

// ── Dynamic list widget ────────────────────────────────────────────────────────
function createListWidget(fieldCfg, initVals = []) {
  const wrap = make("div", {
    class: "list-widget",
    "data-field-id": fieldCfg.id,
    "data-field-type": "list",
  });
  const rows = make("div", { class: "list-rows" });
  wrap.appendChild(rows);

  function addRow(value = "") {
    const row = make("div", { class: "list-row" });
    const input = make("input", {
      type: "text",
      placeholder: fieldCfg.itemPlaceholder || "Enter item…",
      value,
    });
    input.setAttribute("value", value);
    input.addEventListener("input", () =>
      wrap.dispatchEvent(new Event("listchange", { bubbles: true })),
    );
    input.addEventListener("keydown", (e) => {
      if (e.key === "Enter") {
        e.preventDefault();
        addRow();
        rows.lastChild?.querySelector("input")?.focus();
      }
    });
    const del = make(
      "button",
      { type: "button", class: "list-row-delete", "aria-label": "Remove" },
      "×",
    );
    del.addEventListener("click", () => {
      row.remove();
      wrap.dispatchEvent(new Event("listchange", { bubbles: true }));
    });
    row.appendChild(input);
    row.appendChild(del);
    rows.appendChild(row);
    return input;
  }

  (initVals.length ? initVals : [""]).forEach((v) => addRow(v));

  const addBtn = make(
    "button",
    { type: "button", class: "list-add-btn" },
    "+ Add item",
  );
  addBtn.addEventListener("click", () => addRow("").focus());
  wrap.appendChild(addBtn);

  wrap.readValue = () =>
    [...rows.querySelectorAll(".list-row input")]
      .map((i) => i.value.trim())
      .filter(Boolean);
  wrap.writeValue = (arr) => {
    rows.innerHTML = "";
    (Array.isArray(arr) && arr.length ? arr : [""]).forEach((v) => addRow(v));
  };
  return wrap;
}

function createCheckboxGroupWidget(fieldCfg, options = [], initVals = []) {
  const wrap = make("div", {
    class: "checkbox-group-widget",
    id: fieldCfg.id,
    "data-field-id": fieldCfg.id,
    "data-field-type": "checkbox-group",
  });

  const optionsWrap = make("div", { class: "checkbox-group-options" });
  wrap.appendChild(optionsWrap);

  function renderOptions(nextOptions = [], selected = []) {
    const selectedSet = new Set(Array.isArray(selected) ? selected : []);
    optionsWrap.innerHTML = "";

    nextOptions.forEach((opt, index) => {
      const safeId = `${fieldCfg.id}-${index}`;
      const label = make("label", { class: "checkbox-chip", for: safeId });
      const input = make("input", {
        type: "checkbox",
        id: safeId,
        value: opt,
      });
      input.checked = selectedSet.has(opt);
      input.addEventListener("change", () => {
        wrap.dispatchEvent(new Event("change", { bubbles: true }));
      });
      label.appendChild(input);
      label.appendChild(make("span", { class: "checkbox-chip-text" }, opt));
      optionsWrap.appendChild(label);
    });
  }

  wrap.readValue = () =>
    [...optionsWrap.querySelectorAll('input[type="checkbox"]:checked')].map(
      (input) => input.value,
    );

  wrap.writeValue = (vals) => {
    const selected = new Set(Array.isArray(vals) ? vals : []);
    optionsWrap.querySelectorAll('input[type="checkbox"]').forEach((input) => {
      input.checked = selected.has(input.value);
    });
  };

  wrap.setOptions = (vals, selected = wrap.readValue()) => {
    renderOptions(vals, selected);
  };

  renderOptions(options, initVals);
  return wrap;
}

function populateSelectOptions(
  selectEl,
  options = [],
  placeholder = "Select…",
) {
  if (!selectEl) return;
  const currentValue = selectEl.value;
  selectEl.innerHTML = "";
  selectEl.appendChild(createOption(placeholder, ""));
  [...new Set(options.filter(Boolean))].forEach((opt) => {
    selectEl.appendChild(createOption(opt, opt));
  });
  if ([...selectEl.options].some((opt) => opt.value === currentValue)) {
    selectEl.value = currentValue;
  }
}

function uniqueList(...groups) {
  return [...new Set(groups.flat().filter(Boolean))];
}

function getFeaturesForModule(config, moduleId = "") {
  return (config.features || []).filter(
    (feature) => !moduleId || feature.module === moduleId,
  );
}

function aggregateUiHints(items = []) {
  const merged = {};
  const allKeys = new Set(
    items.flatMap((item) => Object.keys(item?.uiHints || {})),
  );

  allKeys.forEach((key) => {
    const values = items.flatMap((item) => {
      const next = item?.uiHints?.[key];
      if (Array.isArray(next)) return next;
      return next ? [next] : [];
    });
    if (values.length) {
      merged[key] = uniqueList(values);
    }
  });

  return merged;
}

function buildAggregateFeature(features = [], moduleId = "") {
  if (!features.length) return null;
  const owners = uniqueList(features.map((feature) => feature.owner));
  return {
    id: ALL_FEATURES_ID,
    name: moduleId ? `All Features in ${moduleId}` : "All Features",
    module: moduleId || "",
    summary: moduleId
      ? `Aggregate context across ${features.length} features in ${moduleId}.`
      : `Aggregate context across ${features.length} features in the catalog.`,
    owner:
      owners.length === 1
        ? owners[0]
        : owners.length > 1
          ? owners.join(", ")
          : "Multiple owners",
    relatedFiles: uniqueList(
      features.flatMap((feature) => feature.relatedFiles || []),
    ),
    relatedServices: uniqueList(
      features.flatMap((feature) => feature.relatedServices || []),
    ),
    uiHints: aggregateUiHints(features),
    promptDefaults: {},
    subFeatures: uniqueList(
      features.flatMap((feature) => feature.subFeatures || []),
    ),
  };
}

function buildAggregateSubFeature(
  subFeatures = [],
  feature = null,
  moduleId = "",
) {
  if (!subFeatures.length) return null;
  return {
    id: ALL_SUBFEATURES_ID,
    name:
      feature && feature.id !== ALL_FEATURES_ID
        ? `All Sub-Features in ${feature.name}`
        : moduleId
          ? `All Sub-Features in ${moduleId}`
          : "All Sub-Features",
    relatedFiles: uniqueList(
      subFeatures.flatMap((subFeature) => subFeature.relatedFiles || []),
      feature?.relatedFiles || [],
    ),
    uiHints: aggregateUiHints(subFeatures),
    promptDefaults:
      feature?.id === ALL_FEATURES_ID ? {} : feature?.promptDefaults || {},
  };
}

function mergePromptDefaults(feature, subFeature) {
  return {
    ...(feature?.promptDefaults || {}),
    ...(subFeature?.promptDefaults || {}),
  };
}

function mergeFeatureUiHints(feature, subFeature) {
  const featureHints = feature?.uiHints || {};
  const subHints = subFeature?.uiHints || {};

  const merged = {};
  const allKeys = new Set([
    ...Object.keys(featureHints),
    ...Object.keys(subHints),
  ]);

  allKeys.forEach((key) => {
    const featureValue = featureHints[key];
    const subValue = subHints[key];
    if (Array.isArray(featureValue) || Array.isArray(subValue)) {
      merged[key] = uniqueList(subValue || [], featureValue || []);
    } else if (subValue !== undefined || featureValue !== undefined) {
      merged[key] = subValue ?? featureValue;
    }
  });

  return {
    ...merged,
    workflowAreas: uniqueList(
      subHints.workflowAreas,
      featureHints.workflowAreas,
      subFeature?.name,
      feature?.name,
    ),
    screens: uniqueList(
      subHints.screens,
      subFeature?.name,
      featureHints.screens,
    ),
    currentUiPatterns: uniqueList(
      subHints.currentUiPatterns,
      featureHints.currentUiPatterns,
    ),
    desiredUiPatterns: uniqueList(
      subHints.desiredUiPatterns,
      featureHints.desiredUiPatterns,
    ),
    uiElements: uniqueList(subHints.uiElements, featureHints.uiElements),
    userActions: uniqueList(subHints.userActions, featureHints.userActions),
    accessibilityNeeds: uniqueList(
      subHints.accessibilityNeeds,
      featureHints.accessibilityNeeds,
    ),
    preserveRules: uniqueList(
      subHints.preserveRules,
      featureHints.preserveRules,
    ),
    acceptanceCriteria: uniqueList(
      subHints.acceptanceCriteria,
      featureHints.acceptanceCriteria,
    ),
    relatedFiles: uniqueList(
      subHints.relatedFiles,
      subFeature?.relatedFiles,
      featureHints.relatedFiles,
      feature?.relatedFiles,
    ),
  };
}

function getSelectedFeatureContext(config) {
  const moduleId = $("module-select")?.value || "";
  const featureId = $("feature-select")?.value || "";
  const subFeatureId = $("subfeature-select")?.value || "";
  const moduleFeatures = getFeaturesForModule(config, moduleId);

  let selectedFeatures = [];
  let feature = null;

  if (featureId === ALL_FEATURES_ID) {
    selectedFeatures = moduleFeatures;
    feature = buildAggregateFeature(selectedFeatures, moduleId);
  } else {
    feature =
      (config.features || []).find(
        (nextFeature) => nextFeature.id === featureId,
      ) || null;
    selectedFeatures = feature ? [feature] : [];
  }

  let subFeature = null;
  if (subFeatureId === ALL_SUBFEATURES_ID) {
    const selectedSubFeatures = selectedFeatures.flatMap(
      (nextFeature) => nextFeature.subFeatures || [],
    );
    subFeature = buildAggregateSubFeature(
      selectedSubFeatures,
      feature,
      moduleId,
    );
  } else if (subFeatureId) {
    subFeature =
      selectedFeatures
        .flatMap((nextFeature) => nextFeature.subFeatures || [])
        .find((nextSubFeature) => nextSubFeature.id === subFeatureId) || null;
  }

  return {
    feature,
    subFeature,
    hints: mergeFeatureUiHints(feature, subFeature),
    promptDefaults: mergePromptDefaults(feature, subFeature),
  };
}

function applyFeatureHints(config, form) {
  if (!config || !form) return;

  const { hints, promptDefaults } = getSelectedFeatureContext(config);
  const shared = config.shared || {};

  document.querySelectorAll("[data-feature-hint-key]").forEach((el) => {
    const key = el.dataset.featureHintKey;
    const baseValues = el.dataset.optionSource
      ? shared[el.dataset.optionSource] || []
      : [];
    const values = uniqueList(hints[key] || [], baseValues);
    if (el.dataset.fieldType === "select") {
      populateSelectOptions(el, values, el.dataset.placeholder || "Select…");
    } else if (el.dataset.fieldType === "checkbox-group") {
      el.setOptions?.(values);
    }
  });

  document.querySelectorAll("[data-autofill-feature-key]").forEach((el) => {
    const key = el.dataset.autofillFeatureKey;
    const values = hints[key] || [];
    const existing = readEl(el);
    const priorAutoFill = el.dataset.autoFilledValue
      ? JSON.parse(el.dataset.autoFilledValue)
      : null;
    const isEmpty = Array.isArray(existing) ? existing.length === 0 : !existing;
    const matchesPriorAutoFill =
      JSON.stringify(existing ?? null) ===
      JSON.stringify(priorAutoFill ?? null);

    if (!values.length) {
      if (matchesPriorAutoFill) {
        writeEl(el, Array.isArray(existing) ? [] : "");
        delete el.dataset.autoFilledValue;
      }
      return;
    }

    if (isEmpty || matchesPriorAutoFill) {
      writeEl(el, values);
      el.dataset.autoFilledValue = JSON.stringify(values);
    }
  });

  Object.entries(promptDefaults).forEach(([fieldId, value]) => {
    const el =
      $(fieldId) || document.querySelector(`[data-field-id="${fieldId}"]`);
    if (!el) return;
    const existing = readEl(el);
    const isEmpty = Array.isArray(existing) ? existing.length === 0 : !existing;
    if (isEmpty) {
      writeEl(el, value);
    }
  });

  const helper = $("feature-aware-hint");
  if (helper) {
    const hasHints =
      hints.screens.length ||
      hints.uiElements.length ||
      hints.relatedFiles.length;
    helper.textContent = hasHints
      ? "Helpful choices were loaded for this feature so the user can pick more and type less."
      : "Pick a feature to load smarter choices and suggested files.";
    helper.classList.toggle("hidden", false);
  }
}

// ── Natural-language pre-fill ──────────────────────────────────────────────────
function renderNlWidget() {
  const form = $("copilot-form");
  if (!form || $("nl-widget")) return;

  const widget = make("div", { id: "nl-widget", class: "nl-widget" });
  const hdr = make("div", { class: "nl-widget-header" });
  hdr.appendChild(
    make("h3", { class: "nl-widget-title" }, "Describe your request"),
  );
  hdr.appendChild(
    make(
      "p",
      { class: "nl-widget-hint" },
      "Write what you want in plain English, then press Parse to fill the structured fields below.",
    ),
  );

  const ta = make("textarea", {
    id: "nl-input",
    class: "nl-input",
    rows: "4",
    placeholder:
      'e.g. "The PO lookup dropdown in receiving workflow shows stale data after a barcode scan — it should refresh automatically. Priority: High."',
  });

  const footer = make("div", { class: "nl-widget-footer" });
  const parseBtn = make(
    "button",
    { id: "nl-parse-btn", type: "button", class: "btn btn-accent btn-sm" },
    "Parse into fields \u2193",
  );
  const clearBtn = make(
    "button",
    { id: "nl-clear-btn", type: "button", class: "btn btn-ghost btn-sm" },
    "Clear",
  );
  const hint = make("span", { id: "nl-hint", class: "nl-result-hint hidden" });
  footer.appendChild(parseBtn);
  footer.appendChild(clearBtn);
  footer.appendChild(hint);

  widget.appendChild(hdr);
  widget.appendChild(ta);
  widget.appendChild(footer);
  form.insertBefore(widget, form.firstChild);
}

function extractListItems(text, fc) {
  const numbered = text.match(/^\s*\d+[.)]\s+(.+)$/gm);
  if (numbered?.length)
    return numbered
      .map((l) => l.replace(/^\s*\d+[.)]\s+/, "").trim())
      .filter(Boolean);
  const bulleted = text.match(/^\s*[-*\u2022]\s+(.+)$/gm);
  if (bulleted?.length)
    return bulleted
      .map((l) => l.replace(/^\s*[-*\u2022]\s+/, "").trim())
      .filter(Boolean);
  if (/step|repro|scen|file|path/i.test(fc.label || "")) {
    return text
      .split(/\.\s+(?=[A-Z])|;\s*|\n+/)
      .map((s) => s.trim())
      .filter((s) => s.length > 4)
      .slice(0, 8);
  }
  return [];
}

function extractRelevantSnippet(text, fc) {
  const keywords = [fc.label, fc.placeholder, fc.helpText]
    .flatMap((s) => (s || "").toLowerCase().split(/\W+/))
    .filter((w) => w.length > 3);
  const sentences = text.split(/(?<=[.!?])\s+|\n{2,}/);
  let bestScore = 0,
    bestSentence = "";
  for (const s of sentences) {
    const sl = s.toLowerCase();
    const score = keywords.reduce((n, kw) => n + (sl.includes(kw) ? 1 : 0), 0);
    if (score > bestScore) {
      bestScore = score;
      bestSentence = s.trim();
    }
  }
  if (bestScore > 0) return bestSentence;
  if (
    fc.type === "textarea" &&
    /summary|desc|context|overview/i.test(fc.label || "")
  ) {
    return sentences[0]?.trim() || "";
  }
  return "";
}

function parseNlIntoFields(form, config, text) {
  const lower = text.toLowerCase();
  const shared = config.shared || {};
  const result = {};
  const currentContext = getSelectedFeatureContext(config);

  const featureSel = $("feature-select");
  if (featureSel && !featureSel.value) {
    let bestFeature = null,
      bestScore = 0;
    for (const f of config.features || []) {
      const bag = [f.name, f.module, f.id, ...(f.relatedFiles || [])]
        .join(" ")
        .toLowerCase()
        .split(/[\s/_.-]+/)
        .filter((w) => w.length > 2);
      const score = bag.reduce((n, w) => n + (lower.includes(w) ? 1 : 0), 0);
      if (score > bestScore) {
        bestScore = score;
        bestFeature = f;
      }
    }
    if (bestFeature && bestScore > 0) result.__feature__ = bestFeature.id;
  }

  for (const group of form.fieldGroups || []) {
    for (const fc of group.fields) {
      if (fc.type === "select") {
        const opts = uniqueList(
          fc.featureHintKey
            ? currentContext.hints[fc.featureHintKey] || []
            : [],
          Array.isArray(fc.options)
            ? fc.options
            : shared[fc.optionSource] || [],
        );
        for (const opt of opts) {
          if (lower.includes(opt.toLowerCase())) {
            result[fc.id] = opt;
            break;
          }
        }
      } else if (fc.type === "checkbox-group") {
        const opts = uniqueList(
          fc.featureHintKey
            ? currentContext.hints[fc.featureHintKey] || []
            : [],
          Array.isArray(fc.options)
            ? fc.options
            : shared[fc.optionSource] || [],
        );
        const matches = opts.filter((opt) => lower.includes(opt.toLowerCase()));
        if (matches.length) result[fc.id] = matches;
      } else if (fc.type === "list") {
        const items = extractListItems(text, fc);
        if (items.length) result[fc.id] = items;
      } else if (fc.type === "text" || fc.type === "textarea") {
        const snippet = extractRelevantSnippet(text, fc);
        if (snippet) result[fc.id] = snippet;
      }
    }
  }
  return result;
}

function initNlPrefill(form, config) {
  renderNlWidget();

  $("nl-parse-btn")?.addEventListener("click", () => {
    const text = $("nl-input")?.value?.trim() || "";
    if (!text) {
      showToast("Enter a description first", "warn");
      return;
    }

    const suggestions = parseNlIntoFields(form, config, text);
    let applied = 0;

    const featureSel = $("feature-select");
    if (suggestions.__feature__ && featureSel && !featureSel.value) {
      featureSel.value = suggestions.__feature__;
      saveSticky(featureSel.value);
      applied++;
    }
    delete suggestions.__feature__;

    for (const [id, val] of Object.entries(suggestions)) {
      const el = $(id) || document.querySelector(`[data-field-id="${id}"]`);
      if (!el) continue;
      const existing = readEl(el);
      const isEmpty = Array.isArray(existing)
        ? existing.length === 0
        : !existing;
      if (isEmpty) {
        writeEl(el, val);
        applied++;
      }
    }

    document.body.dispatchEvent(new Event("input"));

    const hint = $("nl-hint");
    if (hint) {
      hint.textContent =
        applied > 0
          ? `\u2713 Filled ${applied} field${applied !== 1 ? "s" : ""}`
          : "No fields matched \u2014 try more specific text";
      hint.className = `nl-result-hint ${applied > 0 ? "nl-hint-ok" : "nl-hint-none"}`;
    }
    showToast(
      applied > 0
        ? `Filled ${applied} field${applied !== 1 ? "s" : ""}`
        : "No fields matched",
      applied > 0 ? "success" : "info",
    );
  });

  $("nl-clear-btn")?.addEventListener("click", () => {
    const ta = $("nl-input");
    if (ta) ta.value = "";
    const hint = $("nl-hint");
    if (hint) hint.className = "nl-result-hint hidden";
  });
}

// ── Field helpers ──────────────────────────────────────────────────────────────
function readEl(el) {
  if (!el) return "";
  if (el.classList.contains("list-widget")) return el.readValue?.() ?? [];
  if (el.classList.contains("checkbox-group-widget"))
    return el.readValue?.() ?? [];
  if (el.dataset?.fieldType === "tristate")
    return el.dataset.value || "ai-decides";
  if (el.dataset?.fieldType === "checkbox") return el.checked;
  return (el.value ?? "").trim();
}

function writeEl(el, val) {
  if (!el) return;
  if (el.classList.contains("list-widget")) {
    el.writeValue?.(val);
    return;
  }
  if (el.classList.contains("checkbox-group-widget")) {
    el.writeValue?.(val);
    return;
  }
  if (el.dataset?.fieldType === "tristate") {
    const v = String(val || "ai-decides");
    el.dataset.value = v;
    el.querySelectorAll(".tristate-btn").forEach((btn) => {
      btn.classList.toggle("tristate-active", btn.dataset.val === v);
    });
    return;
  }
  if (el.dataset?.fieldType === "checkbox") {
    el.checked = Boolean(val);
    return;
  }
  if (Array.isArray(val)) {
    el.value = val.join("\n");
    return;
  }
  el.value = val ?? "";
}

function allFieldEls() {
  const seen = new Set();
  const out = [];
  document
    .querySelectorAll("[data-field-id]:not(.modal [data-field-id])")
    .forEach((el) => {
      if (!seen.has(el)) {
        seen.add(el);
        out.push(el);
      }
    });
  document
    .querySelectorAll(".list-widget[id]:not(.modal .list-widget)")
    .forEach((el) => {
      if (!seen.has(el)) {
        seen.add(el);
        out.push(el);
      }
    });
  return out;
}

function createOption(text, value) {
  const o = make("option", {});
  o.textContent = text;
  o.value = value;
  return o;
}

// ── Create field element ───────────────────────────────────────────────────────
function createField(fc, shared) {
  const wrap = make("div", {
    class: "field" + (fc.required ? " field-required" : ""),
  });
  const lbl = make("label", { for: fc.id });
  lbl.textContent = fc.label;
  wrap.appendChild(lbl);

  if (fc.type === "list") {
    const w = createListWidget(
      fc,
      Array.isArray(fc.defaultItems) ? fc.defaultItems : [],
    );
    w.id = fc.id;
    wrap.appendChild(w);
    if (fc.featureHintKey) w.dataset.featureHintKey = fc.featureHintKey;
    if (fc.autoFillFeatureKey)
      w.dataset.autofillFeatureKey = fc.autoFillFeatureKey;
  } else if (fc.type === "checkbox-group") {
    const opts = Array.isArray(fc.options)
      ? fc.options
      : shared[fc.optionSource] || [];
    const w = createCheckboxGroupWidget(
      fc,
      opts,
      Array.isArray(fc.defaultItems) ? fc.defaultItems : [],
    );
    if (fc.featureHintKey) w.dataset.featureHintKey = fc.featureHintKey;
    if (fc.optionSource) w.dataset.optionSource = fc.optionSource;
    wrap.appendChild(w);
  } else if (fc.type === "textarea") {
    const ta = make("textarea", {
      id: fc.id,
      "data-field-id": fc.id,
      "data-field-type": "textarea",
      rows: String(fc.rows || 4),
      placeholder: fc.placeholder || "",
    });
    if (fc.required) ta.required = true;
    if (fc.autoFillFeatureKey)
      ta.dataset.autofillFeatureKey = fc.autoFillFeatureKey;
    wrap.appendChild(ta);
  } else if (fc.type === "select") {
    const sel = make("select", {
      id: fc.id,
      "data-field-id": fc.id,
      "data-field-type": "select",
    });
    sel.dataset.placeholder = fc.placeholder || "Select…";
    sel.appendChild(createOption(fc.placeholder || "Select…", ""));
    const opts = Array.isArray(fc.options)
      ? fc.options
      : shared[fc.optionSource] || [];
    opts.forEach((v) => sel.appendChild(createOption(v, v)));
    if (fc.featureHintKey) sel.dataset.featureHintKey = fc.featureHintKey;
    if (fc.optionSource) sel.dataset.optionSource = fc.optionSource;
    if (fc.required) sel.required = true;
    wrap.appendChild(sel);
  } else {
    const inp = make("input", {
      type: fc.type || "text",
      id: fc.id,
      "data-field-id": fc.id,
      "data-field-type": fc.type || "text",
      placeholder: fc.placeholder || "",
    });
    if (fc.required) inp.required = true;
    wrap.appendChild(inp);
  }

  if (fc.helpText)
    wrap.appendChild(make("p", { class: "field-help" }, fc.helpText));
  return wrap;
}

// ── Progress tracking ──────────────────────────────────────────────────────────
function updateProgress(form) {
  if (!form) return;
  const required = [];
  (form.fieldGroups || []).forEach((g) =>
    g.fields.forEach((f) => {
      if (f.required) required.push(f.id);
    }),
  );
  let filled = 0;
  required.forEach((id) => {
    const el = $(id) || document.querySelector(`[data-field-id="${id}"]`);
    const val = readEl(el);
    if (Array.isArray(val) ? val.length > 0 : Boolean(val)) filled++;
  });
  const total = required.length;
  const pct = total > 0 ? Math.round((filled / total) * 100) : 100;
  const bar = $("progress-bar");
  const txt = $("progress-text");
  if (bar) {
    bar.style.width = `${pct}%`;
    bar.className =
      "progress-bar-fill " +
      (pct === 100
        ? "progress-complete"
        : pct > 50
          ? "progress-partial"
          : "progress-low");
  }
  if (txt) txt.textContent = `${filled} / ${total}`;
}

// ── Feature summary ────────────────────────────────────────────────────────────
function showFeatureSummary(feature, subFeature = null) {
  const el = $("feature-summary");
  if (!el) return;
  if (!feature) {
    el.classList.add("hidden");
    return;
  }
  el.classList.remove("hidden");
  el.innerHTML = `
        ${subFeature ? `<div class="feature-summary-row"><span class="feature-label">Sub-feature</span><span class="feature-value">${escHtml(subFeature.name)}</span></div>` : ""}
        <div class="feature-summary-row">
            <span class="feature-label">Summary</span>
            <span class="feature-value">${escHtml(feature.summary)}</span>
        </div>
        <div class="feature-summary-row">
            <span class="feature-label">Owner</span>
            <span class="feature-value">${escHtml(feature.owner)}</span>
        </div>
        <div class="feature-summary-row">
            <span class="feature-label">Files</span>
            <span class="feature-value muted">${((subFeature || feature).relatedFiles || feature.relatedFiles || []).map((f) => `<code>${escHtml(f)}</code>`).join(", ") || "—"}</span>
        </div>`;
}

// ── Guide lists ────────────────────────────────────────────────────────────────
function renderList(id, items) {
  const el = $(id);
  if (!el) return;
  el.innerHTML =
    (items || []).map((i) => `<li>${escHtml(i)}</li>`).join("") || "<li>—</li>";
}

// ── Agent Flags widget (injected into every form sidebar) ─────────────────────
function renderAgentFlags() {
  const actionsDiv = document.querySelector(".sidebar-actions");
  if (!actionsDiv || $("agent-flags-section")) return;

  const section = make("div", {
    id: "agent-flags-section",
    class: "sidebar-section agent-flags-section",
  });
  section.appendChild(
    make("div", { class: "agent-flags-title" }, "Agent Flags"),
  );

  // ── Serena row ───────────────────────────────────────────────────────────────
  const serenaRow = make("div", { class: "agent-flag-row" });
  serenaRow.appendChild(make("span", { class: "agent-flag-label" }, "Serena"));

  const tristate = make("div", {
    id: "serena-mode",
    class: "tristate-toggle",
    "data-field-id": "serena-mode",
    "data-field-type": "tristate",
    "data-value": "ai-decides",
  });

  [
    {
      val: "mandatory",
      symbol: "✔",
      title: "Mandatory — AI must use Serena MCP tools",
    },
    {
      val: "ai-decides",
      symbol: "–",
      title: "Up to AI — agent decides whether to use Serena",
    },
    { val: "off", symbol: "✕", title: "Off — do not use Serena" },
  ].forEach(({ val, symbol, title }) => {
    const btn = make(
      "button",
      {
        type: "button",
        class:
          "tristate-btn" + (val === "ai-decides" ? " tristate-active" : ""),
        "data-val": val,
        title,
      },
      symbol,
    );
    btn.addEventListener("click", () => {
      tristate
        .querySelectorAll(".tristate-btn")
        .forEach((b) => b.classList.remove("tristate-active"));
      btn.classList.add("tristate-active");
      tristate.dataset.value = val;
      document.body.dispatchEvent(new Event("change"));
    });
    tristate.appendChild(btn);
  });

  serenaRow.appendChild(tristate);
  section.appendChild(serenaRow);

  // ── Noob Mode row ────────────────────────────────────────────────────────────
  const noobRow = make("div", { class: "agent-flag-row" });
  noobRow.appendChild(
    make("label", { class: "agent-flag-label", for: "noob-mode" }, "Noob Mode"),
  );
  const toggleLabel = make("label", { class: "toggle-switch" });
  const noobCb = make("input", {
    type: "checkbox",
    id: "noob-mode",
    "data-field-id": "noob-mode",
    "data-field-type": "checkbox",
  });
  toggleLabel.appendChild(noobCb);
  toggleLabel.appendChild(make("span", { class: "toggle-slider" }));
  noobRow.appendChild(toggleLabel);
  section.appendChild(noobRow);

  actionsDiv.parentElement.insertBefore(section, actionsDiv);
}

// ── Draft persistence ──────────────────────────────────────────────────────────
function saveDraft(formId) {
  const vals = {
    _moduleId: $("module-select")?.value || "",
    _featureId: $("feature-select")?.value || "",
    _subFeatureId: $("subfeature-select")?.value || "",
  };
  allFieldEls().forEach((el) => {
    const id = el.id || el.dataset?.fieldId;
    if (id) vals[id] = readEl(el);
  });
  localStorage.setItem(KEY_DRAFT(formId), JSON.stringify(vals));
}

function getDraft(formId) {
  try {
    return JSON.parse(localStorage.getItem(KEY_DRAFT(formId)) || "null");
  } catch {
    return null;
  }
}

function applyDraft(formId, config = null) {
  const draft = getDraft(formId);
  if (!draft) return;
  allFieldEls().forEach((el) => {
    const id = el.id || el.dataset?.fieldId;
    if (id && Object.prototype.hasOwnProperty.call(draft, id))
      writeEl(el, draft[id]);
  });
  if (draft._featureId && config) {
    syncCascadeFromFeatureId(
      config,
      draft._featureId,
      draft._subFeatureId || "",
      draft._moduleId || "",
    );
  } else if (draft._featureId) {
    const sel = $("feature-select");
    if (sel) sel.value = draft._featureId;
  }
}

// ── Collect all form values ────────────────────────────────────────────────────
function collectValues() {
  const out = {};
  allFieldEls().forEach((el) => {
    const id = el.id || el.dataset?.fieldId;
    if (id) out[id] = readEl(el);
  });
  return out;
}

function getFieldValue(values, ids = []) {
  for (const id of ids) {
    const value = values[id];
    if (Array.isArray(value) && value.length) return value.join("; ");
    if (value) return String(value);
  }
  return "";
}

function buildHumanSummary(form, feature, subFeature, values) {
  const title = getFieldValue(values, [
    "requestTitle",
    "bugTitle",
    "docTitle",
    "reviewTitle",
    "databaseTitle",
    "testRequestTitle",
    "mockupTitle",
  ]);
  const problem = getFieldValue(values, [
    "changeSummary",
    "observedBehavior",
    "workflowIntent",
    "motivation",
    "reason",
    "reviewGoal",
    "databaseProblem",
    "testIntent",
    "docProblem",
    "mockupPurpose",
  ]);
  const desiredOutcome = getFieldValue(values, [
    "successLooksLike",
    "expectedBehavior",
    "targetOutcome",
    "preferredOutcome",
    "docOutcome",
  ]);

  const targetName =
    subFeature?.name || feature?.name || "an unspecified feature";
  const includeModule =
    feature?.module &&
    !targetName.toLowerCase().includes(feature.module.toLowerCase());

  const lines = [];
  lines.push(
    `This ${form.title.toLowerCase()} is for ${targetName}${includeModule ? ` in ${feature.module}` : ""}.`,
  );
  if (title) {
    lines.push(`Short title: ${title}.`);
  }
  if (problem) {
    lines.push(`Main problem or request: ${problem}`);
  }
  if (desiredOutcome) {
    lines.push(`Desired result: ${desiredOutcome}`);
  }
  return lines.join(" ").trim();
}

function buildMetadataReviewInfo(feature, values) {
  const moduleName =
    feature?.module || values.moduleName || "the edited module";
  const paths = ["docs/CopilotForms/data/copilot-forms.config.json"];

  if (moduleName && moduleName !== "the edited module") {
    paths.push(`docs/CopilotForms/data/module-metadata/${moduleName}/`);
  } else {
    paths.push("docs/CopilotForms/data/module-metadata/<ModuleName>/");
  }

  return {
    moduleName,
    paths,
    instruction:
      "As part of this request, review and update the CopilotForms metadata for the edited module if it is stale.",
  };
}

// ── Build markdown output ──────────────────────────────────────────────────────
function buildMarkdown(form, feature, values) {
  const lines = [];
  lines.push(`# ${form.title} Export`, "");
  lines.push(`- Generated: ${new Date().toISOString()}`);
  lines.push(`- Form: ${form.id}`);
  const { subFeature: _subFeature } = getSelectedFeatureContext(_config);
  const humanSummary = buildHumanSummary(form, feature, _subFeature, values);
  const metadataReview = buildMetadataReviewInfo(feature, values);
  lines.push(
    `- Feature: ${feature?.name || values.featureName || "Unspecified"}`,
  );
  lines.push(`- Sub-feature: ${_subFeature?.name || "(none selected)"}`);
  lines.push(
    `- Module: ${feature?.module || values.moduleName || "Unspecified"}`,
  );
  lines.push(`- Prompt File: ${form.promptFile}`);
  lines.push(`- Instruction File: ${form.instructionFile}`);
  lines.push(`- Output Folder: ${form.outputFolder}`);
  lines.push("");
  lines.push("## Human Summary", "");
  lines.push(
    humanSummary ||
      "_Not enough information was provided to build a plain-English summary yet._",
  );
  lines.push("");

  // ── Agent Flags ──────────────────────────────────────────────────────────
  const _serenaVal = values["serena-mode"] || "ai-decides";
  const _noobVal = values["noob-mode"] ?? false;
  const _serenaLabel =
    _serenaVal === "mandatory"
      ? "\u2714 Mandatory"
      : _serenaVal === "off"
        ? "\u2715 Off (do not use)"
        : "\u2013 Up to AI";
  lines.push("## Agent Flags", "");
  lines.push(`- Serena: ${_serenaLabel}`);
  lines.push(`- Noob Mode: ${_noobVal ? "\u2714 On" : "\u2715 Off"}`);
  lines.push("");
  if (_noobVal) {
    lines.push(
      "> **Noob Mode active** \u2014 explain all reasoning step-by-step, avoid jargon, define terms on first use, and include inline comments on every non-trivial line of generated code.",
    );
    lines.push("");
  }
  if (_serenaVal === "mandatory") {
    lines.push(
      "> **Serena mandatory** \u2014 you MUST use Serena MCP tools (`get_symbols_overview`, `find_symbol`, `find_referencing_symbols`, `replace_symbol_body`, etc.) for all code navigation and editing. Do not read full files when Serena can provide targeted symbol access.",
    );
    lines.push("");
  } else if (_serenaVal === "off") {
    lines.push(
      "> **Serena disabled** \u2014 do NOT use Serena MCP tools. Use standard file read/edit tools only.",
    );
    lines.push("");
  }

  if (feature) {
    lines.push("## Catalog Context", "");
    lines.push(`- Summary: ${feature.summary}`);
    lines.push(`- Owner: ${feature.owner}`);
    lines.push(
      `- Related Files: ${(feature.relatedFiles || []).join(", ") || "—"}`,
    );
    lines.push(
      `- Related Services: ${(feature.relatedServices || []).join(", ") || "—"}`,
    );
    lines.push("");
  }

  if ((feature?.relatedFiles || []).length) {
    lines.push("## Suggested Starting Points", "");
    (feature.relatedFiles || []).forEach((file) => lines.push(`- ${file}`));
    lines.push("");
  }

  lines.push("## Required Metadata Follow-Up", "");
  lines.push(`- ${metadataReview.instruction}`);
  lines.push(`- Module to review: ${metadataReview.moduleName}`);
  metadataReview.paths.forEach((path) =>
    lines.push(`- Metadata path: ${path}`),
  );
  lines.push("");

  lines.push("## Structured Request", "");

  for (const group of form.fieldGroups || []) {
    lines.push(`## ${group.title}`, "");
    for (const field of group.fields) {
      const val = values[field.id];
      lines.push(`### ${field.label}`);
      if (Array.isArray(val)) {
        if (val.length === 0) lines.push("- None provided");
        else val.forEach((item) => lines.push(`- ${item}`));
      } else {
        lines.push(val ? String(val) : "_Not provided_");
      }
      lines.push("");
    }
  }

  lines.push("## Machine Data", "");
  lines.push("```json");
  lines.push(
    JSON.stringify(
      { formId: form.id, featureId: feature?.id || null, values },
      null,
      2,
    ),
  );
  lines.push("```", "");
  lines.push("## Copilot Execution Note", "");
  lines.push(
    "Use the linked prompt file in Copilot Chat if prompt files are available in your setup. If not, paste this export into chat and mention the prompt file path manually. Treat this export as the source of truth.",
  );
  return lines.join("\n");
}

function buildJson(form, feature, values) {
  const { subFeature } = getSelectedFeatureContext(_config);
  return JSON.stringify(
    {
      generatedAt: new Date().toISOString(),
      formId: form.id,
      title: form.title,
      outputFolder: form.outputFolder,
      promptFile: form.promptFile,
      humanSummary: buildHumanSummary(form, feature, subFeature, values),
      metadataReview: buildMetadataReviewInfo(feature, values),
      feature: feature || null,
      subFeature: subFeature || null,
      values,
    },
    null,
    2,
  );
}

// ── Download helper ────────────────────────────────────────────────────────────
function downloadText(filename, content) {
  const url = URL.createObjectURL(
    new Blob([content], { type: "text/plain;charset=utf-8" }),
  );
  const a = make("a", { href: url, download: filename });
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

async function copyText(content) {
  if (!navigator.clipboard) return false;
  await navigator.clipboard.writeText(content);
  return true;
}

// ── File System Access API save ────────────────────────────────────────────────
let _dirHandle = null;

function splitPathSegments(path) {
  return String(path || "")
    .replaceAll("\\", "/")
    .split("/")
    .filter(Boolean);
}

function getRepoRootFolderName(repoRoot) {
  const segments = splitPathSegments(repoRoot);
  return segments.length ? segments[segments.length - 1] : "";
}

function getRelativeOutputSegments(outputFolder, selectedFolderName, repoRoot) {
  const outputSegments = splitPathSegments(outputFolder);
  const repoRootName = getRepoRootFolderName(repoRoot);
  const loweredSelectedName = String(selectedFolderName || "").toLowerCase();

  if (!outputSegments.length) {
    return [];
  }

  if (loweredSelectedName === String(repoRootName).toLowerCase()) {
    return outputSegments;
  }

  const matchedIndex = outputSegments.findIndex(
    (segment) => segment.toLowerCase() === loweredSelectedName,
  );

  if (matchedIndex >= 0) {
    return outputSegments.slice(matchedIndex + 1);
  }

  return outputSegments;
}

async function ensureDirectoryPath(rootHandle, relativeSegments) {
  let currentHandle = rootHandle;
  for (const segment of relativeSegments) {
    currentHandle = await currentHandle.getDirectoryHandle(segment, {
      create: true,
    });
  }
  return currentHandle;
}

async function saveToOutputs(filename, content, outputFolder, repoRoot) {
  if (!window.showDirectoryPicker) {
    downloadText(filename, content);
    showToast("File System API not available — downloaded instead", "info");
    return true;
  }
  try {
    if (!_dirHandle) {
      showToast(
        "Select the repo root, docs/CopilotForms folder, or outputs folder to enable direct saving…",
        "info",
      );
      _dirHandle = await window.showDirectoryPicker({ mode: "readwrite" });
    }
    const relativeSegments = getRelativeOutputSegments(
      outputFolder,
      _dirHandle.name,
      repoRoot,
    );
    const targetDir = await ensureDirectoryPath(_dirHandle, relativeSegments);
    const fh = await targetDir.getFileHandle(filename, { create: true });
    const w = await fh.createWritable();
    await w.write(content);
    await w.close();
    const relativePath = [...relativeSegments, filename].join("/");
    showToast(`Saved → ${relativePath}`);
    return true;
  } catch (err) {
    if (err.name === "AbortError") {
      return false;
    }
    downloadText(filename, content);
    showToast("Could not save directly — downloaded instead", "warn");
    return true;
  }
}

// ── Output tabs ────────────────────────────────────────────────────────────────
function initOutputTabs() {
  const tabs = [...document.querySelectorAll(".output-tab")];
  tabs.forEach((tab) =>
    tab.addEventListener("click", () => {
      tabs.forEach((t) => t.classList.remove("active"));
      tab.classList.add("active");
      document
        .querySelectorAll(".output-content")
        .forEach((c) => c.classList.add("hidden"));
      $("tab-" + tab.dataset.tab)?.classList.remove("hidden");
    }),
  );
}

// ── Prompt runner ──────────────────────────────────────────────────────────────
function initPromptRunner(form) {
  const btn = $("run-prompt-btn");
  if (!btn || !form?.promptFile) return;
  btn.onclick = () => {
    const serenaVal = $("serena-mode")?.dataset?.value || "ai-decides";
    const noobOn = $("noob-mode")?.checked || false;
    const serenaHint =
      serenaVal === "mandatory"
        ? " Serena MCP tools are MANDATORY — use them for all code navigation and editing."
        : serenaVal === "off"
          ? " Do NOT use Serena MCP tools."
          : "";
    const noobHint = noobOn
      ? " Noob Mode ON — explain step-by-step, avoid jargon, add inline code comments."
      : "";
    const metadataHint =
      " Also review and update the CopilotForms metadata for the edited module as part of the same request.";
    const msg = `Use the prompt file \`${form.promptFile}\` in Copilot Chat if prompt files are available. If you cannot use prompt files directly, paste the saved export into chat and mention the prompt file path manually. Link the export saved to \`${form.outputFolder}\`.${metadataHint}${serenaHint}${noobHint}`;
    copyText(msg);
    showToast("Prompt text copied — paste into Copilot Chat!");
  };
}

// ── Refresh all outputs ────────────────────────────────────────────────────────
function refreshOutputs(config, form) {
  const { feature, subFeature } = getSelectedFeatureContext(config);
  showFeatureSummary(feature, subFeature);

  const values = collectValues();
  const md = buildMarkdown(form, feature, values);
  const json = buildJson(form, feature, values);

  const mdEl = $("markdown-output");
  const jEl = $("json-output");
  if (mdEl) mdEl.value = md;
  if (jEl) jEl.value = json;

  updateProgress(form);
  saveDraft(form.id);
}

// ── Feature catalog cascade helpers ───────────────────────────────────────────
function populateModuleSelect(config) {
  const mSel = $("module-select");
  if (!mSel) return;
  mSel.innerHTML = "";
  mSel.appendChild(createOption("── all modules ──", ""));
  const modules = [...new Set((config.features || []).map((f) => f.module))];
  modules.forEach((m) => mSel.appendChild(createOption(m, m)));
}

function populateFeatureSelect(config, moduleId) {
  const fSel = $("feature-select");
  if (!fSel) return;
  fSel.innerHTML = "";
  fSel.appendChild(createOption("── select feature ──", ""));
  const features = getFeaturesForModule(config, moduleId);
  if (features.length) {
    fSel.appendChild(createOption("── all features ──", ALL_FEATURES_ID));
  }
  features.forEach((feature) =>
    fSel.appendChild(createOption(feature.name, feature.id)),
  );
}

function populateSubFeatureSelect(config, featureId, moduleId = "") {
  const sfSel = $("subfeature-select");
  const sfField = $("subfeature-field");
  if (!sfSel) return;
  const features =
    featureId === ALL_FEATURES_ID
      ? getFeaturesForModule(config, moduleId)
      : (config.features || []).filter((feature) => feature.id === featureId);
  const subs = features.flatMap((feature) =>
    (feature.subFeatures || []).map((subFeature) => ({
      ...subFeature,
      _featureName: feature.name,
    })),
  );
  sfSel.innerHTML = "";
  if (subs.length) {
    sfSel.appendChild(createOption("── select sub-feature ──", ""));
    sfSel.appendChild(
      createOption("── all sub-features ──", ALL_SUBFEATURES_ID),
    );
    subs.forEach((subFeature) =>
      sfSel.appendChild(
        createOption(
          featureId === ALL_FEATURES_ID
            ? `${subFeature._featureName} — ${subFeature.name}`
            : subFeature.name,
          subFeature.id,
        ),
      ),
    );
    sfField?.classList.remove("hidden");
  } else {
    sfField?.classList.add("hidden");
  }
}

function syncCascadeFromFeatureId(
  config,
  featureId,
  subFeatureId = "",
  moduleIdOverride = "",
) {
  const feat = (config.features || []).find((f) => f.id === featureId);
  const moduleId =
    moduleIdOverride || feat?.module || $("module-select")?.value || "";
  const mSel = $("module-select");
  if (mSel) mSel.value = moduleId;
  populateFeatureSelect(config, moduleId);
  const fSel = $("feature-select");
  if (fSel && featureId) fSel.value = featureId;
  populateSubFeatureSelect(config, featureId, moduleId);
  const sfSel = $("subfeature-select");
  if (sfSel && subFeatureId) sfSel.value = subFeatureId;
}

// ── Status bar helper ──────────────────────────────────────────────────────────
function setStatus(msg, type = "ok") {
  const box = $("status-box");
  const txt = $("status-message");
  if (txt) txt.textContent = msg;
  if (box)
    box.className =
      "status-banner" +
      (type === "error"
        ? " status-error"
        : type === "warn"
          ? " status-warn"
          : "");
}

// ── Render the form fields and metadata ────────────────────────────────────────
function renderForm(config, form) {
  const bc = $("form-title-breadcrumb");
  if (bc) bc.textContent = form.title;
  const ti = $("form-title");
  if (ti) ti.textContent = form.title;
  const sm = $("form-summary");
  if (sm) sm.textContent = form.summary;

  const pl = $("prompt-file-link");
  if (pl) {
    pl.textContent = form.promptFile.split("/").pop();
    pl.href = `/${form.promptFile}`;
    pl.title = form.promptFile;
  }
  const il = $("instruction-file-link");
  if (il) {
    il.textContent = form.instructionFile.split("/").pop();
    il.href = `/${form.instructionFile}`;
    il.title = form.instructionFile;
  }

  const fl = $("output-folder");
  if (fl) fl.textContent = form.outputFolder;

  renderList("when-to-use-list", form.whenToUse);
  renderList("before-you-start-list", form.beforeYouStart);
  renderList("end-user-tips-list", form.endUserTips);

  // Module → Feature → Sub-feature cascade
  populateModuleSelect(config);
  populateFeatureSelect(config, "");
  populateSubFeatureSelect(config, "", "");
  const mSelEl = $("module-select");
  const fSelEl = $("feature-select");
  mSelEl?.addEventListener("change", () => {
    populateFeatureSelect(config, mSelEl.value);
    populateSubFeatureSelect(config, "", mSelEl.value);
  });
  fSelEl?.addEventListener("change", () => {
    populateSubFeatureSelect(config, fSelEl.value, mSelEl?.value || "");
  });

  // Dynamic fields
  const container = $("dynamic-fields");
  if (!container) return;
  container.innerHTML = "";
  container.appendChild(
    make(
      "div",
      { id: "feature-aware-hint", class: "feature-aware-hint hidden" },
      "Pick a feature to load smarter choices and suggested files.",
    ),
  );
  const defaults = loadDefaults();
  const defaultMap = {
    priority: defaults.priority,
    audience: defaults.audience,
    scope: defaults.scope,
    reviewScope: defaults.scope,
  };

  for (const group of form.fieldGroups || []) {
    const section = make("section", { class: "field-group" });
    section.appendChild(
      make("h3", { class: "field-group-title" }, group.title),
    );
    if (group.description)
      section.appendChild(
        make("p", { class: "field-group-desc" }, group.description),
      );
    const grid = make("div", {
      class: group.columns === 2 ? "field-grid grid-2" : "field-grid",
    });
    for (const fc of group.fields) {
      const fw = createField(fc, config.shared || {});
      const def = defaultMap[fc.id];
      if (def) {
        const inp = fw.querySelector(`[data-field-id="${fc.id}"],#${fc.id}`);
        if (inp) writeEl(inp, def);
      }
      grid.appendChild(fw);
    }
    section.appendChild(grid);
    container.appendChild(section);
  }
  document.title = `Copilot Forms — ${form.title}`;
}

// ── Collapsible guide sections ─────────────────────────────────────────────────
function initCollapsibles() {
  document.querySelectorAll(".collapsible-trigger").forEach((btn) => {
    btn.addEventListener("click", () => {
      const expanded = btn.getAttribute("aria-expanded") === "true";
      btn.setAttribute("aria-expanded", String(!expanded));
      btn.nextElementSibling?.classList.toggle("hidden", expanded);
    });
  });
}

// ── Defaults modal ─────────────────────────────────────────────────────────────
let _config = null;

function openDefaultsModal() {
  const modal = $("defaults-modal");
  if (!modal || !_config) return;
  const defs = loadDefaults();
  const shared = _config.shared || {};

  function populate(elId, items, key) {
    const el = $(elId);
    if (!el) return;
    el.innerHTML = "";
    el.appendChild(createOption("None", ""));
    (items || []).forEach((v) => el.appendChild(createOption(v, v)));
    if (defs[key]) el.value = defs[key];
  }

  const ff = $("default-feature");
  if (ff) {
    ff.innerHTML = "";
    ff.appendChild(createOption("None", ""));
    (_config.features || []).forEach((f) =>
      ff.appendChild(createOption(`${f.module} — ${f.name}`, f.id)),
    );
    if (defs.featureId) ff.value = defs.featureId;
  }

  populate("default-priority", shared.priorities, "priority");
  populate("default-audience", shared.audiences, "audience");
  populate("default-scope", shared.changeScopes, "scope");

  modal.classList.remove("hidden");
  modal.removeAttribute("aria-hidden");
  $("default-feature")?.focus();
}

function closeDefaultsModal() {
  const m = $("defaults-modal");
  if (m) {
    m.classList.add("hidden");
    m.setAttribute("aria-hidden", "true");
  }
}

// ── URL params pre-fill ────────────────────────────────────────────────────────
function applyUrlParams() {
  const params = new URLSearchParams(location.search);
  const fid = params.get("featureId");
  if (fid) {
    const sel = $("feature-select");
    if (sel) sel.value = fid;
  }
  for (const [k, v] of params.entries()) {
    if (k === "form" || k === "featureId") continue;
    const el =
      document.getElementById(k) ||
      document.querySelector(`[data-field-id="${k}"]`);
    if (el && (el.dataset?.fieldId || el.classList.contains("list-widget")))
      writeEl(el, v);
  }
}

// ── Sticky feature ─────────────────────────────────────────────────────────────
function loadSticky() {
  return sessionStorage.getItem(KEY_STICKY) || "";
}
function saveSticky(id) {
  if (id) sessionStorage.setItem(KEY_STICKY, id);
}

// ── Keyboard shortcuts ─────────────────────────────────────────────────────────
function initKeyboardShortcuts() {
  document.addEventListener("keydown", (e) => {
    if ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key === "C") {
      e.preventDefault();
      const content = $("markdown-output")?.value;
      if (content) copyText(content).then(() => showToast("Markdown copied!"));
    }
    if (e.key === "Escape") closeDefaultsModal();
  });
}

// ── Load config ────────────────────────────────────────────────────────────────
function resolvePath(basePath, relativePath) {
  return new URL(
    `${basePath}/${relativePath}`.replace(/\\/g, "/"),
    window.location.href,
  ).toString();
}

async function loadJsonFile(basePath, relativePath) {
  const r = await fetch(resolvePath(basePath, relativePath), {
    cache: "no-store",
  });
  if (!r.ok)
    throw new Error(`Config load failed (${r.status}) for ${relativePath}`);
  return r.json();
}

async function enrichConfigWithModuleMetadata(config, basePath) {
  const manifestPaths = config.project?.moduleMetadataIndexes || [];
  if (!manifestPaths.length) return config;

  const externalFeatures = [];
  for (const manifestPath of manifestPaths) {
    const manifest = await loadJsonFile(basePath, manifestPath);
    const manifestFolder = manifestPath.split("/").slice(0, -1).join("/");
    for (const featureFile of manifest.featureFiles || []) {
      const feature = await loadJsonFile(
        basePath,
        `${manifestFolder}/${featureFile}`,
      );
      externalFeatures.push(feature);
    }
  }

  const externalIds = new Set(externalFeatures.map((feature) => feature.id));
  return {
    ...config,
    features: [
      ...externalFeatures,
      ...(config.features || []).filter(
        (feature) => !externalIds.has(feature.id),
      ),
    ],
  };
}

async function loadConfig(basePath) {
  const config = await loadJsonFile(basePath, "data/copilot-forms.config.json");
  return enrichConfigWithModuleMetadata(config, basePath);
}

// ── Local config fallback ──────────────────────────────────────────────────────
function wireLocalConfig(cb) {
  const input = $("load-config-input");
  const btn = $("load-config-button");
  if (!input || !btn) return;
  btn.addEventListener("click", () => input.click());
  input.addEventListener("change", async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const cfg = JSON.parse(await file.text());
      setStatus(`Loaded: ${file.name}`, "ok");
      cb(cfg);
    } catch (err) {
      setStatus("Failed to parse config: " + err.message, "error");
    }
  });
}

// ── Index page ─────────────────────────────────────────────────────────────────
function scoreForm(form, query) {
  if (!query) return 1;
  const q = query.toLowerCase();
  const text = [form.title, form.summary, ...(form.whenToUse || [])]
    .join(" ")
    .toLowerCase();
  let score = 0;
  q.split(/\s+/).forEach((word) => {
    if (word.length < 2) return;
    if (text.includes(word)) score++;
    if (form.title.toLowerCase().includes(word)) score += 2;
  });
  return score;
}

function renderRecents() {
  const c = $("recent-exports-list");
  if (!c) return;
  const list = getRecents();
  if (!list.length) {
    c.innerHTML = `<p class="muted">No recent exports yet. Fill out a form and copy or save to see history here.</p>`;
    return;
  }
  c.innerHTML = "";
  list.slice(0, 8).forEach((e) => {
    const a = make("a", {
      class: "recent-item",
      href: `form.html?form=${e.formId}`,
      title: e.timestamp,
    });
    a.innerHTML = `
            <span class="recent-badge">${escHtml(e.formId)}</span>
            <span class="recent-feature">${escHtml(e.featureName)}</span>
            <span class="recent-time">${formatRelTime(e.timestamp)}</span>`;
    c.appendChild(a);
  });
}

function getRecentFormUsageMap() {
  return getRecents().reduce((acc, entry, index) => {
    if (!(entry.formId in acc)) {
      acc[entry.formId] = index;
    }
    return acc;
  }, {});
}

function getCardCategory(card) {
  const tag = card.querySelector(".card-tag");
  if (!tag) return "other";
  if (tag.classList.contains("tag-build")) return "build";
  if (tag.classList.contains("tag-debug")) return "debug";
  if (tag.classList.contains("tag-review")) return "review";
  if (tag.classList.contains("tag-data")) return "data";
  if (tag.classList.contains("tag-docs")) return "docs";
  return "other";
}

function formatCategoryLabel(category) {
  switch (category) {
    case "all":
      return "All";
    case "build":
      return "Build";
    case "debug":
      return "Debug";
    case "review":
      return "Review";
    case "data":
      return "Data";
    case "docs":
      return "Docs";
    default:
      return category.replace(/\b\w/g, (char) => char.toUpperCase());
  }
}

function updateIndexSummary(visibleCount, totalCount, activeFilter, query) {
  const summary = $("form-results-summary");
  if (!summary) return;
  const parts = [`Showing ${visibleCount} of ${totalCount} forms`];
  if (activeFilter && activeFilter !== "all") {
    parts.push(`filter: ${activeFilter}`);
  }
  if (query) {
    parts.push(`search: \"${query}\"`);
  }
  summary.textContent = parts.join(" · ");
}

function renderFilterChips(cards, activeFilter) {
  const host = $("form-filter-chips");
  if (!host) return;
  const categories = [
    "all",
    ...new Set(cards.map((card) => getCardCategory(card))),
  ];
  host.innerHTML = "";
  categories.forEach((category) => {
    const chip = make(
      "button",
      {
        type: "button",
        class: `filter-chip${category === activeFilter ? " filter-chip-active" : ""}`,
        "data-filter": category,
        "aria-pressed": String(category === activeFilter),
      },
      formatCategoryLabel(category),
    );
    host.appendChild(chip);
  });
}

function applyIndexFilters(cards, fMap, state) {
  const query = state.query.trim();
  const recentUsage = getRecentFormUsageMap();
  const scored = cards.map((card, originalIndex) => {
    const form = fMap[card.dataset.formId] || {};
    const category = getCardCategory(card);
    const score = scoreForm(form, query);
    const visible =
      (!query || score > 0) &&
      (state.filter === "all" || state.filter === category);

    return {
      card,
      category,
      originalIndex,
      score,
      title: (form.title || card.querySelector("h2")?.textContent || "").trim(),
      recentRank:
        recentUsage[card.dataset.formId] !== undefined
          ? recentUsage[card.dataset.formId]
          : Number.MAX_SAFE_INTEGER,
      visible,
    };
  });

  const visibleItems = scored.filter((item) => item.visible);
  visibleItems.sort((a, b) => {
    switch (state.sort) {
      case "az":
        return a.title.localeCompare(b.title);
      case "za":
        return b.title.localeCompare(a.title);
      case "category": {
        const byCategory = a.category.localeCompare(b.category);
        return byCategory || a.title.localeCompare(b.title);
      }
      case "recent": {
        const byRecent = a.recentRank - b.recentRank;
        return byRecent || a.title.localeCompare(b.title);
      }
      case "recommended":
      default: {
        const byScore = b.score - a.score;
        return byScore || a.originalIndex - b.originalIndex;
      }
    }
  });

  scored.forEach((item) => {
    item.card.classList.toggle("hidden-by-filter", !item.visible);
    item.card.style.order = "";
  });
  visibleItems.forEach((item, index) => {
    item.card.style.order = String(index);
  });

  $("search-clear")?.classList.toggle("hidden", !query);
  $("form-empty-state")?.classList.toggle("hidden", visibleItems.length !== 0);
  updateIndexSummary(visibleItems.length, cards.length, state.filter, query);
}

function initIndexPage(config) {
  const search = $("form-search");
  const cards = [...document.querySelectorAll(".form-card[data-form-id]")];
  const fMap = {};
  (config.forms || []).forEach((f) => {
    fMap[f.id] = f;
  });

  const state = {
    query: search?.value || "",
    filter: "all",
    sort: $("form-sort")?.value || "recommended",
  };

  const refresh = () => {
    renderFilterChips(cards, state.filter);
    applyIndexFilters(cards, fMap, state);
  };

  if (search) {
    search.addEventListener("input", () => {
      state.query = search.value;
      refresh();
    });
    $("search-clear")?.addEventListener("click", () => {
      search.value = "";
      state.query = "";
      refresh();
      search.focus();
    });
  }

  $("form-filter-chips")?.addEventListener("click", (event) => {
    const chip = event.target.closest("button[data-filter]");
    if (!chip) return;
    state.filter = chip.dataset.filter || "all";
    refresh();
  });

  $("form-sort")?.addEventListener("change", (event) => {
    state.sort = event.target.value || "recommended";
    refresh();
  });

  $("reset-form-filters")?.addEventListener("click", () => {
    state.query = "";
    state.filter = "all";
    state.sort = "recommended";
    if (search) search.value = "";
    if ($("form-sort")) $("form-sort").value = "recommended";
    refresh();
    search?.focus();
  });

  document.addEventListener("keydown", (event) => {
    const tag = event.target?.tagName?.toLowerCase();
    const isEditable =
      tag === "input" ||
      tag === "textarea" ||
      tag === "select" ||
      event.target?.isContentEditable;

    if (event.key === "/" && !isEditable) {
      event.preventDefault();
      search?.focus();
      search?.select();
    }

    if (
      event.key === "Escape" &&
      document.activeElement === search &&
      search?.value
    ) {
      event.preventDefault();
      search.value = "";
      state.query = "";
      refresh();
    }
  });

  $("theme-toggle")?.addEventListener("click", toggleTheme);
  renderRecents();
  refresh();
}

// ── Form page init ─────────────────────────────────────────────────────────────
async function initFormPage(
  config,
  basePath = document.body.dataset.basePath || ".",
) {
  _config = config;

  let formId =
    document.body.dataset.formId ||
    new URLSearchParams(location.search).get("form");
  if (!formId) {
    setStatus("No form ID specified. Use ?form=<id>", "error");
    return;
  }
  document.body.dataset.formId = formId;

  const form = (config.forms || []).find((f) => f.id === formId);
  if (!form) {
    setStatus(`Unknown form: "${formId}"`, "error");
    return;
  }

  setStatus("Config loaded", "ok");
  renderForm(config, form);
  renderAgentFlags();
  initNlPrefill(form, config);

  // Apply feature: sticky → URL → defaults → draft (last wins for draft)
  const sticky = loadSticky();
  if (sticky) syncCascadeFromFeatureId(config, sticky);
  applyUrlParams();
  const defs = loadDefaults();
  if (
    defs.featureId &&
    !new URLSearchParams(location.search).get("featureId")
  ) {
    if (!$("feature-select")?.value)
      syncCascadeFromFeatureId(config, defs.featureId);
  }
  applyDraft(formId, config);
  applyFeatureHints(config, form);

  refreshOutputs(config, form);
  initPromptRunner(form);
  initOutputTabs();
  initCollapsibles();

  // Reactive updates
  document.body.addEventListener("input", () => {
    refreshOutputs(config, form);
    saveSticky($("feature-select")?.value || "");
  });
  document.body.addEventListener("change", () => {
    refreshOutputs(config, form);
    saveSticky($("feature-select")?.value || "");
  });
  document.body.addEventListener("listchange", () =>
    refreshOutputs(config, form),
  );

  $("module-select")?.addEventListener("change", () => {
    applyFeatureHints(config, form);
    refreshOutputs(config, form);
  });

  $("feature-select")?.addEventListener("change", () => {
    applyFeatureHints(config, form);
    refreshOutputs(config, form);
  });

  $("subfeature-select")?.addEventListener("change", () => {
    applyFeatureHints(config, form);
    refreshOutputs(config, form);
  });

  // Copy markdown
  $("copy-markdown")?.addEventListener("click", async () => {
    const content = $("markdown-output")?.value;
    if (!content) return;
    await copyText(content);
    showToast("Markdown copied!");
    const { feature } = getSelectedFeatureContext(config);
    addRecent(form.id, form.title, feature?.name || "", content);
  });

  // Copy JSON
  $("copy-json")?.addEventListener("click", async () => {
    const content = $("json-output")?.value;
    if (!content) return;
    await copyText(content);
    showToast("JSON copied!");
  });

  // Download markdown
  $("download-markdown")?.addEventListener("click", async () => {
    const content = $("markdown-output")?.value || "";
    if (!content) return;
    const featureId = $("feature-select")?.value || "custom";
    const date = new Date().toISOString().slice(0, 10);
    const filename = `${featureId}-${formId}-${date}.export.md`;
    const saved = await saveToOutputs(
      filename,
      content,
      form.outputFolder,
      config.project?.repoRoot,
    );
    if (!saved) {
      downloadText(filename, content);
      showToast("Downloaded!", "info");
    }
  });

  // Download JSON
  $("download-json")?.addEventListener("click", async () => {
    const content = $("json-output")?.value || "";
    if (!content) return;
    const featureId = $("feature-select")?.value || "custom";
    const date = new Date().toISOString().slice(0, 10);
    const filename = `${featureId}-${formId}-${date}.export.json`;
    const saved = await saveToOutputs(
      filename,
      content,
      form.outputFolder,
      config.project?.repoRoot,
    );
    if (!saved) {
      downloadText(filename, content);
      showToast("Downloaded!", "info");
    }
  });

  // Save to outputs folder
  $("save-to-outputs")?.addEventListener("click", async () => {
    const content = $("markdown-output")?.value || "";
    if (!content) return;
    const featureId = $("feature-select")?.value || "custom";
    const date = new Date().toISOString().slice(0, 10);
    const filename = `${featureId}-${formId}-${date}.export.md`;
    await saveToOutputs(
      filename,
      content,
      form.outputFolder,
      config.project?.repoRoot,
    );
    addRecent(
      form.id,
      form.title,
      config.features?.find((f) => f.id === featureId)?.name || "",
      content,
    );
  });

  // Clear draft
  $("clear-draft")?.addEventListener("click", () => {
    localStorage.removeItem(KEY_DRAFT(formId));
    showToast("Draft cleared", "info");
    window.location.reload();
  });

  // Theme
  $("theme-toggle")?.addEventListener("click", toggleTheme);

  // Defaults modal
  $("defaults-btn")?.addEventListener("click", openDefaultsModal);
  $("defaults-close")?.addEventListener("click", closeDefaultsModal);
  $("cancel-defaults")?.addEventListener("click", closeDefaultsModal);
  $("defaults-modal")
    ?.querySelector(".modal-backdrop")
    ?.addEventListener("click", closeDefaultsModal);
  $("save-defaults")?.addEventListener("click", () => {
    saveDefaults({
      featureId: $("default-feature")?.value || "",
      priority: $("default-priority")?.value || "",
      audience: $("default-audience")?.value || "",
      scope: $("default-scope")?.value || "",
    });
    closeDefaultsModal();
    showToast("Defaults saved!");
  });

  wireLocalConfig(async (cfg) => {
    const enrichedConfig = await enrichConfigWithModuleMetadata(cfg, basePath);
    _config = enrichedConfig;
    const nextForm =
      (enrichedConfig.forms || []).find((f) => f.id === formId) || form;
    renderForm(enrichedConfig, nextForm);
    refreshOutputs(enrichedConfig, nextForm);
  });
}

// ── Bootstrap ──────────────────────────────────────────────────────────────────
document.addEventListener("DOMContentLoaded", async () => {
  initTheme();
  initKeyboardShortcuts();

  const basePath = document.body.dataset.basePath || ".";
  const isIndexPage = document.body.dataset.page === "index";

  try {
    const config = await loadConfig(basePath);
    if (isIndexPage) {
      initIndexPage(config);
    } else {
      await initFormPage(config, basePath);
    }
  } catch (err) {
    setStatus(`Failed to load config: ${err.message}`, "error");
    if (!isIndexPage) {
      wireLocalConfig(async (cfg) => {
        setStatus("Loaded from local file", "ok");
        await initFormPage(cfg, basePath);
      });
    }
    console.error("CopilotForms bootstrap error:", err);
  }
});

