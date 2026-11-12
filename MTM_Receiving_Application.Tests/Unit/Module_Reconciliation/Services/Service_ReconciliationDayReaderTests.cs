using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Data;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.Services;

/// <summary>
/// The day reader decides activity from receiving rows and steps days from the
/// cheap range query, never from a preview (FR-009, FR-010, FR-016).
/// </summary>
public sealed class Service_ReconciliationDayReaderTests
{
    private const string DummyConnectionString =
        "Server=localhost;Database=test;Uid=test;Pwd=test;Connection Timeout=1;";

    private static readonly DateOnly WorkingDay = new(2026, 11, 12);

    [Fact]
    public async Task GetDayShouldExcludeReprintsAndReportTheCount()
    {
        var dao = new Mock<Dao_ReconciliationReceivingRows>(DummyConnectionString, Mock.Of<IService_LoggingUtility>());
        dao.Setup(instance => instance.GetDayRowsAsync(WorkingDay))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_ReconciliationRow>
                    {
                        Row(1_000, isReprint: false),
                        Row(500, isReprint: true),
                    }
                )
            );

        var reader = new Service_ReconciliationDayReader(dao.Object);

        Model_Dao_Result<Model_ReconciliationDay> result = await reader.GetDayAsync(WorkingDay);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Rows.Should().ContainSingle();
        result.Data.Summary.ExcludedReprintCount.Should().Be(1);
        result.Data.Summary.Scanned.Should().Be(1);
        result.Data.HasActivity.Should().BeTrue();
        result.Data.State.Should().Be(Enum_ReconciliationDayState.Loaded);
    }

    [Fact]
    public async Task GetDayShouldReportNoActivityWhenTheDayHasNoRows()
    {
        var dao = new Mock<Dao_ReconciliationReceivingRows>(DummyConnectionString, Mock.Of<IService_LoggingUtility>());
        dao.Setup(instance => instance.GetDayRowsAsync(WorkingDay))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ReconciliationRow>())
            );

        var reader = new Service_ReconciliationDayReader(dao.Object);

        Model_Dao_Result<Model_ReconciliationDay> result = await reader.GetDayAsync(WorkingDay);

        result.Data!.HasActivity.Should().BeFalse();
        result.Data.Rows.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActivityDatesShouldComeFromTheRangeQuery()
    {
        var dao = new Mock<Dao_ReconciliationReceivingRows>(DummyConnectionString, Mock.Of<IService_LoggingUtility>());
        dao.Setup(instance =>
                instance.GetActivityDatesAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30))
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<DateOnly> { new(2026, 11, 5), WorkingDay }
                )
            );

        var reader = new Service_ReconciliationDayReader(dao.Object);

        Model_Dao_Result<IReadOnlyList<DateOnly>> result = await reader.GetActivityDatesAsync(
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 30)
        );

        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindNearestActiveDayShouldAnswerTheNearestDayInEitherDirection()
    {
        var dao = new Mock<Dao_ReconciliationReceivingRows>(DummyConnectionString, Mock.Of<IService_LoggingUtility>());
        dao.Setup(instance =>
                instance.GetActivityDatesAsync(It.IsAny<DateOnly>(), WorkingDay.AddDays(-1))
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<DateOnly> { new(2026, 11, 9), new(2026, 11, 10) }
                )
            );
        dao.Setup(instance =>
                instance.GetActivityDatesAsync(WorkingDay.AddDays(1), It.IsAny<DateOnly>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<DateOnly> { new(2026, 11, 14) })
            );

        var reader = new Service_ReconciliationDayReader(dao.Object);

        Model_Dao_Result<DateOnly?> earlier = await reader.FindNearestActiveDayAsync(WorkingDay, -1);
        Model_Dao_Result<DateOnly?> later = await reader.FindNearestActiveDayAsync(WorkingDay, 1);

        earlier.Data.Should().Be(new DateOnly(2026, 11, 10));
        later.Data.Should().Be(new DateOnly(2026, 11, 14));
        dao.Verify(
            instance => instance.GetDayRowsAsync(It.IsAny<DateOnly>()),
            Times.Never,
            "stepping a day must never run a preview"
        );
    }

    [Fact]
    public async Task FindNearestActiveDayShouldReturnNullWhenTheRangeIsExhausted()
    {
        var dao = new Mock<Dao_ReconciliationReceivingRows>(DummyConnectionString, Mock.Of<IService_LoggingUtility>());
        dao.Setup(instance =>
                instance.GetActivityDatesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>())
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<DateOnly>()));

        var reader = new Service_ReconciliationDayReader(dao.Object);

        Model_Dao_Result<DateOnly?> result = await reader.FindNearestActiveDayAsync(WorkingDay, 1);

        result.Data.Should().BeNull();
    }

    private static Model_ReconciliationRow Row(int quantity, bool isReprint) =>
        new()
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = WorkingDay,
            Quantity = quantity,
            SavedLocation = "RECV",
            IsReprint = isReprint,
        };
}
