using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_DunnageBookTests
{
    private static Model_Tool_DunnageBook_TypeGroup CreateGroup(
        string typeName,
        params string[] partIds
    )
    {
        var group = new Model_Tool_DunnageBook_TypeGroup { TypeId = 1, TypeName = typeName };
        foreach (var partId in partIds)
        {
            group.Entries.Add(
                new Model_Tool_DunnageBook_Entry(
                    new Model_DunnagePart
                    {
                        PartId = partId,
                        TypeId = 1,
                        DunnageTypeName = typeName,
                        HomeLocation = "RECV",
                        QuantityType = "Pieces",
                    }
                )
                { Group = group }
            );
        }

        return group;
    }

    private static ViewModel_Tool_DunnageBook CreateViewModel(
        IService_Tool_DunnageBook service,
        Mock<IService_Notification> notificationMock
    ) =>
        new(
            service,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            notificationMock.Object
        );

    [Fact]
    public async Task LoadAsync_ShouldPopulateFilteredGroupsAndTypeFilterOptions()
    {
        var serviceMock = new Mock<IService_Tool_DunnageBook>();
        serviceMock
            .Setup(service => service.LoadDunnageGroupsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_DunnageBook_TypeGroup> { CreateGroup("Pallets", "PLT-001") }
                )
            );

        var viewModel = CreateViewModel(serviceMock.Object, new Mock<IService_Notification>());

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.FilteredGroups.Should().HaveCount(1);
        viewModel.TotalEntryCount.Should().Be(1);
        viewModel.TypeFilterOptions.Should().Contain("All Types");
        viewModel.TypeFilterOptions.Should().Contain("Pallets");
    }

    [Fact]
    public async Task SelectAll_ShouldSelectAllVisibleEntries()
    {
        var serviceMock = new Mock<IService_Tool_DunnageBook>();
        serviceMock
            .Setup(service => service.LoadDunnageGroupsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_DunnageBook_TypeGroup>
                    {
                        CreateGroup("Pallets", "PLT-001", "PLT-002"),
                    }
                )
            );

        var viewModel = CreateViewModel(serviceMock.Object, new Mock<IService_Notification>());
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectAllCommand.Execute(null);

        viewModel.SelectedCount.Should().Be(2);
        viewModel.HasSelection.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateBookAsync_ShouldShowWarning_WhenNothingSelected()
    {
        var serviceMock = new Mock<IService_Tool_DunnageBook>();
        serviceMock
            .Setup(service => service.LoadDunnageGroupsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_DunnageBook_TypeGroup> { CreateGroup("Pallets", "PLT-001") }
                )
            );

        var viewModel = CreateViewModel(serviceMock.Object, new Mock<IService_Notification>());
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.GenerateBookCommand.ExecuteAsync(null);

        serviceMock.Verify(
            service => service.BuildBookAsync(It.IsAny<IReadOnlyList<Model_Tool_DunnageBook_TypeGroup>>(), It.IsAny<Model_Tool_DunnageBook_Config>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SearchText_ShouldFilterVisibleEntriesByPartId()
    {
        var serviceMock = new Mock<IService_Tool_DunnageBook>();
        serviceMock
            .Setup(service => service.LoadDunnageGroupsAsync())
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_DunnageBook_TypeGroup>
                    {
                        CreateGroup("Pallets", "PLT-001", "BOX-002"),
                    }
                )
            );

        var viewModel = CreateViewModel(serviceMock.Object, new Mock<IService_Notification>());
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SearchText = "PLT";

        viewModel.FilteredEntryCount.Should().Be(1);
        viewModel
            .FilteredGroups[0]
            .Entries.Count(entry => entry.IsVisible)
            .Should()
            .Be(1);
    }
}
