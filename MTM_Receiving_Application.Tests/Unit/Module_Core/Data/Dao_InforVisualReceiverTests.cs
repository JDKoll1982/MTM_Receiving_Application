using System;
using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Data;

public sealed class Dao_InforVisualReceiverTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenApplicationIntentIsReadWrite()
    {
        var act = () => new Dao_InforVisualReceiver(
            "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadWrite"
        );

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_ShouldNotThrow_WhenApplicationIntentIsReadOnly()
    {
        var act = () => new Dao_InforVisualReceiver(
            "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly"
        );

        act.Should().NotThrow();
    }

    [Fact]
    public void QueryFile_ShouldContainNoWriteStatements()
    {
        var path = FindRepoFile(
            Path.Combine("Database", "InforVisualScripts", "Queries", "35_GetRecentReceivers.sql")
        );
        var sql = File.ReadAllText(path);

        Regex.IsMatch(sql, @"\bINSERT\b", RegexOptions.IgnoreCase).Should().BeFalse();
        Regex.IsMatch(sql, @"\bUPDATE\b", RegexOptions.IgnoreCase).Should().BeFalse();
        Regex.IsMatch(sql, @"\bDELETE\b", RegexOptions.IgnoreCase).Should().BeFalse();
        Regex.IsMatch(sql, @"\bMERGE\b", RegexOptions.IgnoreCase).Should().BeFalse();
        Regex.IsMatch(sql, @"\bEXEC\b", RegexOptions.IgnoreCase).Should().BeFalse();
    }

    [Fact]
    public void QueryFile_ShouldGroupOnlyByReceiverAndPart()
    {
        // FR-008: a receiver with several lines for one part is one row whose quantity received is
        // the sum of those lines. Grouping on the warehouse or the location as well would split that
        // part across rows, which is the defect this guards.
        var path = FindRepoFile(
            Path.Combine("Database", "InforVisualScripts", "Queries", "35_GetRecentReceivers.sql")
        );
        var sql = File.ReadAllText(path);

        var groupBy = Regex.Match(
            sql,
            @"GROUP\s+BY\s+(?<columns>[^;)]*?)\s*\)\s*,",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        groupBy.Success.Should().BeTrue("the query must group the receipt lines before ranking them");

        var columns = groupBy.Groups["columns"].Value;
        columns.Should().Contain("ReceiverNumber");
        columns.Should().Contain("PartNumber");
        columns
            .Should()
            .NotContain("WAREHOUSE", "a second warehouse for the same part must not split the row");
        columns
            .Should()
            .NotContain("LOCATION", "a second location for the same part must not split the row");
    }

    private static string FindRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException(
            $"Could not find {relativePath} walking up from {AppContext.BaseDirectory}"
        );
    }
}
