using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Contracts.Services
{
    public interface IService_DunnageCSVWriter
    {
        Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads);
    }
}
