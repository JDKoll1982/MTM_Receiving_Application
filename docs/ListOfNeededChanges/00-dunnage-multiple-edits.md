## Dunnage Type Page

- Remove the Next button as it's not needed

## Dunnage Mode Selection Page

- Add the clear all labels button to the bottom of this page the same way it appears in Dunnage Type selection page

## Dunnage - Select Part Page

- Part Details should include the specs for the selected Part ID

## Dunnage Select Part - Edit Part ID & New Part ID Modal

- Apply the same spec-driven cleanup to Quick Add Part as well, if you want both dialogs to behave consistently.
- Allow the user to edit the name of the Part ID instead of it being auto generated
- Fix the Dunnage Type not showing under the Dunnage Type Label (next to the Part ID)
- Instead of the Width, Height, Depth fields add the option to set the Inventory Type as there is currently no way to set this
- Increase the Width of both Edit and New Part ID modals
- Set the number of columns from 1 to 2 to allow for a neater UI for the user
- All changes must be made to both Edit Part ID and New Part ID Modal

## Dunnage - Enter Loads Page

- Remove the Tip card to conserve space
- Only textboxes/numboxes and buttons should have tab indexing
- Place the Number of Loads and the Quantity per loads cards into 1 unified card
- Place the Type and Part text on the same row in the summary card

## Dunnage - Enter Details Page

- Remove the Note card to conserve space
- Remove the Review modal that appears when clicking next, just go to the next screen if all validation passes

## Dunnage - Review & Save Page

- Add the back button to the bottom row (where the Next and Back buttons go on other pages) allowing the user to return to Dunnage - Enter Details if needed
- When saving, remove the start new entry button from the toast text
- After successfully saving, show a modal dialog stating as much with the option to start a new entry or go back to the Dunnage Mode Selection Page
- If the user selects to start a new entry, return to the dunnage type selection page
- If the user selects the mode select, go back to the dunnage mode selection page
