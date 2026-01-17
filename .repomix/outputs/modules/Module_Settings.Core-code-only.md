# User Provided Header
MTM Receiving Application - Module_Settings.Core Code-Only Export

# Files

## File: Module_Settings.Core/Data/Dao_SettingsCoreAudit.cs
```csharp
public class Dao_SettingsCoreAudit
⋮----
public Task<Model_Dao_Result> InsertAsync(Model_SettingsAuditEntry entry)
⋮----
return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public Task<Model_Dao_Result<List<Model_SettingsAuditEntry>>> GetBySettingAsync(string category, string key)
⋮----
return Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
reader => new Model_SettingsAuditEntry
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
Scope = reader.GetString(reader.GetOrdinal("scope")),
Category = reader.GetString(reader.GetOrdinal("category")),
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
OldValue = reader.GetString(reader.GetOrdinal("old_value")),
NewValue = reader.GetString(reader.GetOrdinal("new_value")),
ChangeType = reader.GetString(reader.GetOrdinal("change_type")),
UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
ChangedBy = reader.GetString(reader.GetOrdinal("changed_by")),
ChangedAt = reader.GetDateTime(reader.GetOrdinal("changed_at")),
IpAddress = reader.GetString(reader.GetOrdinal("ip_address")),
WorkstationName = reader.GetString(reader.GetOrdinal("workstation"))
⋮----
public Task<Model_Dao_Result<List<Model_SettingsAuditEntry>>> GetByUserAsync(int userId)
```

## File: Module_Settings.Core/Data/Dao_SettingsCoreRoles.cs
```csharp
public class Dao_SettingsCoreRoles
⋮----
public Task<Model_Dao_Result<List<Model_SettingsRole>>> GetAllAsync()
⋮----
return Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
reader => new Model_SettingsRole
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
RoleName = reader.GetString(reader.GetOrdinal("role_name")),
Description = reader.GetString(reader.GetOrdinal("description")),
CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
```

## File: Module_Settings.Core/Data/Dao_SettingsCoreSystem.cs
```csharp
public class Dao_SettingsCoreSystem
⋮----
public Task<Model_Dao_Result<Model_CoreSetting>> GetByKeyAsync(string category, string key)
⋮----
return Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
reader => new Model_CoreSetting
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
Category = reader.GetString(reader.GetOrdinal("category")),
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
DataType = reader.GetString(reader.GetOrdinal("data_type")),
IsSensitive = reader.GetBoolean(reader.GetOrdinal("is_sensitive")),
IsLocked = reader.GetBoolean(reader.GetOrdinal("is_locked")),
UpdatedBy = reader.GetString(reader.GetOrdinal("updated_by")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
⋮----
public Task<Model_Dao_Result<List<Model_CoreSetting>>> GetByCategoryAsync(string category)
⋮----
return Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public Task<Model_Dao_Result> UpsertAsync(string category, string key, string value, string dataType, bool isSensitive, string updatedBy)
⋮----
return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public Task<Model_Dao_Result> ResetAsync(string category, string key, string updatedBy)
```

## File: Module_Settings.Core/Data/Dao_SettingsCoreUser.cs
```csharp
public class Dao_SettingsCoreUser
⋮----
public Task<Model_Dao_Result<Model_UserSetting>> GetByKeyAsync(int userId, string category, string key)
⋮----
return Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
reader => new Model_UserSetting
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
Category = reader.GetString(reader.GetOrdinal("category")),
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
DataType = reader.GetString(reader.GetOrdinal("data_type")),
UpdatedBy = reader.GetString(reader.GetOrdinal("updated_by")),
UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
⋮----
public Task<Model_Dao_Result<List<Model_UserSetting>>> GetByCategoryAsync(int userId, string category)
⋮----
return Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public Task<Model_Dao_Result> UpsertAsync(int userId, string category, string key, string value, string dataType, string updatedBy)
⋮----
return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public Task<Model_Dao_Result> ResetAsync(int userId, string category, string key, string updatedBy)
```

