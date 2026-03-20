using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Models.Reporting;

public class Model_ReportRowTests
{
    [Fact]
    public void DisplayPo_ReturnsNonPo_ForReceivingNonPoRows()
    {
        var row = new Model_ReportRow
        {
            SourceModule = "Receiving",
            IsNonPOItem = true,
            PONumber = null,
            POLineNumber = null,
        };

        row.DisplayPo.Should().Be("Non-PO");
    }

    [Fact]
    public void DisplayPo_IncludesPOLine_ForReceivingPoRows()
    {
        var row = new Model_ReportRow
        {
            SourceModule = "Receiving",
            IsNonPOItem = false,
            PONumber = "PO-123456",
            POLineNumber = "70",
        };

        row.DisplayPo.Should().Be("PO-123456 / Line 70");
    }

    [Fact]
    public void DisplayPo_ReturnsRawPo_ForNonReceivingRows()
    {
        var row = new Model_ReportRow
        {
            SourceModule = "Volvo",
            IsNonPOItem = true,
            PONumber = "SHIP-001",
            POLineNumber = "10",
        };

        row.DisplayPo.Should().Be("SHIP-001");
    }
}
