using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_VisualAutomation.Enums;
using MTM_Receiving_Application.Module_VisualAutomation.Helpers;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_VisualAutomation.Helpers;

/// <summary>
/// Locks in the reverse-engineered meaning of the Purchase Receipt Entry macro variables so a future
/// change cannot silently re-map a field. Evidence for each expectation is recorded in
/// docs/development/InforVisual-Macro-Variable-Reference.md.
/// </summary>
public sealed class Helper_VisualAutomation_VariableBindingTests
{
    [Fact]
    public void Bindings_ShouldCoverEveryMacroVariable_WhenTheCatalogueIsRead()
    {
        var unmapped = Helper_VisualAutomation_FieldMap
            .MacroVariables.Where(key => Helper_VisualAutomation_FieldMap.BindingFor(key) is null)
            .ToList();

        unmapped.Should().BeEmpty();
        Helper_VisualAutomation_FieldMap.Bindings.Should().HaveCount(87);
    }

    [Theory]
    [InlineData("ORDER_ID", "RECEIVER", "PURC_ORDER_ID")]
    [InlineData("RECEIVER_ID", "RECEIVER", "ID")]
    [InlineData("SITE_ID", "RECEIVER", "SITE_ID")]
    [InlineData("BOL_ID", "RECEIVER", "BOL_ID")]
    [InlineData("VEND_PACK_ID", "RECEIVER", "VEND_PACK_ID")]
    [InlineData("VEND_FREIGHT_ID", "RECEIVER", "VEND_FREIGHT_ID")]
    [InlineData("CARRIER_ID", "RECEIVER", "CARRIER_ID")]
    [InlineData("RECEIVED_DATE", "RECEIVER", "RECEIVED_DATE")]
    [InlineData("VENDOR_ID", "PURCHASE_ORDER", "VENDOR_ID")]
    [InlineData("BUYER", "PURCHASE_ORDER", "BUYER")]
    [InlineData("FREE_ON_BOARD", "PURCHASE_ORDER", "FREE_ON_BOARD")]
    [InlineData("SHIP_VIA", "PURCHASE_ORDER", "SHIP_VIA")]
    [InlineData("STATUS", "PURCHASE_ORDER", "STATUS")]
    [InlineData("POSTING_CANDIDATE", "PURCHASE_ORDER", "POSTING_CANDIDATE")]
    [InlineData("DESIRED_RECV_DATE", "PURC_ORDER_LINE", "DESIRED_RECV_DATE")]
    [InlineData("DISPATCH_ID", "SERVICE_RCVR_DISP", "DISPATCH_ID")]
    [InlineData("RECVR_LONG_DESC", "RECEIVER_LINE", "DESCRIPTION")]
    [InlineData("LONG_DESCRIPTION", "VENDOR_PART", "LONG_DESCRIPTION")]
    public void BindingFor_ShouldResolveTheBackingColumn_WhenTheVariableIsStored(
        string key,
        string table,
        string column
    )
    {
        var binding = Helper_VisualAutomation_FieldMap.BindingFor(key);

        binding.Should().NotBeNull();
        binding!.Table.Should().Be(table);
        binding.Column.Should().Be(column);
        binding.Safety.Should().NotBe(Enum_VisualVariableSafety.Scratch);
    }

    [Theory]
    [InlineData("PO_USER_1", "USER_1")]
    [InlineData("PO_USER_5", "USER_5")]
    [InlineData("PO_USER_10", "USER_10")]
    public void BindingFor_ShouldDropThePoPrefix_WhenTheVariableIsPoUserText(
        string key,
        string column
    )
    {
        var binding = Helper_VisualAutomation_FieldMap.BindingFor(key);

        binding!.Table.Should().Be("PURCHASE_ORDER");
        binding.Column.Should().Be(column);
    }

    [Theory]
    [InlineData("DATABASE")]
    [InlineData("USER")]
    [InlineData("LINES")]
    public void BindingFor_ShouldHaveNoBackingColumn_WhenTheVariableIsRuntimeSupplied(string key)
    {
        var binding = Helper_VisualAutomation_FieldMap.BindingFor(key);

        binding!.Safety.Should().Be(Enum_VisualVariableSafety.Runtime);
        binding.Table.Should().BeEmpty();
        binding.BackingColumn.Should().Be("(not persisted)");
    }

    [Theory]
    [InlineData("MACRO_MESSAGE")]
    [InlineData("MACRO_SUCCESS")]
    public void BindingFor_ShouldBeMacroOutput_WhenTheVariableIsTheStatusChannel(string key)
    {
        Helper_VisualAutomation_FieldMap.BindingFor(key)!.Safety
            .Should()
            .Be(Enum_VisualVariableSafety.MacroOutput);
    }

    [Fact]
    public void BindingFor_ShouldTreatEveryUdfSlotAsScratch_WhenTheModuleDoesNotPersistThem()
    {
        var udfKeys = Helper_VisualAutomation_FieldMap
            .MacroVariables.Where(key => key.StartsWith("udf", System.StringComparison.Ordinal))
            .ToList();

        udfKeys.Should().HaveCount(45);

        foreach (var key in udfKeys)
        {
            var binding = Helper_VisualAutomation_FieldMap.BindingFor(key);
            binding!.Safety.Should().Be(Enum_VisualVariableSafety.Scratch);
            binding.Table.Should().BeEmpty();
            binding.BackingColumn.Should().Be("(not persisted)");
        }
    }

    [Fact]
    public void WritableFields_ShouldNeverExceedTheReceiptOnlySafeClass_WhenQueried()
    {
        var writable = Helper_VisualAutomation_FieldMap
            .MacroVariables.Where(Helper_VisualAutomation_FieldMap.IsWritable)
            .ToList();

        writable.Should().NotBeEmpty();

        foreach (var key in writable)
        {
            var safety = Helper_VisualAutomation_FieldMap.BindingFor(key)!.Safety;
            safety.Should().Be(
                Enum_VisualVariableSafety.ReceiptOnly,
                $"{key} may only be typed into when it has no database consequence"
            );
            Helper_VisualAutomation_FieldMap.IsSafeToWrite(key).Should().BeTrue();
        }
    }

    [Fact]
    public void Bindings_ShouldClassifyFreeOnBoardAsFob_WhenTheScreenLabelSaysFob()
    {
        var binding = Helper_VisualAutomation_FieldMap.BindingFor("FREE_ON_BOARD");

        binding!.Meaning.Should().Contain("FOB");
        Helper_VisualAutomation_FieldMap.LabelFor("FREE_ON_BOARD").Should().Be("FOB");
    }
}
