# Clarification Questions - Analytics & Reporting

**Date**: January 21, 2026  
**Category**: Analytics & Business Intelligence  
**Priority**: Medium

---

## Overview

This document contains questions requiring clarification about analytics requirements, metrics tracking, reporting features, and data visualization for the MTM Waitlist Application.

---

## 1. Key Performance Indicators (KPIs)

### 1.1 Operational KPIs

**Question**: What operational metrics should be tracked?

**Proposed KPIs**:

**Task Metrics**:
- [ ] **Tasks Created** (count per hour/day/week)
- [ ] **Tasks Completed** (count per hour/day/week)
- [ ] **Tasks Pending** (current count)
- [ ] **Tasks In Progress** (current count)
- [ ] **Tasks Cancelled** (count + cancellation rate %)
- [ ] **Average Wait Time** (time from creation to assignment)
- [ ] **Average Completion Time** (time from start to complete)
- [ ] **Tasks Exceeding SLA** (count + percentage)

**Handler Metrics**:
- [ ] **Active Handlers** (currently clocked in/available)
- [ ] **Handlers Utilization** (% of time with assigned tasks)
- [ ] **Tasks Per Handler** (average per shift)
- [ ] **Completion Rate Per Handler** (completed / assigned)
- [ ] **Average Completion Time Per Handler**

**Zone Metrics**:
- [ ] **Tasks Per Zone** (distribution)
- [ ] **Wait Time Per Zone** (average)
- [ ] **Handlers Per Zone** (staffing levels)

**Need Clarification**:
- [ ] Which metrics are **most important** (top 5)?
- [ ] Should metrics be **real-time** or **historical**?
- [ ] Should metrics be **compared** to targets/goals?
- [ ] Should metrics trigger **alerts** (e.g., wait time > 30 min)?

**Impact**: Analytics service design, database schema, UI dashboards

---

### 1.2 Quality Metrics

**Question**: Should quality/accuracy metrics be tracked?

**Proposed Quality Metrics**:
- [ ] **Task Rework Rate** (tasks reassigned or redone)
- [ ] **Cancellation Rate** (by requestor? by handler? by system?)
- [ ] **Error Rate** (wrong part delivered, wrong quantity, etc.)
- [ ] **First-Time Completion Rate** (completed without reassignment)

**Need Clarification**:
- [ ] Are **quality metrics** required?
- [ ] How is **"error"** defined?
- [ ] Should errors be **categorized** (wrong part, damaged, late, etc.)?
- [ ] Should handlers be **rated** on quality (not just speed)?

**Impact**: Data collection requirements, handler feedback

---

## 2. Reporting Requirements

### 2.1 Standard Reports

**Question**: What standard reports are needed?

**Proposed Reports**:

**Daily Report**:
```
Daily Waitlist Summary - January 21, 2026
========================================
Tasks Created:     45
Tasks Completed:   38
Tasks Cancelled:   2
Tasks Pending:     5

Average Wait Time: 12 minutes
Average Completion Time: 18 minutes
Tasks Exceeding SLA: 3 (6.7%)

Top Requestors:
1. John Doe (Operator) - 8 requests
2. Jane Smith (Operator) - 6 requests

Top Handlers:
1. Mike Johnson (MH) - 12 completed
2. Sarah Brown (MH) - 10 completed

Busiest Zones:
1. Zone 3 - 15 tasks
2. Zone 1 - 12 tasks
```

**Weekly Report**:
```
Weekly Waitlist Summary - Week of Jan 15-21, 2026
================================================
Total Tasks: 215
Completed: 190 (88.4%)
Cancelled: 10 (4.7%)
Pending: 15 (7.0%)

Average Wait Time Trend:
Mon: 10 min, Tue: 12 min, Wed: 15 min, Thu: 18 min, Fri: 14 min

Category Breakdown:
Material Handler: 150 (69.8%)
Setup Tech: 40 (18.6%)
Quality: 25 (11.6%)

Top Performing Handlers: [list]
Zones Needing Attention: [list based on bottlenecks]
```

**Need Clarification**:
- [ ] What **time periods** should reports cover (daily, weekly, monthly)?
- [ ] Should reports be **auto-generated** (scheduled)?
- [ ] Should reports be **emailed** (to whom)?
- [ ] What **format** for reports (PDF, Excel, CSV, HTML)?
- [ ] Should reports include **charts/graphs**?
- [ ] Can users **customize reports** (choose metrics, date range)?

**Impact**: Reporting service, export functionality

---

### 2.2 Ad-Hoc Reporting

**Question**: Should users be able to create custom reports?

