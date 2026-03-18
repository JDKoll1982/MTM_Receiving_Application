const storagePrefix = "copilotforms:";

function $(id) {
    return document.getElementById(id);
}

function toTitleCase(value) {
    return value
        .split("-")
        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
        .join(" ");
}

function splitLines(value) {
    return value
        .split(/\r?\n/)
        .map((item) => item.trim())
        .filter(Boolean);
}

function escapeMarkdown(value) {
    return String(value ?? "").replace(/\|/g, "\\|");
}

function createOption(text, value) {
    const option = document.createElement("option");
    option.textContent = text;
    option.value = value;
    return option;
}

function createField(config, shared) {
    const wrapper = document.createElement("div");
    wrapper.className = "field";

    const label = document.createElement("label");
    label.htmlFor = config.id;
    label.textContent = config.label + (config.required ? " *" : "");
    wrapper.appendChild(label);

    let input;
    if (config.type === "textarea" || config.type === "list") {
        input = document.createElement("textarea");
        input.rows = config.rows || 5;
    } else if (config.type === "select") {
        input = document.createElement("select");
        input.appendChild(createOption(config.placeholder || "Select an option", ""));
        const options = Array.isArray(config.options)
            ? config.options
            : shared[config.optionSource] || [];
        for (const optionValue of options) {
            input.appendChild(createOption(optionValue, optionValue));
        }
    } else if (config.type === "checkbox") {
        input = document.createElement("input");
        input.type = "checkbox";
    } else {
        input = document.createElement("input");
        input.type = config.type || "text";
    }

    input.id = config.id;
    input.dataset.fieldId = config.id;
    input.dataset.fieldType = config.type || "text";
    input.placeholder = config.placeholder || "";
    if (config.required) {
        input.required = true;
    }

    wrapper.appendChild(input);

    if (config.helpText) {
        const help = document.createElement("p");
        help.className = "helper-text";
        help.textContent = config.helpText;
        wrapper.appendChild(help);
    }

    return wrapper;
}

function readFieldValue(element) {
    const type = element.dataset.fieldType;
    if (type === "checkbox") {
        return element.checked;
    }
    if (type === "list") {
        return splitLines(element.value);
    }
    return element.value.trim();
}

function writeFieldValue(element, value) {
    const type = element.dataset.fieldType;
    if (type === "checkbox") {
        element.checked = Boolean(value);
        return;
    }
    if (Array.isArray(value)) {
        element.value = value.join("\n");
        return;
    }
    element.value = value ?? "";
}

function buildMarkdown(form, feature, values) {
    const lines = [];
    lines.push(`# ${form.title} Export`);
    lines.push("");
    lines.push(`- Generated: ${new Date().toISOString()}`);
    lines.push(`- Form Id: ${form.id}`);
    lines.push(`- Feature Id: ${feature?.id || "custom"}`);
    lines.push(`- Feature Name: ${feature?.name || values.featureName || "Unspecified"}`);
    lines.push(`- Module: ${feature?.module || values.moduleName || "Unspecified"}`);
    lines.push(`- Prompt File: ${form.promptFile}`);
    lines.push(`- Instruction File: ${form.instructionFile}`);
    lines.push(`- Suggested Output Folder: ${form.outputFolder}`);
    lines.push("");

    if (feature) {
        lines.push("## Catalog Context");
        lines.push("");
        lines.push(`- Summary: ${feature.summary}`);
        lines.push(`- Owner: ${feature.owner}`);
        lines.push(`- Related Files: ${feature.relatedFiles.join(", ")}`);
        lines.push(`- Related Services: ${feature.relatedServices.join(", ")}`);
        lines.push("");
    }

    for (const group of form.fieldGroups) {
        lines.push(`## ${group.title}`);
        lines.push("");
        for (const field of group.fields) {
            const value = values[field.id];
            if (Array.isArray(value)) {
                lines.push(`### ${field.label}`);
                if (value.length === 0) {
                    lines.push("- None provided");
                } else {
                    for (const item of value) {
                        lines.push(`- ${escapeMarkdown(item)}`);
                    }
                }
                lines.push("");
                continue;
            }
            lines.push(`### ${field.label}`);
            lines.push(value ? String(value) : "Not provided");
            lines.push("");
        }
    }

    lines.push("## Machine Data");
    lines.push("");
    lines.push("```json");
    lines.push(JSON.stringify({
        formId: form.id,
        featureId: feature?.id || null,
        featureName: feature?.name || values.featureName || null,
        module: feature?.module || values.moduleName || null,
        values
    }, null, 2));
    lines.push("```");
    lines.push("");
    lines.push("## Copilot Execution Note");
    lines.push("");
    lines.push("Use the linked prompt file first. Treat this export as the source of truth unless the workspace code clearly contradicts it.");
    return lines.join("\n");
}

