using FluentAssertions;
using MTM_Database_Deployment.Models;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Models;

/// <summary>
/// A re-deploy of an already converged database leans on benign skips for every duplicate seed
/// insert, so the counters decide whether the operator sees a clean run or a wall of noise.
/// </summary>
public sealed class Model_DeploymentResultTests
{
    [Fact]
    public void MarkBenignSkip_ShouldAggregatePerObject_WhenTheSameObjectSkipsRepeatedly()
    {
        var result = new Model_DeploymentResult();

        result.MarkBenignSkip("SeedData/03_seed_dunnage_data.sql");
        result.MarkBenignSkip("SeedData/03_seed_dunnage_data.sql");
        result.MarkBenignSkip("SeedData/03_seed_dunnage_data.sql");

        result.BenignSkipCount.Should().Be(3);
        result.BenignSkips.Should().ContainSingle()
            .Which.Value.Should().Be(3);
        result.Messages.Should().BeEmpty("benign skips are summarised, not logged line by line");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MarkBenignSkip_ShouldKeepObjectsSeparate_WhenDifferentObjectsSkip()
    {
        var result = new Model_DeploymentResult();

        result.MarkBenignSkip("SeedData/03_seed_dunnage_data.sql");
        result.MarkBenignSkip("StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql");

        result.BenignSkips.Should().HaveCount(2);
        result.BenignSkips["SeedData/03_seed_dunnage_data.sql"].Should().Be(1);
        result.BenignSkips["StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql"].Should().Be(1);
    }

    [Fact]
    public void IsSuccess_ShouldBeFalse_WhenAnyFailureIsRecorded()
    {
        var result = new Model_DeploymentResult();

        result.MarkApplied("Sql_Files/Schemas/04_Table_dunnage_types.sql");
        result.MarkFailed("Sql_Files/SeedData/03_seed_dunnage_data.sql: Column 'CustomFieldID' cannot be null");

        result.IsSuccess.Should().BeFalse();
        result.FailedCount.Should().Be(1);
        result.Failures.Should().ContainSingle();
    }

    [Fact]
    public void MarkFailed_ShouldAggregateRepeats_WhenTheSameFailureOccursManyTimes()
    {
        var result = new Model_DeploymentResult();

        for (var index = 0; index < 90; index++)
        {
            result.MarkFailed("SeedData/03_seed_dunnage_data.sql: Unknown column 'quantity_type' in 'field list'");
        }

        result.FailedCount.Should().Be(90);
        result.Failures.Should().ContainSingle("a repeated failure is reported once");
        result.FailureCounts.Should().ContainSingle().Which.Value.Should().Be(90);
    }

    [Fact]
    public void MarkFailed_ShouldKeepFirstSeenOrder_WhenDifferentFailuresInterleave()
    {
        var result = new Model_DeploymentResult();

        result.MarkFailed("first");
        result.MarkFailed("second");
        result.MarkFailed("first");

        result.Failures.Should().Equal("first", "second");
    }

    [Fact]
    public void ToReportText_ShouldListEachSkippedObjectOnce_WhenBenignSkipsRepeat()
    {
        var result = new Model_DeploymentResult();
        result.MarkApplied("Sql_Files/Schemas/04_Table_dunnage_types.sql");
        result.MarkBenignSkip("Sql_Files/SeedData/03_seed_dunnage_data.sql");
        result.MarkBenignSkip("Sql_Files/SeedData/03_seed_dunnage_data.sql");

        var report = result.ToReportText();

        report.Should().Contain("Applied: 1 | Skipped: 0 | Already applied: 2 | Failed: 0");
        report.Should().Contain("Sql_Files/SeedData/03_seed_dunnage_data.sql (x2)");
        report.Should().NotContain("FAILURES");
    }

    [Fact]
    public void ToReportText_ShouldIncludeFailureSection_WhenAFailureIsRecorded()
    {
        var result = new Model_DeploymentResult();
        result.MarkFailed("Sql_Files/Views/05_View_dunnage_label_data.sql: Unknown column 'dld.quantity_type'");

        var report = result.ToReportText();

        report.Should().Contain("FAILURES");
        report.Should().Contain("Unknown column 'dld.quantity_type'");
    }

    [Fact]
    public void ToReportText_ShouldPrintOccurrenceCount_WhenAFailureRepeats()
    {
        var result = new Model_DeploymentResult();
        result.MarkFailed("SeedData/03_seed_dunnage_data.sql: Unknown column 'quantity_type' in 'field list'");
        result.MarkFailed("SeedData/03_seed_dunnage_data.sql: Unknown column 'quantity_type' in 'field list'");

        var report = result.ToReportText();

        report.Should().Contain("Failed: 2");
        report.Should().Contain("Unknown column 'quantity_type' in 'field list' (x2)");
    }
}
