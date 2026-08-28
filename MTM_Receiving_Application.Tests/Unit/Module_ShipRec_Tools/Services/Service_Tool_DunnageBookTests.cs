using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

public sealed class Service_Tool_DunnageBookTests
{
    private static Model_DunnageType CreateType(int id, string name) =>
        new() { Id = id, TypeName = name };

    private static Model_DunnagePart CreatePart(
        int id,
        string partId,
        int typeId,
        string typeName,
        string homeLocation = "RECV"
    ) =>
        new()
        {
            Id = id,
            PartId = partId,
            TypeId = typeId,
            DunnageTypeName = typeName,
            HomeLocation = homeLocation,
            QuantityType = "Pieces",
        };

    private static IService_MySQL_Dunnage CreateDunnageServiceMock(
        List<Model_DunnageType> types,
        List<Model_DunnagePart> parts
    )
    {
        var mock = new Mock<IService_MySQL_Dunnage>();
        mock.Setup(service => service.GetAllTypesAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(types));
        mock.Setup(service => service.GetAllPartsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(parts));
        mock.Setup(service => service.GetCustomFieldsByTypeAsync(It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_CustomFieldDefinition>()));
        return mock.Object;
    }

    private static Service_Tool_DunnageBook CreateService(IService_MySQL_Dunnage dunnageService) =>
        new(dunnageService, new Mock<IService_LoggingUtility>().Object);

    [Fact]
    public async Task LoadDunnageGroupsAsync_ShouldGroupPartsByTypeAndResolveCustomFieldValues()
    {
        var customFields = new List<Model_CustomFieldDefinition>
        {
            new() { DisplayOrder = 1, FieldName = "Length" },
        };

        var dunnageMock = new Mock<IService_MySQL_Dunnage>();
        dunnageMock
            .Setup(service => service.GetAllTypesAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnageType> { CreateType(1, "Pallets") }
                )
            );
        dunnageMock
            .Setup(service => service.GetAllPartsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_DunnagePart>
                    {
                        CreatePart(1, "PLT-001", 1, "Pallets"),
                        CreatePart(2, "PLT-002", 1, "Pallets"),
                    }
                )
            );
        dunnageMock
            .Setup(service => service.GetCustomFieldsByTypeAsync(1))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(customFields));

        var parts = dunnageMock.Object;
        var service = CreateService(parts);

        var result = await service.LoadDunnageGroupsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        var group = result.Data![0];
        group.TypeName.Should().Be("Pallets");
        group.Entries.Should().HaveCount(2);
        group.Entries[0].PartId.Should().Be("PLT-001");
    }

    [Fact]
    public async Task BuildBookAsync_ShouldReturnFailure_WhenNoEntriesAreSelected()
    {
        var dunnageService = CreateDunnageServiceMock(
            [CreateType(1, "Pallets")],
            [CreatePart(1, "PLT-001", 1, "Pallets")]
        );
        var service = CreateService(dunnageService);
        var groupsResult = await service.LoadDunnageGroupsAsync();

        var result = await service.BuildBookAsync(groupsResult.Data!, new Model_Tool_DunnageBook_Config());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task BuildBookAsync_ShouldIncludeCoverAndTableOfContentsWithPageNumbers()
    {
        var dunnageService = CreateDunnageServiceMock(
            [CreateType(1, "Pallets")],
            [CreatePart(1, "PLT-001", 1, "Pallets")]
        );
        var service = CreateService(dunnageService);
        var groupsResult = await service.LoadDunnageGroupsAsync();
        groupsResult.Data![0].Entries[0].IsSelected = true;

        var result = await service.BuildBookAsync(
            groupsResult.Data!,
            new Model_Tool_DunnageBook_Config
            {
                CoverTitle = "My Book",
                IncludeCoverPage = true,
                IncludeTableOfContents = true,
            }
        );

        result.IsSuccess.Should().BeTrue();
        result.Data!.HtmlFragment.Should().Contain("cover-title");
        result.Data.HtmlFragment.Should().Contain("Table of Contents");
        result.Data.HtmlFragment.Should().Contain("Pallets (1)");
        result.Data.HtmlFragment.Should().Contain("Page 3 of 3");
    }

    [Fact]
    public async Task BuildBookAsync_ShouldExcludeUnselectedEntries()
    {
        var dunnageService = CreateDunnageServiceMock(
            [CreateType(1, "Pallets")],
            [
                CreatePart(1, "PLT-001", 1, "Pallets"),
                CreatePart(2, "PLT-002", 1, "Pallets"),
            ]
        );
        var service = CreateService(dunnageService);
        var groupsResult = await service.LoadDunnageGroupsAsync();
        groupsResult.Data![0].Entries[0].IsSelected = true;
        groupsResult.Data[0].Entries[1].IsSelected = false;

        var result = await service.BuildBookAsync(
            groupsResult.Data!,
            new Model_Tool_DunnageBook_Config
            {
                IncludeCoverPage = false,
                IncludeTableOfContents = false,
            }
        );

        result.IsSuccess.Should().BeTrue();
        result.Data!.HtmlFragment.Should().Contain("PLT-001");
        result.Data.HtmlFragment.Should().NotContain("PLT-002");
    }

    [Fact]
    public async Task BuildBookAsync_ShouldEmbedFallbackImage_WhenPartHasNoImage()
    {
        var dunnageService = CreateDunnageServiceMock(
            [CreateType(1, "Pallets")],
            [CreatePart(1, "PLT-001", 1, "Pallets")]
        );
        var service = CreateService(dunnageService);
        var groupsResult = await service.LoadDunnageGroupsAsync();
        groupsResult.Data![0].Entries[0].IsSelected = true;

        var result = await service.BuildBookAsync(
            groupsResult.Data!,
            new Model_Tool_DunnageBook_Config
            {
                IncludeCoverPage = false,
                IncludeTableOfContents = false,
            }
        );

        result.IsSuccess.Should().BeTrue();
        result.Data!.HtmlFragment.Should().Contain("data:image/");
        result.Data.HtmlFragment.Should().Contain("class='card-image'");
    }
}
