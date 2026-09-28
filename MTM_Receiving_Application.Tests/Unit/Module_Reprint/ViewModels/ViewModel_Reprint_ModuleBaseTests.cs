using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Reprint.Models;
using MTM_Receiving_Application.Module_Reprint.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reprint.ViewModels;

public sealed class ViewModel_Reprint_ModuleBaseTests
{
    [Fact]
    public async Task ActivateAsync_ShouldLoadRowsAndMarkAlreadyQueuedUnselectable()
    {
        var vm = CreateViewModel(
            load: _ =>
                Task.FromResult(
                    Model_Dao_Result_Factory.Success(
                        new List<Model_ReprintHistoryRow>
                        {
                            new()
                            {
                                HistoryId = "1",
                                RecordDate = DateTime.Today,
                                Part = "A",
                                Quantity = 1,
                                AlreadyQueued = false,
                            },
                            new()
                            {
                                HistoryId = "2",
                                RecordDate = DateTime.Today,
                                Part = "B",
                                Quantity = 2,
                                AlreadyQueued = true,
                            },
                        }
                    )
                )
        );

        await vm.ActivateAsync();

        vm.HistoryRows.Should().HaveCount(2);
        vm.HistoryRows[0].CanSelect.Should().BeTrue();
        vm.HistoryRows[1].CanSelect.Should().BeFalse();
        vm.HistoryRows[1].AlreadyQueued.Should().BeTrue();
        vm.IsSelectAllEnabled.Should().BeTrue();
        vm.SelectedCount.Should().Be(0);
        vm.CanReprint.Should().BeFalse();
    }

    [Fact]
    public async Task SelectAll_ShouldSelectOnlySelectableRows_WhenChecked()
    {
        var vm = CreateViewModel(load: _ => Task.FromResult(Model_Dao_Result_Factory.Success(TestRows())));
        await vm.ActivateAsync();

        vm.IsSelectAllChecked = true;

        vm.HistoryRows[0].IsSelected.Should().BeTrue();
        vm.HistoryRows[1].IsSelected.Should().BeFalse(); // already queued
        vm.SelectedCount.Should().Be(1);
        vm.CanReprint.Should().BeTrue();
    }

    [Fact]
    public async Task ReprintAsync_ShouldRaiseReprintCompleted_WithBatchResult()
    {
        Model_ReprintBatchResult? captured = null;
        var reprintResult = new Model_ReprintBatchResult();
        reprintResult.Queued.Add("1");

        var vm = CreateViewModel(
            load: _ => Task.FromResult(Model_Dao_Result_Factory.Success(TestRows())),
            reprint: _ => Task.FromResult(Model_Dao_Result_Factory.Success(reprintResult))
        );
        vm.ReprintCompleted += (_, result) => captured = result;

        await vm.ActivateAsync();
        vm.HistoryRows[0].IsSelected = true;

        await vm.ReprintCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured.QueuedCount.Should().Be(1);
        vm.SelectedCount.Should().Be(0); // selections cleared after reload
    }

    [Fact]
    public async Task ResetAsync_ShouldRestoreTodayAndClearSearch_WhenInvoked()
    {
        var reloadCount = 0;
        var vm = CreateViewModel(
            load: _ =>
            {
                reloadCount++;
                return Task.FromResult(Model_Dao_Result_Factory.Success(new List<Model_ReprintHistoryRow>()));
            }
        );
        await vm.ActivateAsync();

        vm.StartDate = DateTime.Today.AddDays(-5);
        vm.EndDate = DateTime.Today.AddDays(-1);
        vm.SearchText = "abc";
        var before = reloadCount;

        await vm.ResetCommand.ExecuteAsync(null);

        vm.StartDate!.Value.Date.Should().Be(DateTime.Today);
        vm.EndDate!.Value.Date.Should().Be(DateTime.Today);
        vm.SearchText.Should().BeEmpty();
        reloadCount.Should().BeGreaterThan(before);
    }

    [Fact]
    public async Task BackCommand_ShouldRaiseBackRequested_WhenInvoked()
    {
        var backRequested = false;
        var vm = CreateViewModel();
        vm.BackRequested += (_, _) => backRequested = true;

        vm.BackCommand.Execute(null);

        backRequested.Should().BeTrue();
    }

    private static List<Model_ReprintHistoryRow> TestRows() =>
    [
        new()
        {
            HistoryId = "1",
            RecordDate = DateTime.Today,
            Part = "A",
            Quantity = 1,
            AlreadyQueued = false,
        },
        new()
        {
            HistoryId = "2",
            RecordDate = DateTime.Today,
            Part = "B",
            Quantity = 2,
            AlreadyQueued = true,
        },
    ];

    private static TestReprintViewModel CreateViewModel(
        Func<Model_ReprintHistoryFilter, Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>>>? load = null,
        Func<IReadOnlyList<string>, Task<Model_Dao_Result<Model_ReprintBatchResult>>>? reprint = null
    )
    {
        var settingsCore = new Mock<IService_SettingsCoreFacade>();
        settingsCore
            .Setup(s => s.GetSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_SettingsValue>("not configured"));
        settingsCore
            .Setup(s => s.SetSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        return new TestReprintViewModel(
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object,
            settingsCore.Object,
            new Mock<IService_UserSessionManager>().Object,
            load
                ?? (_ =>
                    Task.FromResult(
                        Model_Dao_Result_Factory.Success(new List<Model_ReprintHistoryRow>())
                    )
                ),
            reprint
                ?? (_ =>
                    Task.FromResult(Model_Dao_Result_Factory.Success(new Model_ReprintBatchResult()))
                )
        );
    }

    private sealed class TestReprintViewModel : ViewModel_Reprint_ModuleBase
    {
        private readonly Func<
            Model_ReprintHistoryFilter,
            Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>>
        > _load;
        private readonly Func<
            IReadOnlyList<string>,
            Task<Model_Dao_Result<Model_ReprintBatchResult>>
        > _reprint;

        protected override string SearchBySettingsKey => "UserPreferences.TestSearchBy";

        protected override string ColumnOptionsSettingsKey => "UserPreferences.TestColumns";

        protected override string ModuleDisplayName => "Test";

        protected override IReadOnlyList<Model_ReprintSearchByOption> BuildSearchByOptions() =>
            [new() { Key = "part", Label = "Part Number" }];

        protected override Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> LoadHistoryAsync(
            Model_ReprintHistoryFilter filter
        ) => _load(filter);

        protected override Task<Model_Dao_Result<Model_ReprintBatchResult>> ExecuteReprintAsync(
            IReadOnlyList<string> historyIds
        ) => _reprint(historyIds);

        public TestReprintViewModel(
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService,
            IService_SettingsCoreFacade settingsCore,
            IService_UserSessionManager sessionManager,
            Func<Model_ReprintHistoryFilter, Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>>> load,
            Func<IReadOnlyList<string>, Task<Model_Dao_Result<Model_ReprintBatchResult>>> reprint
        )
            : base(errorHandler, logger, notificationService, settingsCore, sessionManager)
        {
            _load = load;
            _reprint = reprint;
        }
    }
}
