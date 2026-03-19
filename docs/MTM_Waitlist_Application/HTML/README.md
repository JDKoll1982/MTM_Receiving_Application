# MTM Waitlist Questionnaire System

## Overview

Dynamic questionnaire system that loads department-specific questions based on user's department selection.

## Architecture

### Files Structure

```
HTML/
├── questions/                    # JSON question definitions
│   ├── core.json                # Common questions for all departments
│   ├── production.json          # Production-specific questions  
│   ├── material-handling.json   # Material Handling + Forklift questions
│   ├── it-department.json       # IT-specific questions
│   └── management.json          # Management/Leadership questions
└── generated/
    └── questionnaire.html       # Standalone HTML questionnaire
```

## ✅ COMPLETED

### Core Functionality
- ✅ Dynamic JSON loading from `questions/` folder
- ✅ Department-specific question routing **WORKING**
- ✅ 5 JSON files created (core + 4 departments)
- ✅ Standalone HTML file (no PowerShell generation needed)
- ✅ Navigation buttons at top
- ✅ Clean, working interface
- ✅ Async loading fixed and tested

### Department Files Created
- ✅ `core.json` - Base questions for all users (8 questions)
- ✅ `production.json` - 9 production-specific questions
- ✅ `material-handling.json` - 12 questions including forklift inspections
- ✅ `maintenance.json` - 9 maintenance-specific questions
- ✅ `quality-control.json` - 9 QC/inspection questions
- ✅ `die-shop.json` - 9 die shop specific questions
- ✅ `fabrication-welding.json` - 9 fab/welding questions
- ✅ `assembly.json` - 9 assembly line questions
- ✅ `setup-technicians.json` - 9 machine setup questions
- ✅ `it-department.json` - 4 IT administration questions
- ✅ `management.json` - 6 management/reporting questions

**All 11 JSON files created!** Each department now has unique questions.

### Testing Results
- Material Handling: Loads 12 dept questions → Total 20 questions ✅
- Production: Loads 9 dept questions → Total 17 questions ✅
- Maintenance: Loads 9 dept questions → Total 17 questions ✅
- Quality Control: Loads 9 dept questions → Total 17 questions ✅
- Die Shop: Loads 9 dept questions → Total 17 questions ✅
- Fabrication & Welding: Loads 9 dept questions → Total 17 questions ✅
- Assembly: Loads 9 dept questions → Total 17 questions ✅
- Setup Technicians: Loads 9 dept questions → Total 17 questions ✅
- IT Department: Loads 4 dept questions → Total 12 questions ✅
- Management: Loads 6 dept questions → Total 14 questions ✅
- Department questions load correctly after Shift question ✅
- Each department sees ONLY their own questions ✅

## 📋 TODO (Waiting for Management Guidelines)

Create department-specific JSON files for:
- Quality Control
- Die Shop
- Fabrication & Welding
- Assembly
- Maintenance
- Setup Technicians
- Inventory Specialist
- Outside Service Coordinator

**Current temporary mapping:**
- Most departments → `production.json`
- Maintenance → `material-handling.json`
- Office roles → `management.json`

## Usage

### For End Users (Simple - No Server Required!)
1. Double-click `generated/questionnaire.html` to open in browser
2. Fill out the questionnaire
3. Export results to JSON

**No installation, no server, no technical setup required!**

### For Developers

#### Making Changes to Questions
1. Edit JSON files in `questions/` folder
2. Run the embedding script:

   ```powershell
   .\Embed-JSONData.ps1
   ```

3. Open `questionnaire.html` to test

#### Two Modes
- **Embedded Mode** (default): JSON data embedded in HTML - works offline, no CORS issues
- **Fetch Mode**: Loads JSON from files - requires HTTP server but easier to edit/test

To use Fetch Mode during development:

```powershell
python -m http.server 8080
# Open: http://localhost:8080/generated/questionnaire.html
```

#### Adding New Department
1. Create `questions/department-name.json`
2. Follow format from existing files
3. Run `.\Embed-JSONData.ps1` to embed the new data
4. Update `departmentMap` in `questionnaire.html` if needed

## Question Types Supported

- `text` - Single-line text
- `textarea` - Multi-line text
- `radio` - Single choice
- `checkbox` - Multiple choice
- `dropdown` - Select from list
- `number` - Numeric with min/max
- `range` - Slider
- `rating` - 5-star rating
- `date` - Date picker

## Export Format

Questionnaires export to JSON with all answers and metadata.

## Next Steps

1. Debug department question loading (async timing issue)
2. Get management guidelines for remaining departments
3. Create missing department JSON files
4. Test with real users
5. Deploy to production
