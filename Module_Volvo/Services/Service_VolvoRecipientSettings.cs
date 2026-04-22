using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Volvo-specific recipient settings service used by settings pages and email preview flows.
/// </summary>
public class Service_VolvoRecipientSettings : IService_VolvoRecipientSettings
{
    private readonly Dao_VolvoRecipientSettings _dao;

    public Service_VolvoRecipientSettings(Dao_VolvoRecipientSettings dao)
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
    }

    public async Task<Model_Dao_Result<List<Model_EmailRecipientSetting>>> GetRecipientsAsync()
    {
        return await _dao.GetAllAsync();
    }

    public async Task<Model_Dao_Result> SaveRecipientAsync(Model_EmailRecipientSetting recipient)
    {
        if (recipient is null)
        {
            return Model_Dao_Result_Factory.Failure("Recipient cannot be null.");
        }

        if (recipient.Id > 0)
        {
            return await _dao.UpdateAsync(recipient);
        }

        return await _dao.InsertAsync(recipient);
    }

    public async Task<Model_Dao_Result> DeleteRecipientAsync(int recipientId)
    {
        return await _dao.DeleteAsync(recipientId);
    }

    public async Task<Model_Dao_Result<string>> GetFormattedRecipientsAsync(string recipientType)
    {
        var result = await _dao.GetAllAsync();
        if (!result.IsSuccess || result.Data == null)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                result.ErrorMessage ?? "Failed to load Volvo recipients."
            );
        }

        return Model_Dao_Result_Factory.Success(
            Helper_EmailRecipientFormatting.BuildFormattedRecipientList(result.Data, recipientType)
        );
    }
}
