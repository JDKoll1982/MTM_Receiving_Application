using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

public sealed class Service_ReceivingWorkflowMockDataTests
{
    [Fact]
    public async Task SaveToDatabaseOnlyAsync_ShouldAppendMockReceivingTransactions_WhenMockDataIsEnabled()
    {
        var sessionManagerMock = new Mock<IService_SessionManager>();
        var receivingLabelDataMock = new Mock<IService_ReceivingLabelData>();
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var validationMock = new Mock<IService_ReceivingValidation>();
        var qualityHoldWarningMock = new Mock<IService_QualityHoldWarning>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var appSettingsMock = new Mock<IService_AppSettings>();
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        var viewModelRegistryMock = new Mock<IService_ViewModelRegistry>();
        var userSessionManagerMock = new Mock<IService_UserSessionManager>();

        validationMock
            .Setup(service => service.ValidateSession(It.IsAny<List<Model_ReceivingLoad>>()))
            .Returns(Model_ReceivingValidationResult.Success());
        receivingSettingsMock
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);
        qualityHoldWarningMock
            .Setup(service =>
                service.ConfirmBeforeSaveAsync(It.IsAny<IReadOnlyList<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(true);
        mySqlReceivingMock
            .Setup(service =>
                service.SaveReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(1);
        mockCatalog
            .Setup(service => service.GetCatalog())
            .Returns(
                new Model_InforVisualMockDataCatalog
                {
                    Locations = new List<string> { "W-K0-10", "W-K0-11", "W-K0-12" },
                    PurchaseOrders =
                    [
                        new Model_InforVisualPO
                        {
                            PONumber = "PO-054562B",
                            Parts =
                            [
                                new Model_InforVisualPart
                                {
                                    PartID = "FHS-0420-20",
                                    POLineNumber = "1",
                                    DefaultLocationId = "W-K0-09",
                                },
                            ],
                        },
                    ],
                }
            );
        mockCatalog
            .Setup(service =>
                service.AppendReceivingTransactionsAsync(
                    It.IsAny<IEnumerable<Model_InforVisualMockReceivingTransaction>>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(1, 1));
        sessionManagerMock.Setup(service => service.SessionExists()).Returns(false);

        var service = new Service_ReceivingWorkflow(
            sessionManagerMock.Object,
            receivingLabelDataMock.Object,
            mySqlReceivingMock.Object,
            validationMock.Object,
            qualityHoldWarningMock.Object,
            receivingSettingsMock.Object,
            appSettingsMock.Object,
            mockCatalog.Object,
            new Mock<IService_LoggingUtility>().Object,
            viewModelRegistryMock.Object,
            userSessionManagerMock.Object
        );

        await service.StartWorkflowAsync();
        service.CurrentSession.Loads.Add(
            new Model_ReceivingLoad
            {
                LoadID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PoNumber = "54562B",
                PartID = "FHS-0420-20",
                PoLineNumber = "1",
                InitialLocation = "W-K0-07",
                WeightQuantity = 120,
                UnitOfMeasure = "EA",
                ReceivedDate = new DateTime(2026, 4, 5, 9, 30, 0),
            }
        );

        var result = await service.SaveToDatabaseOnlyAsync();

        result.Success.Should().BeTrue();
        mockCatalog.Verify(
            service =>
                service.AppendReceivingTransactionsAsync(
                    It.Is<IEnumerable<Model_InforVisualMockReceivingTransaction>>(transactions =>
                        transactions.Count() == 1
                        && transactions.First().PONumber == "PO-054562B"
                        && transactions.First().CurrentLocationId == "W-K0-09"
                        && transactions.First().ReceiptLocationId == "W-K0-07"
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SaveToDatabaseOnlyAsync_ShouldCreateDistinctMockDestinationLocations_WhenMultipleLoadsAreSaved()
    {
        var sessionManagerMock = new Mock<IService_SessionManager>();
        var receivingLabelDataMock = new Mock<IService_ReceivingLabelData>();
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var validationMock = new Mock<IService_ReceivingValidation>();
        var qualityHoldWarningMock = new Mock<IService_QualityHoldWarning>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var appSettingsMock = new Mock<IService_AppSettings>();
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        var viewModelRegistryMock = new Mock<IService_ViewModelRegistry>();
        var userSessionManagerMock = new Mock<IService_UserSessionManager>();

        validationMock
            .Setup(service => service.ValidateSession(It.IsAny<List<Model_ReceivingLoad>>()))
            .Returns(Model_ReceivingValidationResult.Success());
        receivingSettingsMock
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);
        qualityHoldWarningMock
            .Setup(service =>
                service.ConfirmBeforeSaveAsync(It.IsAny<IReadOnlyList<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(true);
        mySqlReceivingMock
            .Setup(service =>
                service.SaveReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(2);
        mockCatalog.Setup(service => service.GetLocations()).Returns(["W-K0-10", "W-K0-11"]);
        mockCatalog
            .Setup(service => service.GetCatalog())
            .Returns(
                new Model_InforVisualMockDataCatalog
                {
                    Locations = new List<string> { "W-K0-10", "W-K0-11" },
                    PurchaseOrders =
                    [
                        new Model_InforVisualPO
                        {
                            PONumber = "PO-054562B",
                            Parts =
                            [
                                new Model_InforVisualPart
                                {
                                    PartID = "FHS-0420-20",
                                    POLineNumber = "1",
                                    DefaultLocationId = "W-K0-09",
                                },
                            ],
                        },
                    ],
                }
            );
        mockCatalog
            .Setup(service =>
                service.AppendReceivingTransactionsAsync(
                    It.IsAny<IEnumerable<Model_InforVisualMockReceivingTransaction>>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(2, 2));
        sessionManagerMock.Setup(service => service.SessionExists()).Returns(false);

        var service = new Service_ReceivingWorkflow(
            sessionManagerMock.Object,
            receivingLabelDataMock.Object,
            mySqlReceivingMock.Object,
            validationMock.Object,
            qualityHoldWarningMock.Object,
            receivingSettingsMock.Object,
            appSettingsMock.Object,
            mockCatalog.Object,
            new Mock<IService_LoggingUtility>().Object,
            viewModelRegistryMock.Object,
            userSessionManagerMock.Object
        );

        await service.StartWorkflowAsync();
        service.CurrentSession.Loads.AddRange([
            new Model_ReceivingLoad
            {
                LoadID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PoNumber = "54562B",
                PartID = "FHS-0420-20",
                PoLineNumber = "1",
                InitialLocation = "W-K0-07",
                WeightQuantity = 120,
                UnitOfMeasure = "EA",
                LoadNumber = 1,
                ReceivedDate = new DateTime(2026, 4, 5, 9, 30, 0),
            },
            new Model_ReceivingLoad
            {
                LoadID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PoNumber = "54562B",
                PartID = "FHS-0420-20",
                PoLineNumber = "1",
                InitialLocation = "W-K0-07",
                WeightQuantity = 95,
                UnitOfMeasure = "EA",
                LoadNumber = 2,
                ReceivedDate = new DateTime(2026, 4, 5, 9, 30, 0),
            },
        ]);

        var result = await service.SaveToDatabaseOnlyAsync();

        result.Success.Should().BeTrue();
        mockCatalog.Verify(
            catalog =>
                catalog.AppendReceivingTransactionsAsync(
                    It.Is<IEnumerable<Model_InforVisualMockReceivingTransaction>>(transactions =>
                        transactions.Count() == 2
                        && transactions
                            .Select(transaction => transaction.Quantity)
                            .OrderBy(quantity => quantity)
                            .SequenceEqual(new[] { 95m, 120m })
                        && transactions
                            .Select(transaction => transaction.CurrentLocationId)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count() == 2
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AdvanceToNextStepAsync_ShouldMarkGeneratedLoadsAsQualityHold_WhenCurrentPartRequiresIt()
    {
        var sessionManagerMock = new Mock<IService_SessionManager>();
        var receivingLabelDataMock = new Mock<IService_ReceivingLabelData>();
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var validationMock = new Mock<IService_ReceivingValidation>();
        var qualityHoldWarningMock = new Mock<IService_QualityHoldWarning>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var appSettingsMock = new Mock<IService_AppSettings>();
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        var viewModelRegistryMock = new Mock<IService_ViewModelRegistry>();
        var userSessionManagerMock = new Mock<IService_UserSessionManager>();

        validationMock
            .Setup(service =>
                service.ValidateLocationAsync(It.IsAny<string?>(), It.IsAny<string>())
            )
            .ReturnsAsync(Model_ReceivingValidationResult.Success());
        receivingSettingsMock
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        appSettingsMock.Setup(service => service.GetUseInforVisualMockData()).Returns(true);
        qualityHoldWarningMock
            .Setup(service =>
                service.ConfirmBeforeSaveAsync(It.IsAny<IReadOnlyList<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(true);
        sessionManagerMock.Setup(service => service.SessionExists()).Returns(false);

        var service = new Service_ReceivingWorkflow(
            sessionManagerMock.Object,
            receivingLabelDataMock.Object,
            mySqlReceivingMock.Object,
            validationMock.Object,
            qualityHoldWarningMock.Object,
            receivingSettingsMock.Object,
            appSettingsMock.Object,
            mockCatalog.Object,
            new Mock<IService_LoggingUtility>().Object,
            viewModelRegistryMock.Object,
            userSessionManagerMock.Object
        );

        await service.StartWorkflowAsync();
        service.CurrentPONumber = "PO-033349";
        service.CurrentPart = new Model_InforVisualPart
        {
            PartID = "MMFQH0001",
            POLineNumber = "2",
            Description = "Mock quality hold sheet for receiving workflow validation",
            UnitOfMeasure = "EA",
            QtyOrdered = 240,
            RemainingQuantity = 48,
            DefaultLocationId = "QUALITY",
            RequiresQualityHold = true,
            QualityHoldRestrictionType = "Sheet Material - Quality Hold Required",
        };
        service.NumberOfLoads = 2;
        service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry);

        var result = await service.AdvanceToNextStepAsync();

        result.Success.Should().BeTrue();
        service.CurrentSession.Loads.Should().HaveCount(2);
        service
            .CurrentSession.Loads.Should()
            .OnlyContain(load =>
                load.IsQualityHoldRequired
                && !load.IsQualityHoldAcknowledged
                && load.QualityHoldRestrictionType == "Sheet Material - Quality Hold Required"
                && load.PartID == "MMFQH0001"
            );
    }

    [Fact]
    public async Task SaveSessionAsync_ShouldRequestFinalQualityHoldConfirmation_BeforeSavingLabelQueue()
    {
        var sessionManagerMock = new Mock<IService_SessionManager>();
        var receivingLabelDataMock = new Mock<IService_ReceivingLabelData>();
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var validationMock = new Mock<IService_ReceivingValidation>();
        var qualityHoldWarningMock = new Mock<IService_QualityHoldWarning>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var appSettingsMock = new Mock<IService_AppSettings>();
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        var viewModelRegistryMock = new Mock<IService_ViewModelRegistry>();
        var userSessionManagerMock = new Mock<IService_UserSessionManager>();

        validationMock
            .Setup(service => service.ValidateSession(It.IsAny<List<Model_ReceivingLoad>>()))
            .Returns(Model_ReceivingValidationResult.Success());
        qualityHoldWarningMock
            .Setup(service =>
                service.ConfirmBeforeSaveAsync(It.IsAny<IReadOnlyList<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(true);
        mySqlReceivingMock
            .Setup(service =>
                service.SaveReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(1);
        sessionManagerMock.Setup(service => service.ClearSessionAsync()).ReturnsAsync(true);

        var service = new Service_ReceivingWorkflow(
            sessionManagerMock.Object,
            receivingLabelDataMock.Object,
            mySqlReceivingMock.Object,
            validationMock.Object,
            qualityHoldWarningMock.Object,
            receivingSettingsMock.Object,
            appSettingsMock.Object,
            mockCatalog.Object,
            new Mock<IService_LoggingUtility>().Object,
            viewModelRegistryMock.Object,
            userSessionManagerMock.Object
        );

        service.CurrentSession.Loads.Add(
            new Model_ReceivingLoad
            {
                PartID = "MMFQH0001",
                PoNumber = "PO-033349",
                PoLineNumber = "2",
                LoadNumber = 1,
                WeightQuantity = 120,
                IsQualityHoldRequired = true,
                IsQualityHoldAcknowledged = false,
                QualityHoldRestrictionType = "Sheet Material - Quality Hold Required",
            }
        );

        var result = await service.SaveSessionAsync();

        result.Success.Should().BeTrue();
        service.CurrentSession.Loads.Should().BeEmpty();
        qualityHoldWarningMock.Verify(
            warning =>
                warning.ConfirmBeforeSaveAsync(
                    It.Is<IReadOnlyList<Model_ReceivingLoad>>(loads =>
                        loads.Count == 1
                        && loads[0].PartID == "MMFQH0001"
                        && loads[0].IsQualityHoldRequired
                    )
                ),
            Times.Once
        );
        mySqlReceivingMock.Verify(
            service =>
                service.SaveReceivingLoadsAsync(
                    It.Is<List<Model_ReceivingLoad>>(loads =>
                        loads.Count == 1 && loads[0].IsQualityHoldAcknowledged
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task SaveSessionAsync_ShouldCancelBeforeQueueSave_WhenFinalQualityHoldConfirmationIsDeclined()
    {
        var sessionManagerMock = new Mock<IService_SessionManager>();
        var receivingLabelDataMock = new Mock<IService_ReceivingLabelData>();
        var mySqlReceivingMock = new Mock<IService_MySQL_Receiving>();
        var validationMock = new Mock<IService_ReceivingValidation>();
        var qualityHoldWarningMock = new Mock<IService_QualityHoldWarning>();
        var receivingSettingsMock = new Mock<IService_ReceivingSettings>();
        var appSettingsMock = new Mock<IService_AppSettings>();
        var mockCatalog = new Mock<IService_InforVisualMockDataCatalog>();
        var viewModelRegistryMock = new Mock<IService_ViewModelRegistry>();
        var userSessionManagerMock = new Mock<IService_UserSessionManager>();

        validationMock
            .Setup(service => service.ValidateSession(It.IsAny<List<Model_ReceivingLoad>>()))
            .Returns(Model_ReceivingValidationResult.Success());
        qualityHoldWarningMock
            .Setup(service =>
                service.ConfirmBeforeSaveAsync(It.IsAny<IReadOnlyList<Model_ReceivingLoad>>())
            )
            .ReturnsAsync(false);

        var service = new Service_ReceivingWorkflow(
            sessionManagerMock.Object,
            receivingLabelDataMock.Object,
            mySqlReceivingMock.Object,
            validationMock.Object,
            qualityHoldWarningMock.Object,
            receivingSettingsMock.Object,
            appSettingsMock.Object,
            mockCatalog.Object,
            new Mock<IService_LoggingUtility>().Object,
            viewModelRegistryMock.Object,
            userSessionManagerMock.Object
        );

        service.CurrentSession.Loads.Add(
            new Model_ReceivingLoad
            {
                PartID = "MMFQH0001",
                PoNumber = "PO-033349",
                PoLineNumber = "2",
                LoadNumber = 1,
                WeightQuantity = 120,
                IsQualityHoldRequired = true,
                IsQualityHoldAcknowledged = false,
                QualityHoldRestrictionType = "Sheet Material - Quality Hold Required",
            }
        );

        var result = await service.SaveSessionAsync();

        result.Success.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(error =>
                error.Contains(
                    "Quality hold acknowledgment required before saving",
                    StringComparison.Ordinal
                )
            );
        service.CurrentSession.Loads.Should().HaveCount(1);
        service.CurrentSession.Loads[0].IsQualityHoldAcknowledged.Should().BeFalse();
        mySqlReceivingMock.Verify(
            receiving => receiving.SaveReceivingLoadsAsync(It.IsAny<List<Model_ReceivingLoad>>()),
            Times.Never
        );
        sessionManagerMock.Verify(manager => manager.ClearSessionAsync(), Times.Never);
    }
}
