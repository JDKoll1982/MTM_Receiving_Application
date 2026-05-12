using System.ComponentModel;
using Material.Icons;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

/// <summary>
/// Provides the current title for the shared main-window header.
/// </summary>
public interface IViewModel_HeaderTitleProvider : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the current title that should be shown in the main-window header.
    /// </summary>
    string CurrentHeaderTitle { get; }

    /// <summary>
    /// Gets the optional rich header title shown below the standard page title.
    /// </summary>
    string? CurrentHeaderContextTitle => null;

    /// <summary>
    /// Gets the optional supporting text shown below the rich header title.
    /// </summary>
    string? CurrentHeaderContextSubtitle => null;

    /// <summary>
    /// Gets the optional image shown beside the rich header title.
    /// </summary>
    ImageSource? CurrentHeaderContextImageSource => null;

    /// <summary>
    /// Gets the optional material icon shown when no rich header image is available.
    /// </summary>
    MaterialIconKind? CurrentHeaderContextIconKind => null;
}