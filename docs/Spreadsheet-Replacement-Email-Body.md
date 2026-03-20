## MTM Receiving Application Overview

Last Updated: 2026-03-19

The MTM Receiving Application is a desktop app built to replace the spreadsheet-based processes currently used for receiving, dunnage, and Volvo work. Instead of relying on multiple Google Sheet tabs, manual copy-forward steps, sorting helpers, and separate history sheets, the app brings those workflows into one system designed for daily use.

This is being shared now because the application is nearing its 1.0 release. As it gets close to first full release, it is important for users to understand what the app is replacing, why the change is happening, and what improvements they can expect.

Just as important, this moves MTM process knowledge, packaging rules, shipment rules, and receiving history out of Google-hosted spreadsheets and into a system controlled by MTM. In practical terms, that means our intellectual data is no longer living primarily in Google’s cloud.

### What This App Replaces

- It replaces the Google Sheets process used to record receiving activity, prepare label data, and move completed entries into history.
- It replaces the Google Sheets process used to track packaging materials, customer-supplied returnables, and dunnage history.
- It replaces the spreadsheet-based Volvo shipment and label workflow.
- It also includes tools for shipping and receiving support work, including outside service history lookups for the shipping department.

### What Is Still Being Added Before Release

- A Lot Number lookup feature is planned before release.
- This future feature will help users look up the history of a part number’s lot numbers.
- It is planned to combine transaction history from Infor Visual with this app’s receiving history.
- The goal is to help generate a list of lot numbers that may still be in circulation in the building.
- This will make traceability and follow-up work easier than searching through records by hand, as Infor Visual does not keep track of Material Lot Numbers.

### Purpose Of The App

The purpose of the app is to make receiving-related work faster, more consistent, and easier to manage by:

- giving users guided screens instead of large editable spreadsheets
- reducing manual re-entry, sorting, and cleanup steps
- keeping active label data and historical records organized automatically
- making it easier to review, edit, and report on completed work

### Key Improvements Over The Old Spreadsheet Process

- **One system instead of many Spreadsheets:** Users no longer need to move between multiple Google Sheets and history sheets to complete work.
- **Guided data entry:** The app walks users through the process step by step, which reduces missed fields and inconsistent entries.
- **Built-in validation:** The app checks important values like PO, quantity, location, and related workflow rules while the user is working, instead of leaving errors to be caught later. Example: A user can no longer put the wrong PO with the Wrong Part Number.
- **Less risk of spreadsheet corruption:** In Google Sheets, one accidental cell deletion, one bad paste, or one value entered into the wrong row or column can break the sheet, damage formulas, or mix up records. The app reduces that risk by giving users controlled fields and a structured process instead of an open grid.
- **Cleaner label workflow:** Active label data is prepared for printing in the app, then moved into history through a controlled process instead of manual spreadsheet cleanup.
- **Better history tracking:** Completed records are preserved in structured history data, making it easier to review past work and produce reporting.
- **Less manual sorting and filling:** Tasks that used to depend on spreadsheet helpers, blank-fill actions, or manual sorting are handled by the application workflow.
- **Better support for complex data:** Dunnage specs, receiving details, and Volvo shipment header/line information are handled more clearly than they were in flat spreadsheets.
- **More professional reporting:** Reporting is generated from the application data, which gives a cleaner summary of Receiving, Dunnage, and Volvo activity.  Reporting is now as easy a couple clicks and pasting the output into the body of your email, and it looks ALOT cleaner.

### Spreadsheet vs Application

The old spreadsheet process depended on users knowing which tab to use, when to sort, when to fill blanks, when to move data to history, and how to prepare email summaries. The application replaces that with guided screens and built-in actions.

