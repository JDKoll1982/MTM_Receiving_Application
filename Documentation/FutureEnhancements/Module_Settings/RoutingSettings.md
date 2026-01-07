# Routing Module - Configurable Settings Guide

**Module:** Module_Routing  
**Generated:** 2026-01-06  
**Purpose:** Document all hardcoded values that should be user-configurable

---

## Overview

This document catalogs all hardcoded configuration values discovered in the Routing Module. These values should be moved to a dedicated Settings page within the application, allowing users to customize module behavior without code changes.

**Total Settings Discovered:** 15

---

## 1. CSV Export Configuration

### 1.1 CSV File Paths
- **Current Location:** `appsettings.json` → `RoutingModule:CsvExportPath:Network`
- **Default Value:** (varies by deployment)
- **Description:** Network share path for CSV export (primary location)
- **Recommendation:** Expose in Settings UI with file browser button
- **Validation:** Must be valid UNC path or local path, directory must be writable

### 1.2 CSV Fallback Path
- **Current Location:** `appsettings.json` → `RoutingModule:CsvExportPath:Local`
- **Default Value:** (varies by deployment)
- **Description:** Local fallback path when network path is unavailable
- **Recommendation:** Expose in Settings UI with file browser button
- **Validation:** Must be valid local path, directory must be writable

### 1.3 CSV Retry Max Attempts
- **Current Location:** `Services/RoutingService.cs:234` (hardcoded fallback)
- **Default Value:** `3`
- **Description:** Maximum retry attempts for CSV file writes
- **Recommendation:** Numeric input, range 1-10
- **Business Rule:** Higher values increase success rate but slow down failures

### 1.4 CSV Retry Delay (milliseconds)
- **Current Location:** `Services/RoutingService.cs:235` (hardcoded fallback)
- **Default Value:** `500` ms
- **Description:** Delay between CSV write retry attempts
- **Recommendation:** Numeric input, range 100-5000 ms
- **Business Rule:** Balance between immediate retry and allowing file locks to release

---

## 2. Duplicate Detection Settings

### 2.1 Duplicate Detection Time Window
- **Current Location:** `Data/Dao_RoutingLabel.cs:298` (hardcoded)
- **Current Value:** `24` hours
- **Description:** Time window for checking duplicate labels (same PO/Line/Recipient)
- **Recommendation:** Numeric input, range 1-168 hours (1 week max)
- **Business Rule:** Prevents accidental duplicate labels within configurable timeframe
- **Impact:** Shorter window = more lenient, longer window = stricter duplicate prevention

---

## 3. User Interface Settings

### 3.1 Quick Add Recipient Count
- **Current Location:** `Services/RoutingRecipientService.cs` (method parameter)
- **Current Value:** `5` recipients
- **Description:** Number of top recipients shown as Quick Add buttons
- **Recommendation:** Numeric input, range 3-10
- **Business Rule:** Balance between convenience (more buttons) and screen space

### 3.2 Recipient List Page Size
- **Current Location:** `Services/RoutingService.cs:185` (method parameter)
- **Current Value:** `limit=100`
- **Description:** Default pagination size for label lists
- **Recommendation:** Dropdown [25, 50, 100, 200, 500]
- **Business Rule:** Larger pages load slower but reduce paging

### 3.3 Search Debounce Delay
- **Current Location:** Not implemented (should be)
- **Recommended Value:** `300` ms
- **Description:** Delay before applying search filter during typing
- **Recommendation:** Numeric input, range 100-1000 ms
- **Business Rule:** Prevents excessive filtering on every keystroke

---

## 4. Infor Visual Integration Settings

### 4.1 Site Reference Code
- **Current Location:** `Data/Dao_InforVisualPO.cs` (hardcoded in SQL queries)
- **Current Value:** `'002'`
- **Description:** Warehouse/site code for Infor Visual queries
- **Recommendation:** Text input, 3 characters max
- **Validation:** Must match valid site_ref in Infor Visual database
- **Impact:** Critical - incorrect value returns no PO data

### 4.2 PO Valid Statuses
- **Current Location:** `Data/Dao_InforVisualPO.cs:49` (hardcoded in SQL)
- **Current Value:** `'O', 'P'` (Open, Partial)
- **Description:** PO status codes considered "valid" for routing
- **Recommendation:** Multi-select checkboxes with descriptions
- **Business Rule:** Controls which POs users can create labels for

### 4.3 Connection Timeout
- **Current Location:** Connection string (implicit)
- **Recommended Value:** `15` seconds
- **Description:** Maximum time to wait for Infor Visual connection
- **Recommendation:** Numeric input, range 5-60 seconds
- **Business Rule:** Balance between network delays and user frustration

---

## 5. Session & Security Settings

### 5.1 Default Employee Number (REMOVE IN PRODUCTION)
- **Current Location:** Multiple ViewModels
- **Current Value:** `6229` (hardcoded placeholder)
- **Description:** **CRITICAL: Must be removed before production deployment**
- **Recommendation:** Replace with ISessionService integration
- **Security Risk:** All actions logged as same user if not removed

