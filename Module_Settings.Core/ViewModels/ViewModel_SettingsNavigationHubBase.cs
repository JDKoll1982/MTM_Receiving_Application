using System.Collections.ObjectModel;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

public abstract partial class ViewModel_SettingsNavigationHubBase : ViewModel_Shared_Base
{
    private readonly IService_SettingsPagination _pagination;

    private string _navigationTitle = "Settings";
    private string _currentStepTitle = "Settings";
    private int _currentButtonPage = 1;
    private int _totalButtonPages;
    private bool _isPaginationVisible;
    private bool _isBackVisible;
    private bool _isNextVisible;
    private bool _isCancelVisible;
    private bool _isSaveVisible;
    private bool _isResetVisible;

    protected ViewModel_SettingsNavigationHubBase(
        IService_SettingsPagination pagination,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _pagination = pagination;
        Steps = new ObservableCollection<Model_SettingsNavigationStep>();
        VisibleSteps = new ObservableCollection<Model_SettingsNavigationStep>();

        RecalculatePagination();
    }

    public ObservableCollection<Model_SettingsNavigationStep> Steps { get; }

    public ObservableCollection<Model_SettingsNavigationStep> VisibleSteps { get; }

    public string NavigationTitle
    {
        get => _navigationTitle;
        set
        {
            if (_navigationTitle == value)
            {
                return;
            }

            _navigationTitle = value;
            OnPropertyChanged();
        }
    }

    public string CurrentStepTitle
    {
        get => _currentStepTitle;
        set
        {
            if (_currentStepTitle == value)
            {
                return;
            }

            _currentStepTitle = value;
            OnPropertyChanged();
        }
    }

    public int CurrentButtonPage
    {
        get => _currentButtonPage;
        set
        {
            if (_currentButtonPage == value)
            {
                return;
            }

            _currentButtonPage = value;
            OnPropertyChanged();
            RebuildVisibleSteps();
        }
    }

    public int TotalButtonPages
    {
        get => _totalButtonPages;
        private set
        {
            if (_totalButtonPages == value)
            {
                return;
            }

            _totalButtonPages = value;
            OnPropertyChanged();
        }
    }

    public bool IsPaginationVisible
    {
        get => _isPaginationVisible;
        private set
        {
            if (_isPaginationVisible == value)
            {
                return;
            }

            _isPaginationVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsBackVisible { get => _isBackVisible; protected set { if (_isBackVisible != value) { _isBackVisible = value; OnPropertyChanged(); } } }
    public bool IsNextVisible { get => _isNextVisible; protected set { if (_isNextVisible != value) { _isNextVisible = value; OnPropertyChanged(); } } }
    public bool IsCancelVisible { get => _isCancelVisible; protected set { if (_isCancelVisible != value) { _isCancelVisible = value; OnPropertyChanged(); } } }
    public bool IsSaveVisible { get => _isSaveVisible; protected set { if (_isSaveVisible != value) { _isSaveVisible = value; OnPropertyChanged(); } } }
    public bool IsResetVisible { get => _isResetVisible; protected set { if (_isResetVisible != value) { _isResetVisible = value; OnPropertyChanged(); } } }

    public void SetSteps(params Model_SettingsNavigationStep[] steps)
    {
        Steps.Clear();
        foreach (var step in steps.Where(s => s != null))
        {
            Steps.Add(step);
        }

        CurrentButtonPage = 1;
        RecalculatePagination();
    }

    public void ApplyNavState(ISettingsNavigationNavState? state)
    {
        IsBackVisible = state?.IsBackVisible ?? false;
        IsNextVisible = state?.IsNextVisible ?? false;
        IsCancelVisible = state?.IsCancelVisible ?? false;
        IsSaveVisible = state?.IsSaveVisible ?? false;
        IsResetVisible = state?.IsResetVisible ?? false;
    }

    private void RecalculatePagination()
    {
        TotalButtonPages = _pagination.GetTotalPages(Steps.Count);
        IsPaginationVisible = _pagination.ShouldShowPagination(Steps.Count);

        if (CurrentButtonPage <= 0)
        {
            CurrentButtonPage = 1;
        }

        if (TotalButtonPages > 0 && CurrentButtonPage > TotalButtonPages)
        {
            CurrentButtonPage = TotalButtonPages;
        }

        RebuildVisibleSteps();
    }

    private void RebuildVisibleSteps()
    {
        VisibleSteps.Clear();
        var indices = _pagination.GetPageIndices(Steps.Count, CurrentButtonPage);
        foreach (var idx in indices)
        {
            if (idx >= 0 && idx < Steps.Count)
            {
                VisibleSteps.Add(Steps[idx]);
            }
        }
    }

    public void PrevButtonPage()
    {
        if (!CanPrevButtonPage())
        {
            return;
        }

        CurrentButtonPage--;
    }

    private bool CanPrevButtonPage()
    {
        return IsPaginationVisible && CurrentButtonPage > 1;
    }

    public void NextButtonPage()
    {
        if (!CanNextButtonPage())
        {
            return;
        }

        CurrentButtonPage++;
    }

    private bool CanNextButtonPage()
    {
        return IsPaginationVisible && TotalButtonPages > 0 && CurrentButtonPage < TotalButtonPages;
    }
}