## File: Module_Settings.Core/Data/Dao_SettingsCoreUserRoles.cs
```csharp
public class Dao_SettingsCoreUserRoles
⋮----
public Task<Model_Dao_Result<List<Model_SettingsUserRole>>> GetByUserAsync(int userId)
⋮----
return Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
reader => new Model_SettingsUserRole
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
RoleId = reader.GetInt32(reader.GetOrdinal("role_id")),
AssignedAt = reader.GetDateTime(reader.GetOrdinal("assigned_at"))
```

## File: Module_Settings.Core/Enums/Enum_SettingsDataType.cs
```csharp

```

## File: Module_Settings.Core/Enums/Enum_SettingsPermissionLevel.cs
```csharp

```

## File: Module_Settings.Core/Enums/Enum_SettingsScope.cs
```csharp

```

## File: Module_Settings.Core/Interfaces/IService_SettingsCoreFacade.cs
```csharp
public interface IService_SettingsCoreFacade
⋮----
Task<Model_Dao_Result<Model_SettingsValue>> GetSettingAsync(string category, string key, int? userId = null);
Task<Model_Dao_Result> SetSettingAsync(string category, string key, string value, int? userId = null);
Task<Model_Dao_Result> ResetSettingAsync(string category, string key, int? userId = null);
Task InitializeDefaultsAsync(int? userId = null);
```

## File: Module_Settings.Core/Interfaces/IService_SettingsWindowHost.cs
```csharp
public interface IService_SettingsWindowHost
⋮----
void ShowSettingsWindow();
```

## File: Module_Settings.Core/Interfaces/IService_UserPreferences.cs
```csharp
public interface IService_UserPreferences
⋮----
Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username);
Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode);
Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode);
Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode);
```

## File: Module_Settings.Core/Interfaces/ISettingsCache.cs
```csharp
public interface ISettingsCache
⋮----
bool TryGet(string cacheKey, out Model_SettingsValue? value);
void Set(string cacheKey, Model_SettingsValue value);
void Remove(string cacheKey);
void Clear();
```

## File: Module_Settings.Core/Interfaces/ISettingsEncryptionService.cs
```csharp
public interface ISettingsEncryptionService
⋮----
string Encrypt(string plainText);
string Decrypt(string cipherText);
void RotateKey();
```

## File: Module_Settings.Core/Interfaces/ISettingsManifestProvider.cs
```csharp
public interface ISettingsManifestProvider
⋮----
IReadOnlyCollection<Model_SettingsDefinition> LoadDefinitions();
```

## File: Module_Settings.Core/Interfaces/ISettingsMetadataRegistry.cs
```csharp
public interface ISettingsMetadataRegistry
⋮----
IReadOnlyCollection<Model_SettingsDefinition> GetAll();
Model_SettingsDefinition? GetDefinition(string category, string key);
void Register(Model_SettingsDefinition definition);
```

## File: Module_Settings.Core/Models/Model_CoreSetting.cs
```csharp
public class Model_CoreSetting
```

## File: Module_Settings.Core/Models/Model_SettingsAuditEntry.cs
```csharp
public class Model_SettingsAuditEntry
```

## File: Module_Settings.Core/Models/Model_SettingsDefinition.cs
```csharp
public class Model_SettingsDefinition
```

## File: Module_Settings.Core/Models/Model_SettingsRole.cs
```csharp
public class Model_SettingsRole
```

## File: Module_Settings.Core/Models/Model_SettingsUserRole.cs
```csharp
public class Model_SettingsUserRole
```

## File: Module_Settings.Core/Models/Model_SettingsValue.cs
```csharp
public class Model_SettingsValue
```

## File: Module_Settings.Core/Models/Model_UserSetting.cs
```csharp
public class Model_UserSetting
```

## File: Module_Settings.Core/Services/GetSettingQuery.cs
```csharp

```

