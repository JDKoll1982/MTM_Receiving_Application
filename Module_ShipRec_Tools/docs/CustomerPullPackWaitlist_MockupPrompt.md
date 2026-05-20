# Customer Pull n' Pack Waitlist Mockup Prompt

Last Updated: 2026-05-20

## Purpose

Use this prompt to generate **mockups for the waitlist experience** described in [VolvoMackPullPackReport_ReviewSpec.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/VolvoMackPullPackReport_ReviewSpec.md).

These mockups should fit the **existing MTM Receiving Application** as closely as possible, not a generic web app and not a new design system invented from scratch.

The mockups should specifically reflect the current direction that:

- the **report page** lets users create or update waitlist entries
- the **waitlist** is a **separate entity and separate page**
- the **material handler** uses the **dedicated Waitlist page** to pull orders

Reference images for the current report style are stored beside this prompt:

- [CustomerPullPackReport_Page1.png](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/CustomerPullPackReport_Page1.png)
- [CustomerPullPackReport_Page2.png](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/CustomerPullPackReport_Page2.png)
- [CustomerPullPackReport_Page3.png](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/CustomerPullPackReport_Page3.png)

---

## Prompt To Use

```md
<role>
You are an expert WinUI 3 product designer, desktop application UI architect, and mockup specialist.

Your goal is to create mockups for the MTM Receiving Application that feel like they belong inside the existing app today.

You are not designing a new visual brand.
You are extending an existing internal manufacturing desktop application.

Before producing any mockup, build a concrete mental model of the existing system:
- This is a WinUI 3 desktop application, not a generic marketing website.
- The mockups must match the current MTM application shell and navigation patterns as closely as possible.
- The Ship/Rec Tools area already exists and should remain visually consistent with the current app.
- The waitlist is a separate feature entity from the report, but users can create or update waitlist entries from the report page.
- Material handlers primarily work from the dedicated Waitlist page, not from the report page.
- Review both the built-in WinUI 3 control set and the CommunityToolkit WinUI controls/animations used by the project before choosing controls for the mockups.
</role>

<project-context>
Current repository: MTM_Receiving_Application

Use the current spec as the feature source of truth:
- Module_ShipRec_Tools/docs/VolvoMackPullPackReport_ReviewSpec.md

Use these nearby screenshot files as visual references for the report content and printed layout style:
- Module_ShipRec_Tools/docs/CustomerPullPackReport_Page1.png
- Module_ShipRec_Tools/docs/CustomerPullPackReport_Page2.png
- Module_ShipRec_Tools/docs/CustomerPullPackReport_Page3.png

Use these app/navigation anchors when deciding layout structure:
- MainWindow.xaml.cs -> ShipRecToolsPage route exists
- Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml.cs -> ShipRec Tools is a hosted module container
- Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs -> tools switch inside a Ship/Rec workflow container
- Module_ShipRec_Tools/Views/View_ShipRecTools_ToolSelection.xaml.cs -> Ship/Rec Tools already has a tool selection experience

Use these UI toolkit facts when choosing controls in the mockups:
- Native WinUI 3 controls are available
- CommunityToolkit.WinUI.UI.Controls is referenced by the project
- CommunityToolkit.WinUI.Animations is referenced by the project
- Material.Icons.WinUI3 is referenced by the project
- The installed CommunityToolkit WinUI controls package is an aggregate of these control families:
   - CommunityToolkit.WinUI.UI.Controls.Core
   - CommunityToolkit.WinUI.UI.Controls.DataGrid
   - CommunityToolkit.WinUI.UI.Controls.Input
   - CommunityToolkit.WinUI.UI.Controls.Layout
   - CommunityToolkit.WinUI.UI.Controls.Markdown
   - CommunityToolkit.WinUI.UI.Controls.Media
   - CommunityToolkit.WinUI.UI.Controls.Primitives

Important navigation truth:
- Mockups must include proper navigation and the existing app-style nav/header shell.
- Do not invent a brand-new sidebar, floating dock, mobile nav, or SaaS-style dashboard shell.
- The design should look like it sits inside the same MTM application as the existing Ship/Rec tools.
</project-context>

<toolkit-rules>
Before selecting controls for the mockups, review the controls that are realistically available from:
- built-in WinUI 3
- CommunityToolkit.WinUI.UI.Controls
- CommunityToolkit.WinUI.Animations

Use proper WinUI-style controls in the mockups instead of generic web widgets.

Prefer realistic desktop controls such as:
- NavigationView or the app's existing shell/navigation pattern
- CommandBar / AppBarButton style actions
- AutoSuggestBox for customer or part search when appropriate
- ComboBox for customer, status, and print-preset selection
- DatePicker or date-range controls that feel native to WinUI
- ToggleSwitch / CheckBox for simple filters
- DataGrid-style tabular surfaces where toolkit-backed grid behavior makes sense
- Expander / disclosure patterns for detail sections when appropriate
- ContentDialog or panel-style edit surfaces for create/edit interactions
- InfoBar or equivalent status messaging patterns

Also consider these concrete CommunityToolkit WinUI controls and patterns when they fit the workflow:

- **DataGrid** for dense report rows, waitlist queues, sortable tabular views, row status indicators, and inline action columns
- **DataGridTemplateColumn**, **DataGridTextColumn**, **DataGridComboBoxColumn**, and row-details patterns for richer report and waitlist grids
- **RichSuggestBox** or **TokenizingTextBox** when search, quick customer lookup, favorites, or multi-value assisted entry would benefit from richer suggestion behavior
- **ListDetailsView** when a master-detail split is appropriate for waitlist queue on the left and selected work item details on the right
- **BladeView** when a progressive drill-in pattern is better than modal stacking for report -> waitlist -> detail workflows
- **GridSplitter** if a resizable desktop split-view helps users tune space between queue and detail surfaces
- **DockPanel**, **WrapPanel**, **UniformGrid**, or **WrapLayout** for utility layout surfaces, filter groups, summary panels, or tool-selection cards when native StackPanel/Grid would be too rigid
- **HeaderedContentControl** or **HeaderedItemsControl** for clearly labeled warehouse-oriented grouped panels
- **InAppNotification** for transient completion or problem feedback when a full dialog would be too disruptive
- **DropShadowPanel** for subtle elevation around key panes while still preserving the current MTM desktop feel
- **TabbedCommandBar** if a compact desktop command surface is helpful for switching report/waitlist action sets without introducing a web-style toolbar
- **MarkdownTextBlock** only if there is a real need to render structured help/instructions inline from maintained text, not as a styling gimmick
- **ImageEx** when a mockup includes reference images, thumbnails, or report-preview imagery that should load more robustly than a basic image control
- **RangeSelector** or **RadialGauge** only if they solve a real operational need; do not add them decoratively

Controls that are already visibly used in this codebase should be treated as especially safe reference patterns for the mockups:

- **DataGrid**
- **InfoBar**
- **Expander**

When choosing toolkit controls, favor the ones that improve real operator workflow, dense data entry, or desktop readability.
Do not add CommunityToolkit controls just because they exist.

Avoid describing controls in web-only terms such as:
- sticky SaaS filter chips everywhere
- floating action buttons
- browser-style side drawers that ignore the app shell
- web-only card carousels

Animations should stay subtle and enterprise-appropriate.
If motion is suggested, it should feel like a realistic use of CommunityToolkit WinUI animations, not marketing-site motion design.
</toolkit-rules>

<design-direction>
Create mockups that feel:
- industrial
- practical
- warehouse-friendly
- dense but readable
- close to the current MTM desktop app
- consistent with existing Ship/Rec and Material Availability Board patterns

Avoid:
- flashy startup aesthetics
- playful maximalism
- glassmorphism-heavy marketing UI
- mobile-first app shells
- consumer-app card overload
- Dribbble-style concept art disconnected from the current app

Prioritize:
- strong information hierarchy
- obvious navigation
- easy scanning for warehouse users
- desktop-first layouts
- practical controls for real operational use
- consistency with the current app chrome and module structure
- using controls that make sense for real WinUI 3 and CommunityToolkit WinUI implementation
</design-direction>

<mockup-scope>
Create mockups for the current spec with emphasis on the waitlist flow.

Produce mockups for these screens:

1. Ship/Rec Tools tool-selection screen updated to include the new Customer Pull n' Pack tool.
2. Customer Pull n' Pack report page with:
   - existing app header/navigation
   - customer picker
   - report filters
   - main report grid
   - row-level action to create or update a waitlist entry
   - clear visual indication if a row is already linked to a waitlist record
3. Create/Edit Waitlist entry experience launched from the report page.
4. Dedicated Waitlist page for material handlers showing pull work queue.
5. Waitlist detail state showing an item being worked, with status controls and notes.
6. Feature settings page for Customer Pull n' Pack defaults.
7. Empty-state screen when a selected customer has no open demand.

If useful, also include one compact flow panel showing how a user moves from report row -> waitlist entry -> waitlist work page.
</mockup-scope>

<settings-rules>
Include a mockup for the settings page described by the current review spec.

The settings page should feel like an existing MTM settings screen, not a separate admin console.

The settings mockup should include at minimum:
- default customer
- default date range type
- default date range length
- default sort order
- default filter for shortages only
- default filter for unpulled only
- default print preset
- auto refresh option
- waitlist default status filter
- favorite customers list or equivalent quick-select area

The settings page should visually fit the current app shell and show how it is reached from the Ship/Rec Tools feature flow.
</settings-rules>

<waitlist-rules>
Respect these feature rules from the current spec:
- Waitlist is separate from the report.
- Users can create or update waitlist entries from the report page.
- Material handlers use the Waitlist page for pulling orders.
- Waitlist status values should support: Accepted, Completed, Cancelled, Problem.
- Handler notes are important when status is Problem.
- The UI should reflect that the waitlist is MTM-managed workflow data layered on top of read-only Infor Visual report data.
</waitlist-rules>

<navigation-rules>
Navigation must look like the current app.

Include:
- proper app header/title behavior
- the Ship/Rec Tools context
- a back path to the Ship/Rec tool selection area where appropriate
- consistent placement of commands and page titles

Do not:
- create a separate standalone app shell
- hide the user inside a mockup that no longer looks like MTM
- remove the sense that this page lives inside the current module/navigation structure
</navigation-rules>

<visual-guidance>
Use a visual language that matches the current application as closely as possible:
- desktop WinUI-style spacing and proportions
- restrained enterprise colors
- clear panels, cards, tables, command bars, and status surfaces
- familiar MTM-style headers and navigation placement
- readable dense data presentation
- practical buttons and filters
- controls that could realistically be implemented with native WinUI 3 and CommunityToolkit WinUI controls

The report page should still visually echo the provided report screenshots where appropriate:
- recognizable report grouping
- obvious shortage emphasis
- clear pull date and qty-to-pack emphasis
- visible relationship between parent part, locations, and sub-parts

The waitlist page should feel more task-oriented than the report page:
- queue layout
- statuses easy to scan
- notes and problem states obvious
- actions clear for material handlers

The settings page should feel simpler and more form-driven than the report and waitlist pages:
- grouped settings sections
- desktop-friendly labels and inputs
- clear save/cancel actions
- obvious relationship to the Customer Pull n' Pack feature
</visual-guidance>

<deliverables>
Provide the mockups as a structured mockup package.

For each screen include:
- screen name
- purpose
- where it lives in the app navigation
- key visible controls
- key user actions
- short rationale for why the layout matches the current app

Also provide:
- a short notes section calling out how the mockups preserve current MTM navigation
- a short notes section calling out how the waitlist/report separation is communicated in the UI

If you output multiple mockups, order them by actual user flow.
</deliverables>

<quality-bar>
Your mockups must:
- look like they belong in the current MTM app
- preserve proper navigation and the existing nav/header feel
- avoid generic web-dashboard patterns
- show the waitlist as separate from the report
- still let the report page create or update waitlist entries
- support real warehouse use, not just presentation aesthetics
- use proper WinUI 3 / CommunityToolkit-style controls in the mockup descriptions and layout decisions

If a design choice would make the mockup prettier but less consistent with the current app, choose consistency.
</quality-bar>
```

---

## Recommended Use Notes

- Use this prompt when generating **waitlist-focused UI mockups** for the Customer Pull n' Pack feature.
- Keep [VolvoMackPullPackReport_ReviewSpec.md](c:/Users/johnk/source/repos/MTM_Receiving_Application/Module_ShipRec_Tools/docs/VolvoMackPullPackReport_ReviewSpec.md) open beside the mockup output.
- Keep the three screenshot files nearby as visual references for the report layout and printed report tone.
- If mockups drift toward generic modern dashboards, tighten them back toward the current MTM desktop shell and Ship/Rec tool style.