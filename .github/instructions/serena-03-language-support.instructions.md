---
applyTo: "**"
description: >
  Serena language support — which languages are supported, C# Roslyn setup
  for the MTM project, and the JetBrains plugin alternative.
---

# Serena Language Support

Serena provides semantic code understanding through two alternative backends:

1. **Language Servers (LSP)** — free/open-source; the default backend
2. **Serena JetBrains Plugin** — leverages JetBrains IDE analysis (more powerful for complex projects)

Official docs: <https://oraios.github.io/serena/01-about/020_programming-languages.html>

The underlying LSP abstraction layer is powered by **multilspy**
(<https://github.com/microsoft/multilspy>), which handles communication with language servers.

---

## C# Support (MTM Primary Language)

### Roslyn Language Server (Default for C#)

Serena uses **Microsoft's Roslyn Language Server** for C# — the same engine used by
Visual Studio and the C# extension for VS Code.

**Requirements:**

- .NET 10 or higher — auto-installed by Serena if absent
- On Windows: `pwsh` (PowerShell 7+) — already present in the MTM environment
- Language identifier in `project.yml`: `csharp`

**MTM project.yml:**

```yaml
languages:
  - csharp
```

**Auto-downloaded:** The Roslyn Language Server is automatically downloaded from NuGet.org
when first used. No manual installation required.

**OmniSharp alternative:** Set `csharp_omnisharp` as the language identifier to use OmniSharp
instead of Roslyn. Roslyn is recommended for new projects.

### Roslyn Download Override (Advanced)

If your environment blocks NuGet.org access (e.g., air-gapped or corporate proxy),
you can override the download URL in `%USERPROFILE%\.serena\serena_config.yml`:

```yaml
ls_specific_settings:
  csharp:
    runtime_dependencies:
      - id: "CSharpLanguageServer"
        platform_id: "win-x64"
        url: "https://your-mirror.example.com/roslyn-language-server.win-x64.nupkg"
        package_version: "5.5.0-2.26078.4"
```

---

## JetBrains Plugin Alternative

The **Serena JetBrains Plugin** (<https://plugins.jetbrains.com/plugin/28946-serena/>) is a
more powerful alternative to language servers that leverages JetBrains IDE code analysis.

**Advantages over language servers:**

- External library indexing (NuGet packages fully indexed)
- No separate language server setup required
- Enhanced performance — faster tool execution
- Multi-language polyglot support (everything JetBrains IDEs support)
- Works with Rider (for C# development)

**When to use for MTM:**

- If you use **JetBrains Rider** as your primary IDE, the plugin is highly recommended
- Provides better analysis of NuGet dependencies and auto-generated code
- Rider and CLion are **unsupported** by the plugin (despite being JetBrains IDEs)

**Setup:** Install the plugin from the JetBrains Marketplace, then set in `project.yml`:

```yaml
language_backend: jetbrains
```

Or specify at launch:

```bash
serena start-mcp-server --language-backend JetBrains
```

---

## All Supported Languages

Languages with direct out-of-the-box support (partial list of most relevant):

| Language                  | Notes                                                      |
| ------------------------- | ---------------------------------------------------------- |
| **C#**                    | `csharp` (Roslyn, requires .NET 10+) or `csharp_omnisharp` |
| **TypeScript/JavaScript** | `typescript` (covers both)                                 |
| **Python**                | Built-in support                                           |
| **Java**                  | Built-in support                                           |
| **Go**                    | Requires `gopls` installed                                 |
| **Rust**                  | Requires `rustup`                                          |
| **Kotlin**                | Auto-downloads official Kotlin LS; requires Java 21+       |
| **PHP**                   | `php` (Intelephense) or `php_phpactor`                     |
| **Ruby**                  | `ruby` (ruby-lsp) or `ruby_solargraph`                     |
| **Swift**                 | Built-in support                                           |
| **YAML**                  | Built-in support                                           |
| **Bash**                  | Built-in support                                           |
| **Markdown**              | Must be explicitly enabled; useful for doc-heavy projects  |

For the full list of 40+ supported languages, see:
<https://oraios.github.io/serena/01-about/020_programming-languages.html>

---

## Adding Language Support for Polyglot Projects

For the MTM project mixing C# and SQL scripts, specify multiple languages:

```bash
serena project create --language csharp --language sql --name "MTM_Receiving_Application"
```

Or in `project.yml`:

```yaml
languages:
  - csharp
```

---

## Common Troubleshooting

### Language Server Not Starting

```powershell
# Check if .NET 10 is available
dotnet --version

# Restart Serena language server via dashboard
# Navigate to http://localhost:24282/dashboard
# OR instruct the LLM: "restart the C# language server"
```

### Slow Symbol Resolution

Run project indexing to pre-cache symbols:

```bash
uvx --from git+https://github.com/oraios/serena serena project index
```

### OmniSharp vs Roslyn Decision

Use **Roslyn** (default) for MTM — it is the current Microsoft-supported Language Server
for C# and provides better support for .NET 8 / C# 12 features.

Use `csharp_omnisharp` only if you encounter specific compatibility issues.