## File: Module_Settings.Core/Services/GetSettingQueryHandler.cs
```csharp
public class GetSettingQueryHandler : IRequestHandler<GetSettingQuery, Model_Dao_Result<Model_SettingsValue>>
⋮----
private readonly Dao_SettingsCoreSystem _systemDao;
private readonly Dao_SettingsCoreUser _userDao;
private readonly Dao_SettingsCoreAudit _auditDao;
private readonly ISettingsMetadataRegistry _registry;
private readonly ISettingsCache _cache;
private readonly ISettingsEncryptionService _encryptionService;
private readonly IService_UserSessionManager _sessionManager;
private readonly IService_ErrorHandler _errorHandler;
private readonly IService_Notification _notification;
⋮----
public async Task<Model_Dao_Result<Model_SettingsValue>> Handle(GetSettingQuery request, CancellationToken cancellationToken)
⋮----
var definition = _registry.GetDefinition(request.Category, request.Key);
⋮----
if (_cache.TryGet(cacheKey, out var cached) && cached != null)
⋮----
return Model_Dao_Result_Factory.Success(cached);
⋮----
systemResult = await _systemDao.GetByKeyAsync(request.Category, request.Key);
⋮----
userResult = await _userDao.GetByKeyAsync(userId!.Value, request.Category, request.Key);
⋮----
if (definition.IsSensitive && !string.IsNullOrWhiteSpace(resolvedValue))
⋮----
decryptedValue = _encryptionService.Decrypt(resolvedValue);
⋮----
var settingsValue = new Model_SettingsValue
⋮----
_cache.Set(cacheKey, settingsValue);
return Model_Dao_Result_Factory.Success(settingsValue);
⋮----
private int? ResolveUserId(Enum_SettingsScope scope, int? userId)
⋮----
private async Task WriteDefaultAsync(Model_SettingsDefinition definition, string defaultValue, int? userId)
⋮----
var valueToStore = definition.IsSensitive ? _encryptionService.Encrypt(defaultValue) : defaultValue;
await _systemDao.UpsertAsync(definition.Category, definition.Key, valueToStore, definition.DataType.ToString(), definition.IsSensitive, actor);
⋮----
await _userDao.UpsertAsync(userId.Value, definition.Category, definition.Key, valueToStore, definition.DataType.ToString(), actor);
⋮----
await _auditDao.InsertAsync(new Model_SettingsAuditEntry
⋮----
Scope = definition.Scope.ToString(),
⋮----
private async Task HandleCorruptedValueAsync(Model_SettingsDefinition definition, string currentValue, int? userId, string reason)
⋮----
await _errorHandler.ShowUserErrorAsync(message, "Core Settings Reset", nameof(GetSettingQueryHandler));
_notification.ShowStatus(message, InfoBarSeverity.Warning);
⋮----
private static bool IsValueValid(Enum_SettingsDataType dataType, string value)
⋮----
return int.TryParse(value, out _);
⋮----
return decimal.TryParse(value, out _);
⋮----
return bool.TryParse(value, out _);
⋮----
return DateTime.TryParse(value, out _);
⋮----
_ = JsonDocument.Parse(value);
⋮----
private static string BuildCacheKey(Enum_SettingsScope scope, string category, string key, int? userId)
```

## File: Module_Settings.Core/Services/ResetSettingCommand.cs
```csharp

```

