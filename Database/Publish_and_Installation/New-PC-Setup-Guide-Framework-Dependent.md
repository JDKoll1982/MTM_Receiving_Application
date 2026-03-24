# MTM Receiving Application — Framework-Dependent PC Setup Guide

**Last Updated:** 2026-03-19
**Application:** MTM Receiving Application (framework-dependent deployment)
**Label Software:** TEKLYNX LABELVIEW 2022 Pro (v2022.00.01)

---

## Overview

This guide is for PCs where the .NET runtime and Windows App SDK are **installed locally**
rather than bundled with the application. This is the framework-dependent deployment model —
the publish output on the server share contains only the application code; the runtimes
must be present on every PC before the app will launch.

> **When to use this guide:** IT manages workstations via Intune/SCCM and has confirmed
> runtimes will be pushed to all machines, OR you are manually setting up a specific PC.
>
> **If you are unsure which deployment is in use**, check the server share folder size.
> A self-contained publish is ~200–300 MB. A framework-dependent publish is ~5–20 MB.

**Installation order matters — follow the steps in sequence.**

---

## Step 1 — Install .NET 10 Desktop Runtime

The MTM Receiving Application requires the **.NET 10 Desktop Runtime** (not the ASP.NET Core
Runtime and not the base .NET Runtime alone).

### Which download to use

| Use case | Download |
|---|---|
| End-user PC (running the app only) | **.NET Desktop Runtime 10.0.x** — smaller, runtime only |
| Developer/build machine | **.NET SDK 10.0.201** — includes the runtime plus build tools |

For a standard user PC, install the **Desktop Runtime** only.

### 1a. Download

