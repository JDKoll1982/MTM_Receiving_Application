using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Helpers.Database;

public sealed class Helper_SqlQueryLoaderTests
{
    [Fact]
    public void ExtractQueryFromFile_ShouldKeepCteWhenQueryStartsWithSemicolonWith()
    {
        var sql = """
            -- sample query
            DECLARE @WarehouseCode nvarchar(15) = '002';
            DECLARE @LocationId nvarchar(30) = 'RECV';

            ;WITH NormalizedParameters AS (
                SELECT @WarehouseCode AS WarehouseCode
            )
            SELECT WarehouseCode
            FROM NormalizedParameters;
            """;

        var extracted = Helper_SqlQueryLoader.ExtractQueryFromFile(sql);

        extracted.Should().StartWith(";WITH");
        extracted.Should().Contain("SELECT WarehouseCode");
        extracted.Should().NotContain("DECLARE @WarehouseCode");
    }
}
