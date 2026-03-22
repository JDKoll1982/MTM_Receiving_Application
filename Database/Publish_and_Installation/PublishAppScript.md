# MTM Receiving Application — Publish Scripts

## Deployment Model

The application lives on the server at `X:\Software Development\Live Applications\MTM_Receiving_Application`.
Users launch it via a **desktop shortcut pointing to the `.exe` on the server share** — nothing is installed
locally on each PC. This means:

- **Self-Contained is always required** for the standard deployment. The .NET 10 runtime and Windows App SDK
  must be bundled because the runtime will not be present on user PCs.
- **Framework-Dependent (Option 2) is not suitable** for this deployment model unless every PC has the
  correct runtime pre-installed via IT policy.
- Each option publishes to its **own subfolder** so that experimental builds never overwrite the live app.
  The primary user shortcut always points to `MTM_Receiving_Application`.
- When you publish Option 1, you overwrite the live folder on the server. All users get the update automatically
  the next time they launch the app via their shortcut — no re-deployment to individual machines required.

All commands target `Release` configuration and output directly to the server share.
Replace `-r win-x64` with `-r win-x86` or `-r win-arm64` as needed for the target machine.

---

## 1. Self-Contained (Standard — Recommended ✅)

**What it does:** Bundles the .NET 10 runtime and the Windows App SDK alongside the app.
The entire application runs from the server share — no software needs to be installed on any user PC.

**Why this fits the server-share model:** Users launch straight from the network path via their
desktop shortcut. Everything needed is in the output folder.

**Prerequisites:** None on user PCs. Server share must be accessible and executable permissions must allow
running `.exe` files from the share (check with IT if users get a "can't run from network" error).

