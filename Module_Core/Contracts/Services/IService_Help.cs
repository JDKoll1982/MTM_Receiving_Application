using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums; // For other enums if needed
namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Service for providing contextual help content throughout the application
/// </summary>
public interface IService_Help
{
    /// <summary>
    /// Displays help dialog with content for the specified key
    /// </summary>
    /// <param name="helpKey">The unique key for the help content</param>
    public Task ShowHelpAsync(string helpKey);

    /// <summary>
    /// Shows help for specific dunnage workflow step
    /// </summary>
    /// <param name="step">The workflow step enum</param>
    public Task ShowContextualHelpAsync(Enum_DunnageWorkflowStep step);

    /// <summary>
    /// Shows help for specific receiving workflow step
    /// </summary>
    /// <param name="step">The workflow step enum</param>
    public Task ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step);

    /// <summary>
    /// Retrieves help content by key without showing dialog
    /// </summary>
    /// <param name="key">The unique help key</param>
    /// <returns>Model_HelpContent if found, null otherwise</returns>
    public Model_HelpContent? GetHelpContent(string key);

    /// <summary>
    /// Gets all help content for a category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <returns>List of help content items</returns>
    public List<Model_HelpContent> GetHelpByCategory(string category);

    /// <summary>
    /// Searches help content by title or content
    /// </summary>
    /// <param name="searchTerm">Search string</param>
    /// <returns>List of matching help content items</returns>
    public List<Model_HelpContent> SearchHelp(string searchTerm);

    /// <summary>
    /// Checks if user has dismissed a tip/help permanently
    /// </summary>
    /// <param name="helpKey">The help key to check</param>
    /// <returns>true if dismissed, false otherwise</returns>
    public Task<bool> IsDismissedAsync(string helpKey);

    /// <summary>
    /// Sets whether a tip/help is dismissed
    /// </summary>
    /// <param name="helpKey">The help key</param>
    /// <param name="isDismissed">Whether it should be dismissed</param>
    public Task SetDismissedAsync(string helpKey, bool isDismissed);

    /// <summary>
    /// Gets help content for a dunnage workflow step
    /// </summary>
    /// <param name="step">The workflow step</param>
    public string GetDunnageWorkflowHelp(Enum_DunnageWorkflowStep step);

    /// <summary>
    /// Gets help content for a receiving workflow step
    /// </summary>
    /// <param name="step">The workflow step</param>
    public string GetReceivingWorkflowHelp(Enum_ReceivingWorkflowStep step);

    /// <summary>
    /// Gets tip content for a specific view or feature
    /// </summary>
    /// <param name="viewName">The view name</param>
    public string GetTip(string viewName);

    /// <summary>
    /// Gets placeholder text for a specific input field
    /// </summary>
    /// <param name="fieldName">The field name</param>
    public string GetPlaceholder(string fieldName);

    /// <summary>
    /// Gets tooltip content for a specific UI element
    /// </summary>
    /// <param name="elementName">The element name</param>
    public string GetTooltip(string elementName);

    /// <summary>
    /// Gets InfoBar message content
    /// </summary>
    /// <param name="messageKey">The message key</param>
    public string GetInfoBarMessage(string messageKey);
}


