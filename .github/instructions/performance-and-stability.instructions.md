# Performance, Stability, and Best Practices Guidelines

This document outlines critical guidelines for maintaining application performance, stability, and build efficiency, based on lessons learned during development.

## 1. UI Responsiveness & Threading

### **Rule: Never Block the UI Thread**

* **File I/O**: All file operations (reading, writing, checking existence) must be performed asynchronously or on a background thread.
  * *Bad*: `File.WriteAllText(...)` or `new FileStream(...)` on the UI thread.
  * *Good*: `await File.WriteAllTextAsync(...)` or `await Task.Run(() => ...)` for synchronous APIs.
* **Network Operations**: Network calls (including accessing network shares like `\\server\share`) can hang indefinitely. Always wrap these in `Task.Run` if a native async API is not available.
* **Dispatcher Usage**: When updating the UI from a background thread, use `DispatcherQueue.TryEnqueue`.
  * *Pattern*:

        ```csharp
        await Task.Run(() => {
            // Heavy work
            var result = DoWork();
            App.MainWindow.DispatcherQueue.TryEnqueue(() => {
                // Update UI
                ViewModel.Status = result;
            });
        });
        ```

### **Rule: Immediate UI Feedback**

* When starting a long-running operation (like saving), update the UI status *before* awaiting the operation.
* Avoid `await Task.Delay()` for UI timing unless absolutely necessary for visual transitions.

## 2. Database Integrity & Schema

### **Rule: Validate Data Lengths**

* **Schema Alignment**: Ensure C# string properties do not exceed the `VARCHAR` limits defined in the MySQL schema.
* **Sanitization**: In the Service layer, sanitize or truncate data to fit database constraints *before* attempting the insert.
  * *Example*: If `PONumber` is `VARCHAR(20)`, ensure "PO-123456" fits. If the column is `VARCHAR(6)`, strip the prefix.
* **Schema Updates**: When business requirements change (e.g., longer PO numbers), update the SQL schema definition (`.sql` files) and provide a migration path.

## 3. Logging Strategy

### **Rule: Non-Blocking Logging**

* Logging should never crash the application or block the main execution flow.
* Use a **background queue** (e.g., `BlockingCollection`) for writing logs to disk. The logging method called by the application should only enqueue the message and return immediately.
* **Fail-Safe**: Wrap logging logic in `try-catch` blocks to ensure that a failure to write a log file doesn't bring down the app.

## 4. Build Optimization

### **Rule: Optimize Debug Configuration**

* To speed up local development iteration, disable expensive build steps in the `Debug` configuration.
* **MSIX Tooling**: Disable `EnableMsixTooling` for Debug builds in the `.csproj` file if you are testing the unpackaged executable.

    ```xml
    <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
        <EnableMsixTooling>false</EnableMsixTooling>
    </PropertyGroup>
    ```

## 5. Error Handling

### **Rule: Graceful Degradation**

* **Critical vs. Non-Critical**: Distinguish between critical failures (Database save failed) and non-critical ones (Network CSV save failed).
* **User Notification**: Always inform the user of failures with clear, actionable messages using `IService_ErrorHandler`.
* **Logging**: Log the full stack trace of exceptions to aid in debugging.
