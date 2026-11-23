using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Settings;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Views;

public sealed class View_Receiving_ReceiverSelectionColumnsTests
{
    private static readonly string[] HeaderNames =
    [
        "HeaderReceiverNumber",
        "HeaderPoNumber",
        "HeaderLineNumber",
        "HeaderPartNumber",
        "HeaderVendor",
        "HeaderLocationReceivedTo",
        "HeaderQtyReceived",
        "HeaderQtyOrdered",
        "HeaderQtyRemaining",
    ];

    private static readonly string[] ColumnKeys =
    [
        ReceivingSettingsKeys.ReceiverSelection.ColumnReceiverNumber,
        ReceivingSettingsKeys.ReceiverSelection.ColumnPoNumber,
        ReceivingSettingsKeys.ReceiverSelection.ColumnLineNumber,
        ReceivingSettingsKeys.ReceiverSelection.ColumnPartNumber,
        ReceivingSettingsKeys.ReceiverSelection.ColumnVendor,
        ReceivingSettingsKeys.ReceiverSelection.ColumnLocationReceivedTo,
        ReceivingSettingsKeys.ReceiverSelection.ColumnQtyReceived,
        ReceivingSettingsKeys.ReceiverSelection.ColumnQtyOrdered,
        ReceivingSettingsKeys.ReceiverSelection.ColumnQtyRemaining,
    ];

    private static readonly string[] ExpectedHeaders =
    [
        "Receiver Number",
        "PO #",
        "Line #",
        "Part Number",
        "Vendor",
        "Location Received to",
        "Qty Received",
        "Qty Ordered",
        "Qty Remaining on Line",
    ];

    [Fact]
    public void ReceiverSelectionXaml_ShouldDeclareTheNineHeadersInContractOrder()
    {
        var path = FindRepoFile(
            Path.Combine("Module_Receiving", "Views", "View_Receiving_ReceiverSelection.xaml")
        );
        var content = File.ReadAllText(path);

        var indices = HeaderNames
            .Select(name => content.IndexOf($"x:Name=\"{name}\"", StringComparison.Ordinal))
            .ToArray();

        indices.Should().OnlyContain(index => index >= 0, "every header must be declared in the view");
        indices.Should().BeInAscendingOrder("the column order is the contract");
    }

    [Fact]
    public void StringDefaults_ShouldHoldTheNineHeadersVerbatimInContractOrder()
    {
        // The view binds these nine keys in that order, which is what makes the x:Name order above
        // and the settings order below a single contract.
        var defaults = ReceivingSettingsDefaults.StringDefaults;

        var headers = ColumnKeys.Select(key => defaults[key]).ToArray();

        headers.Should().Equal(ExpectedHeaders);
    }

    [Fact]
    public void LocationColumn_ShouldShowTheWarehouseAlone_WhenTheLineNamesNoLocation()
    {
        var warehouseOnly = new Model_InforVisualReceiverRow
        {
            Warehouse = "002",
            LocationReceivedTo = string.Empty,
        };
        var withLocation = new Model_InforVisualReceiverRow
        {
            Warehouse = "002",
            LocationReceivedTo = "RECV",
        };

        warehouseOnly.LocationReceivedToDisplay.Should().Be("002");
        withLocation.LocationReceivedToDisplay.Should().Be("RECV");
        warehouseOnly
            .LocationReceivedTo.Should()
            .BeEmpty("the carry into Load Entry keeps the raw location");

        var content = File.ReadAllText(
            FindRepoFile(
                Path.Combine("Module_Receiving", "Views", "View_Receiving_ReceiverSelection.xaml")
            )
        );

        content.Should().Contain("x:Bind LocationReceivedToDisplay");
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
