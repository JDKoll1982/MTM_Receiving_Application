namespace MTM_Receiving_Application.Module_Core.Defaults;

/// <summary>
/// Centralized workstation configuration defaults.
/// Prefer loading these values from configuration where possible.
/// </summary>
public static class WorkstationDefaults
{
    public const string SharedTerminalWorkstationType = "shared_terminal";
    public const string PersonalWorkstationWorkstationType = "personal_workstation";

    public const int SharedTerminalTimeoutMinutes = 15;
    public const int PersonalWorkstationTimeoutMinutes = 30;
}
