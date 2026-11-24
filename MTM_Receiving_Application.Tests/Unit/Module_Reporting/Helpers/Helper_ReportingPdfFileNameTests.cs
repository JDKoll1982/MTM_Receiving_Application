using FluentAssertions;
using MTM_Receiving_Application.Module_Reporting.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.Helpers;

public class Helper_ReportingPdfFileNameTests
{
    [Fact]
    public void BuildFileName_ShouldReplaceUnsafeCharacters_WhenTitleHasPathCharacters()
    {
        var fileName = Helper_ReportingPdfFileName.BuildFileName(
            "End of Day Report for 11/18/2026 - 11/24/2026"
        );

        fileName.Should().Be("End of Day Report for 11-18-2026 - 11-24-2026.pdf");
        fileName.Should().NotContain("/");
    }

    [Fact]
    public void BuildFileName_ShouldCollapseSpaces_WhenTitleHasRepeatedSpaces()
    {
        Helper_ReportingPdfFileName
            .BuildFileName("Report: Shift   A")
            .Should()
            .Be("Report- Shift A.pdf");
    }

    [Fact]
    public void BuildFileName_ShouldFallBackToReportSummary_WhenTitleIsBlank()
    {
        Helper_ReportingPdfFileName.BuildFileName("  ").Should().Be("Report Summary.pdf");
    }
}
