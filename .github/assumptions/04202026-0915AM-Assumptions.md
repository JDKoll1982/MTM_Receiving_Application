Last Updated: 2026-04-20

# Dunnage Default Image Location Migration Assumptions

Before implementing the requested Dunnage image-location migration, the following assumptions need confirmation.

1. Missing-image prompt should ask for an image file path, not a folder path.
   Why this assumption is needed: Dunnage type and part records store image file references, and the current UI already uses a file picker for assigning individual images.
   Potential impact if wrong: The implementation could prompt for the wrong kind of location and force an extra step or save unusable paths.
   Alternative interpretations considered: Prompt for the containing folder; prompt for either a folder or a file; skip prompting and only report missing items.

2. If the user cancels the prompt for a missing type or part image, the system should treat that item as having no image and clear the stored image path for that item.
   Why this assumption is needed: Changing the default image root while leaving an unresolved stale path would keep the record pointing at a missing image after migration.
   Potential impact if wrong: Clearing the path could remove a reference the user wanted to keep for later manual repair, while preserving it could leave broken image references in the active data.
   Alternative interpretations considered: Preserve the old stored path and only report it missing; keep the path but mark it unresolved in the status summary; clear only the in-memory path and require a separate save action later.

3. The migration should process every existing Dunnage type and part record that currently has an image reference, moving files from the old root to the new root while preserving the existing relative folder structure when possible.
   Why this assumption is needed: Most Dunnage image paths are stored as relative paths under the configured root, so preserving the relative structure is the least disruptive migration approach.
   Potential impact if wrong: A different folder layout or selective migration could break current image references or create duplicate image trees.
   Alternative interpretations considered: Re-import every image into a freshly generated file name; migrate only files currently found under the old root; update settings only and do not move files automatically.

Please confirm or correct these assumptions before implementation continues, especially assumption 2 about whether canceling a missing-image prompt should clear that record's stored image path or preserve the old reference.
