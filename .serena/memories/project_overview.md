# Project Overview

**Project Name:** MTM Receiving Application

**Purpose:**
A modern WinUI 3 desktop application designed to streamline receiving operations for manufacturing facilities. It handles authentication, label generation (receiving, dunnage, volvo), and integrates with a MySQL database for data persistence and audit trails.

**Key Features:**

- **Multi-tier Authentication:**
  - Personal Workstation (Auto-login via Windows Username)
  - Shared Terminal (PIN-based login)
  - New User Creation (Self-service)
- **Session Management:**
  - Automatic timeout based on workstation type (30m personal, 15m shared).
  - Activity tracking (mouse, keyboard).
- **Label Generation:**
  - Receiving, Dunnage, and Volvo labels.
- **Database Integration:**
  - MySQL backend.
  - Stored procedures for data access.

**Architecture:**

- **Pattern:** MVVM (Model-View-ViewModel).
- **UI Framework:** WinUI 3 (Windows App SDK).
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection.
