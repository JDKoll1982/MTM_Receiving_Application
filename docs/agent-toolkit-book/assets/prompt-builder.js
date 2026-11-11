/* MTM Agent Toolkit — prompt-builder.js
   Composes a prompt from the curated catalog, the reader's context, and this
   repo's house rules. Offline: reads window.MTK_CATALOG, falling back to fetch. */

(function () {
  "use strict";

  var STORAGE_KEY = "mtk-prompt-builder-v1";

  var HOUSE_RULES = [
    "Layering is View → ViewModel → Service → DAO → Database. No view model may call a DAO directly.",
    "All business logic lives in the service layer, never in .xaml.cs code-behind.",
    "MySQL writes go through stored procedures only. No raw MySQL SQL in C#.",
    "Infor Visual SQL Server is read-only. Never issue INSERT, UPDATE, or DELETE against it.",
    "XAML uses x:Bind only. Runtime {Binding} is not permitted.",
    "DAOs return Model_Dao_Result or Model_Dao_Result<T> for expected failures instead of throwing.",
    "Async method names end in Async. Tests use xUnit and FluentAssertions with names of the form MethodName_ShouldResult_WhenCondition.",
    "When behaviour changes, update the matching docs under specs/, docs/, or the module Documentation folder."
  ];

  var DO_NOT = [
    "Do not invent types, files, or stored procedures that do not already exist. Ask instead.",
    "Do not change behaviour beyond what was asked, and do not do opportunistic cleanup.",
    "Do not add NuGet packages, change database schema, or change dependency injection wiring without asking first.",
    "Do not modify anything under Infor Visual or any SQL Server database.",
    "Do not remove or rewrite existing tests to make a build pass.",
    "Do not report work as complete unless you have built it and run the relevant tests."
  ];

  var PRESETS = [
    { name: "Ship a new feature", ids: ["spec-new-feature", "test-dao-unit", "qual-architecture-review"] },
    { name: "Cover a module with tests", ids: ["test-find-gaps", "test-dao-unit", "test-viewmodel-unit", "test-audit-existing"] },
    { name: "Fix a defect", ids: ["dbg-runtime-exception", "spec-bug-pipeline"] },
    { name: "Release to testers", ids: ["pkg-verify-toolchain", "pkg-version-bump", "pkg-msix-installer"] },
    { name: "Onboard someone", ids: ["onb-first-week", "onb-module-tour", "onb-handover-pack"] },
    { name: "Review before merging", ids: ["sec-review-diff", "qual-architecture-review", "test-audit-existing"] }
  ];

  /* Pairs that overlap. The message states the default and the escape condition,
     mirroring the book's decision guide so the two cannot disagree. */
  var CONFLICTS = [
    {
      a: "winui-packaging", b: "winapp-package",
      text: "Both package apps. Prefer winui-packaging for this WinUI 3 app; winapp-package wins only for a non-WinUI target."
    },
    {
      a: "winui-setup", b: "winapp-setup",
      text: "Both check tooling. winui-setup owns the WinUI prerequisite chain; winapp-setup is for SDK init or restore on other projects."
    },
    {
      a: "winui-ui-testing", b: "winapp-ui-automation",
      text: "Both drive the UI. Prefer winui-ui-testing here; winapp-ui-automation is the generic harness for WPF, WinForms, or Electron."
    },
    {
      a: "csharp-xunit", b: "csharp-nunit",
      text: "This repo is xUnit with FluentAssertions. Adding NUnit guidance to the same prompt will contradict it."
    },
    {
      a: "speckit.specify", b: "speckit.companion.auto",
      text: "speckit.specify is a single gated step; speckit.companion.auto runs the whole pipeline without gates. Choosing both is redundant."
    },
    {
      a: "serena", b: "dotnet-plugin-lsp",
      text: "Two C# language servers. Use Serena for symbol work and the dotnet plugin for diagnostics; asking for both doubles memory for no gain."
    }
  ];

  var state = {
    category: null,
    selected: [],
    fields: {}
  };

  var catalog = null;
  var toolIndex = {};

  /* --------------------------------------------------------------- helpers ---- */

  function el(selector, root) {
    return (root || document).querySelector(selector);
  }

  function els(selector, root) {
    return Array.prototype.slice.call((root || document).querySelectorAll(selector));
  }

  function escapeHtml(value) {
    return String(value == null ? "" : value)
      .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function promptById(id) {
    if (!catalog) {
      return null;
    }
    for (var i = 0; i < catalog.prompts.length; i++) {
      if (catalog.prompts[i].id === id) {
        return catalog.prompts[i];
      }
    }
    return null;
  }

  function categoryById(id) {
    if (!catalog) {
      return null;
    }
    for (var i = 0; i < catalog.categories.length; i++) {
      if (catalog.categories[i].id === id) {
        return catalog.categories[i];
      }
    }
    return null;
  }

  function toolLabel(name) {
    var record = toolIndex[name];
    if (!record) {
      return name;
    }
    return name;
  }

  function toolSource(name) {
    var record = toolIndex[name];
    return record ? record.source : "";
  }

  function toolKind(name) {
    var record = toolIndex[name];
    return record ? record.kind : "";
  }

  function load(callback) {
    if (window.MTK_CATALOG) {
      catalog = window.MTK_CATALOG;
      finish();
      return;
    }
    fetch("assets/prompt-catalog.json")
      .then(function (response) { return response.ok ? response.json() : null; })
      .then(function (data) { catalog = data; finish(); })
      .catch(function () { catalog = null; finish(); });

    function finish() {
      var index = window.MTK_INDEX || [];
      // Several kinds can share a name: speckit.specify exists as an agent, a
      // prompt, and a thin skill wrapper. Prefer the most actionable one so the
      // "uses" line names something a reader can actually invoke.
      var PRIORITY = { agent: 5, skill: 4, canvas: 3, mcp: 3, hook: 2, prompt: 1 };
      index.forEach(function (record) {
        var existing = toolIndex[record.name];
        if (!existing) {
          toolIndex[record.name] = record;
          return;
        }
        var score = PRIORITY[record.kind] || 0;
        var current = PRIORITY[existing.kind] || 0;
        if (score > current) {
          toolIndex[record.name] = record;
        }
      });
      callback();
    }
  }

  /* ---------------------------------------------------------------- render ---- */

  function renderCategories() {
    var host = el("[data-pb-categories]");
    if (!host || !catalog) {
      return;
    }
    var counts = {};
    catalog.prompts.forEach(function (prompt) {
      counts[prompt.category] = (counts[prompt.category] || 0) + 1;
    });

    host.innerHTML = catalog.categories.map(function (category) {
      var active = category.id === state.category ? " is-active" : "";
      return '<button type="button" class="pb-cat' + active + '" role="tab" ' +
        'aria-selected="' + (category.id === state.category) + '" data-pb-cat="' +
        escapeHtml(category.id) + '">' + escapeHtml(category.title) +
        '<span class="pb-cat-n">' + (counts[category.id] || 0) + "</span></button>";
    }).join("");

    var summary = el("[data-pb-category-summary]");
    var current = categoryById(state.category);
    if (summary) {
      summary.textContent = current ? current.summary : "";
    }
  }

  function renderPrompts() {
    var host = el("[data-pb-prompts]");
    if (!host || !catalog) {
      return;
    }
    var list = catalog.prompts.filter(function (prompt) {
      return prompt.category === state.category;
    });

    if (!list.length) {
      host.innerHTML = '<p class="pb-list-empty">No prompts in this category.</p>';
      return;
    }

    host.innerHTML = list.map(function (prompt) {
      var checked = state.selected.indexOf(prompt.id) !== -1 ? " checked" : "";
      var uses = prompt.uses.map(function (tool) {
        return '<span class="pb-use" title="' + escapeHtml(toolKind(tool) + " from " + toolSource(tool)) +
          '">' + escapeHtml(toolLabel(tool)) + "</span>";
      }).join("");
      return '<div class="pb-prompt">' +
        '<input type="checkbox" id="pb-' + escapeHtml(prompt.id) + '" data-pb-pick="' +
        escapeHtml(prompt.id) + '"' + checked + ">" +
        "<div>" +
        '<label class="pb-prompt-title" for="pb-' + escapeHtml(prompt.id) + '">' +
        escapeHtml(prompt.title) + "</label>" +
        '<span class="pb-prompt-summary">' + escapeHtml(prompt.summary) + "</span>" +
        (uses ? '<span class="pb-uses">' + uses + "</span>" : "") +
        "</div></div>";
    }).join("");
  }

  function renderSequence() {
    var host = el("[data-pb-sequence]");
    if (!host) {
      return;
    }
    if (!state.selected.length) {
      host.innerHTML = '<li class="pb-seq-empty">Nothing chosen yet. Pick jobs on the left; ' +
        "they are added here in the order you select them, and you can reorder them.</li>";
      updateCount();
      return;
    }

    host.innerHTML = state.selected.map(function (id, index) {
      var prompt = promptById(id);
      if (!prompt) {
        return "";
      }
      var uses = prompt.uses.map(function (tool) {
        return '<span class="pb-use">' + escapeHtml(toolLabel(tool)) + "</span>";
      }).join("");
      return '<li class="pb-seq-item">' +
        '<span class="pb-seq-n">' + (index + 1) + "</span>" +
        "<div>" +
        '<span class="pb-seq-title">' + escapeHtml(prompt.title) + "</span>" +
        '<span class="pb-seq-meta">' + escapeHtml(categoryTitle(prompt.category)) + "</span>" +
        (uses ? '<span class="pb-seq-tools">' + uses + "</span>" : "") +
        "</div>" +
        '<span class="pb-seq-btns">' +
        '<button type="button" class="pb-icon" data-pb-up="' + index + '" aria-label="Move up"' +
        (index === 0 ? " disabled" : "") + ">&#8593;</button>" +
        '<button type="button" class="pb-icon" data-pb-down="' + index + '" aria-label="Move down"' +
        (index === state.selected.length - 1 ? " disabled" : "") + ">&#8595;</button>" +
        '<button type="button" class="pb-icon" data-pb-remove="' + index + '" aria-label="Remove">&#215;</button>' +
        "</span></li>";
    }).join("");
    updateCount();
  }

  function categoryTitle(id) {
    var category = categoryById(id);
    return category ? category.title : id;
  }

  function updateCount() {
    var counter = el("[data-pb-selected-count]");
    if (counter) {
      counter.textContent = state.selected.length + (state.selected.length === 1 ? " selected" : " selected");
    }
  }

  function renderWarnings() {
    var host = el("[data-pb-warnings]");
    if (!host) {
      return;
    }
    var chosen = {};
    state.selected.forEach(function (id) {
      var prompt = promptById(id);
      if (prompt) {
        prompt.uses.forEach(function (tool) { chosen[tool] = true; });
      }
    });

    var messages = [];
    CONFLICTS.forEach(function (conflict) {
      if (chosen[conflict.a] && chosen[conflict.b]) {
        messages.push("<strong>" + escapeHtml(conflict.a) + " and " +
          escapeHtml(conflict.b) + " overlap.</strong> " + escapeHtml(conflict.text));
      }
    });

    if (!messages.length) {
      host.hidden = true;
      host.innerHTML = "";
      return;
    }
    host.hidden = false;
    host.innerHTML = messages.map(function (message) {
      return "<p>" + message + "</p>";
    }).join("");
  }

  /* -------------------------------------------------------------- compose ---- */

  function fieldValue(name) {
    var input = el('[data-pb-field="' + name + '"]');
    return input ? input.value.trim() : "";
  }

  function applyModule(text) {
    var module = fieldValue("module") || "Module_Receiving";
    return text.split("Module_Receiving").join(module);
  }

  function compose() {
    if (!state.selected.length) {
      return "";
    }

    var lines = [];
    var single = state.selected.length === 1;
    var module = fieldValue("module") || "Module_Receiving";

    lines.push("## Goal");
    lines.push("");
    if (single) {
      var only = promptById(state.selected[0]);
      lines.push(applyModule(only.summary));
    } else {
      lines.push("Complete the following work in " + module + ", in the order given. " +
        "Each phase has a result worth reviewing before the next one starts.");
    }
    lines.push("");

    var contextRows = [];
    if (module) { contextRows.push(["Module", module]); }
    if (fieldValue("dao")) { contextRows.push(["DAO or service", fieldValue("dao")]); }
    if (fieldValue("sp")) { contextRows.push(["Stored procedure", fieldValue("sp")]); }
    if (fieldValue("file")) { contextRows.push(["Target file", fieldValue("file")]); }
    if (fieldValue("test")) { contextRows.push(["Tests", fieldValue("test")]); }
    if (fieldValue("spec")) { contextRows.push(["Spec folder", fieldValue("spec")]); }

    if (contextRows.length || fieldValue("acceptance") || fieldValue("extra")) {
      lines.push("## Context");
      lines.push("");
      contextRows.forEach(function (row) {
        lines.push("- " + row[0] + ": " + row[1]);
      });
      if (fieldValue("acceptance")) {
        lines.push("- Acceptance criteria: " + fieldValue("acceptance"));
      }
      if (fieldValue("extra")) {
        lines.push("- Additional context: " + fieldValue("extra"));
      }
      lines.push("");
    }

    var allTools = [];
    state.selected.forEach(function (id) {
      var prompt = promptById(id);
      prompt.uses.forEach(function (tool) {
        if (allTools.indexOf(tool) === -1) {
          allTools.push(tool);
        }
      });
    });

    if (allTools.length) {
      lines.push("## Tools to use");
      lines.push("");
      allTools.forEach(function (tool) {
        var kind = toolKind(tool);
        var source = toolSource(tool);
        var suffix = [kind, source].filter(Boolean).join(", ");
        lines.push("- `" + tool + "`" + (suffix ? " — " + suffix : ""));
      });
      lines.push("");
    }

    lines.push(single ? "## Steps" : "## Steps, in phases");
    lines.push("");

    state.selected.forEach(function (id, index) {
      var prompt = promptById(id);
      if (!single) {
        lines.push("### Phase " + (index + 1) + " — " + prompt.title);
        lines.push("");
      }
      prompt.steps.forEach(function (step, stepIndex) {
        lines.push((stepIndex + 1) + ". " + applyModule(step));
      });
      lines.push("");
      if (!single && index < state.selected.length - 1) {
        lines.push("⟶ Stop after phase " + (index + 1) + " and tell me the result. " +
          "Wait for my confirmation before starting phase " + (index + 2) + ".");
        lines.push("");
      }
    });

    lines.push("## House rules for this repo");
    lines.push("");
    HOUSE_RULES.forEach(function (rule) { lines.push("- " + rule); });
    lines.push("");

    if (isOn("done")) {
      lines.push("## Done when");
      lines.push("");
      lines.push("- The solution builds: `dotnet build MTM_Receiving_Application.slnx`.");
      lines.push("- The relevant tests pass: `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj`.");
      lines.push("- No view model calls a DAO, and no business logic sits in code-behind.");
      lines.push("- Any new MySQL write goes through a stored procedure, and Infor Visual is untouched.");
      lines.push("- Affected documentation or spec files are updated.");
      lines.push("- You have told me what you changed and what you verified.");
      lines.push("");
    }

    if (isOn("dont")) {
      lines.push("## Do not");
      lines.push("");
      DO_NOT.forEach(function (rule) { lines.push("- " + rule); });
      lines.push("");
    }

    return lines.join("\n").replace(/\n{3,}/g, "\n\n").trim() + "\n";
  }

  function isOn(name) {
    var box = el('[data-pb-opt="' + name + '"]');
    return box ? box.checked : true;
  }

  function renderOutput() {
    var out = el("[data-pb-output]");
    var empty = el("[data-pb-empty]");
    var size = el("[data-pb-size]");
    var text = compose();

    if (out) {
      out.textContent = text;
    }
    if (empty) {
      empty.hidden = !!text;
    }
    if (size) {
      size.textContent = text ? text.length + " characters" : "";
    }
  }

  function renderAll() {
    renderCategories();
    renderPrompts();
    renderSequence();
    renderWarnings();
    renderOutput();
  }

  /* ---------------------------------------------------------------- state ---- */

  function save() {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch (error) {
      /* Private browsing or file:// restrictions: state simply is not remembered. */
    }
  }

  function restore() {
    try {
      var raw = window.localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return;
      }
      var parsed = JSON.parse(raw);
      if (parsed && Array.isArray(parsed.selected)) {
        state.selected = parsed.selected.filter(function (id) { return !!promptById(id); });
      }
      if (parsed && parsed.fields && typeof parsed.fields === "object") {
        Object.keys(parsed.fields).forEach(function (name) {
          var input = el('[data-pb-field="' + name + '"]');
          if (input && typeof parsed.fields[name] === "string") {
            input.value = parsed.fields[name];
          }
        });
      }
      if (parsed && parsed.category && categoryById(parsed.category)) {
        state.category = parsed.category;
      }
    } catch (error) {
      /* Ignore unusable stored state rather than failing the page. */
    }
  }

  function captureFields() {
    state.fields = {};
    els("[data-pb-field]").forEach(function (input) {
      state.fields[input.getAttribute("data-pb-field")] = input.value;
    });
  }

  /* ----------------------------------------------------------------- wire ---- */

  function wirePresets() {
    var host = el("[data-pb-presets]");
    if (!host) {
      return;
    }
    host.innerHTML = PRESETS.map(function (preset, index) {
      return '<button type="button" class="pb-preset" data-pb-preset="' + index + '">' +
        escapeHtml(preset.name) + "</button>";
    }).join("");
  }

  function wireOptions() {
    /* Options live in the output panel header so they sit next to what they change. */
    var host = el(".pb-output .pb-panel-head");
    if (!host || el("[data-pb-opt]")) {
      return;
    }
    var box = document.createElement("div");
    box.className = "pb-output-actions";
    box.innerHTML =
      '<label class="pb-opt"><input type="checkbox" data-pb-opt="done" checked> Done when</label>' +
      '<label class="pb-opt"><input type="checkbox" data-pb-opt="dont" checked> Do not</label>';
    host.appendChild(box);
  }

  function loadFacts(callback) {
    if (window.MTK_FACTS) {
      populateFacts(window.MTK_FACTS);
      callback();
      return;
    }
    fetch("assets/facts.js")
      .then(function (response) { return response.text(); })
      .then(function (text) {
        // Executing a generated local file is unnecessary: read the JSON payload.
        var start = text.indexOf("{");
        var end = text.lastIndexOf("}");
        var facts = start >= 0 ? JSON.parse(text.slice(start, end + 1)) : {};
        populateFacts(facts);
        callback();
      })
      .catch(function () {
        // Suggestions are a convenience; without them the fields still work.
        callback();
      });
  }

  function populateFacts(facts) {
    // A datalist filters by whatever is already typed, so an exact match hides
    // every other option. The module set is small and fixed, so it is a real
    // select where the whole list is always one click away.
    var select = el("[data-pb-select]");
    if (select) {
      var modules = facts[select.getAttribute("data-pb-select")] || [];
      var current = select.value || "Module_Receiving";
      if (modules.length) {
        select.innerHTML = modules.map(function (value) {
          return '<option value="' + escapeHtml(value) + '">' + escapeHtml(value) + "</option>";
        }).join("");
        if (modules.indexOf(current) !== -1) {
          select.value = current;
        } else if (current) {
          select.insertAdjacentHTML("afterbegin",
            '<option value="' + escapeHtml(current) + '">' + escapeHtml(current) + "</option>");
          select.value = current;
        }
      }
    }

    els("[data-pb-list]").forEach(function (list) {
      var key = list.getAttribute("data-pb-list");
      var values = facts[key] || [];
      list.innerHTML = values.map(function (value) {
        return '<option value="' + escapeHtml(value) + '"></option>';
      }).join("");
    });

    var counts = els("[data-pb-list]").map(function (list) {
      return (facts[list.getAttribute("data-pb-list")] || []).length;
    });
    var moduleCount = (facts.module || []).length;
    var total = counts.reduce(function (sum, n) { return sum + n; }, 0) + moduleCount;
    var hint = el(".pb-context .pb-hint");
    if (hint && total && !hint.getAttribute("data-facts-annotated")) {
      hint.setAttribute("data-facts-annotated", "1");
      hint.insertAdjacentHTML("afterbegin",
        "<strong>" + total + " suggestions</strong> parsed from the repository. ");
    }
  }

  function init() {
    // facts.js is a plain script tag, so the suggestions are already available
    // here. Populate before restoring state, otherwise a saved module value has
    // no matching option yet and would be dropped.
    if (window.MTK_FACTS) {
      populateFacts(window.MTK_FACTS);
    }

    load(function () {
      if (!catalog) {
        var out = el("[data-pb-output]");
        if (out) {
          out.textContent = "The prompt catalog could not be loaded. " +
            "Open this page through a local web server, or regenerate assets/prompt-catalog.js " +
            "with build-pages.py.";
        }
        return;
      }

      state.category = catalog.categories[0].id;
      restore();
      if (!state.category) {
        state.category = catalog.categories[0].id;
      }

      wirePresets();
      wireOptions();
      renderAll();

      loadFacts(function () { /* suggestions are additive; nothing to re-render */ });

      document.addEventListener("change", function (event) {
        var pick = event.target.closest("[data-pb-pick]");
        if (pick) {
          var id = pick.getAttribute("data-pb-pick");
          var at = state.selected.indexOf(id);
          if (pick.checked && at === -1) {
            state.selected.push(id);
          } else if (!pick.checked && at !== -1) {
            state.selected.splice(at, 1);
          }
          renderSequence();
          renderWarnings();
          renderOutput();
          save();
          return;
        }
        if (event.target.matches("[data-pb-opt]")) {
          renderOutput();
        }
      });

      document.addEventListener("click", function (event) {
        var preset = event.target.closest("[data-pb-preset]");
        if (preset) {
          var chosen = PRESETS[Number(preset.getAttribute("data-pb-preset"))];
          state.selected = chosen.ids.filter(function (id) { return !!promptById(id); });
          renderAll();
          save();
          return;
        }

        var cat = event.target.closest("[data-pb-cat]");
        if (cat) {
          state.category = cat.getAttribute("data-pb-cat");
          renderAll();
          save();
          return;
        }

        var up = event.target.closest("[data-pb-up]");
        if (up) {
          var u = Number(up.getAttribute("data-pb-up"));
          if (u > 0) {
            var moved = state.selected.splice(u, 1)[0];
            state.selected.splice(u - 1, 0, moved);
            renderSequence();
            renderOutput();
            save();
          }
          return;
        }

        var down = event.target.closest("[data-pb-down]");
        if (down) {
          var d = Number(down.getAttribute("data-pb-down"));
          if (d < state.selected.length - 1) {
            var movedDown = state.selected.splice(d, 1)[0];
            state.selected.splice(d + 1, 0, movedDown);
            renderSequence();
            renderOutput();
            save();
          }
          return;
        }

        var remove = event.target.closest("[data-pb-remove]");
        if (remove) {
          state.selected.splice(Number(remove.getAttribute("data-pb-remove")), 1);
          renderAll();
          save();
          return;
        }

        if (event.target.closest("[data-pb-clear]")) {
          state.selected = [];
          renderAll();
          save();
          return;
        }

        if (event.target.closest("[data-pb-copy]")) {
          copyOutput(event.target.closest("[data-pb-copy]"));
        }
      });

      els("[data-pb-field]").forEach(function (input) {
        input.addEventListener("input", function () {
          renderOutput();
          captureFields();
          save();
        });
      });
    });
  }

  function copyOutput(button) {
    var out = el("[data-pb-output]");
    if (!out) {
      return;
    }
    var text = out.textContent;
    var original = button.textContent;

    function done() {
      button.textContent = "Copied";
      window.setTimeout(function () { button.textContent = original; }, 1600);
    }

    function fallback() {
      var area = document.createElement("textarea");
      area.value = text;
      area.setAttribute("readonly", "");
      area.style.position = "fixed";
      area.style.opacity = "0";
      document.body.appendChild(area);
      area.select();
      var ok = false;
      try {
        ok = document.execCommand("copy");
      } catch (error) {
        ok = false;
      }
      document.body.removeChild(area);
      if (ok) {
        done();
      } else {
        var range = document.createRange();
        range.selectNodeContents(out);
        var selection = window.getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
        button.textContent = "Press Ctrl+C";
      }
    }

    if (!text) {
      return;
    }
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard.writeText(text).then(done, fallback);
    } else {
      fallback();
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