### 5.2 CSV Path Validation Base Directory
- **Current Location:** Not implemented (should be)
- **Recommended Value:** Application base directory or designated exports folder
- **Description:** Root directory for CSV path security validation
- **Recommendation:** Path display (read-only), set during installation
- **Security Rule:** Prevents path traversal attacks (CSV export outside allowed directory)

---

## 6. Performance & Caching

### 6.1 Recipient Cache Lifetime
- **Current Location:** Not implemented (should be)
- **Recommended Value:** `300` seconds (5 minutes)
- **Description:** How long to cache recipient list before refreshing
- **Recommendation:** Numeric input, range 60-3600 seconds
- **Business Rule:** Longer cache = faster UI, but stale data

### 6.2 PO Line Cache Lifetime
- **Current Location:** Not implemented (should be)
- **Recommended Value:** `60` seconds (1 minute)
- **Description:** How long to cache PO lines from Infor Visual
- **Recommendation:** Numeric input, range 30-600 seconds
- **Business Rule:** Shorter cache = more accurate inventory data

---

## 7. History & Audit Settings

### 7.1 Label History Retention Days
- **Current Location:** Not implemented (should be)
- **Recommended Value:** `365` days (1 year)
- **Description:** How long to keep label edit history before archival
- **Recommendation:** Numeric input, range 90-1825 days (5 years)
- **Business Rule:** Compliance requirement may dictate minimum retention

---

## Implementation Priority

### High Priority (Blocks Production)
1. **CSV Export Paths** - Must be configurable per deployment
2. **Site Reference Code** - Must match actual Infor Visual site
3. **Default Employee Number** - Must remove hardcoded placeholder

### Medium Priority (Improves UX)
4. **Duplicate Detection Window** - Business rule varies by customer
5. **Quick Add Recipient Count** - User preference
6. **CSV Retry Policy** - Network reliability varies

### Low Priority (Nice to Have)
7. **Pagination Settings** - Power users may want customization
8. **Cache Lifetimes** - Performance tuning
9. **Search Debounce** - Minor UX improvement

---

## Proposed Settings UI

### Tab: "CSV Export"
- Network Path: `[_______________] [Browse...]`
- Local Fallback Path: `[_______________] [Browse...]`
- Retry Attempts: `[3▼]` (1-10)
- Retry Delay (ms): `[500]` (100-5000)
- **[Test Paths]** button - validates both paths are writable

### Tab: "Duplicate Detection"
- Time Window (hours): `[24]` (1-168)
- **Info:** "Prevents creating duplicate labels within this timeframe for same PO/Line/Recipient"

### Tab: "User Interface"
- Quick Add Buttons: `[5]` (3-10)
- Default Page Size: `[100▼]` (25/50/100/200/500)
- Search Delay (ms): `[300]` (100-1000)

### Tab: "Infor Visual Integration"
- Site Reference: `[002]` (max 3 chars)
- Valid PO Statuses: `☑ Open (O)  ☑ Partial (P)  ☐ Closed (C)`
- Connection Timeout (sec): `[15]` (5-60)
- **[Test Connection]** button

### Tab: "Advanced"
- Recipient Cache (sec): `[300]` (60-3600)
- PO Line Cache (sec): `[60]` (30-600)
- History Retention (days): `[365]` (90-1825)

---

## Database Schema for Settings

```sql
CREATE TABLE routing_settings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    setting_key VARCHAR(100) NOT NULL UNIQUE,
    setting_value VARCHAR(500) NOT NULL,
    setting_type ENUM('String', 'Integer', 'Boolean', 'Path') NOT NULL,
    description TEXT,
    default_value VARCHAR(500),
    min_value INT NULL,
    max_value INT NULL,
    updated_by INT,
    updated_date DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_key (setting_key)
);

-- Seed default values
INSERT INTO routing_settings (setting_key, setting_value, setting_type, description, default_value, min_value, max_value) VALUES
('CsvExportPath_Network', '', 'Path', 'Primary CSV export path (network share)', '', NULL, NULL),
('CsvExportPath_Local', 'C:\\Routing\\CSV', 'Path', 'Fallback CSV export path (local)', 'C:\\Routing\\CSV', NULL, NULL),
('CsvRetry_MaxAttempts', '3', 'Integer', 'Maximum CSV write retry attempts', '3', 1, 10),
('CsvRetry_DelayMs', '500', 'Integer', 'Delay between retry attempts (ms)', '500', 100, 5000),
('DuplicateDetection_HoursWindow', '24', 'Integer', 'Duplicate label detection window (hours)', '24', 1, 168),
('QuickAdd_RecipientCount', '5', 'Integer', 'Number of Quick Add recipient buttons', '5', 3, 10),
('InforVisual_SiteRef', '002', 'String', 'Infor Visual site/warehouse code', '002', NULL, NULL);
```

---

## Related Files

- **Hardcoded Values Found In:**
  - `Services/RoutingService.cs`
  - `Data/Dao_InforVisualPO.cs`
  - `Data/Dao_RoutingLabel.cs`
  - Multiple ViewModels
  
- **Configuration Files:**
  - `appsettings.json` (some settings already exist)
  
- **Needs Implementation:**
  - Settings ViewModel
  - Settings View (XAML)
  - Settings Service (CRUD operations)
  - Settings DAO (database access)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-06  
**Next Review:** After implementing Settings page
