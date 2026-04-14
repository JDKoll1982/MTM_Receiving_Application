using System;
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

    [Fact]
    public void LoadAndPrepareQuery_ShouldKeepInputPartNumberInAssociatedPartRunsProjection()
    {
        var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
            "20_GetMaterialAvailabilityAssociatedPartRuns.sql"
        );

        var projectionStart = query.IndexOf(
            "SELECT TOP (@MaxResults)",
            StringComparison.OrdinalIgnoreCase
        );
        projectionStart.Should().BeGreaterThanOrEqualTo(0);

        var projection = query[projectionStart..];
        var fromIndex = projection.IndexOf(
            "FROM MatchingRequirements",
            StringComparison.OrdinalIgnoreCase
        );

        fromIndex.Should().BeGreaterThan(0);
        projection[..fromIndex].Should().Contain("InputPartNumber");
    }

    [Fact]
    public void LoadAndPrepareQuery_ShouldDetectBlanketOrdersByPurchaseOrderSuffix()
    {
        var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
            "19_GetMaterialAvailabilityIncomingSupply.sql"
        );

        query.Should().Contain("RIGHT(RTRIM(po.ID), 1) = 'B'");
    }

    [Fact]
    public void LoadAndPrepareQuery_ShouldNotFilterAssociatedPartRunsByWarehouse()
    {
        var query = Helper_SqlQueryLoader.LoadAndPrepareQuery(
            "20_GetMaterialAvailabilityAssociatedPartRuns.sql"
        );

        query.Should().NotContain("r.WAREHOUSE_ID = np.WarehouseCode");
        query.Should().NotContain("wo.WAREHOUSE_ID = np.WarehouseCode");
    }
}
