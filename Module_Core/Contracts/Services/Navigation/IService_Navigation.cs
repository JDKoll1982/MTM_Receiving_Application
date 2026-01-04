using Microsoft.UI.Xaml.Controls;
using System;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services.Navigation;

/// <summary>
/// Service for handling page navigation
/// </summary>
public interface IService_Navigation
{
    /// <summary>
    /// Navigate to a page type
    /// </summary>
    /// <param name="frame">The frame to navigate in</param>
    /// <param name="pageType">The type of page to navigate to</param>
    /// <param name="parameter">Optional navigation parameter</param>
    /// <returns>True if navigation succeeded</returns>
    public bool NavigateTo(Frame frame, Type pageType, object? parameter = null);

    /// <summary>
    /// Navigate to a page type by name
    /// </summary>
    /// <param name="frame">The frame to navigate in</param>
    /// <param name="pageTypeName">Full name of the page type</param>
    /// <param name="parameter">Optional navigation parameter</param>
    /// <returns>True if navigation succeeded</returns>
    public bool NavigateTo(Frame frame, string pageTypeName, object? parameter = null);

    /// <summary>
    /// Clear the navigation frame
    /// </summary>
    /// <param name="frame">The frame to clear</param>
    public void ClearNavigation(Frame frame);

    /// <summary>
    /// Check if navigation can go back
    /// </summary>
    /// <param name="frame"></param>
    public bool CanGoBack(Frame frame);

    /// <summary>
    /// Navigate back
    /// </summary>
    /// <param name="frame"></param>
    public void GoBack(Frame frame);
}