function buildJson(form, feature, values) {
    return JSON.stringify({
        generatedAt: new Date().toISOString(),
        formId: form.id,
        title: form.title,
        outputFolder: form.outputFolder,
        promptFile: form.promptFile,
        instructionFile: form.instructionFile,
        feature: feature || null,
        values
    }, null, 2);
}

function downloadText(filename, content) {
    const blob = new Blob([content], { type: "text/plain;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
}

async function copyText(content) {
    if (!navigator.clipboard) {
        return false;
    }
    await navigator.clipboard.writeText(content);
    return true;
}

async function loadConfig(basePath) {
    const response = await fetch(`${basePath}/data/copilot-forms.config.json`, { cache: "no-store" });
    if (!response.ok) {
        throw new Error(`Unable to load config: ${response.status}`);
    }
    return response.json();
}

function populateFeatureSummary(feature) {
    const summary = $("feature-summary");
    if (!summary) {
        return;
    }
    if (!feature) {
        summary.innerHTML = "<p class=\"muted\">Select a feature from the catalog or choose Custom / Not listed.</p>";
        return;
    }

    summary.innerHTML = `
    <div>
      <dt>Summary</dt>
      <dd>${feature.summary}</dd>
    </div>
    <div>
      <dt>Owner</dt>
      <dd>${feature.owner}</dd>
    </div>
    <div>
      <dt>Files</dt>
      <dd>${feature.relatedFiles.join(", ")}</dd>
    </div>
    <div>
      <dt>Services</dt>
      <dd>${feature.relatedServices.join(", ")}</dd>
    </div>`;
}

function renderGuideList(elementId, items, emptyText) {
    const element = $(elementId);
    if (!element) {
        return;
    }

    if (!items || items.length === 0) {
        element.innerHTML = `<li>${emptyText}</li>`;
        return;
    }

    element.innerHTML = items.map((item) => `<li>${item}</li>`).join("");
}

function saveDraft(formId) {
    const values = {};
    document.querySelectorAll("[data-field-id]").forEach((element) => {
        values[element.dataset.fieldId] = readFieldValue(element);
    });
    localStorage.setItem(storagePrefix + formId, JSON.stringify(values));
}

function loadDraft(formId) {
    try {
        const raw = localStorage.getItem(storagePrefix + formId);
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

function renderForm(config, form) {
    $("form-title").textContent = form.title;
    $("form-summary").textContent = form.summary;
    $("prompt-file-link").textContent = form.promptFile;
    $("prompt-file-link").href = `../../${form.promptFile}`;
    $("instruction-file-link").textContent = form.instructionFile;
    $("instruction-file-link").href = `../../${form.instructionFile}`;
    $("output-folder").textContent = form.outputFolder;
    renderGuideList("when-to-use-list", form.whenToUse, "Use this form when the request clearly matches this subject.");
    renderGuideList("before-you-start-list", form.beforeYouStart, "Gather the smallest amount of context needed before asking Copilot to act.");
    renderGuideList("end-user-tips-list", form.endUserTips, "Be concrete, use exact screen names, and state what success looks like.");

    const featureSelect = $("feature-select");
    featureSelect.innerHTML = "";
    featureSelect.appendChild(createOption("Custom / Not listed", ""));
    for (const feature of config.features) {
        featureSelect.appendChild(createOption(`${feature.module} - ${feature.name}`, feature.id));
    }

    const container = $("dynamic-fields");
    container.innerHTML = "";
    for (const group of form.fieldGroups) {
        const section = document.createElement("section");
        section.className = "field-group";

        const title = document.createElement("h3");
        title.textContent = group.title;
        section.appendChild(title);

        if (group.description) {
            const description = document.createElement("p");
            description.className = "helper-text";
            description.textContent = group.description;
            section.appendChild(description);
        }

        const grid = document.createElement("div");
        grid.className = group.columns === 2 ? "field-grid columns-2" : "field-grid";
        for (const field of group.fields) {
            grid.appendChild(createField(field, config.shared));
        }
        section.appendChild(grid);
        container.appendChild(section);
    }
}

function refreshOutputs(config, form) {
    const featureId = $("feature-select").value;
    const feature = config.features.find((item) => item.id === featureId) || null;
    populateFeatureSummary(feature);

    const values = {};
    document.querySelectorAll("[data-field-id]").forEach((element) => {
        values[element.dataset.fieldId] = readFieldValue(element);
    });
    values.selectedFeatureId = featureId || null;

    const markdown = buildMarkdown(form, feature, values);
    const json = buildJson(form, feature, values);
    $("markdown-output").value = markdown;
    $("json-output").value = json;
    saveDraft(form.id);
}

function applyDraft(formId) {
    const draft = loadDraft(formId);
    if (!draft) {
        return;
    }
    document.querySelectorAll("[data-field-id]").forEach((element) => {
        if (Object.prototype.hasOwnProperty.call(draft, element.dataset.fieldId)) {
            writeFieldValue(element, draft[element.dataset.fieldId]);
        }
    });
    if (draft.selectedFeatureId && $("feature-select")) {
        $("feature-select").value = draft.selectedFeatureId;
    }
}

async function initializeForm(config) {
    const formId = document.body.dataset.formId;
    const form = config.forms.find((item) => item.id === formId);
    if (!form) {
        throw new Error(`Unknown form id: ${formId}`);
    }

    renderForm(config, form);
    applyDraft(form.id);
    refreshOutputs(config, form);

    document.body.addEventListener("input", () => refreshOutputs(config, form));
    document.body.addEventListener("change", () => refreshOutputs(config, form));

    $("copy-markdown").addEventListener("click", async () => {
        await copyText($("markdown-output").value);
    });
    $("copy-json").addEventListener("click", async () => {
        await copyText($("json-output").value);
    });
    $("download-markdown").addEventListener("click", () => {
        downloadText(`${form.id}.export.md`, $("markdown-output").value);
    });
    $("download-json").addEventListener("click", () => {
        downloadText(`${form.id}.export.json`, $("json-output").value);
    });
    $("clear-draft").addEventListener("click", () => {
        localStorage.removeItem(storagePrefix + form.id);
        window.location.reload();
    });
}

function wireLocalConfigFallback() {
    const input = $("load-config-input");
    const button = $("load-config-button");
    if (!input || !button) {
        return;
    }

    button.addEventListener("click", () => input.click());
    input.addEventListener("change", async (event) => {
        const file = event.target.files?.[0];
        if (!file) {
            return;
        }
        const raw = await file.text();
        const config = JSON.parse(raw);
        $("status-message").textContent = `Loaded local config: ${file.name}`;
        $("status-box").classList.remove("warning");
        initializeForm(config).catch((error) => {
            $("status-message").textContent = error.message;
            $("status-box").classList.add("warning");
        });
    });
}

window.addEventListener("DOMContentLoaded", async () => {
    if (!document.body.dataset.formId) {
        return;
    }

    wireLocalConfigFallback();
    const basePath = document.body.dataset.basePath || ".";
    try {
        const config = await loadConfig(basePath);
        $("status-message").textContent = "Loaded shared config successfully.";
        await initializeForm(config);
    } catch (error) {
        $("status-message").textContent = `${error.message}. Use Load Local Config if you opened this file directly from disk.`;
        $("status-box").classList.add("warning");
    }
});
