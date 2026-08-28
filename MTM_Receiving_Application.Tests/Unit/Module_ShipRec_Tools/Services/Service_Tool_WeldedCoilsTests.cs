using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

public sealed class Service_Tool_WeldedCoilsTests
{
    private static Mock<IService_InforVisual> CreateInforVisualMock(bool partExists = true)
    {
        var mock = new Mock<IService_InforVisual>();
        mock.Setup(service => service.PartExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(partExists));
        return mock;
    }

    private static Service_Tool_WeldedCoils CreateService(
        Mock<Dao_Tool_WeldedCoil> daoMock,
        Mock<IService_InforVisual>? inforVisualMock = null
    ) =>
        new(
            daoMock.Object,
            (inforVisualMock ?? CreateInforVisualMock()).Object,
            new Mock<IService_LoggingUtility>().Object
        );

    [Fact]
    public async Task GetAllAsync_ShouldReturnDaoResult()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var expected = new List<Model_Tool_WeldedCoil>
        {
            new() { Id = 1, PartId = "21-28841", IsActive = true },
        };
        daoMock
            .Setup(dao => dao.GetAllAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expected));
        var service = CreateService(daoMock);

        var result = await service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().ContainSingle().Which.PartId.Should().Be("21-28841");
    }

    [Fact]
    public async Task InsertAsync_ShouldTrimAndUppercasePartId()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        daoMock
            .Setup(dao => dao.InsertAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        var service = CreateService(daoMock);

        var result = await service.InsertAsync("  abc ");

        result.IsSuccess.Should().BeTrue();
        daoMock.Verify(dao => dao.InsertAsync("ABC"), Times.Once);
    }

    [Fact]
    public async Task InsertAsync_ShouldReturnFailure_WhenEmpty()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var service = CreateService(daoMock);

        var result = await service.InsertAsync("   ");

        result.IsSuccess.Should().BeFalse();
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldTrimAndUppercasePartId()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        daoMock
            .Setup(dao => dao.UpdateAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var service = CreateService(daoMock);

        var result = await service.UpdateAsync(5, "  xyz ");

        result.IsSuccess.Should().BeTrue();
        daoMock.Verify(dao => dao.UpdateAsync(5, "XYZ"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenEmpty()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var service = CreateService(daoMock);

        var result = await service.UpdateAsync(5, " ");

        result.IsSuccess.Should().BeFalse();
        daoMock.Verify(dao => dao.UpdateAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SetActiveAsync_ShouldDelegateToDao()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        daoMock
            .Setup(dao => dao.SetActiveAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var service = CreateService(daoMock);

        var result = await service.SetActiveAsync(5, false);

        result.IsSuccess.Should().BeTrue();
        daoMock.Verify(dao => dao.SetActiveAsync(5, false), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelegateToDao()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        daoMock
            .Setup(dao => dao.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var service = CreateService(daoMock);

        var result = await service.DeleteAsync(5);

        result.IsSuccess.Should().BeTrue();
        daoMock.Verify(dao => dao.DeleteAsync(5), Times.Once);
    }

    [Fact]
    public async Task InsertAsync_ShouldReturnFailure_WhenPartNotInPartMaster()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var inforVisualMock = CreateInforVisualMock(partExists: false);
        var service = CreateService(daoMock, inforVisualMock);

        var result = await service.InsertAsync("MMC0000262");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InsertAsync_ShouldReturnFailure_WhenPartMasterCheckFails()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var inforVisualMock = new Mock<IService_InforVisual>();
        inforVisualMock
            .Setup(service => service.PartExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<bool>("erp down"));
        var service = CreateService(daoMock, inforVisualMock);

        var result = await service.InsertAsync("MMC0000262");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Could not verify");
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task InsertAsync_ShouldReturnFailure_WhenTooLong()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var service = CreateService(daoMock);

        var result = await service.InsertAsync("123456789012");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("11 characters");
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFailure_WhenPartNotInPartMaster()
    {
        var daoMock = new Mock<Dao_Tool_WeldedCoil>("Server=x;Database=y;");
        var inforVisualMock = CreateInforVisualMock(partExists: false);
        var service = CreateService(daoMock, inforVisualMock);

        var result = await service.UpdateAsync(5, "MMC0000262");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
        daoMock.Verify(dao => dao.UpdateAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }
}
