using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Reporting.Services;

/// <summary>
/// Reporting-specific recipient settings service used by settings pages and preview clipboard actions.
/// </summary>
public class Service_ReportingRecipientSettings : IService_ReportingRecipientSettings
{
    private readonly Dao_ReportingRecipientSettings _dao;

    public Service_ReportingRecipientSettings(Dao_ReportingRecipientSettings dao)
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
                result.ErrorMessage ?? "Failed to load Reporting recipients."
            );
        }

        return Model_Dao_Result_Factory.Success(
            Helper_EmailRecipientFormatting.BuildFormattedRecipientList(result.Data, recipientType)
        );
    }
}
