# MTM Update Pages

This folder contains end-user update pages generated alongside application version bumps.

## Structure

- `mockup/` contains the reference pattern that versioned update pages should follow.
- `{version}/` contains the actual release page for a specific application version.
- Shared release assets live in the parent folder as `styles.css` and `app.js`.

## Expected Files Per Version

Each version folder should contain:

1. `index.html`

Version folders must reuse the shared assets in the parent updates folder:

- `../styles.css`
- `../app.js`

## Logo Usage

Versioned pages and the mockup should reference the MTM logo from:

`../../../Assets/MTMLogo.jpg`

## Content Rules

- Keep the copy focused on operators and end users.
- Only describe changes verified by the actual diff.
- Call out required user action explicitly.
- If no action is required, say so plainly.