## File: Module_Settings.Core/Services/ResetSettingCommandHandler.cs
```csharp
public class ResetSettingCommandHandler : IRequestHandler<ResetSettingCommand, Model_Dao_Result>
⋮----
private readonly Dao_SettingsCoreSystem _systemDao;
private readonly Dao_SettingsCoreUser _userDao;
private readonly Dao_SettingsCoreAudit _auditDao;
private readonly ISettingsMetadataRegistry _registry;
private readonly ISettingsCache _cache;
private readonly ISettingsEncryptionService _encryptionService;
private readonly IService_UserSessionManager _sessionManager;
⋮----
public async Task<Model_Dao_Result> Handle(ResetSettingCommand request, CancellationToken cancellationToken)
⋮----
var definition = _registry.GetDefinition(request.Category, request.Key);
⋮----
return Model_Dao_Result_Factory.Failure($"Unknown setting: {request.Category}:{request.Key}");
⋮----
return Model_Dao_Result_Factory.Failure("User context required for user-scoped settings.");
⋮----
var valueToStore = definition.IsSensitive ? _encryptionService.Encrypt(definition.DefaultValue) : definition.DefaultValue;
Model_Dao_Result result;
⋮----
var currentResult = await _systemDao.GetByKeyAsync(request.Category, request.Key);
⋮----
return Model_Dao_Result_Factory.Failure("Setting is locked and cannot be reset.");
⋮----
result = await _systemDao.UpsertAsync(request.Category, request.Key, valueToStore, definition.DataType.ToString(), definition.IsSensitive, actor);
⋮----
result = await _userDao.UpsertAsync(userId!.Value, request.Category, request.Key, valueToStore, definition.DataType.ToString(), actor);
⋮----
await _auditDao.InsertAsync(new Model_SettingsAuditEntry
⋮----
Scope = scope.ToString(),
⋮----
_cache.Remove(BuildCacheKey(scope, request.Category, request.Key, userId));
⋮----
private int? ResolveUserId(Enum_SettingsScope scope, int? userId)
⋮----
private static string BuildCacheKey(Enum_SettingsScope scope, string category, string key, int? userId)
```

## File: Module_Settings.Core/Services/Service_SettingsCache.cs
```csharp
public class Service_SettingsCache : ISettingsCache
⋮----
public bool TryGet(string cacheKey, out Model_SettingsValue? value)
⋮----
return _cache.TryGetValue(cacheKey, out value);
⋮----
public void Set(string cacheKey, Model_SettingsValue value)
⋮----
public void Remove(string cacheKey)
⋮----
_cache.TryRemove(cacheKey, out _);
⋮----
public void Clear()
⋮----
_cache.Clear();
```

## File: Module_Settings.Core/Services/Service_SettingsCoreFacade.cs
```csharp
public class Service_SettingsCoreFacade : IService_SettingsCoreFacade
⋮----
private readonly IMediator _mediator;
private readonly ISettingsMetadataRegistry _registry;
⋮----
public Task<Model_Dao_Result<Model_SettingsValue>> GetSettingAsync(string category, string key, int? userId = null)
⋮----
return _mediator.Send(new GetSettingQuery(category, key, userId));
⋮----
public Task<Model_Dao_Result> SetSettingAsync(string category, string key, string value, int? userId = null)
⋮----
return _mediator.Send(new SetSettingCommand(category, key, value, userId));
⋮----
public Task<Model_Dao_Result> ResetSettingAsync(string category, string key, int? userId = null)
⋮----
return _mediator.Send(new ResetSettingCommand(category, key, userId));
⋮----
public async Task InitializeDefaultsAsync(int? userId = null)
⋮----
foreach (var definition in _registry.GetAll())
⋮----
await _mediator.Send(new GetSettingQuery(definition.Category, definition.Key, userId));
```

## File: Module_Settings.Core/Services/Service_SettingsEncryptionService.cs
```csharp
public class Service_SettingsEncryptionService : ISettingsEncryptionService
⋮----
private static readonly string KeyPath = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
⋮----
public string Encrypt(string plainText)
⋮----
using var aes = Aes.Create();
⋮----
aes.GenerateIV();
using var encryptor = aes.CreateEncryptor();
var plainBytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
⋮----
Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);
return Convert.ToBase64String(payload);
⋮----
public string Decrypt(string cipherText)
⋮----
if (string.IsNullOrWhiteSpace(cipherText))
⋮----
var payload = Convert.FromBase64String(cipherText);
⋮----
Buffer.BlockCopy(payload, 0, iv, 0, ivLength);
Buffer.BlockCopy(payload, ivLength, cipherBytes, 0, cipherBytes.Length);
⋮----
using var decryptor = aes.CreateDecryptor();
var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
return Encoding.UTF8.GetString(plainBytes);
⋮----
public void RotateKey()
⋮----
var directory = Path.GetDirectoryName(KeyPath);
if (!string.IsNullOrWhiteSpace(directory))
⋮----
Directory.CreateDirectory(directory);
⋮----
var newKey = RandomNumberGenerator.GetBytes(KeySize);
var protectedKey = ProtectedData.Protect(newKey, null, DataProtectionScope.CurrentUser);
File.WriteAllBytes(KeyPath, protectedKey);
⋮----
private static byte[] GetOrCreateKey()
⋮----
if (File.Exists(KeyPath))
⋮----
var protectedKey = File.ReadAllBytes(KeyPath);
return ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
⋮----
var key = RandomNumberGenerator.GetBytes(KeySize);
var encrypted = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
File.WriteAllBytes(KeyPath, encrypted);
```

