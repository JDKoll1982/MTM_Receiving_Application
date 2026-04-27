1. Login Module
Purpose: Allow users to authenticate and grant access to appropriate modules. Features:

User authentication (username/password, role recognition).
Role-based access: restricts operator, lead, or admin access to their designated modules.
Contains isolated code; updates to other modules do not impact login.
2. Operator Module
Purpose: Interface for press and floor operators to access relevant tasks. Features:

Personalized dashboard showing operator-specific tasks and wait list.
Dropdown menus/favorites for common item requests (minimizes manual entry).
Step-by-step guided wizards for common workflows (e.g., requesting a box).
Ability to log into jobs and record required materials.
3. Wait List Module
Purpose: Centralized place for all production/material requests and tracking. Features:

Add, update, and remove task items (e.g., material pickups, job requests).
Segregated views by user type (operator, lead, quality).
Auto-assign tasks based on location/zone, urgency ("red zone" tasks).
Analytics control for production leads.
Integration with Visual server for pulling work order data.
Time tracking for each wait list item (preset/adjustable by admin).
Recent and favorites tabs for quickly re-accessing common items.
“Quick Add” functionality for manual, task-less pickups.
4. Material Handling Module
Purpose: Tools for material handlers to manage materials and logistics on the floor. Features:

Task assignment by zone/location.
Auto-assignment or manual assignment of material runs.
Notification and flagging of incorrect part movement (e.g., wrong part on truck).
Project tracking (log in/out of special projects).
Data trail for completed actions.
Integration with truck/van scheduling as needed.
5. Quality Control Module
Purpose: Report and manage quality issues promptly. Features:

Operators can create quality-related tasks (e.g., flag bad parts).
Quality technicians see only their relevant queue.
Support for rapid notification options (email, Teams message, intercom, etc.), respecting security restrictions.
Dedicated wait list for the quality team, distinct from general operator wait lists.
6. Analytics & Admin Module
Purpose: Oversight, analytics, and system configuration. Features:

Analytics dashboards for leads/admins to review job durations, material handler activity, and task completion.
Ability to set and adjust job task default times.
Access to override, audit trails, and adjustment controls.
Consent workflow for production-wide updates (approval required from key users).
7. Training Module
Purpose: Facilitate onboarding and continuous education. Features:

Training workflows for new operators and leads.
Step-by-step guides or interactive checklists.
Structured training programs with feedback collection.
8. System Integration Module
Purpose: Handles connections to external systems (Visual server, MySQL, WHIP app, etc.). Features:

Pulls/pushes data between app and existing backend/ERP.
Auto-detection of site/computer via IP for site-specific configuration.
Ensures compliance with security and in-house restrictions.
Notes on Implementation:
Each module should be as isolated/independent as possible ("self-reliant"), to minimize impact from changes elsewhere.
UI/UX should reflect user role, surface only relevant modules, and simplify actions to reduce training/error rates.
Collect feedback during rollout to improve modules iteratively.
Ensure all workflow changes and new features require admin/lead approval before going live.
