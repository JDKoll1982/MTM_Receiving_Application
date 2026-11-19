using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.UI;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

/// <summary>
/// The shell's bindable projection of the active page's shell actions.
/// </summary>
/// <remarks>
/// The resolved provider's type is only known at run time, so the bar binds to this fixed
/// surface while every value still comes from the page. The projection holds no domain logic: it
/// copies the provider's snapshot and forwards the three default-checkbox ticks back through
/// <see cref="IViewModel_ShellActionProvider.SetDefaultMode"/>.
/// </remarks>
public partial class ViewModel_Shared_ShellActionBar : ObservableObject
{
    private IViewModel_ShellActionProvider? _provider;
    private bool _isProjecting;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _showModeGroup;

    [ObservableProperty]
    private Model_ShellModeAction _guided = Model_ShellModeAction.Hidden;

    [ObservableProperty]
    private Model_ShellModeAction _manual = Model_ShellModeAction.Hidden;

    [ObservableProperty]
    private Model_ShellModeAction _edit = Model_ShellModeAction.Hidden;

    [ObservableProperty]
    private Model_ShellModeAction _overlay = Model_ShellModeAction.Hidden;

    [ObservableProperty]
    private Model_ShellActionSlot _back = Model_ShellActionSlot.Hidden;

    [ObservableProperty]
    private Model_ShellActionSlot _next = Model_ShellActionSlot.Hidden;

    [ObservableProperty]
    private Model_ShellActionSlot _clearLabelData = Model_ShellActionSlot.Hidden;

    [ObservableProperty]
    private Model_ShellActionSlot _help = Model_ShellActionSlot.Hidden;

    [ObservableProperty]
    private bool? _isGuidedDefault;

    [ObservableProperty]
    private bool? _isManualDefault;

    [ObservableProperty]
    private bool? _isEditDefault;

    /// <summary>
    /// Starts following a page's provider and shows its first snapshot.
    /// </summary>
    /// <param name="provider">The provider the active page supplied.</param>
    public void Attach(IViewModel_ShellActionProvider provider)
    {
        Detach();

        _provider = provider;
        provider.PropertyChanged += OnProviderPropertyChanged;
        Project();
    }

    /// <summary>
    /// Stops following the current page and collapses the bar immediately, so the page takes the
    /// full remaining height instead of waiting for a later provider.
    /// </summary>
    public void Detach()
    {
        if (_provider is not null)
        {
            _provider.PropertyChanged -= OnProviderPropertyChanged;
            _provider = null;
        }

        _isProjecting = true;
        try
        {
            IsVisible = false;
            ShowModeGroup = false;
            Guided = Model_ShellModeAction.Hidden;
            Manual = Model_ShellModeAction.Hidden;
            Edit = Model_ShellModeAction.Hidden;
            Overlay = Model_ShellModeAction.Hidden;
            Back = Model_ShellActionSlot.Hidden;
            Next = Model_ShellActionSlot.Hidden;
            ClearLabelData = Model_ShellActionSlot.Hidden;
            Help = Model_ShellActionSlot.Hidden;
            IsGuidedDefault = false;
            IsManualDefault = false;
            IsEditDefault = false;
        }
        finally
        {
            _isProjecting = false;
        }
    }

    partial void OnIsGuidedDefaultChanged(bool? value) => ApplyDefaultChange(value == true, "guided");

    partial void OnIsManualDefaultChanged(bool? value) => ApplyDefaultChange(value == true, "manual");

    partial void OnIsEditDefaultChanged(bool? value) => ApplyDefaultChange(value == true, "edit");

    private void ApplyDefaultChange(bool isTicked, string mode)
    {
        if (_isProjecting || _provider is null)
        {
            return;
        }

        _provider.SetDefaultMode(isTicked ? mode : null);

        // Re-reading the snapshot is what clears the other two boxes, so one place decides.
        Project();
    }

    private void OnProviderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName)
            || e.PropertyName == nameof(IViewModel_ShellActionProvider.ShellActions))
        {
            Project();
        }
    }

    private void Project()
    {
        Model_ShellActionBarState state = _provider?.ShellActions ?? Model_ShellActionBarState.Empty;

        _isProjecting = true;
        try
        {
            ShowModeGroup = state.ShowModeGroup;
            Guided = state.Guided;
            Manual = state.Manual;
            Edit = state.Edit;
            Overlay = state.Overlay;
            Back = state.Back;
            Next = state.Next;
            ClearLabelData = state.ClearLabelData;
            Help = state.Help;
            IsGuidedDefault = state.IsGuidedDefault;
            IsManualDefault = state.IsManualDefault;
            IsEditDefault = state.IsEditDefault;

            IsVisible = _provider is not null && state.IsVisible && HasAnyAction(state);
        }
        finally
        {
            _isProjecting = false;
        }
    }

    private static bool HasAnyAction(Model_ShellActionBarState state) =>
        state.ShowModeGroup
        || state.Back.IsVisible
        || state.Next.IsVisible
        || state.ClearLabelData.IsVisible
        || state.Help.IsVisible;
}
