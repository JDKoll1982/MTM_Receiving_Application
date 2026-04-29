using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Volvo.Contracts;

public interface IService_VolvoSettings
{
    Task<string> GetStringAsync(string key, int? userId = null);

    Task SaveStringAsync(string key, string value, int? userId = null);
}
