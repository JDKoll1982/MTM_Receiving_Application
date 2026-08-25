using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_MySQL_DunnageTests
{
    [Fact]
    public async Task DeleteTypeAsync_ShouldPassCurrentUserToDaoDelete()
    {
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
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

        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        daoPart
            .Setup(dao => dao.GetByTypeAsync(5))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_DunnagePart>()));

        var imageStorage = new Mock<IService_DunnageImageStorage>();
        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            imageStorage: imageStorage.Object
        );

        var result = await service.DeleteTypeAsync(5);

        result.IsSuccess.Should().BeTrue();
        daoType.Verify(dao => dao.DeleteAsync(5, "System"), Times.Once);
        imageStorage.Verify(
            storage => storage.DeleteImageAsync("Images/Dunnage/type.png"),
            Times.Once
        );
    }

    [Fact]
    public async Task DeletePartAsync_ShouldReturnFriendlyMessage_WhenHistoryReferencesExist()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
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
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
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

    [Fact]
    public async Task InsertPartWithInventoryAsync_ShouldInjectNormalizedImagePathIntoSpecJson()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        daoPart
            .Setup(dao => dao.GetAllAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_DunnagePart>()));
        daoPart
            .Setup(dao =>
                dao.InsertWithInventoryAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(42));

        var imageStorage = new Mock<IService_DunnageImageStorage>();
        imageStorage
            .Setup(storage =>
                storage.ImportPartImageAsync(It.IsAny<string>(), "Pallet", "PART-300")
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success("Parts/Pallet-PART-300.png"));
        imageStorage
            .Setup(storage => storage.GetNormalizedFullPath("Parts/Pallet-PART-300.png"))
            .Returns("\\\\server\\share\\Dunnage\\Parts\\Pallet-PART-300.png");

        var service = CreateService(daoPart.Object, imageStorage: imageStorage.Object);
        var part = new Model_DunnagePart
        {
            PartId = "PART-300",
            TypeId = 9,
            DunnageTypeName = "Pallet",
            ImagePath = Path.Combine(Path.GetTempPath(), "part300.png"),
            HomeLocation = "A-01",
        };
        part.SetUdcValue(1, "Blue");

        var result = await service.InsertPartWithInventoryAsync(part, "Not Inventoried");

        result.IsSuccess.Should().BeTrue();
        daoPart.Verify(
            dao =>
                dao.InsertWithInventoryAsync(
                    "PART-300",
                    9,
                    "Blue",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    "Parts/Pallet-PART-300.png",
                    "Quantity",
                    "A-01",
                    "Not Inventoried",
                    string.Empty,
                    "System"
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task InsertPartWithInventoryAsync_ShouldReturnFriendlyDuplicateFailure_WhenPartIdAlreadyExists()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        daoPart
            .Setup(dao => dao.GetAllAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnagePart>
                    {
                        new() { Id = 1, PartId = null! },
                        new() { Id = 2, PartId = "PART-300" },
                    }
                )
            );

        var service = CreateService(daoPart.Object);
        var part = new Model_DunnagePart
        {
            PartId = "PART-300",
            TypeId = 9,
            DunnageTypeName = "Pallet",
            HomeLocation = "A-01",
        };

        var result = await service.InsertPartWithInventoryAsync(part, "Not Inventoried");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("A dunnage part with Part ID 'PART-300' already exists.");
        daoPart.Verify(
            dao =>
                dao.InsertWithInventoryAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task InsertTypeAsync_ShouldUseTypeImageImport_WhenImagePathIsAbsolute()
    {
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        daoType
            .Setup(dao =>
                dao.InsertAsync(
                    "Pallet",
                    It.IsAny<string>(),
                    "Types/DunnageType-Pallet.png",
                    "System"
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(12));

        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        var imageStorage = new Mock<IService_DunnageImageStorage>();
        imageStorage
            .Setup(storage => storage.ImportTypeImageAsync(It.IsAny<string>(), "Pallet"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success("Types/DunnageType-Pallet.png"));

        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            imageStorage: imageStorage.Object
        );

        var type = new Model_DunnageType
        {
            TypeName = "Pallet",
            Icon = "PackageVariantClosed",
            ImagePath = Path.Combine(Path.GetTempPath(), "pallet.png"),
        };

        var result = await service.InsertTypeAsync(type);

        result.IsSuccess.Should().BeTrue();
        type.ImagePath.Should().Be("Types/DunnageType-Pallet.png");
        imageStorage.Verify(
            storage => storage.ImportTypeImageAsync(It.IsAny<string>(), "Pallet"),
            Times.Once
        );
        imageStorage.Verify(
            storage => storage.ImportImageAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    private static Service_MySQL_Dunnage CreateService(
        Dao_DunnagePart daoPart,
        Dao_DunnageType? daoType = null,
        IService_DunnageImageStorage? imageStorage = null
    )
    {
        return new Service_MySQL_Dunnage(
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_UserSessionManager>().Object,
            new Mock<Dao_DunnageLoad>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_DunnageLabelData>("Server=172.16.1.104;Database=test;").Object,
            daoType ?? new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;").Object,
            daoPart,
            new Mock<Dao_DunnageQuantityType>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_InventoriedDunnage>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_DunnageUserPreference>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_DunnageNonPOEntry>("Server=172.16.1.104;Database=test;").Object,
            imageStorage ?? new Mock<IService_DunnageImageStorage>().Object
        );
    }
}
