Module-Based App Development Meeting
Transcript

<https://otter.ai/u/X72N1XefzoSKYrd36Uzm9QCyAHM?view=summary>

The meeting focused on the development of a module-based app for managing work orders and material handling. Key points included the app's integration with MySQL and Visual servers, the creation of self-contained modules for different tasks (e.g., receiving, dunnage labels), and a guided wizard for operators. The app will feature a login module, a guided wizard for operators, and a quality task module. Material handlers will have a favorites tab and auto-assignment based on zones. The app will also include a quick add feature for non-wait list tasks and a data trail for project management. Training programs will be developed for new press operators.

Action Items
[ ] Compile all collected feature requests and meeting input into a mockup for the module-based waitlist app (version planning), to drive design and future development.
[ ] Create and deliver training for press operators and leads on the new waitlist app (initial rollout), including building training programs and scheduling sessions before deployment.
[ ] Update the WHIP app to support flagging/validation for items moved to truck locations and integrate that behavior with the new waitlist workflows.
[ ] Obtain a list of site identifiers / IP addresses for each press-floor computer so the app can auto-set site ID and show the correct site data.
[ ] Meet with quality and IT/security stakeholders to investigate and plan an automated quality alert notification method (email, Teams, intercom, or paging) and identify required security approvals.
[ ] Follow up with Austin (and quality leadership) about participation and alignment on quality notification workflow and training plan.
Outline
Module-Based Application Overview
Speaker 1 explains the module-based structure of the new application, similar to the Whip app, connecting to the MySQL server and pulling information from the Visual server.
Speaker 1 emphasizes that the app will not write to the Visual server, ensuring data integrity.
Speaker 1 describes the modular approach, where different modules handle specific tasks, such as material handlers and setup texts, allowing for easy updates without affecting other modules.
Speaker 3 requests an example of a module, and Speaker 1 provides an example of a receiving application, explaining how modules are self-contained and do not affect other modules.
Login Module and Operator Access
Speaker 1 demonstrates a login module, which is self-contained and does not affect production changes.
Speaker 1 explains that operators have access to their own modules, which are separate from other modules, ensuring that changes to one module do not affect others.
Speaker 1 shows how different modules, such as receiving labels and dunnage labels, are managed independently.
Speaker 1 introduces the idea of a guided wizard for operators, simplifying the process by guiding them step-by-step through tasks.
Wait List and Analytics Rights
Speaker 1 discusses the wait list, explaining that everyone can add items, but only certain users, like production leads, have analytics rights to view operator activities.
Speaker 1 clarifies that operators can see the wait list but cannot view analytics, ensuring data privacy.
Speaker 1 suggests adding modules for specific tasks, such as tracking operators logged into jobs or adding notes for dies, to streamline operations.
Speaker 1 mentions the need for Visual to store certain data, such as dunnage, to enable better integration with the new application.
Material Handling and Simplification
Speaker 4 suggests simplifying the material handling process by having a "recent" tab for frequently used items, reducing the need for operators to search for items.
Speaker 1 agrees and suggests a "favorites" tab for commonly used items, allowing operators to quickly access them.
Speaker 4 raises concerns about overlaps and accumulating tasks on the wait list, which Speaker 1 addresses by suggesting auto-assignment based on location and task priority.
Speaker 1 proposes delegating material handlers to specific zones, with the app automatically assigning tasks based on location and task priority.
Quality Control and Communication
Speaker 1 discusses integrating a quality module into the wait list, allowing operators to add quality tasks that only quality technicians can see.
Speaker 4 mentions issues with quality control, such as reworking parts in the spot welding department, and suggests automatic email alerts for quality issues.
Speaker 1 considers the possibility of integrating the app with the intercom system to send quality alerts, but acknowledges potential issues with office personnel listening to alerts all day.
Speaker 5 suggests sending alerts to phones, but Speaker 1 notes that in-house phone apps are not allowed.
Training and Simplification for Operators
Speaker 4 asks about training new press operators on the new wait list system, and Speaker 1 suggests creating training programs to ensure operators understand the new system.
Speaker 1 emphasizes the importance of simplifying the system to reduce errors and improve efficiency.
Speaker 1 mentions the potential for the app to provide a simplified version of work orders, making it easier for operators to understand and follow.
Speaker 1 notes that the initial version of the app will focus on integrating with Visual and simplifying tasks, with additional features to be added later based on feedback.
Data Collection and Reporting
Speaker 1 discusses the importance of data collection and reporting, noting that Nick and Chris will need to approve any updates before they are rolled out.
Speaker 1 explains that the initial version of the app will focus on integrating with Visual and simplifying tasks, with additional features to be added later based on feedback.
Speaker 1 mentions the potential for the app to provide a simplified version of work orders, making it easier for operators to understand and follow.
Speaker 1 notes that the app will have a preset time for each job type, which can be adjusted by Brett or Nick based on data collected from the app.
Handling Material and Tasks
Speaker 1 discusses the need for better handling of material and tasks, suggesting a system where material handlers can quickly add tasks to the wait list.
Speaker 5 suggests that material handlers should be able to log tasks manually, similar to the old system, to ensure they get credit for their work.
Speaker 1 agrees and suggests adding a "quick add" button for material handlers to log tasks manually.
Speaker 1 mentions the potential for the app to provide a simplified version of work orders, making it easier for operators to understand and follow.
Project Management and Data Trails
Speaker 1 discusses the need for better project management, suggesting a system where material handlers can log in and out of specific projects.
Speaker 1 explains that this will create a data trail, allowing leads to see what tasks have been completed and what is still pending.
Speaker 1 mentions that this will help with data collection and reporting, ensuring that all tasks are accounted for.
Speaker 1 notes that this system will also help with training new material handlers, as they can see what tasks need to be done and how they should be logged.
Final Thoughts and Next Steps
Speaker 1 asks if anyone has any final thoughts or questions, and Speaker 5 mentions the need to start running material handlers when there is nothing on the wait list.
Speaker 1 suggests adding a "quick add" button for material handlers to log tasks manually, ensuring they get credit for their work.
Speaker 1 mentions the potential for the app to provide a simplified version of work orders, making it easier for operators to understand and follow.
Speaker 1 notes that the initial version of the app will focus on integrating with Visual and simplifying tasks, with additional features to be added later based on feedback.
