using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.Services.Navigation;
using System;

namespace MTM_Receiving_Application.Module_Core.Services.Navigation;

/// <summary>
/// Service for handling page navigation
/// </summary>
public class Service_Navigation : IService_Navigation
{
    private readonly IService_LoggingUtility _logger;

    public Service_Navigation(IService_LoggingUtility logger)
    {
        _logger = logger;
    }

    public bool NavigateTo(Frame frame, Type pageType, object? parameter = null)
    {
        if (frame == null)
        {
            _logger.LogError("Navigation frame is null", null, "Service_Navigation");
            return false;
        }

        if (pageType == null)
        {
            _logger.LogError("Page type is null", null, "Service_Navigation");
            return false;
        }

        try
        {
            var result = frame.Navigate(pageType, parameter);
            if (result)
            {
                _logger.LogInfo($"Navigated to {pageType.Name}", "Service_Navigation");
            }
            else
            {
                _logger.LogWarning($"Failed to navigate to {pageType.Name}", "Service_Navigation");
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Navigation error: {ex.Message}", ex, "Service_Navigation");
            return false;
        }
    }

    public bool NavigateTo(Frame frame, string pageTypeName, object? parameter = null)
    {
        try
        {
            var pageType = Type.GetType(pageTypeName);
            if (pageType == null)
            {
                _logger.LogError($"Could not find type: {pageTypeName}", null, "Service_Navigation");
                return false;
            }
            return NavigateTo(frame, pageType, parameter);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Navigation error: {ex.Message}", ex, "Service_Navigation");
            return false;
        }
    }

    public void ClearNavigation(Frame frame)
    {
        if (frame?.Content != null)
        {
            frame.Content = null;
            _logger.LogInfo("Navigation cleared", "Service_Navigation");
        }
    }

    public bool CanGoBack(Frame frame)
    {
        return frame?.CanGoBack ?? false;
    }

    public void GoBack(Frame frame)
    {
        if (frame?.CanGoBack == true)
        {
            frame.GoBack();
            _logger.LogInfo("Navigated back", "Service_Navigation");
        }
    }
}

