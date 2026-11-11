/* MTM Agent Toolkit — nav.js
   Single source of truth for the chapter list, the sidebar, and the contents page.
   Rendering the navigation from data keeps seventeen pages identical and offline. */

window.MTK_NAV = [
  {
    slug: "spec-driven-development",
    title: "Spec-driven development",
    desc: "Spec Kit pipeline, its agents, and the companion board",
    count: 47
  },
  {
    slug: "dotnet-development",
    title: "C# and .NET development",
    desc: "Language server, refactoring, MSBuild, and framework guidance",
    count: 14
  },
  {
    slug: "testing",
    title: "Testing",
    desc: "Test generation, execution, coverage, and quality analysis",
    count: 19
  },
  {
    slug: "winui-platform",
    title: "WinUI 3 and the Windows platform",
    desc: "XAML patterns, design review, Windows API lookup, UI automation",
    count: 16
  },
  {
    slug: "packaging-signing",
    title: "Packaging, signing, and distribution",
    desc: "MSIX, certificates, package identity, manifests",
    count: 7
  },
  {
    slug: "database-sql",
    title: "Database and SQL",
    desc: "Stored procedures, query review, and ERP read-only discipline",
    count: 6
  },
  {
    slug: "code-quality",
    title: "Code quality and review",
    desc: "Review agents, assertion audits, anti-pattern detection",
    count: 7
  },
  {
    slug: "security-supply-chain",
    title: "Security and supply chain",
    desc: "Scanning alerts, dependency triage, prompt safety",
    count: 3
  },
  {
    slug: "documentation-diagrams",
    title: "Documentation and diagrams",
    desc: "Word output, draw.io diagrams, and documenter agents",
    count: 4
  },
  {
    slug: "context-planning",
    title: "Context, planning, and testability",
    desc: "Change planning, context maps, and static-dependency seams",
    count: 19
  },
  {
    slug: "mcp-and-environment",
    title: "MCP servers and environment",
    desc: "Serena, Playwright, Microsoft Learn, hooks, and the two language servers",
    count: 5
  },
  {
    slug: "repo-assets",
    title: "Repo skills, agents, and prompts",
    desc: "Everything in .github that is not plugin-provided",
    count: 122
  }
];

window.MTK_META = {
  title: "MTM Agent Toolkit",
  subtitle:
    "Every plugin, skill, agent, canvas, and MCP server available to this workspace, organised by the job you are trying to do.",
  generatedOn: "2026-11-11",
  plugins: 13,
  skills: 71,
  agents: 21,
  canvases: 4,
  mcpServers: 3,
  total: 269
};

(function () {
  "use strict";

  function prefix() {
    var path = window.location.pathname.replace(/\\/g, "/");
    return /\/chapters\/[^/]*$/.test(path) ? "../" : "";
  }

  function totalEntries() {
    return window.MTK_NAV.reduce(function (sum, item) {
      return sum + (item.count || 0);
    }, 0);
  }

  function renderSidebar() {
    var host = document.querySelector("[data-nav]");
    if (!host) {
      return;
    }
    var root = prefix();
    var chapters = window.MTK_NAV.map(function (item) {
      return (
        '<li><a href="' +
        root +
        "chapters/" +
        item.slug +
        '.html"><span>' +
        item.title +
        '</span><span class="nav-count">' +
        item.count +
        "</span></a></li>"
      );
    }).join("");

    host.innerHTML =
      '<a class="brand" href="' +
      root +
      'index.html">' +
      window.MTK_META.title +
      "</a>" +
      '<p class="brand-sub">Workspace tool reference</p>' +
      '<div class="search-wrap no-print">' +
      '<input class="search" type="search" placeholder="Search ' + totalEntries() + ' entries" ' +
      'aria-label="Search the book" data-search-input autocomplete="off">' +
      '<div class="search-results" data-search-results hidden></div>' +
      "</div>" +
      '<nav class="nav-group" aria-label="Chapters">' +
      '<p class="nav-label">Chapters</p>' +
      '<ul class="nav-list">' +
      chapters +
      "</ul>" +
      "</nav>" +
      '<nav class="nav-group" aria-label="Tools">' +
      '<p class="nav-label">Tools</p>' +
      '<ul class="nav-list">' +
      "<li><a href=" +
      '"' +
      root +
      'prompt-builder.html"><span>Prompt builder</span><span class="nav-count">' +
      (window.MTK_CATALOG && window.MTK_CATALOG.prompts ? window.MTK_CATALOG.prompts.length : "") +
      "</span></a></li>" +
      "<li><a href=" +
      '"' +
      root +
      'index-by-source.html"><span>Index by source</span><span class="nav-count">' +
      totalEntries() +
      "</span></a></li>" +
      "<li><a href=" +
      '"' +
      root +
      'decision-guide.html"><span>Which tool when</span></a></li>' +
      "<li><a href=" +
      '"' +
      root +
      'print.html"><span>Print the book</span></a></li>' +
      "</ul></nav>" +
      '<p class="nav-note">Generated ' +
      window.MTK_META.generatedOn +
      ". Renders offline. Run <code>check-drift.py</code> after a plugin update.</p>";
  }

  /* The contents page uses the same list, so the two can never disagree. */
  function renderContents() {
    var host = document.querySelector("[data-contents]");
    if (!host) {
      return;
    }
    host.innerHTML = window.MTK_NAV.map(function (item) {
      return (
        "<li><a href=\"chapters/" +
        item.slug +
        '.html"><span class="num"></span><span><span class="label">' +
        item.title +
        '</span><span class="desc">' +
        item.desc +
        '</span></span><span class="count">' +
        item.count +
        " entries</span></a></li>"
      );
    }).join("");
  }

  function renderMeta() {
    var host = document.querySelector("[data-cover-meta]");
    if (!host) {
      return;
    }
    var meta = window.MTK_META;
    host.innerHTML =
      "<div><dt>Plugins</dt><dd>" +
      meta.plugins +
      "</dd></div>" +
      "<div><dt>Skills</dt><dd>" +
      meta.skills +
      "<small>unique</small></dd></div>" +
      "<div><dt>Agents</dt><dd>" +
      meta.agents +
      "</dd></div>" +
      "<div><dt>Canvases</dt><dd>" +
      meta.canvases +
      "</dd></div>" +
      "<div><dt>MCP servers</dt><dd>" +
      meta.mcpServers +
      "</dd></div>" +
      "<div><dt>Entries documented</dt><dd>" +
      totalEntries() +
      "</dd></div>";
  }

  function init() {
    renderSidebar();
    renderContents();
    renderMeta();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
