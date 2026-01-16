# Playwright MCP — Instructions (Any Project)

## What it’s for

Browser automation: navigate pages, fill forms, upload files, inspect console logs.

## Install (VS Code `.vscode/mcp.json`)

```jsonc
{
  "servers": {
    "playwright": {
      "type": "stdio",
      "command": "npx",
      "args": ["@playwright/mcp@latest"]
    }
  }
}
```

## First-run setup

If you see a “browser not installed” error, run:

- `mcp_playwright_browser_install`

## Tools in this environment

- `mcp_playwright_browser_install`
- `mcp_playwright_browser_evaluate`
- `mcp_playwright_browser_drag`
- `mcp_playwright_browser_fill_form`
- `mcp_playwright_browser_file_upload`
- `mcp_playwright_browser_console_messages`

## Example prompts

- “Open <https://example.com> and extract the page title.”
- “Fill this form and submit, then report validation errors.”
- “Show console errors after clicking the login button.”
