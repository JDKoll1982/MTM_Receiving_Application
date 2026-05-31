1. Assumption: Increase the splash screen host window height from 800 to approximately 980 pixels before showing the new user account creation dialog.
   - Why this assumption is needed: The request says to increase the splash screen height so the modal can be longer and avoid a scroll bar, but it does not specify the target height.
   - Potential impact if wrong: If the new height is too small, the dialog may still need scrolling or feel cramped; if too large, it may look oversized on smaller displays.
   - Alternative interpretations considered: Increase only the ContentDialog max height without resizing the host window; increase both width and height; choose a different height such as 900 or 1000 pixels.
   - Please confirm, correct, or clarify the desired splash window height before implementation continues.

2. Assumption: Remove the ERP expander and checkbox entirely, keep ERP credentials always visible, and require Visual/Infor username and password during account creation.
   - Why this assumption is needed: The request says ERP System Access is not optional and the checkbox/collapsibility should be removed, but it does not explicitly state whether the ERP fields should become required validation inputs.
   - Potential impact if wrong: If ERP credentials should remain optional in validation, forcing them would block account creation; if they should be required, leaving validation optional would allow incomplete account setup.
   - Alternative interpretations considered: Keep ERP fields always visible but still optional; keep them always visible and required only when provided; make both ERP fields required for all new accounts.
   - Please confirm, correct, or clarify whether ERP username and password must now be required to create an account before implementation continues.
