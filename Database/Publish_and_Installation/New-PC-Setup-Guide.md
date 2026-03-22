# MTM Receiving Application — New PC Setup Guide

**Last Updated:** 2026-03-19
**Application:** MTM Receiving Application (server-share deployment)
**Label Software:** TEKLYNX LABELVIEW 2022 Pro (v2022.00.01)

---

## Overview

This guide covers everything needed to set up a new PC to run the MTM Receiving Application
and print labels via LABELVIEW. No software is installed from the app itself — it runs directly
from the server share. The only local setup required is the MySQL ODBC driver for LABELVIEW.

---

## Step 1 — Create the Desktop Shortcut

1. Right-click the desktop → **New → Shortcut**
2. Set the target to the server path:

   ```
   X:\Software Development\Live Applications\MTM_Receiving_Application\MTM_Receiving_Application.exe
   ```

3. Name the shortcut: **MTM Receiving Application**
4. The app is self-contained — no runtime installation is needed on this PC.

> **Note:** If the app fails to launch with a network error, contact IT to confirm the share
> is accessible and the user account has **Read & Execute** permission on the folder.

---

## Step 2 — Install MySQL ODBC Driver (32-bit) for LABELVIEW

LABELVIEW 2022 is a **32-bit application** and requires a **32-bit MySQL ODBC driver**.
The 64-bit driver will not appear in LABELVIEW's data source list and must **not** be used here.

### 2a. Download

1. Go to: [https://dev.mysql.com/downloads/connector/odbc/](https://dev.mysql.com/downloads/connector/odbc/)
2. In the **Select Operating System** dropdown, choose: **Windows (x86, 32-bit)**
3. Download the **MSI Installer** for the latest version (9.6.0 or newer)
   - File will be named: `mysql-connector-odbc-<version>-win32.msi`

> **Do not** download the `winx64.msi` — that is the 64-bit version and will not work with LABELVIEW.

### 2b. Install

1. Run the downloaded `.msi` file
2. Accept defaults and complete the installation
3. No reboot is typically required

---

## Step 3 — Configure the 32-bit ODBC Data Source

> ⚠️ **The ODBC Data Source Administrator must be run as Administrator.**
> Changes to System DSNs require elevated privileges. If you open it without admin rights,
> you will be able to see existing DSNs but the **Add**, **Remove**, and **Configure** buttons
> will appear greyed out or changes will silently fail.

### 3a. Open the 32-bit ODBC Administrator as Admin

The standard Windows ODBC tool (`odbcad32.exe` in System32) opens the **64-bit** version.
LABELVIEW requires the **32-bit** administrator:

1. Press **Win + R**, type the following path exactly, and press **Ctrl + Shift + Enter**
   (this runs it as Administrator):

   ```
   C:\Windows\SysWOW64\odbcad32.exe
   ```

   Alternatively:
   - Open **File Explorer** and navigate to `C:\Windows\SysWOW64\`
   - Right-click `odbcad32.exe` → **Run as administrator**

2. Confirm the UAC prompt.
3. Verify the title bar reads **ODBC Data Source Administrator (32-bit)**.

### 3b. Add the System DSN

1. Click the **System DSN** tab
2. Click **Add...**
3. Select **MySQL ODBC {Version} Unicode Driver** (or the version you installed) → click **Finish**
4. Fill in the fields:

   | Field | Value |
   |---|---|
   | **Data Source Name** | `MTM Receiving Application` |
   | **Description** | MTM Receiving Application MySQL |
   | **TCP/IP Server** | `localhost` |
   | **Port** | `3306` |
   | **User** | root |
   | **Password** | root |
   | **Database** | `mtm_receiving_application` |

5. Click **Test** to verify the connection succeeds
6. Click **OK** to save

> **Tip:** The DSN name must match exactly — LABELVIEW templates reference it by name.
> Use `MTM Receiving Application` (with spaces, matching capitalisation).

---

## Step 4 — Verify LABELVIEW Can See the Data Source

1. Open **LABELVIEW 2022**
2. Open a label template that uses a database connection
3. Go to **File → Database Setup** (or open the data source connection within the label)
4. Confirm the `MTM Receiving Application` DSN appears in the list
5. Test the connection from within LABELVIEW

---

## Reference Links

| Resource | Link |
|---|---|
| MySQL ODBC Connector Downloads | [dev.mysql.com/downloads/connector/odbc](https://dev.mysql.com/downloads/connector/odbc/) |
| .NET 10 Desktop Runtime (if ever needed) | [dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) |
| Windows App SDK Runtime (if ever needed) | [learn.microsoft.com — Windows App SDK Downloads](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) |
| TEKLYNX LABELVIEW Support | [teklynx.com/en/products/label-design-software/labelview](https://www.teklynx.com/en/products/label-design-software/labelview) |

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| App won't launch — network error | Share not mapped or no permissions | Ask IT to map drive and grant Read & Execute |
| App launches but crashes immediately | Missing Visual C++ runtime (rare) | Install [VC++ Redistributable x64](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) |
| ODBC DSN not visible in LABELVIEW | 64-bit driver installed, or 32-bit admin not used | Reinstall using 32-bit MSI; use `SysWOW64\odbcad32.exe` |
| Add/Configure buttons greyed out in ODBC Admin | Not running as Administrator | Close and reopen `odbcad32.exe` with **Run as administrator** |
| ODBC test passes but LABELVIEW can't connect | DSN name mismatch | Ensure DSN is named exactly `MTM Receiving Application` |
| LABELVIEW licence error on new PC | Network licence seat in use | Contact IT — LABELVIEW uses a Network Key licence (Serial: NSPN217883) |
