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
    public async Task LoadCoilsAsync_ShouldPopulateCoilsAndTotalCount()
    {
        var serviceMock = CreateServiceMockWithCoils(
            CreateCoil(1, "A-001"),
            CreateCoil(2, "B-002"),
            CreateCoil(3, "C-003")
        );
        var viewModel = CreateViewModel(serviceMock.Object);

        await viewModel.LoadCoilsAsync();

        viewModel.Coils.Should().HaveCount(3);
        viewModel.TotalCount.Should().Be(3);
        viewModel.TotalCountText.Should().Be("3 coil(s)");
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
    public async Task DeleteCoilAsync_ShouldDeleteRowAndUpdateCount()
    {
        var coilA = CreateCoil(1, "21-28841");
        var coilB = CreateCoil(2, "21-28842");
        var serviceMock = CreateServiceMockWithCoils(coilA, coilB);
        serviceMock
            .Setup(service => service.DeleteAsync(1))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();

        await viewModel.DeleteCoilAsync(coilA);

        serviceMock.Verify(service => service.DeleteAsync(1), Times.Once);
        viewModel.Coils.Should().ContainSingle(c => c.Id == 2);
        viewModel.TotalCount.Should().Be(1);
        viewModel.StatusMessage.Should().Contain("Deleted");
    }

    [Fact]
    public async Task DeleteCoilAsync_ShouldKeepRow_WhenServiceFails()
    {
        var coil = CreateCoil(1, "21-28841");
        var serviceMock = CreateServiceMockWithCoils(coil);
        serviceMock
            .Setup(service => service.DeleteAsync(1))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure("delete failed"));
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();

        await viewModel.DeleteCoilAsync(coil);

        viewModel.Coils.Should().ContainSingle();
        viewModel.TotalCount.Should().Be(1);
        viewModel.StatusMessage.Should().Contain("delete failed");
    }

    [Fact]
    public async Task DeleteCoilAsync_ShouldNotDelete_WhenBusy()
    {
        var coil = CreateCoil(1, "21-28841");
        var serviceMock = CreateServiceMockWithCoils(coil);
        var viewModel = CreateViewModel(serviceMock.Object);
        await viewModel.LoadCoilsAsync();
        viewModel.IsBusy = true;

        await viewModel.DeleteCoilAsync(coil);

        serviceMock.Verify(service => service.DeleteAsync(It.IsAny<int>()), Times.Never);
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
