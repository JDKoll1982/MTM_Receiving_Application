using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for saving receiving load data to MySQL database.
    /// </summary>
    public class Service_MySQL_Receiving : IService_MySQL_Receiving
    {
        private const string EditModeOwnershipRestrictionMessage =
            "You can only view, modify, or remove rows that you created. Admin and Developer users have unrestricted Edit Mode access.";

        private readonly Dao_ReceivingLoad _receivingLoadDao;
        private readonly Dao_ReceivingLabelData _receivingLabelDataDao;
        private readonly Dao_ReceivingNonPOEntry _receivingNonPoEntryDao;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_UserSessionManager? _sessionManager;
        private readonly IService_UserPrivileges? _userPrivileges;

        public Service_MySQL_Receiving(
            Dao_ReceivingLoad receivingLoadDao,
            Dao_ReceivingLabelData receivingLabelDataDao,
            Dao_ReceivingNonPOEntry receivingNonPoEntryDao,
            IService_LoggingUtility logger,
            IService_UserSessionManager sessionManager,
            IService_UserPrivileges userPrivileges
        )
        {
            _receivingLoadDao = receivingLoadDao;
            _receivingLabelDataDao = receivingLabelDataDao;
            _receivingNonPoEntryDao = receivingNonPoEntryDao;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionManager =
                sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _userPrivileges =
                userPrivileges ?? throw new ArgumentNullException(nameof(userPrivileges));
        }

        // Constructor for backward compatibility if needed, but DI should handle it.
        public Service_MySQL_Receiving(string connectionString, IService_LoggingUtility logger)
        {
            _receivingLoadDao = new Dao_ReceivingLoad(connectionString);
            _receivingLabelDataDao = new Dao_ReceivingLabelData(connectionString);
            _receivingNonPoEntryDao = new Dao_ReceivingNonPOEntry(connectionString);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessionManager = null;
            _userPrivileges = null;
        }

        public async Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            _logger.LogInfo($"Saving {loads.Count} loads to receiving label queue table.");

            var result = await _receivingLabelDataDao.SaveLoadsAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully saved {result.Data} loads.");
                return result.Data;
            }

            _logger.LogError($"Failed to save loads: {result.ErrorMessage}", result.Exception);
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }

        public async Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            var accessContext = await GetEditModeAccessContextAsync();
            if (!accessContext.HasContext)
            {
                throw new InvalidOperationException(
                    "An active user session is required to update Receiving Edit Mode rows."
                );
            }

            var unauthorizedLoads = GetUnauthorizedLoads(
                loads,
                accessContext.EmployeeNumber,
                accessContext.WindowsUsername,
                accessContext.HasFullAccess
            );
            if (unauthorizedLoads.Count > 0)
            {
                throw new InvalidOperationException(EditModeOwnershipRestrictionMessage);
            }

            _logger.LogInfo($"Updating {loads.Count} loads in database.");

            var result = await _receivingLoadDao.UpdateLoadsAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully updated {result.Data} loads.");
                return result.Data;
            }

            _logger.LogError($"Failed to update loads: {result.ErrorMessage}", result.Exception);
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }

        public async Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            var accessContext = await GetEditModeAccessContextAsync();
            if (!accessContext.HasContext)
            {
                throw new InvalidOperationException(
                    "An active user session is required to delete Receiving Edit Mode rows."
                );
            }

            var unauthorizedLoads = GetUnauthorizedLoads(
                loads,
                accessContext.EmployeeNumber,
                accessContext.WindowsUsername,
                accessContext.HasFullAccess
            );
            if (unauthorizedLoads.Count > 0)
            {
                throw new InvalidOperationException(EditModeOwnershipRestrictionMessage);
            }

            _logger.LogInfo($"Deleting {loads.Count} loads from database.");

            var result = await _receivingLoadDao.DeleteLoadsAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully deleted {result.Data} loads.");
                return result.Data;
            }

            _logger.LogError($"Failed to delete loads: {result.ErrorMessage}", result.Exception);
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }

        public async Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(
            string partID,
            DateTime startDate,
            DateTime endDate
        )
        {
            var result = await _receivingLoadDao.GetHistoryAsync(partID, startDate, endDate);

            if (result.IsSuccess)
            {
                return result.Data ?? new List<Model_ReceivingLoad>();
            }

            _logger.LogError(
                $"Failed to get receiving history: {result.ErrorMessage}",
                result.Exception
            );
            return new List<Model_ReceivingLoad>();
        }

        public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            _logger.LogInfo(
                $"Retrieving all receiving loads from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
            );

            var result = await _receivingLoadDao.GetAllAsync(startDate, endDate);
            if (!result.IsSuccess)
            {
                _logger.LogError(
                    $"Failed to retrieve receiving loads: {result.ErrorMessage}",
                    result.Exception
                );
                return result;
            }

            var accessContext = await GetEditModeAccessContextAsync();
            if (!accessContext.HasContext)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(
                    "An active user session is required to load Receiving Edit Mode history."
                );
            }

            var allLoads = result.Data ?? new List<Model_ReceivingLoad>();
            var visibleLoads = ApplyOwnershipFilter(
                allLoads,
                accessContext.EmployeeNumber,
                accessContext.WindowsUsername,
                accessContext.HasFullAccess
            );

            if (visibleLoads.Count != allLoads.Count)
            {
                _logger.LogInfo(
                    $"Filtered {allLoads.Count - visibleLoads.Count} history rows created by other users.",
                    nameof(Service_MySQL_Receiving)
                );
            }

            result.Data = visibleLoads;
            _logger.LogInfo($"Retrieved {visibleLoads.Count} receiving loads from database");
            return result;
        }

        public async Task<bool> TestConnectionAsync()
        {
            return true;
        }

        public async Task<Model_Dao_Result<int>> ClearLabelDataToHistoryAsync(
            string archivedBy,
            int employeeNumber,
            bool clearAllRows
        )
        {
            _logger.LogInfo(
                clearAllRows
                    ? $"Clearing all receiving label queue rows to history by user: {archivedBy}"
                    : $"Clearing receiving label queue rows for employee {employeeNumber} by user: {archivedBy}"
            );
            var result = await _receivingLabelDataDao.ClearLabelDataToHistoryAsync(
                archivedBy,
                employeeNumber,
                clearAllRows
            );

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Clear Label Data completed. Rows moved: {result.Data}");
            }
            else
            {
                _logger.LogError(
                    $"Clear Label Data failed: {result.ErrorMessage}",
                    result.Exception
                );
            }

            return result;
        }

        public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetCurrentLabelDataAsync()
        {
            _logger.LogInfo("Loading current label data from receiving_label_data queue");
            var result = await _receivingLabelDataDao.GetCurrentLabelDataAsync();
            if (!result.IsSuccess)
            {
                _logger.LogError(
                    $"Failed to load current label data: {result.ErrorMessage}",
                    result.Exception
                );
                return result;
            }

            var accessContext = await GetEditModeAccessContextAsync();
            if (!accessContext.HasContext)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(
                    "An active user session is required to load Current Labels in Receiving Edit Mode."
                );
            }

            var allLoads = result.Data ?? new List<Model_ReceivingLoad>();
            var visibleLoads = ApplyOwnershipFilter(
                allLoads,
                accessContext.EmployeeNumber,
                accessContext.WindowsUsername,
                accessContext.HasFullAccess
            );

            if (visibleLoads.Count != allLoads.Count)
            {
                _logger.LogInfo(
                    $"Filtered {allLoads.Count - visibleLoads.Count} current-label rows created by other users.",
                    nameof(Service_MySQL_Receiving)
                );
            }

            result.Data = visibleLoads;
            _logger.LogInfo($"Loaded {visibleLoads.Count} rows from current label queue");
            return result;
        }

        public async Task<bool> HasActiveLabelDataAsync()
        {
            _logger.LogInfo("Checking whether receiving_label_data contains any rows");
            var result = await _receivingLabelDataDao.GetCurrentLabelDataAsync();

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    $"Failed to check receiving label data availability: {result.ErrorMessage}",
                    result.Exception
                );
                return false;
            }

            return (result.Data?.Count ?? 0) > 0;
        }

        public async Task<int> UpdateCurrentLabelDataAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            var accessContext = await GetEditModeAccessContextAsync();
            if (!accessContext.HasContext)
            {
                throw new InvalidOperationException(
                    "An active user session is required to update Current Labels in Receiving Edit Mode."
                );
            }

            var unauthorizedLoads = GetUnauthorizedLoads(
                loads,
                accessContext.EmployeeNumber,
                accessContext.WindowsUsername,
                accessContext.HasFullAccess
            );
            if (unauthorizedLoads.Count > 0)
            {
                throw new InvalidOperationException(EditModeOwnershipRestrictionMessage);
            }

            _logger.LogInfo($"Updating {loads.Count} rows in receiving_label_data");
            var result = await _receivingLabelDataDao.UpdateCurrentLabelDataAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully updated {result.Data} label data rows");
                return result.Data;
            }

            _logger.LogError(
                $"Failed to update label data: {result.ErrorMessage}",
                result.Exception
            );
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }

        public async Task<int> DeleteCurrentLabelDataAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            var accessContext = await GetEditModeAccessContextAsync();
            if (!accessContext.HasContext)
            {
                throw new InvalidOperationException(
                    "An active user session is required to delete Current Labels in Receiving Edit Mode."
                );
            }

            var unauthorizedLoads = GetUnauthorizedLoads(
                loads,
                accessContext.EmployeeNumber,
                accessContext.WindowsUsername,
                accessContext.HasFullAccess
            );
            if (unauthorizedLoads.Count > 0)
            {
                throw new InvalidOperationException(EditModeOwnershipRestrictionMessage);
            }

            _logger.LogInfo($"Deleting {loads.Count} rows from receiving_label_data");
            var result = await _receivingLabelDataDao.DeleteCurrentLabelDataAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully deleted {result.Data} label data rows");
                return result.Data;
            }

            _logger.LogError(
                $"Failed to delete label data: {result.ErrorMessage}",
                result.Exception
            );
            throw new InvalidOperationException(result.ErrorMessage, result.Exception);
        }

        public async Task<Model_Dao_Result<List<Model_ReceivingNonPOEntry>>> GetNonPOEntriesAsync()
        {
            return await _receivingNonPoEntryDao.GetAllAsync();
        }

        public async Task<Model_Dao_Result> SaveNonPOEntryAsync(string value, string createdBy)
        {
            return await _receivingNonPoEntryDao.UpsertAsync(value, createdBy);
        }

        public async Task<Model_Dao_Result> DeleteNonPOEntryAsync(int id)
        {
            return await _receivingNonPoEntryDao.DeleteAsync(id);
        }

        public async Task<Model_Dao_Result<string?>> GetNonPOPartDefaultAsync(string partId)
        {
            return await _receivingNonPoEntryDao.GetPartDefaultAsync(partId);
        }

        public async Task<Model_Dao_Result> SaveNonPOPartDefaultAsync(
            string partId,
            string value,
            string updatedBy
        )
        {
            return await _receivingNonPoEntryDao.UpsertPartDefaultAsync(partId, value, updatedBy);
        }

        public async Task<Model_Dao_Result<int>> InsertFromHistoryAsync(int historyId)
        {
            var currentUser = _sessionManager?.CurrentSession?.User;
            var queuedBy = currentUser?.WindowsUsername ?? "SYSTEM";
            var employeeNumber = currentUser?.EmployeeNumber ?? 0;

            _logger.LogInfo($"Queuing history record {historyId} for reprint by {queuedBy}");
            return await _receivingLabelDataDao.InsertFromHistoryAsync(
                historyId,
                queuedBy,
                employeeNumber
            );
        }

        public async Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
            Model_ReprintHistoryFilter filter
        )
        {
            return await _receivingLoadDao.GetReprintHistoryAsync(filter);
        }

        private async Task<(
            bool HasContext,
            int EmployeeNumber,
            string? WindowsUsername,
            bool HasFullAccess
        )> GetEditModeAccessContextAsync()
        {
            var currentUser = _sessionManager?.CurrentSession?.User;
            if (currentUser == null || currentUser.EmployeeNumber <= 0)
            {
                return (false, 0, null, false);
            }

            if (_userPrivileges != null)
            {
                if (
                    !_userPrivileges.IsInitialized
                    || _userPrivileges.CurrentUserId != currentUser.EmployeeNumber
                )
                {
                    var initializeResult = await _userPrivileges.InitializeAsync(
                        currentUser.EmployeeNumber
                    );
                    if (!initializeResult.Success)
                    {
                        _logger.LogWarning(
                            initializeResult.ErrorMessage
                                ?? "Failed to initialize current user privileges for edit mode.",
                            nameof(Service_MySQL_Receiving)
                        );
                    }
                }

                return (
                    true,
                    currentUser.EmployeeNumber,
                    currentUser.WindowsUsername,
                    _userPrivileges.IsInitialized
                        && _userPrivileges.CurrentUserId == currentUser.EmployeeNumber
                        && _userPrivileges.HasAnyRole("Admin", "Developer")
                );
            }

            return (true, currentUser.EmployeeNumber, currentUser.WindowsUsername, false);
        }

        private static List<Model_ReceivingLoad> ApplyOwnershipFilter(
            IEnumerable<Model_ReceivingLoad> loads,
            int employeeNumber,
            string? windowsUsername,
            bool hasFullAccess
        )
        {
            var sourceLoads = loads?.ToList() ?? new List<Model_ReceivingLoad>();
            if (hasFullAccess)
            {
                return sourceLoads;
            }

            return sourceLoads
                .Where(load => IsLoadOwnedByCurrentUser(load, employeeNumber, windowsUsername))
                .ToList();
        }

        private static List<Model_ReceivingLoad> GetUnauthorizedLoads(
            IEnumerable<Model_ReceivingLoad> loads,
            int employeeNumber,
            string? windowsUsername,
            bool hasFullAccess
        )
        {
            if (hasFullAccess)
            {
                return new List<Model_ReceivingLoad>();
            }

            return (loads ?? Enumerable.Empty<Model_ReceivingLoad>())
                .Where(load => !IsLoadOwnedByCurrentUser(load, employeeNumber, windowsUsername))
                .ToList();
        }

        private static bool IsLoadOwnedByCurrentUser(
            Model_ReceivingLoad load,
            int employeeNumber,
            string? windowsUsername
        )
        {
            if (load == null)
            {
                return false;
            }

            if (
                employeeNumber > 0
                && load.EmployeeNumber > 0
                && load.EmployeeNumber == employeeNumber
            )
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(windowsUsername)
                && !string.IsNullOrWhiteSpace(load.UserId)
                && string.Equals(load.UserId, windowsUsername, StringComparison.OrdinalIgnoreCase);
        }
    }
}