## File: Module_Settings.Core/Services/Service_SettingsManifestProvider.cs
```csharp
public class Service_SettingsManifestProvider : ISettingsManifestProvider
⋮----
public IReadOnlyCollection<Model_SettingsDefinition> LoadDefinitions()
⋮----
var absolutePath = Path.Combine(AppContext.BaseDirectory, ManifestPath);
if (!File.Exists(absolutePath))
⋮----
var json = File.ReadAllText(absolutePath);
var document = JsonDocument.Parse(json);
⋮----
if (!document.RootElement.TryGetProperty("settings", out var settingsArray))
⋮----
foreach (var item in settingsArray.EnumerateArray())
⋮----
settings.Add(new Model_SettingsDefinition
⋮----
Category = item.GetProperty("category").GetString() ?? string.Empty,
Key = item.GetProperty("key").GetString() ?? string.Empty,
DisplayName = item.GetProperty("displayName").GetString() ?? string.Empty,
DefaultValue = item.GetProperty("defaultValue").GetString() ?? string.Empty,
DataType = Enum.Parse<Enum_SettingsDataType>(item.GetProperty("dataType").GetString() ?? "String", true),
Scope = Enum.Parse<Enum_SettingsScope>(item.GetProperty("scope").GetString() ?? "System", true),
PermissionLevel = Enum.Parse<Enum_SettingsPermissionLevel>(item.GetProperty("permissionLevel").GetString() ?? "User", true),
IsSensitive = item.GetProperty("isSensitive").GetBoolean(),
ValidationRules = item.TryGetProperty("validationRules", out var rules) ? rules.GetString() : null
```

## File: Module_Settings.Core/Services/Service_SettingsMetadataRegistry.cs
```csharp
public class Service_SettingsMetadataRegistry : ISettingsMetadataRegistry
⋮----
foreach (var definition in manifestProvider.LoadDefinitions())
⋮----
public IReadOnlyCollection<Model_SettingsDefinition> GetAll()
⋮----
return _definitions.Values.ToList();
⋮----
public Model_SettingsDefinition? GetDefinition(string category, string key)
⋮----
return _definitions.TryGetValue(BuildKey(category, key), out var definition)
⋮----
public void Register(Model_SettingsDefinition definition)
⋮----
private static string BuildKey(string category, string key)
```

## File: Module_Settings.Core/Services/Service_SettingsWindowHost.cs
```csharp
public class Service_SettingsWindowHost : IService_SettingsWindowHost
⋮----
private readonly IServiceProvider _serviceProvider;
⋮----
public void ShowSettingsWindow()
⋮----
_settingsWindow.Activate();
```

