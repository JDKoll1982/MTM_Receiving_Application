using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service for providing contextual help content throughout the application
/// </summary>
public interface IService_Help
{
    /// <summary>
    /// Gets help content for a dunnage workflow step
    /// </summary>
    /// <param name="step"></param>
    public string GetDunnageWorkflowHelp(Enum_DunnageWorkflowStep step);

    /// <summary>
    /// Gets help content for a receiving workflow step
    /// </summary>
    /// <param name="step"></param>
    public string GetReceivingWorkflowHelp(Enum_ReceivingWorkflowStep step);

    /// <summary>
    /// Gets tip content for a specific view or feature
    /// </summary>
    /// <param name="viewName"></param>
    public string GetTip(string viewName);

    /// <summary>
    /// Gets placeholder text for a specific input field
    /// </summary>
    /// <param name="fieldName"></param>
    public string GetPlaceholder(string fieldName);

    /// <summary>
    /// Gets tooltip content for a specific UI element
    /// </summary>
    /// <param name="elementName"></param>
    public string GetTooltip(string elementName);

    /// <summary>
    /// Gets InfoBar message content
    /// </summary>
    /// <param name="messageKey"></param>
    public string GetInfoBarMessage(string messageKey);
}
