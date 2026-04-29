using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

public interface IService_LabelViewLauncher
{
    string DefaultExecutablePath { get; }

    bool IsExecutablePathValid(string executablePath);

    bool IsLabelFilePathValid(string labelFilePath);

    Task<string?> ResolveExecutablePathAsync();

    Task<Model_Dao_Result> LaunchLabelAsync(string labelFilePath);
}
