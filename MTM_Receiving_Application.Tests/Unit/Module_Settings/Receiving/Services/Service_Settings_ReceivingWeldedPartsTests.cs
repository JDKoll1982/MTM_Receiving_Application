using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Receiving.Data;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Receiving.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Receiving.Services;

public sealed class Service_Settings_ReceivingWeldedPartsTests
{
    private static Mock<IService_InforVisual> CreateInforVisualMock(bool partExists = true)
    {
        var mock = new Mock<IService_InforVisual>();
        mock.Setup(service => service.PartExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(partExists));
        return mock;
    }

    private static Service_Settings_ReceivingWeldedParts CreateService(
        Mock<Dao_Settings_ReceivingWeldedParts> daoMock,
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
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        var expected = new List<Model_Settings_ReceivingWeldedPart>
        {
            new() { Id = 1, PartId = "21-28841" },
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
    public async Task AddAsync_ShouldTrimAndUppercasePartId()
    {
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        daoMock
            .Setup(dao => dao.InsertAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<int>(1));
        var service = CreateService(daoMock);

        var result = await service.AddAsync("  abc ");

        result.IsSuccess.Should().BeTrue();
        daoMock.Verify(dao => dao.InsertAsync("ABC"), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFailure_WhenEmpty()
    {
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        var service = CreateService(daoMock);

        var result = await service.AddAsync("   ");

        result.IsSuccess.Should().BeFalse();
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFailure_WhenTooLong()
    {
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        var service = CreateService(daoMock);

        var result = await service.AddAsync("123456789012");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("11 characters");
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFailure_WhenPartNotInPartMaster()
    {
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        var service = CreateService(daoMock, CreateInforVisualMock(partExists: false));

        var result = await service.AddAsync("MMC0000262");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnFailure_WhenPartMasterCheckFails()
    {
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        var inforVisualMock = new Mock<IService_InforVisual>();
        inforVisualMock
            .Setup(service => service.PartExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<bool>("erp down"));
        var service = CreateService(daoMock, inforVisualMock);

        var result = await service.AddAsync("MMC0000262");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Could not verify");
        daoMock.Verify(dao => dao.InsertAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDelegateToDao()
    {
        var daoMock = new Mock<Dao_Settings_ReceivingWeldedParts>("Server=x;Database=y;");
        daoMock
            .Setup(dao => dao.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());
        var service = CreateService(daoMock);

        var result = await service.DeleteAsync(5);

        result.IsSuccess.Should().BeTrue();
        daoMock.Verify(dao => dao.DeleteAsync(5), Times.Once);
    }
}
