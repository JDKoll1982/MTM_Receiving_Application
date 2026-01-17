using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for system-level core settings.
/// </summary>
public partial class ViewModel_Settings_System : ViewModel_Shared_Base
{
    private readonly ISettingsMetadataRegistry _registry;

    [ObservableProperty]
    private ObservableCollection<Model_SettingsDefinition> _definitions;

    public ViewModel_Settings_System(
        ISettingsMetadataRegistry registry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _registry = registry;
        Definitions = new ObservableCollection<Model_SettingsDefinition>(
            _registry.GetAll().Where(d => d.Scope == Enum_SettingsScope.System));
    }
}
