/* MTM Agent Toolkit — book.js
   Navigation state, offline search, copy buttons, and index filtering.
   Works from file:// with no network: search-index.json is attempted first,
   and window.MTK_INDEX (assets/search-index.js) is the offline fallback. */

(function () {
  "use strict";

  var INDEX_URL = "../assets/search-index.json";

  function rootPrefix() {
    // Pages live either at the book root or one level down in chapters/.
    var path = window.location.pathname.replace(/\\/g, "/");
    return /\/chapters\/[^/]*$/.test(path) ? "../" : "";
  }

  /* ------------------------------------------------------------ sidebar ---- */

  function markCurrentPage() {
    var path = window.location.pathname.replace(/\\/g, "/");
    var current = path.split("/").pop() || "index.html";
    var links = document.querySelectorAll(".nav-list a");
    for (var i = 0; i < links.length; i++) {
      var href = (links[i].getAttribute("href") || "").split("/").pop();
      if (href && href === current) {
        links[i].classList.add("is-current");
        links[i].setAttribute("aria-current", "page");
      }
    }
  }

  /* ------------------------------------------------------------- search ---- */

  var index = null;
  var indexPromise = null;

  function loadIndex() {
    if (indexPromise) {
      return indexPromise;
    }
    indexPromise = new Promise(function (resolve) {
      if (window.MTK_INDEX) {
        index = window.MTK_INDEX;
        resolve(index);
        return;
      }
      var url = rootPrefix() + "assets/search-index.json";
      fetch(url)
        .then(function (response) {
          return response.ok ? response.json() : [];
        })
        .then(function (data) {
          index = Array.isArray(data) ? data : [];
          resolve(index);
        })
        .catch(function () {
          index = [];
          resolve(index);
        });
    });
    return indexPromise;
  }

  function escapeHtml(value) {
    return String(value == null ? "" : value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function score(entry, terms) {
    var name = (entry.name || "").toLowerCase();
    var source = (entry.source || "").toLowerCase();
    var chapter = (entry.chapter || "").toLowerCase();
    var description = (entry.description || "").toLowerCase();
    var keywords = (entry.keywords || "").toLowerCase();
    var kind = entry.kind || "";
    var flat = terms.join(" ");
    var haystack = name + " " + keywords + " " + source + " " + chapter + " " + description;

    // A multi-word query must match every word, otherwise "ui automation"
    // matches anything mentioning "ui" and precision collapses.
    for (var t = 0; t < terms.length; t++) {
      if (terms[t] && haystack.indexOf(terms[t]) === -1) {
        return 0;
      }
    }

    var total = 0;
    if (name === flat) {
      total += 120;
    } else if (name.indexOf(flat) === 0) {
      total += 90;
    } else if (name.indexOf(flat) !== -1) {
      total += 70;
    }
    if (entry.primary) {
      total += 40;
    }

    for (var i = 0; i < terms.length; i++) {
      var term = terms[i];
      if (!term) {
        continue;
      }
      if (name === term) {
        total += 60;
      } else if (name.indexOf(term) === 0) {
        total += 45;
      } else if (name.indexOf(term) !== -1) {
        total += 34;
      }
      if (keywords.indexOf(term) !== -1) {
        total += 30;
      }
      if (source.indexOf(term) !== -1) {
        total += 18;
      }
      if (chapter.indexOf(term) !== -1) {
        total += 16;
      }
      if (description.indexOf(term) !== -1) {
        total += 12;
      }
    }

    // Generic prompt files are numerous; keep them behind real tools.
    if (kind === "prompt") {
      total -= 22;
    } else if (kind === "skill" || kind === "agent" || kind === "canvas") {
      total += 10;
    }
    return total;
  }

  function wireSearch() {
    var input = document.querySelector("[data-search-input]");
    var panel = document.querySelector("[data-search-results]");
    if (!input || !panel) {
      return;
    }

    function close() {
      panel.hidden = true;
      panel.innerHTML = "";
    }

    function render(terms) {
      loadIndex().then(function (entries) {
        var hits = [];
        for (var i = 0; i < entries.length; i++) {
          var value = score(entries[i], terms);
          if (value > 0) {
            hits.push({ entry: entries[i], value: value });
          }
        }
        hits.sort(function (a, b) {
          if (b.value !== a.value) {
            return b.value - a.value;
          }
          return (a.entry.name || "").localeCompare(b.entry.name || "");
        });
        hits = hits.slice(0, 24);

        var prefix = rootPrefix();
        var chapterHits = matchChapters(terms).map(function (chapter) {
          return (
            '<a class="search-hit" href="' +
            prefix +
            "chapters/" +
            chapter.slug +
            '.html"><span class="hit-name">' +
            escapeHtml(chapter.title) +
            '</span><span class="hit-meta">chapter &middot; ' +
            chapter.count +
            " entries</span></a>"
          );
        }).join("");

        if (!hits.length && !chapterHits) {
          panel.innerHTML =
            '<p class="search-empty">No entry matches &ldquo;' +
            escapeHtml(terms.join(" ")) +
            "&rdquo;. Try a plugin name, a skill name, or a job word such as testing.</p>";
          panel.hidden = false;
          return;
        }

        var html = chapterHits + hits
          .map(function (hit) {
            var entry = hit.entry;
            var chapterPath =
              prefix + "chapters/" + escapeHtml(entry.chapter || "") + ".html";
            return (
              '<a class="search-hit" href="' +
              chapterPath +
              "#" +
              escapeHtml(entry.anchor || "") +
              '"><span class="hit-name">' +
              escapeHtml(entry.name) +
              '</span><span class="hit-meta">' +
              escapeHtml(entry.kind || "") +
              " &middot; " +
              escapeHtml(entry.source || "") +
              "</span></a>"
            );
          })
          .join("");

        panel.innerHTML = html;
        panel.hidden = false;
      });
    }

    /* A job word should reach the chapter that answers it, not only entries. */
    function matchChapters(terms) {
      var nav = window.MTK_NAV || [];
      var query = terms.join(" ").toLowerCase();
      var matched = [];
      for (var i = 0; i < nav.length; i++) {
        var title = (nav[i].title || "").toLowerCase();
        if (title.indexOf(query) !== -1 || query.indexOf(title) !== -1) {
          matched.push(nav[i]);
        }
      }
      return matched.slice(0, 2);
    }

    input.addEventListener("input", function () {
      var query = input.value.trim().toLowerCase();
      if (query.length < 2) {
        close();
        return;
      }
      render(query.split(/\s+/));
    });

    input.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        input.value = "";
        close();
        input.blur();
      }
    });

    document.addEventListener("click", function (event) {
      if (!panel.contains(event.target) && event.target !== input) {
        close();
      }
    });
  }

  /* ------------------------------------------------------- copy buttons ---- */

  function wireCopyButtons() {
    var blocks = document.querySelectorAll(".entry-prompt");
    for (var i = 0; i < blocks.length; i++) {
      (function (block) {
        if (block.getAttribute("data-copy-wired") === "1") {
          return;
        }
        var code = block.querySelector("code");
        if (!code) {
          return;
        }
        // The button is authored into the HTML so it renders even if this
        // script never runs; here we only attach behaviour to it.
        var button = block.querySelector(".copy-btn");
        if (!button) {
          button = document.createElement("button");
          button.type = "button";
          button.className = "copy-btn";
          button.setAttribute("data-copy", "");
          button.textContent = "Copy";
          block.insertBefore(button, block.firstChild);
        }
        block.setAttribute("data-copy-wired", "1");

        button.addEventListener("click", function () {
          var text = code.innerText;
          function done() {
            button.textContent = "Copied";
            button.classList.add("is-copied");
            window.setTimeout(function () {
              button.textContent = "Copy";
              button.classList.remove("is-copied");
            }, 1600);
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
              // Last resort: surface the text selected so Ctrl+C works.
              var range = document.createRange();
              range.selectNodeContents(code);
              var selection = window.getSelection();
              selection.removeAllRanges();
              selection.addRange(range);
              button.textContent = "Press Ctrl+C";
            }
          }
          if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).then(done, fallback);
          } else {
            fallback();
          }
        });
      })(blocks[i]);
    }
  }

  /* ----------------------------------------------------- index filtering ---- */

  function wireIndexFilter() {
    var input = document.querySelector("[data-index-filter]");
    if (!input) {
      return;
    }
    var counter = document.querySelector("[data-index-count]");
    var rows = document.querySelectorAll("[data-index-row]");
    var groups = document.querySelectorAll("[data-source-group]");

    function apply() {
      var query = input.value.trim().toLowerCase();
      var shown = 0;
      for (var i = 0; i < rows.length; i++) {
        var haystack = rows[i].getAttribute("data-index-row") || "";
        var match = !query || haystack.indexOf(query) !== -1;
        rows[i].style.display = match ? "" : "none";
        if (match) {
          shown++;
        }
      }
      for (var g = 0; g < groups.length; g++) {
        var visible = groups[g].querySelector(
          '[data-index-row]:not([style*="display: none"])'
        );
        groups[g].style.display = visible ? "" : "none";
      }
      if (counter) {
        counter.textContent =
          shown + " of " + rows.length + (query ? " entries" : " entries");
      }
    }

    input.addEventListener("input", apply);
    apply();
  }

  /* ------------------------------------------------------ target arrival ---- */

  function noteTarget() {
    if (!window.location.hash) {
      return;
    }
    var target = document.getElementById(window.location.hash.slice(1));
    if (!target) {
      return;
    }
    target.setAttribute("tabindex", "-1");
    window.setTimeout(function () {
      target.focus({ preventScroll: true });
    }, 60);
  }

  function init() {
    markCurrentPage();
    wireSearch();
    wireCopyButtons();
    wireIndexFilter();
    noteTarget();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