1. Go to: [https://dotnet.microsoft.com/en-us/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
2. Under **Run apps — Runtime**, find **.NET Desktop Runtime 10.0.x**
3. Click **x64** under Windows to download
   - File will be named: `windowsdesktop-runtime-10.0.x-win-x64.exe`

> **SDK alternative (developer machines only):**
> Under **Build apps — SDK**, download **SDK 10.0.201 → Windows x64**
> File: `dotnet-sdk-10.0.201-win-x64.exe`
> The SDK includes the Desktop Runtime — you do not need to install both.

### 1b. Install

1. Run the downloaded `.exe` as Administrator
2. Accept the licence and click **Install**
3. Click **Close** when complete
4. No reboot is typically required

### 1c. Verify

Open a command prompt and run:

```cmd
dotnet --list-runtimes
```

Confirm you see a line containing:

```
Microsoft.WindowsDesktop.App 10.0.x
```

---

## Step 2 — Install Windows App SDK Runtime

The MTM Receiving Application is built with WinUI 3 and requires the **Windows App SDK Runtime**
(1.8 or newer). This is separate from the .NET runtime and must be installed independently.

### 2a. Download

1. Go to: [https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
2. Under **Stable release → Windows App SDK 1.8**, find the latest version (1.8.6 or newer)
3. Click **Installer (x64)** to download
   - File will be named: `WindowsAppRuntimeInstall-x64.exe`

> **Do not** use the Redistributable (ZIP) — that is for IT mass deployment tools.
> For a single PC, use the standalone `WindowsAppRuntimeInstall-x64.exe`.

### 2b. Install

1. Run `WindowsAppRuntimeInstall-x64.exe` as Administrator
2. The installer runs silently and completes in a few seconds
3. No reboot is required

> **Note:** If you see an error about a package already being installed at the same or newer
> version, the correct runtime is already present — skip to Step 3.

---

## Step 3 — Create the Desktop Shortcut

1. Right-click the desktop → **New → Shortcut**
2. Set the target to the server path:

   ```
   X:\Software Development\Live Applications\MTM_Receiving_Application\MTM_Receiving_Application.exe
   ```

3. Name the shortcut: **MTM Receiving Application**

> **Note:** If the app fails to launch after installing the runtimes above, confirm both
> the .NET Desktop Runtime and Windows App SDK are installed correctly (Steps 1c and 2b),
> then check that the network share is accessible and the user has **Read & Execute** permission.

---

## Step 4 — Install MySQL ODBC Driver (32-bit) for LABELVIEW

LABELVIEW 2022 is a **32-bit application** and requires a **32-bit MySQL ODBC driver**.
The 64-bit driver will not appear in LABELVIEW's data source list.

### 4a. Download

1. Go to: [https://dev.mysql.com/downloads/connector/odbc/](https://dev.mysql.com/downloads/connector/odbc/)
2. In the **Select Operating System** dropdown, choose: **Windows (x86, 32-bit)**
3. Download the **MSI Installer** (9.6.0 or newer)
   - File will be named: `mysql-connector-odbc-<version>-win32.msi`

> **Do not** download the `winx64.msi` — that is the 64-bit version and will not work with LABELVIEW.

### 4b. Install

1. Run the downloaded `.msi` file
2. Accept defaults and complete the installation

---

## Step 5 — Configure the 32-bit ODBC Data Source

> ⚠️ **The ODBC Data Source Administrator must be run as Administrator.**
> Without elevation, the **Add**, **Remove**, and **Configure** buttons will be greyed out
> or changes will silently fail.

### 5a. Open the 32-bit ODBC Administrator as Admin

1. Press **Win + R**, type the path below, then press **Ctrl + Shift + Enter** to run as Administrator:

   ```
   C:\Windows\SysWOW64\odbcad32.exe
   ```

   Alternatively: navigate to `C:\Windows\SysWOW64\`, right-click `odbcad32.exe` → **Run as administrator**

2. Confirm the UAC prompt
3. Verify the title bar reads **ODBC Data Source Administrator (32-bit)**

### 5b. Add the System DSN

1. Click the **System DSN** tab
2. Click **Add...**
3. Select **MySQL ODBC 9.6 Unicode Driver** (32-bit) → click **Finish**
4. Fill in the fields:

   | Field | Value |
   |---|---|
   | **Data Source Name** | `MTM Receiving Application` |
   | **Description** | MTM Receiving Application MySQL |
   | **TCP/IP Server** | `172.16.1.104` |
   | **Port** | `3306` |
   | **User** | root |
   | **Password** | root |
   | **Database** | `mtm_receiving_application` |

5. Click **Test** to verify the connection succeeds
6. Click **OK** to save

> **Tip:** The DSN name must match exactly — LABELVIEW templates reference it by name.
> Use `MTM Receiving Application` (spaces and capitalisation must match).

---

## Step 6 — Verify LABELVIEW Can See the Data Source

1. Open **LABELVIEW 2022**
2. Open a label template that uses a database connection
3. Confirm the `MTM Receiving Application` DSN appears in the list
4. Test the connection from within LABELVIEW

---

## Summary Checklist

- [ ] .NET 10 Desktop Runtime (or SDK 10.0.201) installed — `dotnet --list-runtimes` confirms `Microsoft.WindowsDesktop.App 10.0.x`
- [ ] Windows App SDK Runtime 1.8.x installed (`WindowsAppRuntimeInstall-x64.exe`)
- [ ] Desktop shortcut created pointing to `X:\Software Development\Live Applications\MTM_Receiving_Application\MTM_Receiving_Application.exe`
- [ ] MySQL ODBC 32-bit driver installed (`win32.msi`)
- [ ] System DSN `MTM Receiving Application` created in 32-bit ODBC Admin (run as Administrator)
- [ ] LABELVIEW can connect to the DSN

---

## Reference Links

| Resource | Link |
|---|---|
| .NET 10 Downloads | [dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) |
| Windows App SDK Downloads | [learn.microsoft.com — Windows App SDK Downloads](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) |
| MySQL ODBC Connector Downloads | [dev.mysql.com/downloads/connector/odbc](https://dev.mysql.com/downloads/connector/odbc/) |
| TEKLYNX LABELVIEW Support | [teklynx.com/en/products/label-design-software/labelview](https://www.teklynx.com/en/products/label-design-software/labelview) |

---

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| App won't launch — "framework not found" | .NET 10 Desktop Runtime missing | Reinstall Step 1; verify with `dotnet --list-runtimes` |
| App won't launch — WinUI error / missing DLL | Windows App SDK not installed | Reinstall Step 2 (`WindowsAppRuntimeInstall-x64.exe`) |
| App won't launch — network error | Share not mapped or no permissions | Ask IT to map `X:` drive and grant Read & Execute |
| App launches but crashes immediately | Wrong architecture runtime installed | Confirm x64 runtime matches x64 publish; not x86 |
| ODBC DSN not visible in LABELVIEW | 64-bit driver installed, or 32-bit admin not used | Reinstall using `win32.msi`; use `SysWOW64\odbcad32.exe` |
| Add/Configure buttons greyed out in ODBC Admin | Not running as Administrator | Close and reopen with **Run as administrator** (Ctrl+Shift+Enter) |
| ODBC test passes but LABELVIEW can't connect | DSN name mismatch | Ensure DSN is named exactly `MTM Receiving Application` |
| LABELVIEW licence error on new PC | Network licence seat in use | Contact IT — LABELVIEW uses a Network Key licence (Serial: NSPN217883) |
