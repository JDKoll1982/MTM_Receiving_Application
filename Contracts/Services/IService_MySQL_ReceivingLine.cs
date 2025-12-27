using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Contracts.Services;

public interface IService_MySQL_ReceivingLine
{
    /// <summary>Insert a new receiving line</summary>
    Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line);

    // Add other methods if needed by ReceivingLabelViewModel
}
