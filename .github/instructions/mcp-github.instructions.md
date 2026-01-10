# GitHub MCP — Instructions (Any Project)

This covers both:
- **Remote GitHub MCP** (hosted): easiest
- **Local GitHub MCP via Docker**: maximum control

## Remote GitHub MCP (recommended)
```jsonc
{
  "servers": {
    "githubRemote": {
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/"
    }
  }
}
```

## Local GitHub MCP via Docker (PAT required)
```jsonc
{
  "inputs": [
    {
      "type": "promptString",
      "id": "github_token",
      "description": "GitHub Personal Access Token",
      "password": true
    }
  ],
  "servers": {
    "githubLocal": {
      "type": "stdio",
      "command": "docker",
      "args": [
        "run", "-i", "--rm",
        "-e", "GITHUB_PERSONAL_ACCESS_TOKEN",
        "ghcr.io/github/github-mcp-server"
      ],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "${input:github_token}"
      }
    }
  }
}
```

## Safe defaults
- Use fine-grained PATs, short expirations
- Prefer read-only permissions unless automation is required

## Common use cases
- Repo browsing, file retrieval, history/commit audit
- Issue/PR automation
- Assigning Copilot to issues

## Tools in this environment
Local:
- `mcp_githublocal_get_file_contents`
- `mcp_githublocal_create_or_update_file`
- `mcp_githublocal_list_commits`
- `mcp_githublocal_get_teams`
- `mcp_githublocal_list_issue_types`
- `mcp_githublocal_assign_copilot_to_issue`
- `mcp_githublocal_add_comment_to_pending_review`

Remote:
- `mcp_githubremote_create_or_update_file`
- `mcp_githubremote_assign_copilot_to_issue`

## Example prompts
- “List my open PRs and summarize what changed.”
- “Create a branch, update docs, and open a PR.”
- “Assign Copilot to issue #123.”
