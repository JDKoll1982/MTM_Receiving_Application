# Publish And Build Process Improvement Proposal

Last Updated: 2026-04-29

## Scope

This document summarizes improvement opportunities found by reviewing:

- `PubLog.md`
- `MTM_Receiving_Application.csproj`
- `Database/Publish_and_Installation/Publish-App-GUI.ps1`

No code or build logic was changed as part of this review. This is a proposal only.

## Current Observations

The 2026-04-29 self-contained publish completed successfully in `00:04:37.72`, but the log shows several avoidable costs in the default path:

- The publish ran with `-v detailed`, which generated a very large log and drove constant UI/file streaming in the publish tool.
- The command published directly to the network share output path when `Full overwrite` was selected.
- A full `Restore` target still ran during publish.
- The publish spent a meaningful amount of time in the WinUI XAML compiler passes.
- The staged reuse path exists, but its file comparison logic uses SHA256 hashing on every candidate file.
- The project file says `PublishReadyToRun` is disabled because of WinUI 3 COM interop crashes, but the GUI publish tool still presents ReadyToRun as a safe, faster option.
- The project is unpackaged (`WindowsPackageType=None`), but MSIX tooling is still enabled and imported during evaluation.

## Recommended Changes

### 1. Make Local Staging Plus Differential Sync The Default Publish Path

**What I saw now**

- The logged command published to `X:\MH_RESOURCE\Material_Handler\MTM Receiving Application` directly.
- The log later shows file copy activity writing publish output straight to the share.
- The script already supports a staged publish path, but it is only used when the operator chooses not to overwrite an existing self-contained deployment.

**Before effect**

- The network share is in the hot path for the publish itself.
- A slow or busy share can lengthen publish time.
- A failed publish can leave the share partially updated.
- Full overwrite creates the most network traffic and the highest interruption risk for users launching from the share.

**Proposed change**

- Always publish to a local staging folder first.
- Sync staged output to the network share afterward.
- Keep `full overwrite` as an explicit maintenance option instead of the default behavior.

**After effect**

- Faster and more predictable publish times.
- Lower chance of partial deployments on the share.
- Reduced network chatter during the actual `dotnet publish` phase.
- Easier rollback if the staged result fails validation before sync.

**Effect on the codebase**

- This would affect only the publish script behavior.
- No application runtime behavior would change.
- Temp disk usage on the build machine would increase during publish.

### 2. Add A Fast Publish Path That Skips Restore And Optionally Skips Build

**What I saw now**

- `PubLog.md` shows the project running the `Restore` target during publish.
- The restore step also contacted NuGet vulnerability feeds.
- The publish then continued through normal build work, including XAML compilation and app host creation.

**Before effect**

- Every publish pays restore cost, even when packages and project inputs have not changed.
- Re-publishing the same commit or retrying the same publish option takes longer than necessary.
- Publish time becomes more sensitive to external NuGet availability.

**Proposed change**

- Split the flow into explicit phases:
  - `dotnet restore` only when project or package inputs changed
  - `dotnet build -c Release -r win-x64` once per code revision
  - `dotnet publish --no-restore --no-build` for repeat publishes of the same validated build
- Expose this as a `fast republish` mode in the GUI rather than forcing it all the time.

**After effect**

- Much faster repeat publishes.
- Less network dependence on NuGet for routine deployment.
- Better separation between compile failures and deployment failures.

**Effect on the codebase**

- This would be a publish tool change, not an application behavior change.
- The script would need a reliable rule for when cached build output is still valid.
- Documentation for operators would need one short note explaining when to use normal publish vs fast republish.

### 3. Reduce Default Publish Verbosity And Keep Detailed Logging As Opt-In

**What I saw now**

- `Publish-App-GUI.ps1` sets `$script:PublishVerbosity = 'detailed'`.
- The script redirects command output to a temp file and polls that file every 200 ms to stream the log into the UI.
- The attached publish log is extremely verbose and includes a large amount of MSBuild property reassignment and SDK resolution noise.

**Before effect**

