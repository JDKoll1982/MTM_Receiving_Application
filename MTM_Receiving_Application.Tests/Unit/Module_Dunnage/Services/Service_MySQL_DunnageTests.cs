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
    public async Task DeletePartAsync_ShouldCascadeDeletePart_WhenHistoryReferencesExist()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        daoPart
            .Setup(dao => dao.GetByIdAsync("PART-100"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_DunnagePart
                    {
                        Id = 7,
                        PartId = "PART-100",
                        ImagePath = "Images/Dunnage/test.png",
                    }
                )
            );
        daoPart.Setup(dao => dao.DeleteAsync(7)).ReturnsAsync(Model_Dao_Result_Factory.Success());

        var imageStorage = new Mock<IService_DunnageImageStorage>();
        var service = CreateService(daoPart.Object, imageStorage: imageStorage.Object);

        var result = await service.DeletePartAsync("PART-100");

        result.IsSuccess.Should().BeTrue();
        daoPart.Verify(dao => dao.DeleteAsync(7), Times.Once);
        imageStorage.Verify(
            storage => storage.DeleteImageAsync("Images/Dunnage/test.png"),
            Times.Once
        );
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
    public async Task GetPartDeleteImpactAsync_ShouldReturnDaoImpact()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        var impact = new Model_DunnagePartDeleteImpact
        {
            LabelDataCount = 3,
            HistoryCount = 5,
            InventoryCount = 1,
        };
        daoPart
            .Setup(dao => dao.GetDeleteImpactAsync("PART-100"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(impact));

        var service = CreateService(daoPart.Object);

        var result = await service.GetPartDeleteImpactAsync("PART-100");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(impact);
    }

    [Fact]
    public async Task GetTypeDeleteImpactAsync_ShouldReturnDaoImpact()
    {
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        var impact = new Model_DunnageTypeDeleteImpact
        {
            PartsCount = 4,
            LabelDataCount = 2,
            HistoryCount = 7,
        };
        daoType
            .Setup(dao => dao.GetDeleteImpactAsync(9))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(impact));

        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        var service = CreateService(daoPart.Object, daoType: daoType.Object);

        var result = await service.GetTypeDeleteImpactAsync(9);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(impact);
    }

    [Fact]
    public async Task DeleteTypeAsync_ShouldCleanUpPartAndTypeImages_WhenPartsExist()
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
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnagePart>
                    {
                        new()
                        {
                            Id = 1,
                            PartId = "BIN-1",
                            ImagePath = "Images/Dunnage/part1.png",
                        },
                        new() { Id = 2, PartId = "BIN-2", ImagePath = null },
                    }
                )
            );

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
            storage => storage.DeleteImageAsync("Images/Dunnage/part1.png"),
            Times.Once
        );
        imageStorage.Verify(
            storage => storage.DeleteImageAsync("Images/Dunnage/type.png"),
            Times.Once
        );
    }

    [Fact]
    public async Task ChangePartTypeAsync_ShouldMapSharedFieldValueIntoTargetSlot()
    {
        var (daoPart, captured) = SetupChangeTypeCapture();
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        daoType
            .Setup(dao => dao.GetByIdAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_DunnageType { Id = 2, TypeName = "Bins" })
            );

        var daoCustom = new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;");
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(1))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 1, FieldName = "Length", FieldType = "Number", DisplayOrder = 1 },
                    }
                )
            );
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 2, FieldName = "Length", FieldType = "Number", DisplayOrder = 3 },
                    }
                )
            );

        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            daoCustomField: daoCustom.Object
        );
        var part = new Model_DunnagePart { PartId = "P-1", TypeId = 1 };
        part.SetUdcValue(1, "14");

        var result = await service.ChangePartTypeAsync(part, 2, new Dictionary<string, string?>());

        result.IsSuccess.Should().BeTrue();
        daoCustom.Verify(
            dao =>
                dao.InsertAsync(
                    It.IsAny<int>(),
                    It.IsAny<Model_CustomFieldDefinition>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
        captured[2].Should().Be("14");
    }

    [Fact]
    public async Task ChangePartTypeAsync_ShouldAddMissingSourceFieldsAsOptional_AndCarryValues()
    {
        var (daoPart, captured) = SetupChangeTypeCapture();
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        daoType
            .Setup(dao => dao.GetByIdAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_DunnageType { Id = 2, TypeName = "Bins" })
            );

        var daoCustom = new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;");
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(1))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 1, FieldName = "Length", FieldType = "Number", DisplayOrder = 1 },
                    }
                )
            );
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(2))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_CustomFieldDefinition>()));
        daoCustom
            .Setup(dao =>
                dao.InsertAsync(
                    It.IsAny<int>(),
                    It.IsAny<Model_CustomFieldDefinition>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(50));

        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            daoCustomField: daoCustom.Object
        );
        var part = new Model_DunnagePart { PartId = "P-1", TypeId = 1 };
        part.SetUdcValue(1, "14");

        var result = await service.ChangePartTypeAsync(part, 2, new Dictionary<string, string?>());

        result.IsSuccess.Should().BeTrue();
        daoCustom.Verify(
            dao =>
                dao.InsertAsync(
                    It.IsAny<int>(),
                    It.Is<Model_CustomFieldDefinition>(field =>
                        field.FieldName == "Length"
                        && field.DisplayOrder == 1
                        && field.IsRequired == false
                    ),
                    It.IsAny<string>()
                ),
            Times.Once
        );
        captured[0].Should().Be("14");
    }

    [Fact]
    public async Task ChangePartTypeAsync_ShouldRejectWhenRequiredTargetFieldHasNoValue()
    {
        var (daoPart, _) = SetupChangeTypeCapture();
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        daoType
            .Setup(dao => dao.GetByIdAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_DunnageType { Id = 2, TypeName = "Bins" })
            );

        var daoCustom = new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;");
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(1))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 1, FieldName = "Length", FieldType = "Number", DisplayOrder = 1 },
                    }
                )
            );
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 1, FieldName = "Length", FieldType = "Number", DisplayOrder = 1 },
                        new()
                        {
                            Id = 2,
                            FieldName = "Department",
                            FieldType = "Text",
                            DisplayOrder = 2,
                            IsRequired = true,
                        },
                    }
                )
            );

        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            daoCustomField: daoCustom.Object
        );
        var part = new Model_DunnagePart { PartId = "P-1", TypeId = 1 };
        part.SetUdcValue(1, "14");

        var result = await service.ChangePartTypeAsync(part, 2, new Dictionary<string, string?>());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Required field 'Department'");
        daoPart.Verify(dao => dao.ChangeTypeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangePartTypeAsync_ShouldUseProvidedRequiredValue()
    {
        var (daoPart, captured) = SetupChangeTypeCapture();
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        daoType
            .Setup(dao => dao.GetByIdAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_DunnageType { Id = 2, TypeName = "Bins" })
            );

        var daoCustom = new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;");
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(1))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 1, FieldName = "Length", FieldType = "Number", DisplayOrder = 1 },
                    }
                )
            );
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new()
                        {
                            Id = 2,
                            FieldName = "Department",
                            FieldType = "Text",
                            DisplayOrder = 1,
                            IsRequired = true,
                        },
                    }
                )
            );
        daoCustom
            .Setup(dao =>
                dao.InsertAsync(
                    It.IsAny<int>(),
                    It.IsAny<Model_CustomFieldDefinition>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(50));

        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            daoCustomField: daoCustom.Object
        );
        var part = new Model_DunnagePart { PartId = "P-1", TypeId = 1 };

        var result = await service.ChangePartTypeAsync(
            part,
            2,
            new Dictionary<string, string?> { { "Department", "Die Shop" } }
        );

        result.IsSuccess.Should().BeTrue();
        captured[0].Should().Be("Die Shop");
    }

    [Fact]
    public async Task ChangePartTypeAsync_ShouldRejectWhenTargetTypeHasNoOpenSlots()
    {
        var (daoPart, _) = SetupChangeTypeCapture();
        var daoType = new Mock<Dao_DunnageType>("Server=172.16.1.104;Database=test;");
        daoType
            .Setup(dao => dao.GetByIdAsync(2))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new Model_DunnageType { Id = 2, TypeName = "Full" })
            );

        var daoCustom = new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;");
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(1))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_CustomFieldDefinition>
                    {
                        new() { Id = 1, FieldName = "Length", FieldType = "Number", DisplayOrder = 1 },
                    }
                )
            );
        var fullTarget = Enumerable
            .Range(1, 10)
            .Select(index =>
                new Model_CustomFieldDefinition
                {
                    Id = index,
                    FieldName = $"Field{index}",
                    FieldType = "Text",
                    DisplayOrder = index,
                }
            )
            .ToList();
        daoCustom
            .Setup(dao => dao.GetByTypeAsync(2))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(fullTarget));

        var service = CreateService(
            daoPart.Object,
            daoType: daoType.Object,
            daoCustomField: daoCustom.Object
        );
        var part = new Model_DunnagePart { PartId = "P-1", TypeId = 1 };
        part.SetUdcValue(1, "14");

        var result = await service.ChangePartTypeAsync(part, 2, new Dictionary<string, string?>());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("open spec slots");
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
                    It.IsAny<string>(),
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
                    It.IsAny<string>(),
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
        IService_DunnageImageStorage? imageStorage = null,
        Dao_DunnageCustomField? daoCustomField = null
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
            daoCustomField ?? new Mock<Dao_DunnageCustomField>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_DunnageUserPreference>("Server=172.16.1.104;Database=test;").Object,
            new Mock<Dao_DunnageNonPOEntry>("Server=172.16.1.104;Database=test;").Object,
            imageStorage ?? new Mock<IService_DunnageImageStorage>().Object
        );
    }

    private static (Mock<Dao_DunnagePart> DaoPart, string?[] Captured)
        SetupChangeTypeCapture()
    {
        var daoPart = new Mock<Dao_DunnagePart>("Server=172.16.1.104;Database=test;");
        var captured = new string?[10];
        daoPart
            .Setup(dao =>
                dao.ChangeTypeAsync(
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
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success())
            .Callback<
                string,
                int,
                string?,
                string?,
                string?,
                string?,
                string?,
                string?,
                string?,
                string?,
                string?,
                string?,
                string
            >((_, _, u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, _) =>
            {
                captured[0] = u1;
                captured[1] = u2;
                captured[2] = u3;
                captured[3] = u4;
                captured[4] = u5;
                captured[5] = u6;
                captured[6] = u7;
                captured[7] = u8;
                captured[8] = u9;
                captured[9] = u10;
            });

        return (daoPart, captured);
    }
}
