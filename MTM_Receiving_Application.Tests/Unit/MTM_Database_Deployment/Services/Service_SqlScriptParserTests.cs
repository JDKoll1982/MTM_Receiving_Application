using FluentAssertions;
using MTM_Database_Deployment.Services;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Services;

/// <summary>
/// The deployment tool streams every script through <see cref="Service_SqlScriptParser"/>, so a
/// split that is wrong in either direction corrupts a deployment: a misplaced split truncates a
/// procedure body, and a missed split silently drops the rest of a file.
/// </summary>
public sealed class Service_SqlScriptParserTests
{
    [Fact]
    public void SplitStatements_ShouldSplitOnSemicolon_WhenScriptHasPlainStatements()
    {
        var statements = Service_SqlScriptParser.SplitStatements("SELECT 1; SELECT 2;");

        statements.Should().Equal("SELECT 1", "SELECT 2");
    }

    [Fact]
    public void SplitStatements_ShouldKeepProcedureBodyIntact_WhenDelimiterDirectiveIsUsed()
    {
        var script = """
            DELIMITER $$

            DROP PROCEDURE IF EXISTS `sp_Demo` $$

            CREATE PROCEDURE `sp_Demo`()
            BEGIN
                SET @a = 1;
                SET @b = 2;
            END $$

            DELIMITER ;
            """;

        var statements = Service_SqlScriptParser.SplitStatements(script);

        statements.Should().HaveCount(2);
        statements[0].Should().Be("DROP PROCEDURE IF EXISTS `sp_Demo`");
        statements[1].Should().Contain("SET @a = 1;").And.Contain("SET @b = 2;").And.EndWith("END");
    }

    [Fact]
    public void SplitStatements_ShouldTreatSemicolonAsData_WhenItSitsInsideAStringLiteral()
    {
        var statements = Service_SqlScriptParser.SplitStatements(
            "INSERT INTO dunnage_types (type_name) VALUES ('Tape; Strapping'); SELECT 2;");

        statements.Should().HaveCount(2);
        statements[0].Should().Contain("'Tape; Strapping'");
        statements[1].Should().Be("SELECT 2");
    }

    [Fact]
    public void SplitStatements_ShouldKeepEscapedQuote_WhenStringLiteralUsesBackslashEscape()
    {
        var statements = Service_SqlScriptParser.SplitStatements(
            "INSERT INTO t VALUES ('it\\'s; fine'); SELECT 2;");

        statements.Should().HaveCount(2);
        statements[0].Should().Contain("it\\'s; fine");
    }

    [Fact]
    public void SplitStatements_ShouldNotSplit_WhenSemicolonSitsInsideALineComment()
    {
        var script = """
            -- default values come first; then the choices
            SELECT 1;
            """;

        var statements = Service_SqlScriptParser.SplitStatements(script);

        statements.Should().ContainSingle();
        statements[0].Should().Contain("-- default values come first; then the choices")
            .And.Contain("SELECT 1");
    }

    [Fact]
    public void SplitStatements_ShouldNotSplit_WhenSemicolonSitsInsideAHashComment()
    {
        var statements = Service_SqlScriptParser.SplitStatements("# seed; rerun safe\nSELECT 1;");

        statements.Should().ContainSingle();
        statements[0].Should().Contain("# seed; rerun safe");
    }

    [Fact]
    public void SplitStatements_ShouldNotSplit_WhenSemicolonSitsInsideABlockComment()
    {
        var script = """
            /* one; two */
            SELECT 1;
            """;

        var statements = Service_SqlScriptParser.SplitStatements(script);

        statements.Should().ContainSingle();
        statements[0].Should().Contain("/* one; two */");
    }

    [Fact]
    public void SplitStatements_ShouldReturnTrailingStatement_WhenScriptHasNoFinalDelimiter()
    {
        var statements = Service_SqlScriptParser.SplitStatements("SELECT 1;\nSELECT 2");

        statements.Should().Equal("SELECT 1", "SELECT 2");
    }

    [Fact]
    public void SplitStatements_ShouldReturnNothing_WhenScriptIsEmptyOrWhitespace()
    {
        Service_SqlScriptParser.SplitStatements(string.Empty).Should().BeEmpty();
        Service_SqlScriptParser.SplitStatements("  \r\n\t ").Should().BeEmpty();
    }
}
