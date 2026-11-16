using System.Collections.Generic;
using System.ComponentModel;

namespace MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

/// <summary>
/// Provides the guided-walk step bar that the shared main-window header renders beside the title.
/// </summary>
/// <remarks>
/// The header owns the control, so a page never draws its own step bar. The provider only reports
/// state; it cannot start or skip a step, which keeps the walk's Next/Back buttons the only way to
/// move through it.
/// </remarks>
public interface IViewModel_HeaderStepBarProvider : INotifyPropertyChanged
{
    /// <summary>
    /// Gets a value indicating whether the header should show the step bar. False for every page
    /// that is not part of a guided walk, so the header stays clean elsewhere.
    /// </summary>
    bool IsHeaderStepBarVisible { get; }

    /// <summary>
    /// Gets the step labels in walk order. The bar shows one circle per entry.
    /// </summary>
    IReadOnlyList<string> HeaderStepBarSteps { get; }

    /// <summary>
    /// Gets the zero-based index of the step the operator is on, or -1 when the walk is not active.
    /// </summary>
    int HeaderStepBarIndex { get; }
}
