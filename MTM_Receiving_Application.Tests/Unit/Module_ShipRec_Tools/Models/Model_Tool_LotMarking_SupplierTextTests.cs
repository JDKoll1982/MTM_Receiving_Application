using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Models;

/// <summary>
/// The Supplier the list and the report show. A lot is keyed on the supplier group, which is the Infor
/// Visual id for most vendors, so the column reads the name the receipt carried instead.
/// </summary>
public sealed class Model_Tool_LotMarking_SupplierTextTests
{
    [Fact]
    public void SupplierText_ShouldBeTheReceiptName_WhenBothTheNameAndTheGroupAreRecorded()
    {
        var lot = new Model_Tool_LotMarking
        {
            SupplierId = "VID-001080-01",
            SupplierName = "MST STEEL CORPORATION",
        };

        lot.SupplierText.Should().Be("MST STEEL CORPORATION");
    }

    [Fact]
    public void SupplierText_ShouldBeTheGroup_WhenNoNameWasRecorded()
    {
        var lot = new Model_Tool_LotMarking { SupplierId = "NONPO", SupplierName = string.Empty };

        lot.SupplierText.Should().Be("NONPO", "a lot with no supplier still needs a label");
    }

    [Fact]
    public void SupplierText_ShouldBeTheGroup_WhenTheRecordedNameIsOnlyWhitespace()
    {
        var lot = new Model_Tool_LotMarking { SupplierId = "NONPO", SupplierName = "   " };

        lot.SupplierText.Should().Be("NONPO");
    }
}
