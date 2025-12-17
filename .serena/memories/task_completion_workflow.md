# Task Completion Workflow

When a task is completed:

1. **Verify Implementation:**
   - Ensure code compiles (`dotnet build`).
   - Ensure functionality meets requirements.

2. **Run Tests:**
   - Run unit tests (`dotnet test`).
   - Add new tests if new functionality was added.

3. **Update Documentation:**
   - Update `tasks.md` (mark tasks as completed `[X]`).
   - Update relevant documentation files if architecture or features changed.

4. **Code Quality:**
   - Check for unused usings.
   - Ensure naming conventions are followed.
   - Verify error handling is in place (try-catch blocks in DAOs/Services).
