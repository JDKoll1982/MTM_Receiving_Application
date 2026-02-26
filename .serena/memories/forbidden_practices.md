# Forbidden Practices (Constitutional Violations)

## Critical Violations

### ❌ ViewModels Calling DAOs Directly

```csharp
// FORBIDDEN - ViewModel → DAO
var result = await Dao_ReceivingLine.InsertAsync(line);
var dao = new Dao_User(connectionString);

// REQUIRED - ViewModel → Service → DAO
var result = await _receivingService.AddLineAsync(line);
```

### ❌ Services with Direct Database Access

```csharp
// FORBIDDEN - Service accessing database directly
using var connection = new MySqlConnection(_connectionString);
await Helper_Database_StoredProcedure.ExecuteAsync(...);

// REQUIRED - Service delegates to DAO
return await _receivingLineDao.InsertAsync(line);
```

### ❌ Static DAO Classes

```csharp
// FORBIDDEN - Static DAO
public static class Dao_ReceivingLoad { }

// REQUIRED - Instance-based DAO
public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    public Dao_ReceivingLoad(string connectionString) { ... }
}
```

### ❌ Writes to Infor Visual Database

```csharp
// FORBIDDEN - Any INSERT/UPDATE/DELETE on Infor Visual
string query = "UPDATE po SET status = 'Received'"; // NEVER!

// REQUIRED - Only SELECT queries
string query = "SELECT * FROM PURCHASE_ORDER WHERE ID = @PoNumber";
```

### ❌ Direct SQL for MySQL Operations

```csharp
// FORBIDDEN - Raw SQL in C# code
string sql = "INSERT INTO receiving_line ...";

// REQUIRED - Stored procedures only
await Helper_Database_StoredProcedure.ExecuteAsync("sp_sp_Receiving_Line_Insert", parameters);
```

## High-Priority Violations

### ❌ Throwing Exceptions from DAOs

```csharp
// FORBIDDEN
public async Task<List<Model_Line>> GetAllAsync()
{
    throw new Exception("Not found"); // NO!
}

// REQUIRED
public async Task<Model_Dao_Result<List<Model_Line>>> GetAllAsync()
{
    return DaoResultFactory.Failure<List<Model_Line>>("Not found");
}
```

### ❌ Using Binding Instead of x:Bind

```xml
<!-- FORBIDDEN -->
<TextBox Text="{Binding MyProperty}"/>

<!-- REQUIRED -->
<TextBox Text="{x:Bind ViewModel.MyProperty, Mode=TwoWay}"/>
```

### ❌ Business Logic in View Code-Behind

```csharp
// FORBIDDEN - In MyView.xaml.cs
private async void Button_Click(object sender, RoutedEventArgs e)
{
    var result = await _service.SaveData();
}

// REQUIRED - In MyViewModel.cs
[RelayCommand]
private async Task SaveAsync()
{
    var result = await _service.SaveData();
}
```

### ❌ Non-Partial ViewModels

```csharp
// FORBIDDEN - Won't compile with CommunityToolkit.Mvvm
public class MyViewModel : BaseViewModel { }

// REQUIRED
public partial class MyViewModel : BaseViewModel { }
```

### ❌ Static Factory Methods in Model Classes

```csharp
// FORBIDDEN - Creates circular dependencies
public class Model_Dao_Result
{
    public static Model_Dao_Result Failure(string msg) { }
}

// REQUIRED - Use DaoResultFactory
return DaoResultFactory.Failure("Error message");
```

## Medium-Priority Violations

### ❌ DAO Not Registered in DI

```csharp
// FORBIDDEN - Missing DI registration
public class Dao_NewEntity { }

// REQUIRED - Register in App.xaml.cs
services.AddSingleton(sp => new Dao_NewEntity(connectionString));
```

### ❌ Hardcoded Connection Strings

```csharp
// FORBIDDEN
private const string ConnectionString = "Server=localhost...";

// REQUIRED
private readonly string _connectionString;
public Dao_Entity(string connectionString) { _connectionString = connectionString; }
```

### ❌ MessageBox.Show for Errors

```csharp
// FORBIDDEN
MessageBox.Show("Error occurred");

// REQUIRED
_errorHandler.ShowUserError("Error occurred", "Error", nameof(MethodName));
```

## Validation Checklist

Before committing, verify:

- [ ] No `Dao_*` references in ViewModel files
- [ ] No `Helper_Database_*` calls in ViewModels
- [ ] No `Helper_Database_*` calls in Services (only in DAOs)
- [ ] All DAOs are instance-based (no `static class`)
- [ ] All DAOs registered in DI container
- [ ] No `INSERT`/`UPDATE`/`DELETE` on Infor Visual
- [ ] All MySQL operations use stored procedures
- [ ] All ViewModels are `partial` classes
- [ ] All XAML uses `x:Bind` (not `Binding`)
- [ ] No business logic in `.xaml.cs` files
