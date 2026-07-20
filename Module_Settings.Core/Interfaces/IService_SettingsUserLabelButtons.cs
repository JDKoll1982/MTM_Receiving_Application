using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

public interface IService_SettingsUserLabelButtons
{
    Task<List<Model_MainWindowLabelButton>> GetButtonsAsync();

    Task<Model_Dao_Result> SaveButtonsAsync(List<Model_MainWindowLabelButton> buttons);

    Task<Model_Dao_Result> ResetToDefaultsAsync();
}
