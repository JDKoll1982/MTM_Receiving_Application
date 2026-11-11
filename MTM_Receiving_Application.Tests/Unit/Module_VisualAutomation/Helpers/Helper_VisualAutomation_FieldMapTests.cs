using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_VisualAutomation.Helpers;
using MTM_Receiving_Application.Module_VisualAutomation.Probes;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_VisualAutomation.Helpers;

/// <summary>
/// Covers the mapping between the macro variable list VISUAL shows in its editor and the labels the
/// receipt window actually paints.
/// </summary>
public sealed class Helper_VisualAutomation_FieldMapTests
{
    [Fact]
    public void MacroVariables_ShouldContainEveryVariableTheEditorOffers_WhenRead()
    {
        var variables = Helper_VisualAutomation_FieldMap.MacroVariables;

        variables.Should().Contain("ORDER_ID");
        variables.Should().Contain("MACRO_MESSAGE");
        variables.Should().Contain("udfString1");
        variables.Should().Contain("udfURL5");
        variables.Should().Contain("PO_USER_10");
        variables.Should().OnlyHaveUniqueItems();
        variables.Count.Should().Be(87);
    }

    [Theory]
    [InlineData("ORDER_ID", "Order ID")]
    [InlineData("SITE_ID", "Site ID")]
    [InlineData("DISPATCH_ID", "Service Dispatch ID")]
    [InlineData("FREE_ON_BOARD", "FOB")]
    [InlineData("BOL_ID", "Bill of Lading ID")]
    public void LabelFor_ShouldReturnTheOnScreenLabel_WhenTheVariableIsVisible(
        string key,
        string expectedLabel
    )
    {
        Helper_VisualAutomation_FieldMap.LabelFor(key).Should().Be(expectedLabel);
    }

    [Theory]
    [InlineData("LINES")]
    [InlineData("MACRO_MESSAGE")]
    [InlineData("udfString1")]
    [InlineData("CONTACT_FAX")]
    public void LabelFor_ShouldReturnNull_WhenTheVariableHasNoVisibleControl(string key)
    {
        Helper_VisualAutomation_FieldMap.LabelFor(key).Should().BeNull();
    }

    [Fact]
    public void VisibleVariables_ShouldBeASubsetOfEveryMacroVariable_WhenRead()
    {
        var all = Helper_VisualAutomation_FieldMap.MacroVariables;

        Helper_VisualAutomation_FieldMap.VisibleVariables.Should().OnlyContain(key => all.Contains(key));
        Helper_VisualAutomation_FieldMap.VisibleVariables.Count.Should().BeLessThan(all.Count);
    }

    [Theory]
    [InlineData("BOL_ID", true)]
    [InlineData("VEND_PACK_ID", true)]
    [InlineData("DESIRED_RECV_DATE", false)]
    [InlineData("BUYER", false)]
    [InlineData("udfNumber1", false)]
    public void IsWritable_ShouldOnlyAllowFreeTextFields_WhenQueried(string key, bool expected)
    {
        Helper_VisualAutomation_FieldMap.IsWritable(key).Should().Be(expected);
    }

    [Fact]
    public void WritableFields_ShouldAllHaveAVisibleControl_WhenRead()
    {
        var writable = Helper_VisualAutomation_FieldMap.MacroVariables
            .Where(Helper_VisualAutomation_FieldMap.IsWritable)
            .ToList();

        writable.Should().NotBeEmpty();
        writable.Should().OnlyContain(key => Helper_VisualAutomation_FieldMap.LabelFor(key) != null);
    }

    [Theory]
    [InlineData("Order ID", "ORDER_ID")]
    [InlineData("  order id  ", "ORDER_ID")]
    [InlineData("Bill of Lading ID:", "BOL_ID")]
    [InlineData("FOB", "FREE_ON_BOARD")]
    public void KeyForLabel_ShouldResolveTheMacroVariable_WhenTheLabelIsKnown(
        string label,
        string expectedKey
    )
    {
        Helper_VisualAutomation_FieldMap.KeyForLabel(label).Should().Be(expectedKey);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Total Qty Recv")]
    public void KeyForLabel_ShouldReturnNull_WhenTheLabelIsNotMapped(string label)
    {
        Helper_VisualAutomation_FieldMap.KeyForLabel(label).Should().BeNull();
    }

    [Fact]
    public void NormalizeLabel_ShouldCollapseWhitespaceAndCase_WhenGivenAMessyLabel()
    {
        Helper_VisualAutomation_FieldMap.NormalizeLabel("  Bill   of  Lading ID : ")
            .Should()
            .Be("BILL OF LADING ID");
    }

    [Fact]
    public void Probes_ShouldReferenceOnlyKnownVariables_WhenTheCatalogueIsRead()
    {
        var known = Helper_VisualAutomation_FieldMap.MacroVariables;

        foreach (var probe in VisualProbeCatalog.All)
        {
            foreach (var key in probe.ReportedKeys)
            {
                if (key is "POSTING_STATE_BEFORE" or "POSTING_STATE_AFTER")
                {
                    continue;
                }

                known.Should().Contain(key, $"probe {probe.Id} reports on a variable VISUAL must expose");
            }
        }
    }

    [Fact]
    public void Probes_ShouldHaveUniqueIdentifiers_WhenTheCatalogueIsRead()
    {
        VisualProbeCatalog.All.Select(probe => probe.Id).Should().OnlyHaveUniqueItems();
        VisualProbeCatalog.All.Should().HaveCount(10);
    }

    [Fact]
    public void Probes_ShouldDescribeTheUiEffect_WhenAnyProbeWritesToTheWindow()
    {
        var writers = VisualProbeCatalog.All.Where(probe => probe.WritesToWindow).ToList();

        writers.Should().NotBeEmpty();
        writers.Should().OnlyContain(probe => probe.UiSteps.Length > 0);
        writers.SelectMany(probe => probe.ReportedKeys)
            .Should()
            .OnlyContain(key => Helper_VisualAutomation_FieldMap.IsWritable(key));
    }
}
