using FluentAssertions;
using MTM_Receiving_Application.Module_VisualAutomation.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_VisualAutomation.Models;

/// <summary>
/// Covers the values the probes send to Infor VISUAL: they start blank, a run is blocked until every one
/// is filled in, and the dialog can edit a copy without touching the live set.
/// </summary>
public sealed class Model_VisualProbeInputsTests
{
    [Fact]
    public void NewInputs_ShouldBeBlankAndIncomplete_WhenCreated()
    {
        var inputs = new Model_VisualProbeInputs();

        inputs.BolId.Should().BeEmpty();
        inputs.VendPackId.Should().BeEmpty();
        inputs.VendFreightId.Should().BeEmpty();
        inputs.IsComplete.Should().BeFalse();
        inputs.MissingFields().Should().BeEquivalentTo("BOL_ID", "VEND_PACK_ID", "VEND_FREIGHT_ID");
    }

    [Fact]
    public void IsComplete_ShouldBeTrue_WhenEveryValueIsFilled()
    {
        var inputs = new Model_VisualProbeInputs
        {
            BolId = "TEST-BOL",
            VendPackId = "TEST-PACK",
            VendFreightId = "TEST-FRT",
        };

        inputs.IsComplete.Should().BeTrue();
        inputs.MissingFields().Should().BeEmpty();
        inputs.DescribeMissing().Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "TEST-PACK", "TEST-FRT", "BOL_ID")]
    [InlineData("TEST-BOL", "", "TEST-FRT", "VEND_PACK_ID")]
    [InlineData("TEST-BOL", "TEST-PACK", "", "VEND_FREIGHT_ID")]
    [InlineData("   ", "TEST-PACK", "TEST-FRT", "BOL_ID")]
    public void MissingFields_ShouldNameTheBlankValue_WhenOneIsEmpty(
        string bolId,
        string packId,
        string freightId,
        string expectedMissing
    )
    {
        var inputs = new Model_VisualProbeInputs
        {
            BolId = bolId,
            VendPackId = packId,
            VendFreightId = freightId,
        };

        inputs.IsComplete.Should().BeFalse();
        inputs.MissingFields().Should().ContainSingle().Which.Should().Be(expectedMissing);
        inputs.DescribeMissing().Should().Contain(expectedMissing);
    }

    [Fact]
    public void ToWriteValues_ShouldTrimAndKeyByMacroVariable_WhenValuesHavePadding()
    {
        var inputs = new Model_VisualProbeInputs
        {
            BolId = "  TEST-BOL  ",
            VendPackId = "TEST-PACK",
            VendFreightId = " TEST-FRT ",
        };

        var values = inputs.ToWriteValues();

        values["BOL_ID"].Should().Be("TEST-BOL");
        values["VEND_PACK_ID"].Should().Be("TEST-PACK");
        values["VEND_FREIGHT_ID"].Should().Be("TEST-FRT");
        values.Should().HaveCount(3);
    }

    [Fact]
    public void Clone_ShouldBeIndependent_WhenTheCopyIsEdited()
    {
        var original = new Model_VisualProbeInputs
        {
            BolId = "TEST-BOL",
            VendPackId = "TEST-PACK",
            VendFreightId = "TEST-FRT",
        };

        var copy = original.Clone();
        copy.BolId = "CHANGED";

        original.BolId.Should().Be("TEST-BOL");
        copy.VendPackId.Should().Be("TEST-PACK");
    }

    [Fact]
    public void ToWriteValues_ShouldOnlyCoverFieldsTheProbesActuallyWrite_WhenInspected()
    {
        var inputs = new Model_VisualProbeInputs
        {
            BolId = "a",
            VendPackId = "b",
            VendFreightId = "c",
        };

        inputs.ToWriteValues().Keys.Should().BeEquivalentTo(
            "BOL_ID",
            "VEND_PACK_ID",
            "VEND_FREIGHT_ID"
        );
    }
}
