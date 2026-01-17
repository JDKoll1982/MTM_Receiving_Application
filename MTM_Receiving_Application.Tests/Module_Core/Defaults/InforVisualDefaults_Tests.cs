using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Defaults;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using Model_InforVisualPart = MTM_Receiving_Application.Module_Core.Models.InforVisual.Model_InforVisualPart;
using Model_InforVisualPO = MTM_Receiving_Application.Module_Core.Models.InforVisual.Model_InforVisualPO;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Defaults;

public class InforVisualDefaults_Tests
{
    [Fact]
    public void DefaultSiteId_ShouldNotBeNullOrWhiteSpace()
    {
        InforVisualDefaults.DefaultSiteId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void DefaultUom_ShouldNotBeNullOrWhiteSpace()
    {
        InforVisualDefaults.DefaultUom.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void DefaultPartStatus_ShouldNotBeNullOrWhiteSpace()
    {
        InforVisualDefaults.DefaultPartStatus.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Model_InforVisualConnection_DefaultsShouldMatch()
    {
        var model = new Model_InforVisualConnection();

        model.Server.Should().Be(InforVisualDefaults.DefaultServer);
        model.Database.Should().Be(InforVisualDefaults.DefaultDatabase);
        model.SiteId.Should().Be(InforVisualDefaults.DefaultSiteId);
    }

    [Fact]
    public void Model_InforVisualPart_DefaultsShouldMatch()
    {
        var model = new Model_InforVisualPart();

        model.PrimaryUom.Should().Be(InforVisualDefaults.DefaultUom);
        model.DefaultSite.Should().Be(InforVisualDefaults.DefaultSiteId);
        model.PartStatus.Should().Be(InforVisualDefaults.DefaultPartStatus);
    }

    [Fact]
    public void Model_InforVisualPO_DefaultsShouldMatch()
    {
        var model = new Model_InforVisualPO();

        model.UnitOfMeasure.Should().Be(InforVisualDefaults.DefaultUom);
        model.SiteId.Should().Be(InforVisualDefaults.DefaultSiteId);
    }
}