| Area | Old Spreadsheet Process | What Users See In The App | Improvement |
|---|---|---|---|
| Data ownership | Important process knowledge and records lived in Google Sheets. | Information is kept in MTM’s application and data systems. | Gets MTM intellectual data off Google’s cloud and under tighter internal control. |
| Receiving work | Users entered rows by hand, filled in blanks, sorted data, and moved records to history manually. | Users move through guided receiving screens with clear steps from start to finish. | Easier to follow and less dependent on memory or spreadsheet habits. |
| Error prevention | Problems were easier to miss until after the spreadsheet had already been filled out. | The app checks entries as the user works. | Mistakes are caught earlier. |
| Spreadsheet damage risk | A sheet could be thrown off by deleting the wrong cell, typing in the wrong place, pasting over formulas, or shifting data into the wrong row. | Users work in guided screens and defined fields instead of editing a large open spreadsheet grid. | Chances of accidental corruption and mixed-up records is practicaly impossible now. |
| Fast row entry | Users typed directly into spreadsheet rows and relied on helper actions to clean things up later. | Users can still enter rows quickly, but with guided fields, save controls, and built-in checks. | Keeps speed while improving control. |
| Editing and corrections | Fixes often meant manually finding rows and adjusting cells. | Users can review, search, filter, edit, and save changes in dedicated screens. | Corrections are easier and more reliable. |
| Moving completed work to history | Users had to trigger manual steps to move finished work out of the active sheet. | The app handles active work and completed history in a more controlled way. | Reduces manual cleanup and lowers the chance of missed steps. |
| Reports and email summaries | Users depended on spreadsheet ranges and helper logic to create grouped summaries. | The app provides built-in summaries and reporting views. | Reporting is cleaner and more consistent. |
| Shipping department support tools | No Support | The app already includes a shipping department tool for looking up outside service history by part number or vendor. | Gives shipping a faster and cleaner way to review outside service activity. |
| Dunnage tracking | Dunnage information was handled as flat spreadsheet rows, where users had to manually input each item by hand. | Users have guided screens for selecting types, parts, quantities, details, review, manual entry, and editing. | Better fit for real packaging workflows. |
| Dunnage setup and maintenance | Keeping part and packaging details organized in spreadsheets was difficult. | The app gives users dedicated screens to manage packaging types, parts, details, and tracked inventory lists. | Makes ongoing maintenance more consistent. |
| Volvo shipment handling | Shipment work in spreadsheets depends heavily on manual row handling and user memory. | Users can build shipments, review them, save unfinished work, complete shipments, review history, and make edits in one place. | Brings the full process into one controlled workflow. |
| User consistency | Success depended heavily on each person knowing the spreadsheet process. | Users see a structured, repeatable set of screens and actions. | Training is easier and results are more consistent between users. |

### Infor Visual Integration

Another major change is that the application can use live ERP information in ways the Google Sheets process does not.

| Area | Old Spreadsheet Process | What Users See In The App | Why This Matters |
|---|---|---|---|
| Receiving | Users typed in purchase order, part, and location information by hand. | The app can pull live receiving-related information and use it while the user works. | Improves accuracy and reduces mismatches. |
| Receiving decisions | Spreadsheet logic could organize data after entry, but it could not guide users with live business context. | The app can help users work from current ERP information rather than only from typed values. | Makes the workflow smarter and more dependable. |
| Dunnage | Dunnage tracking in spreadsheets depended on manual entry and local knowledge. | The app keeps dunnage information in a structured internal system with dedicated screens for setup, entry, and history. | Even where live ERP lookups are not the focus, the app is still far more controlled than a spreadsheet. |
| Volvo | Shipment work in spreadsheets did not naturally bring in live reference information while users built or edited shipments. | The app can use current ERP information to support shipment work and part/location context. | Gives users better support while they are entering and reviewing shipments. |
| Future lot number lookup | There are currently a couple of spreadsheets floating around for certain Part Numbers in reguards to Lot Numbers. | A planned lot number lookup feature will help bring together lot history from both the ERP's Transaction History side and the app’s own receiving history. | Will improve traceability and make it easier to identify lot numbers that may still be in circulation. |
| Overall | Google Sheets mostly acted as a place to type, sort, and summarize rows. | The app combines MTM-controlled records with live business information where it helps users most. | Creates a stronger long-term system than spreadsheets alone. |

**Important note:** The app uses live ERP information to help users validate and complete work. It is not replacing the ERP system itself.  Also anything in the Ship/Recv Tools are tools meant to supliment Infor Visual by not hogging a Licence just to do a casual search.

### What Users Can Expect

- a more structured and easier-to-follow process
- NO manual spreadsheet steps
- less risk of accidental spreadsheet damage from deleting or overwriting the wrong cell
- more reliable saved history
- better consistency between users
- a system designed to grow beyond the limits of the old spreadsheet workflow
- live training will be available for all personnel who require it and for anyone who wants additional help getting comfortable with the new system
- continued improvement before release, including additional lookup tools for traceability and shipping/receiving support

### In Short

This app is not just a new screen for the same process. It is an upgrade from a spreadsheet-driven system to a dedicated application that improves accuracy, reduces manual work, gives MTM better control of its data, removes important business knowledge from Google-hosted spreadsheets, and continues to grow with additional tools being added before release.