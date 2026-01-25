using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

public interface IService_Receiving_Business_MySQL_ReceivingLine
{
    /// <summary>Insert a new receiving line</summary>
    /// <param name="line"></param>
    public Task<Model_Dao_Result> InsertReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line);

    // Add other methods if needed by ReceivingLabelViewModel
}