## File: Module_Settings.Core/Services/Service_UserPreferences.cs
```csharp
public class Service_UserPreferences : IService_UserPreferences
⋮----
private readonly Dao_User _userDao;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
public async Task<Model_Dao_Result<Model_UserPreference>> GetLatestUserPreferenceAsync(string username)
⋮----
if (string.IsNullOrWhiteSpace(normalizedUsername))
⋮----
var userResult = await _userDao.GetUserByWindowsUsernameAsync(normalizedUsername);
⋮----
_logger.LogError(
⋮----
var preference = new Model_UserPreference
⋮----
DefaultReceivingMode = string.IsNullOrWhiteSpace(userResult.Data.DefaultReceivingMode)
⋮----
DefaultDunnageMode = string.IsNullOrWhiteSpace(userResult.Data.DefaultDunnageMode)
⋮----
return Model_Dao_Result_Factory.Success(preference);
⋮----
_logger.LogError($"Error getting user preference for {username}", ex, "UserPreferences");
⋮----
public async Task<Model_Dao_Result> UpdateDefaultModeAsync(string username, string defaultMode)
⋮----
return Model_Dao_Result_Factory.Success();
⋮----
_logger.LogWarning($"Default mode update skipped: user not found ({normalizedUsername})");
⋮----
var normalizedMode = string.IsNullOrWhiteSpace(defaultMode)
⋮----
: defaultMode.Trim().ToLowerInvariant();
var result = await _userDao.UpdateDefaultModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
⋮----
_logger.LogWarning($"Default mode update failed (non-blocking): {result.ErrorMessage}");
⋮----
_logger.LogError($"Error updating default mode for {username} (non-blocking)", ex, "UserPreferences");
⋮----
public async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(string username, string defaultMode)
⋮----
_logger.LogWarning($"Receiving default update skipped: user not found ({normalizedUsername})");
⋮----
var result = await _userDao.UpdateDefaultReceivingModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
⋮----
_logger.LogWarning($"Receiving default update failed (non-blocking): {result.ErrorMessage}");
⋮----
_logger.LogError($"Error updating receiving default for {username} (non-blocking)", ex, "UserPreferences");
⋮----
public async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(string username, string defaultMode)
⋮----
_logger.LogWarning($"Dunnage default update skipped: user not found ({normalizedUsername})");
⋮----
var result = await _userDao.UpdateDefaultDunnageModeAsync(userResult.Data.EmployeeNumber, normalizedMode);
⋮----
_logger.LogWarning($"Dunnage default update failed (non-blocking): {result.ErrorMessage}");
⋮----
_logger.LogError($"Error updating dunnage default for {username} (non-blocking)", ex, "UserPreferences");
```

## File: Module_Settings.Core/Services/SetSettingCommand.cs
```csharp

```

## File: Module_Settings.Core/Services/SetSettingCommandHandler.cs
```csharp
public class SetSettingCommandHandler : IRequestHandler<SetSettingCommand, Model_Dao_Result>
⋮----
private readonly Dao_SettingsCoreSystem _systemDao;
private readonly Dao_SettingsCoreUser _userDao;
private readonly Dao_SettingsCoreAudit _auditDao;
private readonly Dao_SettingsCoreRoles _rolesDao;
private readonly Dao_SettingsCoreUserRoles _userRolesDao;
private readonly ISettingsMetadataRegistry _registry;
private readonly ISettingsCache _cache;
private readonly ISettingsEncryptionService _encryptionService;
private readonly IService_UserSessionManager _sessionManager;
⋮----
public async Task<Model_Dao_Result> Handle(SetSettingCommand request, CancellationToken cancellationToken)
⋮----
var definition = _registry.GetDefinition(request.Category, request.Key);
⋮----
return Model_Dao_Result_Factory.Failure($"Unknown setting: {request.Category}:{request.Key}");
⋮----
return Model_Dao_Result_Factory.Failure("User context required for user-scoped settings.");
⋮----
return Model_Dao_Result_Factory.Failure("Insufficient permissions to modify this setting.");
⋮----
return Model_Dao_Result_Factory.Failure($"Invalid value for {definition.DataType}.");
⋮----
var currentResult = await _systemDao.GetByKeyAsync(request.Category, request.Key);
⋮----
return Model_Dao_Result_Factory.Failure("Setting is locked and cannot be modified.");
⋮----
var storedValue = definition.IsSensitive ? _encryptionService.Encrypt(request.Value) : request.Value;
Model_Dao_Result result;
⋮----
result = await _systemDao.UpsertAsync(request.Category, request.Key, storedValue, definition.DataType.ToString(), definition.IsSensitive, actor);
⋮----
result = await _userDao.UpsertAsync(userId!.Value, request.Category, request.Key, storedValue, definition.DataType.ToString(), actor);
⋮----
await _auditDao.InsertAsync(new Model_SettingsAuditEntry
⋮----
Scope = scope.ToString(),
⋮----
_cache.Remove(BuildCacheKey(scope, request.Category, request.Key, userId));
⋮----
private int? ResolveUserId(Enum_SettingsScope scope, int? userId)
⋮----
private async Task<bool> HasPermissionAsync(Enum_SettingsPermissionLevel required, int? userId)
⋮----
var rolesResult = await _rolesDao.GetAllAsync();
⋮----
var userRolesResult = await _userRolesDao.GetByUserAsync(userId.Value);
⋮----
var userRoleIds = userRolesResult.Data.Select(r => r.RoleId).ToHashSet();
⋮----
.Where(r => userRoleIds.Contains(r.Id))
.Select(r => r.RoleName)
.ToHashSet(StringComparer.OrdinalIgnoreCase);
⋮----
Enum_SettingsPermissionLevel.Supervisor => userRoleNames.Contains("Supervisor") || userRoleNames.Contains("Admin") || userRoleNames.Contains("Developer"),
Enum_SettingsPermissionLevel.Admin => userRoleNames.Contains("Admin") || userRoleNames.Contains("Developer"),
Enum_SettingsPermissionLevel.Developer => userRoleNames.Contains("Developer"),
⋮----
private static string BuildCacheKey(Enum_SettingsScope scope, string category, string key, int? userId)
⋮----
private static bool IsValueValid(Enum_SettingsDataType dataType, string value)
⋮----
return int.TryParse(value, out _);
⋮----
return decimal.TryParse(value, out _);
⋮----
return bool.TryParse(value, out _);
⋮----
return DateTime.TryParse(value, out _);
⋮----
_ = JsonDocument.Parse(value);
```

