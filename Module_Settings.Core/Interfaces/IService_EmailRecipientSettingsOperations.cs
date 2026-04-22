using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Shared operations surface for module-specific email recipient settings.
/// </summary>
public interface IService_EmailRecipientSettingsOperations
{
    Task<Model_Dao_Result<List<Model_EmailRecipientSetting>>> GetRecipientsAsync();

    Task<Model_Dao_Result> SaveRecipientAsync(Model_EmailRecipientSetting recipient);

    Task<Model_Dao_Result> DeleteRecipientAsync(int recipientId);

    Task<Model_Dao_Result<string>> GetFormattedRecipientsAsync(string recipientType);
}
