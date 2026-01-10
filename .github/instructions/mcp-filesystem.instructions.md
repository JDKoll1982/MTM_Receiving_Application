# Filesystem MCP — Instructions (Any Project)

## What it’s for
Safe, scoped file operations: read/write/edit/search/list within allowed directories.

## Install (NPX)
```jsonc
{
  "servers": {
    "filesystem": {
      "type": "stdio",
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-filesystem",
        "${workspaceFolder}"
      ]
    }
  }
}
```

## Windows note (Roots URI warnings)
Some setups may produce warnings when VS Code sends roots as `file:///c%3A/...`.
If your server supports only strict file URI parsing, prefer passing allowed directories via args (as above).

## Tools in this environment
- `mcp_filesystem_list_allowed_directories`
- `mcp_filesystem_read_text_file`
- `mcp_filesystem_read_file` (deprecated)
- `mcp_filesystem_read_multiple_files`
- `mcp_filesystem_read_media_file`
- `mcp_filesystem_list_directory`
- `mcp_filesystem_list_directory_with_sizes`
- `mcp_filesystem_directory_tree`
- `mcp_filesystem_search_files`
- `mcp_filesystem_get_file_info`
- `mcp_filesystem_create_directory`
- `mcp_filesystem_write_file`
- `mcp_filesystem_edit_file`
- `mcp_filesystem_move_file`

## Example prompts
- “List allowed directories and confirm sandboxing.”
- “Search for all `*.csproj` files.”
- “Create `docs/` and write `mcp.md`.”