## File: Module_Settings.Core/Validators/SetSettingCommandValidator.cs
```csharp
public class SetSettingCommandValidator : AbstractValidator<SetSettingCommand>
⋮----
RuleFor(x => x.Category).NotEmpty();
RuleFor(x => x.Key).NotEmpty();
RuleFor(x => x.Value).NotNull();
```

## File: Module_Settings.Core/ViewModels/ViewModel_Settings_Database.cs
```csharp
public partial class ViewModel_Settings_Database : ViewModel_Shared_Base
```

## File: Module_Settings.Core/ViewModels/ViewModel_Settings_Logging.cs
```csharp
public partial class ViewModel_Settings_Logging : ViewModel_Shared_Base
```

## File: Module_Settings.Core/ViewModels/ViewModel_Settings_SharedPaths.cs
```csharp
public partial class ViewModel_Settings_SharedPaths : ViewModel_Shared_Base
```

## File: Module_Settings.Core/ViewModels/ViewModel_Settings_System.cs
```csharp
public partial class ViewModel_Settings_System : ViewModel_Shared_Base
⋮----
private readonly ISettingsMetadataRegistry _registry;
⋮----
_registry.GetAll().Where(d => d.Scope == Enum_SettingsScope.System));
```

## File: Module_Settings.Core/ViewModels/ViewModel_Settings_Theme.cs
```csharp
public partial class ViewModel_Settings_Theme : ViewModel_Shared_Base
```

## File: Module_Settings.Core/ViewModels/ViewModel_Settings_Users.cs
```csharp
public partial class ViewModel_Settings_Users : ViewModel_Shared_Base
```

## File: Module_Settings.Core/ViewModels/ViewModel_SettingsWindow.cs
```csharp
public partial class ViewModel_SettingsWindow : ViewModel_Shared_Base
```

