using FluentAssertions;
using MTM_Database_Deployment.Models;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Models;

/// <summary>
/// <see cref="Model_ColumnDefinition.ToColumnDefinitionSql"/> feeds every generated
/// <c>ADD COLUMN</c> / <c>MODIFY COLUMN</c>. information_schema reports string defaults as a bare
/// literal, so emitting them raw produced <c>DEFAULT Quantity</c> and MySQL rejected the statement.
/// </summary>
public sealed class Model_ColumnDefinitionTests
{
    [Fact]
    public void ToColumnDefinitionSql_ShouldQuoteDefault_WhenColumnIsAStringType()
    {
        var column = new Model_ColumnDefinition
        {
            Name = "quantity_type",
            DataType = "varchar(100)",
            IsNullable = false,
            DefaultValue = "Quantity"
        };

        column.ToColumnDefinitionSql().Should().Be("varchar(100) NOT NULL DEFAULT 'Quantity'");
    }

    [Fact]
    public void ToColumnDefinitionSql_ShouldEscapeEmbeddedQuote_WhenDefaultContainsOne()
    {
        var column = new Model_ColumnDefinition
        {
            Name = "note",
            DataType = "varchar(20)",
            IsNullable = true,
            DefaultValue = "it's"
        };

        column.ToColumnDefinitionSql().Should().Contain("DEFAULT 'it''s'");
    }

    [Theory]
    [InlineData("int", "0")]
    [InlineData("decimal(18,4)", "1.5000")]
    [InlineData("bigint unsigned", "0")]
    public void ToColumnDefinitionSql_ShouldLeaveDefaultUnquoted_WhenColumnIsNumeric(string dataType, string defaultValue)
    {
        var column = new Model_ColumnDefinition
        {
            Name = "amount",
            DataType = dataType,
            IsNullable = true,
            DefaultValue = defaultValue
        };

        column.ToColumnDefinitionSql().Should().Contain($"DEFAULT {defaultValue}")
            .And.NotContain($"DEFAULT '{defaultValue}'");
    }

    [Fact]
    public void ToColumnDefinitionSql_ShouldLeaveDefaultUnquoted_WhenColumnUsesAGeneratedTimestamp()
    {
        var column = new Model_ColumnDefinition
        {
            Name = "created_date",
            DataType = "datetime",
            IsNullable = false,
            DefaultValue = "CURRENT_TIMESTAMP",
            Extra = "DEFAULT_GENERATED"
        };

        column.ToColumnDefinitionSql().Should().Be("datetime NOT NULL DEFAULT CURRENT_TIMESTAMP");
    }

    [Fact]
    public void ToColumnDefinitionSql_ShouldKeepAutoIncrementAndOnUpdate_WhenBothArePresent()
    {
        var column = new Model_ColumnDefinition
        {
            Name = "modified_date",
            DataType = "datetime",
            IsNullable = true,
            DefaultValue = "CURRENT_TIMESTAMP",
            Extra = "DEFAULT_GENERATED on update CURRENT_TIMESTAMP"
        };

        column.ToColumnDefinitionSql().Should().Be(
            "datetime NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
    }

    [Fact]
    public void ToColumnDefinitionSql_ShouldNotEmitDefault_WhenNoDefaultIsReported()
    {
        var column = new Model_ColumnDefinition
        {
            Name = "icon",
            DataType = "varchar(50)",
            IsNullable = true,
            DefaultValue = null
        };

        column.ToColumnDefinitionSql().Should().Be("varchar(50) NULL");
    }
}