> **Reference:** [dotnet publish command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-x64 --self-contained true -o "X:\Software Development\Live Applications\MTM_Receiving_Application"
```

---

## 2. Framework-Dependent — ⚠️ NOT Recommended for Server-Share Deployment

**What it does:** Publishes only the application code — no runtime is bundled in the output folder.
The .NET 10 runtime and Windows App SDK must already be installed on every PC that runs the app.

**Why this does not fit the server-share model:** Because users are not running the app locally
from an installed copy, there is no guarantee the required runtime exists on their PC. If it is
missing, the app will fail to launch with a cryptic error.

**Only use this if** IT has confirmed .NET 10 Desktop Runtime and the Windows App SDK are
deployed to every user machine via group policy or an endpoint management tool (e.g., Intune, SCCM).

**Why you might still choose this:**
- IT already manages workstations via Intune/SCCM and deploys runtimes as part of the standard image — the prereqs will always be present.
- The publish output is significantly smaller, so pushing updates to the server share is faster.
- Keeps the .NET runtime version centrally controlled by IT — patching a .NET security vulnerability only requires IT to push a runtime update, not a full app republish.
- Useful in environments where disk space on the server share is constrained.

**Prerequisites:**
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed on **every** user PC
- [Windows App SDK Runtime](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) installed on **every** user PC

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-x64 --self-contained false -o "X:\Software Development\Live Applications\MTM_Receiving_Application_FD"
```

---

## 3. Self-Contained + Single File

**What it does:** Same as Self-Contained, but compresses everything into a single `.exe`.
Some WinUI 3 native assets will still appear as loose files next to the exe.

**Server-share note:** Launching a large single file across a network can feel slower on first run
while the runtime extracts to the user's local temp folder. Subsequent launches are faster.
For a server-share deployment the standard Self-Contained (Option 1) typically launches more
reliably since all files are already loose on disk.

**Why you might still choose this:**
- You want a visually clean share folder with a single `.exe` rather than hundreds of loose files — easier to explain to users what to shortcut.
- Simpler to back up or version a single file on the server.
- The first-run extraction delay only happens once per user (files are cached in `%TEMP%\.net`); after that, launches are as fast as Option 1.
- Works well if the network is fast and cold-start time is not a concern.

**Prerequisites:** None on user PCs. Same server share accessibility requirement as Option 1.

**Note:** Startup may be slightly slower on first launch while the runtime extracts to temp.

> **Reference:** [Single-file deployment docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "X:\Software Development\Live Applications\MTM_Receiving_Application_SingleFile"
```

---

## 4. Self-Contained + ReadyToRun (Faster Startup)

**What it does:** Pre-compiles assemblies to native code at publish time. Results in
noticeably faster cold-start times at the cost of a larger output folder.

**Server-share note:** This is a good choice for a server-share deployment because startup speed
matters more when launching over a network. The pre-compiled assemblies reduce the CPU work each
user PC must do on launch, which partially compensates for the network file-read overhead.

**Prerequisites:** None on user PCs. Build machine must be `win-x64` when targeting `win-x64`
(cross-compilation is supported but slower).

> **Reference:** [ReadyToRun compilation docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run)

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true -o "X:\Software Development\Live Applications\MTM_Receiving_Application_R2R"
```

---

## 5. Self-Contained + Trimmed (Smaller Output)

**What it does:** Removes unused assemblies and types from the output to reduce folder size.

**Server-share note:** A smaller folder means less data transferred over the network on each
launch. This can improve startup speed on slower network connections. However, the risk of
runtime failures due to trimming is high with WinUI 3.

**Why you might still choose this:**
- The server share is on a genuinely slow link (e.g., remote site over VPN) and folder size is causing noticeable launch delays — trimming can cut the output by 30–60%.
- Disk space on the server share is heavily constrained.
- You have already addressed all trim warnings in the codebase and run a full test pass with no runtime failures — in that case, the risk is reduced to acceptable.
- You need the smallest possible artifact for automated deployment pipelines.

**Prerequisites:** None on user PCs, but **use with caution** — WinUI 3 relies heavily on
reflection and dynamic type loading. Thorough testing after trimming is required to catch
missing-at-runtime errors before deploying to users.

> **Reference:** [Trim self-contained deployments docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true -o "X:\Software Development\Live Applications\MTM_Receiving_Application_Trimmed"
```

---

## 6. Self-Contained + Single File + ReadyToRun (Optimized)

**What it does:** Combines single-file packaging with ReadyToRun pre-compilation.
Best balance of deployment simplicity and startup performance without the risks of trimming.

**Server-share note:** The ReadyToRun pre-compilation helps offset the network launch overhead,
but see the single-file caveat in Option 3 regarding first-run extraction time.

**Why you might still choose this:**
- You want the fastest possible startup after the first-run extraction has happened — this is the best performing option once the cache is warm.
- You prefer a cleaner share folder (single file) AND need fast startup for power users who launch the app many times per day.
- The one-time first-run delay is acceptable as a trade-off for better ongoing performance.
- A good choice if you control the user environment and can pre-warm the extraction cache during onboarding (e.g., via an IT logon script).

**Prerequisites:** None on user PCs. Same server share accessibility requirement as Option 1.

> **References:**
> - [Single-file deployment docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview)
> - [ReadyToRun compilation docs](https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run)

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o "X:\Software Development\Live Applications\MTM_Receiving_Application_Optimized"
```

---

## 7. ARM64 Variant (Self-Contained)

**What it does:** Same as Option 1, but compiled for ARM64 machines (e.g., Surface Pro X,
Snapdragon-based PCs). The csproj already declares `win-arm64` as a supported RID.

**Server-share note:** Publish to a separate output folder so x64 and ARM64 users each have
their own shortcut pointing to the correct build. Both folders can live on the same server share.

**Prerequisites:** None on user PCs. Can be cross-compiled from any Windows machine.

> **Reference:** [.NET Runtime Identifier (RID) catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

```cmd
dotnet publish "c:\Users\jkoll\source\repos\MTM_Receiving_Application\MTM_Receiving_Application.csproj" -c Release -r win-arm64 --self-contained true -o "X:\Software Development\Live Applications\MTM_Receiving_Application_ARM64"
```

---

## Desktop Shortcut Setup

After publishing, create a shortcut on each user's desktop that points to:

```
X:\Software Development\Live Applications\MTM_Receiving_Application\MTM_Receiving_Application.exe
```

- The shortcut target path should use the mapped drive letter or UNC path as appropriate for your network.
- Users need at minimum **Read & Execute** permission on the server share folder.
- To push shortcuts to all desktops, use a Group Policy logon script or your IT endpoint management tool.

---

## Quick Reference

| Option | Output Folder | Runtime Bundled | Single EXE | Faster Startup | Smaller Size | Safe for WinUI 3 | Server-Share Suitable |
|---|---|---|---|---|---|---|---|
| 1. Self-Contained | `MTM_Receiving_Application` | ✅ | ❌ | — | — | ✅ | ✅ Best choice |
| 2. Framework-Dependent | `MTM_Receiving_Application_FD` | ❌ | ❌ | — | ✅ | ✅ | ⚠️ Avoid unless IT managed |
| 3. Single File | `MTM_Receiving_Application_SingleFile` | ✅ | ~✅ | ❌ slight | — | ✅ | ⚠️ Slower first launch |
| 4. ReadyToRun | `MTM_Receiving_Application_R2R` | ✅ | ❌ | ✅ | ❌ larger | ✅ | ✅ Good for slow networks |
| 5. Trimmed | `MTM_Receiving_Application_Trimmed` | ✅ | ❌ | — | ✅ | ⚠️ Test thoroughly | ⚠️ Test before deploying |
| 6. Single File + R2R | `MTM_Receiving_Application_Optimized` | ✅ | ~✅ | ✅ | — | ✅ | ⚠️ Slower first launch |
| 7. ARM64 | `MTM_Receiving_Application_ARM64` | ✅ | ❌ | — | — | ✅ | ✅ Separate folder/shortcut |
