using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Service_CustomerPullPackMockDataCatalogTests
{
    [Fact]
    public void GetDemandRows_ShouldMergeRuntimeRowsOverStaticRows_AndNormalizeValues()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var staticPath = Path.Combine(tempDirectory, "customer-pull-pack-mock-data.json");
            var runtimePath = Path.Combine(tempDirectory, "customer-pull-pack.mock-runtime.json");

            File.WriteAllText(
                staticPath,
                """
                {
                  "DemandRows": [
                    {
                      "SourceLineKey": " line-1 ",
                      "CustomerId": " volvo ",
                      "CustomerName": " Volvo Trucks ",
                      "CustomerOrderId": " co-100 ",
                      "ParentPartId": " 8870214 ",
                      "SourceLocationId": " recv ",
                      "ShipQuantity": 10.0,
                      "PullDate": "2026-05-25T00:00:00",
                      "QuantityToPack": 10.0,
                      "FgOnHandQuantity": 15.0,
                      "FgLocationId": " fg-a1-01 ",
                      "ShortageFlag": false,
                      "LateOrderFlag": false,
                      "PulledFlag": false,
                      "HasLinkedWaitlist": false,
                      "LinkedWaitlistId": "",
                      "LinkedWaitlistStatus": "",
                      "RequesterNote": " static note ",
                      "RecheckIndicator": false
                    }
                  ],
                  "LocationRows": [
                    {
                      "LocationKey": " key-1 ",
                      "SourceLineKey": " line-1 ",
                      "ParentPartId": " 8870214 ",
                      "LocationId": " v-b0-12 ",
                      "DisplayLabel": " Rack 1 ",
                      "OnHandQuantity": 5.0,
                      "SourceType": " SubPartOnHand ",
                      "InitiallySelected": false
                    }
                  ]
                }
                """
            );

            File.WriteAllText(
                runtimePath,
                """
                {
                  "DemandRows": [
                    {
                      "SourceLineKey": "LINE-1",
                      "CustomerId": "VOLVO",
                      "CustomerName": "Runtime Volvo",
                      "CustomerOrderId": "CO-100",
                      "ParentPartId": "8870214",
                      "SourceLocationId": "RECV",
                      "ShipQuantity": 12.0,
                      "PullDate": "2026-05-24T00:00:00",
                      "QuantityToPack": 12.0,
                      "FgOnHandQuantity": 20.0,
                      "FgLocationId": "FG-A1-02",
                      "ShortageFlag": true,
                      "LateOrderFlag": false,
                      "PulledFlag": false,
                      "HasLinkedWaitlist": true,
                      "LinkedWaitlistId": "CPP-WL-1000",
                      "LinkedWaitlistStatus": "Requested",
                      "RequesterNote": "runtime note",
                      "RecheckIndicator": true
                    }
                  ],
                  "LocationRows": [
                    {
                      "LocationKey": "KEY-1",
                      "SourceLineKey": "LINE-1",
                      "ParentPartId": "8870214",
                      "LocationId": "V-B0-14",
                      "DisplayLabel": "Runtime Rack",
                      "OnHandQuantity": 9.0,
                      "SourceType": "SubPartOnHand",
                      "InitiallySelected": true
                    }
                  ]
                }
                """
            );

            var service = new Service_CustomerPullPackMockDataCatalog(staticPath, runtimePath);

            var demandRows = service.GetDemandRows();
            var locationRows = service.GetLocationRows();

            demandRows.Should().ContainSingle();
            demandRows[0].SourceLineKey.Should().Be("LINE-1");
            demandRows[0].CustomerId.Should().Be("VOLVO");
            demandRows[0].CustomerName.Should().Be("Runtime Volvo");
            demandRows[0].FgLocationId.Should().Be("FG-A1-02");
            demandRows[0].RequesterNote.Should().Be("runtime note");
            demandRows[0].HasLinkedWaitlist.Should().BeTrue();

            locationRows.Should().ContainSingle();
            locationRows[0].LocationKey.Should().Be("KEY-1");
            locationRows[0].LocationId.Should().Be("V-B0-14");
            locationRows[0].InitiallySelected.Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void GetCatalog_ShouldExposeMergedDemandAndLocationRows()
    {
        var tempDirectory = CreateTempDirectory();
        try
        {
            var staticPath = Path.Combine(tempDirectory, "customer-pull-pack-mock-data.json");
            var runtimePath = Path.Combine(tempDirectory, "customer-pull-pack.mock-runtime.json");

            File.WriteAllText(
                staticPath,
                """
                {
                  "DemandRows": [
                    {
                      "SourceLineKey": "LINE-1",
                      "CustomerId": "VOLVO",
                      "CustomerName": "Volvo Trucks",
                      "CustomerOrderId": "CO-1",
                      "ParentPartId": "8870214",
                      "SourceLocationId": "RECV",
                      "ShipQuantity": 10.0,
                      "PullDate": "2026-05-25T00:00:00",
                      "QuantityToPack": 10.0,
                      "FgOnHandQuantity": 10.0,
                      "FgLocationId": "FG-A1-01",
                      "ShortageFlag": false,
                      "LateOrderFlag": false,
                      "PulledFlag": false,
                      "HasLinkedWaitlist": false,
                      "LinkedWaitlistId": "",
                      "LinkedWaitlistStatus": "",
                      "RequesterNote": "",
                      "RecheckIndicator": false
                    }
                  ],
                  "LocationRows": [
                    {
                      "LocationKey": "KEY-1",
                      "SourceLineKey": "LINE-1",
                      "ParentPartId": "8870214",
                      "LocationId": "V-B0-12",
                      "DisplayLabel": "Rack 1",
                      "OnHandQuantity": 5.0,
                      "SourceType": "SubPartOnHand",
                      "InitiallySelected": false
                    }
                  ]
                }
                """
            );

            File.WriteAllText(runtimePath, "{\n  \"DemandRows\": [],\n  \"LocationRows\": []\n}");

            var service = new Service_CustomerPullPackMockDataCatalog(staticPath, runtimePath);

            var catalog = service.GetCatalog();

            catalog.DemandRows.Should().ContainSingle();
            catalog.LocationRows.Should().ContainSingle();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"cpp-mock-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }
}