**Proposed Ad-Hoc Features**:
- [ ] **Date range selector** (from/to dates)
- [ ] **Metric selector** (choose which KPIs to include)
- [ ] **Filter by zone, category, handler, requestor**
- [ ] **Group by** (day, week, month, zone, category)
- [ ] **Export** (CSV, Excel, PDF)

**Need Clarification**:
- [ ] Who can **create ad-hoc reports** (all users? leads only?)?
- [ ] Should ad-hoc reports be **saved** (for reuse)?
- [ ] Should ad-hoc reports be **scheduled** (run automatically)?
- [ ] What **data retention** for ad-hoc queries (query last 6 months? 1 year?)?

**Impact**: Reporting UI complexity, query performance

---

## 3. Data Visualization

### 3.1 Chart Types

**Question**: What types of charts/graphs are needed?

**Proposed Charts**:

**Dashboard Charts**:
- [ ] **Line Chart**: Wait time trend over time
- [ ] **Bar Chart**: Tasks per category, tasks per zone
- [ ] **Pie Chart**: Task distribution by status, by category
- [ ] **Gauge**: Current vs target SLA compliance
- [ ] **Heat Map**: Busiest zones by hour
- [ ] **Sparklines**: Inline trends (7-day wait time mini-chart)

**Example Dashboard Layout**:
```
+------------------------------------------------+
| Tasks Created Today: 45    Completed: 38      |
| Pending: 5                 Avg Wait: 12 min   |
+------------------------------------------------+
| [Line Chart: Wait Time Trend (Last 7 Days)]   |
+------------------------------------------------+
| [Bar Chart: Tasks by Category]                |
| [Pie Chart: Tasks by Status]                  |
+------------------------------------------------+
```

**Need Clarification**:
- [ ] Which **chart types** are preferred?
- [ ] Should charts be **interactive** (click to drill down)?
- [ ] Should charts be **real-time** or **static snapshots**?
- [ ] Should charts be **exportable** (as images)?
- [ ] What **charting library** to use (LiveCharts, SyncFusion, custom)?

**Impact**: Charting library choice, UI complexity

---

### 3.2 Dashboard Customization

**Question**: Should users be able to customize their dashboard?

**Proposed Customization**:
- [ ] **Choose widgets** (select which KPIs/charts to show)
- [ ] **Arrange widgets** (drag-and-drop layout)
- [ ] **Widget size** (resize charts)
- [ ] **Save layouts** (per user)
- [ ] **Default layouts** (per role)

**Need Clarification**:
- [ ] Should dashboards be **customizable**?
- [ ] Should customization be **per-user** or **per-role**?
- [ ] Should there be **predefined templates** (Operator Dashboard, Lead Dashboard)?
- [ ] Can users **share dashboards** (export/import layouts)?

**Impact**: Dashboard framework complexity

---

## 4. Real-Time Analytics

### 4.1 Live Updates

**Question**: Should analytics update in real-time?

