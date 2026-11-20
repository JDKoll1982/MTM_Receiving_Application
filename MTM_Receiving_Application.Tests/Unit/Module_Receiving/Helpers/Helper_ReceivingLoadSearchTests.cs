using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Edit Mode's Find box searches every column the row is read from the database with,
/// not only the columns the grid shows.
/// </summary>
public sealed class Helper_ReceivingLoadSearchTests
{
    [Fact]
    public void Matches_ShouldFindARowByAColumnTheGridDoesNotShow_WhenSearchingEveryField()
    {
        Model_ReceivingLoad load = Load();

        Helper_ReceivingLoadSearch
            .Matches(load, Helper_ReceivingLoadSearch.AllFieldsKey, "released")
            .Should()
            .BeTrue("the PO status is read from the database even though no column shows it");
        Helper_ReceivingLoadSearch
            .Matches(load, Helper_ReceivingLoadSearch.AllFieldsKey, "jkoll")
            .Should()
            .BeTrue("the user who entered the row is read from the database too");
        Helper_ReceivingLoadSearch
            .Matches(load, Helper_ReceivingLoadSearch.AllFieldsKey, "V-B2-28")
            .Should()
            .BeTrue("the saved location is searchable whether or not it is a shown column");
    }

    [Fact]
    public void Matches_ShouldStayInTheChosenColumn_WhenAColumnIsPicked()
    {
        Model_ReceivingLoad load = Load();

        Helper_ReceivingLoadSearch
            .Matches(load, "PO Status", "released")
            .Should()
            .BeTrue();
        Helper_ReceivingLoadSearch
            .Matches(load, "Part ID", "released")
            .Should()
            .BeFalse("a column search must not leak into the other columns");
        Helper_ReceivingLoadSearch
            .Matches(load, "Heat/Lot", "HT-99")
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Matches_ShouldFindADecimalQuantity_WhenTheClerkTypesItAsStored()
    {
        Model_ReceivingLoad load = Load();

        Helper_ReceivingLoadSearch
            .Matches(load, "Weight/Qty", "10070")
            .Should()
            .BeTrue("a weight stored as a decimal is read back as the clerk sees it");
        Helper_ReceivingLoadSearch
            .Matches(load, "Weight/Qty", "10070.00")
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Matches_ShouldFindAReceivedDate_InEitherWrittenForm()
    {
        Model_ReceivingLoad load = Load();

        Helper_ReceivingLoadSearch
            .Matches(load, "Received Date", "11/20/2026")
            .Should()
            .BeTrue();
        Helper_ReceivingLoadSearch
            .Matches(load, "Received Date", "2026-11-20")
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Matches_ShouldReadEveryField_WhenTheKeyIsOneThisBuildNoLongerOffers()
    {
        Model_ReceivingLoad load = Load();

        Helper_ReceivingLoadSearch
            .Matches(load, "A Column That Used To Exist", "released")
            .Should()
            .BeTrue("a saved preference from an older build falls back rather than matching nothing");
    }

    [Fact]
    public void Matches_ShouldMatchEveryRow_WhenTheTermIsBlank()
    {
        Model_ReceivingLoad load = Load();

        Helper_ReceivingLoadSearch.Matches(load, "Part ID", "   ").Should().BeTrue();
    }

    [Fact]
    public void ColumnKeys_ShouldOfferEveryColumnTheRowIsReadWith_AndStartOnAllFields()
    {
        Helper_ReceivingLoadSearch.ColumnKeys[0].Should().Be(Helper_ReceivingLoadSearch.AllFieldsKey);
        Helper_ReceivingLoadSearch
            .ColumnKeys.Should()
            .Contain(
                [
                    "Part ID",
                    "Part Description",
                    "Part Type",
                    "PO Number",
                    "PO Line",
                    "PO Status",
                    "PO Due Date",
                    "Qty Ordered",
                    "Unit of Measure",
                    "Vendor",
                    "Heat/Lot",
                    "Heat Shape",
                    "Heat Color",
                    "Location",
                    "Load #",
                    "Weight/Qty",
                    "Remaining Qty",
                    "Packages/Load",
                    "Weight/Package",
                    "Package Type",
                    "Received Date",
                    "Created At",
                    "User",
                    "Employee #",
                    "Quality Hold Required",
                    "Quality Hold Acknowledged",
                    "Hold Restriction",
                ]
            );
    }

    private static Model_ReceivingLoad Load() =>
        new()
        {
            PartID = "MMC0000657",
            PartDescription = "Coil, 14Ga X 47.000",
            PartType = "Coil",
            PoNumber = "PO-071250",
            PoLineNumber = "1",
            PoStatus = "Released",
            PoDueDate = new DateTime(2026, 11, 25),
            QtyOrdered = 83_300m,
            UnitOfMeasure = "EA",
            PoVendor = "Metals USA - Horicon",
            HeatLotNumber = "HT-99",
            HeatShapeCode = "circle",
            HeatColorCode = "green",
            InitialLocation = "V-B2-28",
            LoadNumber = 1,
            WeightQuantity = 10_070m,
            RemainingQuantity = 10_070,
            PackagesPerLoad = 2,
            WeightPerPackage = 5_035m,
            PackageTypeName = "Coil",
            ReceivedDate = new DateTime(2026, 11, 20, 7, 11, 12),
            CreatedAt = new DateTime(2026, 11, 20, 7, 11, 12),
            UserId = "jkoll",
            EmployeeNumber = 1234,
            IsQualityHoldRequired = false,
            IsQualityHoldAcknowledged = true,
            QualityHoldRestrictionType = "None",
        };
}
