# MTM Waitlist Questionnaire System

## Overview

This directory contains a wizard-based questionnaire system for gathering requirements for the MTM Waitlist Application. The system supports both digital (web-based wizard) and printable (markdown) formats.

---

## File Structure

```
HTML/
├── wizard.html                      # Main wizard interface (loads JSON data)
├── data/
│   ├── core-all-roles-v2.json      # Questions for all roles
│   ├── operator.json               # Operator-specific questions
│   ├── material-handler.json       # Material Handler questions
│   ├── material-handler-lead.json  # Material Handler Lead questions
│   ├── production-lead.json        # Production Lead questions
│   ├── quality.json                # Quality Control questions
│   ├── setup-tech.json             # Setup Technician questions
│   ├── inventory-specialist.json   # Inventory Specialist questions
│   ├── outside-service.json        # Outside Service Coordinator questions
│   ├── it-department.json          # IT Department questions
│   ├── die-shop.json               # Die Shop questions
│   ├── fabrication-welding.json    # Fabrication & Welding questions
│   └── assembly.json               # Assembly questions
└── README.md                        # This file

PRINTABLE-*.md                       # Markdown versions for printing
```

---

## Usage

### Web-Based Wizard

1. **Open the wizard:**
   ```
   wizard.html?data=data/core-all-roles-v2.json
   ```

2. **Navigate through questions:**
   - Click "Next" to move forward
   - Click "Previous" to go back
   - Progress bar shows current position
   - Answers auto-save to browser localStorage every 30 seconds

3. **Review answers:**
   - On last question, click "Review Answers"
   - Edit any answer by clicking "Edit Answer"

4. **Export:**
   - Click "Finish & Export"
   - Downloads JSON file to your computer
   - Sends data to server (if configured)

### Printable Markdown

For users without computer access:

1. Open `PRINTABLE-Core-All-Roles.md` (or role-specific version)
2. Print or convert to PDF
3. Fill out by hand
4. Manually enter responses later

---

## Question Types Supported

The wizard supports multiple question types with custom styling:

### 1. **Radio (Single Choice)**
```json
{
    "type": "radio",
    "question": "How should people log in?",
    "options": [
        "Windows/AD",
        "Username and password",
        "Hybrid"
    ],
    "allowOther": true
}
```
- Large clickable options
- Visual selection feedback
- Optional "Other" field

### 2. **Checkbox (Multiple Choice)**
```json
{
    "type": "checkbox",
    "question": "What fields are required?",
    "options": [
        {"value": "work_order", "label": "Work order number"},
        {"value": "part_number", "label": "Part number"}
    ],
    "allowOther": true
}
```
- Can select multiple options
- Visual selection indicators
- Optional "Other" field

### 3. **Dropdown**
```json
{
    "type": "dropdown",
    "question": "Your department?",
    "options": ["Production", "Material Handling", "Quality"]
}
```
- Compact for long lists
- Good for known values

### 4. **Text Input**
```json
{
    "type": "text",
    "question": "Your name?",
    "placeholder": "John Doe"
}
```

### 5. **Number Input**
```json
{
    "type": "number",
    "question": "How many recent requests?",
    "min": 5,
    "max": 50,
    "placeholder": "10"
}
```

### 6. **Date Input**
```json
{
    "type": "date",
    "question": "Completion date?"
}
```

### 7. **Textarea (Long Text)**
```json
{
    "type": "textarea",
    "question": "What works with tablesready.com?",
    "placeholder": "List features...",
    "rows": 4
}
```

### 8. **Range Slider**
```json
{
    "type": "range",
    "question": "Auto-logout time (minutes)?",
    "min": 1,
    "max": 60,
    "default": 5,
    "minLabel": "1 min",
    "maxLabel": "60 min"
}
```
- Visual slider with value display
- Great for numeric ranges

### 9. **Rating (Stars)**
```json
{
    "type": "rating",
    "question": "How important is this feature?"
}
```
- 5-star rating system
- Interactive stars

### 10. **Matrix (Grid)**
```json
{
    "type": "matrix",
    "question": "Rate each feature",
    "rows": ["Feature A", "Feature B"],
    "columns": ["Poor", "Fair", "Good", "Excellent"]
}
```
- Compare multiple items across same scale
- Radio buttons in grid format

### 11. **List (Dynamic Add/Remove)**
```json
{
    "type": "list",
    "question": "List all the issues you've encountered"
}
```
- Start with one item
- Add/remove items dynamically
- Good for unknown quantity responses

---

## Additional Features

### Info Boxes

Add context to questions:

```json
{
    "question": "...",
    "whyMatters": "Explains the business impact of this decision",
    "example": "Concrete example from your facility",
    "note": "Important reminder or constraint"
}
```

### Decision Tracking

Track who made decisions:

