using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_WeldedCoilsTests
{
    private static Model_Tool_WeldedCoil CreateCoil(int id, string partId, bool isActive = true) =>
        new() { Id = id, PartId = partId, IsActive = isActive };

    private static ViewModel_Tool_WeldedCoils CreateViewModel(IService_Tool_WeldedCoils service) =>
        new(
            service,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Mock<IService_Tool_WeldedCoils> CreateServiceMockWithCoils(
        params Model_Tool_WeldedCoil[] coils
    )
    {
        var serviceMock = new Mock<IService_Tool_WeldedCoils>();
        serviceMock
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(coils.ToList()));
        return serviceMock;
    }

    [Fact]
    public async Task LoadCoilsAsync_ShouldPopulateCoilsAndCounts()
    {
        var serviceMock = CreateServiceMockWithCoils(
            CreateCoil(1, "A-001", isActive: true),
            CreateCoil(2, "B-002", isActive: true),
            CreateCoil(3, "C-003", isActive: false)
        );
        var viewModel = CreateViewModel(serviceMock.Object);

        await viewModel.LoadCoilsAsync();

        viewModel.Coils.Should().HaveCount(3);
        viewModel.ActiveCount.Should().Be(2);
        viewModel.InactiveCount.Should().Be(1);
        viewModel.ActiveCountText.Should().Be("2 active");
        viewModel.InactiveCountText.Should().Be("1 inactive");
    }

    [Fact]
    public async Task LoadCoilsAsync_ShouldShowError_WhenServiceFails()
    {
        var serviceMock = new Mock<IService_Tool_WeldedCoils>();
        serviceMock
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<List<Model_Tool_WeldedCoil>>("boom"));
        var viewModel = CreateViewModel(serviceMock.Object);

        await viewModel.LoadCoilsAsync();

        viewModel.Coils.Should().BeEmpty();
        viewModel.StatusMessage.Should().Contain("boom");
    }

    [Fact]
    public async Task AddCoilAsync_ShouldCallInsertAndReload_WhenValid()
    {
        var serviceMock = CreateServiceMockWithCoils();
        serviceMock
            .Setup(service => service.InsertAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        var viewModel = CreateViewModel(serviceMock.Object);
        viewModel.NewPartId = " 21-28841 ";

        await viewModel.AddCoilCommand.ExecuteAsync(null);

        serviceMock.Verify(service => service.InsertAsync("21-28841"), Times.Once);
        serviceMock.Verify(service => service.GetAllAsync(), Times.AtLeastOnce);
        viewModel.NewPartId.Should().BeEmpty();
    }

    [Fact]
    public async Task AddCoilAsync_ShouldNotCallInsert_WhenDuplicate()
    {
        var serviceMock = CreateServiceMockWithCoils(CreateCoil(1, "21-28841"));
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();
        viewModel.NewPartId = "21-28841";

        await viewModel.AddCoilCommand.ExecuteAsync(null);

        serviceMock.Verify(service => service.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddCoilAsync_ShouldNotCallInsert_WhenEmpty()
    {
        var serviceMock = CreateServiceMockWithCoils();
        var viewModel = CreateViewModel(serviceMock.Object);
        viewModel.NewPartId = "   ";

        await viewModel.AddCoilCommand.ExecuteAsync(null);

        serviceMock.Verify(service => service.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ToggleActiveAsync_ShouldCallSetActiveAndRecount()
    {
        var coil = CreateCoil(1, "21-28841", isActive: true);
        var serviceMock = CreateServiceMockWithCoils(coil);
        serviceMock
            .Setup(service => service.SetActiveAsync(1, false))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();

        coil.IsActive = false;
        await viewModel.ToggleActiveAsync(coil);

        serviceMock.Verify(service => service.SetActiveAsync(1, false), Times.Once);
        viewModel.ActiveCount.Should().Be(0);
        viewModel.InactiveCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteSelectedAsync_ShouldDeleteSelectedCoilAndClearSelection()
    {
        var coilA = CreateCoil(1, "21-28841", isActive: true);
        var coilB = CreateCoil(2, "21-28842", isActive: false);
        var serviceMock = CreateServiceMockWithCoils(coilA, coilB);
        serviceMock
            .Setup(service => service.DeleteAsync(1))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();
        viewModel.SelectedCoil = coilA;

        await viewModel.DeleteSelectedCommand.ExecuteAsync(null);

        serviceMock.Verify(service => service.DeleteAsync(1), Times.Once);
        viewModel.Coils.Should().ContainSingle(c => c.Id == 2);
        viewModel.SelectedCoil.Should().BeNull();
        viewModel.ActiveCount.Should().Be(0);
        viewModel.InactiveCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteSelectedCommand_ShouldNotDelete_WhenNoSelection()
    {
        var serviceMock = CreateServiceMockWithCoils(CreateCoil(1, "21-28841"));
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();

        await viewModel.DeleteSelectedCommand.ExecuteAsync(null);

        serviceMock.Verify(service => service.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SearchText_ShouldFilterFilteredCoils()
    {
        var serviceMock = CreateServiceMockWithCoils(
            CreateCoil(1, "MMC0000262"),
            CreateCoil(2, "MMC0000583"),
            CreateCoil(3, "MMC0000770")
        );
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();

        viewModel.SearchText = "583";

        viewModel.FilteredCoils.Should().ContainSingle().Which.PartId.Should().Be("MMC0000583");
        viewModel.IsFilteredEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task SearchText_ShouldShowAllCoils_WhenCleared()
    {
        var serviceMock = CreateServiceMockWithCoils(
            CreateCoil(1, "MMC0000262"),
            CreateCoil(2, "MMC0000583")
        );
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();
        viewModel.SearchText = "262";
        viewModel.FilteredCoils.Should().ContainSingle();

        viewModel.SearchText = string.Empty;

        viewModel.FilteredCoils.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddCoilAsync_ShouldNotCallInsert_WhenBusy()
    {
        var serviceMock = CreateServiceMockWithCoils();
        serviceMock
            .Setup(service => service.InsertAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        var viewModel = CreateViewModel(serviceMock.Object);
        viewModel.NewPartId = "21-28841";
        viewModel.IsBusy = true;

        await viewModel.AddCoilCommand.ExecuteAsync(null);

        serviceMock.Verify(service => service.InsertAsync(It.IsAny<string>()), Times.Never);
    }
}