- More disk I/O on the build machine.
- More UI churn inside the publish tool.
- Harder to spot the important warnings or failures.
- The log file is much larger than it needs to be for a normal successful publish.

**Proposed change**

- Change the default verbosity to `minimal` or `normal`.
- Add an explicit `diagnostic publish` or `detailed logging` toggle for troubleshooting sessions.

**After effect**

- Cleaner routine publish logs.
- Lower logging overhead.
- Easier operator experience when a publish succeeds.
- Detailed traces remain available when the team actually needs them.

**Effect on the codebase**

- Script-only change.
- No impact on produced binaries.
- Support workflows improve because diagnostic logging becomes intentional instead of always-on.

### 4. Replace Hash-Every-File Sync With A Cheaper Differential Copy Strategy

**What I saw now**

- The staged sync path compares files with `Get-FileHash -Algorithm SHA256` for both source and destination when file lengths match.
- On a self-contained WinUI publish, that can mean hashing a large number of files before deciding whether to copy them.

**Before effect**

- Extra CPU and disk reads on the build machine.
- Extra reads from the network share for destination file hashing.
- The reuse path can become slower than expected, especially for large deployments over a share.

**Proposed change**

- Replace the current comparison strategy with one of these approaches:
  - `robocopy` mirroring with retry and exclusion rules
  - size plus timestamp comparison first, with hashing only as a fallback when needed

**After effect**

- Faster staged merges.
- Lower CPU and share read pressure.
- Better scaling as publish output size grows.

**Effect on the codebase**

- Script-only change.
- No impact on application binaries.
- Needs one validation pass to ensure `_PublishLogs` and any intentionally preserved folders still behave correctly.

### 5. Align The ReadyToRun Option With The Current Project Safety Policy

**What I saw now**

- The project file contains:
  - `PublishReadyToRun=False`
  - a comment stating ReadyToRun is disabled because it causes COM interop crashes in WinUI 3
- The GUI publish tool still offers:
  - `3  ReadyToRun — Faster Startup`
  - text describing it as a safe choice for all WinUI 3 features

**Before effect**

- The project and the publish tool communicate conflicting guidance.
- An operator can produce a build that the project file itself treats as unsafe.
- This increases the risk of publishing a startup-optimized build that is less stable in production.

**Proposed change**

- Reclassify ReadyToRun as `experimental`.
- Hide it behind an advanced toggle, or remove it until the team re-validates it on the current .NET 10 and Windows App SDK stack.

**After effect**

- The publish UI matches the project’s actual safety guidance.
- Lower risk of an accidental unstable deployment.
- Performance experiments can still happen, but intentionally.

**Effect on the codebase**

- Publish tool messaging and option handling would change.
- Application code would not change.
- This is mainly a deployment safety improvement.

### 6. Make MSIX Tooling Conditional For Packaging Scenarios Only

**What I saw now**

- The project is configured with `WindowsPackageType=None`, which indicates unpackaged output.
- The log still shows MSIX build tooling targets being imported and evaluated.
- `EnableMsixTooling` is set to `true` unconditionally in the project file.

**Before effect**

- Extra packaging-related target evaluation during routine folder publish.
- More log noise.
- Possible extra build overhead for a deployment path that does not appear to use MSIX packaging.

**Proposed change**

- Make `EnableMsixTooling` conditional.
- Keep it on only for packaging workflows that actually need it.
- Use a dedicated publish profile or explicit property for packaged builds if those still exist.

**After effect**

- Leaner unpackaged build and publish evaluation.
- Simpler logs.
- Better separation between share deployment and packaging workflows.

**Effect on the codebase**

- Project file or publish profile change.
- Requires verification that no current packaging workflow depends on the unconditional setting.
- No runtime behavior change for the unpackaged app if validated correctly.

## Secondary Investigation Worth Doing

### Investigate Why Non-`en-US` Resource Files Still Copy During Publish

**What I saw now**

- The publish tool passes `-p:SatelliteResourceLanguages="en-US"`.
- The log still shows many non-`en-US` `.mui` files being copied during publish.

**Why this matters**

