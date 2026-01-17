# Repomix Usage

Use these presets to generate AI-friendly repository bundles.

## Presets

- Code only (minimal tokens): [.repomix/config/repomix.code-only.config.json](../../.repomix/config/repomix.code-only.config.json)
- Full context with git: [.repomix/config/repomix.full-context.config.json](../../.repomix/config/repomix.full-context.config.json)
- Copilot docs context: [.repomix/config/repomix-copilot-docs.json](../../.repomix/config/repomix-copilot-docs.json)

## Commands

```powershell
# Markdown output (no config)
npx repomix@latest --style markdown

# Code-only, compressed (fast, low tokens) -> Markdown
npx repomix@latest --config .repomix/config/repomix.code-only.config.json --style markdown

# Full context with diffs + logs (larger output) -> Markdown
npx repomix@latest --config .repomix/config/repomix.full-context.config.json --style markdown

# Documentation-focused bundle used by Copilot -> Markdown
npx repomix@latest --config .repomix/config/repomix-copilot-docs.json --style markdown
```

## Notes

- Security checks are enabled by default; redact secrets before sharing outputs.
- For very large repos, increase `splitOutput` or narrow `include` patterns.
- Tokenization uses `o200k_base` (good for GPT-4o/4o-mini). Adjust if needed.