## File: Module_Settings.Core/Views/View_Settings_CoreWindow.xaml
```
<Window
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_CoreWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels"
    xmlns:views="using:MTM_Receiving_Application.Module_Settings.Core.Views">

    <Grid>
        <NavigationView x:Name="SettingsNavView"
                        IsSettingsVisible="False"
                        SelectionChanged="OnSelectionChanged">
            <NavigationView.MenuItems>
                <NavigationViewItem Content="System" Tag="System" Icon="Setting" />
                <NavigationViewItem Content="Users &amp; Privileges" Tag="Users" Icon="Contact" />
                <NavigationViewItem Content="UI Theme" Tag="Theme" Icon="Brush" />
                <NavigationViewItem Content="Database" Tag="Database" Icon="Database" />
                <NavigationViewItem Content="Logging" Tag="Logging" Icon="Document" />
                <NavigationViewItem Content="Shared Paths" Tag="SharedPaths" Icon="Folder" />
            </NavigationView.MenuItems>
            <Frame x:Name="SettingsFrame" />
        </NavigationView>
    </Grid>
</Window>
```

## File: Module_Settings.Core/Views/View_Settings_CoreWindow.xaml.cs
```csharp
public sealed partial class View_Settings_CoreWindow : Window
⋮----
WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1400, 900);
⋮----
SettingsFrame.Navigate(typeof(View_Settings_System));
⋮----
private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
⋮----
SettingsFrame.Navigate(typeof(View_Settings_Users));
⋮----
SettingsFrame.Navigate(typeof(View_Settings_Theme));
⋮----
SettingsFrame.Navigate(typeof(View_Settings_Database));
⋮----
SettingsFrame.Navigate(typeof(View_Settings_Logging));
⋮----
SettingsFrame.Navigate(typeof(View_Settings_SharedPaths));
```

## File: Module_Settings.Core/Views/View_Settings_Database.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_Database"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="12">
            <TextBlock Text="Database" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Settings.Core/Views/View_Settings_Database.xaml.cs
```csharp
public sealed partial class View_Settings_Database : Page
```

## File: Module_Settings.Core/Views/View_Settings_Logging.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_Logging"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="12">
            <TextBlock Text="Logging" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Settings.Core/Views/View_Settings_Logging.xaml.cs
```csharp
public sealed partial class View_Settings_Logging : Page
```

## File: Module_Settings.Core/Views/View_Settings_SharedPaths.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_SharedPaths"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="12">
            <TextBlock Text="Shared Paths" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Settings.Core/Views/View_Settings_SharedPaths.xaml.cs
```csharp
public sealed partial class View_Settings_SharedPaths : Page
```

## File: Module_Settings.Core/Views/View_Settings_System.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_System"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels"
    xmlns:models="using:MTM_Receiving_Application.Module_Settings.Core.Models">

    <Grid Padding="20">
        <StackPanel Spacing="12">
            <TextBlock Text="System Settings" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="Core system defaults registered in the settings manifest." />
            <ItemsControl ItemsSource="{x:Bind ViewModel.Definitions, Mode=OneWay}">
                <ItemsControl.ItemTemplate>
                    <DataTemplate x:DataType="models:Model_SettingsDefinition">
                        <StackPanel Spacing="2" Margin="0,6,0,6">
                            <TextBlock Text="{x:Bind DisplayName}" FontWeight="SemiBold" />
                            <TextBlock Text="{x:Bind Key}" Style="{StaticResource CaptionTextBlockStyle}" />
                        </StackPanel>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Settings.Core/Views/View_Settings_System.xaml.cs
```csharp
public sealed partial class View_Settings_System : Page
```

## File: Module_Settings.Core/Views/View_Settings_Theme.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_Theme"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="12">
            <TextBlock Text="UI Theme" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Settings.Core/Views/View_Settings_Theme.xaml.cs
```csharp
public sealed partial class View_Settings_Theme : Page
```

## File: Module_Settings.Core/Views/View_Settings_Users.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.Core.Views.View_Settings_Users"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.Core.ViewModels">

    <Grid Padding="20">
        <StackPanel Spacing="12">
            <TextBlock Text="Users &amp; Privileges" Style="{StaticResource TitleTextBlockStyle}" />
            <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" />
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Settings.Core/Views/View_Settings_Users.xaml.cs
```csharp
public sealed partial class View_Settings_Users : Page
```