- If those files are not required for the deployed scenario, the output footprint is larger than expected.
- On a network share deployment, footprint directly affects sync and launch characteristics.

**Recommendation**

- Investigate which package or SDK component is introducing those resources.
- Treat this as a measurement task first, not an immediate code change.
- Only remove localized resources after confirming there is no impact on WinUI, framework components, or user locale behavior.

## Changes I Would Not Recommend As A Default Optimization

These are already called out in the project file and should stay disabled by default unless the team re-tests them thoroughly:

- `PublishTrimmed=true`
- `PublishReadyToRun=true` as a default
- single-file publish for this WinUI 3 application

The current project comments correctly identify risk around WinUI reflection, XAML binding, DI, JSON serialization, and COM interop.

## Suggested Priority Order

1. Default to local staging plus differential sync.
2. Lower default verbosity and add an opt-in diagnostic mode.
3. Add fast republish with `--no-restore` and `--no-build` when valid.
4. Rework staged sync to avoid SHA256 hashing on every file.
5. Fix the ReadyToRun option mismatch.
6. Condition MSIX tooling for packaging-only workflows.
7. Investigate localization payload reduction separately.

## Concrete Implementation Order For All Proposals

This section turns each proposal into a practical rollout step with scope, dependencies, risk, and expected effort.

### Step 1. Default To Local Staging Plus Differential Sync

**Why first**

- Highest safety gain for share-based deployment.
- Independent of the application runtime.
- Creates the safer deployment foundation for later publish-speed improvements.

**Implementation shape**

- Update the publish script so self-contained publish goes to a local staging directory by default.
- Make sync-to-share the normal completion path.
- Keep full overwrite as an explicit maintenance action.

**Dependencies**

- None.

**Estimated effort**

- Medium.

**Implementation risk**

- Low.

**Before effect**

- Publish output is written directly to the share during the publish itself.
- Interrupted publishes can leave a partially updated deployment folder.

**After effect**

- Publish completes locally first, then the share is updated in a controlled step.
- Deployment becomes more resilient and easier to validate before the share changes.

**Codebase effect**

- `Database/Publish_and_Installation/Publish-App-GUI.ps1` only.

### Step 2. Reduce Default Publish Verbosity

**Why second**

- Very low-risk change.
- Makes all later measurements easier because logs become smaller and easier to compare.
- Improves operator experience immediately.

**Implementation shape**

- Change the default verbosity from `detailed` to `normal` or `minimal`.
- Add a toggle for detailed logging when troubleshooting is needed.

**Dependencies**

- None.

**Estimated effort**

- Small.

**Implementation risk**

- Low.

**Before effect**

- Large logs, extra UI churn, and more difficulty spotting real warnings.

**After effect**

- Smaller logs, cleaner publish output, and less overhead in the WPF publish UI.

**Codebase effect**

- `Database/Publish_and_Installation/Publish-App-GUI.ps1` only.

### Step 3. Fix The ReadyToRun Option Mismatch

**Why third**

- This is a correctness and deployment-safety fix.
- The current UI guidance conflicts with the project’s own safety comments.
- It should be corrected before more operators rely on the current option wording.

**Implementation shape**

- Rename the ReadyToRun option to `experimental`, hide it behind an advanced mode, or remove it.
- Update the option notes so they match the current WinUI 3 stability guidance.

**Dependencies**

- None.

**Estimated effort**

- Small.

**Implementation risk**

- Low.

**Before effect**

- The GUI suggests ReadyToRun is safe even though the project file documents known instability.

**After effect**

- Operators see guidance that matches the actual project policy.
- Lower chance of accidentally deploying an unstable build flavor.

**Codebase effect**

- `Database/Publish_and_Installation/Publish-App-GUI.ps1` only.

### Step 4. Replace Hash-Every-File Differential Sync

**Why fourth**

- Best done after Step 1 because Step 1 makes staged sync the standard path.
- Once staged sync is the default, improving its internal copy strategy becomes more valuable.

**Implementation shape**

- Replace SHA256-per-file comparison with a cheaper strategy.
- Prefer `robocopy` mirroring or size-plus-timestamp comparison, with hashing only as fallback.

