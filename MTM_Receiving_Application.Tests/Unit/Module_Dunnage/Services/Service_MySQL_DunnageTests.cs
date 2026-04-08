using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_MySQL_DunnageTests
{
    [Fact]
    public async Task DeleteTypeAsync_ShouldPassCurrentUserToDaoDelete()
    {
        var daoType = new Mock<Dao_DunnageType>("Server=localhost;Database=test;");
        daoType
            .Setup(dao => dao.GetByIdAsync(5))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_DunnageType
                    {
                        Id = 5,
                        TypeName = "Bins",
                        ImagePath = "Images/Dunnage/type.png",
                    }
                )
            );
        daoType
            .Setup(dao => dao.DeleteAsync(5, "System"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var daoPart = new Mock<Dao_DunnagePart>("Server=localhost;Database=test;");
        daoPart
            .Setup(dao => dao.GetByTypeAsync(5))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_DunnagePart>()));

        var daoSpec = new Mock<Dao_DunnageSpec>("Server=localhost;Database=test;");
        daoSpec
            .Setup(dao => dao.DeleteByTypeAsync(5))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        var imageStorage = new Mock<IService_DunnageImageStorage>();
        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            daoSpec: daoSpec.Object,
            imageStorage: imageStorage.Object
        );

        var result = await service.DeleteTypeAsync(5);

        result.IsSuccess.Should().BeTrue();
        daoType.Verify(dao => dao.DeleteAsync(5, "System"), Times.Once);
        daoSpec.Verify(dao => dao.DeleteByTypeAsync(5), Times.Once);
        imageStorage.Verify(
            storage => storage.DeleteImageAsync("Images/Dunnage/type.png"),
            Times.Once
        );
    }

    [Fact]
    public async Task DeletePartAsync_ShouldReturnFriendlyMessage_WhenHistoryReferencesExist()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=localhost;Database=test;");
        daoPart
            .Setup(dao => dao.GetByIdAsync("PART-100"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_DunnagePart { Id = 7, PartId = "PART-100" }
                )
            );
        daoPart
            .Setup(dao => dao.CountTransactionsAsync("PART-100"))
            .Returns(Task.FromResult(Model_Dao_Result_Factory.Success<int>(2)));

        var service = CreateService(daoPart.Object);

        var result = await service.DeletePartAsync("PART-100");

        result.IsSuccess.Should().BeFalse();
        result
            .ErrorMessage.Should()
            .Be(
                "Part 'PART-100' can't be deleted because it is referenced by 2 Dunnage history records. Remove or reassign those history records first."
            );
        daoPart.Verify(dao => dao.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeletePartAsync_ShouldDeletePart_WhenNoHistoryReferencesExist()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=localhost;Database=test;");
        daoPart
            .Setup(dao => dao.GetByIdAsync("PART-200"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_DunnagePart
                    {
                        Id = 8,
                        PartId = "PART-200",
                        ImagePath = "Images/Dunnage/test.png",
                    }
                )
            );
        daoPart
            .Setup(dao => dao.CountTransactionsAsync("PART-200"))
            .Returns(Task.FromResult(Model_Dao_Result_Factory.Success<int>(0)));
        daoPart.Setup(dao => dao.DeleteAsync(8)).ReturnsAsync(Model_Dao_Result_Factory.Success());

        var imageStorage = new Mock<IService_DunnageImageStorage>();
        var service = CreateService(daoPart.Object, imageStorage: imageStorage.Object);

        var result = await service.DeletePartAsync("PART-200");

        result.IsSuccess.Should().BeTrue();
        daoPart.Verify(dao => dao.DeleteAsync(8), Times.Once);
        imageStorage.Verify(
            storage => storage.DeleteImageAsync("Images/Dunnage/test.png"),
            Times.Once
        );
    }

    private static Service_MySQL_Dunnage CreateService(
        Dao_DunnagePart daoPart,
        Dao_DunnageType? daoType = null,
        Dao_DunnageSpec? daoSpec = null,
        IService_DunnageImageStorage? imageStorage = null
    )
    {
        return new Service_MySQL_Dunnage(
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<Dao_DunnageLoad>("Server=localhost;Database=test;").Object,
            new Mock<Dao_DunnageLabelData>("Server=localhost;Database=test;").Object,
            daoType ?? new Mock<Dao_DunnageType>("Server=localhost;Database=test;").Object,
            daoPart,
            daoSpec ?? new Mock<Dao_DunnageSpec>("Server=localhost;Database=test;").Object,
            new Mock<Dao_InventoriedDunnage>("Server=localhost;Database=test;").Object,
            new Mock<Dao_DunnageCustomField>("Server=localhost;Database=test;").Object,
            new Mock<Dao_DunnageUserPreference>("Server=localhost;Database=test;").Object,
            new Mock<Dao_DunnageNonPOEntry>("Server=localhost;Database=test;").Object,
            imageStorage ?? new Mock<IService_DunnageImageStorage>().Object
        );
    }
}
