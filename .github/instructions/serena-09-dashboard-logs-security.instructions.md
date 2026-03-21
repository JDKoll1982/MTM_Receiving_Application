---
applyTo: "**"
description: >
  Serena dashboard and GUI tool overview, accessing and reading logs,
  and security considerations for using Serena safely in the MTM project.
---

# Serena Dashboard, Logs, and Security

---

## Dashboard

Official docs: <https://oraios.github.io/serena/02-usage/060_dashboard.html>

Serena includes a **web-based dashboard** (enabled by default) for monitoring and managing
the current session.

### Accessing the Dashboard

When Serena is running:

```
http://localhost:24282/dashboard/index.html
```

A higher port (24283, 24284, etc.) is used if 24282 is unavailable.

**Via AI:** Ask the AI to open it:

```
"Open the Serena dashboard"
```

### Dashboard Features

| Tab               | What You Can See / Do                                                |
| ----------------- | -------------------------------------------------------------------- |
| **Overview**      | Active project, context, modes, language server status               |
| **Configuration** | Current global and project settings; toggle active languages         |
| **Tool Calls**    | Live feed of every tool call the AI made, with arguments and results |
| **Logs**          | Live log stream with level filtering                                 |
| **Memories**      | Browse, create, edit, and delete memories for the active project     |

**Recommended during MTM development:** Keep the Tool Calls tab open in a browser window
to see exactly what Serena is reading and editing in real time.

### Disable Auto-Open

If the browser opening automatically is disruptive:

```yaml
# serena_config.yml
web_dashboard_open_on_launch: false
```

Or at startup:

```bash
serena start-mcp-server --open-web-dashboard false
```

The dashboard still runs at `http://localhost:24282` — you can open it manually when needed.

---

### GUI Tool (Windows Native)

Serena also provides a native Windows GUI tool (disabled by default) that shows a log
viewer window and lets you:

- Shut down the Serena agent
- Launch the dashboard URL from a button

Enable in `serena_config.yml`:

```yaml
# gui_tool: true   # Uncomment to enable
```

Primarily supported on Windows. macOS is unsupported.

---

## Logs

Official docs: <https://oraios.github.io/serena/02-usage/065_logs.html>

### Accessing Logs

**Via Dashboard:** Navigate to `http://localhost:24282/dashboard` → **Logs** tab.

**Via GUI Tool:** If enabled, the native window shows live logs.

**Persisted Log Files (Windows):**

```
%USERPROFILE%\.serena\logs\
```

### Log Levels

| Level     | Use                                        |
| --------- | ------------------------------------------ |
| `INFO`    | Standard operational messages (default)    |
| `DEBUG`   | Detailed tool arguments, LSP communication |
| `WARNING` | Non-fatal issues                           |
| `ERROR`   | Failures requiring attention               |

Configure in `serena_config.yml`:

```yaml
log_level: INFO # Change to DEBUG for troubleshooting
```

### LSP Tracing (Advanced)

Enable full language server communication logging for debugging Roslyn symbol issues:

```yaml
# serena_config.yml
enable_lsp_tracing: true # Very verbose; only for troubleshooting
```

### Common Log Patterns to Watch For

| Log Entry                    | Meaning                                       |
| ---------------------------- | --------------------------------------------- |
| `Language server started`    | Roslyn is ready                               |
| `Indexed N symbols`          | Project index complete                        |
| `Memory read: ...`           | Serena is drawing on stored knowledge         |
| `Symbol not found: ...`      | Check name_path_pattern spelling              |
| `Language server restart`    | LSP crashed; Serena is recovering             |
| `Tool call: execute_command` | Serena is running a shell command — review it |

---

## Security Considerations

Official docs: <https://oraios.github.io/serena/02-usage/070_security.html>

Serena has powerful tools: file modification and shell command execution. Apply these
safeguards for safe MTM development.

### Fundamental Risk

```
Serena can:
  ✔ Read any file in the project
  ✔ Write/delete any file
  ✔ Execute shell commands (dotnet build, git, etc.)

Mitigation is YOUR responsibility as the developer.
```

### Recommended Safeguards

#### 1. Use Git for Every Session

Always start from a committed state:

```bash
git status              # Ensure clean working tree
git commit -am "checkpoint before Serena session"
```

Review changes before accepting:

```bash
git diff               # What did Serena change?
git diff --stat        # Which files changed?
```

If something looks wrong:

```bash
git checkout -- .      # Discard all unstaged changes
git reset --hard HEAD  # Nuclear option
```

#### 2. Monitor Tool Executions

Use the dashboard's **Tool Calls** tab or your MCP client's tool approval features to
review every call the AI makes before it executes.

In VSCode GitHub Copilot, there is a "tool call confirmation" dialog. **Always read it.**

#### 3. Enable Read-Only Mode for Analysis Tasks

If you only need Serena to analyze code (no edits), set:

```yaml
# .serena/project.yml
read_only: true
```

Or per session:

```bash
serena start-mcp-server --mode read-only
```

#### 4. Disable Dangerous Tools per Project

Disable the shell execution tool if you don't need it:

```yaml
# .serena/project.yml
excluded_tools:
  - execute_command
```

#### 5. Use Docker for Maximum Isolation

For sensitive operations where you want to prevent any host filesystem access outside
the project:

```bash
docker run -it --rm \
  -v "C:\Users\johnk\source\repos\MTM_Receiving_Application:/workspace:rw" \
  ghcr.io/oraios/serena:latest \
  start-mcp-server --project /workspace
```

### MTM-Specific Security Notes

- **SQL Server is READ ONLY** — Serena cannot directly cause write operations to Infor Visual;
  the connection string enforces `ApplicationIntent=ReadOnly`
- **MySQL credentials** are in `appsettings.json` / environment variables — never hardcoded
  in source files; Serena will not find them to leak them
- **Secrets** in `appsettings.Development.json` are gitignored — Serena will not check this
  into source control
- **Review Serena's generated SQL** in stored procedure files before deploying to MySQL

### Shell Command Transparency

When Serena calls `execute_command`, the command and its arguments are visible in:

1. The dashboard Tool Calls tab
2. The MCP client's tool approval dialog
3. The log file

**Never approve a command you don't understand.** Common safe commands:

- `dotnet build` — safe
- `dotnet test` — safe
- `git diff` — safe (read-only)
- `git status` — safe
- `git commit` — safe **if you've reviewed the diff first**

Treat these with caution:

- `git push` — review carefully
- `git reset --hard` — destructive
- `rm` / `del` / file deletions — review carefully