```json
{
    "question": "...",
    "requiresDecision": true
}
```
Adds fields for:
- Decided By (name/role)
- Decision Date

---

## Customization

### Update Export Destination

1. Open `wizard.html`
2. Find: `%%EXPORT_DESTINATION%%`
3. Replace with your endpoint:
   ```javascript
   const exportEndpoint = 'https://your-server.com/api/answers';
   ```

### Create New Questionnaire

1. Create JSON file in `data/` folder
2. Follow structure from `core-all-roles-v2.json`
3. Open wizard: `wizard.html?data=data/your-file.json`
4. Create matching printable markdown

---

## JSON Structure

```json
{
    "title": "Display title",
    "subtitle": "Subtitle text",
    "instructions": "How to answer",
    "questions": [
        {
            "id": "unique_id",
            "type": "radio|checkbox|dropdown|text|number|date|textarea|range|rating|matrix|list",
            "question": "The question text",
            "description": "Optional description",
            "whyMatters": "Optional business context",
            "example": "Optional example",
            "note": "Optional important note",
            "options": [], // For radio, checkbox, dropdown
            "allowOther": true, // For radio, checkbox
            "requiresDecision": true, // Adds decision tracking
            "placeholder": "...", // For text inputs
            "min": 1, // For number, range
            "max": 100, // For number, range
            "default": 50, // For range
            "minLabel": "Low", // For range
            "maxLabel": "High", // For range
            "rows": 4, // For textarea
            "columns": [], // For matrix
            "rows": [] // For matrix (different context than textarea)
        }
    ]
}
```

---

## Export Format

Exported data includes:

```json
{
    "questionnaire": "data/core-all-roles-v2.json",
    "submittedAt": "2026-01-21T10:30:00.000Z",
    "answers": {
        "question_id": {
            "questionId": "unique_id",
            "question": "The question text",
            "value": "User's answer",
            "decidedBy": "John Smith",
            "decisionDate": "2026-01-21"
        }
    }
}
```

---

## Features

✅ **Wizard Navigation** - Step through questions one at a time  
✅ **Progress Bar** - Visual indication of completion  
✅ **Auto-Save** - Saves to localStorage every 30 seconds  
✅ **Review Page** - See all answers before submitting  
✅ **Edit Answers** - Jump back to any question from review  
✅ **Dual Export** - Downloads JSON + posts to server  
✅ **Print-Friendly** - Markdown versions for offline use  
✅ **Custom Styling** - Each question type has unique design  
✅ **Responsive** - Works on desktop, tablet, kiosk  
✅ **No Dependencies** - Pure HTML/CSS/JavaScript  

---

## Browser Support

- Chrome 90+
- Edge 90+
- Firefox 88+
- Safari 14+

---

## Development

### Add New Question Type

1. Add rendering function in `wizard.html`:
   ```javascript
   function renderMyType(q, index) {
       return `<div>Custom HTML</div>`;
   }
   ```

2. Add case to `renderQuestion()`:
   ```javascript
   case 'mytype':
       html += renderMyType(q, index);
       break;
   ```

3. Add save/restore logic in `saveAnswer()` and `restoreAnswer()`

4. Add custom CSS for `.question-type-mytype`

---

## Creating Printable Markdown from JSON

Use this template structure:

```markdown
### Question Number. Question Text

**Question:** [description]

**Options:**
- ☐ Option 1
- ☐ Option 2

> **Why This Matters:**  
> [whyMatters text]

**Your Answer:**

```

_____________________________________________________________________________
```

**Decided By:** _________________________ **Date:** _____________
```

---

## Tips

1. **Keep questions focused** - One concept per question
2. **Use appropriate types** - Radio for single choice, checkbox for multiple
3. **Add context** - Use whyMatters and example liberally
4. **Test the flow** - Navigate through entire wizard before deploying
5. **Test auto-save** - Refresh mid-questionnaire to verify restore works
6. **Consider mobile** - Some users may access on tablets or mobile carts

---

## Troubleshooting

**Questions don't load:**
- Check JSON syntax with a validator
- Verify file path in URL parameter
- Check browser console for errors

**Answers don't save:**
- Check localStorage quota (5-10MB limit)
- Verify browser allows localStorage
- Check for JavaScript errors

**Export doesn't work:**
- Replace `%%EXPORT_DESTINATION%%` placeholder
- Check network connectivity
- Verify server endpoint is accessible

---

## Future Enhancements

- [ ] Multi-language support
- [ ] Conditional questions (skip logic)
- [ ] File upload question type
- [ ] Signature capture
- [ ] PDF export (in addition to JSON)
- [ ] Email notification on completion
- [ ] Admin dashboard for reviewing submissions
- [ ] Analytics on question completion rates

---

**Questions or Issues?**  
Contact IT Department or Project Lead
