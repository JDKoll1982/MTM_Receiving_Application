# Architecture Refactoring Guide

## Overview

This guide documents the architectural standards enforced during the "Architecture Compliance Refactoring" initiative (Feature 007). It serves as a reference for maintaining the integrity of the MTM Receiving Application's architecture.

## Core Principles

1. **MVVM Strictness**: ViewModels must NEVER access DAOs directly. They must use Services.
2. **Service Layering**: Services must delegate all database operations to DAOs. Services should not contain direct SQL or database connection logic.
3. **Instance-Based DAOs**: DAOs must be instance-based classes registered in the Dependency Injection (DI) container, not static classes.
4. **Infor Visual Read-Only**: All interactions with the Infor Visual ERP database must be strictly READ-ONLY.

## Refactoring Patterns

### 1. ViewModel Refactoring

**Violation**: ViewModel instantiates or calls static DAO.
**Fix**:

1. Identify the data operation.
2. Create or locate a Service interface (`IService_*`) that provides this operation.
3. Inject the Service into the ViewModel's constructor.
4. Replace DAO calls with Service calls.

### 2. Service Refactoring

**Violation**: Service creates `MySqlConnection` or executes SQL directly.
**Fix**:

1. Create a DAO class (`Dao_*`) for the entity if one doesn't exist.
2. Move the SQL/Stored Procedure logic into the DAO.
3. Inject the DAO into the Service's constructor.
4. Call the DAO method from the Service.

### 3. Static DAO Conversion

**Violation**: DAO is a `static class` with `static` methods.
**Fix**:

1. Change class to `public class` (remove `static`).
2. Add a constructor that accepts a connection string.
3. Remove `static` keyword from all methods.
4. Register the DAO in `App.xaml.cs` as a Singleton.

## Dependency Injection Registration

All services and DAOs must be registered in `App.xaml.cs`.

```csharp
// DAOs (Singleton)
services.AddSingleton(sp => new Dao_ReceivingLoad(Helper_Database_Variables.GetConnectionString()));

// Services (Transient or Singleton depending on state)
services.AddTransient<IService_MySQL_Receiving, Service_MySQL_Receiving>();
```

## Verification

Use the `class-dependency-graph.dot` or similar tools to visualize dependencies.

- **Good**: ViewModel -> IService -> Service -> Dao -> DatabaseHelper
- **Bad**: ViewModel -> Dao
- **Bad**: Service -> DatabaseHelper (Directly)