**Dependencies**

- Step 1 recommended first.

**Estimated effort**

- Medium.

**Implementation risk**

- Medium.

**Before effect**

- Reuse mode still performs expensive file comparison work.
- Network share reads remain heavier than necessary.

**After effect**

- Differential sync becomes materially faster, especially for large self-contained publishes.

**Codebase effect**

- `Database/Publish_and_Installation/Publish-App-GUI.ps1` only.
- Requires careful validation of preserve rules such as `_PublishLogs`.

### Step 5. Add Fast Republish With `--no-restore` And Optional `--no-build`

**Why fifth**

- This offers strong time savings, but it depends on having a clear and safe publish workflow first.
- It is more sensitive to stale build output than the earlier steps.

**Implementation shape**

- Add a distinct `fast republish` mode.
- Use explicit checks to confirm whether restore and build outputs are still valid before skipping them.
- Keep the normal publish path available as the default safe option.

**Dependencies**

- Steps 1 and 2 are recommended first.
- Step 3 is recommended first so operators are not combining cached output with misleading ReadyToRun guidance.

**Estimated effort**

- Medium to large.

**Implementation risk**

- Medium.

**Before effect**

- Every publish pays restore and build cost, even when repeating the same artifact generation.

**After effect**

- Repeat publishes of the same validated build can complete much faster.
- Publish failures become easier to classify as build-stage vs deployment-stage issues.

**Codebase effect**

- Primarily `Database/Publish_and_Installation/Publish-App-GUI.ps1`.
- May also require one small supporting documentation update for operator guidance.

### Step 6. Make MSIX Tooling Conditional

**Why sixth**

- This touches the project file rather than only the publish script.
- It should come after the script-level improvements because it needs broader validation.

**Implementation shape**

- Make `EnableMsixTooling` conditional instead of always `true`.
- Preserve an explicit path for any packaging scenarios that still rely on MSIX tooling.

**Dependencies**

- None technically, but it is safer after the publish-script changes are stabilized.

**Estimated effort**

- Medium.

**Implementation risk**

- Medium.

**Before effect**

- Packaging-related targets are evaluated even for normal unpackaged share deployment.

**After effect**

- Leaner project evaluation for the common deployment path.
- Potentially smaller logs and less publish overhead.

**Codebase effect**

- `MTM_Receiving_Application.csproj` and possibly one publish profile or packaging path if one exists.

### Step 7. Investigate Satellite Resource Payload Reduction

**Why last**

- This is not a guaranteed safe optimization.
- The right outcome may be `no change` if the resources are required by framework components.
- It should be treated as a measured investigation, not a direct cleanup.

**Implementation shape**

- Measure which packages introduce the extra `.mui` files.
- Confirm whether they are required for WinUI, Windows App SDK, or framework dependencies.
- Only then decide whether resource filtering is safe.

**Dependencies**

- None.

**Estimated effort**

- Medium.

**Implementation risk**

- Medium to high.

**Before effect**

- Publish output appears to contain more localization payload than intended.

**After effect**

- Best case: smaller deployment footprint and faster syncs.
- Conservative case: no change, but with confirmed evidence for why the extra resources must remain.

**Codebase effect**

- Could affect the publish script, project file, or neither, depending on the investigation result.

## Delivery Recommendation

If this work is implemented in batches, the safest grouping is:

1. Batch A: Steps 1, 2, and 3.
2. Batch B: Step 4.
3. Batch C: Step 5.
4. Batch D: Step 6.
5. Batch E: Step 7 investigation only.

That order keeps the first batch script-only, low-risk, and immediately useful, while postponing the more validation-heavy project-file and payload-optimization work.

## Expected Overall Outcome

If the top items are implemented, the publish/build process should become:

- faster for repeat deployments
- safer for network-share publishing
- less noisy for normal operators
- more aligned with the project’s actual WinUI stability constraints
- easier to diagnose when something does go wrong

The main effect on the codebase would be limited to the publish script and a small amount of publish-related project configuration. The application’s runtime behavior should remain unchanged if these changes are applied carefully.