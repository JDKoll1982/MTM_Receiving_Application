🎨 Comprehensive UI Design Analysis: Module_Receiving vs Module_Routing
Background Colors & Layers
Element	Module_Receiving	Module_Routing	Notes
Page Background	ApplicationPageBackgroundThemeBrush	ApplicationPageBackgroundThemeBrush	✅ Same
Card Backgrounds	CardBackgroundFillColorDefaultBrush	Missing/using ApplicationPageBackgroundThemeBrush	⚠️ Routing lacks card layering
Input Containers	CardBackgroundFillColorDefaultBrush + borders	Plain borders only	⚠️ Routing has flat design
Accent Headers	AccentFillColorDefaultBrush with white text	Not used	⚠️ Routing lacks visual emphasis
Status Boxes	CardBackgroundFillColorSecondaryBrush	Not present	⚠️ Routing lacks status indicators
List/Grid Containers	CardBackgroundFillColorDefaultBrush background	Transparent/border only	⚠️ Low visual hierarchy
Padding & Spacing Inconsistencies
Location	Receiving	Routing	Issue
Page/Grid Padding	20-24px (varies by depth)	24px (uniform)	Receiving has nuanced spacing
Card Padding	20px (input cards), 16px (list cards)	24px everywhere	⚠️ Routing over-padded
Stack Spacing	16px (sections), 12px (groups), 8px (tight)	16px everywhere	⚠️ Routing lacks hierarchy
Input Fields	8px between label and input	12px (from Header property)	Inconsistent
Button Spacing	12px between buttons	12px	✅ Same
Border Styles
Element	Receiving	Routing	Issue
Corner Radius	8px (cards), 4px (inputs/secondary)	4px everywhere	⚠️ Routing less polished
Border Thickness	1 (standard)	1	✅ Same
Border Brush	CardStrokeColorDefaultBrush	CardStrokeColorDefaultBrush	✅ Same
Typography & Icons
Element	Receiving	Routing	Issue
Page Titles	TitleTextBlockStyle	TitleTextBlockStyle	✅ Same
Section Headers	SubtitleTextBlockStyle	SubtitleTextBlockStyle	✅ Same
Field Labels	BodyStrongTextBlockStyle + FontIcon	Plain Header text	⚠️ Routing lacks visual richness
Icon Foreground	AccentTextFillColorPrimaryBrush (primary fields), AccentTextFillColorSecondaryBrush (meta)	Not used	⚠️ Routing has no icons
Icon Size	14-16px (contextual)	N/A	⚠️ Missing entirely
FontIcon Glyphs Used	&#xE8A5; (PO), &#xE8B7; (Part), &#xE7B8; (Package), &#xE8B4; (Heat), &#xEA8B; (Weight)	None	⚠️ No visual language
Button Styles
Type	Receiving	Routing	Issue
Primary Actions	AccentButtonStyle with icon + text in StackPanel	AccentButtonStyle text only	⚠️ Routing less descriptive
Secondary Actions	Default style	Default style	✅ Same
Icon Buttons	FontIcon (16px) + TextBlock in horizontal StackPanel, 8px spacing	Not used	⚠️ Routing has text-only buttons
Info Banners & Status
Element	Receiving	Routing	Issue
Accent Info Headers	AccentFillColorDefaultBrush background, white text, rounded corners, centered	Not used	⚠️ Routing lacks emphasis
Info Bars	InfoBar with Warning/Success severity	Not used	⚠️ No validation feedback
Status Indicators	Bordered boxes with icon + label + value	Plain text in StackPanel	⚠️ Routing lacks visual status
Progress Rings	20x20px next to status text	20x20px	✅ Same size
List/Grid Presentation
Element	Receiving	Routing	Issue
List Items	Card-wrapped (CardBackgroundFillColorDefaultBrush), 8px radius, 1px borders	Plain ListView with 4px rounded border container	⚠️ Routing less defined
Grid Headers	Bold (FontWeight="SemiBold")	Bold	✅ Same
DataGrid Usage	CommunityToolkit DataGrid with horizontal gridlines	ListView only	⚠️ Routing simpler
Empty State	Centered placeholder text with muted color	Centered text	✅ Similar
Review/Summary Screens
Element	Receiving	Routing	Issue
Layout	Two-column grid (max 900px width)	Single stack	⚠️ Routing poor scannability
Field Presentation	Icon + Label (bold) / Value structure	Label (bold): Value inline	⚠️ Routing lacks structure
Edit Buttons	Per-section with SubtleButtonStyle	Single "Edit" text	⚠️ Routing less granular
Read-Only Fields	ControlFillColorDisabledBrush background	Same appearance as editable	⚠️ Routing lacks distinction
Section Separators	MenuFlyoutSeparator	MenuFlyoutSeparator	✅ Same
🎯 Priority Fixes by Impact
CRITICAL (Breaks Visual Language)
Add CardBackgroundFillColorDefaultBrush to all input containers with 8px radius
Add FontIcon headers to every form field
Wrap Step1 PO input in accent-colored card matching Receiving's style
Redesign Step3 review as two-column layout with icons
HIGH (User Experience)
Add status indicator boxes (PO validation, recipient count)
Implement InfoBar for errors/warnings
Add icon + text to primary action buttons
Reduce padding from 24px to 20px in cards
MEDIUM (Polish)
Add accent header banner to Step 2 showing PO context
Differentiate read-only fields in Step 3
Wrap list items in individual card backgrounds
Add placeholder text to all inputs