**Proposed Real-Time Features**:
- [ ] **Live task count** (updates as tasks created/completed)
- [ ] **Live wait time** (recalculates every minute)
- [ ] **Live handler status** (shows who's working on what)
- [ ] **Live alerts** (SLA breach notification)

**Need Clarification**:
- [ ] Should analytics be **real-time** (SignalR push) or **polled** (refresh every N seconds)?
- [ ] What **refresh rate** (every 5 sec? 30 sec? 1 min?)?
- [ ] Should real-time be **pausable** (when user is analyzing data)?
- [ ] Should there be **visual indicator** of refresh (spinner, timestamp)?

**Impact**: SignalR infrastructure, server load, battery usage (if mobile)

---

### 4.2 Alerts & Notifications

**Question**: Should analytics trigger alerts?

**Proposed Alerts**:
```
Alert: Wait Time Exceeding SLA
- Threshold: Tasks waiting > 30 minutes
- Notification: Desktop toast, banner
- Recipients: Production Lead, Material Handler Lead

Alert: Handler Utilization Low
- Threshold: Handlers < 50% utilized
- Notification: Email to Production Lead
- Action: Consider reassigning zones

Alert: Task Backlog
- Threshold: > 10 pending tasks in a zone
- Notification: Desktop toast to handlers
- Action: Prompt handlers to claim tasks
```

**Need Clarification**:
- [ ] What **alert conditions** are needed?
- [ ] What **notification methods** (toast, email, SMS, in-app)?
- [ ] Who **receives alerts** (role-based)?
- [ ] Should alerts be **configurable** (admin sets thresholds)?
- [ ] Should alerts be **suppressible** (snooze, dismiss)?
- [ ] Should alert history be **tracked**?

**Impact**: Alerting service, notification infrastructure

---

## 5. Trend Analysis

### 5.1 Historical Trends

**Question**: What historical trends should be analyzed?

**Proposed Trend Views**:

**Wait Time Trend**:
```
Wait Time Over Time (Last 30 Days)
[Line Chart showing daily average wait time]

Insight: Wait time increased 15% in Week 3
Possible cause: Handler absenteeism, increased demand
```

**Volume Trend**:
```
Task Volume by Category (Last 90 Days)
[Stacked Area Chart showing category volumes]

Insight: Material Handler requests increased by 20%
Setup Tech requests decreased by 10%
```

**Handler Performance Trend**:
```
Handler Completion Rate (Last 60 Days)
[Line Chart per handler showing completion rate over time]

Insight: Mike Johnson's completion rate improved from 85% to 95%
```

**Need Clarification**:
- [ ] What **time periods** for trends (7 days? 30 days? 90 days? custom?)?
- [ ] Should trends include **anomaly detection** (highlight outliers)?
- [ ] Should trends include **forecasting** (predict next week's volume)?
- [ ] Should trends include **comparisons** (this week vs last week)?

**Impact**: Analytics complexity, data science features

---

### 5.2 Predictive Analytics

**Question**: Should the system predict future demand?

**Proposed Predictive Features**:
- [ ] **Demand Forecast**: Predict task volume for next shift/day/week
- [ ] **Staffing Recommendations**: Suggest handler count per zone
- [ ] **Bottleneck Prediction**: Identify zones likely to have delays
- [ ] **SLA Risk Score**: Predict likelihood of SLA breach

**Need Clarification**:
- [ ] Are **predictive analytics** required for MVP?
- [ ] Should forecasts be **simple** (moving average) or **advanced** (ML models)?
- [ ] Should forecasts be **displayed to users** (or just admins)?
- [ ] How **accurate** should forecasts be (acceptable error margin)?

**Impact**: Data science complexity, machine learning infrastructure

---

## 6. Handler Performance Analytics

### 6.1 Individual Performance

**Question**: What individual handler metrics should be tracked?

**Proposed Handler Dashboard**:
```
Mike Johnson (Material Handler)
Today: 8 tasks completed, 2 in progress, 0 cancelled
Average Completion Time: 15 minutes (vs team avg: 18 min)
Completion Rate: 100% (all assigned tasks completed)
On-Time Rate: 87.5% (7 of 8 completed within SLA)

This Week:
35 tasks completed
Average: 16 minutes per task
Top Zone: Zone 3 (15 tasks)

Performance Trend:
[Line Chart showing completion time over last 30 days]
```

**Need Clarification**:
- [ ] Should handlers **see their own performance**?
- [ ] Should handlers **see peer comparison** (ranked? anonymized?)?
- [ ] Should performance metrics affect **incentives/bonuses**?
- [ ] Should performance be **reviewed** (by leads, in 1-on-1s)?

**Impact**: Handler motivation, competitive culture, privacy

---

### 6.2 Team Performance

**Question**: Should team-level analytics be provided?

**Proposed Team Dashboard** (for Material Handler Lead):
```
Material Handler Team Performance
Active Handlers: 5 of 8 (3 offline/break)
Tasks In Progress: 10
Tasks Pending: 3

Team Average Completion Time: 18 minutes
Top Performer: Mike Johnson (15 min avg)
Needs Support: Sarah Brown (25 min avg, new hire)

Zone Coverage:
Zone 1: 2 handlers (adequate)
Zone 2: 1 handler (understaffed)
Zone 3: 2 handlers (adequate)
```

**Need Clarification**:
- [ ] Should leads see **individual handler metrics**?
- [ ] Should leads see **team rankings** (sorted by performance)?
- [ ] Should leads be able to **drill down** (click handler to see details)?
- [ ] Should team metrics be **compared** to other teams?

**Impact**: Lead dashboard complexity, cross-team visibility

---

## 7. Export & Integration

### 7.1 Data Export

**Question**: What export formats are needed?

**Proposed Export Options**:
- [ ] **CSV**: For Excel analysis, data manipulation
- [ ] **Excel (.xlsx)**: With formatting, charts, multiple sheets
- [ ] **PDF**: For printing, archiving, sharing
- [ ] **JSON**: For API integration, data transfer
- [ ] **Image (PNG/JPG)**: For charts, dashboards

**Need Clarification**:
- [ ] Which **export formats** are required?
- [ ] Should exports be **scheduled** (auto-send daily report)?
- [ ] Should exports be **compressed** (ZIP for large datasets)?
- [ ] Where should exports be **saved** (local disk? network share? email?)?

**Impact**: Export service design

---

### 7.2 External Integration

**Question**: Should analytics data be accessible via API or external tools?

**Proposed Integration**:
- [ ] **REST API**: Expose analytics endpoints for external tools
- [ ] **Power BI**: Connect Power BI to MySQL for custom dashboards
- [ ] **Excel**: Live data connection (ODBC) for pivot tables
- [ ] **Data Warehouse**: ETL to separate analytics database

**Need Clarification**:
- [ ] Should there be a **public API** for analytics data?
- [ ] Should analytics data be **exported to data warehouse** (separate from operational DB)?
- [ ] Should **Power BI** or other BI tools be used (instead of building custom dashboards)?
- [ ] What **security** for external access (API keys, OAuth)?

**Impact**: API design, integration complexity

---

## 8. Reusable Components from MTM Receiving

### 8.1 CSV Export Service

**Question**: Can we reuse MTM Receiving's CSV export functionality?

**MTM Receiving Has**:
- `Service_CSVExport`
- Exports data to CSV files
- Customizable column selection

**Need Clarification**:
- [ ] Should we **reuse as-is** or **adapt**?
- [ ] What **differences** for Waitlist app exports?
- [ ] Should we **extract to shared library**?

**Impact**: Code reuse, development time

---

### 8.2 Reporting Infrastructure

**Question**: Does MTM Receiving have reporting infrastructure we can reuse?

**Need Clarification**:
- [ ] Does MTM Receiving have **report templates**?
- [ ] Does MTM Receiving have **scheduled reports**?
- [ ] Can we reuse **charting components**?

**Impact**: Development effort, consistency

---

## 9. Analytics Performance

### 9.1 Query Optimization

**Question**: How should analytics queries be optimized?

**Proposed Strategies**:
- [ ] **Indexed Queries**: Ensure all analytics queries use indexes
- [ ] **Materialized Views**: Pre-aggregate common metrics
- [ ] **Caching**: Cache expensive analytics queries (Redis)
- [ ] **Batch Processing**: Run heavy analytics nightly (off-peak)

**Need Clarification**:
- [ ] What is the **acceptable query time** for analytics (< 1 sec? < 5 sec?)?
- [ ] Should analytics queries run on **separate read replica** (avoid impacting operational DB)?
- [ ] Should **expensive reports** run asynchronously (background job)?
- [ ] Should users see **loading indicators** for slow queries?

**Impact**: Database design, user experience

---

### 9.2 Data Retention

**Question**: How long should analytics data be retained?

**Proposed Retention Policy**:
```
Operational Data (tasks, assignments):
- Last 90 days: Online (MySQL, full detail)
- 91-365 days: Archived (compressed, queryable)
- > 1 year: Cold storage (S3, rarely accessed)

Aggregated Metrics:
- Last 365 days: Online (daily summaries)
- > 1 year: Archived (monthly summaries)

Audit Logs:
- Last 2 years: Online
- > 2 years: Archived (compliance requirement)
```

**Need Clarification**:
- [ ] What is the **retention period** for operational data?
- [ ] What is the **retention period** for aggregated metrics?
- [ ] Should old data be **deleted** or **archived**?
- [ ] What is the **archival strategy** (separate database? file storage?)?

**Impact**: Storage costs, query performance, compliance

---

## Action Items

### Critical (Before Analytics Development)
1. [ ] Define top 5-10 KPIs to track
2. [ ] Define standard reports (daily, weekly, monthly)
3. [ ] Define dashboard layouts for each role
4. [ ] Define real-time vs batch analytics

### High Priority (Before Alpha)
5. [ ] Choose charting library
6. [ ] Define export formats (CSV, Excel, PDF)
7. [ ] Define alert conditions and notification methods
8. [ ] Define handler performance metrics (visibility rules)

### Medium Priority (Before Beta)
9. [ ] Implement trend analysis (historical views)
10. [ ] Implement ad-hoc reporting (custom queries)
11. [ ] Implement dashboard customization (if needed)
12. [ ] Optimize analytics queries (indexes, caching)

### Low Priority (Before Production)
13. [ ] Predictive analytics (if needed)
14. [ ] External integrations (API, Power BI)
15. [ ] Data retention and archival
16. [ ] Advanced visualizations

---

## Next Steps

1. **Analytics Requirements Meeting**: Review KPIs with stakeholders
2. **Create Metrics Dictionary**: Define each KPI precisely
3. **Design Dashboard Mockups**: Visual designs for each role's dashboard
4. **Prototype Charts**: Test charting library with sample data
5. **Develop Analytics Service**: Implement core analytics queries
6. **Test Performance**: Load test with realistic data volumes
7. **User Acceptance Testing**: Validate dashboards with actual users

---

**Document Owner**: Business Analyst / Product Owner  
**Review Date**: [To Be Scheduled]  
**Status**: Pending Stakeholder Input
