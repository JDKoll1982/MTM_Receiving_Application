# Using GitHub actions/starter-workflows for MTM Receiving Application

## Overview

The [actions/starter-workflows](https://github.com/actions/starter-workflows) repository provides a collection of reusable GitHub Actions workflow templates for popular languages, frameworks, and deployment targets. These templates help you quickly set up CI/CD pipelines for your application.

For the MTM Receiving Application (.NET 8, WinUI 3, MySQL/SQL Server), you can use these workflows to automate builds, tests, and deployment.

---

## How to Use Starter Workflows

### 1. Browse the Repository

- Visit: [actions/starter-workflows](https://github.com/actions/starter-workflows)
- Explore folders:
  - `ci/` – Continuous Integration templates (build, test)
  - `deploy/` – Deployment templates (Azure, AWS, Docker, etc.)
  - `automation/` – Utility workflows (labeling, issue triage)

### 2. Find a Suitable Template

For .NET projects, look for:
- `ci/dotnet.yml` – Build and test .NET projects
- `deploy/azure-webapp-dotnet.yml` – Deploy to Azure Web Apps (if needed)

### 3. Copy the Workflow

- Click the desired workflow file (e.g., `ci/dotnet.yml`)
- Copy the YAML contents

### 4. Add to Your Repository

- In your repo, create `.github/workflows/` if it doesn't exist
- Add a new file, e.g., `.github/workflows/ci.yml`
- Paste the workflow YAML

### 5. Customize for Your Project

- Set the correct `dotnet-version` (e.g., `8.0.x`)
- Update `solution` or `project` path if needed
- Add steps for database setup, environment variables, or deployment

**Example: Basic .NET CI Workflow**
```yaml
name: .NET Build & Test

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --configuration Release
    - name: Test
      run: dotnet test --no-build --verbosity normal